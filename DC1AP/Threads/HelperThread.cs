using Archipelago.Core.Util;
using DC1AP.Constants;
using DC1AP.Mem;

namespace DC1AP.Threads
{
    /// <summary>
    /// More complex monitoring of memory/game state than Memory.Monitor methods
    /// </summary>
    internal class HelperThread
    {
        // Floors are 0 indexed
        private const int CatFloor = 7;
        private const int EmptyFloor1 = 3;
        private const int EmptyFloor2 = 10;
        private const short GarbageGeo = 0x21fa;  // Garbage value of 0x2222 - 0x0028, builtin offset for items vs atla loot table

        private static object _lock = new();
        private static List<Atla>[] atlaMap = new List<Atla>[6];
        private static bool[] dungeonsMapped = [false, false, false, false, false, false];

        private static bool runThread = false;
        private static bool catPlaced = false;

        // Flag used to prevent re-running Startup() multiple times waiting for player to load game.
        private static bool playableState = true;

        internal static bool RunThread { get => runThread; set => runThread = value; }

        /// <summary>
        /// Handle startup and the player reloading from memory by zeroing out various values and reinitializing data.
        /// </summary>
        private static void Startup()
        {
            atlaMap = new List<Atla>[6];
            dungeonsMapped = [false, false, false, false, false, false];
            catPlaced = false;
            playableState = false;

            InitAtla();

            // Can't use a loop here as InitAtla will only reference the loop counter's final value: Options.Goal
            // TODO there may be a way to make this loop?
            if (!dungeonsMapped[0])
            {
                Memory.MonitorAddressForAction<byte>(GeoAddrs.AtlaFlagAddrs[0], () => InitAtla(0), (o) => { return playableState && o != 0xff; });
            }
            if (!dungeonsMapped[1])
            {
                Memory.MonitorAddressForAction<byte>(GeoAddrs.AtlaFlagAddrs[1], () => InitAtla(1), (o) => { return playableState && o != 0xff; });
            }
            if (Options.Goal > 2 && !dungeonsMapped[2])
            {
                Memory.MonitorAddressForAction<byte>(GeoAddrs.AtlaFlagAddrs[2], () => InitAtla(2), (o) => { return playableState && o != 0xff; });
            }
            if (Options.Goal > 3 && !dungeonsMapped[3])
            {
                Memory.MonitorAddressForAction<byte>(GeoAddrs.AtlaFlagAddrs[3], () => InitAtla(3), (o) => { return playableState && o != 0xff; });
            }
            if (Options.Goal > 4 && !dungeonsMapped[4])
            {
                Memory.MonitorAddressForAction<byte>(GeoAddrs.AtlaFlagAddrs[4], () => InitAtla(4), (o) => { return playableState && o != 0xff; });
            }
            if (Options.Goal > 5 && !dungeonsMapped[5])
            {
                Memory.MonitorAddressForAction<byte>(GeoAddrs.AtlaFlagAddrs[5], () => InitAtla(5), (o) => { return playableState && o != 0xff; });
            }
        }

