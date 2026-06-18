using Archipelago.Core.Util;
using Archipelago.MultiClient.Net.Models;
using DC1AP.Constants;
using DC1AP.Mem;
using DC1AP.Threads;
using Serilog;
using System.Collections.Generic;
using System.Linq;
using static DC1AP.Georama.BuildingCoords;

namespace DC1AP.Georama
{
    internal class GeoBuilding
    {
        internal static Dictionary<long, GeoBuilding> buildings = [];
        private static short Multiplier = 5;

        private const uint EventFlagOffset = 4;
        private const uint ItemShortOffset = 3;
        private const int MaxItemCount = 6;

        public string Name;
        public long ApId;
        public uint BaseAddr;
        public GeoItem[] Items = [];
        public short BuildingId;
        public int Multi = 0;
        public BuildingCoords? AnyCoords;
        // Only D6 should have nothing for HundoCoords so this should be safe to have empty.
        public BuildingCoords HundoCoords = new();
        public BuildingCoords[]? MultiCoords;

        protected short buildingValue;
        //private short buildingCount;
        private uint placedCountAddr;
        protected uint BuildingCountAddr;
        private Towns town;

        // -1/2 pi, 0, 1/2 pi, or pi.  Matches the integer based orientation field above
        public float OrientationFloat = 0.0f;

        // Mystery values.  Buildings can't be placed without 'em!
        public int[] MInts1;
        public int[] MInts2;
        public float[] MCoords1;
        public float[] MCoords2;

        internal short BuildingValue { get => buildingValue; set => buildingValue = value; }
        internal virtual Towns Town { get => town; }

        /// <summary>
        /// Pull values from the addresses above and set them locally to compare when checking against the game's values.
        /// </summary>
        internal virtual void ReadValues()
        {
            buildingValue = Memory.ReadShort(BaseAddr);
            placedCountAddr = BaseAddr + sizeof(short);
            BuildingCountAddr = BaseAddr + sizeof(int);
            //buildingCount = Memory.ReadShort(BuildingCountAddr);
        }

        // Currently unused, keeping because it might become useful?
        private bool HasBuilding()
        {
            return Memory.ReadShort(BaseAddr) != 0;
        }

        internal void Init(Towns townId)
        {
            town = townId;
        }

        internal bool EventSeen()
        {
            return Memory.ReadByte(BaseAddr - EventFlagOffset) != 0;
        }

        private void SeeEvent()
        {
            Memory.WriteByte(BaseAddr - EventFlagOffset, 1);

            if (PlayerState.GetCurrentTown() == ((int)town))
            {
                // TODO duplicate code, consider making a method
                uint addr = GeoAddrs.BldDataTable;
                // Find first empty entry in the table
                int index = 0;
                while (Memory.ReadInt(addr + GeoAddrs.BldDataBldIdOffset) != BuildingId && index < 128)
                {
                    addr += GeoAddrs.BldDataTableOffset;
                    index++;
                }

                uint table = Memory.ReadUInt(addr + GeoAddrs.BldDataAddrOffset);
                Memory.Write(table + sizeof(int), 1);
            }
        }

        internal void UnseeEvent()
        {
            Memory.WriteByte(BaseAddr - EventFlagOffset, 0);
        }

        /// <summary>
        /// Give player a building piece while they are in town.  Needs to update an extra table paged in based on current town.
        /// </summary>
        internal virtual void GiveBuildingTown()
        {
            uint baseAddr = (uint)(GeoAddrs.CurTownFirstBld + GeoAddrs.CurTownBldOffset * BuildingId);

            Memory.Write(baseAddr + GeoAddrs.CurTownBldIdOffset, buildingValue + 1);

            if (Multi > 0 && buildingValue < Multi)
            {
                Memory.Write(baseAddr + GeoAddrs.CurTownBldOwnedOffset, Multiplier*(buildingValue+1));
                Memory.Write(baseAddr + GeoAddrs.CurTownBldCountOffset, Multiplier * (buildingValue + 1));
            }
            else if (buildingValue == 0)
            {
                Memory.Write(baseAddr + GeoAddrs.CurTownBldOwnedOffset, 1);
                Memory.Write(baseAddr + GeoAddrs.CurTownBldCountOffset, 1);
            }
            else if (buildingValue <= Items.Length)
            {
                GeoItem item = Items[buildingValue - 1];
                uint itemAddr = (uint)(baseAddr + GeoAddrs.CurTownBldPiece1FlagOffset + (GeoAddrs.CurTownBldPieceOffset * item.SlotId));
                Memory.Write(itemAddr, 1);
            }
            // Nothing to place as an extra copy has been received
            else
                return;

            GiveBuilding(true);
        }

