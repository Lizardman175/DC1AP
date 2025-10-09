
namespace DC1AP
{
    internal static class Options
    {
        private static int goal = 3;
        private static float absMultiplier = 2f;
        private static bool openDungeon = true;
        private static bool allBosses = false;
        private static bool starterWeapons = false;
        //private static bool givePockets = false;
        private static bool miracleSanity = false;

        public static int Goal { get => goal; }
        public static float AbsMultiplier { get => absMultiplier; }
        public static bool OpenDungeon { get => openDungeon; }
        public static bool AllBosses { get => allBosses; }
        public static bool StarterWeapons { get => starterWeapons; }
        public static bool MiracleSanity { get => miracleSanity; }
        //public static bool GivePockets { get => givePockets; }

        // used to test various values without generating a new multiworld
        static Options()
        {
            //goal = 3;
            //openDungeon = true;
            //allBosses = true;
        }

        public static void ParseOptions(Dictionary<string, object> options)
        {
            goal = Int32.Parse(options["goal"].ToString());  // What dungeon to randomize through
            absMultiplier = (Int32.Parse(options["abs_multiplier"].ToString()) + 1.0f) / 2;
            allBosses = options["all_bosses"].ToString() != "0";  // All bosses up to dungeon goal
            openDungeon = options["open_dungeon"].ToString() != "0";  // All floors logically accessible will be unlocked
            //miracleSanity = options["miracle_chests"].ToString() != "0";  // Shuffle in miracle chests
            starterWeapons = options["starter_weapons"].ToString() != "0";  // All floors logically accessible will be unlocked
            //givePockets = options["give_pockets"].ToString() != "0";  // Give all available pockets at the start
        }
    }
}
