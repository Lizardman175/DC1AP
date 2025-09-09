using Archipelago.Core.Util;
using DC1AP.Constants;
using DC1AP.IDs;
using DC1AP.Mem;
using DC1AP.Threads;

namespace DC1AP.Georama
{
    internal class GeoBuilding
    {
        private const int ItemShortOffset = 3;
        private const int MaxItemCount = 6;

        public required string Name;
        public long[] ApIds;
        public uint BaseAddr;
        public GeoItem[] Items;
        public int BuildingId;
        public bool Multi = false;

        private short buildingCount;
        private bool collected = false;
        private uint placedCountAddr;
        private uint BuildingCountAddr;
        private Towns town;

        /// <summary>
        /// Pull values from the addresses above and set them locally to compare when checking against the server's values.
        /// </summary>
        internal void ReadValues()
        {
            placedCountAddr = BaseAddr + sizeof(short);
            BuildingCountAddr = BaseAddr + sizeof(int);
            buildingCount = Memory.ReadShort(BuildingCountAddr);

            if (Multi && !collected) buildingCount = 0;

            for (int i = 0; i < Items.Length; i++)
            {
                uint itemAddr = BaseAddr + GeoAddrs.HouseInvOffset + (uint)(GeoAddrs.BldFieldDelta * i);
                Items[i].Init(itemAddr, BuildingId, i+1);
            }
        }

        internal bool IsMemInit()
        {
            // TODO junk data might make this not evaluate to true?
            if (Memory.ReadShort(BuildingCountAddr) == 0)
            {
                return false;
            }

            return true;
        }

        internal void Init(int t, bool firstInit)
        {
            town = (Towns)t;

            if (firstInit)
            {
                // Some entries have junk data by default, we want to clear that out
                Memory.Write(placedCountAddr, (short)0);
                Memory.Write(BuildingCountAddr, (short)1);

                // We want to set all 6 item slots to zero to clear any junk values, rather than just the count of Items
                for (int i = ItemShortOffset; i < ItemShortOffset + MaxItemCount; i++)
                {
                    Memory.Write((uint)BaseAddr + (uint)(i * sizeof(short)), (short)0);
                }
            }
        }

        /// <summary>
        /// </summary>
        internal void GiveBuilding(long id)
        {
            int bit = 0;
            if (Multi) bit = ApIds.ToList().IndexOf(id);
            
            if (!OpenMem.TestGeoMaskBit(town, BuildingId, bit))
            {
                Memory.Write(BaseAddr, (short)1);
                if (Multi)
                {
                    byte mask = OpenMem.GetGeoMask(town, BuildingId);

                    buildingCount = 0;
                    for (int i = 0; i < 8; i++)
                    {
                        byte test = (byte)(mask & (1 << i));
                        if (test > 0) buildingCount += 5;
                    }
                    // TODO can move the bit set above this if and this should be removable.
                    buildingCount += 5;
                    Memory.Write(BuildingCountAddr, buildingCount);
                }

                OpenMem.SetGeoMaskBit(town, BuildingId, bit);
                ItemQueue.AddMsg("Received " + Name + ".");
            }
        }

        internal GeoItem[] GetItems() => Items;
    }
}
