using Archipelago.Core.Util;
using DC1AP.Constants;
using DC1AP.Locations;
using DC1AP.Mem;
using DC1AP.Utils;
using System;
using System.Collections.Generic;
using System.Threading;


namespace DC1AP.Threads
{
    internal class MiracleChestMgmt
    {
        /*
         * 2 tables exist: current town and current interior.  Most zones use the town one and interior is used for geo buildings, Goro's house, and Rando's house
         * Interior table is also used for all of Yellow Drops outside of the factory I think
         * Edge cases:
         *  2 chests in Bunbuku's house are not shuffled so we need to not clear those values so the player can get the vanilla items.
         *  Mayor's pet prickly: special area of memory. Probably event data.
         *  Sundew chest flag
         * 
         * Thread watches if player is in town.
         *  If in town, determine which and zero out MCs.  Can either map out the data or determine if a piece is an MC on the fly by 
         *  checking if the first field is set and the one below it is 2.
         */

        private static List<List<MiracleChest>> chests = [[], [], [], [], []];

        /// <summary>
        /// Read in the Miracle Chest data.
        /// </summary>
        internal static void Init()
        {
            if (Options.MiracleSanity)
            {
                chests = [[], [], [], [], []];
                string[] lines = Resources.Embedded.MiracleChests.Split('\n');

                foreach (string line in lines)
                {
                    string[] split = line.Split(',');
                    int townInt = Int32.Parse(split[2]);

                    MiracleChest mc = new(long.Parse(split[1]),
                        uint.Parse(split[5].Remove(0, 2), System.Globalization.NumberStyles.HexNumber),
                        Byte.Parse(split[6].Remove(0, 2), System.Globalization.NumberStyles.HexNumber));
                    // Only track chests the player hasn't opened yet.
                    if (!mc.CheckChest())
                        chests[townInt].Add(mc);
                }
            }
        }

        internal static void DoLoop(object? parameters)
        {
            if (!Options.MiracleSanity)
                return;

            int currZoneId = -1;
            int currInteriorId = -1;
            int currAltInteriorId = -1;

            while (PlayerState.GetGameState())
            {
                int zoneId = Memory.ReadInt(MiscAddrs.CurZoneAddr);

                if (PlayerState.PlayerMovableInTown())
                {
                    int interiorId = Memory.ReadByte(MiscAddrs.InteriorIdAddr);
                    int altInteriorId = Memory.ReadByte(MiscAddrs.AlternateIntIdAddr);
                    bool inInterior = Memory.ReadByte(MiscAddrs.InInteriorFlagAddr) != 0;

                    if ((zoneId <= (int)Towns.Castle && zoneId < Options.Goal) || zoneId > (int)Towns.Castle)
                    {
                        if (zoneId != currZoneId)
                        {
                            currZoneId = zoneId;
                            if (zoneId != MiscAddrs.DeadTreeZone)
                                EmptyMiracleChests(ItemValues.TownChestDataAddr);
                        }
                        // Small case when connecting if the player is in town the interior ID will still be the last interior value
                        if (inInterior && (interiorId != currInteriorId || altInteriorId != currAltInteriorId))
                        {
                            currInteriorId = interiorId;
                            currAltInteriorId = altInteriorId;

                            // Mayor's House
                            if (currZoneId == 0 && currInteriorId == 255)
                                EmptyMiracleChests(ItemValues.InteriorChestDataAddr, mayor: true);
                            // Bunbuku's House
                            else if (zoneId == 1 && interiorId == 2)
                                EmptyMiracleChests(ItemValues.InteriorChestDataAddr, bunbuku: true);
                            else
                                EmptyMiracleChests(ItemValues.InteriorChestDataAddr);
                        }
                    }
                }
                // If in a dungeon, reset the flags.
                else if (currZoneId != -1)
                {
                    currZoneId = -1;
                    currInteriorId = -1;
                    currAltInteriorId = -1;
                }

                if (PlayerState.IsPlayerInTown())
                {
                    if (zoneId == MiscAddrs.NoruneZone)
                        CheckTown(Towns.Norune);
                    else if (zoneId == MiscAddrs.MatatakiZone || zoneId == MiscAddrs.GoroZone || zoneId == MiscAddrs.TreeZone)
                        CheckTown(Towns.Matataki);
                    else if (zoneId == MiscAddrs.QueensZone || zoneId == MiscAddrs.QueensDockZone)
                        CheckTown(Towns.Queens);
                    else if (zoneId == MiscAddrs.MuskaZone || zoneId == MiscAddrs.SMTExtZone)
                        CheckTown(Towns.Muska);
                    else if (zoneId == MiscAddrs.YellowDropsZone || zoneId == MiscAddrs.FactoryZone)
                        CheckTown(Towns.Factory);
                }

                Thread.Sleep(50);
            }
        }

        /// <summary>
        /// Checks all MCs for the given town.
        /// </summary>
        /// <param name="town"></param>
        private static void CheckTown(Towns town)
        {
            if (town < Towns.Castle && PlayerState.PlayerMovableTown())
                chests[(int)town].RemoveAll(mc => mc.CheckChest());
        }

        /// <summary>
        /// Empties loot table for currently loaded chests
        /// </summary>
        /// <param name="addr"></param>
        /// <param name="mayor"></param>
        /// <param name="bunbuku"></param>
        private static void EmptyMiracleChests(uint addr, bool mayor = false, bool bunbuku = false)
        {
            // Some of this area is apparently protected so I can't read a full struct of data at once. We don't need most of it anyway.
            int chestFlag = Memory.ReadInt(addr);
            int skipCount = 0;
            while (chestFlag > 0)
            {
                int objectType = Memory.ReadInt(addr + ItemValues.ObjetTypeOffset);
                int itemId = Memory.ReadInt(addr + ItemValues.ChestItemIdOffset);
                if (objectType == 2)
                {
                    if (bunbuku && skipCount < 2)
                        skipCount++;
                    else
                        Memory.Write(addr + ItemValues.ChestItemIdOffset, -1);
                }
                addr += ItemValues.InteractableOffset;
                chestFlag = Memory.ReadInt(addr);
            }

            if (mayor)
            {
                Memory.Write(ItemValues.PricklyDisplayAddr, -1);
                Memory.Write(ItemValues.PricklyValueAddr, -1);
            }
        }
    }
}
