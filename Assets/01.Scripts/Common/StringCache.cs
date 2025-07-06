using System.Collections.Generic;

namespace Common
{
    public static class StringCache
    {
        private static readonly Dictionary<string, string> cache = new Dictionary<string, string>();

        public static string Get(string format, params object[] args)
        {
            var key = format + string.Join("", args);
            if (!cache.ContainsKey(key))
            {
                cache[key] = string.Format(format, args);
            }
            return cache[key];
        }
    }
}