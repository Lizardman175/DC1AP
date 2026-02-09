using Archipelago.Core.Util;
using DC1AP.Constants;
using DC1AP.Threads;
using DC1AP.Utils;
using System.Collections.Generic;
using System.Text.Json;

namespace DC1AP.Georama
{
    internal class GeoInvMgmt
    {
        private static readonly List<GeoBuilding[]> buildingFiles = [Resources.Embedded.Norune, Resources.Embedded.Matataki,
                                                                     Resources.Embedded.Queens, Resources.Embedded.Muska,
                                                                     Resources.Embedded.Factory, Resources.Embedded.Castle];

        /// <summary>
        /// Reads the .json for the building data
        /// </summary>
        internal static void Init()
        {
            JsonSerializerOptions jOptions = new(JsonSerializerDefaults.Web)
            {
                AllowOutOfOrderMetadataProperties = true,
                IncludeFields = true
            };

            for (int i = 0; i < Options.Goal; i++)
                GeoBuilding.buildings[i] = buildingFiles[i];
        }

        /// <summary>
        /// Read the memory for each building.  Need to rerun if the player loads a save file so we are synced with the game.
        /// </summary>
        internal static void InitBuildings()
        {
            for (int i = 0; i < Options.Goal; i++)
            {
                GeoBuilding[]? buildings = GeoBuilding.buildings[i];
                if (buildings == null) continue;

                Towns town = (Towns)i;

                foreach (GeoBuilding building in buildings)
                {
                    building.Init(town);
                    building.ReadValues();
                }
            }

            VerifyItems();
        }

        internal static bool GiveGeorama(long itemId)
        {
            bool added = false;

            for (int i = 0; i < Options.Goal; i++)
            {
                GeoBuilding[] list = GeoBuilding.buildings[i];
                if (list == null) continue;

                foreach (GeoBuilding building in list)
                {
                    if (building.ApId == itemId)
                    {
                        ItemQueue.AddGeorama(building);
                        added = true;
                        break;
                    }
                }

                if (added) break;
            }

            return added;
        }

        /// <summary>
        /// Verify the current item list against what the player has and add any missing buildings from the server.
        /// </summary>
        internal static void VerifyItems()
        {
            foreach (GeoBuilding[]? buildingList in GeoBuilding.buildings)
            {
                if (buildingList == null) continue;
                foreach (GeoBuilding building in buildingList)
                    building.CheckItems();
            }
        }

        internal static bool RemoveGeoItem(short itemId, int dungeon)
        {
            bool success = false;
            uint addr = GeoAddrs.TownGeoInv[dungeon];

            for (int i = 0; i < MiscConstants.GeoMaxItemCount; i++)
            {
                short id = Memory.ReadShort(addr);
                if (id == itemId)
                {
                    Memory.Write(addr, (short)-1);
                    success = true;
                    break;
                }
                addr += sizeof(short);
            }

            return success;
        }
    }
}
