
namespace DC1AP.Constants
{
    // Misc address values that aren't enough to warrant grouping to their own file.
    internal class MiscAddrs
    {
        internal const uint GameIdAddr = 0x0029E4F0;

        // These are blocks of shorts mapped to a table of allowable characters.
        // 0x00A2 is A, 0x00BC is a.  Go up from there to get other letters.  Before A2 is Japanese.
        //internal const uint ToanNameAddr = 0x1CD4188;
        //internal const uint XiaoNameAddr = 0x1CD41C8;
        //internal const uint GoroNameAddr = 0x1CD4208;
        //internal const uint RubyNameAddr = 0x1CD4248;
        //internal const uint UngagaNameAddr = 0x1CD4288;
        //internal const uint OsmondNameAddr = 0x1CD42C8;

        // 1CD4100 : B10B

        // For managing adding items
        //internal const uint InvStart = 0x01CDD8BA;
        //internal const uint AttachStart = 0x01CE1A48;

        // Set to 1 to enable the world map
        internal const uint MapFlagAddr = 0x1CDD86C;

        // Addresses for visit count of each town/dungeon on the map.  value >0 makes them show up on the map (curious what negative numbers would do)
        // Value > 0 also prevents dungeons from initializing by default.
        //internal const int NoruneCountAddr = 0x1CE7028; // Norune village counter (clearing it makes Norune unavailable; unless doing a non-standard start, don't edit!)
        internal const uint DBCCountAddr = 0x1CE70C8; // DBC counter

        // There is no value that will disable Matataki other than the map flag itself
        internal const uint WOFCountAddr = 0x1CE70CA; // Wise owl forest

        //internal const uint BrownbooCountAddr = 0x1CE7044; // Brownboo Village

        internal const uint QueensCountAddr = 0x1CE702C;
        internal const uint SWCountAddr = 0x01CE70CC;

        internal const uint MuskaCountAddr = 0x01CE702E;
        internal const uint SMTExtCountAddr = 0x01CE707C; // sun/moon temple exterior counter
        internal const uint SMTIntCountAddr = 0x01CE70CE; // SMT interior (dungeon)

        internal const uint MFCountAddr = 0x01CE7030; // Moon factory
        internal const uint YDCountAddr = 0x01CE7056; // Yellow Drops
        internal const uint MSCountAddr = 0x01CE70D0; // Moon sea

        internal const uint GOTCountAddr = 0x01CE70D2; // Gallery of Time
        internal const uint DHCCountAddr = 0x01CE7078; // DHC? Seems to also activate ThE sHaFt

        // >0 will open the demon shaft dungeon for business
        //internal const uint ShaftCounterAddr = 0x1CE70A0;

        // How many floors are available for a given dungeon.  0 indexed.  Setting these early will skip normal dungeon initialization.
        internal const uint DBCFloorCountAddr = 0x01CDD80B;
        internal const uint WOFFloorCountAddr = 0x01CDD80C;
        internal const uint SWFloorCountAddr  = 0x01CDD80D;
        internal const uint SMTFloorCountAddr = 0x01CDD80E;
        internal const uint MSFloorCountAddr  = 0x01CDD80F;
        internal const uint DHCFloorCountAddr = 0x01CDD810;
        internal const uint DSFloorCountAddr  = 0x01CDD811;

        internal static uint[] FloorCountAddrs = [DBCFloorCountAddr, WOFFloorCountAddr, SWFloorCountAddr, SMTFloorCountAddr, MSFloorCountAddr, DHCFloorCountAddr];
        // Floor counts are 0 indexed
        internal static byte[] FloorCountFront = [7, 8, 8, 8, 7, 23];
        internal static byte[] FloorCountRear  = [14, 16, 16, 17, 14, 23];

        // 1 = Walking Mode, 2 = On Menu, 3 = Door Menu, 4 = Floor picker screen, 5 = Ally Quick Select,  7 = Next Floor Screen
        // Only give items/atla in dungeon when this is 1
        internal const uint DungeonMode = 0x002A355C;
        internal const uint CurDungeon = 0x002A3594;    // 0 index
        //internal const uint CurDungeon = 0x1CD954C;     // 0 index - I don't think this is correct, sometimes it is not the correct dungeon value
        internal const uint CurFloor = 0x01CD954E;      // 0 index
        internal const uint InDungeonFlag = 0x1CD954F;  // -1 if not in dungeon, 0 if in dungeon
        internal const uint BackFloorFlag = 0x002A34B4;  // 0 or 1

        // Player state.  0 is main title, 1 is demo reel, 2 is town, 3 is dungeon
        internal const uint PlayerState = 0x002A2534;

        // Technically these are the kill counts on the boss floors, but can only be 1
        //internal const uint DranFlag = 0x01CDD200;
        internal const uint UtanFlag = 0x01CDD2CC;
        //internal const uint SaiaFlag = 0x01CE47A9;  // Others are using the kill count but we can't here as her shield counts as a kill
        //internal const uint CurseFlag = 0x01CDD45E;
        //internal const uint JoeFlag = 0x01CDD520;
        //internal const uint GenieFlag = 0x01CDD5FC;

        internal const uint BossKillAddr = 0x01CE47A8;  // Sets to 100 x boss number (except for Utan?)

        //internal static readonly uint[] BossKillFlags = [DranFlag, UtanFlag, SaiaFlag, CurseFlag, JoeFlag, GenieFlag];

        // Curiously, there are 11 weapon slots in memory, but only 10 in game?
        internal const uint WeaponOffset = 0xF8;
        //internal const uint CharWeaponsOffset = 0x0AA8;
        private const uint ToanWeaponAddr = 0x01CDDA58;
        private const uint XiaoWeaponAddr = 0x01CDE500;
        private const uint GoroWeaponAddr = 0x01CDEFA8;
        private const uint RubyWeaponAddr = 0x01CDFA50;
        private const uint UngagaWeaponAddr = 0x01CE04F8;
        private const uint OsmondWeaponAddr = 0x01CE0FA0;

        internal static readonly uint[] WeaponAddrs = [ToanWeaponAddr, XiaoWeaponAddr, GoroWeaponAddr,  RubyWeaponAddr, UngagaWeaponAddr, OsmondWeaponAddr];

        internal const int EnemyCount = 15;
        internal const uint EnemyOffset = 0x0190;
        internal const uint FirstEnemy = 0x01E16BA0;
        internal const uint FirstEnemyAbsAddr = 0x01E16C50;

        internal const uint InvMaxAddr = 0x01CDD8AC;  // Byte.  Can't exceed 100 or we run past the buffer.
        internal const uint InvCurAddr = 0x01CDD8AD;  // Byte.  Next byte starts the active item shorts, followed by 3 shorts giving count of the active items per slot, then shorts for the other items.
    }
}
