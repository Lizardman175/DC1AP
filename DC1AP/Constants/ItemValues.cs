

using System.Collections.Generic;

namespace DC1AP.Constants
{
    internal class ItemValues
    {
        /// Inventory addrs/values
        public const uint InvMaxAddr = 0x01CDD8AC;  // Byte.  Can't exceed 100 or we run past the buffer.
        public const byte InvMaxLimit = 10;  // Subtract from the value at InvMaxAddr
        public const uint InvCurAddr = 0x01CDD8AD;  // Byte.  Next byte starts the active item shorts, followed by 3 shorts giving count of the active items per slot, then shorts for the other items.

        public const uint FirstInvAddr = 0x01CDD8BA;

        /// Chest addrs/values
        public const uint TownChestDataAddr = 0x003C6BD0;
        public const uint InteriorChestDataAddr = 0x003D2710;
        public const uint ObjetTypeOffset = 0x10;
        public const uint ChestItemIdOffset = 0x1C;
        public const uint InteractableOffset = 0x90;

        // Item displayed during prickly cutscene.  Changes with zone change.  Need to account for when entering mayor's house.
        public const uint PricklyDisplayAddr = 0x00415148;
        // Determines item received from Mayor's closet
        public const uint PricklyValueAddr = 0x004156D4;

        //public const uint AlnetRewardDisplayAddr = 0x0041478C;
        //public const uint AlnetRewardValueAddr = 0x00415D54;

        /// Shops
        public const uint ShopDataTableAddr  = 0x00292020;  // Only resets with a game reset, not a save file load.
        public const uint MostRecentShopAddr = 0x002A2764;
        public const uint ShopPriceTableAddr = 0x00291B80;  // Starts with fire gem, id 81.  2 bytes of buy price, 2 of sell

        // TODO make an enum?
        // Other shop index values: rando (don't use): 0x0F, storage (at least the Hag): 0x64
        public const uint GafferShopIndex = 0x00;
        public const uint GafferShopAltIndex = 0x0D;  // No Pike version
        public const uint OwlShopIndex = 0x01;
        public const uint RutyShopIndex = 0x02;
        // Suzy has 4 shops depending on which option is selected during completion scene
        public const uint SuzyShop1Index = 0x03;  // Default shop
        public const uint SuzyShop2Index = 0x04;  // Freshen Up option
        public const uint SuzyShop3Index = 0x05;  // Magical option
        public const uint SuzyShop4Index = 0x06;  // Fighting option
        
        public const uint LanaShopIndex = 0x07;
        public const uint JackShopIndex = 0x08;
        public const uint BrookeShopIndex = 0x09;
        public const uint LedanShopIndex = 0x0A;
        public const uint JokerShopIndex = 0x0C;
        public const uint SimbaShop1Index = 0x0B;  // Items
        public const uint SimbaShop2Index = 0x10;  // Gems
        public const uint SimbaShop3Index = 0x11;  // Attachments

        public const uint ShopInvOffset = 0x28;

        public static readonly uint[] ShopAddrs = [ShopDataTableAddr, // + GafferShopIndex * ShopInvOffset,
                                                   ShopDataTableAddr + OwlShopIndex * ShopInvOffset,
                                                   ShopDataTableAddr + RutyShopIndex * ShopInvOffset,
                                                   ShopDataTableAddr + SuzyShop1Index * ShopInvOffset,
                                                   ShopDataTableAddr + SuzyShop2Index * ShopInvOffset,
                                                   ShopDataTableAddr + SuzyShop3Index * ShopInvOffset,
                                                   ShopDataTableAddr + SuzyShop4Index * ShopInvOffset,
                                                   ShopDataTableAddr + LanaShopIndex * ShopInvOffset,
                                                   ShopDataTableAddr + JackShopIndex * ShopInvOffset,
                                                   ShopDataTableAddr + BrookeShopIndex * ShopInvOffset,
                                                   ShopDataTableAddr + LedanShopIndex * ShopInvOffset,
                                                   ShopDataTableAddr + SimbaShop1Index * ShopInvOffset,
                                                   ShopDataTableAddr + JokerShopIndex * ShopInvOffset,
                                                   ShopDataTableAddr + GafferShopAltIndex * ShopInvOffset,
                                                   0,  // No known shop for 0xE.  Seems to be a variant of the wise owl shop but lacking the sword and bait items
                                                   0,  // Rando, unused since he is missable
                                                   ShopDataTableAddr + SimbaShop2Index * ShopInvOffset,
                                                   ShopDataTableAddr + SimbaShop3Index * ShopInvOffset];
        
