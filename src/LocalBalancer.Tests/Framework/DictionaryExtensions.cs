using System.Collections.Generic;

namespace LocalBalancer.Tests.Framework
{
    internal static class DictionaryExtensions
    {
        public static Dictionary<TKey, TValue> Merge<TKey, TValue>(this Dictionary<TKey, TValue> current, params Dictionary<TKey, TValue>[] dictionaries)
        {
            foreach (Dictionary<TKey, TValue> dict in dictionaries)
            {
                foreach (KeyValuePair<TKey, TValue> x in dict)
                {
                    current[x.Key] = x.Value;
                }
            }

            return current;
        }
    }
}