using Archipelago.Core.Util;

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
        private static readonly int SlotNameLen = 16;  // Len from AP max slot name size

        // Byte
        internal static readonly uint GoalAddr = (uint)(SlotNameAddr + SlotNameLen);

        // Short
        private static readonly uint IndexAddr = GoalAddr + 1;

        // Short
        private static readonly uint CollectedCountAddr = (uint)(IndexAddr + 2);

        // Add other bytes to be used before this one!
        private static readonly uint CountBytesStart = CollectedCountAddr + 1;

        // Map of item IDs to addresses
        private static readonly Dictionary<long, uint> itemCountAddrs = [];

        /// <summary>
        /// Returns the stored slot name, or empty string if unset.
        /// </summary>
        /// <returns></returns>
        internal static string GetSlotName()
        {
            System.Text.Encoding? encoding = System.Text.Encoding.UTF8;

            byte[] bytes = Memory.ReadByteArray(SlotNameAddr, SlotNameLen, Enums.Endianness.Little);

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

        /// <summary>
        /// Write the given slot name to memory.  Must be <= 16 chars.
        /// </summary>
        /// <param name="s"></param>
        internal static void SetSlotName(String s)
        {
            if (s.Length > SlotNameLen)
            {
                // Should be unreachable, server should verify before this point.
                throw new ArgumentException("Slot name must be less than " + (1 + SlotNameLen) + " chars.");
            }
            else if (GetSlotName().Equals(""))
                Memory.WriteString(SlotNameAddr, s);
        }

        internal static short GetIndex()
        {
            return Memory.ReadShort(IndexAddr);
        }

        internal static void SetIndex(short value)
        {
            Memory.Write(IndexAddr, value);
        }

        internal static void IncIndex()
        {
            Memory.Write(IndexAddr, (short)(GetIndex() + 1));
        }

        internal static void InitItemCountAddrs(long[] itemKeys, long[] attachKeys)
        {
            uint addr = CountBytesStart;
            foreach (var key in itemKeys.Order())
            {
                itemCountAddrs[key] = addr;
                addr++;
            }
            foreach (var attachKey in attachKeys.Order())
            {
                itemCountAddrs[attachKey] = addr;
                addr++;
            }
        }

        internal static byte ReadItemCountValue(long itemId)
        {
            return Memory.ReadByte(itemCountAddrs[itemId]);
        }

        internal static void IncItemCountValue(long itemId)
        {
            byte value = (byte)(Memory.ReadByte(itemCountAddrs[itemId]) + 1);
            Memory.WriteByte(itemCountAddrs[itemId], value);
        }
    }
}
