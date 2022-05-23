using System;
using System.Collections.Generic;

namespace SOFM.Tests
{
    public class RawDocument : Dictionary<string, object>
    {
        public Type Type { get; }

        public IEnumerable<string> Get(string key)
        {
            if (!ContainsKey(key)) return new string[0];

            var o = this[key] as string;

            return new List<string> { o };
        }
    }
}