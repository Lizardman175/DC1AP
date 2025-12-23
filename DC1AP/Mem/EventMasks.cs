using Archipelago.Core.Util;
using System;

namespace DC1AP.Mem
{
    internal class EventMasks
    {
        internal static uint DialogAddr1 = 0x01CE43A8;
        // 0x80: skip pre-SMT dialog
        internal static uint DialogAddr2 = 0x01CE43A9;
        internal static uint DialogAddr3 = 0x01CE43AC;
        internal static uint DialogAddr4 = 0x01CE43AD;  // 0x02 is releated to end game cutscenes
        internal static uint DialogAddr5 = 0x01CE43AE;
        internal static uint DialogAddr6 = 0x01CE43B4;
        internal static uint DialogAddr7 = 0x01CE43B5;
        internal static uint DialogAddr8 = 0x01CE43B8;
        internal static uint DialogAddr9 = 0x01CE43AA;

        // Does more than just shipwreck, but that's the only thing we interact with it for currently
        private static uint ShipwreckKeyAddr = 0x01CE43AB;
        private const byte ShipwreckKeyValue = 0xFF - 0x10;

        // 01CE43AF Boss kill flag for Joe?: sets to 0x01

        // Flags for last dungeon events viewed.
        // 0x40 = crowning day, 0x80 = ceremony
        private static uint EastKingStoryAddr1 = 0x01CE43C4;
        // 0x01 = reunion, 0x02 = campaign, 0x04 = menace, 0x08 = Deal
        // 0x10 = dark power, 0x20 = assassin, 0x40 = protected, 0x80 = demon 
        private static uint EastKingStoryAddr2 = 0x01CE43C5;
        // 0x01 = Things Lost, 0x02 = Departure 
        private static uint EastKingStoryAddr3 = 0x01CE43C6;

        internal static (uint, byte)[] D6StorySkip = [(EastKingStoryAddr1, 0x40), (EastKingStoryAddr1, 0x80),
            (EastKingStoryAddr2, 0x01), (EastKingStoryAddr2, 0x02), (EastKingStoryAddr2, 0x04), (EastKingStoryAddr2, 0x08),
            (EastKingStoryAddr2, 0x10), (EastKingStoryAddr2, 0x20), (EastKingStoryAddr2, 0x40), (EastKingStoryAddr2, 0x80),
            (EastKingStoryAddr3, 0x01), (EastKingStoryAddr3, 0x02)];

        // Note: Norune starts 0xC further than one expects.  Presumably removed content.
        private const uint T1NpcChatFlagAddr = 0x01CD5290;
        private const int T1NpcChatCount = 12;
        private const uint T2NpcChatFlagAddr = 0x01CD5E3C;
        private const int T2NpcChatCount = 12;
        // Joker has a slot without having hidden pieces
        private const uint T3NpcChatFlagAddr = 0x01CD69F4;
        private const int T3NpcChatCount = 13;
        // Note: Like Norune, the first slot doesn't actually affect anything.  Count reflects Chief not having any ? pieces
        private const uint T4NpcChatFlagAddr = 0x01CD75B8;
        private const int T4NpcChatCount = 10;

        private static readonly (uint, int)[] TownNpcDialogFlags =
            [(T1NpcChatFlagAddr, T1NpcChatCount),
             (T2NpcChatFlagAddr, T2NpcChatCount),
             (T3NpcChatFlagAddr, T3NpcChatCount),
             (T4NpcChatFlagAddr, T4NpcChatCount) ];

        private const uint TownNpcChatOffset = 0x0C;
        private const byte NpcChatFlag = 0x02;

