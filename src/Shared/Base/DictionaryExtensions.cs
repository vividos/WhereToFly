using System;
using System.Collections.Generic;

namespace WhereToFly.Shared.Base
{
    /// <summary>
    /// Extension methods for generic dictionaries
    /// </summary>
    public static class DictionaryExtensions
    {
        /// <summary>
        /// Gets value from dictionary, or a default value when key isn't found in dictionary.
        /// </summary>
        /// <typeparam name="TKey">type of key in dictionary</typeparam>
        /// <typeparam name="TValue">type of value in dictionary</typeparam>
        /// <param name="dict">dictionary to search for key</param>
        /// <param name="key">key to search for</param>
        /// <param name="defaultValue">default value to return when key isn't found in dictionary</param>
        /// <returns>found value, or the default value</returns>
        public static TValue GetValueOrDefault<TKey, TValue>(this IDictionary<TKey, TValue> dict, TKey key, TValue defaultValue)
        {
            if (dict == null)
            {
                throw new ArgumentNullException(nameof(dict));
            }

            return dict.TryGetValue(key, out TValue value) ? value : defaultValue;
        }
    }
}
