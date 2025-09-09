using Archipelago.Core.Util;
using DC1AP.Constants;

namespace DC1AP.Mem
{
    internal static class MemFuncs
    {
        private const int GarbageInt = 0x22222222;

        internal static Dictionary<int, Atla> AtlaById = [];

        private static readonly List<Atla> collectedAtla = [];

        // TODO This may come back around.  I believe the issues I had with manually setting atla per dungeon floor was something else.
        // If we do come back to it, want to randomize the floors on slot name + seed so floors can be normalized for race purposes but still random in a multi-DC multiworld
        //internal static void InitDungeons()
        //{
            // Hard-coded atla per floor counts
            //int[][] atlaPerFloor = [[8, 7, 6, 0, 6, 8, 8, 0, 8, 7, 0, 7, 8, 8],  // First line unused for now.
            //                        [8, 8, 8, 0, 6, 8, 8, 8, 0, 7, 6, 0, 6, 8, 8, 5],
            //                        [8, 8, 7, 5, 0, 2, 8, 4, 0, 8, 5, 0, 6, 8, 1, 2, 2],
            //                        [5, 2, 5, 6, 0, 8, 4, 3, 0, 6, 5, 7, 0, 5, 4, 2, 5],
            //                        [4, 6, 7, 0, 2, 2, 8, 0, 7, 6, 0, 4, 6, 4],
            //                        [4, 3, 3, 3, 2, 2, 4, 4, 3, 3, 3, 3, 3, 3, 3, 4, 4, 0, 0, 0, 0, 0, 5]];

            // Bröther can I have some lööps
            //for (int i = 0; i < Options.Goal; i++)
            //{
                // Set floor count so the game doesn't initialize the atla per floor
                //if (Options.OpenDungeon)
                //    Memory.Write(MiscAddrs.FloorCountAddrs[i], MiscAddrs.FloorCountFront[i]);
                //else
                //    Memory.Write(MiscAddrs.FloorCountAddrs[i], (byte)0);

                //uint addr = GeoAddrs.AtlaFlagAddrs[i];

                //int[] atla = atlaPerFloor[i];
                //for (int j = 0; j < atla.Length; j++)
                //{
                //    int k = 0;
                //    for (k = 0; k < atla[j]; k++)
                //    {
                //        Memory.Write(addr, MiscConstants.AtlaAvailable);
                //        addr += sizeof(int);
                //    }
                //    for (; k < 8; k++)
                //    {
                //        Memory.Write(addr, MiscConstants.AtlaUnavailable);
                //        addr += sizeof(int);
                //    }

                //    addr += sizeof(int) * (uint)(MiscConstants.MaxAtlaPerFloor - k);  // Shift addr by any remaining values
                //}
            //}
        //}

        /// <summary>
        /// </summary>
        internal static void MapAtla()
        {
            // TODO this and MapAtlaDungeon OBE?
            for (int i = 0; i < GeoAddrs.AtlaTables.Count && i < Options.Goal; i++)
            {
                uint addr = GeoAddrs.AtlaTables[i].Item1;
                int value = Memory.ReadInt(addr);

                // First entry is empty, dungeon unitialized  TODO this should set up a watcher before the continue
                //if (value == AtlaNonexistent) { Watchers.WatchDungeonAtla(i); }
                //else MapAtlaDungeon(i);
            }
        }

        /// <summary>
        /// Map out the atla for the given dungeon ID.  Expects that the dungeon is initialized first!
        /// TODO not used
        /// </summary>
        /// <param name="dungeonId">0-indexed dungeon id.</param>
        //internal static void MapAtlaDungeon(int dungeonId)
        //{
        //    int halfwayCount = 0;
        //    if (dungeonId < GeoAddrs.AtlaHalfwayCounts.Count) { halfwayCount = GeoAddrs.AtlaHalfwayCounts[dungeonId]; }

        //    int atlaCount = 0;

        //    int locationId = 0;
        //    locationId = (dungeonId + 1) * 1000;

        //    (uint, int) atlaTable = GeoAddrs.AtlaTables[dungeonId];
        //    uint addr = atlaTable.Item1;

        //    while (atlaCount < atlaTable.Item2)
        //    {
        //        int value = Memory.ReadInt(addr);

        //        if (value != MiscConstants.AtlaUnavailable)
        //        {
        //            atlaCount++;

        //            locationId += atlaCount;

        //            if (halfwayCount > 0 && atlaCount > halfwayCount) { locationId += 200; }
        //            else { locationId += 100; }

        //            Atla atla = new(addr, dungeonId, locationId, value == MiscConstants.AtlaClaimed);
        //            AtlaById.Add(locationId, atla);  // TODO getting duplicate key exception if game was already initialized

        //            if (value == MiscConstants.AtlaClaimed)
        //            {
        //                collectedAtla.Add(atla);
        //            }
        //            else
        //            {
        //                //Watchers.WatchAtla(atla);
        //            }
        //        }

        //        addr += sizeof(int);
        //    }
        //}

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

        //internal static void ClearAtlaTables()
        //{
        //    for (int i = 0; i < Options.Goal; i++)
        //    {
        //        ClearAtlaTable(i);
        //    }
        //}

        /// <summary>
        /// Player opened atla, send data back to AP server.
        /// </summary>
        /// <param name="atla"></param>
        public static void ProcessAtla(Atla atla)
        {
            Archipelago.Core.Models.Location l = new();
            l.Id = atla.LocationId;
            App.Client.SendLocation(l);
        }

        /// <summary>
        /// Give the player a georama building piece.  
        /// ONLY RUN FROM A DUNGEON! Setting a building in town the game will reset it for some reason.
        /// TODO this region of memory will get overwritten in town, but there may be another region to manage it?  Must be so the player can make changes
        /// </summary>
        /// <param name="town">0 Indexed town id</param>
        /// <param name="id">ID of the item received</param>
        //internal static void GiveGeoBuilding(IDs.Towns town, short id)
        //{
        //    uint addr = GeoAddresses.TOWN_BLD_INV[(int)town];
        //    // TODO handle rivers etc.  Need to figure out how IDs will work for those.
        //    addr += (uint) (id * GeoAddresses.HouseOffset);
        //    Console.WriteLine(addr);

        //    Memory.Write(addr, (short) 1);  // Flag indicating building is received
        //    addr += sizeof(short);
        //    //Memory.Write(addr, (short) 0);  // TODO only do this on first init (probably init when the town does)
        //    addr += sizeof(short);
        //    Memory.Write(addr, (short) 1);  // Item owned count.  TODO part of river/etc handling.

        //    // Initialize item slot flags (again TODO as with first init above)
        //    //for (int i = 0; i < 6; i++)
        //    //{
        //    //    Memory.Write(addr, (short)0);
        //    //    addr += 2;
        //    //}
        //}

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
