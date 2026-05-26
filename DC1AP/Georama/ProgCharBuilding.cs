using Archipelago.Core.Util;
using DC1AP.Constants;
using DC1AP.Mem;

namespace DC1AP.Georama
{
    /// <summary>
    /// Special version of the GeoBuilding class that handles progressive char recruitment items.  Contains each of the
    /// four buildings in a map and will call their respective GiveBuilding() funcs in order.
    /// </summary>
    internal class ProgCharBuilding : GeoBuilding
    {
        internal static ProgCharBuilding? charBuilding;

        internal static ProgCharBuilding GetInstance()
        {
            if (charBuilding == null)
                charBuilding = new();

            return charBuilding;
        }

        private ProgCharBuilding()
        {
            Name = "Progressive Char Building";
            ApId = MiscConstants.ProgCharBldId;

            BuildingCountAddr = OpenMem.ProgCharBldAddr;
            BaseAddr = OpenMem.ProgCharBldAddr;
        }

        internal override Towns Town
        {
            get
            {
                GeoBuilding? bld = DetermineBuilding();
                Towns t = bld == null ? Towns.Castle : bld.Town;
                return t;
            }
        }

        internal override void ReadValues()
        {
            buildingValue = Memory.ReadShort(OpenMem.ProgCharBldAddr);
        }

        internal override void GiveBuildingTown()
        {
            GeoBuilding? building = DetermineBuilding();
            if (building != null)
            {
                building.GiveBuildingTown();
                buildingValue++;
                Memory.WriteByte(BuildingCountAddr, (byte)buildingValue);
            }
        }

        internal override void GiveBuilding(bool inTown = false)
        {
            GeoBuilding? building = DetermineBuilding();
            if (building != null)
            {
                building.GiveBuilding();
                buildingValue++;
                Memory.WriteByte(BuildingCountAddr, (byte)buildingValue);
            }
        }

        private GeoBuilding? DetermineBuilding()
        {
            if (buildings.TryGetValue(MiscConstants.PlayerHouseId, out GeoBuilding? playerHouse) && playerHouse.BuildingValue < 7)
            {
                return playerHouse;
            }
            else if (buildings.TryGetValue(MiscConstants.CacaosHouseId, out GeoBuilding? cacaoHouse) && cacaoHouse.BuildingValue < 7)
            {
                return cacaoHouse;
            }
            else if (buildings.TryGetValue(MiscConstants.KingHideoutId, out GeoBuilding? kingsHideout) && kingsHideout.BuildingValue < 7)
            {
                return kingsHideout;
            }
            else if (buildings.TryGetValue(MiscConstants.SisterHouseId, out GeoBuilding? sisterHouse) && sisterHouse.BuildingValue < 7)
            {
                return sisterHouse;
            }

            return null;
        }
    }
}