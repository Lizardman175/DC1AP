using Archipelago.Core.Util;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DC1AP.Mem
{
    /// <summary>
    /// Empty(?) block of memory on the mem card to use as we see fit.
    /// </summary>
    internal class OpenMem
    {
        private static readonly uint StartMem = 0x01CD4330;
        //private static readonly uint EndMem = 0x01CD4780;  Just here for reference; don't go past this byte!

        private static readonly uint SlotNameAddr = StartMem;
        private static readonly uint SlotNameLen = 32;  // Len from AP max slot name size

        // Byte
        internal static readonly uint GoalAddr = SlotNameAddr + SlotNameLen;

        // Add other bytes to be used before this one! RoomSeedAddr will be initialized after the itemCountAddrs block and has dynamic size.
        private static readonly uint CountBytesStart = GoalAddr + 2;

        // Map of item IDs to addresses
        private static readonly Dictionary<long, uint> ItemCountAddrs = [];

        // 64-bit
        private static uint RoomSeedAddr = 0x0;

        /// <summary>
        /// Returns the stored slot name, or empty string if unset.
        /// </summary>
        /// <returns></returns>
        internal static string GetSlotName()
        {
            Encoding? encoding = Encoding.Unicode;

            byte[] bytes = Memory.ReadByteArray(SlotNameAddr, (int)SlotNameLen, Enums.Endianness.Little);

            string s = encoding.GetString(bytes);
            string s2 = "";

            // Ignore nulls in the string
            foreach (var item in s)
            {
                if (item == 0) break;
                s2 += item;
            }

            return s2;
        }

        internal static bool TestRoomSeed()
        {
            string seed = App.Client.CurrentSession.RoomState.Seed;
            string memSeed = Memory.ReadString(RoomSeedAddr, seed.Length);
            bool result = seed == memSeed;
            if (!result)
            {
                Log.Logger.Error("Room seed mismatch. Expected " + seed + ", found " + memSeed + ".");
            }

            return result;
        }

        /// <summary>
        /// Write the given slot name and multiworld seed to the memory card.  Must be <= 16 chars.
        /// </summary>
        /// <param name="slotName"></param>
        internal static void SetSlotData(string slotName)
        {
            if (slotName.Length > SlotNameLen)
            {
                // Should be unreachable, server should verify before this point.
                throw new ArgumentException("Slot name must be less than " + (1 + SlotNameLen) + " chars.");
            }
            else if (GetSlotName().Equals(""))
            {
                Memory.WriteString(SlotNameAddr, slotName, Enums.Endianness.Little, Encoding.Unicode);
                Memory.WriteString(RoomSeedAddr, App.Client.CurrentSession.RoomState.Seed);
            }
        }

        internal static void InitItemCountAddrs(long[] itemKeys, long[] attachKeys)
        {
            uint addr = CountBytesStart;
            foreach (var key in itemKeys.Order())
            {
                ItemCountAddrs[key] = addr;
                addr++;
            }
            foreach (var attachKey in attachKeys.Order())
            {
                ItemCountAddrs[attachKey] = addr;
                addr++;
            }
            RoomSeedAddr = addr;
        }

        internal static byte ReadItemCountValue(long itemId)
        {
            return Memory.ReadByte(ItemCountAddrs[itemId]);
        }

        internal static void IncItemCountValue(long itemId)
        {
            byte value = (byte)(Memory.ReadByte(ItemCountAddrs[itemId]) + 1);
            Memory.WriteByte(ItemCountAddrs[itemId], value);
        }
    }
}
