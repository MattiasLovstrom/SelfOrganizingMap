using System;
using System.Collections.Generic;
using System.Linq;
using SelfOrganizingMap;

namespace SOFM.Tests
{
    public class Collector
    {
        private Dictionary<string, List<string>> _lists = new Dictionary<string, List<string>>();
        private List<RawDocument> _data = new List<RawDocument>();

        public Collector(List<string> keys)
        {
            foreach (var key in keys)
            {
                _lists.Add(key, new List<string>());
            }
        }

        public void Import(RawDocument document)
        {
            _data.Add(document);
            foreach (var key in _lists.Keys)
            {
                if (!document.TryGetValue(key, out var rawValue)) continue;
                var value = rawValue as string;
                if (value == null  || _lists[key].Contains(value)) continue;

                _lists[key].Add(value);
            }
        }

        public IEnumerable<RawDocument> Documents => _data;
        

        public Vector ToNormalizedData(RawDocument data)
        {
            var input = new Vector();
            foreach (var key in _lists.Keys)
            {
                if (data.TryGetValue(key, out var value))
                {
                    input.Add(1.0*(_lists[key].IndexOf((string) value) + 1.0) / (_lists[key].Count + 1));
                }
                else
                {
                    input.Add(0);
                }
            }

            return input;
        }

        public IEnumerable<string> GetList(string key)
        {
            return _lists[key];
        }

        public void SetList(string key, IEnumerable<string> list)
        {
            _lists[key] = new List<string>(list);
        }
    }

    public class CollectorData
    {
        private readonly Dictionary<string, string> _keyValue = new Dictionary<string, string>();

        public CollectorData(IEnumerable<string> keys, RawDocument document)
        {
            foreach (var key in keys)
            {
                Add(key, document.Get(key).FirstOrDefault());
            }
        }

        private void Add(string key, string data)
        {
            if (data == null) return;

            _keyValue.Add(key, data);
        }

        public bool TryGetValue(string key, out string  value)
        {
            return  _keyValue.TryGetValue(key, out value);
        }
    }
}