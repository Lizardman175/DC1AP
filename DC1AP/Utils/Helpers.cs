using System.Reflection;

namespace DC1AP.Utils
{
    public static class Helpers
    {
        public static string GetAppVersion()
        {
            var assembly = Assembly.GetEntryAssembly();
            var attribute = assembly?.GetCustomAttribute<AssemblyInformationalVersionAttribute>();
            return attribute?.InformationalVersion ?? GetAssemblyVersion();
        }
        public static string GetAssemblyVersion()
        {
            var assemblyVersion = Assembly.GetEntryAssembly()?.GetName().Version?.ToString();

            return assemblyVersion ?? "Unknown";
        }
    }
}
