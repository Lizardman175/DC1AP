
namespace DC1AP.Georama
{
    public class BuildingCoords
    {
        public short Orientation = 0;

        // These are actually floats, but we're just stuffing raw byte values into the memory.
        public uint X;
        public float Y;
        public uint Z;
    }
}
