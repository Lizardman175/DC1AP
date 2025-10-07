using Archipelago.Core.Util;
using DC1AP.Constants;
using DC1AP.Items;

namespace DC1AP.Mem
{
    internal class CharFuncs
    {
        private static Boolean xiao = false;
        private static Boolean goro = false;
        private static Boolean ruby = false;
        private static Boolean ungaga = false;
        private static Boolean osmond = false;

        // Indicates which weapon slot is equipped by the char 0-9.  FF for no weapon/not recruited.
        //private static uint ToanWeaponSlot = 0x01CDD88C;  // Never sets to FF, even at title screen
        private static uint XiaoSlotAddr = 0x01CDD88D;
        private static uint GoroSlotAddr = 0x01CDD88E;
        private static uint RubySlotAddr = 0x01CDD88F;
        private static uint UngagaSlotAddr = 0x01CDD890;
        private static uint OsmondSlotAddr = 0x01CDD891;

        internal static bool Xiao { get => xiao; }
        internal static bool Goro { get => goro; }
        internal static bool Ruby { get => ruby; }
        internal static bool Ungaga { get => ungaga; }
        internal static bool Osmond { get => osmond; }

        internal static void Init()
        {
            xiao = false;
            goro = false;
            ruby = false;
            ungaga = false;
            osmond = false;

            if (Memory.ReadByte(XiaoSlotAddr) == 0xff)
            {
                Memory.MonitorAddressForAction<Byte>(XiaoSlotAddr, () => XiaoGained(), (o) => { return o != 0xff; });
            }
            else xiao = true;

            if (Memory.ReadByte(GoroSlotAddr) == 0xff)
            {
                Memory.MonitorAddressForAction<Byte>(GoroSlotAddr, () => GoroGained(), (o) => { return o != 0xff; });
            }
            else goro = true;

            // The else fallthrough if Options fails shouldn't affect anything for the randomzier on these checks.
            if (Memory.ReadByte(RubySlotAddr) == 0xff)
            {
                if (Options.Goal > 2)
                    Memory.MonitorAddressForAction<Byte>(RubySlotAddr, () => RubyGained(), (o) => { return o != 0xff; });
            }
            else ruby = true;

            if (Memory.ReadByte(UngagaSlotAddr) == 0xff)
            {
                if (Options.Goal > 3)
                    Memory.MonitorAddressForAction<Byte>(UngagaSlotAddr, () => UngagaGained(), (o) => { return o != 0xff; });
            }
            else ungaga = true;

            if (Memory.ReadByte(OsmondSlotAddr) == 0xff)
            {
                if (Options.Goal > 4)
                    Memory.MonitorAddressForAction<Byte>(OsmondSlotAddr, () => OsmondGained(), (o) => { return o != 0xff; });
            }
            else osmond = true;
        }

        #region CharUnlocks
        // Unlock things based on the player gaining chars and the YAML options.

        /// <summary>
        /// When the player recruits Xiao, give them Matataki & Queens access, back of DBC (conditional)
        /// </summary>
        private static void XiaoGained()
        {
            if (PlayerState.PlayerReady())
            {
                xiao = true;
                if (Options.OpenDungeon)
                {
                    Memory.WriteByte(MiscAddrs.FloorCountAddrs[(int)Towns.Norune], MiscAddrs.FloorCountRear[(int)Towns.Norune]);
                }

                Memory.WriteByte(MiscAddrs.MapFlagAddr, 0x01);

                if (Options.Goal > ((int)Towns.Matataki) + 1)
                {
                    Memory.WriteByte(MiscAddrs.QueensCountAddr, 1);
                }

                Weapons.GiveCharWeapon(1);
            }
            else
                Memory.MonitorAddressForAction<Byte>(XiaoSlotAddr, () => XiaoGained(), (o) => { return o != 0xff; });

        }

        private static void GoroGained()
        {
            if (PlayerState.PlayerReady())
            {
                goro = true;
                if (Options.OpenDungeon)
                {
                    Memory.WriteByte(MiscAddrs.FloorCountAddrs[(int)Towns.Matataki], MiscAddrs.FloorCountRear[(int)Towns.Matataki]);
                }

                if (Options.Goal > (int)Towns.Matataki)
                {
                    Memory.WriteByte(MiscAddrs.QueensCountAddr, 1);
                }

                Weapons.GiveCharWeapon(2);
            }
            else
                Memory.MonitorAddressForAction<Byte>(GoroSlotAddr, () => GoroGained(), (o) => { return o != 0xff; });
        }

        private static void RubyGained()
        {
            if (PlayerState.PlayerReady())
            {
                ruby = true;
                if (Options.OpenDungeon)
                {
                    Memory.WriteByte(MiscAddrs.FloorCountAddrs[(int)Towns.Queens], MiscAddrs.FloorCountRear[(int)Towns.Queens]);
                }

                if (Options.Goal > (int)Towns.Queens)
                {
                    Memory.WriteByte(MiscAddrs.MuskaCountAddr, 1);
                    Memory.WriteByte(MiscAddrs.SMTExtCountAddr, 1);
                }

                Weapons.GiveCharWeapon(3);
            }
            else
                Memory.MonitorAddressForAction<Byte>(RubySlotAddr, () => RubyGained(), (o) => { return o != 0xff; });
        }

        private static void UngagaGained()
        {
            if (PlayerState.PlayerReady())
            {
                ungaga = true;
                if (Options.OpenDungeon)
                {
                    Memory.WriteByte(MiscAddrs.FloorCountAddrs[(int)Towns.Muska], MiscAddrs.FloorCountRear[(int)Towns.Muska]);
                }

                if (Options.Goal > (int)Towns.Muska)
                {
                    Memory.WriteByte(MiscAddrs.YDCountAddr, 1);
                    Memory.WriteByte(MiscAddrs.MFCountAddr, 1);
                }

                Weapons.GiveCharWeapon(4);
            }
            else
                Memory.MonitorAddressForAction<Byte>(UngagaSlotAddr, () => UngagaGained(), (o) => { return o != 0xff; });
        }

        private static void OsmondGained()
        {
            if (PlayerState.PlayerReady())
            {
                osmond = true;
                if (Options.OpenDungeon)
                {
                    Memory.WriteByte(MiscAddrs.FloorCountAddrs[(int)Towns.Factory], MiscAddrs.FloorCountRear[(int)Towns.Factory]);
                }

                if (Options.Goal > (int)Towns.Factory)
                {
                    Memory.WriteByte(MiscAddrs.DHCCountAddr, 1);
                }

                Weapons.GiveCharWeapon(5);
            }
            else
                Memory.MonitorAddressForAction<Byte>(OsmondSlotAddr, () => OsmondGained(), (o) => { return o != 0xff; });
        }
        #endregion
    }
}

