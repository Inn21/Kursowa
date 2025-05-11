#region

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using JetBrains.Annotations;

#endregion

namespace Engine.Core.Utils.Extensions
{
    public static class StringExtensions
    {
        [MustUseReturnValue]
        public static bool EndsWithAny(this string name, StringComparison stringComparison, string[] ends)
        {
            return ends.Any(end => name.EndsWith(end, stringComparison));
        }

        [MustUseReturnValue]
        public static bool IsNullOrEmpty(this string value)
        {
            return string.IsNullOrEmpty(value);
        }

        [MustUseReturnValue]
        public static int ContainsCount(this string value, char symbol)
        {
            if (value.IsNullOrEmpty()) return 0;

            var result = 0;
            var count = value.Length;

            for (var i = 0; i < count; i++)
                if (value[i] == symbol)
                    ++result;

            return result;
        }

        [MustUseReturnValue]
        public static string Truncate(this string value, int maxLength)
        {
            if (string.IsNullOrEmpty(value)) return value;
            return value.Length <= maxLength ? value : value.Substring(0, maxLength);
        }

        [MustUseReturnValue]
        public static string Repeat(this string value, int count)
        {
            if (count == 1 || value.IsNullOrEmpty()) return value;
            if (count <= 0) return string.Empty;

            var sb = new StringBuilder(count * value.Length);
            for (var i = 0; i < count; i++) sb.Append(value);

            return sb.ToString();
        }

        public static string RemoveAllOccurences(this string input, string[] subStrings)
        {
            var stack = new Stack<char>();

            for (var i = 0; i < input.Length; i++)
            {
                // Push everything in the stack
                stack.Push(input[i]);
                foreach (var subString in subStrings)
                    // Only compare with substring if stack length is equal or grater than substring
                    if (stack.Count >= subString.Length)
                    {
                        // Temp substring to keep track of popped elements
                        var temp = string.Empty;
                        for (var j = subString.Length - 1; j >= 0; j--)
                            if (stack.Peek() == subString[j])
                            {
                                // If match then pop and keep that in temp
                                temp = stack.Pop() + temp;
                            }
                            else
                            {
                                // If not matched then re insert any popped chars in stack
                                foreach (var c in temp) stack.Push(c);
                                break;
                            }
                    }
            }

            return GetString(stack);
        }

        public static string GetString(Stack<char> stack)
        {
            var str = "";
            while (stack.Count > 0) str = stack.Pop() + str;
            return str;
        }
    }
}