

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
        
        public static readonly uint[] ShopItemAPIDs = [97111_1300,  // Gaffer + Pike
                                                       97111_2300,  // Owl
                                                       97111_3300,  // Ruty
                                                       97111_3310,  // Suzy
                                                       97111_3310,  // Suzy
                                                       97111_3310,  // Suzy
                                                       97111_3310,  // Suzy
                                                       97111_3320,  // Lana
                                                       97111_3330,  // Jack
                                                       97111_4300,  // Brooke
                                                       97111_5300,  // Ledan
                                                       97111_6300,  // Simba items
                                                       97111_3340,  // Joker
                                                       97111_1300,  // Gaffer alt
                                                       0,  // No known shop for 0xE.  Seems to be a variant of the wise owl shop but lacking the sword and bait items
                                                       0,  // Rando, unused since he is missable
                                                       97111_6310,  // Simba gems
                                                       97111_6320];  // Simba attachments

        public const short FireGemID = 0x51;  // First item in shop price list; used to calculate offset for other items
        public const short MedusaPowderID = 0xAB;
        public const short WarpPowderID = 0xAD;
        public const short AttackID = 0x5B;
        public const short EnduranceID = 0x5C;
        public const short Item1Cost = 300;
        public const short Item2Cost = 500;

        public const uint GafferAttackAddr = 0x00292036;
        public const uint GafferEnduranceAddr = 0x00292038;
    }
}
