
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
        private static bool deathLink = false;
        private static string toanName = "Toan";
        private static string xiaoName = "Xiao";
        private static string goroName = "Goro";
        private static string rubyName = "Ruby";
        private static string ungagaName = "Ungaga";
        private static string osmondName = "Osmond";

        public static int Goal { get => goal; }
        public static AttachMultConfig AttachMultConfig { get => attachMultConfig; }
        public static float AttachMultiplier { get => attachMultiplier; }
        public static float AbsMultiplier { get => absMultiplier; }
        public static bool OpenDungeon { get => openDungeon; }
        public static bool AllBosses { get => allBosses; }
        public static bool StarterWeapons { get => starterWeapons; }
        public static bool MiracleSanity { get => miracleSanity; }
        internal static AutobuildFlags Autobuild { get => autobuild; }
        internal static bool DeathLink { get => deathLink; }
        internal static string ToanName { get => toanName; }
        internal static string XiaoName { get => xiaoName; }
        internal static string GoroName { get => goroName; }
        internal static string RubyName { get => rubyName; }
        internal static string UngagaName { get => ungagaName; }
        internal static string OsmondName { get => osmondName; }

        internal static void ParseOptions(Dictionary<string, object> options)
        {
#pragma warning disable CS8604 // Possible null reference argument.
#pragma warning disable CS8601 // Possible null reference.
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
            deathLink = options["death_link"].ToString() != "0";
            toanName = options["toan_name"].ToString();
            xiaoName = options["xiao_name"].ToString();
            goroName = options["goro_name"].ToString();
            rubyName = options["ruby_name"].ToString();
            ungagaName = options["ungaga_name"].ToString();
            osmondName = options["osmond_name"].ToString();
#pragma warning restore CS8601 // Possible null reference.
#pragma warning restore CS8604 // Possible null reference argument.
        }
    }
}