        public static readonly uint[] ShopItemAPIDs = [97111_3100,  // Gaffer + Pike
                                                       97111_3200,  // Owl
                                                       97111_3300,  // Ruty
                                                       97111_3310,  // Suzy
                                                       97111_3310,  // Suzy
                                                       97111_3310,  // Suzy
                                                       97111_3310,  // Suzy
                                                       97111_3320,  // Lana
                                                       97111_3330,  // Jack
                                                       97111_3400,  // Brooke
                                                       97111_3500,  // Ledan
                                                       97111_3600,  // Simba items
                                                       97111_3340,  // Joker
                                                       97111_3100,  // Gaffer alt
                                                       0,  // No known shop for 0xE.  Seems to be a variant of the wise owl shop but lacking the sword and bait items
                                                       0,  // Rando, unused since he is missable
                                                       97111_3610,  // Simba gems
                                                       97111_3620];  // Simba attachments

        public const short FireGemID = 0x51;  // First item in shop price list; used to calculate offset for other items
        public const short MedusaPowderID = 0xAB;
        public const short WarpPowderID = 0xAD;
        public const short CarrotID = 0xBA;
        public const short AttackID = 0x5B;
        public const short EnduranceID = 0x5C;
        public const short Item1Cost = 300;
        public const short Item2Cost = 500;

        public const uint GafferAttackAddr = 0x00292036;
        public const uint GafferEnduranceAddr = 0x00292038;

        // int.  Default of -1
        public const uint FishCatchAddr = 0x01D1CC30;
        // int?  0 for unused but also Bobo fish, check size for zero if this is zero
        public const uint FirstFishLogAddr = 0x01CD4320;
        public const uint FishLogEntryOffset = 0x10;
        public const uint FishLogSizeOffset = 0x06;

        public static readonly Dictionary<int, int> FishLogToApId = new()
        {
            [0] = 97111_4000,    // Bobo
            [1] = 97111_4001,    // Gobbler
            [2] = 97111_4002,    // Nonky
            [3] = 97111_4003,    // Kaji
            [4] = 97111_4004,    // Baku Baku
            [5] = 97111_4005,    // Mardan Garayan
            [6] = 97111_4006,    // Gummy
            [7] = 97111_4007,    // Niler
            [8] = 97111_4009,    // Umadakara (seems unused)
            [9] = 97111_4009,    // Umadakara
            [10] = 97111_4010,   // Tarton
            [11] = 97111_4011,   // Piccoly
            [12] = 97111_4012,   // Bon
            [13] = 97111_4013,   // Hama Hama
            [14] = 97111_4014,   // Negie
            [15] = 97111_4015,   // Den
            [16] = 97111_4016,   // Heela
            [17] = 97111_4017,   // Baron Garayan
        };

        public static readonly Dictionary<int, int> FishCatchToApId = new()
        {
            [0x28] = 97111_4000, // Bobo
            [0x29] = 97111_4001, // Gobbler
            [0x2A] = 97111_4002, // Nonky
            [0x2B] = 97111_4003, // Kaji
            [0x2C] = 97111_4004, // Baku Baku
            [0x2D] = 97111_4005, // Mardan Garayan
            [0x2E] = 97111_4006, // Gummy
            [0x2F] = 97111_4007, // Niler
            [0x31] = 97111_4009, // Umadakara
            [0x1E] = 97111_4010, // Tarton
            [0x1F] = 97111_4011, // Piccoly
            [0x20] = 97111_4012, // Bon
            [0x21] = 97111_4013, // Hama Hama
            [0x22] = 97111_4014, // Negie
            [0x23] = 97111_4015, // Den
            [0x24] = 97111_4016, // Heela
            [0x25] = 97111_4017, // Baron Garayan
        };
    }
}
