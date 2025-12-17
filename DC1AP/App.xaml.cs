/*
 * MIT License
 *
 * Copyright (c) 2025 ArsonAssassin
 *
 * Permission is hereby granted, free of charge, to any person obtaining a copy
 * of this software and associated documentation files (the "Software"), to deal
 * in the Software without restriction, including without limitation the rights
 * to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
 * copies of the Software, and to permit persons to whom the Software is
 * vfurnished to do so, subject to the following conditions:
 * 
 * The above copyright notice and this permission notice shall be included in all
 * copies or substantial portions of the Software.
 * 
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
 * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
 * FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
 * AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
 * LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
 * OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
 * SOFTWARE.
 */
using Archipelago.Core;
using Archipelago.Core.GameClients;
using Archipelago.Core.Models;
using Archipelago.Core.Util;
using Archipelago.MultiClient.Net.BounceFeatures.DeathLink;
using Archipelago.MultiClient.Net.MessageLog.Messages;
using Archipelago.MultiClient.Net.Models;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using DC1AP.Constants;
using DC1AP.Georama;
using DC1AP.Items;
using DC1AP.Mem;
using DC1AP.Models;
using DC1AP.Threads;
using DC1AP.ViewModels;
using DC1AP.Views;
using Newtonsoft.Json;
using ReactiveUI;
using Serilog;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Concurrency;
using System.Threading;
using System.Threading.Tasks;
using Color = Avalonia.Media.Color;

// Adapted from github.com/ArsonAssassin/Archipelago-Avalonia-Template
namespace DC1AP
{
    public partial class App : Application
    {
        internal static ArchipelagoClient Client { get; set; }

        private static MainWindowViewModel Context;
        private static readonly object _lockObject = new();

        private static readonly ConcurrentQueue<Location> locationQueue = new();

        //private DeathLinkService _deathlinkService;
        private Thread queueThread;
        private Thread helperThread;
        private Thread reconnectThread;
        private GenericGameClient? ps2Client;

        public override void Initialize()
        {
            AvaloniaXamlLoader.Load(this);

            Context = new MainWindowViewModel() { ConnectButtonEnabled = true };
            Context.ConnectClicked += Context_ConnectClicked;
            Context.CommandReceived += (_, a) => Client?.SendMessage(a.Command);

            // TODO save last used host/slot?
            Context.Host = "localhost:38281";
            //Context.Slot = "DC1";

            InventoryMgmt.InitInventoryMgmt();
        }

        public override void OnFrameworkInitializationCompleted()
        {
            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                desktop.MainWindow = new MainWindow
                {
                    DataContext = Context
                };
            }
            else if (ApplicationLifetime is ISingleViewApplicationLifetime singleViewPlatform)
            {
                singleViewPlatform.MainView = new MainWindow
                {
                    DataContext = Context
                };
            }
            base.OnFrameworkInitializationCompleted();
        }

        private async void Context_ConnectClicked(object? sender, ConnectClickedEventArgs e)
        {
            Context.ConnectButtonEnabled = false;
            Log.Logger.Information("Connecting...");

            PlayerState.ValidGameState = false;

            if (Client != null)
            {
                Client.Connected -= OnConnected;
                Client.Disconnected -= OnDisconnected;
                Client.ItemReceived -= Client_ItemReceived;
                Client.MessageReceived -= Client_MessageReceived;

                //if (_deathlinkService != null)
                //{
                //    _deathlinkService.OnDeathLinkReceived -= _deathlinkService_OnDeathLinkReceived;
                //    _deathlinkService = null;
                //}
                Client.CancelMonitors();
            }

            ps2Client = PS2Connect();

            if (ps2Client == null)
            {
                Context.ConnectButtonEnabled = true;
                return;
            }

            // Connect to archipelago server
            Client = new ArchipelagoClient(ps2Client);

            Client.Connected += OnConnected;
            Client.Disconnected += OnDisconnected;

            await Client.Connect(e.Host, "Dark Cloud 1");

            Client.ItemReceived += Client_ItemReceived;
            Client.MessageReceived += Client_MessageReceived;

            await Client.Login(e.Slot, !string.IsNullOrWhiteSpace(e.Password) ? e.Password : null);

            if (!Client.IsConnected || !Client.IsLoggedIn)
            {
                Context.ConnectButtonEnabled = true;
                return;
            }

            // Pull out options from AP
            Options.ParseOptions(Client.Options);

            if (reconnectThread == null)
            {
                reconnectThread = new(new ParameterizedThreadStart(Reconnect))
                {
                    IsBackground = true
                };
                reconnectThread.Start();
            }

            GeoInvMgmt.Init();

            // Initialize things once the player is connected
            if (PlayerState.PlayerReady())
            {
                PlayerReady(e.Slot);
            }
            else
            {
                PlayerNotReady(e.Slot);
            }

            //if (Client.Options.ContainsKey("EnableDeathlink") && (bool)Client.Options["EnableDeathlink"])
            //{
            //    var _deathlinkService = Client.EnableDeathLink();
            //    _deathlinkService.OnDeathLinkReceived += _deathlinkService_OnDeathLinkReceived;
            //    // TODO listen for player death
            //}

            if (queueThread == null)
            {
                queueThread = new Thread(new ParameterizedThreadStart(ItemQueue.ThreadLoop))
                {
                    IsBackground = true
                };
                queueThread.Start();
            }

            if (helperThread == null)
            {
                helperThread = new Thread(new ParameterizedThreadStart(HelperThread.DoLoop))
                {
                    IsBackground = true
                };
                helperThread.Start();
            }

            Context.ConnectButtonEnabled = true;

            ReadGameState();
            MessageFuncs.InitOverlay();
        }

