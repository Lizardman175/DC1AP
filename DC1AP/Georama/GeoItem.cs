
namespace DC1AP.Georama
{
    internal class GeoItem
    {
        public required String Name;
        // Item ID in game to add to inventory if building pieces have been rearranged
        public int ItemId;
        // Slot index in the building 0-5
        public int SlotId;
    }
}
