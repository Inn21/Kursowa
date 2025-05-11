namespace Engine.Core.Utils.Extensions
{
    public static class IntegerExtension
    {
        public static string DisplayWithSuffix(this int num)
        {
            var number = num.ToString();
            if (number.EndsWith("11")) return number + "th";
            if (number.EndsWith("12")) return number + "th";
            if (number.EndsWith("13")) return number + "th";
            if (number.EndsWith("1")) return number + "st";
            if (number.EndsWith("2")) return number + "nd";
            if (number.EndsWith("3")) return number + "rd";
            return number + "th";
        }

        public static string DisplayShortened(this int num)
        {
            return num switch
            {
                >= 100000000 => (num / 1000000).ToString("#,0M"),
                >= 10000000 => (num / 1000000).ToString("0.#") + "M",
                >= 100000 => (num / 1000).ToString("#,0K"),
                >= 10000 => (num / 1000).ToString("0.#") + "K",
                _ => num.ToString("#,0")
            };
        }

        public static string DisplayShortened(this long num)
        {
            return num switch
            {
                >= 100000000 => (num / 1000000).ToString("#,0M"),
                >= 10000000 => (num / 1000000).ToString("0.#") + "M",
                >= 100000 => (num / 1000).ToString("#,0K"),
                >= 10000 => (num / 1000).ToString("0.#") + "K",
                _ => num.ToString("#,0")
            };
        }
    }
}