        internal static void DoLoop(object? parameters)
        {
            runThread = true;

            // TODO not sure if these are useful yet.  If so, move them out of the method and add reset to Startup()
            int mostRecentFloor = -1;
            int mostRecentDungeon = -1;
            bool isBackFloor = false;

            Startup();

            while (runThread)
            {
                bool playerReady = PlayerState.PlayerReady();

                // Handle player resets
                if (playableState && !playerReady)
                {
                    Startup();
                }
                else if (playerReady)
                {
                    if (!playableState) playableState = true;

                    CheckAtla();

                    /*
                     * Dungeon checks TODO:
                     *  Atla collected: keep a map and compare against it. If an atla gets collected, send it out.  If a reload happens and the atla is already collected, clear it.
                     *  Adjust atla height based on item quality
                     */
                    if (Memory.ReadByte(MiscAddrs.InDungeonFlag) != 0xFF)
                    {
                        byte curDungeon = Memory.ReadByte(MiscAddrs.CurDungeon);
                        byte curFloor = Memory.ReadByte(MiscAddrs.CurFloor);
                        bool curBackFloor = Memory.ReadByte(MiscAddrs.BackFloorFlag) != 0;

                        // Clear out junk georama pieces if collected
                        // TODO could probably be a reaction to the player looting atla?
                        if (curDungeon < Options.Goal)
                        {
                            uint addr = GeoAddrs.TownGeoInv[curDungeon];

                            for (int j = 0; j < MiscConstants.GeoMaxItemCount; j++)
                            {
                                if (Memory.ReadShort(addr) == GarbageGeo) Memory.Write(addr, (short)-1);
                                addr += sizeof(short);
                            }
                        }

                        // Hide the stray cat atla if present.  There is special code around it in game so we can't use it.
                        if (curDungeon == 0 && curFloor == CatFloor && Memory.ReadInt(GeoAddrs.AtlaCollectedFlag) != 0)
                        {
                            Memory.Write(GeoAddrs.AtlaCollectedFlag, 0);
                        }

                        if (isBackFloor != curBackFloor || curFloor != mostRecentFloor)
                        {
                            Enemies.MultiplyABS();
                        }

                        mostRecentDungeon = curDungeon;
                        mostRecentFloor = curFloor;
                        isBackFloor = curBackFloor;
                    }
                    // Reset dungeon values if player is not in a dungeon.
                    else
                    {
                        mostRecentFloor = -1;
                        mostRecentDungeon = -1;
                        isBackFloor = false;
                    }
                }

                Thread.Sleep(500);
            }
        }

        /// <summary>
        /// Check for state change of the atla in current dungeon and send to the server if something was collected
        /// </summary>
        private static void CheckAtla()
        {
            if (PlayerState.IsPlayerInDungeon())
            {
                byte curDun = Memory.ReadByte(MiscAddrs.CurDungeon);
                if (curDun < Options.Goal && atlaMap[curDun] != null)
                {
                    // TODO this doesn't have empty entries for each floor yet
                    List<Atla> dunAtla = atlaMap[curDun];
                    foreach (Atla atla in dunAtla)
                    {
                        if (!atla.Collected && Memory.ReadInt(atla.Address) == MiscConstants.AtlaClaimed)
                        {
                            atla.Collected = true;
                            App.SendLocation(atla.LocationId);
                        }
                        else
                        {
                            CheckForCollectedAtla(atla);
                        }
                    }
                }
            }
            // Check for collected atla if we are outside of a dungeon
            else
            {
                for (int i = 0; i < Options.Goal; i++)
                {
                    List<Atla> dunAtla = atlaMap[i];
                    if (dunAtla != null)
                    {
                        foreach (Atla atla in dunAtla)
                        {
                            CheckForCollectedAtla(atla);
                        }
                    }
                }
            }
        }

        private static void CheckForCollectedAtla(Atla atla)
        {
            if (!atla.Collected && App.Client.CurrentSession.Locations.AllLocationsChecked.Contains(atla.LocationId))
            {
                atla.Collected = true;
                Memory.Write(atla.Address, MiscConstants.AtlaClaimed);
            }
        }

        /// <summary>
        /// </summary>
        private static void InitAtla()
        {
            for (int dun = 0; dun < Options.Goal; dun++)
            {
                InitAtla(dun);
            }
        }

