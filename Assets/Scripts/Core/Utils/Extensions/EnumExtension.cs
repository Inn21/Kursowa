#region

using System;
using System.Collections.Generic;
using System.Linq;

#endregion

namespace Engine.Core.Utils.Extensions
{
    public static class EnumExtension
    {
        public static int ToInt<TEnum>(this TEnum value) where TEnum : Enum
        {
            return (int)(object)value;
        }

        public static Dictionary<T, TU> ToDictionary<T, TU>() where T : Enum
        {
            return Enum.GetNames(typeof(T))
                .ToDictionary(name => (T)Enum.Parse(typeof(T), name), name => (TU)(object)name);
        }

        public static Dictionary<TKey, TValue> ToDictionary<TEnum, TKey, TValue>() where TEnum : Enum
        {
            return Enum.GetNames(typeof(TEnum)).ToDictionary(name => (TKey)Enum.Parse(typeof(TEnum), name),
                name => (TValue)(object)name);
        }

        public static Dictionary<string, TEnum> ToDictionary<TEnum>() where TEnum : Enum
        {
            return Enum.GetNames(typeof(TEnum))
                .ToDictionary(name => name, name => (TEnum)Enum.Parse(typeof(TEnum), name));
        }
    }
}