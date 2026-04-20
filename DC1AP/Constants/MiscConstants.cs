
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

    internal enum AttachMultConfig
    {
        None = 1,
        MwOnly = 2,
        All = 3
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

    internal enum ItemCategory
    {
        Inventory = 0,
        Weapon,
        Attachment,
        FactoryGeo // For removing the Sun Sphere
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
        internal const int NoruneRiverId = 971_110_115;
        internal const int NoruneBridgeId = 971_110_116;
        internal const int CouscousId = 971_110_205;
        internal const int MushroomId = 971_110_207;
        internal const int EarthAId = 971_110_218;
        internal const int EarthBId = 971_110_219;
        internal const int Watermill1Id = 971_110_211;
        internal const int Watermill2Id = 971_110_212;
        internal const int Watermill3Id = 971_110_213;
        internal const int MatatakiRiverId = 971_110_216;
        internal const int MatatakiBridgeId = 971_110_217;

        internal const int ProgCharBldId = 971_110_000;
        internal const int PlayerHouseId = 971_110_100;
        internal const int CacaosHouseId = 971_110_201;
        internal const int KingHideoutId = 971_110_308;
        internal const int SisterHouseId = 971_110_403;
        
        internal const short MatatakiReqRiverCount = 5;
        internal const short MatatakiBridgeRiverCount = 6;
        internal const short NoruneBridgeRiverCount = 3;

        internal const short SunSphereItemId = 0x0F;
        internal const short MoonOrbItemId = 0xF2;

        // Building indexes for the 4 pilots to skip their cutscenes
        internal static readonly int[] FactoryEventSkips = [3, 4, 9, 10];

        internal const int AttachIdBase = 971_112_000;
        internal const int ItemIdBase = 971_111_000;

        internal const long MoonOrbId = 971_111_242;
        internal const long FeatherId = 971_111_235;

        internal const long FishCandyId = 971_111_137;
        internal const long GrassCakeId = 971_111_138;
        internal const long ParfaitId = 971_111_139;
        internal const long JerkyId = 971_111_140;
        internal const long CookieId = 971_111_141;

        internal const long FruitOfEdenId = 971_111_180;
        internal const long GourdId = 971_111_182;

        internal const int DarkGenieApId = 971_119_999;

        internal const long HornedKeyChestId = 971_111_063;

        private const long HornedKeyApId = 971_111_207;
        private const long PocketApId = 971_111_101;
        private const long SundewApId = 971_111_225;

        internal static readonly long[] KeyItemApIds = [HornedKeyApId, PocketApId, SundewApId];

        private const short HornedKeyId = 207;
        private const short PocketId = 179;
        private const short SundewId = 225;

        internal static readonly short[] KeyItemIds = [HornedKeyId, PocketId, SundewId];
    }
}
