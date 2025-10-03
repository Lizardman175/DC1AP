/*
 * MIT License

 * Copyright (c) 2025 ArsonAssassin

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
using Archipelago.Core.MauiGUI;
using Archipelago.Core.MauiGUI.Models;
using Archipelago.Core.MauiGUI.ViewModels;
using Archipelago.Core.Models;
using Archipelago.Core.Util;
using Archipelago.MultiClient.Net.BounceFeatures.DeathLink;
using Archipelago.MultiClient.Net.MessageLog.Messages;
using DC1AP.Constants;
using DC1AP.Georama;
using DC1AP.Mem;
using DC1AP.Threads;
using Newtonsoft.Json;
using Serilog;
using System.Collections.Concurrent;

// Adapted from github.com/ArsonAssassin/Archipelago-Maui-Template
namespace DC1AP
{
    public partial class App : Application
    {
        internal static ArchipelagoClient Client { get; set; }

        private static MainPageViewModel Context;
        private static readonly object _lockObject = new();

        private static ConcurrentQueue<Archipelago.Core.Models.Location> locationQueue = new();

        //private DeathLinkService _deathlinkService;
        private Thread queueThread;
        private Thread helperThread;
        private GenericGameClient? ps2Client;

        public App()
        {
            InitializeComponent();

            Context = new MainPageViewModel();
            Context.ConnectClicked += Context_ConnectClicked;
            Context.CommandReceived += (e, a) =>
            {
                Client?.SendMessage(a.Command);
            };
            // TODO save last used host?
            Context.Host = "localhost:38281";
            //Context.Slot = "DC1";
            MainPage = new MainPage(Context);
            Context.ConnectButtonEnabled = true;
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

            ps2Client = PS2Connect(e.Slot);

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

            if (!Client.IsConnected)
            {
                Context.ConnectButtonEnabled = true;
                return;
            }

            Thread reconnectThread = new Thread(new ParameterizedThreadStart(Reconnect))
            {
                IsBackground = true,
            };
            reconnectThread.Start();

            //if (Client.Options.ContainsKey("EnableDeathlink") && (bool)Client.Options["EnableDeathlink"])
            //{
            //    var _deathlinkService = Client.EnableDeathLink();
            //    _deathlinkService.OnDeathLinkReceived += _deathlinkService_OnDeathLinkReceived;
            //    // TODO listen for player death
            //}

            // Pull out options from AP
            Options.ParseOptions(Client.Options);

            WatchGoal();

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
        }

        #region PS2
        private static byte bossKillTest = 0;

        private GenericGameClient? PS2Connect(String slotName)
        {
            String gameId = "BASCUS-97111dkcloud";

            GenericGameClient client = new GenericGameClient("pcsx2-qt");
            try
            {
                client.Connect();
            }
            catch (System.ArgumentException)
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

            GeoInvMgmt.Init();

            // Initialize things once the player is connected
            if (PlayerState.PlayerReady())
            {
                PlayerReady(slotName);
            }
            else
            {
                PlayerNotReady(slotName);
            }

            return client;
        }

        private void PlayerReady(string slotName)
        {
            String currSlot = OpenMem.GetSlotName();
            if (currSlot != slotName && currSlot.Length > 0)
            {
                Log.Logger.Error("Wrong slot name. Current save is using slot \"" + currSlot + "\".");
                PlayerState.ValidGameState = false;
                return;
            }

            if (currSlot == "")
            {
                // Store player's slot name into memcard
                OpenMem.SetSlotName(slotName);
                OpenMem.SetIndex(0);

                EventMasks.InitMasks();

                // Show the geo menu for each town randomized and init the respective tables
                for (int i = 0; i < Options.Goal; i++)
                {
                    Memory.Write(GeoAddrs.GeoMenuFlagAddrs[i], (short)1);
                }

                GeoInvMgmt.InitBuildings(true);
            }
            else GeoInvMgmt.InitBuildings(false);

            CharFuncs.Init();
            PlayerState.ValidGameState = true;

            // Watch for the player to reset the game, then change the valid state flag and ready up to connect again.
            Memory.MonitorAddressForAction<byte>(MiscAddrs.PlayerState, () => PlayerNotReady(slotName), (o) => { return o <= 1; });

            // Skip needing Yaya to dance on your head if doing Saia once the building event viewed flag is set.
            if (Options.Goal >= 3)
            {
                Memory.MonitorAddressForAction<short>(GeoAddrs.YayaBldEventFlag, () => EventMasks.SkipYaya(), (o) => { return o  >= 1; });
            }
        }

        private void PlayerNotReady(string slotName)
        {
            PlayerState.ValidGameState = false;
            Memory.MonitorAddressForAction<byte>(MiscAddrs.PlayerState, () => PlayerReady(slotName), (o) => { return o > 1; });
        }

        internal static async Task SendLocation(int locId)
        {
            Archipelago.Core.Models.Location loc = new()
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
                            Memory.MonitorAddressForAction<short>(MiscAddrs.BossKillAddr, () => AddBossKill(mask), (o) => { return o == (short)value; });
                        }
                    }
                }

                Memory.MonitorAddressForAction<byte>(OpenMem.GoalAddr, () => Client.SendGoalCompletion(), (o) => { return CheckBossKills(); });
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
        /// <param name="value">Value with bit set for killed boss.</param>
        private static void AddBossKill(byte value)
        {
            byte b = Memory.ReadByte(OpenMem.GoalAddr);
            b |= value;
            Memory.WriteByte(OpenMem.GoalAddr, b);
        }

        private static bool CheckBossKills()
        {
            return Memory.ReadByte(OpenMem.GoalAddr) == bossKillTest;
        }
        #endregion

        private void _deathlinkService_OnDeathLinkReceived(DeathLink deathLink)
        {
            // TODO kill player :(
        }

        private static void Client_ItemReceived(object? sender, ItemReceivedEventArgs e)
        {
            LogItem(e.Item);  // TODO not working?  I think this goes to the received items tab.

            // TODO miracle chests: test the item id and add inventory item instead of geo
            GeoInvMgmt.GiveItem(e.Item.Id);
        }

        private void Client_MessageReceived(object? sender, Archipelago.Core.Models.MessageReceivedEventArgs e)
        {
            if (e.Message.Parts.Any(x => x.Text == "[Hint]: "))
            {
                LogHint(e.Message);
            }
            Log.Logger.Information(JsonConvert.SerializeObject(e.Message));
        }

        private static void LogItem(Archipelago.Core.Models.Item item)
        {
            var messageToLog = new LogListItem(new List<TextSpan>()
            {
                new TextSpan(){Text = $"[{item.Id.ToString()}] -", TextColor = Color.FromRgb(255, 255, 255)},
                new TextSpan(){Text = $"{item.Name}", TextColor = Color.FromRgb(200, 255, 200)},
                //new TextSpan(){Text = $"x{item.Quantity.ToString()}", TextColor = Color.FromRgb(200, 255, 200)}
            });
            lock (_lockObject)
            {
                Application.Current.Dispatcher.DispatchAsync(() =>
                {
                    Context.ItemList.Add(messageToLog);
                });
            }
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
                spans.Add(new TextSpan() { Text = part.Text, TextColor = Color.FromRgb(part.Color.R, part.Color.G, part.Color.B) });
            }
            lock (_lockObject)
            {
                Application.Current.Dispatcher.DispatchAsync(() =>
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

        protected override Window CreateWindow(IActivationState activationState)
        {
            var window = base.CreateWindow(activationState);
            if (DeviceInfo.Current.Platform == DevicePlatform.WinUI)
            {
                window.Title = "Dark Cloud 1 Archipelago Randomizer";
            }
            window.Width = 600;

            return window;
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
                        Client.Dispose();
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
                    while (locationQueue.TryDequeue(out Archipelago.Core.Models.Location? loc))
                    {
                        Client.SendLocation(loc);
                    }
                }

                Thread.Sleep(waitTime);
            }
        }
    }
}
