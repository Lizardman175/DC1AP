using Archipelago.Core.Util;
using DC1AP.Constants;
using DC1AP.Mem;
using DC1AP.Threads;

namespace DC1AP.Georama
{
    internal class GeoBuilding
    {
        internal static List<GeoBuilding[]?> buildings = [null, null, null, null, null, null];
        private static short Multiplier = 5;

        private const uint EventFlagOffset = 4;
        private const uint ItemShortOffset = 3;
        private const int MaxItemCount = 6;

        public required string Name;
        public long ApId;
        public uint BaseAddr;
        public GeoItem[] Items = [];
        public short BuildingId;
        public int Multi = 0;
        public BuildingCoords? AnyCoords;
        // Only D6 should have nothing for HundoCoords so this should be safe to have empty.
        public BuildingCoords HundoCoords = new();
        public BuildingCoords[]? MultiCoords;

        private short buildingValue;
        //private short buildingCount;
        private uint placedCountAddr;
        private uint BuildingCountAddr;
        private Towns town;

        /// <summary>
        /// Pull values from the addresses above and set them locally to compare when checking against the game's values.
        /// </summary>
        internal void ReadValues()
        {
            buildingValue = Memory.ReadShort(BaseAddr);
            placedCountAddr = BaseAddr + sizeof(short);
            BuildingCountAddr = BaseAddr + sizeof(int);
            //buildingCount = Memory.ReadShort(BuildingCountAddr);
        }

        private bool HasBuilding()
        {
            return Memory.ReadShort(BuildingCountAddr) != 0;
        }

        internal void Init(Towns townId)
        {
            town = townId;

            if (!HasBuilding())
            {
                // Some entries have junk data by default, we want to clear that out
                Memory.Write(placedCountAddr, (short)0);
                Memory.Write(BuildingCountAddr, (short)1);

                // We want to set all 6 item slots to zero to clear any junk values, rather than just the count of Items
                for (uint i = ItemShortOffset; i < ItemShortOffset + MaxItemCount; i++)
                {
                    Memory.Write(BaseAddr + (i * sizeof(short)), (short)0);
                }
            }
        }

        /// <summary>
        /// </summary>
        internal void GiveBuilding()
        {
            // Small chance of race condition giving us 2 items at the same time.  Double check the count before adding.
            // Also prevents trying to add too many of an item if /send was used on the server.
            if (buildingValue == CountThisBuilding()) return;

            string? msg = null;

            // Buildings with multiple copies, like river
            if (Multi != 0 && buildingValue < Multi)
            {
                buildingValue++;
                short buildingCount = (short)(Multiplier * buildingValue);
                Memory.Write(BuildingCountAddr, buildingCount);

                // Auto building of towns. Castle has no town to build, factory has no multi buildings
                if (town < Towns.Factory)
                {
                    BuildMultiBuilding();
                }

                msg = "Received " + Name + ".";
            }
            // First piece, just enable the building
            else if (buildingValue == 0)
            {
                buildingValue = 1;

                // Auto building of towns. Castle has no town to build.
                if (town < Towns.Castle)
                {
                    BuildBuilding();
                }

                // Skip the dialog only events for the 4 pilots
                if (town == Towns.Factory && MiscConstants.FactoryEventSkips.Contains(BuildingId))
                {
                    Memory.Write(BaseAddr - EventFlagOffset, (short)1);
                }

                msg = "Received " + Name + ".";
            }
            // Not first piece, add next item
            else if (buildingValue <= Items.Length)
            {
                GeoItem item = Items[buildingValue - 1];
                uint itemAddr = (uint)(BaseAddr + GeoAddrs.HouseInvOffset + (sizeof(short) * item.SlotId));

                // If there isn't an item set in the item's slot, put it there.  Otherwise, add it to the player's inventory.
                if (Memory.ReadShort(itemAddr) == 0)
                    Memory.Write(itemAddr, (short)1);
                else
                    MemFuncs.GiveGeoItem(town, (short)item.ItemId);

                // Skip having to view the d6 events after getting the last item
                if (town == Towns.Castle && buildingValue == Items.Length)
                    EventMasks.SetD6Flag(BuildingId);

                buildingValue++;

                msg = "Received " + item.Name + " for " + town.ToString() + ".";
            }

            if (msg != null)
            {
                Memory.Write(BaseAddr, buildingValue);
                ItemQueue.AddMsg(msg);
                OpenMem.IncIndex();
            }
        }

        /// <summary>
        /// Checks how many of this item the player is supposed to have versus how many they actually have and adds items to the queue if missing.
        /// </summary>
        internal void CheckItems()
        {
            int count = CountThisBuilding();

            if (count > buildingValue)
            {
                for (int i = buildingValue; i < count; i++)
                {
                    ItemQueue.AddGeorama(this);
                }
            }
        }

