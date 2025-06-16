using System;
using UnityEngine;

namespace _PROJECT.Scripts.Application.Features.Save
{
    [Serializable]
    public class SaveData
    {
        public SerializableDictionary<string, string> Data = new();

        public T GetValue<T>(string key, T defaultValue)
        {
            if (Data.TryGetValue(key, out var str))
                try
                {
                    var targetType = typeof(T);

                    if (targetType == typeof(ulong))
                        return (T)(object)ulong.Parse(str);
                    if (targetType == typeof(uint))
                        return (T)(object)uint.Parse(str);
                    if (targetType == typeof(long))
                        return (T)(object)long.Parse(str);
                    if (targetType == typeof(int))
                        return (T)(object)int.Parse(str);
                    if (targetType == typeof(float))
                        return (T)(object)float.Parse(str);
                    if (targetType == typeof(double))
                        return (T)(object)double.Parse(str);
                    if (targetType == typeof(bool))
                        return (T)(object)bool.Parse(str);
                    if (targetType == typeof(string))
                        return (T)(object)str;

                    return (T)Convert.ChangeType(str, targetType);
                }
                catch (Exception e)
                {
                    Debug.LogWarning($"Conversion failed for key '{key}': {e.Message}");
                }

            return defaultValue;
        }
    }
}