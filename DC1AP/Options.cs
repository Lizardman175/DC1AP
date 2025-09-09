
namespace DC1AP
{
    internal static class Options
    {
        private static int goal = 2;
        private static bool openDungeon = true;
        private static bool allBosses = false;
        //private static bool miracleSanity = false;

        public static int Goal { get => goal; }
        public static bool OpenDungeon { get => openDungeon; }
        public static bool AllBosses { get => allBosses; }
        //public static bool MiracleSanity { get => miracleSanity; }

        // used to test various values without generating a new rando
        static Options()
        {
            //goal = 2;
            //openDungeon = true;
            //allBosses = true;
        }

        public static void ParseOptions(Dictionary<string, object> options)
        {
            goal = Int32.Parse(options["goal"].ToString());  // What dungeon to randomize through
            allBosses = options["all_bosses"].ToString() != "0";  // All bosses up to dungeon goal
            openDungeon = options["open_dungeon"].ToString() != "0";  // All floors logically accessible will be unlocked
            //miracleSanity = options["miracle_chests"].ToString() != "0";  // Shuffle in miracle chests
        }
    }
}
