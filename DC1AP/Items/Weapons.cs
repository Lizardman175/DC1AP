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
#pragma warning disable IDE0044 // Add readonly modifier.  These are used but not directly.
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
#pragma warning restore IDE0044 // Add readonly modifier
        }

        // Hard coded weapons for now
        // Shamshir
        private static Weapon toan = new(0x010E, 20, 30, 70, 6, 32, 2, 0, 0, 8, 0, 0, 0, 0, 0, 0, 0, 0, 10, 0, 0, 0);
        // Steel slingshot
        private static Weapon xiao = new(0x012d, 14, 40, 43, 0, 48, 5, 0, 0, 0, 0, 0, 0, 0, 0, 5, 0, 0, 5, 5, 0, 0);
        // Steel Hammer
        private static Weapon goro = new(0x013c, 25, 40, 20, 0, 50, 5, 0, 0, 0, 0, 0, 0, 0, 0, 12, 0, 0, 0, 10, 10, 0);
        // Platinum ring
        private static Weapon ruby = new(0x014f, 17, 40, 66, 40, 55, 1, 1, 20, 1, 1, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0);
        // Halberd
        private static Weapon ungaga = new(0x015e, 44, 28, 75, 0, 52, 3, 0, 10, 0, 15, 0, 0, 0, 0, 0, 0, 8, 5, 8, 0, 0);
        // Snail
        private static Weapon osmond = new(0x0176, 34, 50, 60, 10, 45, 0, 5, 5, 5, 5, 5, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0);

        private static readonly Weapon[] starterWeapons = [toan, xiao, goro, ruby, ungaga, osmond];

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
