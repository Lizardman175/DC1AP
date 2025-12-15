using Archipelago.Core.Util;
using DC1AP.Constants;
using System.Collections.Generic;

namespace DC1AP.Mem
{
    internal static class MemFuncs
    {
        private const int GarbageInt = 0x22222222;

        internal static Dictionary<int, Atla> AtlaById = [];

        internal static void ClearAtlaTable(int town)
        {
            (uint, int) atlaTable = GeoAddrs.AtlaTables[town];
            uint addr = atlaTable.Item1;

            // Skip if we already garbaged this table.
            if (Memory.ReadInt(addr) != GarbageInt)
            {
                for (int i = 0; i < atlaTable.Item2; i++)
                {
                    Memory.Write(addr, GarbageInt);

                    // Make floor-specific atla available on any floor otherwise the game freezes!
                    // Note: -1 here makes the atla appear in the front half of the dungeon.
                    // TODO D6: special handling for final dungeon?
                    if (Memory.ReadInt(addr + GeoAddrs.GeoFloorOffset) > 0)
                        Memory.Write(addr + GeoAddrs.GeoFloorOffset, -1);

                    addr += GeoAddrs.GeoItemOffset;
                }
            }
        }

        /// <summary>
        /// Find an empty space in geo inventory to add the item (should always be enough space as no town have > 120 atla pieces)
        /// </summary>
        /// <param name="town">Town id</param>
        /// <param name="id">Georama item ID</param>
        internal static void GiveGeoItem(Towns town, short id)
        {
            uint addr = GeoAddrs.TownGeoInv[(int)town];
            int count = 0;

            while (count < MiscConstants.GeoMaxItemCount)
            {
                short curValue = Memory.ReadShort(addr);

                if (curValue == -1)
                {
                    Memory.Write(addr, id);
                    break;
                }

                count++;
                addr += sizeof(short);
            }
        }
    }
}
