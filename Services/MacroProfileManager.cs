using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using RobloxMacro.Models;

namespace RobloxMacro.Services
{
    /// <summary>
    /// Сервис для управления сохраненными профилями макросов
    /// </summary>
    public class MacroProfileManager
    {
        private readonly string _profilesDirectory;

        /// <summary>
        /// Модель профиля макроса
        /// </summary>
        public class MacroProfile
        {
            [JsonPropertyName("name")]
            public string Name { get; set; }

            [JsonPropertyName("gameMode")]
            public GameMode GameMode { get; set; }

            [JsonPropertyName("description")]
            public string Description { get; set; }

            [JsonPropertyName("createdAt")]
            public DateTime CreatedAt { get; set; }

            [JsonPropertyName("actions")]
            public List<MacroAction> Actions { get; set; } = new();

            [JsonPropertyName("repetitions")]
            public int Repetitions { get; set; } = 1;

            [JsonPropertyName("infiniteRepeat")]
            public bool InfiniteRepeat { get; set; } = false;
        }

        public MacroProfileManager(string profilesDirectory = null)
        {
            if (profilesDirectory == null)
            {
                profilesDirectory = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                    "RobloxMacro",
                    "Profiles"
                );
            }

            _profilesDirectory = profilesDirectory;

            // Создать директорию, если её нет
            if (!Directory.Exists(_profilesDirectory))
            {
                Directory.CreateDirectory(_profilesDirectory);
            }
        }

        /// <summary>
        /// Сохранить макрос как профиль
        /// </summary>
        public bool SaveProfile(string profileName, GameMode gameMode, List<MacroAction> actions, string description = "", int repetitions = 1, bool infiniteRepeat = false)
        {
            try
            {
                var profile = new MacroProfile
                {
                    Name = profileName,
                    GameMode = gameMode,
                    Description = description,
                    CreatedAt = DateTime.Now,
                    Actions = actions,
                    Repetitions = repetitions,
                    InfiniteRepeat = infiniteRepeat
                };

                string fileName = SanitizeFileName(profileName) + ".json";
                string filePath = Path.Combine(_profilesDirectory, fileName);

                var options = new JsonSerializerOptions { WriteIndented = true };
                string json = JsonSerializer.Serialize(profile, options);
                File.WriteAllText(filePath, json);

                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error saving profile: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Загрузить макрос из профиля
        /// </summary>
        public MacroProfile LoadProfile(string profileName)
        {
            try
            {
                string fileName = SanitizeFileName(profileName) + ".json";
                string filePath = Path.Combine(_profilesDirectory, fileName);

                if (!File.Exists(filePath))
                    return null;

                string json = File.ReadAllText(filePath);
                var profile = JsonSerializer.Deserialize<MacroProfile>(json);

                return profile;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading profile: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Получить список всех сохраненных профилей
        /// </summary>
        public List<string> GetAvailableProfiles()
        {
            var profiles = new List<string>();

            try
            {
                var files = Directory.GetFiles(_profilesDirectory, "*.json");

                foreach (var file in files)
                {
                    string profileName = Path.GetFileNameWithoutExtension(file);
                    profiles.Add(profileName);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error getting profiles: {ex.Message}");
            }

            return profiles;
        }

        /// <summary>
        /// Удалить профиль
        /// </summary>
        public bool DeleteProfile(string profileName)
        {
            try
            {
                string fileName = SanitizeFileName(profileName) + ".json";
                string filePath = Path.Combine(_profilesDirectory, fileName);

                if (File.Exists(filePath))
                {
                    File.Delete(filePath);
                    return true;
                }

                return false;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error deleting profile: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Получить информацию о профиле
        /// </summary>
        public string GetProfileInfo(string profileName)
        {
            var profile = LoadProfile(profileName);

            if (profile == null)
                return "Профиль не найден";

            return $"{profile.Name} | Режим: {profile.GameMode} | Действий: {profile.Actions.Count} | Создан: {profile.CreatedAt:dd.MM.yyyy HH:mm}";
        }

        /// <summary>
        /// Санитизировать имя файла
        /// </summary>
        private string SanitizeFileName(string fileName)
        {
            foreach (char c in Path.GetInvalidFileNameChars())
            {
                fileName = fileName.Replace(c, '_');
            }

            return fileName;
        }
    }
}
