using Archipelago.Core.Util;
using DC1AP.Constants;
using DC1AP.Threads;
using System;
using System.Text.Json;

namespace DC1AP.Items
{
    internal class InventoryMgmt
    {
        private static Dictionary<long, InvItem>? ItemData;
        private static Dictionary<long, Attachment>? AttachmentData;

        private const uint MaxAttachCount = 40;
        private const uint InvMaxAddr = 0x01CDD8AC;  // Byte.  Can't exceed 100 or we run past the buffer.
        private const uint InvCurAddr = 0x01CDD8AD;  // Byte.  Next byte starts the active item shorts, followed by 3 shorts giving count of the active items per slot, then shorts for the other items.
        private const uint FirstAttchAddr = 0x01CE1A48;
        private const short FeatherDuration = 0x42cc;
        private static Random random = new Random();

        /*
         *  0 for most items. Duration for things like feathers, amulets. Gives value item restores as well for curatives
         *  but doesn't seem to do anything if changed. -1 or 0 for no item (sometimes ghost values as well. Seems to be
         *  from moving items from the active list with square?)
         */
        internal const uint FirstItemDurationAddr = 0x01CDD988; // Short

        // Add 1 byte for the CurAddr field, then 2 for each short past the first addr
        //private static uint[] ActiveItemAddrs = [InvCurAddr + 1, InvCurAddr + 3, InvCurAddr + 5];
        //private static uint[] ActiveItemCountAddrs = [InvCurAddr + 7, InvCurAddr + 9, InvCurAddr + 11];

        private static uint FirstInvAddr = 0x01CDD8BA;

        internal static void InitInventoryMgmt()
        {
            JsonSerializerOptions jOptions = new(JsonSerializerDefaults.Web)
            {
                AllowOutOfOrderMetadataProperties = true,
                IncludeFields = true
            };

            string filename = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Items", "Items.json");
            string json = File.ReadAllText(filename);
            ItemData = JsonSerializer.Deserialize<Dictionary<long, InvItem>>(json, jOptions);

            filename = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Items", "Attachments.json");
            json = File.ReadAllText(filename);
            AttachmentData = JsonSerializer.Deserialize<Dictionary<long, Attachment>>(json, jOptions);
        }

        /// <summary>
        /// Searches for an empty inventory slot and gives the player the item supplied.  Returns true if successful, false if inventory is full.
        /// </summary>
        /// <param name="itemId"></param>
        /// <returns></returns>
        internal static bool GiveItem(long itemId)
        {
            byte maxInv = Memory.ReadByte(InvMaxAddr);
            byte curInv = Memory.ReadByte(InvCurAddr);
            InvItem item = ItemData[itemId];

            if (curInv < maxInv)
            {
                // TODO not accouting for the items in the active bar?  Managed to set 51/50 items
                for (int i = 0; i < maxInv; i++)
                {
                    uint addr = (uint)(FirstInvAddr + sizeof(short) * i);
                    short itemValue = Memory.ReadShort(addr);

                    if (itemValue == -1)
                    {
                        Memory.Write(addr, item.ItemID);
                        uint durationAddr = FirstItemDurationAddr + (uint)(sizeof(short) * i);
                        if (item.ValueMax > 0)
                        {
                            // Items with usage values like amulets/dran's feather need this value set or they break immediately.
                            // Other items like bread & water have a value but it doesn't seem to matter; set them just in case.
                            if (item.ValueMax == item.ValueMin)
                                Memory.Write(durationAddr, (short)item.ValueMin);
                            else
                                Memory.Write(durationAddr, (short)random.Next(item.ValueMin, item.ValueMax + 1));
                        }
                        else
                        {
                            Memory.Write(durationAddr, -1);
                        }

                        ItemQueue.AddMsg("Received " + item.Name + ".");
                        return true;
                    }
                }
            }

            return false;
        }

        internal static bool GiveAttachment(long itemId)
        {
            Attachment item = AttachmentData[itemId];

            for (int i = 0; i < MaxAttachCount; i++)
            {
                uint addr = (uint)(FirstAttchAddr + 0x20 * i);
                short itemValue = Memory.ReadShort(addr);

                if (itemValue == -1)
                {
                    Memory.Write(addr, item.ItemID);

                    // Some of these fiels are actually shorts but we shouldn't be setting large enough values to matter.
                    for (int val = 0; val < item.ValueOffsets.Length; val++)
                    {
                        Memory.WriteByte((ulong)(addr + item.ValueOffsets[val]), (byte)item.Values[val]);
                    }

                    ItemQueue.AddMsg("Received " + item.Name + ".");
                    return true;
                }
            }
            return false;
        }

        internal static void GiveFreeFeather()
        {
            // TODO magic number for Dran's Feather. Create a constant when doing the miracle chests update.
            GiveItem(971111235);
        }

        internal static bool RemoveInvItem(short itemId)
        {
            bool success = false;
            byte maxInv = Memory.ReadByte(InvMaxAddr);
            uint addr = FirstInvAddr;

            for (int i = 0; i < maxInv; i++)
            {
                short id = Memory.ReadShort(addr);
                if (id == itemId)
                {
                    Memory.Write(addr, (short)-1);
                    success = true;
                    break;
                }
                addr += sizeof(short);
            }

            return success;
        }

        internal static bool RemoveAttchItem(short itemId)
        {
            // TODO
            return true;
        }
    }
}
