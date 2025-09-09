using DC1AP.Threads;
using Serilog;
using System.Text.Json;

namespace DC1AP.Georama
{
    internal class GeoInvMgmt
    {
        private static readonly List<String> buildingFiles = ["NoruneBuildings.json", "MatatakiBuildings.json", "QueensBuildings.json",
                                                              "MuskaBuildings.json",  "FactoryBuildings.json",  "CastleBuildings.json"];
        private static readonly List<GeoBuilding[]> buildings = [];

        public static void Init() 
        {
            ReadBuildingsJson();
            ReadBuildingsMem();
        }

        private static void ReadBuildingsJson()
        {
            for (int i = 0; i < Options.Goal; i++)
            {
                string filename = System.IO.Path.Combine(System.AppDomain.CurrentDomain.BaseDirectory, "Georama", buildingFiles[i]);
                string json = File.ReadAllText(filename);
                JsonSerializerOptions jOptions = new(JsonSerializerDefaults.Web);
                jOptions.AllowOutOfOrderMetadataProperties = true;
                jOptions.IncludeFields = true;
                GeoBuilding[]? jsonBuildings = JsonSerializer.Deserialize<GeoBuilding[]>(json, jOptions);

                if (jsonBuildings != null) buildings.Add(jsonBuildings);
                else Log.Logger.Error("Failed to read " + buildingFiles[i]);
            }
        }

        private static void ReadBuildingsMem()
        {
            for (int i = 0; i < buildings.Count; i++)
            {
                GeoBuilding[] bld = buildings[i];
                if (bld[i].IsMemInit())
                {
                    ReadBuildingMem(bld);
                }
                else
                {
                    // TODO watch for initialization, then call readBuildingMem
                    // This might be OBE
                }
            }
        }

        // TODO why is this called twice?
        private static void ReadBuildingMem(GeoBuilding[] buildings)
        {
            foreach (GeoBuilding building in buildings) building.ReadValues();
        }

        // TODO remove firstInit with progressive item update
        internal static void InitBuildings(bool firstInit)
        {
            for (int i = 0; i < Options.Goal; i++)
            {
                foreach (GeoBuilding building in buildings[i])
                {
                    building.Init(i, firstInit);
                }

                ReadBuildingMem(buildings[i]);
            }
        }

        public static bool GiveItem(long itemId)
        {
            bool added = false;

            // TODO this may be reducable; we'd need a map of ID to building/item or something.  For now, search building/item names
            // TODO part 2: this will change drastically when implementing progressive georama, so don't bother cleaning now.
            for (int i = 0; i < buildings.Count; i++)
            {
                GeoBuilding[] list = buildings[i];

                foreach (GeoBuilding building in list)
                {
                    // TODO test if the player has the item in question first in the event of syncing with the server.
                    if (building.ApIds.Contains(itemId))
                    {
                        ItemQueue.AddGeoBuilding(building, itemId);
                        added = true;
                    }
                    else
                    {
                        foreach (GeoItem item in building.GetItems())
                        {
                            if (item.ApId == itemId)
                            {
                                ItemQueue.AddGeoItem(item, (IDs.Towns)i);
                                added = true;
                                break;
                            }
                        }
                    }

                    if (added) break;
                }

                if (added) break;
            }

            return added;
        }
    }
}
