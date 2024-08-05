using System;
using System.Collections.Generic;

namespace StartScript
{
    public static class StringExtensions
    {
        public static bool Contains(this string str, char c)
        {
            for (int i = 0; i < str.Length; i++)
                if (str[i] == c)
                    return true;
            return false;
        }

        public static string AssertLenght(this string str, int lenght, char fillwith)
        {
            if (str.Length >= lenght)
                return str;

            var buffer = new char[lenght];

            for (int i = 0; i < str.Length; i++)
            {
                buffer[i] = str[i];
            }
            for (int i = str.Length; i < lenght; i++)
            {
                buffer[i] = fillwith;
            }

            return new string(buffer);
        }
    }

    public static class MiscExtensions
    {


        public static IEnumerable<T> RangeBetween<T>(this IList<T> list, int start, int end)
        {
            for (int i = start; i < Math.Min(list.Count, end); i++)
            {
                yield return list[i];
            }
        }

        public static IEnumerable<T> ForEach<T>(this IEnumerable<T> it, Action<T> act)
        {
            foreach (var item in it)
            {
                act(item);
            }
            return it;
        }
    }

}

