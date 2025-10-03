using Archipelago.Core.Util;

namespace DC1AP.Mem
{
    internal class EventMasks
    {
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
            InitMask(0x01CE43A8, 0xC0);
            // Skips initial cutscene when entering Matataki
            InitMask(0x01CE43A9, 0x02);
            // Skip dran pre-fight dialog (1b)
            InitMask(0x01CE43AC, 0x01);
            // Skip town building tutorial (and manual item)
            InitMask(0x01CE43AD, 0x01);
            // Skips the withered tree convo when first visiting Matataki
            InitMask(0x01CE43AE, 0x08);
            // First dungeon floor tutorial + cat cutscene
            InitMask(0x01CE43B4, 0xB0);
            // More of the dungeon tutorials I believe.  4 is the lock-on tutorial, 1 is charge attack upgrade
            InitMask(0x01CE43B5, 0x07);
        }

        private static void InitMask(uint addr, byte mask)
        {
            byte tempMask = Memory.ReadByte(addr);
            tempMask |= mask;
            Memory.WriteByte(addr, tempMask);
        }
    }
}
