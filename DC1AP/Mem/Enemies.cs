using Archipelago.Core.Util;
using System;
using System.Threading;

namespace DC1AP.Mem
{
    internal class Enemies
    {
        private static uint EnemyOffset = 0x9C;
        private static uint ABSOffset = 0x2C;
        //private static uint EnemyIdOffset = 0x3C;

        private static uint FirstEnemy = 0x0027FB40;  // some text indicator of the enemy.  probably a filename reference
        //2A34B4 (what is this?)

        internal static void MultiplyABS()
        {
            float mult = Options.AbsMultiplier;

            if (mult == 1)
                return;

            uint enemyAddr = FirstEnemy;
            uint enemyAbsAddr = FirstEnemy + ABSOffset;
            byte enemyText;

            do
            {
                enemyText = Memory.ReadByte(enemyAddr);

                // Letter 'e'.
                // Some enemy data begins with 'c' but isn't actually enemies? This might be skipping bosses
                if (enemyText == 0x65)
                {
                    int tempAbs = Memory.ReadInt(enemyAbsAddr);
                    tempAbs = (int)MathF.Round(mult * tempAbs);
                    Memory.Write(enemyAbsAddr, tempAbs);
                }

                enemyAddr += EnemyOffset;
                enemyAbsAddr += EnemyOffset;
            } while (enemyText > 0);
        }
    }
}
