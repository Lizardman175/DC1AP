using DC1AP.Georama;
using DC1AP.Items;
using System.Collections.Concurrent;
using System.IO;
using System.Reflection;
using System.Text;
using System.Text.Json;

namespace DC1AP.Utils
{
    internal class Resources
    {
        public static class Embedded
        {
            static JsonSerializerOptions jOptions = new(JsonSerializerDefaults.Web)
            {
                AllowOutOfOrderMetadataProperties = true,
                IncludeFields = true
            };

            public static ConcurrentDictionary<long, InvItem> Items
            {
                get
                {
                    var info = Assembly.GetExecutingAssembly().GetName();
                    var name = info.Name;
                    using var stream = Assembly
                        .GetExecutingAssembly()
                        .GetManifestResourceStream($"{name}.Items.Items.json")!;
                    using var streamReader = new StreamReader(stream, Encoding.UTF8);
                    return JsonSerializer.Deserialize<ConcurrentDictionary<long, InvItem>>(streamReader.ReadToEnd(), jOptions);
                }
            }

            public static ConcurrentDictionary<long, Attachment> Attachments
            {
                get
                {
                    var info = Assembly.GetExecutingAssembly().GetName();
                    var name = info.Name;
                    using var stream = Assembly
                        .GetExecutingAssembly()
                        .GetManifestResourceStream($"{name}.Items.Attachments.json")!;
                    using var streamReader = new StreamReader(stream, Encoding.UTF8);
                    return JsonSerializer.Deserialize<ConcurrentDictionary<long, Attachment>>(streamReader.ReadToEnd(), jOptions);
                }
            }

            public static GeoBuilding[] Norune
            {
                get
                {
                    var info = Assembly.GetExecutingAssembly().GetName();
                    var name = info.Name;
                    using var stream = Assembly
                        .GetExecutingAssembly()
                        .GetManifestResourceStream($"{name}.Georama.NoruneBuildings.json")!;
                    using var streamReader = new StreamReader(stream, Encoding.UTF8);
                    return JsonSerializer.Deserialize<GeoBuilding[]>(streamReader.ReadToEnd(), jOptions);
                }
            }

            public static GeoBuilding[] Matataki
            {
                get
                {
                    var info = Assembly.GetExecutingAssembly().GetName();
                    var name = info.Name;
                    using var stream = Assembly
                        .GetExecutingAssembly()
                        .GetManifestResourceStream($"{name}.Georama.MatatakiBuildings.json")!;
                    using var streamReader = new StreamReader(stream, Encoding.UTF8);
                    return JsonSerializer.Deserialize<GeoBuilding[]>(streamReader.ReadToEnd(), jOptions);
                }
            }

            public static GeoBuilding[] Queens
            {
                get
                {
                    var info = Assembly.GetExecutingAssembly().GetName();
                    var name = info.Name;
                    using var stream = Assembly
                        .GetExecutingAssembly()
                        .GetManifestResourceStream($"{name}.Georama.QueensBuildings.json")!;
                    using var streamReader = new StreamReader(stream, Encoding.UTF8);
                    return JsonSerializer.Deserialize<GeoBuilding[]>(streamReader.ReadToEnd(), jOptions);
                }
            }

            public static GeoBuilding[] Muska
            {
                get
                {
                    var info = Assembly.GetExecutingAssembly().GetName();
                    var name = info.Name;
                    using var stream = Assembly
                        .GetExecutingAssembly()
                        .GetManifestResourceStream($"{name}.Georama.MuskaBuildings.json")!;
                    using var streamReader = new StreamReader(stream, Encoding.UTF8);
                    return JsonSerializer.Deserialize<GeoBuilding[]>(streamReader.ReadToEnd(), jOptions);
                }
            }

            public static GeoBuilding[] Factory
            {
                get
                {
                    var info = Assembly.GetExecutingAssembly().GetName();
                    var name = info.Name;
                    using var stream = Assembly
                        .GetExecutingAssembly()
                        .GetManifestResourceStream($"{name}.Georama.FactoryBuildings.json")!;
                    using var streamReader = new StreamReader(stream, Encoding.UTF8);
                    return JsonSerializer.Deserialize<GeoBuilding[]>(streamReader.ReadToEnd(), jOptions);
                }
            }

            public static GeoBuilding[] Castle
            {
                get
                {
                    var info = Assembly.GetExecutingAssembly().GetName();
                    var name = info.Name;
                    using var stream = Assembly
                        .GetExecutingAssembly()
                        .GetManifestResourceStream($"{name}.Georama.CastleBuildings.json")!;
                    using var streamReader = new StreamReader(stream, Encoding.UTF8);
                    return JsonSerializer.Deserialize<GeoBuilding[]>(streamReader.ReadToEnd(), jOptions);
               }
            }

            public static string MiracleChests
            {
                get
                {
                    var info = Assembly.GetExecutingAssembly().GetName();
                    var name = info.Name;
                    using var stream = Assembly
                        .GetExecutingAssembly()
                        .GetManifestResourceStream($"{name}.Items.miracle_locations.csv")!;
                    using var streamReader = new StreamReader(stream, Encoding.UTF8);
                    return streamReader.ReadToEnd();
                }
            }
        }
    }
}