        /*
         * Unused but known flags/values:
         * 
         * TODO more info in comments about other bits we aren't setting as values are discovered
         * 
         * 1CE4828 == 2 after the opening cutscenes.  Can be used to test if the player is loaded better than other location I think
         * 1CE43AB 0001 when opening Dran's doorg
         * 0x01CE43A8 Flags: 1 skips mayor dialog for key (and also items!), 2 skips using the mayor's key to access the dungeon
         *                   4 skips first Goro convo before fight, 8 skips Goro fight
         *                   
         * 1CE47A8 is a short that goes up by 100 per boss kill (sort of... Utan doesn't affect, Saia bumps it to 300)
         * Could manually incrment by 100 after Utan if that is how it works? Then monitor the value (offset for the extra 100 from Saia)
         * 100 after Dran, 300 after Saia, 400 after Curse, 500 after Joe.  Need to know how fighting bosses out of order will affect this or we can't use it for Saia.
         */
        internal static void InitMasks()
        {
            // Skip first scene entering Queens (40) & first dialog with Randro (80)
            // 11b skips need to get the mayor's key & open DBC, but you skip getting the first items.
            OrMask(DialogAddr1, 0xC0);
            // Skips initial cutscene when entering Matataki
            OrMask(DialogAddr2, 0x02);
            // Skip dran pre-fight dialog (1b)
            OrMask(DialogAddr3, 0x01);
            // Skip town building tutorial (and manual item)
            OrMask(DialogAddr4, 0x01);
            // Skips the withered tree convo when first visiting Matataki
            OrMask(DialogAddr5, 0x08);
            // First dungeon floor tutorial + cat cutscene
            OrMask(DialogAddr6, 0xB0);
            // More of the dungeon tutorials I believe.  4 is the lock-on tutorial, 1 is charge attack upgrade
            OrMask(DialogAddr7, 0x07);
            // Muska Lacka dialog.  0x04 and 0x08 are the Theo/Ungaga dialogs. 0x10 is the gol/sil convo on floor 9, 0x20 and 0x40 are gol and sil being dead respectively.
            OrMask(DialogAddr8, 0x0C);
            // Addr8+1 goes to 0x10 for Osmond scene
            
            OrMask(DialogAddr9, 0x40); // Factory entrance sequence
            OrMask(DialogAddr3, 0x08); // Skip the pre robot battle dialog. (doesn't actually skip it?)
            OrMask(DialogAddr3, 0x10); // Genie/Robot fight
            OrMask(DialogAddr3, 0x80); // Skip initial DHC dialog

            InitNpcFlags();
        }

        private const byte yayaMask = 0x60;

        internal static void SkipYaya()
        {
            // 0x20 for fruit scene, 0x40 for Rando scene. Genie scene doesn't seem to set a flag?
            OrMask(DialogAddr2, yayaMask);
        }

        internal static bool YayaDone()
        {
            return (Memory.ReadByte(DialogAddr2) & yayaMask) == yayaMask;
        }

        /// <summary>
        /// Set the flags indicating NPCs have been talked to enabling putting pieces into their buildings.
        /// </summary>
        private static void InitNpcFlags()
        {
            for (int ii = 0; ii < Math.Min(TownNpcDialogFlags.Length, Options.Goal); ii++)
            {
                var item = TownNpcDialogFlags[ii];
                uint addr = item.Item1;
                for (int jj = 0; jj < item.Item2; jj++)
                {
                    OrMask(addr, NpcChatFlag);
                    addr += TownNpcChatOffset;
                }
            }
        }

        /// <summary>
        /// Logical ORs the given mask into the byte at addr.
        /// </summary>
        /// <param name="addr">Address to OR</param>
        /// <param name="mask">Value to OR into the address</param>
        private static void OrMask(uint addr, byte mask)
        {
            byte tempMask = Memory.ReadByte(addr);
            tempMask |= mask;
            Memory.WriteByte(addr, tempMask);
        }

        internal static void ClearShipwreckKey()
        {
            byte tempMask = Memory.ReadByte(ShipwreckKeyAddr);
            tempMask &= ShipwreckKeyValue;
            Memory.WriteByte(ShipwreckKeyAddr, tempMask);
        }

        internal static void SetD6Flag(int buildingId)
        {
            (uint, byte) set = D6StorySkip[buildingId];
            OrMask(set.Item1, set.Item2);
        }
    }
}
