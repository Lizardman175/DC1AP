using Archipelago.Core.Models;
using Archipelago.Core.Util;
using DC1AP.Constants;
using DC1AP.Mem;
using DC1AP.Threads;

namespace DC1AP.Georama
{
    internal class GeoBuilding
    {
        public static short Multiplier = 5;

        private const int ItemShortOffset = 3;
        private const int MaxItemCount = 6;

        public required string Name;
        public long ApId;
        public uint BaseAddr;
        public GeoItem[] Items;
        public int BuildingId;
        public int Multi = 0;

        private short buildingValue;
        //private short buildingCount;
        private uint placedCountAddr;
        private uint BuildingCountAddr;
        private Towns town;

        /// <summary>
        /// Pull values from the addresses above and set them locally to compare when checking against the server's values.
        /// </summary>
        internal void ReadValues()
        {
            buildingValue = Memory.ReadShort(BaseAddr);
            placedCountAddr = BaseAddr + sizeof(short);
            BuildingCountAddr = BaseAddr + sizeof(int);
            //buildingCount = Memory.ReadShort(BuildingCountAddr);

            //if (Multi > 0 && !collected) buildingCount = 0;

            //for (int i = 0; i < Items.Length; i++)
            //{
            //    uint itemAddr = BaseAddr + GeoAddrs.HouseInvOffset + (uint)(GeoAddrs.BldFieldDelta * i);
            //}
        }

        internal bool IsMemInit()
        {
            // TODO junk data might make this not evaluate to true?
            if (Memory.ReadShort(BuildingCountAddr) == 0)
            {
                return false;
            }

            return true;
        }

        internal void Init(int t, bool firstInit)
        {
            town = (Towns)t;

            if (firstInit)
            {
                // Some entries have junk data by default, we want to clear that out
                Memory.Write(placedCountAddr, (short)0);
                Memory.Write(BuildingCountAddr, (short)1);

                // We want to set all 6 item slots to zero to clear any junk values, rather than just the count of Items
                for (int i = ItemShortOffset; i < ItemShortOffset + MaxItemCount; i++)
                {
                    Memory.Write((uint)BaseAddr + (uint)(i * sizeof(short)), (short)0);
                }
            }
        }

        /// <summary>
        /// </summary>
        internal void GiveBuilding()
        {
            // Small chance of race condition giving us 2 items at the same time.  Double check the count before adding.
            if (buildingValue == CountThisBuilding()) return;

            string? msg = null;

            // Buildings with multiple copies, like river
            if (Multi != 0 && buildingValue < Multi)
            {
                buildingValue++;
                short buildingCount = (short)(Multiplier * buildingValue);
                Memory.Write(BuildingCountAddr, buildingCount);
                msg = "Received " + Name + ".";
            }
            // First piece, just enable the building
            else if (buildingValue == 0)
            {
                buildingValue = 1;
                msg = "Received " + Name + ".";
            }
            // Not first piece, add next item
            else if (buildingValue < Items.Length + 1)
            {
                if (buildingValue <= Items.Length)
                {
                    GeoItem item = Items[buildingValue - 1];
                    uint itemAddr = (uint)(BaseAddr + GeoAddrs.HouseInvOffset + (sizeof(short) * item.SlotId));

                    // If there isn't an item set in the item's slot, put it there.  Otherwise, add it to the player's inventory.
                    if (Memory.ReadShort(itemAddr) == 0)
                    {
                        Memory.Write(itemAddr, (short)1);
                    }
                    else
                    {
                        MemFuncs.GiveGeoItem(town, (short)item.ItemId);
                    }

                    buildingValue++;

                    msg = "Received " + item.Name + " for " + town.ToString() + ".";
                }
            }

            Memory.Write(BaseAddr, buildingValue);

            if (msg != null)
            {
                ItemQueue.AddMsg(msg);
                OpenMem.IncIndex();
            }
        }

        /// <summary>
        /// Checks how many of this item the player is supposed to have versus how many they actually have and adds items to the queue if missing.
        /// </summary>
        internal void CheckItems()
        {
            int count = CountThisBuilding();

            if (count > buildingValue)
            {
                for (int i = buildingValue; i < count; i++)
                {
                    ItemQueue.AddGeorama(this);
                }
            }
        }

        /// <summary>
        /// Counts how many of this building the player is supposed to have per the GameState.
        /// </summary>
        /// <returns></returns>
        private int CountThisBuilding()
        {
            int count = 0;
            for (int i = 0; i < App.Client.GameState.ReceivedItems.Count; i++)
            {
                if (App.Client.GameState.ReceivedItems[i].Id == ApId)
                {
                    count++;
                }
            }

            return count;
        }
    }
}
