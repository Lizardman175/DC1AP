using Archipelago.Core.Util;
using DC1AP.Constants;
using DC1AP.Georama;
using DC1AP.Mem;
using Serilog;
using System.Collections.Concurrent;

namespace DC1AP.Threads
{
    internal class ItemQueue
    {
        private const int DisplayTime = 350; // cs, 3.5 seconds
        //private static int MsToCs = 10;  // Convert 1000ths of a second to 100ths

        internal static ConcurrentQueue<(GeoBuilding, long)> GeoBuildingQueue = new();
        internal static ConcurrentQueue<(GeoItem, Towns)> GeoItemQueue = new();
        internal static ConcurrentQueue<string> MsgQueue = new();

        // TODO not currently used
        internal static bool RunThread = true;

        internal static void AddGeoBuilding(GeoBuilding geoBuilding, long id)
        {
            if (PlayerState.PlayerReady())
                GeoBuildingQueue.Enqueue((geoBuilding, id));
        }

        internal static void AddGeoItem(GeoItem geoItem, Towns town)
        {
            if (PlayerState.PlayerReady())
                GeoItemQueue.Enqueue((geoItem, town));
        }

        internal static void AddMsg(string msg)
        {
            if (PlayerState.PlayerReady())
            {
                Log.Logger.Information(msg);
                MsgQueue.Enqueue(msg);
            }
        }

        internal static void ThreadLoop(object? parameters)
        {
            RunThread = true;

            // Clean out the queues before stopping
            while (RunThread || !GeoItemQueue.IsEmpty || !GeoBuildingQueue.IsEmpty || !MsgQueue.IsEmpty)
            {
                if (PlayerState.PlayerReady())
                {
                    short curIndex = OpenMem.GetIndex();
                    if (App.Client.GameState != null && App.Client.GameState.LastCheckedIndex > curIndex)
                    {
                        for (; curIndex < App.Client.GameState.ReceivedItems.Count; curIndex++)
                            GeoInvMgmt.GiveItem(App.Client.GameState.ReceivedItems[curIndex].Id);
                        // TODO don't do this? may incur race condition with GeoItem etc. Those shouldn't increment this if they're already set so it shouldn't cause issue though.
                        OpenMem.SetIndex(curIndex);
                    }

                    // Clear remaining messages once player leaves dungeon
                    if (!PlayerState.IsPlayerInDungeon() && !MsgQueue.IsEmpty) MsgQueue.Clear();

                    // Geo items can only be received in dungeon
                    if (PlayerState.CanGiveItemDungeon())
                    {
                        while (PlayerState.CanGiveItemDungeon() && GeoBuildingQueue.TryDequeue(out (GeoBuilding, long) geoBuilding))
                        {
                            geoBuilding.Item1.GiveBuilding(geoBuilding.Item2);
                        }
                        while (PlayerState.CanGiveItemDungeon() && GeoItemQueue.TryDequeue(out (GeoItem, Towns) geoItem))
                        {
                            geoItem.Item1.GiveItem(geoItem.Item2);
                        }

                        // Display queued up messages after the last one fades.
                        // TODO need extra flag checks; message won't display as player is entering a dungeon floor or if atla are collected too close together, when other messages are displayed, etc.
                        if (Memory.ReadShort(MiscAddrs.DunMsgDurAddr) == 0 && MsgQueue.TryDequeue(out string? msg))
                            // TODO nums
                            MessageFuncs.DisplayMessageDungeon(msg, 1, 20, DisplayTime);
                        
                    }
                }
                // Player hasn't started the game, or has reset so clear the queues.
                else
                {
                    GeoBuildingQueue.Clear();
                    GeoItemQueue.Clear();
                    MsgQueue.Clear();
                }

                Thread.Sleep(1000);
            }
        }
    }
}
