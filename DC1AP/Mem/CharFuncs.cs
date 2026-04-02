using Archipelago.Core.Util;
using DC1AP.Constants;
using DC1AP.Items;
using System;

namespace DC1AP.Mem
{
    internal class CharFuncs
    {
        private static readonly char[] normalCharTable =
        [
            'A', 'B', 'C', 'D', 'E', 'F', 'G', 'H', 'I', 'J', 'K', 'L', 'M',
            'N', 'O', 'P', 'Q', 'R', 'S', 'T', 'U', 'V', 'W', 'X', 'Y', 'Z',

            'a', 'b', 'c', 'd', 'e', 'f', 'g', 'h', 'i', 'j', 'k', 'l', 'm',
            'n', 'o', 'p', 'q', 'r', 's', 't', 'u', 'v', 'w', 'x', 'y', 'z',

            '0', '1', '2', '3', '4', '5', '6', '7', '8', '9',

            ' ', '!', '"', '#', '$', '%', '&', '\'', '(', ')', '*', '+', ',', '-', '.', '/',
            ':', '<', '=', '>', '?', '@', '[', ']', '|', '{', '}', '~'
        ];

        private static readonly short[] nameCharTable =
        [
            // A-Z
            0xA2, 0xA3, 0xA4, 0xA5, 0xA6, 0xA7, 0xA8, 0xA9, 0xAA, 0xAB, 0xAC, 0xAD, 0xAE,
            0xAF, 0xB0, 0xB1, 0xB2, 0xB3, 0xB4, 0xB5, 0xB6, 0xB7, 0xB8, 0xB9, 0xBA, 0xBB,

            // a-z
            0xBC, 0xBD, 0xBE, 0xBF, 0xC0, 0xC1, 0xC2, 0xC3, 0xC4, 0xC5, 0xC6, 0xC7, 0xC8,
            0xC9, 0xCA, 0xCB, 0xCC, 0xCD, 0xCE, 0xCF, 0xD0, 0xD1, 0xD2, 0xD3, 0xD4, 0xD5,

            //0     1     2     3     4     5     6     7     8     9
            0xF2, 0xF3, 0xF4, 0xF5, 0xF6, 0xF7, 0xF8, 0xF9, 0xFA, 0xFB,

            //SPC   !     "     #     $       %     &     '     (     )     *     +     ,       -     .       /
            0xE6, 0xD9, 0xD8, 0xDB, 0x0102, 0xE1, 0xDC, 0xD6, 0xE3, 0xE4, 0xDF, 0xDD, 0x0100, 0xDE, 0x0101, 0xE0,

            //:     <     =     >     ?     @     [     ]     |     {     }    ~
            0xFF, 0xE8, 0xD7, 0xE9, 0xDA, 0xE5, 0xF0, 0xF1, 0xE7, 0xEA, 0xEB, 0xE2

            // EC-EF are half brackets, not including
            // FC is an unused period. FD is some hollow period?, FE is more whitespace
            // Curiously, the game uses E6 and 0103 for whitespace.  The latter only when selecting the empty space
            // just after the $.  Not sure if there is any difference
        ];

        private const int ToanIndex = 0;  // Placeholder value since OpenMem supports a Toan flag
        private const int XiaoIndex = 1;
        private const int GoroIndex = 2;
        private const int RubyIndex = 3;
        private const int UngagaIndex = 4;
        private const int OsmondIndex = 5;

        private static bool xiao = false;
        private static bool goro = false;
        private static bool ruby = false;
        private static bool ungaga = false;
        private static bool osmond = false;

        public static bool Xiao { get => xiao; set => xiao = value; }
        public static bool Goro { get => goro; set => goro = value; }
        public static bool Ruby { get => ruby; set => ruby = value; }
        public static bool Ungaga { get => ungaga; set => ungaga = value; }
        public static bool Osmond { get => osmond; set => osmond = value; }

        internal static bool HaveChar(int index)
        {
            switch (index)
            {
                case (int)Towns.Norune:
                    return xiao;
                case (int)Towns.Matataki:
                    return goro;
                case (int)Towns.Queens:
                    return ruby;
                case (int)Towns.Muska:
                    return ungaga;
                case (int)Towns.Factory:
                case (int)Towns.Castle:
                    return osmond;
                default:
                    return false;
            }
        }

        internal static void Init()
        {
            xiao = Memory.ReadByte(MiscAddrs.XiaoSlotAddr) != 0xff && OpenMem.CheckCharReceived(XiaoIndex);
            goro = Memory.ReadByte(MiscAddrs.GoroSlotAddr) != 0xff && OpenMem.CheckCharReceived(GoroIndex);
            ruby = Memory.ReadByte(MiscAddrs.RubySlotAddr) != 0xff && OpenMem.CheckCharReceived(RubyIndex);
            ungaga = Memory.ReadByte(MiscAddrs.UngagaSlotAddr) != 0xff && OpenMem.CheckCharReceived(UngagaIndex);
            osmond = Memory.ReadByte(MiscAddrs.OsmondSlotAddr) != 0xff && OpenMem.CheckCharReceived(OsmondIndex);
        }

        internal static void CheckForChars()
        {
            if (!xiao)
            {
                if (Memory.ReadByte(MiscAddrs.XiaoSlotAddr) != 0xff)
                    XiaoGained();
            }
            else if (!goro)
            {
                if (Memory.ReadByte(MiscAddrs.GoroSlotAddr) != 0xff)
                    GoroGained();
            }
            else if (!ruby)
            {
                if (Memory.ReadByte(MiscAddrs.RubySlotAddr) != 0xff)
                    RubyGained();
            }
            else if (!ungaga)
            {
                if (Memory.ReadByte(MiscAddrs.UngagaSlotAddr) != 0xff)
                    UngagaGained();
            }
            else if (!osmond)
            {
                if (Memory.ReadByte(MiscAddrs.OsmondSlotAddr) != 0xff)
                    OsmondGained();
            }
        }

