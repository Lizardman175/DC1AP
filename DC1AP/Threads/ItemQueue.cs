using Archipelago.Core.Util;
using DC1AP.Constants;
using DC1AP.Georama;
using DC1AP.Items;
using DC1AP.Mem;
using Serilog;
using System.Collections.Concurrent;
using System.Threading;

namespace DC1AP.Threads
{
    internal class ItemQueue
    {
        private const int DisplayTime = 350; // cs, 3.5 seconds
        //private static int MsToCs = 10;  // Convert 1000ths of a second to 100ths

        private static ConcurrentQueue<GeoBuilding> GeoBuildingQueue = new();
        private static ConcurrentQueue<long> InventoryQueue = new();
        private static ConcurrentQueue<long> AttachmentQueue = new();
        private static ConcurrentQueue<string> MsgQueue = new();

        internal static bool checkItems = false;

        internal static bool RunThread = true;

        internal static void AddGeorama(GeoBuilding geoBuilding)
        {
            if (PlayerState.PlayerReady())
                GeoBuildingQueue.Enqueue(geoBuilding);
        }

        internal static void AddItem(long apId)
        {
            InventoryQueue.Enqueue(apId);
        }

        internal static void AddAttachment(long apId)
        {
            AttachmentQueue.Enqueue(apId);
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
            bool result = true;
            bool itemReceived = false;
            bool attachmentReceived = false;

            // Clean out the queues before stopping
            while (RunThread)
            {
                Thread.Sleep(100);

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
                        if (Memory.ReadShort(MiscAddrs.DunMsgIdAddr) == -1 &&
                            Memory.ReadInt(MiscAddrs.AtlaOpeningFlagAddr) == 0 &&
                            Memory.ReadByte(MiscAddrs.LoadingIntoDungeonFlagAddr) == 0 &&
                            MsgQueue.TryDequeue(out string? msg))
                            // TODO nums
                            MessageFuncs.DisplayMessageDungeon(msg, 1, 20, DisplayTime);
                    }

                    result = true;
                    while (result && PlayerState.CanGiveItem() && InventoryQueue.TryDequeue(out long apId))
                    {
                        result = InventoryMgmt.GiveItem(apId);
                        // If we fail to give the item because inventory is full, requeue it
                        if (!result)
                            InventoryQueue.Enqueue(apId);
                        else
                            itemReceived = true;
                    }
                    // Extra flag so we don't spam the player with messages.
                    if (!result && itemReceived)
                    {
                        AddMsg(InventoryQueue.Count + " item(s) remain in queue but inventory is full.");
                    }
                    itemReceived = false;

                    result = true;
                    while (result && PlayerState.CanGiveItem() && AttachmentQueue.TryDequeue(out long apId))
                    {
                        result = InventoryMgmt.GiveAttachment(apId);
                        // If we fail to give the item because inventory is full, requeue it
                        if (!result)
                            AttachmentQueue.Enqueue(apId);
                        else
                            attachmentReceived = true;
                    }
                    if (!result && attachmentReceived)
                    {
                        AddMsg(AttachmentQueue.Count + " attachment(s) remain in queue but inventory is full.");
                    }
                    attachmentReceived = false;

                    // Don't add to the queue if items are already in it to reduce collisions.
                    if (checkItems)
                    {
                        // TODO need to revisit the IsEmpty check and why.  It seems to work though
                        if (GeoBuildingQueue.IsEmpty)
                            GeoInvMgmt.VerifyItems();
                        if (InventoryQueue.IsEmpty && AttachmentQueue.IsEmpty)
                            InventoryMgmt.VerifyItems();
                        checkItems = false;
                    }
                }
                // Player hasn't started the game, or has reset so clear the queues.
                else
                {
                    ClearQueues();
                }
            }
        }

        /// <summary>
        /// Waits until the player has the specified item then removes it from their inventory and exits.
        /// </summary>
        /// <param name="itemId"></param>
        /// <param name="itemCat"></param>
        internal static void RemoveItemLoop(short itemId, ItemCategory itemCat)
        {
            bool found = false;
            while (!found)
            {
                if (!PlayerState.PlayerReady())
                    break;

                switch (itemCat)
                {
                    case ItemCategory.Inventory:
                        found = InventoryMgmt.RemoveInvItem(itemId);
                        break;
                    case ItemCategory.FactoryGeo:
                        found = GeoInvMgmt.RemoveGeoItem(itemId, (int)Towns.Factory);
                        break;
                    default:
                        found = true;
                        break;
                }
                Thread.Sleep(500);
            }
        }

        internal static void ClearQueues()
        {
            GeoBuildingQueue.Clear();
            InventoryQueue.Clear();
            AttachmentQueue.Clear();
            MsgQueue.Clear();
        }
    }
}