        /// <summary>
        /// </summary>
        internal virtual void GiveBuilding(bool inTown = false)
        {
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
                    BuildMultiBuilding(inTown);
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
                    BuildBuilding(inTown);
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
                {
                    if (!Memory.Write(itemAddr, (short)1))
                    {
                        Log.Logger.Error("Failed to add " + Name + ", please report with emulator version and reproduction steps.");
                        return;
                    }
                }
                else
                    MemFuncs.GiveGeoItem(town, (short)item.ItemId);

                // Skip the dialog-only events for the 4 pilots
                if (town == Towns.Factory && MiscConstants.FactoryEventSkips.Contains(BuildingId) && buildingValue == Items.Length)
                {
                    SeeEvent();
                    CharFuncs.GoTAccess();
                }
                // Skip having to view the d6 events after getting the last item
                else if (town == Towns.Castle && buildingValue == Items.Length)
                {
                    SeeEvent();
                    EventMasks.SetD6Flag(BuildingId);
                }

                buildingValue++;

                msg = "Received " + item.Name + " for " + town.ToString() + ".";
            }

            if (msg != null)
            {
                Memory.Write(BaseAddr, buildingValue);
                ItemQueue.AddMsg(msg);
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
        protected int CountThisBuilding()
        {
            int count = 0;
            foreach (ItemInfo item in App.Client.CurrentSession.Items.AllItemsReceived)
            {
                if (item.ItemId == ApId) count++;
            }
            return count;
        }

        #region TownBuilding
        private void BuildBuilding(bool inTown)
        {
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
                    if (ApId == MiscConstants.CouscousId)
                    {
                        uint earthBAddr = FindBuildingById(town, MiscConstants.EarthBId);
                        WriteBuildingMap(HundoCoords, inTown, earthBAddr != 0);
                    }
                    else if (ApId == MiscConstants.MushroomId)
                    {
                        uint earthAAddr = FindBuildingById(town, MiscConstants.EarthAId);
                        WriteBuildingMap(HundoCoords, inTown, earthAAddr != 0);
                    }
                    else if (ApId == MiscConstants.EarthBId)
                    {
                        uint couscousAddr = FindBuildingById(town, MiscConstants.CouscousId);
                        if (couscousAddr != 0)
                        {
                            couscousAddr += sizeof(long);
                            Memory.Write(couscousAddr, buildings[MiscConstants.CouscousId].HundoCoords.Y);
                        }
                        WriteBuildingMap(HundoCoords, inTown);
                    }
                    else if (ApId == MiscConstants.EarthAId)
                    {
                        uint mushAddr = FindBuildingById(town, MiscConstants.MushroomId);
                        if (mushAddr != 0)
                        {
                            mushAddr += sizeof(long);
                            Memory.Write(mushAddr, buildings[MiscConstants.MushroomId].HundoCoords.Y);
                        }
                        WriteBuildingMap(HundoCoords, inTown);
                    }
                    else if (ApId >= MiscConstants.Watermill1Id && ApId <= MiscConstants.Watermill3Id)
                    {
                        if (ApId == MiscConstants.Watermill1Id)
                        {
                            if (buildings[MiscConstants.MatatakiRiverId].buildingValue > 0)
                                WriteBuildingMap(HundoCoords, inTown);
                        }
                        else
                        {
                            // Need at least 2 sets of river pieces for the second and third watermill
                            if (buildings[MiscConstants.MatatakiRiverId].buildingValue > 1)
                                WriteBuildingMap(HundoCoords, inTown);
                        }
                    }
                    else
                        WriteBuildingMap(HundoCoords, inTown);
                }
                else
                    WriteBuildingMap(HundoCoords, inTown);
            }
            else if (AnyCoords != null && Options.Autobuild == AutobuildFlags.Any)
            {
                if (town == Towns.Matataki && ApId == MiscConstants.Watermill1Id)
                {
                    if (buildings[MiscConstants.MatatakiRiverId].buildingValue > 0)
                        WriteBuildingMap(AnyCoords, inTown);
                }
                else if (town == Towns.Matataki && (ApId == MiscConstants.Watermill2Id || ApId == MiscConstants.Watermill3Id))
                {
                    if (buildings[MiscConstants.MatatakiRiverId].buildingValue > 1)
                        WriteBuildingMap(AnyCoords, inTown);
                }
                else
                    WriteBuildingMap(AnyCoords, inTown);
            }
        }

