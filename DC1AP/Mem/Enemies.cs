using Archipelago.Core.Util;
using System;

namespace DC1AP.Mem
{
    internal class Enemies
    {
        private const uint FirstEnemy = 0x0027FB00;  // Some text indicator of the enemy.  Probably a filename reference
        private const char EnemyIndicator = 'e';  // First value of above address if an enemy we want to edit

        private const uint EnemyOffset = 0x9C;
        private const uint ABSOffset = 0x6C;
        //private const uint EnemyIdOffset = 0x7C;
        //2A34B4 (what is this?)

        private const int FirstEnemyDefaultAbs = 5;

        internal static void MultiplyABS()
        {
            if (Options.AbsMultiplier == 1.0f)
                return;

            bool checkIfMultiplied = true;
            uint enemyAddr = FirstEnemy;
            uint enemyAbsAddr = FirstEnemy + ABSOffset;
            byte enemyText;

            do
            {
                enemyText = Memory.ReadByte(enemyAddr);

                if (enemyText == EnemyIndicator)
                {
                    int tempAbs = Memory.ReadInt(enemyAbsAddr);
                    // Don't multiply if already multiplied
                    if (checkIfMultiplied)
                    {
                        if (tempAbs != FirstEnemyDefaultAbs)
                            return;
                        checkIfMultiplied = false;
                    }
                    tempAbs = (int)MathF.Round(Options.AbsMultiplier * tempAbs);
                    Memory.Write(enemyAbsAddr, tempAbs);
                }

                enemyAddr += EnemyOffset;
                enemyAbsAddr += EnemyOffset;
            } while (enemyText > 0);
        }
    }
}
