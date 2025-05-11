#region

using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using Random = UnityEngine.Random;

#endregion

namespace Engine.Core.Utils.Extensions
{
    public static class CollectionExtensions
    {
        [MustUseReturnValue]
        public static bool IsNullOrEmpty<T>(this IReadOnlyCollection<T> data)
        {
            return data == null || data.Count == 0;
        }

        [MustUseReturnValue]
        public static bool IsNullOrEmpty<TKey, TData>(this IDictionary<TKey, TData> data)
        {
            return data == null || data.Count == 0;
        }

        public static void EnsurePresent<TKey, TData>(this IDictionary<TKey, TData> data, TKey key,
            TData defaultData = default)
        {
            if (!data.ContainsKey(key)) data.Add(key, defaultData);
        }

        public static void EnsurePresent<T>(this HashSet<T> data, T value)
        {
            if (!data.Contains(value)) data.Add(value);
        }

        [MustUseReturnValue]
        public static TData GetValueOrDefault<TKey, TData>(this IDictionary<TKey, TData> data, TKey key,
            TData defaultData = default)
        {
            if (!data.IsNullOrEmpty() &&
                data.TryGetValue(key, out var result))
                return result;

            return defaultData;
        }

        [MustUseReturnValue]
        public static TValue[] ValuesToArray<TKey, TValue>(this IDictionary<TKey, TValue> dictionary)
        {
            if (dictionary == null) return null;
            if (dictionary.Count == 0) return Array.Empty<TValue>();

            var result = new TValue[dictionary.Count];
            dictionary.Values.CopyTo(result, 0);
            return result;
        }

        [MustUseReturnValue]
        public static TKey[] KeysToArray<TKey, TValue>(this IDictionary<TKey, TValue> dictionary)
        {
            if (dictionary == null) return null;
            if (dictionary.Count == 0) return Array.Empty<TKey>();

            var keys = new TKey[dictionary.Count];
            dictionary.Keys.CopyTo(keys, 0);
            return keys;
        }

        [MustUseReturnValue]
        public static KeyValuePair<TKey, TValue>[] KeyValuePairsToArray<TKey, TValue>(
            this IDictionary<TKey, TValue> dictionary)
        {
            if (dictionary == null) return null;
            if (dictionary.Count == 0) return Array.Empty<KeyValuePair<TKey, TValue>>();

            var keyValuePairs = new KeyValuePair<TKey, TValue>[dictionary.Count];
            dictionary.CopyTo(keyValuePairs, 0);
            return keyValuePairs;
        }

        public static void Swap<T>(this IList<T> list, int id0, int id1)
        {
            if (id0 == id1) return;

            (list[id0], list[id1]) = (list[id1], list[id0]);
        }

        [MustUseReturnValue]
        public static int GetRandomId<T>(this IReadOnlyList<T> list)
        {
            if (list.IsNullOrEmpty()) return -1;
            return Random.Range(0, list.Count);
        }

        [MustUseReturnValue]
        public static T GetRandom<T>(this IReadOnlyList<T> list)
        {
            return list[GetRandomId(list)];
        }

        public static void EnsurePresent<T>(this IList<T> data, T value)
        {
            if (!data.Contains(value)) data.Add(value);
        }
    }
}