
namespace DC1AP.Constants
{
    internal static class GeoAddrs
    {
        // Elements of a house start here as 2 byte fields.  non-zero = item placed. can be used to verify collected AP items if necessary
        public const uint HouseInvOffset = 0x06;

        // Distance in mem between npc houses
        public const uint HouseOffset = 24;

        // TODO presumed distance between town parts: 0x820.  might differ town to town based on size/count of things
        // 4 bytes before the start of Toan's house is a flag related to the completion event.  set means the event has been seen.  requires full building completion to function
        // unknown what the short after events might do yet. Might be related to D6?

        // Distance between building fields
        public const uint BldFieldDelta = sizeof(short);

        /*
         * 4 bytes before the values = event seen flag.  Don't really need (for now) so addresses point to the useful bytes.
         *  - Might be able to set this to avoid watching the final dungeon's memory scenes.
         * nonzero at this address (short) == on
         * move over 1 short: number of the element placed
         * 1 more short: the building count (useful for rivers, etc.)
         * next 6 shorts are flags for item being slotted in
         * 
         * Note: each town starts 3000 bytes apart
         */
        // Norune georama buildings (houses etc.; left pane)
        public const uint T1GeoBldStart = 0x01CD4828;

        // Matataki Georama
        public const uint T2GeoBldStart = 0x01CD53E0;

        // Queens georama
        public const uint T3GeoBldStart = 0x01CD5F98;

        // Muska georama
        public const uint T4GeoBldStart = 0x01CD6B4C;

        // Factory georama
        public const uint T5BldStart = 0x01CD7708;

        // Castle georama
        public const uint T6BldStart = 0x01CD82C4;

        public static readonly uint[] TownBldInv = [T1GeoBldStart, T2GeoBldStart, T3GeoBldStart, T4GeoBldStart, T5BldStart, T6BldStart];

        // Distance between inventories of towns
        public const uint GeoInvOffset = 0x100;

        // Norune village georama inventory.  Each of these is a short, default of -1 if empty.
        private const uint T1GeoInv = 0x01CD8E0C;
        private const uint T2GeoInv = T1GeoInv + GeoInvOffset; // Matataki offset
        private const uint T3GeoInv = T2GeoInv + GeoInvOffset; // Presumed Queens offset
        private const uint T4GeoInv = T3GeoInv + GeoInvOffset; // Presumed ML offset
        private const uint T5GeoInv = T4GeoInv + GeoInvOffset; // Presumed Factory offset
        private const uint T6GeoInv = T5GeoInv + GeoInvOffset; // Presumed Last dungeon offset

        public static readonly uint[] TownGeoInv = [T1GeoInv, T2GeoInv, T3GeoInv, T4GeoInv, T5GeoInv, T6GeoInv];

        // Atla per floor memory start addr by dungeon.  4 bytes each.
        // -1 == uninit or no atla there.  -2 == available (also various >0 values for static floor atla).  -3 == collected
        private const uint DBCAtlaFlag = 0x01CD97C4;
        private const uint WOFAtlaFlag = 0x01CD9CC4;
        private const uint SWAtlaFlag = 0x01CDA1C4;
        private const uint SMTAtlaFlag = 0x01CDA6C4;
        private const uint FacAtlaFlag = 0x01CDABC4;
        private const uint CastleAtlaFlag = 0x01CDB0C4;

        public static uint[] AtlaFlagAddrs = [DBCAtlaFlag, WOFAtlaFlag, SWAtlaFlag, SMTAtlaFlag, FacAtlaFlag, CastleAtlaFlag];

        // Georama loot table references.  All are 4byte fields.  As these tables get initialized, they'll be filled with junk IDs
        // This table is initialized when entering the dungeon for the first time.  We'll need to watch for the memory to change then set the values to junk
        public const uint GeoFloorOffset = 4;
        public const uint GeoCountOffset = 8;
        public const uint GeoItemOffset = 12;  // Offset to next entry in table from start addr.

