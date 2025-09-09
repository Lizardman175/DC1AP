using Archipelago.Core.Util;
using DC1AP.Constants;

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

        /*
         * Norune: 17 buildins
         * Matataki: 20
         * Queens: 14
         * Muska: 14
         * factory: 14
         * Memories: 12
         * 91 total 0x58
         */
        // TODO these aren't used except for their sizes...could just make constants for them.
        // Ignoring magic nums until progressive atla update since these will go away then
        //private static readonly byte[] NoruneGeoMasks = new byte[17];
        //private static readonly byte[] MatatakiGeoMasks = new byte[20];
        //private static readonly byte[] QueensGeoMasks = new byte[14];
        //private static readonly byte[] MuskaGeoMasks = new byte[14];
        //private static readonly byte[] FactoryGeoMasks = new byte[14];
        //private static readonly byte[] CastleGeoMasks = new byte[12];

        // TODO progressive atla: use the base attr for the buildings to handle masking the items rather bytes here.  Save these to reuse for MCs
        private static readonly uint NoruneFirstMask = IndexAddr + 2;
        private static readonly uint MatatakiFirstMask = (uint)(NoruneFirstMask + 17);
        private static readonly uint QueensFirstMask = (uint)(MatatakiFirstMask + 20);
        private static readonly uint MuskaFirstMask = (uint)(QueensFirstMask + 14);
        private static readonly uint FactoryFirstMask = (uint)(MuskaFirstMask + 14);
        private static readonly uint CastleFirstMask = (uint)(FactoryFirstMask + 14);

        //private static readonly List<byte[]> maskLists = [NoruneGeoMasks, MatatakiGeoMasks, QueensGeoMasks, MuskaGeoMasks, FactoryGeoMasks, CastleGeoMasks];
        private static readonly List<uint> maskAddrs = [NoruneFirstMask, MatatakiFirstMask, QueensFirstMask, MuskaFirstMask, FactoryFirstMask, CastleFirstMask];

        // TODO 0.2 move this near IndexAddr, rename IndexAddr (do with progressive item update)
        private static readonly uint CollectedCountAddr = (uint)(CastleFirstMask + 12);

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

        public static void SetGeoMaskBit(Towns town, int bldIndex, int bit)
        {
            Memory.WriteBit((uint)(maskAddrs[(int)town] + bldIndex), bit, true);
        }

        public static Boolean TestGeoMaskBit(Towns town, int bldIndex, int bit)
        {
            return Memory.ReadBit((uint)(maskAddrs[((int)town)] + bldIndex), bit);
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

        internal static byte GetGeoMask(Towns town, int buildingId)
        {
            return Memory.ReadByte((ulong)(maskAddrs[(int)town] + buildingId));
        }
    }
}