        private void BuildMultiBuilding(bool inTown)
        {
            // Ewwww...
            if (MultiCoords != null &&
                 (Options.Autobuild == AutobuildFlags.Hundo ||
                 (Options.Autobuild == AutobuildFlags.Any && town == Towns.Matataki && ApId == MiscConstants.MatatakiRiverId) ||  // Need rivers for any%
                 (town == Towns.Muska && (Options.Autobuild == AutobuildFlags.Muska || Options.Autobuild == AutobuildFlags.MuskaRobot)) ||
                 (town == Towns.Factory && (Options.Autobuild == AutobuildFlags.Robot || Options.Autobuild == AutobuildFlags.MuskaRobot)))
               )
            {
                if (!(town == Towns.Norune && ApId == MiscConstants.NoruneBridgeId) &&
                    !(town == Towns.Matataki && ApId == MiscConstants.MatatakiBridgeId))
                {
                    int max = Multiplier * buildingValue;
                    int i = Multiplier * (buildingValue - 1);
                    // Leave off the 25th river piece so the player can access the "bugged" MCs before finishing the river
                    if (town == Towns.Matataki && ApId == MiscConstants.MatatakiRiverId && max >= Multiplier * MiscConstants.MatatakiReqRiverCount)
                    {
                        max--;
                        i--;
                    }

                    for (; i < max; i++)
                    {
                        WriteBuildingMap(MultiCoords[i], inTown);
                    }

                    if (town == Towns.Norune && ApId == MiscConstants.NoruneRiverId &&
                        buildingValue == MiscConstants.NoruneBridgeRiverCount &&
                        buildings[MiscConstants.NoruneBridgeId].buildingValue > 0)
                    {
                        buildings[MiscConstants.NoruneBridgeId].BuildMultiBuilding(inTown);
                    }

                    if (town == Towns.Matataki && ApId == MiscConstants.MatatakiRiverId)
                    {
                        if (buildingValue == 1 && buildings[MiscConstants.Watermill1Id].buildingValue > 0)
                        {
                            buildings[MiscConstants.Watermill1Id].BuildBuilding(inTown);
                        }
                        else if (buildingValue == 2)
                        {
                            if (buildings[MiscConstants.Watermill2Id].buildingValue > 0)
                                buildings[MiscConstants.Watermill2Id].BuildBuilding(inTown);
                            if (buildings[MiscConstants.Watermill3Id].buildingValue > 0)
                                buildings[MiscConstants.Watermill3Id].BuildBuilding(inTown);
                        }
                        
                        if (buildingValue == MiscConstants.MatatakiBridgeRiverCount &&
                            buildings[MiscConstants.MatatakiBridgeId].buildingValue > 0)
                        {
                            buildings[MiscConstants.MatatakiBridgeId].BuildMultiBuilding(inTown);
                        }
                    }
                }
                // Norune Bridge after River
                else if (town == Towns.Norune && ApId == MiscConstants.NoruneBridgeId &&
                    buildings[MiscConstants.NoruneRiverId].buildingValue >= MiscConstants.NoruneBridgeRiverCount)
                {
                    for (int i = Multiplier * (buildingValue - 1); i < Multiplier * buildingValue; i++)
                    {
                        WriteBuildingMap(MultiCoords[i], inTown);
                    }
                }
                // Matataki Bridge after River
                else if (town == Towns.Matataki && ApId == MiscConstants.MatatakiBridgeId &&
                    buildings[MiscConstants.MatatakiRiverId].buildingValue >= MiscConstants.MatatakiBridgeRiverCount)
                {
                    for (int i = Multiplier * (buildingValue - 1); i < Multiplier * buildingValue; i++)
                    {
                        WriteBuildingMap(MultiCoords[i], inTown);
                    }
                }
            }
        }

        // Is this harder than it needs to be? maybe. It is fun though!
        private void WriteBuildingMap(BuildingCoords map, bool inTown, bool useY = true)
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
            Vector vector = new(map.X, map.Y, map.Z);
            if (!useY) vector.y = 0.0f;

            Memory.WriteStruct<Vector>(addr, vector);

            if (MultiCoords != null)
                Memory.Write(placedCountAddr, (short)(buildingValue * 5));
            else
                Memory.Write(placedCountAddr, (short)1);

