using Archipelago.Core.Util;

namespace DC1AP.Mem
{
    internal class Enemies
    {
        private static uint EnemyOffset = 0x0190;
        private static uint ABSOffset = 0xb0;

        private static uint FirstEnemy = 0x01E16BA0;  // This field is an int. -1 if no enemy (or mimic?)
        //2A34B4

        /// <summary>
        /// Expects caller to know if this was already done to not do it multiple times.
        /// </summary>
        internal static void MultiplyABS()
        {
            float mult = Options.AbsMultiplier;

            if (mult == 1)
                return;

            // Brief sleep as we are fighting the game initializing the dungeon.
            Thread.Sleep(2000);

            uint enemyAddr = FirstEnemy;
            uint enemyAbsAddr = enemyAddr + ABSOffset;

            for (int i = 0; i < 15; i++)
            {
                if (Memory.ReadInt(enemyAddr) != -1)
                {
                    int tempAbs = Memory.ReadInt(enemyAbsAddr);
                    tempAbs = (int) MathF.Round(mult * tempAbs);
                    Memory.Write(enemyAbsAddr, tempAbs);
                }

                enemyAddr += EnemyOffset;
                enemyAbsAddr += EnemyOffset;
            }
        }
    }
}
