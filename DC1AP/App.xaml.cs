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
 * furnished to do so, subject to the following conditions:
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
using Archipelago.Core.Helpers;
using Archipelago.Core.Models;
using Archipelago.Core.Util;
using Archipelago.MultiClient.Net.BounceFeatures.DeathLink;
using Archipelago.MultiClient.Net.MessageLog.Messages;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using DC1AP.Constants;
using DC1AP.Georama;
using DC1AP.Items;
using DC1AP.Locations;
using DC1AP.Mem;
using DC1AP.Models;
using DC1AP.Threads;
using DC1AP.ViewModels;
using DC1AP.Views;
using Newtonsoft.Json;
using ReactiveUI;
using Serilog;
using System;
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
        public const string ClientVersion = "0.6.1";

        internal static ArchipelagoClient Client { get; set; }

        private static MainWindowViewModel Context;
        private static readonly object _lockObject = new();

        private Thread queueThread;
        private Thread helperThread;
        private Thread chestThread;
        private Thread reconnectThread;
        private GameClient? ps2Client;
        private bool diviningHouseDone = false;
        private bool cathedralDone = false;
        private bool manualDisconnect = false;

        private DeathLinkService? _deathlinkService = null;
        internal static bool deathFromDeathlink = false;
        private static string slotName = string.Empty;
        private static string seedName = string.Empty;

        // Genie handled differently so not in the lists
        private static readonly string[] bossNamesSrc = ["dc1_Dran_", "dc1_Utan_", "dc1_Saia_", "dc1_Curse_", "dc1_Joe_", "dc1_Genie_"];
        private static readonly string[] bossNames = bossNamesSrc;
        private static readonly int[] bossMasks = [1, 2, 4, 8, 16, 32];

        private static App app;

        public override void Initialize()
        {
            AvaloniaXamlLoader.Load(this);

            Context = new MainWindowViewModel() { ConnectButtonEnabled = true };
            Context.ConnectClicked += Context_ConnectClicked;
            Context.CommandReceived += (_, a) => Client?.SendMessage(a.Command);

            AppSettings settings = AppSettings.LoadAppSettings();
            Context.Host = settings.Host;
            Context.Slot = settings.Slot;

            InventoryMgmt.InitInventoryMgmt();
            app = this;
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

            // This feels dirty
            if (Context.ConnectBtnText.StartsWith("Dis") || (Client != null && Client.IsConnected))
                Disconnect();
            else
                Connect(e);

            Context.ConnectButtonEnabled = true;
        }

        private async void Disconnect()
        {
            manualDisconnect = true;
            PlayerState.ClearGameState();
            Client.Disconnect();
            Context.ConnectBtnText = "Connect";
        }

        private async void Connect(ConnectClickedEventArgs e)
        {
            Log.Logger.Information("Connecting...");

            PlayerState.ClearGameState();

            if (Client != null)
            {
                Client.Connected -= OnConnected;
                Client.Disconnected -= OnDisconnected;
                Client.MessageReceived -= Client_MessageReceived;

                if (_deathlinkService != null)
                {
                    _deathlinkService.OnDeathLinkReceived -= _deathlinkService_OnDeathLinkReceived;
                    _deathlinkService = null;
                }
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
            
            await Client.Login(e.Slot, !string.IsNullOrWhiteSpace(e.Password) ? e.Password : null);

            if (!Client.IsConnected || !Client.IsLoggedIn)
            {
                Context.ConnectButtonEnabled = true;
                return;
            }

            manualDisconnect = false;
            AppSettings.SaveAppSettings(new AppSettings(e.Host, e.Slot));

            Client.ItemManager.ItemReceived += Client_ItemReceived;
            Client.ItemManager.ReceiveReady(Client.CurrentSession);
            Client.MessageReceived += Client_MessageReceived;

            slotName = e.Slot;
            
            try
            {
                // Pull out options from AP
                Options.ParseOptions(Client.Options);
            }
            catch (FormatException)
            {
                Log.Logger.Error("Failed to parse options");
                Context.ConnectButtonEnabled = true;
                return;
            }

            if (reconnectThread == null || reconnectThread.ThreadState != ThreadState.Running)
            {
                reconnectThread = new(new ThreadStart(MonitorDisconnect))
                {
                    IsBackground = true
                };
                reconnectThread.Start();
            }

            GeoInvMgmt.Init();
            PlayerState.SetGameState();

            // Initialize things once the player is connected
            if (PlayerState.PlayerReady())
            {
                PlayerReady(slotName);
                
                // Handle default names if the player connects while ready then resets the game at some point
                Memory.MonitorAddressForAction<short>(MiscAddrs.ToanNameAddr, () => SetDefaultNames(true), (o) => { return o == 0; });
            }
            else
            {
                PlayerNotReady(slotName);

                // Handle default names if the player connects while not ready
                new Thread(() => SetDefaultNames(true))
                {
                    IsBackground = true
                }.Start();
            }

            if (Options.DeathLink)
            {
                _deathlinkService = Client.EnableDeathLink();
                _deathlinkService.OnDeathLinkReceived += _deathlinkService_OnDeathLinkReceived;
                ListenForDeath();
            }

            if (queueThread == null)
            {
                queueThread = new Thread(new ThreadStart(ItemQueue.ThreadLoop))
                {
                    IsBackground = true
                };
                queueThread.Start();
            }

            if (helperThread == null)
            {
                helperThread = new Thread(new ThreadStart(HelperThread.DoLoop))
                {
                    IsBackground = true
                };
                helperThread.Start();
            }

            Context.ConnectBtnText = "Disconnect";
        }

        #region PS2
        private static byte bossKillTest = 0;

        private GameClient? PS2Connect()
        {
            String gameId = "BASCUS-97111dkcloud";

            GameClient client = new("pcsx2-qt");
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
            if (!Client.CurrentSession.Socket.Connected)
                return;

            Thread.Sleep(50);
            string currSlot = OpenMem.GetSlotName();
            int slotNum = Client.CurrentSession.Players.ActivePlayer.Slot;
            seedName = Client.CurrentSession.RoomState.Seed;

            for (int i = 0; i < Options.Goal; i++)
            {
                bossNames[i] = bossNamesSrc[i] + slotNum;
            }

            // First load for this save, so do extra stuff
            if (currSlot == "")
            {
                // Check first atla in DBC. If already set, then the user may have loaded a vanilla save.
                if (Memory.ReadInt(GeoAddrs.AtlaFlagAddrs[0]) != MiscConstants.AtlaUnavailable)
                {
                    PlayerState.ClearGameState();
                    Log.Logger.Error("Vanilla save loaded or first dungeon already entered.  Load a rando save or start a clean save file.   ");
                    return;
                }
                OpenMem.SetSlotData(slotName);
                EventMasks.InitMasks();
                Weapons.GiveCharWeapon(0);
                InventoryMgmt.GiveFreeFeather();

                for (int i = 0; i < Options.Goal; i++)
                {
                    Client.CurrentSession.DataStorage[bossNames[i]] = false;
                }
            }
            else if (currSlot != slotName)
            {
                // Padding because Avalonia keeps cutting things off...
                PlayerState.ClearGameState();
                Log.Logger.Error("Wrong slot name. Current save is using slot: " + currSlot + "      ");
                return;
            }
            else if (!OpenMem.TestRoomSeed(Client.CurrentSession.RoomState.Seed))
            {
                // The call in the if above logs an error for us
                PlayerState.ClearGameState();
                return;
            }

            SetDefaultNames(false);
            GeoInvMgmt.InitBuildings();
            CharFuncs.Init(slotNum);
            Enemies.MultiplyABS();
            InventoryMgmt.MultiplyAttachments();
            ShopMgmt.UpdateShops();
            Fish.CheckFishLog();
            Fish.WatchFishCatchField();

            // Check for any missing items after a connect/reconnect
            ItemQueue.checkItems = true;

            // Skip needing Yaya to dance on your head if doing Saia once the building event viewed flag is set.
            if (Options.Goal >= 3 && !EventMasks.YayaDone())
            {
                diviningHouseDone = false;
                cathedralDone = false;

                Memory.MonitorAddressForAction<short>(GeoAddrs.YayaBldEventFlag, AckDivHouse, (o) => { return o >= 1; });
                Memory.MonitorAddressForAction<short>(GeoAddrs.CathedralBldEventFlag, AckCathedral, (o) => { return o >= 1; });
            }

            PlayerState.SetGameState();
            MiracleChestMgmt.Init();

            // Other threads shouldn't stop, but if disconnecting from a slot without MC shuffle and connecting to one with MC shuffle, need to test thread state.
            if (chestThread == null || chestThread.ThreadState == ThreadState.Stopped)
            {
                chestThread = new Thread(new ThreadStart(MiracleChestMgmt.DoLoop))
                {
                    IsBackground = true
                };
                chestThread.Start();
            }

            // Watch for the player to reset the game, then change the valid state flag and ready up to connect again.
            Memory.MonitorAddressForAction<int>(MiscAddrs.TimeOfDayAddr, () => PlayerNotReady(slotName), (o) => { return o == 0; });
            WatchGoal();

            Log.Logger.Information("Connected and Ready!");
        }

        private void PlayerNotReady(string slotName)
        {
            PlayerState.ClearGameState();
            ItemQueue.ClearQueues();
            if (Client.CurrentSession.Socket.Connected)
            {
                HelperThread.Startup();
                Memory.MonitorAddressForAction<int>(MiscAddrs.TimeOfDayAddr, () => PlayerReady(slotName), (o) => { return o != 0; });
            }
        }

        private static void SetDefaultNames(bool sleep)
        {
            if (sleep)
                Thread.Sleep(3000);

            CharFuncs.SetDefaultCharName(MiscAddrs.ToanNameAddr, Options.ToanName);
            CharFuncs.SetDefaultCharName(MiscAddrs.XiaoNameAddr, Options.XiaoName);
            CharFuncs.SetDefaultCharName(MiscAddrs.GoroNameAddr, Options.GoroName);
            CharFuncs.SetDefaultCharName(MiscAddrs.RubyNameAddr, Options.RubyName);
            CharFuncs.SetDefaultCharName(MiscAddrs.UngagaNameAddr, Options.UngagaName);
            // Ungaga uses the mem card address since the player can't change his name in game
            CharFuncs.SetDefaultCharName(MiscAddrs.UngagaNameSaveAddr, Options.UngagaName);
            CharFuncs.SetDefaultCharName(MiscAddrs.OsmondNameAddr, Options.OsmondName);

            Memory.MonitorAddressForAction<short>(MiscAddrs.ToanNameAddr, () => SetDefaultNames(true), (o) => { return o == 0; });
        }

        private void AckDivHouse()
        {
            diviningHouseDone = true;
            if (cathedralDone)
                EventMasks.SkipYaya();
        }

        private void AckCathedral()
        {
            cathedralDone = true;
            if (diviningHouseDone)
                EventMasks.SkipYaya();
        }

        internal static async Task SendLocation(int locId)
        {
            // Test slot info before sending checks in case the player has loaded a save state to avoid releasing extra items.
            if (PlayerState.GetGameState() && OpenMem.TestSlotInfo(slotName, seedName))
            {
                Location loc = new()
                {
                    Id = locId
                };

                if (Client.CurrentSession != null && Client.CurrentSession.Socket.Connected)
                    App.Client.SendLocationAsync(loc);
            }
            else
            {
                PlayerState.ClearGameState();
            }
        }

        private void ListenForDeath()
        {
            for (int i = 0; i < MiscAddrs.HpAddrs.Length; i++)
            {
                uint addr = MiscAddrs.HpAddrs[i];
                short curValue = Memory.ReadShort(addr);

                // Connected while player is dead, don't send a death and wait for revive (or for the char to be recruited)
                if (curValue <= 0)
                    Memory.MonitorAddressForAction<short>(addr, () => HandleCharRevive(addr), (o) => { return o > 0; });
                else
                    Memory.MonitorAddressForAction<short>(addr, () => HandleCharDeath(addr), (o) => { return o <= 0; });
            }
        }

        private void HandleCharDeath(uint addr)
        {
            // Don't death link on game reset
            if (PlayerState.PlayerReady() && !deathFromDeathlink)
            {
                DeathLink dl = new(slotName);
                _deathlinkService.SendDeathLink(dl);
                Log.Logger.Information("DeathLink: Sending Death to your friends...");
            }

            deathFromDeathlink = false;

            // Monitor for the char to be revived.
            Memory.MonitorAddressForAction<short>(addr, () => HandleCharRevive(addr), (o) => { return o > 0; });
        }

        private void HandleCharRevive(uint addr)
        {
            Memory.MonitorAddressForAction<short>(addr, () => HandleCharDeath(addr), (o) => { return o <= 0; });
        }

        private static void WatchGoal()
        {
            int maxLoop = Options.Goal;
            if (Client.CurrentSession.Locations.AllLocations.Contains(971117405)) maxLoop++;

            if (Options.AllBosses)
            {
                byte currKills = Memory.ReadByte(OpenMem.GoalAddr);

                if ((currKills & MiscConstants.DarkGenieMask) == 0 & Options.Goal >= 6 && Client.CurrentSession.DataStorage[bossNames[5]] == true)
                {
                    bossKillTest |= MiscConstants.DarkGenieMask;
                    currKills |= MiscConstants.DarkGenieMask;
                    Memory.WriteByte(OpenMem.GoalAddr, currKills);
                }

                for (int i = 0; i < maxLoop; i++)
                {
                    byte mask = (byte)(1 << i);
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
                            Memory.MonitorAddressForAction<short>(MiscAddrs.BossKillAddr, () => AddBossKill(mask, true), (o) => { return o == (short)value; });
                        }

                        // Genie shouldn't get reset
                        if (i < bossNames.Length)
                        {
                            Client.CurrentSession.DataStorage[bossNames[i]] = false;
                        }
                    }
                    else if (i < bossNames.Length)
                    {
                        Client.CurrentSession.DataStorage[bossNames[i]] = true;
                    }
                }
            }
            else
            {
                // For some reason, the Boss Kill Flag doesn't set for Utan so use the floor kill count instead
                if (Options.Goal == 2)
                {
                    Memory.MonitorAddressForAction<byte>(MiscAddrs.UtanFlag, Client.SendGoalCompletion, (o) => { return o != 0; });
                }
                else if (Client.CurrentSession.Locations.AllLocations.Contains(971117405))
                {
                    Memory.MonitorAddressForAction<short>(MiscAddrs.BossKillAddr, Client.SendGoalCompletion, (o) => { return o == 700; });
                    Memory.MonitorAddressForAction<short>(MiscAddrs.BossKillAddr, SendFGoalCompletion, (o) => { return o == 600; });
                }
                else
                {
                    Memory.MonitorAddressForAction<short>(MiscAddrs.BossKillAddr, Client.SendGoalCompletion, (o) => { return o == Options.Goal * 100; });
                }
            }

            // If reloading after the genie fight with more bosses, add the boss kill here
            if (Client.CurrentSession.DataStorage[bossNames[5]] == true)
                AddBossKill(MiscConstants.DarkGenieMask);
        }

        /// <summary>
        /// Mask the boss kills into the goal byte.
        /// </summary>
        /// <param name="mask">Bit to set for killed boss.</param>
        /// <param name="trueKill">Flag indicating if this was called from actually killing the boss.</param>
        internal static void AddBossKill(byte mask, bool trueKill = false)
        {
            byte bb = Memory.ReadByte(OpenMem.GoalAddr);
            bb |= mask;
            Memory.WriteByte(OpenMem.GoalAddr, bb);

            try
            {
                // Track on the server that the Dark Genie has been killed
                if (mask == MiscConstants.DarkGenieMask)
                {
                    if (!Client.CurrentSession.DataStorage[bossNames[5]] && Client.CurrentSession.Locations.AllLocations.Contains(971117405))
                        SendFGoalCompletion();

                    Client.CurrentSession.DataStorage[bossNames[5]] = true;
                    // The game will reset after the credits but it doesn't clear the time of day field.  This will force PlayerNotReady() to be called to avoid issues.
                    // Only call if from actually beating the boss.  If the item is collected, clearing Time of Day will cause issues.
                    if (trueKill)
                        Memory.Write(MiscAddrs.TimeOfDayAddr, 0);
                }
                else if (bossMasks.IndexOf(mask) != -1)
                {
                    Client.CurrentSession.DataStorage[bossNames[bossMasks.IndexOf(mask)]] = true;
                }

                if (bb == bossKillTest)
                {
                    Client.SendGoalCompletion();
                    return;
                }
            }
            catch (Exception)
            {
                // Ignore. The DataStorage access can collide harmlessly when there are duplicate monitors due to resets (or whatever reason the game keeps tripping PlayerReady/NotReady)
                // TODO since we can't pass cancellation tokens to the monitors, should probably monitor boss kills in HelperThread or similar instead of this jank
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
                else if ((bb & (1 << (int)Towns.Factory)) == 0)
                {
                    Memory.Write(MiscAddrs.BossKillAddr, (short)400);
                }
                else if ((bb & (1 << (int)Towns.Castle)) == 0)
                {
                    Memory.Write(MiscAddrs.BossKillAddr, (short)500);
                }
            }
        }

        private static void SendFGoalCompletion()
        {
            Log.Logger.Warning("A far stronger foe yet awaits you...");
        }
        #endregion

        private void _deathlinkService_OnDeathLinkReceived(DeathLink deathLink)
        {
            // Let the thread kill the player once they are in a dungeon
            HelperThread.doDeathLink = true;
            HelperThread.deathlinkSource = deathLink.Source;
        }

        private static void Client_ItemReceived(object? sender, ItemReceivedEventArgs e)
        {
            long itemId = e.Item.Id;

            if (itemId >= MiscConstants.AttachIdBase)
            {
                ItemQueue.AddAttachment(itemId);
            }
            else if (itemId >= MiscConstants.ItemIdBase)
            {
                if (MiscConstants.KeyItemApIds.Contains(itemId))
                    ItemQueue.AddKeyItem(itemId);
                else
                    ItemQueue.AddItem(itemId);
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
                //LogHint(e.Message);
                // TODO fix hint logging with Avalonia
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
            app.Reconnect();
        }

        private async void Reconnect()
        {
            int waitTime = 100;

            while ((Client.CurrentSession == null || !Client.CurrentSession.Socket.Connected) && !manualDisconnect)
            {
                // Connect to archipelago server
                Client = new ArchipelagoClient(ps2Client);

                Client.Connected += OnConnected;
                Client.Disconnected += OnDisconnected;

                await Client.Connect(Context.Host, "Dark Cloud 1");

                if (!Client.IsConnected && waitTime < 10_000)
                {
                    waitTime += 1000;
                }

                Thread.Sleep(waitTime);
            }

            if (Client.CurrentSession != null && Client.CurrentSession.Socket.Connected)
            {
                Client.MessageReceived += Client_MessageReceived;

                await Client.Login(Context.Slot, !string.IsNullOrWhiteSpace(Context.Password) ? Context.Password : null);

                Thread.Sleep(50);

                Client.ItemManager.ItemReceived += Client_ItemReceived;
                Client.ItemManager.ReceiveReady(Client.CurrentSession);

                Thread.Sleep(100);
                Log.Logger.Information("Reconnected to Archipelago");

                PlayerNotReady(slotName);

                if (reconnectThread.ThreadState != ThreadState.Running)
                {
                    reconnectThread = new(new ThreadStart(MonitorDisconnect))
                    {
                        IsBackground = true
                    };
                    reconnectThread.Start();
                }
            }
        }

        internal void MonitorDisconnect()
        {
            while (true)
            {
                if ((Client.CurrentSession == null || !Client.CurrentSession.Socket.Connected) && !manualDisconnect)
                {
                    PlayerNotReady("");
                    Client.Disconnect();

                    Client.Connected -= OnConnected;
                    Client.Disconnected -= OnDisconnected;
                    Client.MessageReceived -= Client_MessageReceived;
                    _deathlinkService?.OnDeathLinkReceived -= _deathlinkService_OnDeathLinkReceived;
                    _deathlinkService = null;

                    break;
                }

                Thread.Sleep(100);
            }
        }
    }
}
