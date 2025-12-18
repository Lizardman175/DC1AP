using Archipelago.Core.Util;
using DC1AP.Constants;
using DC1AP.Mem;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;


namespace DC1AP.Items
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
        private const uint TownChestDataAddr = 0x003C6BD0;
        private const uint InteriorChestDataAddr = 0x003D2710;
        private const uint ObjetTypeOffset = 0x10;
        private const uint ChestItemIdOffset = 0x1C;
        private const uint InteractableOffset = 0x90;

        // Item displayed during prickly cutscene.  Changes with zone change.  Need to account for when entering mayor's house.
        private const uint PricklyDisplayAddr = 0x00415148;
        // Determines item received from Mayor's closet
        private const uint PricklyValueAddr = 0x004156D4;

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
                        Byte.Parse(split[6].Remove(0, 2), System.Globalization.NumberStyles.HexNumber));
                    // Only track chests the player hasn't opened yet.
                    if (!mc.CheckChest(false))
                        chests[townInt].Add(mc);
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

        internal static void DoLoop(object? parameters)
        {
            if (!Options.MiracleSanity)
                return;

            int currZoneId = -1;
            int currInteriorId = -1;
            int currAltInteriorId = -1;

            while (PlayerState.ValidGameState)
            {
                Thread.Sleep(50);

                int zoneId = Memory.ReadInt(MiscAddrs.CurZoneAddr);

                if (PlayerState.IsPlayerInTown() && PlayerState.PlayerMovableTown())
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
                                EmptyMiracleChests(TownChestDataAddr);
                        }
                        // Small case when connecting if the player is in town the interior ID will still be the last interior value
                        if (inInterior && (interiorId != currInteriorId || altInteriorId != currAltInteriorId))
                        {
                            currInteriorId = interiorId;
                            currAltInteriorId = altInteriorId;

                            // Mayor's House
                            if (currZoneId == 0 && currInteriorId == 255)
                                EmptyMiracleChests(InteriorChestDataAddr, mayor: true);
                            // Bunbuku's House
                            else if (zoneId == 1 && interiorId == 2)
                                EmptyMiracleChests(InteriorChestDataAddr, bunbuku: true);
                            else
                                EmptyMiracleChests(InteriorChestDataAddr);
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
                        MiracleChestMgmt.CheckTown(Towns.Norune);
                    else if (zoneId == MiscAddrs.MatatakiZone || zoneId == MiscAddrs.GoroZone || zoneId == MiscAddrs.TreeZone)
                        MiracleChestMgmt.CheckTown(Towns.Matataki);
                    else if (zoneId == MiscAddrs.QueensZone || zoneId == MiscAddrs.QueensDockZone)
                        MiracleChestMgmt.CheckTown(Towns.Queens);
                    else if (zoneId == MiscAddrs.MuskaZone || zoneId == MiscAddrs.SMTExtZone)
                        MiracleChestMgmt.CheckTown(Towns.Muska);
                    else if (zoneId == MiscAddrs.YellowDropsZone || zoneId == MiscAddrs.FactoryZone)
                        MiracleChestMgmt.CheckTown(Towns.Factory);
                }
            }
        }

        private static void EmptyMiracleChests(uint addr, bool mayor = false, bool bunbuku = false)
        {
            // Some of this area is apparently protected so I can't read a full struct of data at once. We don't need most of it anyway.
            int chestFlag = Memory.ReadInt(addr);
            int skipCount = 0;
            while (chestFlag > 0)
            {
                int objectType = Memory.ReadInt(addr + ObjetTypeOffset);
                int itemId = Memory.ReadInt(addr + ChestItemIdOffset);
                if (objectType == 2)
                {
                    if (bunbuku && skipCount < 2)
                        skipCount++;
                    else
                        Memory.Write(addr + ChestItemIdOffset, -1);
                }
                addr += InteractableOffset;
                chestFlag = Memory.ReadInt(addr);
            }

            if (mayor)
            {
                Memory.Write(PricklyDisplayAddr, -1);
                Memory.Write(PricklyValueAddr, -1);
            }
        }


    }
}
