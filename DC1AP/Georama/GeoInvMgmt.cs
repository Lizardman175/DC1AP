using DC1AP.Threads;
using Serilog;
using System.Text.Json;

namespace DC1AP.Georama
{
    internal class GeoInvMgmt
    {
        private static readonly List<string> buildingFiles = ["NoruneBuildings.json", "MatatakiBuildings.json", "QueensBuildings.json",
                                                              "MuskaBuildings.json",  "FactoryBuildings.json",  "CastleBuildings.json"];
        private static readonly List<GeoBuilding[]> buildings = [];

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
            {
                string filename = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Georama", buildingFiles[i]);
                string json = File.ReadAllText(filename);
                GeoBuilding[]? jsonBuildings = JsonSerializer.Deserialize<GeoBuilding[]>(json, jOptions);

                if (jsonBuildings != null) buildings.Add(jsonBuildings);
                else Log.Logger.Error("Failed to read " + buildingFiles[i]);
            }
        }

        /// <summary>
        /// Read the memory for each building.  Need to rerun if the player loads a save file so we are synced with the game.
        /// </summary>
        internal static void InitBuildings()
        {
            for (int i = 0; i < buildings.Count; i++)
            {
                foreach (GeoBuilding building in buildings[i])
                {
                    if (!building.HasBuilding()) building.Init();
                    building.ReadValues(i);
                }
            }

            VerifyItems();
        }

        internal static bool GiveItem(long itemId)
        {
            bool added = false;

            for (int i = 0; i < buildings.Count; i++)
            {
                GeoBuilding[] list = buildings[i];

                foreach (GeoBuilding building in list)
                {
                    // TODO test if the player has the item in question first in the event of syncing with the server.
                    if (building.ApId == itemId)
                    {
                        ItemQueue.AddGeorama(building);
                        added = true;
                    }

                    if (added) break;
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
            foreach (GeoBuilding[] buildingList in buildings)
                foreach (GeoBuilding building in buildingList)
                    building.CheckItems();
        }
    }
}
