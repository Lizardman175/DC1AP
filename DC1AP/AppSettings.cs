using Serilog;
using System;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace DC1AP
{
    public class AppSettings
    {
        private const string Filename = "dc1_settings.json";
        private static readonly JsonSerializerOptions options = new() { WriteIndented = true };

        public AppSettings() { }

        public AppSettings(string host, string slot)
        {
            Host = host;
            Slot = slot;
        }

        [JsonInclude]
        public string Host { get; set; } = String.Empty;
        [JsonInclude]
        public string Slot { get; set; } = String.Empty;

        public static AppSettings LoadAppSettings()
        {
            if (!File.Exists(Filename))
                return new AppSettings();

            string json = File.ReadAllText(Filename);
            using var streamReader = new StreamReader(File.OpenRead(Filename), Encoding.UTF8);
            return JsonSerializer.Deserialize<AppSettings>(streamReader.ReadToEnd()) ?? new AppSettings();
        }

        public static void SaveAppSettings(AppSettings settings)
        {
            string json = JsonSerializer.Serialize(settings);
            try
            {
                File.WriteAllText(Filename, json);
            }
            catch (Exception)
            {
                Log.Error("Failed to save connection info to file");
            }
        }
    }
}
