using System;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace _PROJECT.Scripts.Application.Features.Save
{
    public class JsonSaveFeature : ISaveFeature
    {
        public void Clear(SaveFileVariant variant)
        {
            var path = GetSavePath(variant);
            if (File.Exists(path)) File.Delete(path);
        }

        public bool IsKeyPresent(string key, SaveFileVariant variant)
        {
            var data = LoadData(variant);
            return data.Data.ContainsKey(key);
        }

        public T Load<T>(string key, T defaultValue, SaveFileVariant variant)
        {
            return LoadData(variant).GetValue(key, defaultValue);
        }

        public void Save<T>(T value, string key, SaveFileVariant variant)
        {
            var data = LoadData(variant);
            data.Data[key] = value.ToString();
            SaveData(variant, data);
        }

        public void DeleteKey(string key, SaveFileVariant variant)
        {
            var data = LoadData(variant);
            if (data.Data.Remove(key)) SaveData(variant, data);
        }

        private string GetSavePath(SaveFileVariant variant)
        {
            var basePath = UnityEngine.Application.persistentDataPath;
            return variant switch
            {
                SaveFileVariant.General => Path.Combine(basePath, "GeneralData.json"),
            };
        }

        private static string GetSavePathStatic(SaveFileVariant variant)
        {
            var basePath = UnityEngine.Application.persistentDataPath;
            return variant switch
            {
                SaveFileVariant.General => Path.Combine(basePath, "GeneralData.json"),
            };
        }

        private SaveData LoadData(SaveFileVariant variant)
        {
            var path = GetSavePath(variant);
            if (!File.Exists(path)) return new SaveData();

            try
            {
                var json = File.ReadAllText(path);
                return JsonUtility.FromJson<SaveData>(json) ?? new SaveData();
            }
            catch (Exception e)
            {
                Debug.LogError($"Load failed: {e.Message}");
                return new SaveData();
            }
        }

        private void SaveData(SaveFileVariant variant, SaveData data)
        {
            try
            {
                var json = JsonUtility.ToJson(data, true);
                File.WriteAllText(GetSavePath(variant), json);
            }
            catch (Exception e)
            {
                Debug.LogError($"Save failed: {e.Message}");
            }
        }

        public static void ClearStatic(SaveFileVariant variant)
        {
            var path = GetSavePathStatic(variant);
            if (File.Exists(path)) File.Delete(path);
        }
    }
}