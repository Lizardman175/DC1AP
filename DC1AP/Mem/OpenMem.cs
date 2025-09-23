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
        internal static readonly uint IndexAddr = GoalAddr + 1;

        // Short
        internal static readonly uint CollectedCountAddr = (uint)(IndexAddr + 2);

        /*
         * Norune: 17 buildins
         * Matataki: 20
         * Queens: 14
         * Muska: 14
         * factory: 14
         * Memories: 12
         * 91 total 0x58
         */

        // Prep for future bytes. Going to need double the above bytes for MCs, and then some
        //private static readonly uint NextByte = CollectedCountAddr + 1;

        /// <summary>
        /// Returns the stored slot name, or empty string if unset.
        /// </summary>
        /// <returns></returns>
        public static string GetSlotName()
        {
            // TODO make this a generic func for other strings.
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
        public static void SetSlotName(String s)
        {
            if (s.Length > SlotNameLen)
            {
                // Should be unreachable, server should verify before this point.
                throw new ArgumentException("Slot name must be less than " + (1 + SlotNameLen) + " chars.");
            }
            else if (GetSlotName().Equals(""))
                Memory.WriteString(SlotNameAddr, s);
        }

        public static short GetIndex()
        {
            return Memory.ReadShort(IndexAddr);
        }

        public static void SetIndex(short value)
        {
            Memory.Write(IndexAddr, value);
        }

        public static void IncIndex()
        {
            Memory.Write(IndexAddr, (short)(GetIndex() + 1));
        }
    }
}