        private static void ReadGameState()
        {
            foreach (ItemInfo item in Client.CurrentSession.Items.AllItemsReceived)
            {
                long id = item.ItemId;

                if (id < MiscConstants.ItemIdBase)
                    GeoInvMgmt.IncGeoCount(id);
            }
        }

        #region PS2
        private static byte bossKillTest = 0;

        private GenericGameClient? PS2Connect()
        {
            String gameId = "BASCUS-97111dkcloud";

            GenericGameClient client = new("pcsx2-qt");
            try
            {
                client.Connect();
            }
            catch (ArgumentException)
            {
                Log.Logger.Error("PCSX2 not running, open PCSX2 before connecting!");
                Context.ConnectButtonEnabled = true;
                return null;
            }

            Log.Logger.Information("Connected to game.");

            Memory.CurrentProcId = Memory.GetProcessID("pcsx2-qt");
            Memory.GlobalOffset = Memory.GetPCSX2Offset();

            // Verify correct game/version
            String gameIdTest = Memory.ReadString(MiscAddrs.GameIdAddr, gameId.Length);
            if (!gameId.Equals(gameIdTest))
            {
                Log.Logger.Error("Wrong game or wrong version of Dark Cloud, please load NTSC version of the game.");
                return null;
            }

            return client;
        }

        private void PlayerReady(string slotName)
        {
            Thread.Sleep(50);
            string currSlot = OpenMem.GetSlotName();

            // First load for this save, so do extra stuff
            if (currSlot == "")
            {
                OpenMem.SetSlotData(slotName);
                EventMasks.InitMasks();
                Weapons.GiveCharWeapon(0);
                InventoryMgmt.GiveFreeFeather();
            }
            else if (currSlot != slotName)
            {
                Log.Logger.Error("Wrong slot name. Current save is using slot \"" + currSlot + "\".");
                PlayerState.ValidGameState = false;
                return;
            }
            else if (!OpenMem.TestRoomSeed())
            {
                PlayerState.ValidGameState = false;
                return;
            }

            InventoryMgmt.CheckAttachments(true);
            MiracleChestMgmt.Init();
            GeoInvMgmt.InitBuildings();
            CharFuncs.Init();
            Enemies.MultiplyABS();

            // Check for any missing items after a connect/reconnect
            ItemQueue.checkItems = true;

            // Skip needing Yaya to dance on your head if doing Saia once the building event viewed flag is set.
            if (Options.Goal >= 3 && !EventMasks.YayaDone())
            {
                Memory.MonitorAddressForAction<short>(GeoAddrs.YayaBldEventFlag, EventMasks.SkipYaya, (o) => { return o >= 1; });
            }

            PlayerState.ValidGameState = true;

            new Thread(new ParameterizedThreadStart(MiracleChestMgmt.DoLoop))
            {
                IsBackground = true
            }.Start();

            // Watch for the player to reset the game, then change the valid state flag and ready up to connect again.
            Memory.MonitorAddressForAction<byte>(MiscAddrs.PlayerState, () => PlayerNotReady(slotName), (o) => { return o <= 1; });
            WatchGoal();
        }

