using Archipelago.Core.Util;
using DC1AP.Constants;

namespace DC1AP.Items
{
    // shoutout to https://deconstruction.fandom.com/wiki/Dark_Cloud for the indexes/sizes
    internal class Weapons
    {
        private struct Weapon(short id, short atk, short end, short spd, short mgc, short whp,
                              byte selectedElement, byte fi, byte ice, byte lit, byte wind, byte holy,
                              byte drag, byte undead, byte fish, byte rock, byte plant,
                              byte beast, byte sky, byte metal, byte mimic, byte mage)
        {
            private short id = id;
            private short level = 0;

            private short atk = atk;
            private short end = end;
            private short spd = spd;
            private short mgc = mgc;
            private short maxhp = whp;

            private float hp = (float)whp;

            private short exp = 0;
            private byte selectedElement = selectedElement;

            private byte fi = fi;
            private byte ice = ice;
            private byte lit = lit;
            private byte wind = wind;
            private byte holy = holy;

            private byte drag = drag;
            private byte undead = undead;
            private byte fish = fish;
            private byte rock = rock;
            private byte plant = plant;
            private byte beast = beast;
            private byte sky = sky;
            private byte metal = metal;
            private byte mimic = mimic;
            private byte mage = mage;
        }

        // Hard coded weapons for now
        // Shamshir
        private static Weapon toan = new(0x010E, 20, 30, 70, 6, 32, 1, 0, 8, 0, 0, 0, 0, 0, 0, 0, 0, 0, 10, 0, 0, 0);
        // Steel slingshot
        private static Weapon xiao = new(0x012d, 14, 40, 43, 0, 48, 5, 0, 0, 0, 0, 0, 0, 0, 0, 5, 0, 0, 5, 5, 0, 0);
        // Steel Hammer
        private static Weapon goro = new(0x013c, 25, 40, 20, 0, 50, 5, 0, 0, 0, 0, 0, 0, 0, 0, 12, 0, 0, 0, 10, 10, 0);
        // Platinum ring
        private static Weapon ruby = new(0x014f, 17, 40, 66, 40, 55, 1, 0, 20, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0);
        // Halberd
        private static Weapon ungaga = new(0x015e, 44, 28, 75, 0, 52, 3, 0, 10, 0, 15, 0, 0, 0, 0, 0, 0, 8, 5, 8, 0, 0);
        // Snail
        private static Weapon osmond = new(0x0176, 34, 50, 60, 10, 45, 0, 5, 5, 5, 5, 5, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0);

        private static readonly Weapon[] starterWeapons = [toan, xiao, goro, ruby, ungaga, osmond];

        // TODO remove or comment out before checkin!
        // Chronicle2 Sword
        //private static Weapon cheatToan = new(0x012A, 350, 99, 99, 99, 99, 5, 99, 99, 99, 99, 99, 99, 99, 99, 99, 99, 99, 99, 99, 99, 99);
        // Angel Gear slingshot
        //private static Weapon cheatXiao = new(0x0139, 256, 99, 99, 99, 99, 4, 99, 99, 99, 99, 99, 99, 99, 99, 99, 99, 99, 99, 99, 99, 99);
        // Inferno
        //private static Weapon cheatGoro = new(0x0149, 350, 99, 99, 99, 99, 3, 99, 99, 99, 99, 99, 99, 99, 99, 99, 99, 99, 99, 99, 99, 99);
        // Secret Armlet
        //private static Weapon cheatRuby = new(0x0159, 155, 99, 99, 99, 99, 2, 99, 99, 99, 99, 99, 99, 99, 99, 99, 99, 99, 99, 99, 99, 99);
        // Hercule's Wrath
        //private static Weapon cheatUngaga = new(0x0164, 256, 99, 99, 50, 99, 1, 99, 99, 99, 99, 99, 99, 99, 99, 99, 99, 99, 99, 99, 99, 99);
        // Supernova
        //private static Weapon cheatOsmond = new(0x0175, 256, 99, 99, 99, 99, 0, 99, 99, 99, 99, 99, 99, 99, 99, 99, 99, 99, 99, 99, 99, 99);

        //private static readonly Weapon[] cheatWeapons = [cheatToan, cheatXiao, cheatGoro, cheatRuby, cheatUngaga, cheatOsmond];

        internal static void GiveCharWeapon(int character)
        {
            if (Options.StarterWeapons)
            {
                uint weaponAddr = MiscAddrs.WeaponAddrs[character] + MiscAddrs.WeaponOffset;

                // First weapon should already be set at this point, so skip that slot and look for a blank.
                int i;
                for (i = 1; i < 10; i++)
                {
                    short weaponValue = Memory.ReadShort(weaponAddr);
                    if (weaponValue == -1)
                    {
                        break;
                    }
                    weaponAddr += MiscAddrs.WeaponOffset;
                }

                if (i < 10)  // If the player already has a full inventory of weapons for the char, don't bother giving another one.
                    Memory.WriteStruct(weaponAddr, starterWeapons[character]);
            }
        }
    }
}