        internal static void SetDefaultCharName(uint addr, string name)
        {
            char[] nameArray = name.ToCharArray();

            int i;

            for (i = 0; i < nameArray.Length; i++)
            {
                char c = nameArray[i];
                int index = Array.IndexOf(normalCharTable, c);
                Memory.Write(addr, nameCharTable[index]);
                addr += sizeof(short);
            }

            // Erase extra chars
            for (;  i < 10; i++)
            {
                Memory.Write(addr, (short)0);
                addr += sizeof(short);
            }
        }

        #region CharUnlocks
        /// <summary>
        /// When the player recruits Xiao, give them Matataki & Queens access, back of DBC (conditional)
        /// </summary>
        private static void XiaoGained()
        {
            if (PlayerState.PlayerMovableInTown())
            {
                xiao = true;
                if (Options.OpenDungeon && Memory.ReadByte(MiscAddrs.FloorCountAddrs[(int)Towns.Norune]) != 0xFF)
                {
                    Memory.WriteByte(MiscAddrs.FloorCountAddrs[(int)Towns.Norune], MiscAddrs.FloorCountRear[(int)Towns.Norune]);
                }

                Memory.WriteByte(MiscAddrs.MapFlagAddr, 0x01);

                Weapons.GiveCharWeapon((int)Towns.Norune + 1);
                if (Options.MiracleSanity)
                    InventoryMgmt.VerifyItems();

                OpenMem.SetCharReceived(XiaoIndex);
            }
        }

        private static void GoroGained()
        {
            if (PlayerState.PlayerMovableInTown())
            {
                goro = true;
                if (Options.OpenDungeon && Memory.ReadByte(MiscAddrs.FloorCountAddrs[(int)Towns.Matataki]) != 0xFF)
                {
                    Memory.WriteByte(MiscAddrs.FloorCountAddrs[(int)Towns.Matataki], MiscAddrs.FloorCountRear[(int)Towns.Matataki]);
                }

                if (Options.Goal > (int)Towns.Matataki + 1)
                {
                    Memory.WriteByte(MiscAddrs.QueensCountAddr, 1);
                }

                Weapons.GiveCharWeapon((int)Towns.Matataki + 1);
                if (Options.MiracleSanity)
                    InventoryMgmt.VerifyItems();

                OpenMem.SetCharReceived(GoroIndex);
            }
        }

        private static void RubyGained()
        {
            if (PlayerState.PlayerMovableInTown())
            {
                ruby = true;
                if (Options.OpenDungeon && Memory.ReadByte(MiscAddrs.FloorCountAddrs[(int)Towns.Queens]) != 0xFF)
                {
                    // -1 so we don't add the boss floor, requiring the player to get the key.
                    Memory.WriteByte(MiscAddrs.FloorCountAddrs[(int)Towns.Queens], (byte)(MiscAddrs.FloorCountRear[(int)Towns.Queens] - 1));
                }

                if (Options.Goal > (int)Towns.Queens + 1)
                {
                    Memory.WriteByte(MiscAddrs.MuskaCountAddr, 1);
                    Memory.WriteByte(MiscAddrs.SMTExtCountAddr, 1);
                }

                Weapons.GiveCharWeapon((int)Towns.Queens + 1);
                if (Options.MiracleSanity)
                    InventoryMgmt.VerifyItems();

                OpenMem.SetCharReceived(RubyIndex);
            }
        }

        private static void UngagaGained()
        {
            if (PlayerState.PlayerMovableInTown())
            {
                ungaga = true;
                if (Options.OpenDungeon && Memory.ReadByte(MiscAddrs.FloorCountAddrs[(int)Towns.Muska]) != 0xFF)
                {
                    Memory.WriteByte(MiscAddrs.FloorCountAddrs[(int)Towns.Muska], MiscAddrs.FloorCountRear[(int)Towns.Muska]);
                }

                if (Options.Goal > (int)Towns.Muska + 1)
                {
                    Memory.WriteByte(MiscAddrs.YDCountAddr, 1);
                    Memory.WriteByte(MiscAddrs.MFCountAddr, 1);
                }

                Weapons.GiveCharWeapon((int)Towns.Muska + 1);
                if (Options.MiracleSanity)
                    InventoryMgmt.VerifyItems();

                OpenMem.SetCharReceived(UngagaIndex);
            }
        }

        private static void OsmondGained()
        {
            if (PlayerState.PlayerMovableInTown())
            {
                osmond = true;
                if (Options.OpenDungeon && Memory.ReadByte(MiscAddrs.FloorCountAddrs[(int)Towns.Factory]) != 0xFF)
                {
                    Memory.WriteByte(MiscAddrs.FloorCountAddrs[(int)Towns.Factory], MiscAddrs.FloorCountRear[(int)Towns.Factory]);
                }

                if (Options.Goal > (int)Towns.Factory + 1)
                {
                    Memory.WriteByte(MiscAddrs.DHCCountAddr, 1);
                }

                Weapons.GiveCharWeapon((int)Towns.Factory + 1);
                if (Options.MiracleSanity)
                    InventoryMgmt.VerifyItems();

                OpenMem.SetCharReceived(OsmondIndex);
            }
        }
        #endregion
    }
}
