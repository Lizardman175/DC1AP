
namespace DC1AP.Georama
{
    public class BuildingCoords
    {
        public float X;
        public float Y;
        public float Z;

        public short Orientation = 0;
        public float OrientationFloat = 0;

        public uint TableIndex = 0;
        // Note: these are only the perimeter for things like Earth A that have both dimensions larger than 2 so it won't have x*z entries necessarily
        public uint[]? Addrs;
    }
}
