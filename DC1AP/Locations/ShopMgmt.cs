using Archipelago.Core.Util;
using DC1AP.Constants;
using Serilog;
using System.Collections.ObjectModel;

namespace DC1AP.Locations
{
    internal class ShopMgmt
    {
        internal static void UpdateShops()
        {
            // TODO instead of returning, clear changes to shop inventory?
            if (!Options.ShopShuffle) return;

            // Update price for replacer items
            uint addr = ItemValues.ShopPriceTableAddr + 4 * (ItemValues.MedusaPowderID - ItemValues.FireGemID);
            Memory.Write(addr, (short)ItemValues.Item1Cost);
            Memory.Write(addr + sizeof(short), (short)ItemValues.Item1Cost / 2);

            addr = ItemValues.ShopPriceTableAddr + 4 * (ItemValues.WarpPowderID - ItemValues.FireGemID);
            Memory.Write(addr, (short)ItemValues.Item2Cost);
            Memory.Write(addr + sizeof(short), (short)ItemValues.Item2Cost / 2);

            switch (Options.Goal)
            {
                case 6:
                    UpdateShopItems(ItemValues.SimbaShop1Index);
                    UpdateShopItems(ItemValues.SimbaShop2Index);
                    UpdateShopItems(ItemValues.SimbaShop3Index);
                    goto case 5;
                case 5:
                    UpdateShopItems(ItemValues.LedanShopIndex);
                    goto case 4;
                case 4:
                    UpdateShopItems(ItemValues.BrookeShopIndex);
                    goto case 3;
                case 3:
                    UpdateShopItems(ItemValues.RutyShopIndex);
                    UpdateShopItems(ItemValues.SuzyShop1Index);
                    UpdateShopItems(ItemValues.SuzyShop2Index);
                    UpdateShopItems(ItemValues.SuzyShop3Index);
                    UpdateShopItems(ItemValues.SuzyShop4Index);
                    UpdateShopItems(ItemValues.LanaShopIndex);
                    UpdateShopItems(ItemValues.JackShopIndex);
                    UpdateShopItems(ItemValues.JokerShopIndex);
                    goto case 2;
                case 2:
                    UpdateShopItems(ItemValues.OwlShopIndex);
                    UpdateShopItems(ItemValues.GafferShopIndex);
                    UpdateShopItems(ItemValues.GafferShopAltIndex);
                    break;
            }
        }

        internal static void UpdateShopItems(uint shopIndex)
        {
            if (shopIndex > ItemValues.ShopAddrs.Length || ItemValues.ShopAddrs[shopIndex] == 0)
            {
                Log.Logger.Error("Invalid shop index, tell Lizardman " + shopIndex + ".    ");
                return;
            }

            // Doing this here so external files can just call this function and not worry about the edge case.
            if (shopIndex == ItemValues.GafferShopIndex)
            {
                AddGafferItems();
                return;
            }

            uint baseAddr = ItemValues.ShopAddrs[shopIndex];
            uint apid = ItemValues.ShopItemAPIDs[shopIndex];

            ReadOnlyCollection<long> locations = App.Client.CurrentSession.Locations.AllMissingLocations;

            if (locations.Contains(apid))
            {
                uint addr = FindNextShopEntry(baseAddr, ItemValues.MedusaPowderID);
                if (addr == 0)
                {
                    Log.Logger.Error("Unable to add first item for shop, tell Lizardman " + shopIndex + ".    ");
                    return;
                }
                // Item already in the list, just return
                else if (addr == 1)
                    return;

                Memory.Write(addr, ItemValues.MedusaPowderID);
                Memory.Write(addr + sizeof(short), (short)-1);
            }
            else
            {
                FindAndClearShopItem(baseAddr, ItemValues.MedusaPowderID);
            }

            if (locations.Contains(apid + 1))
            {
                uint addr = FindNextShopEntry(baseAddr, ItemValues.WarpPowderID);
                if (addr == 0)
                {
                    Log.Logger.Error("Unable to add second item for shop, tell Lizardman " + shopIndex + ".    ");
                    return;
                }
                // Item already in the list, just return
                else if (addr == 1)
                    return;

                Memory.Write(addr, ItemValues.WarpPowderID);
                Memory.Write(addr + sizeof(short), (short)-1);
            }
            else
            {
                FindAndClearShopItem(baseAddr, ItemValues.WarpPowderID);
            }
        }

        private static void FindAndClearShopItem(uint addr, short itemId)
        {
            int count = 0;
            while (count < 20)  // Max shop size in the table
            {
                ushort value = (ushort)Memory.ReadShort(addr);
                if (value == itemId)
                {
                    // Peak at next value and move it
                    ushort nextValue = (ushort)Memory.ReadShort(addr + sizeof(short));
                    Memory.Write(addr, nextValue);

                    if (nextValue != 0xFFFF)
                    {
                        nextValue = (ushort)Memory.ReadShort(addr + sizeof(short) * 2);
                        Memory.Write(addr + sizeof(short), nextValue);
                        Memory.Write(addr + sizeof(short) * 2, (short)0);
                    }

                    break;
                }
                addr += 2;
                count++;
            }
        }

        private static uint FindNextShopEntry(uint addr, short itemId)
        {
            int count = 0;
            while (count < 20)  // Max shop size in the table
            {
                ushort value = (ushort)Memory.ReadShort(addr);
                if (value == 0xFFFF || value == 0)  // 0 shouldn't happen?
                    return addr;
                // Item already in list, return 1 so we don't re-add the item.
                if (value == itemId)
                    return 1;
                addr += 2;
                count++;
            }

            return 0;
        }
        
        /// <summary>
        /// Gaffer's buggy is already at the max capacity of items in the shop table (20) so temporarily replace the attack/endurance attachments.
        /// </summary>
        private static void AddGafferItems()
        {
            uint apid = ItemValues.ShopItemAPIDs[ItemValues.GafferShopIndex];

            ReadOnlyCollection<long> locations = App.Client.CurrentSession.Locations.AllMissingLocations;

            // First item, replace attack with medusa powder (or reverse)
            if (locations.Contains(apid))
                Memory.Write(ItemValues.GafferAttackAddr, ItemValues.MedusaPowderID);
            else
                Memory.Write(ItemValues.GafferAttackAddr, ItemValues.AttackID);

            if (locations.Contains(apid + 1))
                Memory.Write(ItemValues.GafferEnduranceAddr, ItemValues.WarpPowderID);
            else
                Memory.Write(ItemValues.GafferEnduranceAddr, ItemValues.EnduranceID);
        }
    }
}
