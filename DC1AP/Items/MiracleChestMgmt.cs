using DC1AP.Constants;
using System;
using System.Collections.Generic;
using System.IO;


namespace DC1AP.Items
{
    internal class MiracleChestMgmt
    {
        private static List<List<MiracleChest>> chests = [[], [], [], [], []];

        /// <summary>
        /// Read in the Miracle Chest data.
        /// </summary>
        internal static void Init()
        {
            if (Options.MiracleSanity)
            {
                chests = [[], [], [], [], []];
                StreamReader reader = new(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Items", "miracle_locations.csv"));
                string? line;

                while ((line = reader.ReadLine()) != null)
                {
                    string[] split = line.Split(',');
                    int townInt = Int32.Parse(split[2]);

                    MiracleChest mc = new(long.Parse(split[1]),
                        uint.Parse(split[5].Remove(0, 2), System.Globalization.NumberStyles.HexNumber),
                        Byte.Parse(split[6].Remove(0, 2), System.Globalization.NumberStyles.HexNumber),
                        split[7]);
                    // Only track chests the player hasn't opened yet.
                    if (!mc.CheckChest(false))
                        chests[townInt].Add(mc);
                }

                if (Options.SundewChest)
                {
                    MiracleChest sundewChest = new(971_112_075,
                        uint.Parse("01CE4887", System.Globalization.NumberStyles.HexNumber),
                        8, "Sundew");
                    if (!sundewChest.CheckChest(false))
                        chests[(int)Towns.Matataki].Add(sundewChest);
                }
            }
        }

        /// <summary>
        /// Checks all MCs for the given town.
        /// </summary>
        /// <param name="town"></param>
        internal static void CheckTown(Towns town)
        {
            if (town < Towns.Castle)
                chests[(int)town].RemoveAll(mc => mc.CheckChest(true));
        }
    }
}
