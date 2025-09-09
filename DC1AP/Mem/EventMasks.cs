using Archipelago.Core.Util;

namespace DC1AP.Mem
{
    internal class EventMasks
    {                
        /*
         * Unused but known flags/values:
         * 
         * 0x01CE43A8 Flags: 1 skips mayor dialog for key (and also items!), 2 skips using the mayor's key to access the dungeon
         *                   4 skips first Goro convo before fight, 8 skips Goro fight
         */
        internal static void InitMasks()
        {
            // TODO more info in comments about other bits we aren't setting as values are discovered
            // Skips initial cutscene when entering Matataki
            InitMask(0x01CE43A9, 0x02);
            // Skip town building dialog (and manual item)
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
            byte tempMask = Memory.ReadByte(mask);
            tempMask |= mask;
            Memory.WriteByte(addr, tempMask);
        }
    }
}
