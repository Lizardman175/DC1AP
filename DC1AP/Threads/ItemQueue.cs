using Archipelago.Core.Util;
using DC1AP.Constants;
using DC1AP.Georama;
using DC1AP.Items;
using DC1AP.Mem;
using Serilog;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading;

namespace DC1AP.Threads
{
    internal class ItemQueue
    {
        private const int DisplayTime = 350; // cs, 3.5 seconds
        //private static int MsToCs = 10;  // Convert 1000ths of a second to 100ths

        private static ConcurrentQueue<GeoBuilding> geoBuildingQueue = new();
        private static ConcurrentQueue<long> keyItemQueue = new();
        private static ConcurrentQueue<long> inventoryQueue = new();
        private static ConcurrentQueue<long> attachmentQueue = new();
        private static ConcurrentQueue<string> msgQueue = new();

        private static int oldKeyCount = 0;
        private static int oldInvCount = 0;
        private static int oldAttachCount = 0;

        internal static bool checkItems = false;

        internal static bool runThread = true;

        internal static void AddGeorama(GeoBuilding geoBuilding)
        {
            if (PlayerState.PlayerReady())
                geoBuildingQueue.Enqueue(geoBuilding);
        }

        internal static void AddKeyItem(long apId)
        {
            keyItemQueue.Enqueue(apId);
        }

        internal static void AddItem(long apId)
        {
            if (CanQueueItem(apId))
                inventoryQueue.Enqueue(apId);
        }

        internal static void AddAttachment(long apId)
        {
            attachmentQueue.Enqueue(apId);
        }

        internal static void AddMsg(string msg)
        {
            if (PlayerState.PlayerReady())
            {
                Log.Logger.Information(msg);
                msgQueue.Enqueue(msg);
            }
        }

        /// <summary>
        /// Tests if the given item can go into the queue based on recruited characters.
        /// </summary>
        /// <param name="apId"></param>
        /// <returns></returns>
        private static bool CanQueueItem(long apId)
        {
            if ((apId == MiscConstants.CookieId && !CharFuncs.Osmond) ||
                    (apId == MiscConstants.JerkyId && !CharFuncs.Ungaga) ||
                    (apId == MiscConstants.ParfaitId && !CharFuncs.Ruby) ||
                    (apId == MiscConstants.GrassCakeId && !CharFuncs.Goro) ||
                    (apId == MiscConstants.FishCandyId && !CharFuncs.Xiao))
            {
                return false;
            }
            // Limit FoE/Gourd based on recruited chars to avoid flooding inventory with unusable items.
            else if (apId == MiscConstants.GourdId || apId == MiscConstants.FruitOfEdenId)
            {
                byte count = (byte)(OpenMem.ReadItemCountValue(apId) + inventoryQueue.Count(val => val == apId));
                byte max = 7;

                if (CharFuncs.Osmond)
                    return true;
                else if (CharFuncs.Ungaga)
                    max *= 5;
                else if (CharFuncs.Ruby)
                    max *= 4;
                else if (CharFuncs.Goro)
                    max *= 3;
                else if (CharFuncs.Xiao)
                    max *= 2;

                return max > count;
            }

            return true;
        }

        internal static void ThreadLoop(object? parameters)
        {
            runThread = true;
            bool result = true;
            bool itemReceived = false;
            bool attachmentReceived = false;

            // Clean out the queues before stopping
            while (runThread)
            {
                Thread.Sleep(100);

                if (PlayerState.PlayerReady())
                {
                    // Clear remaining messages once player leaves dungeon
                    if (!PlayerState.IsPlayerInDungeon() && !msgQueue.IsEmpty) msgQueue.Clear();

                    // Geo items can only be received in dungeon
                    if (PlayerState.CanGiveItemDungeon())
                    {
                        while (PlayerState.CanGiveItemDungeon() && geoBuildingQueue.TryDequeue(out GeoBuilding geoBuilding))
                        {
                            geoBuilding.GiveBuilding();
                        }

                        // Display queued up messages after the last one fades.
                        if (Memory.ReadShort(MiscAddrs.DunMsgIdAddr) == -1 &&
                            Memory.ReadInt(MiscAddrs.AtlaOpeningFlagAddr) == 0 &&
                            Memory.ReadByte(MiscAddrs.LoadingIntoDungeonFlagAddr) == 0 &&
                            msgQueue.TryDequeue(out string? msg))
                            // TODO nums
                            MessageFuncs.DisplayMessageDungeon(msg, 1, 20, DisplayTime);
                    }

                    itemReceived = false;
                    result = true;

                    while (result && PlayerState.CanGiveItem() && keyItemQueue.TryDequeue(out long apId))
                    {
                        result = InventoryMgmt.GiveItem(apId);
                        // If we fail to give the item because inventory is full, requeue it
                        if (!result)
                        {
                            keyItemQueue.Enqueue(apId);
                            break;
                        }
                        else
                            itemReceived = true;
                    }
                    // Extra flag so we don't spam the player with messages.
                    if (!result && (itemReceived || oldKeyCount != keyItemQueue.Count))
                    {
                        AddMsg(keyItemQueue.Count + " key item(s) remain in queue but inventory is full.");
                    }
                    oldKeyCount = keyItemQueue.Count;
                    itemReceived = false;

                    result = true;
                    while (result && PlayerState.CanGiveItem() && inventoryQueue.TryDequeue(out long apId))
                    {
                        result = InventoryMgmt.GiveItem(apId);
                        // If we fail to give the item because inventory is full, requeue it
                        if (!result)
                        {
                            inventoryQueue.Enqueue(apId);
                            break;
                        }
                        else
                            itemReceived = true;
                    }
                    // Extra flag so we don't spam the player with messages.
                    if (!result && (itemReceived || oldInvCount != inventoryQueue.Count))
                    {
                        AddMsg(inventoryQueue.Count + " item(s) remain in queue but inventory is full.");
                    }
                    oldInvCount = inventoryQueue.Count;

                    result = true;
                    while (result && PlayerState.CanGiveItem() && attachmentQueue.TryDequeue(out long apId))
                    {
                        result = InventoryMgmt.GiveAttachment(apId);
                        // If we fail to give the item because inventory is full, requeue it
                        if (!result)
                        {
                            attachmentQueue.Enqueue(apId);
                            break;
                        }
                        else
                            attachmentReceived = true;
                    }
                    if (!result && (attachmentReceived || oldAttachCount != attachmentQueue.Count))
                    {
                        AddMsg(attachmentQueue.Count + " attachment(s) remain in queue but inventory is full.");
                    }

                    oldAttachCount = attachmentQueue.Count;
                    attachmentReceived = false;

                    if (checkItems)
                    {
                        InventoryMgmt.VerifyItems();
                        GeoInvMgmt.VerifyItems();
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
            geoBuildingQueue.Clear();
            ClearItemQueues();
            msgQueue.Clear();
        }

        internal static void ClearItemQueues()
        {
            keyItemQueue.Clear();
            inventoryQueue.Clear();
            attachmentQueue.Clear();
        }
    }
}
