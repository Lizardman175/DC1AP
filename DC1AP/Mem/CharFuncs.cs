using Archipelago.Core.Util;
using DC1AP.Constants;
using DC1AP.Items;

namespace DC1AP.Mem
{
    internal class CharFuncs
    {
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
            xiao = Memory.ReadByte(MiscAddrs.XiaoSlotAddr) != 0xff;
            goro = Memory.ReadByte(MiscAddrs.GoroSlotAddr) != 0xff;
            ruby = Memory.ReadByte(MiscAddrs.RubySlotAddr) != 0xff;
            ungaga = Memory.ReadByte(MiscAddrs.UngagaSlotAddr) != 0xff;
            osmond = Memory.ReadByte(MiscAddrs.OsmondSlotAddr) != 0xff;
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

        #region CharUnlocks
        /// <summary>
        /// When the player recruits Xiao, give them Matataki & Queens access, back of DBC (conditional)
        /// </summary>
        private static void XiaoGained()
        {
            if (PlayerState.PlayerReady())
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
            }

        }

        private static void GoroGained()
        {
            if (PlayerState.PlayerReady())
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
            }
        }

        private static void RubyGained()
        {
            if (PlayerState.PlayerReady())
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
            }
        }

        private static void UngagaGained()
        {
            if (PlayerState.PlayerReady())
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
            }
        }

        private static void OsmondGained()
        {
            if (PlayerState.PlayerReady())
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
            }
        }
        #endregion
    }
}
