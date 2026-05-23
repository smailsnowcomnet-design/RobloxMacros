using System;
using System.IO;
using System.Text.Json;

namespace RobloxMacro.Services
{
    public record SettingsData
    (
        int HotkeyStartStopKey,
        int HotkeyRecordKey,
        string SaveFolderPath,
        int CameraWidth,
        int CameraHeight,
        int CameraDepth,
        bool IsAlwaysOnTop
    );

    public static class SettingsService
    {
        private static readonly string SettingsFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "RobloxMacro");
        private static readonly string SettingsFile = Path.Combine(SettingsFolder, "settings.json");

        public static SettingsData Load()
        {
            try
            {
                if (!Directory.Exists(SettingsFolder))
                    Directory.CreateDirectory(SettingsFolder);

                if (!File.Exists(SettingsFile))
                    return new SettingsData(0x76, 0x78, Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), 1920, 1080, 0, false);

                var json = File.ReadAllText(SettingsFile);
                var settings = JsonSerializer.Deserialize<SettingsData>(json);
                return settings ?? new SettingsData(0x76, 0x78, Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), 1920, 1080, 0, false);
            }
            catch
            {
                return new SettingsData(0x76, 0x78, Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), 1920, 1080, 0, false);
            }
        }

        public static void Save(SettingsData data)
        {
            if (!Directory.Exists(SettingsFolder))
                Directory.CreateDirectory(SettingsFolder);

            var json = JsonSerializer.Serialize(data, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(SettingsFile, json);
        }
    }
}
