using Archipelago.Core.Util;
using DC1AP.Constants;

namespace DC1AP.Items
{
    internal class InventoryMgmt
    {
        private const uint InvMaxAddr = 0x01CDD8AC;  // Byte.  Can't exceed 100 or we run past the buffer.
        private const uint InvCurAddr = 0x01CDD8AD;  // Byte.  Next byte starts the active item shorts, followed by 3 shorts giving count of the active items per slot, then shorts for the other items.

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

        /// <summary>
        /// Searches for an empty inventory slot and gives the player the item supplied.  Returns true if successful, false if inventory is full.
        /// </summary>
        /// <param name="itemId"></param>
        /// <returns></returns>
        internal static bool GiveItem(short itemId)
        {
            bool success = false;

            byte maxInv = Memory.ReadByte(InvMaxAddr);
            byte curInv = Memory.ReadByte(InvCurAddr);

            if (curInv < maxInv)
            {
                for (int i = 0; i < maxInv; i++)
                {
                    uint addr = (uint)(FirstInvAddr + sizeof(short) * i);
                    short item = Memory.ReadShort(addr);

                    if (item == -1)
                    {
                        Memory.Write(addr, itemId);
                        success = true;
                        break;
                    }
                }
            }

            return success;
        }

        internal static void GiveFreeFeather()
        {
            // TODO magic number for Dran's Feather. Create a constant when doing the miracle chests update.
            GiveItem(235);
            Memory.Write(FirstItemDurationAddr, (short)0x42cc);
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
            return true;
        }

        internal static bool RemoveGeoItem(short itemId, int dungeon)
        {
            bool success = false;
            uint addr = GeoAddrs.TownGeoInv[dungeon];

            for (int i = 0; i < MiscConstants.GeoMaxItemCount; i++)
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
    }
}
