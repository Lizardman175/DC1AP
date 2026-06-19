using Archipelago.Core.Util;
using DC1AP.Constants;
using DC1AP.Locations;
using DC1AP.Mem;
using System.Collections.Generic;
using System.Threading;

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
        private const short GarbageGeo = 0x21FA;  // Garbage value of 0x2222 - 0x0028, builtin offset for items vs atla loot table

        private static readonly object _lock = new();
        private static List<Atla>[] atlaMap = new List<Atla>[6];
        private static bool[] dungeonsMapped = [false, false, false, false, false, false];

        private static bool catPlaced = false;

        // Flag used to prevent re-running Startup() multiple times waiting for player to load game.
        private static bool playableState = true;

        /// <summary>
        /// Handle startup and the player reloading from memory by zeroing out various values and reinitializing data.
        /// </summary>
        internal static void Startup()
        {
            Clear();
            InitAtla();

            for (int i = 0; i < Options.Goal; i++)
            {
                int x = i;
                if (!dungeonsMapped[i])
                {
                    Memory.MonitorAddressForAction<int>(GeoAddrs.LastAltaPerDungeon[x], () => InitAtla(x), (o) => { return playableState && o != -1; });
                }
            }
        }

        private static void Clear()
        {
            atlaMap = new List<Atla>[6];
            dungeonsMapped = [false, false, false, false, false, false];
            catPlaced = false;
            playableState = false;
        }

        internal static void DoLoop(object? parameters)
        {
            Startup();

            while (true)
            {
                if (PlayerState.PlayerReady())
                {
                    if (!playableState) playableState = true;

                    CheckAtla();

                    if (Memory.ReadByte(MiscAddrs.InDungeonFlag) != 0xFF)
                    {
                        byte curDungeon = PlayerState.GetCurDungeon();
                        byte curFloor = Memory.ReadByte(MiscAddrs.CurFloor);

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
                            Memory.Write(GeoAddrs.AtlaCollectedFlag, 0);
                    }
                    else if (PlayerState.PlayerMovableInTown())
                    {
                        CharFuncs.CheckForChars();
                    }
                }

                Thread.Sleep(100);
            }
        }

        /// <summary>
        /// Check for state change of the atla in current dungeon and send to the server if something was collected
        /// </summary>
        private static void CheckAtla()
        {
            if (PlayerState.IsPlayerInDungeon())
            {
                byte curDun = PlayerState.GetCurDungeon();
                if (curDun < Options.Goal && atlaMap[curDun] != null)
                {
                    List<Atla> dunAtla = atlaMap[curDun];
                    foreach (Atla atla in dunAtla)
                    {
                        if (!atla.Collected && Memory.ReadInt(atla.Address) == MiscConstants.AtlaClaimed)
                        {
                            atla.Collected = true;

                            if (!App.Client.CurrentSession.Locations.AllLocationsChecked.Contains(atla.LocationId))
                                App.SendLocation(atla.LocationId);
                        }
                        else
                            CheckForCollectedAtla(atla);
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
                // Don't read the atla until the player enters the first dungeon since we can't initialize it.
                // Don't rerun or run if the game isn't in a valid state
                if (!PlayerState.GetGameState() || dungeonsMapped[dun] ||
                    (Memory.ReadInt(GeoAddrs.AtlaFlagAddrs[dun]) == MiscConstants.AtlaUnavailable && (!PlayerState.IsPlayerInDungeon() || PlayerState.GetCurDungeon() != dun))) return;

                // Only bother checking if not updated yet.
                if (!catPlaced && dun == 0)
                    catPlaced = Memory.ReadInt(GeoAddrs.CatlaAddr) == MiscConstants.AtlaUnavailable;

                MemFuncs.ClearAtlaTable(dun);

                uint addr = GeoAddrs.AtlaFlagAddrs[dun];
                // TODO magic nums.
                int atlaId = MiscConstants.BaseId + 101 + 1000 * (dun + 1);
                List<Atla> dunAtla = [];
                int maxFloor = MiscAddrs.FloorCountRear[dun];
                // Final dungeon floor counter is 1 less than it should be to not give early access to the Genie.  This is needed to check the final floor's atla.
                if (dun == (int)Towns.Castle) maxFloor++;

                for (int floor = 0; floor < maxFloor; floor++)
                {
                    // Adjust value for back half of a dungeon
                    if (dunAtla.Count == GeoAddrs.AtlaHalfwayCounts[dun] && dun != (int)Towns.Castle)
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
                                    App.SendLocation(atlaId);
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
                if (Memory.ReadByte(MiscAddrs.FloorCountAddrs[dun]) == 0 && Options.OpenDungeon && Options.Goal > dun)
                {
                    if (CharFuncs.HaveChar(dun))
                    {
                        byte floorCount = MiscAddrs.FloorCountRear[dun];
                        // Don't give free access to the boss room in Shipwreck
                        if (dun == (int)Towns.Queens)
                            floorCount--;
                        Memory.WriteByte(MiscAddrs.FloorCountAddrs[dun], floorCount);
                    }
                    else
                        Memory.WriteByte(MiscAddrs.FloorCountAddrs[dun], MiscAddrs.FloorCountFront[dun]);

                    if (dun == (int)Towns.Castle) EventMasks.CheckD6Flags();
                }

                atlaMap[dun] = dunAtla;
                dungeonsMapped[dun] = true;
            }
        }
    }
}
