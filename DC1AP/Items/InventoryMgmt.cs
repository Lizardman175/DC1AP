using Archipelago.Core.Util;
using DC1AP.Constants;
using Serilog;

namespace DC1AP.Items
{
    internal class InventoryMgmt
    {
        //private const byte MaxInv = 100;
        private const byte StartingMaxInv = 50;
        private const byte PocketSize = 10;
        private const uint InvMaxAddr = 0x01CDD8AC;  // Byte.  Can't exceed 100 or we run past the buffer.
        private const uint InvCurAddr = 0x01CDD8AD;  // Byte.  Next byte starts the active item shorts, followed by 3 shorts giving count of the active items per slot, then shorts for the other items.

        // Add 1 byte for the CurAddr field, then 2 for each short past the first addr
        //private static uint[] ActiveItemAddrs = [InvCurAddr + 1, InvCurAddr + 3, InvCurAddr + 5];
        private static uint[] ActiveItemCountAddrs = [InvCurAddr + 7, InvCurAddr + 9, InvCurAddr + 11];

        private static uint FirstInvAddr = ActiveItemCountAddrs[2] + 2;

        internal static void GivePockets()
        {
            if (Options.GivePockets)
            {
                byte maxInvSize = Memory.ReadByte(InvMaxAddr);

                if (maxInvSize > StartingMaxInv)
                {
                    Log.Error("Player already has more than 50 max inventory, not adding pockets.");
                }

                byte temp;

                // 3 pockets are available regardless of Options. 2 in Norune, 1 in Matataki
                maxInvSize += PocketSize * 3;

                // TODO magic numbers as we implement miracle chests. Also, don't clear flags if miracle shuffle
                if (!Options.MiracleSanity)
                {
                    // Pocket MC in Mayor's House
                    temp = Memory.ReadByte(0x01CE484A);
                    temp |= 1;
                    Memory.WriteByte(0x01CE484A, temp);

                    // Pocket MC in Goro's House
                    temp = Memory.ReadByte(0x01CE4B13);
                    temp |= 0x40;
                    Memory.WriteByte(0x01CE4B13, temp);

                    // Paige's House reward Pocket
                    Memory.Write(30230636, (short)1);
                }

                if (Options.Goal > 2)
                {
                    maxInvSize += PocketSize;

                    if (!Options.MiracleSanity)
                    {
                        // Pocket MC in Cathedral
                        temp = Memory.ReadByte(0x01CE48BF);
                        temp |= 0x40;
                        Memory.WriteByte(0x01CE48BF, temp);
                    }
                }

                if (Options.Goal > 3)
                {
                    //curInvSize += PocketSize;
                    // TODO with Muska Lacka update
                }

                Memory.Write(InvMaxAddr, maxInvSize);
            }
        }

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
            Memory.Write(MiscAddrs.FirstItemDurationAddr, (short)0x42cc);
        }
    }
}