        private void PlayerNotReady(string slotName)
        {
            PlayerState.ValidGameState = false;
            ItemQueue.ClearQueues();
            Memory.MonitorAddressForAction<byte>(MiscAddrs.PlayerState, () => PlayerReady(slotName), (o) => { return o == 2; });
        }

        internal static async Task SendLocation(int locId)
        {
            Location loc = new()
            {
                Id = locId
            };

            if (Client.CurrentSession != null && Client.CurrentSession.Socket.Connected) 
                App.Client.SendLocation(loc);
            else
                locationQueue.Enqueue(loc);
        }

        private static void WatchGoal()
        {
            if (Options.AllBosses)
            {
                for (int i = 0; i < Options.Goal; i++)
                {
                    byte mask = (byte)(1 << i);
                    byte currKills = Memory.ReadByte(OpenMem.GoalAddr);
                    bossKillTest |= mask;

                    if ((currKills & mask) == 0)
                    {
                        // For some reason, the Boss Kill Flag doesn't set for Utan so use the floor kill count instead
                        if (i == 1)
                        {
                            Memory.MonitorAddressForAction<byte>(MiscAddrs.UtanFlag, () => AddBossKill(mask), (o) => { return o != 0; });
                        }
                        else
                        {
                            int value = (i + 1) * 100;
                            Memory.MonitorAddressForAction<short>(MiscAddrs.BossKillAddr, () => AddBossKill(mask), (o) => { return o == (short) value; });
                        }
                    }
                }
            }
            else
                // For some reason, the Boss Kill Flag doesn't set for Utan so use the floor kill count instead
                if (Options.Goal == 2)
                    Memory.MonitorAddressForAction<byte>(MiscAddrs.UtanFlag, () => Client.SendGoalCompletion(), (o) => { return o != 0; });
                else
                    Memory.MonitorAddressForAction<short>(MiscAddrs.BossKillAddr, () => Client.SendGoalCompletion(), (o) => { return o == Options.Goal * 100; });
        }

        /// <summary>
        /// Mask the boss kills into the goal byte.
        /// </summary>
        /// <param name="mask">Bit to set for killed boss.</param>
        private static void AddBossKill(byte mask)
        {
            byte bb = Memory.ReadByte(OpenMem.GoalAddr);
            bb |= mask;
            Memory.WriteByte(OpenMem.GoalAddr, bb);

            if (bb == bossKillTest)
            {
                Client.SendGoalCompletion();
                return;
            }

            // Take away the useless Moon Orb item since we already have Muska Lacka access
            if (mask == 1 << (int)Towns.Queens)
            {
                new Thread(() => ItemQueue.RemoveItemLoop(MiscConstants.MoonOrbItemId, ItemCategory.Inventory))
                {
                    IsBackground = true
                }.Start();

                // Prevent the player from refighting the boss
                Memory.WriteByte(MiscAddrs.FloorCountAddrs[(int)Towns.Queens], (byte)(MiscAddrs.FloorCountRear[(int)Towns.Queens] - 1));
                EventMasks.ClearShipwreckKey();
            }
            // Don't want the player to be able to activate the giant as it will remove miracle chests.
            else if (mask == 1 << (int)Towns.Factory && Options.MiracleSanity)
            {
                new Thread(() => ItemQueue.RemoveItemLoop(MiscConstants.SunSphereItemId, ItemCategory.FactoryGeo))
                {
                    IsBackground = true
                }.Start();
            }

            // If early bosses aren't yet defeated, lower the flag value so the player can't be locked out of earlier bosses.
            if (mask > 1 << (int)Towns.Matataki)
            {
                if ((bb & 1) == 0)
                {
                    Memory.Write(MiscAddrs.BossKillAddr, (short)0);
                    // Small edge case if the player leaves after the Curse fight before finishing the boat ride, need to monitor again for the boss re-fight potentially
                    if (mask == 1 << (int)Towns.Muska)
                    {
                        int value = ((int)Towns.Muska + 1) * 100;
                        Memory.MonitorAddressForAction<short>(MiscAddrs.BossKillAddr, () => AddBossKill(mask), (o) => { return o == (short)value; });
                    }
                }
                else if ((bb & (1 << (int)Towns.Queens)) == 0)
                {
                    Memory.Write(MiscAddrs.BossKillAddr, (short)100);
                    // Small edge case if the player leaves after the Curse fight before finishing the boat ride, need to monitor again for the boss re-fight potentially
                    if (mask == 1 << (int)Towns.Muska)
                    {
                        int value = ((int)Towns.Muska + 1) * 100;
                        Memory.MonitorAddressForAction<short>(MiscAddrs.BossKillAddr, () => AddBossKill(mask), (o) => { return o == (short)value; });
                    }
                }
                else if ((bb & (1 << (int)Towns.Muska)) == 0)
                {
                    Memory.Write(MiscAddrs.BossKillAddr, (short)300);
                }
            }
        }
        #endregion

