
using DC1AP.Constants;
using System;
using System.Collections.Generic;

namespace DC1AP
{
    internal static class Options
    {
        private static int goal = 6;
        private static AttachMultConfig attachMultConfig = 0;
        private static float attachMultiplier = 1f;
        private static float absMultiplier = 2f;
        private static bool openDungeon = true;
        private static bool allBosses = false;
        private static bool starterWeapons = false;
        private static bool miracleSanity = false;
        private static AutobuildFlags autobuild = AutobuildFlags.Off;

        public static int Goal { get => goal; }
        public static AttachMultConfig AttachMultConfig { get => attachMultConfig; }
        public static float AttachMultiplier { get => attachMultiplier; }
        public static float AbsMultiplier { get => absMultiplier; }
        public static bool OpenDungeon { get => openDungeon; }
        public static bool AllBosses { get => allBosses; }
        public static bool StarterWeapons { get => starterWeapons; }
        public static bool MiracleSanity { get => miracleSanity; }
        internal static AutobuildFlags Autobuild { get => autobuild; }

        internal static void ParseOptions(Dictionary<string, object> options)
        {
#pragma warning disable CS8604 // Possible null reference argument.
            goal = Int32.Parse(options["goal"].ToString());  // What dungeon to randomize through
            attachMultConfig = (AttachMultConfig)Int32.Parse(options["attach_mult_config"].ToString());
            if (attachMultConfig > 0)
                attachMultiplier = (Int32.Parse(options["attach_multiplier"].ToString()) + 1.0f) / 2;
            absMultiplier = (Int32.Parse(options["abs_multiplier"].ToString()) + 1.0f) / 2;
            allBosses = options["all_bosses"].ToString() != "0";  // All bosses up to dungeon goal
            openDungeon = options["open_dungeon"].ToString() != "0";  // All floors logically accessible will be unlocked
            starterWeapons = options["starter_weapons"].ToString() != "0";  // All floors logically accessible will be unlocked
            autobuild = (AutobuildFlags)Int32.Parse(options["auto_build"].ToString());
            miracleSanity = options["miracle_sanity"].ToString() != "0";  // Shuffle in miracle chests
#pragma warning restore CS8604 // Possible null reference argument.
        }
    }
}