        /// <summary>
        /// Counts how many of this building the player is supposed to have per the GameState.
        /// </summary>
        /// <returns></returns>
        private int CountThisBuilding()
        {
            return GeoInvMgmt.buildingCounts.ContainsKey(ApId) ? GeoInvMgmt.buildingCounts[ApId] : 0;
        }

        #region TownBuilding
        private void BuildBuilding()
        {
            GeoBuilding[]? townBuildings = buildings[(int)town];
            if (townBuildings == null) return;

            // Eww...
            if (HundoCoords != null &&
                 (Options.Autobuild == AutobuildFlags.Hundo ||
                 (town == Towns.Muska && (Options.Autobuild == AutobuildFlags.Muska || Options.Autobuild == AutobuildFlags.MuskaRobot)) ||
                 (town == Towns.Factory && (Options.Autobuild == AutobuildFlags.Robot || Options.Autobuild == AutobuildFlags.MuskaRobot)))
               )
            {
                if (town == Towns.Matataki)
                {
                    // For Matataki, need to set the height of Couscous/Mushroom house based on the presence of Earth B/A respectively
                    if (BuildingId == MiscConstants.CouscousId)
                    {
                        uint earthBAddr = FindBuildingById(town, MiscConstants.EarthBId);
                        WriteBuildingMap(HundoCoords, earthBAddr != 0);
                    }
                    else if (BuildingId == MiscConstants.MushroomId)
                    {
                        uint earthAAddr = FindBuildingById(town, MiscConstants.EarthAId);
                        WriteBuildingMap(HundoCoords, earthAAddr != 0);
                    }
                    else if (BuildingId == MiscConstants.EarthBId)
                    {
                        uint couscousAddr = FindBuildingById(town, MiscConstants.CouscousId);
                        if (couscousAddr != 0)
                        {
                            couscousAddr += sizeof(long);
                            Memory.Write(couscousAddr, townBuildings[MiscConstants.CouscousId].HundoCoords.Y);
                        }
                        WriteBuildingMap(HundoCoords);
                    }
                    else if (BuildingId == MiscConstants.EarthAId)
                    {
                        uint mushAddr = FindBuildingById(town, MiscConstants.MushroomId);
                        if (mushAddr != 0)
                        {
                            mushAddr += sizeof(long);
                            Memory.Write(mushAddr, townBuildings[MiscConstants.MushroomId].HundoCoords.Y);
                        }
                        WriteBuildingMap(HundoCoords);
                    }
                    else if (BuildingId >= MiscConstants.Watermill1Id && BuildingId <= MiscConstants.Watermill3Id)
                    {
                        if (BuildingId == MiscConstants.Watermill1Id)
                        {
                            if (townBuildings[MiscConstants.MatatakiRiverId].buildingValue > 0)
                                WriteBuildingMap(HundoCoords);
                        }
                        else
                        {
                            // Need at least 2 sets of river pieces for the second and third watermill
                            if (townBuildings[MiscConstants.MatatakiRiverId].buildingValue > 1)
                                WriteBuildingMap(HundoCoords);
                        }
                    }
                    else
                        WriteBuildingMap(HundoCoords);
                }
                else
                    WriteBuildingMap(HundoCoords);
            }
            else if (AnyCoords != null && Options.Autobuild == AutobuildFlags.Any)
            {
                if (town == Towns.Matataki && BuildingId == MiscConstants.Watermill1Id)
                {
                    if (townBuildings[MiscConstants.MatatakiRiverId].buildingValue > 0)
                        WriteBuildingMap(AnyCoords);
                }
                else if (town == Towns.Matataki && (BuildingId == MiscConstants.Watermill2Id || BuildingId == MiscConstants.Watermill3Id))
                {
                    if (townBuildings[MiscConstants.MatatakiRiverId].buildingValue > 1)
                        WriteBuildingMap(AnyCoords);
                }
                else
                    WriteBuildingMap(AnyCoords);
            }
        }

