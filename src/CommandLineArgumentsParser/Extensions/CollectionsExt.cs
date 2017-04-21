using System.Collections.Generic;

namespace CommandLineParser.Extensions
{
    public static class CollectionsExt
    {
        public static void AddAll<TKey, TValue>(this Dictionary<TKey, TValue> dictionary, IEnumerable<KeyValuePair<TKey, TValue>> entries)
        {
            foreach (var entry in entries)
            {
                dictionary.Add(entry.Key, entry.Value);
            }
        }

        public static void AddUnderKeys<TKey, TValue>(this Dictionary<TKey, TValue> dictionary, TValue value, IEnumerable<TKey> keys)
        {
            foreach (var key in keys)
            {
                dictionary.Add(key, value);
            }
        }
    }
}