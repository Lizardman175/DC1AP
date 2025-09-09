
namespace DC1AP.Mem
{
    internal class Atla(ulong addr, int dungeon, int locationId, bool collected = false)
    {
        private readonly ulong addr = addr;
        private readonly int dungeon = dungeon;
        private readonly int locationId = locationId;
        private bool collected = collected;
        //private readonly int floor = floor;
        //private readonly int itemId = itemId;

        internal ulong Address { get { return addr; } }
        internal int Dungeon { get { return dungeon; } }
        internal int LocationId { get { return locationId; } }

        internal bool Collected { get { return collected; } set { collected = value; } }
        //internal int Floor { get { return floor; } }
        //internal int ItemId { get { return itemId; } }
    }
}
