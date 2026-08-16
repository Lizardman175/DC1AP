using DC1AP.Constants;
using System;
using System.Collections.Generic;
using System.Text.Json;

namespace DC1AP
{
    internal static class Options
    {
        private static int goal = 6;
        private static int memoryCount;
        private static AttachMultConfig attachMultConfig = 0;
        private static float attachMultiplier = 1f;
        private static float absMultiplier = 2f;
        private static bool openDungeon = true;
        private static bool allBosses = false;
        private static bool starterWeapons = false;
        private static bool miracleSanity = false;
        private static bool shopSanity = false;
        private static int fishSanity = 0;
        private static int floorSanity = 0;
        private static bool deathLink = false;
        private static AutobuildFlags autobuild = AutobuildFlags.Off;
        private static string toanName = "Toan";
        private static string xiaoName = "Xiao";
        private static string goroName = "Goro";
        private static string rubyName = "Ruby";
        private static string ungagaName = "Ungaga";
        private static string osmondName = "Osmond";
        private static List<List<int>>? atlaPerFloor = null;

        public static int Goal { get => goal; }
        public static int MemoryCount { get => memoryCount; }
        public static AttachMultConfig AttachMultConfig { get => attachMultConfig; }
        public static float AttachMultiplier { get => attachMultiplier; }
        public static float AbsMultiplier { get => absMultiplier; }
        public static bool OpenDungeon { get => openDungeon; }
        public static bool AllBosses { get => allBosses; }
        public static bool StarterWeapons { get => starterWeapons; }
        public static bool MiracleSanity { get => miracleSanity; }
        public static bool ShopSanity { get => shopSanity; }
        public static int FishSanity { get => fishSanity; }
        public static int FloorSanity { get => floorSanity; }
        public static AutobuildFlags Autobuild { get => autobuild; }
        public static bool DeathLink { get => deathLink; }
        public static string ToanName { get => toanName; }
        public static string XiaoName { get => xiaoName; }
        public static string GoroName { get => goroName; }
        public static string RubyName { get => rubyName; }
        public static string UngagaName { get => ungagaName; }
        public static string OsmondName { get => osmondName; }
        public static List<List<int>>? AtlaPerFloor { get => atlaPerFloor; }

        internal static void ParseOptions(Dictionary<string, object> options)
        {
            goal = ((JsonElement)options["goal"]).Deserialize<int>();  // What dungeon to randomize through
            memoryCount = ((JsonElement)options["memory_count"]).Deserialize<int>();
            attachMultConfig = (AttachMultConfig)((JsonElement)options["attach_mult_config"]).Deserialize<int>();
            if (attachMultConfig > 0)
                attachMultiplier = (((JsonElement)options["attach_multiplier"]).Deserialize<int>() + 1.0f) / 2;
            absMultiplier = (((JsonElement)options["abs_multiplier"]).Deserialize<int>() + 1.0f) / 2;
            allBosses = ((JsonElement)options["all_bosses"]).Deserialize<int>() != 0;  // All bosses up to dungeon goal
            openDungeon = ((JsonElement)options["open_dungeon"]).Deserialize<int>() != 0;  // All floors logically accessible will be unlocked
            starterWeapons = ((JsonElement)options["starter_weapons"]).Deserialize<int>() != 0;  // All floors logically accessible will be unlocked
            autobuild = (AutobuildFlags)((JsonElement)options["auto_build"]).Deserialize<int>();
            miracleSanity = ((JsonElement)options["miracle_sanity"]).Deserialize<int>() != 0;  // Shuffle in miracle chests
            shopSanity = ((JsonElement)options["shop_sanity"]).Deserialize<int>() != 0;
            fishSanity = ((JsonElement)options["fish_sanity"]).Deserialize<int>();
            floorSanity = ((JsonElement)options["floor_sanity"]).Deserialize<int>();
            deathLink = ((JsonElement)options["death_link"]).Deserialize<int>() != 0;
            toanName = ((JsonElement)options["toan_name"]).Deserialize<String>()!;
            xiaoName = ((JsonElement)options["xiao_name"]).Deserialize<String>()!;
            goroName = ((JsonElement)options["goro_name"]).Deserialize<String>()!;
            rubyName = ((JsonElement)options["ruby_name"]).Deserialize<String>()!;
            ungagaName = ((JsonElement)options["ungaga_name"]).Deserialize<String>()!;
            osmondName = ((JsonElement)options["osmond_name"]).Deserialize<String>()!;
            // TODO use this.  Still wrapped in if in case we remove it
            if (options.TryGetValue("apf", out object? value))
            {
                if (value != null)
                {
                    atlaPerFloor = ((JsonElement)value).Deserialize<List<List<int>>>();
                }
            }
        }
    }
}
