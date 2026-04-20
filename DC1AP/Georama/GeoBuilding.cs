using Archipelago.Core.Util;
using Archipelago.MultiClient.Net.Models;
using DC1AP.Constants;
using DC1AP.Mem;
using DC1AP.Threads;
using Serilog;
using System.Collections.Generic;
using System.Linq;

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

        /// <summary>
        /// Give player a building piece while they are in town.  Needs to update an extra table paged in based on current town.
        /// </summary>
        internal virtual void GiveBuildingTown()
        {
            // Small chance of race condition giving us 2 items at the same time.  Double check the count before adding.
            // Also prevents trying to add too many of an item if /send was used on the server.
            //if (buildingValue == CountThisBuilding()) return;

            uint baseAddr = (uint)(GeoAddrs.CurTownFirstBld + GeoAddrs.CurTownBldOffset * BuildingId);

            Memory.Write(baseAddr + GeoAddrs.CurTownBldOwnedOffset, buildingValue + 1);

            if (Multi != 0 && buildingValue < Multi)
            {
                //Memory.Write(baseAddr + GeoAddrs.CurTownBldCountOffset, Multiplier*(buildingValue+1));
            }
            else if (buildingValue == 0)
            {
                //Memory.Write(baseAddr + GeoAddrs.CurTownBldOwnedOffset, 1);
                Memory.Write(baseAddr + GeoAddrs.CurTownBldCountOffset, 1);
            }
            else
            {
                GeoItem item = Items[buildingValue - 1];
                uint itemAddr = (uint)(baseAddr + GeoAddrs.CurTownBldPiece1FlagOffset + (GeoAddrs.CurTownBldPieceOffset * item.SlotId));
                Memory.Write(itemAddr, 1);
            }

            // TODO map

            GiveBuilding(false);
        }

        /// <summary>
        /// </summary>
        internal virtual void GiveBuilding(bool inTown = false)
        {
            // Small chance of race condition giving us 2 items at the same time.  Double check the count before adding.
            // Also prevents trying to add too many of an item if /send was used on the server.
            //if (buildingValue == CountThisBuilding()) return;
            // TODO can this IF be removed?

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

                // Skip the dialog only events for the 4 pilots
                if (town == Towns.Factory && MiscConstants.FactoryEventSkips.Contains(BuildingId))
                {
                    Memory.Write(BaseAddr - EventFlagOffset, 1);
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
                        WriteBuildingMap(HundoCoords, earthBAddr != 0);
                    }
                    else if (ApId == MiscConstants.MushroomId)
                    {
                        uint earthAAddr = FindBuildingById(town, MiscConstants.EarthAId);
                        WriteBuildingMap(HundoCoords, earthAAddr != 0);
                    }
                    else if (ApId == MiscConstants.EarthBId)
                    {
                        uint couscousAddr = FindBuildingById(town, MiscConstants.CouscousId);
                        if (couscousAddr != 0)
                        {
                            couscousAddr += sizeof(long);
                            Memory.Write(couscousAddr, buildings[MiscConstants.CouscousId].HundoCoords.Y);
                        }
                        WriteBuildingMap(HundoCoords);
                    }
                    else if (ApId == MiscConstants.EarthAId)
                    {
                        uint mushAddr = FindBuildingById(town, MiscConstants.MushroomId);
                        if (mushAddr != 0)
                        {
                            mushAddr += sizeof(long);
                            Memory.Write(mushAddr, buildings[MiscConstants.MushroomId].HundoCoords.Y);
                        }
                        WriteBuildingMap(HundoCoords);
                    }
                    else if (ApId >= MiscConstants.Watermill1Id && ApId <= MiscConstants.Watermill3Id)
                    {
                        if (ApId == MiscConstants.Watermill1Id)
                        {
                            if (buildings[MiscConstants.MatatakiRiverId].buildingValue > 0)
                                WriteBuildingMap(HundoCoords);
                        }
                        else
                        {
                            // Need at least 2 sets of river pieces for the second and third watermill
                            if (buildings[MiscConstants.MatatakiRiverId].buildingValue > 1)
                                WriteBuildingMap(HundoCoords);
                        }
                    }
                    else
                        WriteBuildingMap(HundoCoords);
                }
                else
                    WriteBuildingMap(HundoCoords, true, inTown);
            }
            else if (AnyCoords != null && Options.Autobuild == AutobuildFlags.Any)
            {
                if (town == Towns.Matataki && ApId == MiscConstants.Watermill1Id)
                {
                    if (buildings[MiscConstants.MatatakiRiverId].buildingValue > 0)
                        WriteBuildingMap(AnyCoords);
                }
                else if (town == Towns.Matataki && (ApId == MiscConstants.Watermill2Id || ApId == MiscConstants.Watermill3Id))
                {
                    if (buildings[MiscConstants.MatatakiRiverId].buildingValue > 1)
                        WriteBuildingMap(AnyCoords);
                }
                else
                    WriteBuildingMap(AnyCoords);
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
                        WriteBuildingMap(MultiCoords[i]);
                    }

                    // TODO test the calls where inTown was added
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
                        WriteBuildingMap(MultiCoords[i]);
                    }
                }
                // Matataki Bridge after River
                else if (town == Towns.Matataki && ApId == MiscConstants.MatatakiBridgeId &&
                    buildings[MiscConstants.MatatakiRiverId].buildingValue >= MiscConstants.MatatakiBridgeRiverCount)
                {
                    for (int i = Multiplier * (buildingValue - 1); i < Multiplier * buildingValue; i++)
                    {
                        WriteBuildingMap(MultiCoords[i]);
                    }
                }
            }
        }

        // TODO un-default inTown to see what needs updating
        private void WriteBuildingMap(BuildingCoords map, bool useY = true, bool inTown=false)
        {
            if (inTown && MultiCoords == null && town == Towns.Norune)// used to diable pond:  && BuildingId != 12)
            {
                // TODO need to find next available slot rather than BuildingId as the index
                uint addr = 0x00376E80 + (uint)(0x2a0 * BuildingId);

                //Memory.Write(0x377040, 0x42c80000);
                //Memory.Write(0x377048, 0x01);
                //Memory.Write(0x37704C, 0x01);

                // Orientation value * .5pi.  Needed for orientation to set correctly.
                Memory.Write(addr + 0x54, OrientationFloat);

                // Seem to be memory addresses where data for the building live.  doesn't seem to affect anything to ignore?
                //Memory.Write(0x3770A8, 0x3c3f40);
                //Memory.Write(0x3770AC, 0x3c3fB0);

                // Makes the building tangible.  Actually a collection of floats.  Seems to be boundary coordinates for the building or something?
                // TODO Hoping we don't need these for river/road/bridge
                for (int i = 0; i < MCoords1.Length; i++)
                {
                    Memory.Write(addr + 0x120 + (uint)(i * 4), MCoords1[i]);
                }
                for (int i = 0; i < MCoords2.Length; i++)
                {
                    Memory.Write(addr + 0x130 + (uint)(i * 4), MCoords2[i]);
                }

                // Needed to enter building
                Memory.Write(addr + 0xE0, (long)0);
                //Memory.Write(0x376F80, 0x449920);

                // These 7 are needed to make the building actually appear.  Not sure what they mean
                for (int i = 0; i < MInts1.Length; i++)
                {
                    Memory.Write((uint)(addr + 0xA0 + i * 0x4), MInts1[i]);
                }
                //Memory.Write(0x376F20, 0xa00050);
                //Memory.Write(0x376F24, 0xa392d0);
                //Memory.Write(0x376F28, 0xa63c90);

                for (int i = 0; i < MInts2.Length; i++)
                {
                    Memory.Write((uint)(addr + 0xC0 + i * 0x4), MInts2[i]);
                }
                //Memory.Write(0x376F40, 0xa82dd0);
                //Memory.Write(0x376F44, 0xa7c150);
                //Memory.Write(0x376F48, 0xa8ef10);
                //Memory.Write(0x376F4C, 0xa956d0);

                Memory.Write(addr + 0xE8, (long)map.Orientation);
                Memory.Write(addr + 0xD8, (long)BuildingId);
                Memory.Write(addr + 0xF8, GeoAddrs.CurTownFirstBld + GeoAddrs.CurTownBldOffset*BuildingId);
                //Memory.Write(addr + 0xF8 + 4, 0x0C);

                Memory.Write(addr, map.X);
                //addr += sizeof(float);

                if (useY) Memory.Write(addr + sizeof(float), map.Y);
                else Memory.Write(addr, 0.0f);
                //addr += sizeof(float);

                Memory.Write(addr + sizeof(float)*2, map.Z);
                Memory.Write(addr + sizeof(float)*3, 1.0f);

                // Doesn't seem we need to set these
                //Memory.Write(addr + 0x1C, 0xDACB8981);
                //Memory.Write(addr + 0x2C, 0xDACB8981);
                //Memory.Write(addr + 0x3C, 0xDACB8981);
                //Memory.Write(addr + 0x5C, 0xDACB8981);
                //Memory.Write(addr + 0x6C, 0xDACB8981);
                //Memory.Write(addr + 0x7C, 0xDACB8981);

                // Magic value to make buildings render
                if (town == Towns.Norune && BuildingId == 12)
                    Memory.Write(addr + 0xD4, 01);
                else
                    Memory.Write(addr + 0xD4, 02);

                //addr = 0x00376E80 + 
            }
            else
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
            int temp = Memory.ReadInt(addr);
            GeoBuilding target = buildings[apid];

            while (temp != -1)
            {
                short buildingIdMem = Memory.ReadShort(addr);
                if (buildingIdMem == target.BuildingId)
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