        private void _deathlinkService_OnDeathLinkReceived(DeathLink deathLink)
        {
            // TODO kill player x_x
        }

        private static void Client_ItemReceived(object? sender, ItemReceivedEventArgs e)
        {
            long itemId = e.Item.Id;
            if (itemId >= MiscConstants.AttachIdBase)
            {
                InventoryMgmt.IncAttachCount(itemId);
                ItemQueue.AddAttachment(itemId);
            }
            else if (itemId >= MiscConstants.ItemIdBase)
            {
                if (InventoryMgmt.CanGiveItem(itemId))
                {
                    InventoryMgmt.IncItemCount(itemId);
                    ItemQueue.AddItem(itemId);
                }
            }
            else
            {
                GeoInvMgmt.GiveGeorama(itemId);
            }
        }

        private void Client_MessageReceived(object? sender, MessageReceivedEventArgs e)
        {
            if (e.Message.Parts.Any(x => x.Text == "[Hint]: "))
            {
                LogHint(e.Message);
            }
            Log.Logger.Information(JsonConvert.SerializeObject(e.Message));
        }

        private static void LogHint(LogMessage message)
        {
            var newMessage = message.Parts.Select(x => x.Text);

            if (Context.HintList.Any(x => x.TextSpans.Select(y => y.Text) == newMessage))
            {
                return; //Hint already in list
            }
            List<TextSpan> spans = new List<TextSpan>();
            foreach (var part in message.Parts)
            {
                spans.Add(new TextSpan() { Text = part.Text, TextColor = new SolidColorBrush(Color.FromRgb(part.Color.R, part.Color.G, part.Color.B)) });
            }
            lock (_lockObject)
            {
                RxApp.MainThreadScheduler.Schedule(() =>
                {
                    Context.HintList.Add(new LogListItem(spans));
                });
            }
        }

        private static void OnConnected(object? sender, EventArgs? args)
        {
            Log.Logger.Information("Connected to Archipelago");
            Log.Logger.Information($"Playing {Client.CurrentSession.ConnectionInfo.Game} as {Client.CurrentSession.Players.GetPlayerName(Client.CurrentSession.ConnectionInfo.Slot)}");
        }

        private static void OnDisconnected(object? sender, EventArgs? args)
        {
            Log.Logger.Information("Disconnected from Archipelago");
        }

        private async void Reconnect(object? parameters)
        {
            int waitTime = 100;

            while (true)
            {
                if (Client.CurrentSession == null || !Client.CurrentSession.Socket.Connected)
                {
                    waitTime = 0;  // Setup for longer wait time on reconnect attempts

                    if (Client != null)
                    {
                        Client.Disconnect();

                        Client.Connected -= OnConnected;
                        Client.Disconnected -= OnDisconnected;
                        Client.ItemReceived -= Client_ItemReceived;
                        Client.MessageReceived -= Client_MessageReceived;

                        //if (_deathlinkService != null)
                        //{
                        //    _deathlinkService.OnDeathLinkReceived -= _deathlinkService_OnDeathLinkReceived;
                        //    _deathlinkService = null;
                        //}
                        Client.CancelMonitors();
                        //Client.Dispose();
                    }

                    // Connect to archipelago server
                    Client = new ArchipelagoClient(ps2Client);

                    Client.Connected += OnConnected;
                    Client.Disconnected += OnDisconnected;

                    await Client.Connect(Context.Host, "Dark Cloud 1");

                    if (!Client.IsConnected && waitTime < 10_000)
                    {
                        waitTime += 1000;
                    }
                    else if (Client.IsConnected)
                    {
                        Client.ItemReceived += Client_ItemReceived;
                        Client.MessageReceived += Client_MessageReceived;

                        await Client.Login(Context.Slot, !string.IsNullOrWhiteSpace(Context.Password) ? Context.Password : null);

                        Log.Logger.Information("Reconnected to Archipelago");
                        waitTime = 100;
                    }
                }
                else
                {
                    while (locationQueue.TryDequeue(out Location? loc))
                    {
                        Client.SendLocation(loc);
                    }
                }

                Thread.Sleep(waitTime);
            }
        }
    }
}
