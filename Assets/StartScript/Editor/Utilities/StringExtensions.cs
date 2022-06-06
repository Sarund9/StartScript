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


    }

}

