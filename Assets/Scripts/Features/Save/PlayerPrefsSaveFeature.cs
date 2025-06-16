using UnityEngine;
using UnityEngine.SceneManagement;

namespace _PROJECT.Scripts.Application.Features.Save
{
    public class PlayerPrefsSaveFeature
    {
        private string GetPrefixedKey(string key)
        {
            var sceneName = SceneManager.GetActiveScene().name;
            return $"{sceneName}_{key}";
        }

        public void Clear()
        {
            PlayerPrefs.DeleteAll();
        }

        public bool IsKeyPresent(string key, bool isPrefixed = true)
        {
            if (isPrefixed) return PlayerPrefs.HasKey(GetPrefixedKey(key));
            return PlayerPrefs.HasKey(key);
        }

        public void DeleteKey(string key)
        {
            var prefixedKey = GetPrefixedKey(key);

            if (PlayerPrefs.HasKey(prefixedKey)) PlayerPrefs.DeleteKey(prefixedKey);

            if (PlayerPrefs.HasKey(key)) PlayerPrefs.DeleteKey(key);

            PlayerPrefs.Save();
        }

        #region Load With Prefix

        public int LoadInt(string key, int def = 0)
        {
            return PlayerPrefs.GetInt(GetPrefixedKey(key), def);
        }

        public bool LoadBool(string key, bool def = false)
        {
            return PlayerPrefs.GetInt(GetPrefixedKey(key), def ? 1 : 0) == 1;
        }

        public float LoadFloat(string key, float def = 0)
        {
            return PlayerPrefs.GetFloat(GetPrefixedKey(key), def);
        }

        public string LoadString(string key, string def = "")
        {
            return PlayerPrefs.GetString(GetPrefixedKey(key), def);
        }

        public ulong LoadUlong(string key, ulong def = 0)
        {
            var val = PlayerPrefs.GetString(GetPrefixedKey(key), def.ToString());
            return ulong.TryParse(val, out var result) ? result : def;
        }

        #endregion

        #region Load Without Prefix

        public int LoadIntWithoutPrefix(string key, int def = 0)
        {
            return PlayerPrefs.GetInt(key, def);
        }

        public float LoadFloatWithoutPrefix(string key, float def = 0)
        {
            return PlayerPrefs.GetFloat(key, def);
        }

        public bool LoadBoolWithoutPrefix(string key, bool def = false)
        {
            return PlayerPrefs.GetInt(key, def ? 1 : 0) == 1;
        }

        public string LoadStringWithoutPrefix(string key, string def = "")
        {
            return PlayerPrefs.GetString(key, def);
        }

        public ulong LoadUlongWithoutPrefix(string key, ulong def = 0)
        {
            var val = PlayerPrefs.GetString(key, def.ToString());
            return ulong.TryParse(val, out var result) ? result : def;
        }

        #endregion

        #region Save With Prefix

        public void Save(int value, string key)
        {
            PlayerPrefs.SetInt(GetPrefixedKey(key), value);
            PlayerPrefs.Save();
        }

        public void Save(bool value, string key)
        {
            PlayerPrefs.SetInt(GetPrefixedKey(key), value ? 1 : 0);
            PlayerPrefs.Save();
        }

        public void Save(float value, string key)
        {
            PlayerPrefs.SetFloat(GetPrefixedKey(key), value);
            PlayerPrefs.Save();
        }

        public void Save(ulong value, string key)
        {
            PlayerPrefs.SetString(GetPrefixedKey(key), value.ToString());
            PlayerPrefs.Save();
        }

        public void Save(string value, string key)
        {
            PlayerPrefs.SetString(GetPrefixedKey(key), value);
            PlayerPrefs.Save();
        }

        #endregion

        #region Save Without Prefix

        public void SaveWithoutPrefix(int value, string key)
        {
            PlayerPrefs.SetInt(key, value);
            PlayerPrefs.Save();
        }

        public void SaveWithoutPrefix(bool value, string key)
        {
            PlayerPrefs.SetInt(key, value ? 1 : 0);
            PlayerPrefs.Save();
        }

        public void SaveWithoutPrefix(float value, string key)
        {
            PlayerPrefs.SetFloat(key, value);
            PlayerPrefs.Save();
        }

        public void SaveWithoutPrefix(ulong value, string key)
        {
            PlayerPrefs.SetString(key, value.ToString());
            PlayerPrefs.Save();
        }

        public void SaveWithoutPrefix(string value, string key)
        {
            PlayerPrefs.SetString(key, value);
            PlayerPrefs.Save();
        }

        #endregion
    }
}