        private void BuildMultiBuilding()
        {
            // Ewwww...
            if (MultiCoords != null &&
                 (Options.Autobuild == AutobuildFlags.Hundo ||
                 (Options.Autobuild == AutobuildFlags.Any && town == Towns.Matataki && BuildingId == MiscConstants.MatatakiRiverId) ||  // Need rivers for any%
                 (town == Towns.Muska && (Options.Autobuild == AutobuildFlags.Muska || Options.Autobuild == AutobuildFlags.MuskaRobot)) ||
                 (town == Towns.Factory && (Options.Autobuild == AutobuildFlags.Robot || Options.Autobuild == AutobuildFlags.MuskaRobot)))
               )
            {
                GeoBuilding[]? townBuildings = buildings[(int)town];
                if (townBuildings == null)
                {
                    return;
                }

                if (!(town == Towns.Norune && BuildingId == MiscConstants.NoruneBridgeId) &&
                    !(town == Towns.Matataki && BuildingId == MiscConstants.MatatakiBridgeId))
                {
                    int max = Multiplier * buildingValue;
                    int i = Multiplier * (buildingValue - 1);
                    // Leave off the 25th river piece so the player can access the "bugged" MCs before finishing the river
                    if (town == Towns.Matataki && BuildingId == MiscConstants.MatatakiRiverId && max >= Multiplier * MiscConstants.MatatakiReqRiverCount)
                    {
                        max--;
                        i--;
                    }

                    for (; i < max; i++)
                    {
                        WriteBuildingMap(MultiCoords[i]);
                    }

                    if (town == Towns.Norune && BuildingId == MiscConstants.NoruneRiverId &&
                        buildingValue == MiscConstants.NoruneBridgeRiverCount &&
                        townBuildings[MiscConstants.NoruneBridgeId].buildingValue > 0)
                    {
                        townBuildings[MiscConstants.NoruneBridgeId].BuildMultiBuilding();
                    }

                    if (town == Towns.Matataki && BuildingId == MiscConstants.MatatakiRiverId)
                    {
                        if (buildingValue == 1 && townBuildings[MiscConstants.Watermill1Id].buildingValue > 0)
                        {
                            townBuildings[MiscConstants.Watermill1Id].BuildBuilding();
                        }
                        else if (buildingValue == 2)
                        {
                            if (townBuildings[MiscConstants.Watermill2Id].buildingValue > 0)
                                townBuildings[MiscConstants.Watermill2Id].BuildBuilding();
                            if (townBuildings[MiscConstants.Watermill3Id].buildingValue > 0)
                                townBuildings[MiscConstants.Watermill3Id].BuildBuilding();
                        }
                        
                        if (buildingValue == MiscConstants.MatatakiBridgeRiverCount &&
                            townBuildings[MiscConstants.MatatakiBridgeId].buildingValue > 0)
                        {
                            townBuildings[MiscConstants.MatatakiBridgeId].BuildMultiBuilding();
                        }
                    }
                }
                // Norune Bridge after River
                else if (town == Towns.Norune && BuildingId == MiscConstants.NoruneBridgeId &&
                    townBuildings[MiscConstants.NoruneRiverId].buildingValue >= MiscConstants.NoruneBridgeRiverCount)
                {
                    for (int i = Multiplier * (buildingValue - 1); i < Multiplier * buildingValue; i++)
                    {
                        WriteBuildingMap(MultiCoords[i]);
                    }
                }
                // Matataki Bridge after River
                else if (town == Towns.Matataki && BuildingId == MiscConstants.MatatakiBridgeId &&
                    townBuildings[MiscConstants.MatatakiRiverId].buildingValue >= MiscConstants.MatatakiBridgeRiverCount)
                {
                    for (int i = Multiplier * (buildingValue - 1); i < Multiplier * buildingValue; i++)
                    {
                        WriteBuildingMap(MultiCoords[i]);
                    }
                }
            }
        }

        private void WriteBuildingMap(BuildingCoords map, bool useY = true)
        {
            uint addr = GeoAddrs.TownMapAddrs[((int)town)];
            short buildingIdMem = Memory.ReadShort(addr);

            // Find the first empty building slot
            while (buildingIdMem != -1)
            {
                addr += 0x10; // 4 ints
                buildingIdMem = Memory.ReadShort(addr);
            }

            Memory.Write(addr, BuildingId);

            addr += sizeof(short);
            Memory.Write(addr, map.Orientation);

            addr += sizeof(short);
            Memory.Write(addr, map.X);

            addr += sizeof(float);
            if (useY) Memory.Write(addr, map.Y);
            else Memory.Write(addr, 0.0f);

            addr += sizeof(float);
            Memory.Write(addr, map.Z);
        }

        /// <summary>
        /// Finds the given building ID for the given town and returns the addr for it.  Returns 0 if not found.
        /// </summary>
        /// <param name="town">Town to search through.</param>
        /// <param name="id">Building ID to search for.</param>
        /// <returns>The addr for the building or 0 if not found.</returns>
        private static uint FindBuildingById(Towns town, short id)
        {
            uint addr = GeoAddrs.TownMapAddrs[((int)town)];
            int temp = Memory.ReadInt(addr);

            while (temp != -1)
            {
                short buildingIdMem = Memory.ReadShort(addr);
                if (buildingIdMem == id)
                {
                    return addr;
                }
                addr += 0x10;
            }

            return 0;
        }
        #endregion
    }
}
