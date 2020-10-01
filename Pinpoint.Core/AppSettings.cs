﻿using System.Text.Json;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Pinpoint.Core
{
    public static class AppSettings
    {
        private static readonly List<Setting> Settings = new List<Setting>();

        public static T GetAs<T>(string key)
        {
            return !(Get(key) is Setting item) ? default : (T) item.Value;
        }

        public static List<T> GetListAs<T>(string key)
        {
            var json = GetAs<JsonElement>(key);
            return json.EnumerateArray()
                .Select(elem => JsonSerializer.Deserialize<T>(elem.ToString()))
                .ToList();
        }

        public static string GetStr(string key)
        {
            return GetAs<string>(key);
        }

        public static object Get(string key)
        {
            return Settings.FirstOrDefault(s => s.Key.Equals(key));
        }

        public static void Put(string key, object value)
        {
            var index = IndexOf(key);

            if (index == -1)
            {
                Settings.Add(new Setting(key, value));
            }
            else
            {
                Settings[index] = new Setting(key, value);
            }
        }

        public static void PutAndSave(string key, object value)
        {
            Put(key, value);
            Save();
        }

        public static bool Contains(string key)
        {
            return IndexOf(key) >= 0;
        }

        public static int IndexOf(string key)
        {
            return Settings.FindIndex(s => s.Key.Equals(key));
        }

        public static void Save()
        {
            if (!File.Exists(AppConstants.SettingsFilePath))
            {
                Directory.CreateDirectory(AppConstants.MainDirectory);
                File.Create(AppConstants.SettingsFilePath).Close();
            }

            var json = JsonSerializer.Serialize(Settings);
            File.WriteAllText(AppConstants.SettingsFilePath, json);
        }

        public static bool Load()
        {
            if (!File.Exists(AppConstants.SettingsFilePath))
            {
                return false;
            }

            var json = File.ReadAllText(AppConstants.SettingsFilePath);
            if (string.IsNullOrEmpty(json))
            {
                return false;
            }

            var container = JsonSerializer.Deserialize<List<Setting>>(json);
            foreach (var setting in container)
            {
                Put(setting.Key, setting.Value);
            }

            return true;
        }

        private class Setting
        {
            public Setting()
            {
            }

            public Setting(string key, object value)
            {
                Key = key;
                Value = value;
            }

            public string Key { get; set; }

            public object Value { get; set; }
        }
    }
}
