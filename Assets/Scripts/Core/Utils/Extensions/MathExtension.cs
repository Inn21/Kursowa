#region

using UnityEngine;

#endregion

namespace Engine.Core.Utils.Extensions
{
    public static class MathExtension
    {
        public static long Clamp(long value, long min, long max)
        {
            if (value < min) return min;
            return value > max ? max : value;
        }

        public static int LongToSortingInt(this long value)
        {
            return value switch
            {
                < 0 => -1,
                > 0 => 1,
                _ => 0
            };
        }

        public static long PositivePow(long value, long power)
        {
            if (value <= 1 || power == 1) return value;

            long result = 1;
            while (power > 0)
            {
                result *= value;
                --power;
            }

            return result;
        }

        public static float UnitIntervalRange(float stageStartRange, float stageFinishRange, float newStartRange,
            float newFinishRange, float floatingValue)
        {
            var normalizedValue = Mathf.InverseLerp(stageStartRange, stageFinishRange, floatingValue);
            return Mathf.Lerp(newStartRange, newFinishRange, normalizedValue);
        }

        public static float GetSquareDiagonal(float x1, float x2, float y1, float y2)
        {
            return Mathf.Sqrt(Mathf.Pow(Mathf.Abs(x1) + Mathf.Abs(x2), 2) +
                              Mathf.Pow(Mathf.Abs(y1) + Mathf.Abs(y2), 2));
        }
    }
}