        public const uint NoruneTableAddr = 0x01CDB5C4;  // Toan's House is the first one.
        public const int NoruneTableCount = 78;  // 78 unique entries, some have multiple copies in the count field.
        public const int DBC1Count = 43;

        public const uint MatatakiTableAddr = 0x01CDBA74;
        public const int MatatakiTableCount = 87;
        public const int WOF1Count = 54;

        public const uint QueensTableAddr = 0x01CDBF24;
        public const int QueensTableCount = 71;
        public const int SW1Count = 42;

        public const uint MuskaTableAddr = 0x01CDC3D4;
        public const int MuskaTableCount = 65;
        public const int SMT1Count = 33;

        public const uint FactoryTableAddr = 0x01CDC884;
        public const int FactoryTableCount = 56;
        public const int MS1Count = 29;

        public const uint CastleTableAddr = 0x01CDCD34;
        public const int CastleTableCount = 62;

        public static readonly List<(uint, int)> AtlaTables = new([(NoruneTableAddr, NoruneTableCount), (MatatakiTableAddr, MatatakiTableCount),
                                                                   (QueensTableAddr, QueensTableCount), (MuskaTableAddr, MuskaTableCount),
                                                                   (FactoryTableAddr, FactoryTableCount), (CastleTableAddr, CastleTableCount)]);

        // TODO need to get the halfway count for the moon and add it here as well
        // TODO how should the last dungeon be handled?
        // TODO better name? first half of each dungeon count.
        public static readonly List<int> AtlaHalfwayCounts = new([DBC1Count, WOF1Count, SW1Count, SMT1Count, FactoryTableCount, CastleTableCount]);

        public const uint CatlaAddr = 0x01CD98A4;  // Cat's Atla

        // Flags to enable georama parts for each town.  Furthest out flag will enable all before it (boolean shorts)
        public const uint NoruneGeoFlagAddr = 0x1CE7028;
        public const uint MatatakiGeoFlagAddr = 0x1CE702A;
        public const uint QueensGeoFlagAddr = 0x1CE702C;
        public const uint MuskaGeoFlagAddr = 0x1CE702E;
        public const uint FactoryGeoFlagAddr = 0x1CE7030;

        // TODO: there doesn't appear to be a flag for the last dungeon? For now, add the factory one again so the array
        // has 6 elements.  The only thing I've found to set the last geo also enables the last dungeon: 0x1CE70D2
        public static uint[] GeoMenuFlagAddrs = [NoruneGeoFlagAddr, MatatakiGeoFlagAddr, QueensGeoFlagAddr, MuskaGeoFlagAddr, FactoryGeoFlagAddr, FactoryGeoFlagAddr];

        // Coords and flag for first atla on the given floor. Here for reference
        // XYZ all F32, flag is int.
        //public static uint AtlaCoordX = 0x01DD0860;
        //public static uint AtlaCoordY = 0x01DD0864;
        //public static uint AtlaCoordZ = 0x01DD0868;
        public static uint AtlaCollectedFlag = 0x01DD0874;

        // Space between atla as above
        public static int FloorAtlaOffset = 0x20;
    }
}

/*
 * TODO Info for 0.3 when autobuilding towns:
 * 
 * 1CD4A64: Seems to be start of first building placed in Norune.  Short.  building ID (ordered in finished list: 00 is player house, 01 is Macho house, 0d is a tree etc.)
 * 1CD4A66: Orientation of building.  00 == faces towards Mayor, 02 == faces away.  values outside 0-3 or -3-0 cause Bad Things
 * Next 3 are F32 coords: XYZ (Y is unused outside of Queens).  Putting a building outside the allowed bounds seems to just reset it in your inventory
 * 1CD4A68: 4 byte float
 * 1CD4A6C: 4 bytes of 0 for most towns, used in queens
 * 1CD4A70: 4 byte float
 */