using Archipelago.Core.Util;
using System;

namespace DC1AP.Mem
{
    internal class Enemies
    {
        private const uint FirstEnemyAddr = 0x0027FB00;  // Some text indicator of the enemy.  Probably a filename reference
        private const char EnemyIndicator = 'e';  // First value of above address if an enemy we want to edit

        // Values: -1: dead/no enemy, 1: inactive, 2: active
        private const uint FirstDunEnemyAddr = 0x01E16BA0;
        private const uint DunEnemyOffset = 0x190;

        private const uint EnemyOffset = 0x9C;
        private const uint ABSOffset = 0x6C;
        //private const uint EnemyIdOffset = 0x7C;
        //2A34B4 (what is this?)

        private const int FirstEnemyDefaultAbs = 5;

        private static readonly uint[] DunEnemyAddrs = [FirstDunEnemyAddr, FirstDunEnemyAddr + DunEnemyOffset, FirstDunEnemyAddr + (DunEnemyOffset * 2),
            FirstDunEnemyAddr + (DunEnemyOffset * 3), FirstDunEnemyAddr + (DunEnemyOffset * 4), FirstDunEnemyAddr + (DunEnemyOffset * 5),
            FirstDunEnemyAddr + (DunEnemyOffset * 6), FirstDunEnemyAddr + (DunEnemyOffset * 7), FirstDunEnemyAddr + (DunEnemyOffset * 8),
            FirstDunEnemyAddr + (DunEnemyOffset * 9), FirstDunEnemyAddr + (DunEnemyOffset * 10), FirstDunEnemyAddr + (DunEnemyOffset * 11),
            FirstDunEnemyAddr + (DunEnemyOffset * 12), FirstDunEnemyAddr + (DunEnemyOffset * 13), FirstDunEnemyAddr + (DunEnemyOffset * 14)];

        internal static void MultiplyABS()
        {
            if (Options.AbsMultiplier == 1.0f)
                return;

            bool checkIfMultiplied = true;
            uint enemyAddr = FirstEnemyAddr;
            uint enemyAbsAddr = FirstEnemyAddr + ABSOffset;
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

        /// <summary>
        /// Returns false if any enemy is still alive on the current floor, otherwise true.
        /// </summary>
        /// <returns></returns>
        internal static bool CheckEnemyKills()
        {
            foreach (uint addr in DunEnemyAddrs)
                if (Memory.ReadInt(addr) != -1)
                    return false;

            return true;
        }
    }
}
