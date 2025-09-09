using Archipelago.Core.Util;
using DC1AP.Mem;
using DC1AP.Threads;
using DC1AP.Constants;

namespace DC1AP.Georama
{
    // TODO removal or major rework to be done with progressive georama update, don't worry much about this file for now.
    internal class GeoItem
    {
        public required String Name;
        public long ApId;
        public int ItemId;
        private int bit;
        private uint addr;
        private int bldId;

        public void Init(uint addr, int bldId, int bit)
        {
            this.addr = addr;
            this.bit = bit;
            this.bldId = bldId;
        }

        public void GiveItem(Towns townId)
        {
            if (!OpenMem.TestGeoMaskBit(townId, bldId, bit))
            {
                if (Memory.ReadShort(addr) == 1)
                {
                    MemFuncs.GiveGeoItem(townId, (short)ItemId);
                }
                else
                {
                    Memory.Write(addr, (short)1);
                }

                OpenMem.SetGeoMaskBit(townId, bldId, bit);
                ItemQueue.AddMsg("Received " + Name + " for " + townId.ToString());
            }
        }
    }
}
