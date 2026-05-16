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

        internal override Towns Town { get
            {
                GeoBuilding? bld = DetermineBuilding();
                Towns t = bld == null ? Towns.Castle : bld.Town;
                return t;
            }
        }

        private ProgCharBuilding()
        {
            Name = "Progressive Char Building";
            ApId = MiscConstants.ProgCharBldId;

            buildingValue = 0;
            BuildingCountAddr = OpenMem.ProgCharBldAddr;
        }

        internal override void ReadValues()
        {
            // Intentionally empty override to avoid running base method
        }

        internal override void GiveBuildingTown()
        {
            DetermineBuilding()?.GiveBuildingTown();
        }

        internal override void GiveBuilding(bool inTown = false)
        {
            DetermineBuilding()?.GiveBuilding();
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