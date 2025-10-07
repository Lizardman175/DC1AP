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

        private static ConcurrentQueue<GeoBuilding> GeoBuildingQueue = new();
        private static ConcurrentQueue<string> MsgQueue = new();

        internal static bool checkItems = false;

        // TODO not currently used
        internal static bool RunThread = true;

        internal static void AddGeorama(GeoBuilding geoBuilding)
        {
            if (PlayerState.PlayerReady())
                GeoBuildingQueue.Enqueue(geoBuilding);
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
            while (RunThread || !GeoBuildingQueue.IsEmpty || !MsgQueue.IsEmpty)
            {
                Thread.Sleep(1000);

                if (PlayerState.PlayerReady())
                {
                    // Clear remaining messages once player leaves dungeon
                    if (!PlayerState.IsPlayerInDungeon() && !MsgQueue.IsEmpty) MsgQueue.Clear();

                    // Geo items can only be received in dungeon
                    if (PlayerState.CanGiveItemDungeon())
                    {
                        while (PlayerState.CanGiveItemDungeon() && GeoBuildingQueue.TryDequeue(out GeoBuilding geoBuilding))
                        {
                            geoBuilding.GiveBuilding();
                        }

                        // Display queued up messages after the last one fades.
                        // TODO need extra flag checks; message won't display as player is entering a dungeon floor or if atla are collected too close together, when other messages are displayed, etc.
                        if (Memory.ReadShort(MessageFuncs.DunMsgDurAddr) == 0 && MsgQueue.TryDequeue(out string? msg))
                            // TODO nums
                            MessageFuncs.DisplayMessageDungeon(msg, 1, 20, DisplayTime);
                        
                    }

                    // Don't add to the queue if items are already in it to reduce collisions.
                    if (checkItems && GeoBuildingQueue.Count == 0)
                    {
                        GeoInvMgmt.VerifyItems();
                        checkItems = false;
                    }
                }
                // Player hasn't started the game, or has reset so clear the queues.
                else
                {
                    GeoBuildingQueue.Clear();
                    MsgQueue.Clear();
                }
            }
        }
    }
}