            if (inTown)
            {
                // Note: table has 0-127 possible entries but no town has that many geo parts
                addr = GeoAddrs.BldDataTable;
                // Find first empty entry in the table
                int index = 0;
                while (Memory.ReadInt(addr + GeoAddrs.BldDataCoordsOffset) != 0 && index < 128)
                {
                    addr += GeoAddrs.BldDataTableOffset;
                    index++;
                }

                // Most of this table is copied from a table paged in for the town starting 0x003977C0, then +2A0 for each building
                uint sourceAddr = GeoAddrs.BldDataTableSrc + (uint)(GeoAddrs.BldDataTableOffset * BuildingId);
                byte[] source = Memory.ReadByteArray(sourceAddr, GeoAddrs.BldDataTableOffset);
                Memory.WriteByteArray(addr, source);

                Memory.Write(addr + GeoAddrs.BldDataTableIdxOffset, map.TableIndex);

                // Orientation value * .5pi.  Needed for orientation to set correctly.
                Memory.Write(addr + GeoAddrs.BldDataFOrientOffset, map.OrientationFloat);
                Memory.Write(addr + GeoAddrs.BldDataOrientOffset, (int)map.Orientation);

                // If Couscous/mush are already placed, put them on top of the Earths before placing the Earths
                if (town == Towns.Matataki)
                {
                    if (ApId == MiscConstants.EarthAId)
                        UpdateBldY(MiscConstants.MushroomId);
                    else if (ApId == MiscConstants.EarthBId)
                        UpdateBldY(MiscConstants.CouscousId);
                }

                Memory.WriteStruct<Vector>(addr + GeoAddrs.BldDataCoordsOffset, vector);
                Memory.Write(addr + GeoAddrs.BldDataCoordsOffset + sizeof(float) * 3, 1.0f);  // W value for the quaternion; not used in the other coords so for reuse of vector object, not putting in the struct

                Memory.Write(addr + GeoAddrs.BldDataHundoOffset, 100.0f);

                int partsExtra = Memory.ReadInt(addr + GeoAddrs.BldDataPartsExtraOffset);

                // TODO Need to either figure out how to trigger the game placing road/river/bridge, do some tedious work to make pieces place with correct geometry, or just not do these piece types
                //if (partsExtra == 1)
                //{
                //    if (town == Towns.Norune)
                //        Memory.Write(addr + 0xB0, 0x0101d690);
                //    else if (town == Towns.Muska && BuildingId == 0x0D)
                //    {
                //        Memory.Write(addr + 0xE8, 1);
                //        Memory.Write(addr + 0xB0, 0xee7cd0);
                //        Memory.Write(addr + 0xBC, 0xEE8550);
                //        Memory.Write(addr + 0xD0, 0xee8dd0);
                //    }
                //}

                // Not actually sure what this is, but is the value actually used in the grid table. Usually same as the building ID but river, bridge and others have different numbers.
                int partsNo = Memory.ReadInt(addr + GeoAddrs.BldDataPartsNoOffset);

                foreach (uint mapAddr in map.Addrs)
                {
                    GridEntry entry = new();
                    entry.BuildingId = (uint)partsNo;
                    entry.tableIndex = (uint)index;
                    if (partsExtra != 0)
                        entry.eighty = GeoAddrs.BldDataPartExtraFlag;
                    entry.partsExtra = partsExtra;

                    Memory.WriteStruct(mapAddr, entry);
                }
            }
        }

        private static void UpdateBldY(int bldId)
        {
            // Note: table has 0-127 possible entries but no town has that many geo parts
            uint addr2 = GeoAddrs.BldDataTable;
            // Find first empty entry in the table
            int index2 = 0;
            bool found = false;
            GeoBuilding bld = buildings[bldId];
            if (bld.buildingValue == 0) return;  // Building not placed, nothing to do

            while (Memory.ReadInt(addr2 + GeoAddrs.BldDataCoordsOffset) != 0 && index2 < 128)
            {
                if (Memory.ReadInt(addr2 + GeoAddrs.BldDataBldIdOffset) == bld.BuildingId)
                {
                    found = true;
                    break;
                }

                addr2 += GeoAddrs.BldDataTableOffset;
                index2++;
            }

            if (found)
            {
                Memory.Write(addr2 + GeoAddrs.BldDataCoordsOffset + sizeof(float), bld.HundoCoords.Y);
            }
        }

        /// <summary>
        /// Finds the given building ID for the given town and returns the addr for it.  Returns 0 if not found.
        /// </summary>
        /// <param name="town">Town to search through.</param>
        /// <param name="id">Building ID to search for.</param>
        /// <returns>The addr for the building or 0 if not found.</returns>
        private static uint FindBuildingById(Towns town, int apid)
        {
            uint addr = GeoAddrs.TownMapAddrs[((int)town)];
            ushort temp = (ushort)Memory.ReadShort(addr);
            GeoBuilding target = buildings[apid];

            while (temp != 0xFFFF)
            {
                short buildingIdMem = Memory.ReadShort(addr);
                if (buildingIdMem == target.BuildingId)
                {
                    return addr;
                }
                addr += 0x10;
                temp = (ushort)Memory.ReadShort(addr);
            }

            return 0;
        }
        #endregion
    }
}
