using System.Collections.Generic;

namespace System.CommandLine.Binding
{
    internal static class DictionaryExtensions
    {
        public static TValue GetOrAdd<TKey, TValue>(
            this IDictionary<TKey, TValue> source,
            TKey key,
            Func<TKey, TValue> create)
        {
            if (source.TryGetValue(key, out TValue value))
            {
                return value;
            }
            else
            {
                value = create(key);

                source.Add(key, value);

                return value;
            }
        }
    }
}