        private static void InitAtla(int dun)
        {
            lock (_lock)
            {
                // Brief sleep to allow the game to initialize the dungeon
                Thread.Sleep(500);

                // Skip current dungeon if already mapped out
                // Don't read the atla until the player enters the first dungeon since we can't initialize it.
                // Don't rerun or run if the game isn't in a valid state
                if (dungeonsMapped[dun] || Memory.ReadInt(GeoAddrs.AtlaFlagAddrs[dun]) == MiscConstants.AtlaUnavailable || !PlayerState.ValidGameState) return;

                // Only bother checking if not updated yet.
                if (!catPlaced && dun == 0)
                    catPlaced = Memory.ReadInt(GeoAddrs.CatlaAddr) == MiscConstants.AtlaUnavailable;

                MemFuncs.ClearAtlaTable(dun);

                uint addr = GeoAddrs.AtlaFlagAddrs[dun];
                // TODO magic nums.  Atla values are likely to change soon, so not bothering yet.
                int atlaId = MiscConstants.BaseId + 101 + 1000 * (dun + 1);
                List<Atla> dunAtla = [];

                // TODO D6: make sure this behaves
                for (int floor = 0; floor < MiscAddrs.FloorCountRear[dun]; floor++)
                {
                    // Adjust value for back half of a dungeon
                    if (dunAtla.Count == GeoAddrs.AtlaHalfwayCounts[dun])
                        atlaId = MiscConstants.BaseId + 201 + 1000 * (dun + 1);

                    for (int slot = 0; slot < MiscConstants.MaxAtlaPerFloor; slot++)
                    {
                        int atlaValue = Memory.ReadInt(addr);
                        if (atlaValue != MiscConstants.AtlaUnavailable)
                        {
                            Atla newAtla;
                            newAtla = new Atla(addr, dun, atlaId);
                            if (atlaValue == MiscConstants.AtlaClaimed)
                            {
                                newAtla.Collected = true;
                                if (!App.Client.CurrentSession.Locations.AllLocationsChecked.Contains(atlaId))
                                {
                                    App.SendLocation(atlaId);
                                }
                            }

                            dunAtla.Add(newAtla);

                            atlaId++;
                        }
                        // Place the stray cat's atla in the first available slot and remove it from floor 8. 3rd floor can't have atla for first dun
                        // The floor 8 atla has special programming we need to work around as its presences is based on the player having/not having the stray cat
                        else if (!catPlaced && dun == 0 && floor != EmptyFloor1 && floor != EmptyFloor2)
                        {
                            Atla newAtla = new Atla(addr, dun, atlaId);
                            dunAtla.Add(newAtla);

                            Memory.Write(addr, MiscConstants.AtlaAvailable);
                            Memory.Write(GeoAddrs.CatlaAddr, MiscConstants.AtlaUnavailable);
                            catPlaced = true;
                            atlaId++;
                        }
                        addr += sizeof(int);
                    }
                }

                // Set floor count based on dungeon flag/char recruits
                // TODO should CharFuncs.cs do this instead? Probably not for front floor counts?
                // TODO loop once char bools are in an array
                if (Memory.ReadByte(MiscAddrs.FloorCountAddrs[dun]) == 0 && Options.OpenDungeon)
                {
                    if (dun == (int)Towns.Norune)
                    {
                        if (CharFuncs.Xiao)
                            Memory.WriteByte(MiscAddrs.FloorCountAddrs[dun], MiscAddrs.FloorCountRear[dun]);
                        else
                            Memory.WriteByte(MiscAddrs.FloorCountAddrs[dun], MiscAddrs.FloorCountFront[dun]);
                    }
                    else if (dun == (int)Towns.Matataki)
                    {
                        if (CharFuncs.Goro)
                            Memory.WriteByte(MiscAddrs.FloorCountAddrs[dun], MiscAddrs.FloorCountRear[dun]);
                        else
                            Memory.WriteByte(MiscAddrs.FloorCountAddrs[dun], MiscAddrs.FloorCountFront[dun]);
                    }
                    else if (Options.Goal > 2 && dun == (int)Towns.Queens)
                    {
                        if (CharFuncs.Ruby)
                            Memory.WriteByte(MiscAddrs.FloorCountAddrs[dun], MiscAddrs.FloorCountRear[dun]);
                        else
                            Memory.WriteByte(MiscAddrs.FloorCountAddrs[dun], MiscAddrs.FloorCountFront[dun]);
                    }
                    else if (Options.Goal > 3 && dun == (int)Towns.Muska)
                    {
                        if (CharFuncs.Ungaga)
                            Memory.WriteByte(MiscAddrs.FloorCountAddrs[dun], MiscAddrs.FloorCountRear[dun]);
                        else
                            Memory.WriteByte(MiscAddrs.FloorCountAddrs[dun], MiscAddrs.FloorCountFront[dun]);
                    }
                    else if (Options.Goal > 4 && dun == (int)Towns.Factory)
                    {
                        if (CharFuncs.Osmond)
                            Memory.WriteByte(MiscAddrs.FloorCountAddrs[dun], MiscAddrs.FloorCountRear[dun]);
                        else
                            Memory.WriteByte(MiscAddrs.FloorCountAddrs[dun], MiscAddrs.FloorCountFront[dun]);
                    }
                }

                atlaMap[dun] = dunAtla;
                dungeonsMapped[dun] = true;
            }
        }
    }
}
