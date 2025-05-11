#region

using System;
using JetBrains.Annotations;
using UnityEngine;

#endregion

namespace Engine.Core.Utils.Extensions
{
    public static class GeometryExtension
    {
        [MustUseReturnValue]
        public static bool IsEqual(this Vector3 vec1, Vector3 vec2, float precision = 0.0001f)
        {
            return (vec2 - vec1).sqrMagnitude < precision;
        }

        [MustUseReturnValue]
        public static Vector3 Abs(this Vector3 vec)
        {
            return new Vector3(Math.Abs(vec.x), Math.Abs(vec.y), Math.Abs(vec.z));
        }

        [MustUseReturnValue]
        public static Vector3 Inversed(this Vector3 vec)
        {
            return new Vector3(1 / vec.x, 1 / vec.y, 1 / vec.z);
        }

        [MustUseReturnValue]
        public static Vector3 Negative(this Vector3 vec)
        {
            return new Vector3(-vec.x, -vec.y, -vec.z);
        }
    }
}