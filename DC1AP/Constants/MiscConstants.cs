
namespace DC1AP.Constants
{
    internal enum Towns
    {
        Norune,
        Matataki,
        Queens,
        Muska,
        Factory,
        Castle
    }

    internal enum AutobuildFlags
    {
        Off = 0,
        Any = 1,
        Hundo = 2,
        Muska = 3,
        Robot = 4,
        MuskaRobot = 5
    }

    internal class MiscConstants
    {
        internal const int BaseId = 971_110_000;

        internal const int MaxAtlaPerFloor = 8;
        internal const int GeoMaxItemCount = 120;

        // Atla mem flag values
        internal const int AtlaUnavailable = -1;
        internal const int AtlaAvailable = -2;
        internal const int AtlaClaimed = -3;

        // All building IDs are in the .json.  Some are needed for edge cases here.
        internal const short NoruneRiverId = 15;
        internal const short NoruneBridgeId = 16;
        internal const short CouscousId = 5;
        internal const short MushroomId = 7;
        internal const short EarthAId = 18;
        internal const short EarthBId = 19;
        internal const short Watermill1Id = 11;
        internal const short Watermill2Id = 12;
        internal const short Watermill3Id = 13;
        internal const short MatatakiRiverId = 16;
        internal const short MatatakiBridgeId = 17;

        internal const short MatatakiReqRiverCount = 5;
        internal const short MatatakiBridgeRiverCount = 6;
        internal const short NoruneBridgeRiverCount = 3;

        internal static int[] FactoryEventSkips = [3, 4, 9, 10];
    }
}
