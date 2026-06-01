

namespace DC1AP.Georama
{
    public struct GridEntry
    {
        // TODO note for future: this isn't always the building ID; bridges are usually 6 here
        public uint BuildingId;
        // Seems unused?
        public uint int2 = 0;
        public uint int3 = 0;
        // Index into 376E70 table.  Not from json but determined on the fly
        public uint tableIndex = 0;
        // TODO Seen as 0x80 or 0x81.  May be 80 for large building, 81 for single piece? maybe 81 for dupe items like rivers?  Windmill is 80 so likely river/road/etc.
        // Roads are also only 80.  Hypothesis: river is 81 since it supports other buildings.  Need to examine earth a/b, bridges, watermills, and buildings on earth a/b.
        // Watermills are 81 as well.
        public uint eighty = 0x80;
        // Road is setting to 1, river to 2, bridge 3, pond/oasis 4, watermill 5.  Seems constant for the individual piece.  Buildings and are 0 here.
        // This lines up with buildings that have special handling for transformations based on being near other of the same part
        public int partsExtra;
        // Not sure we need to place this value?  Defaults to -1 and haven't yet seen it not -1
        public int minusOne = -1;

        public GridEntry()
        {
        }
    };
}
