using System.Collections.Generic;
using System.Linq;

namespace SOFM.Tests
{
    public class Inputs
    {
        private List<string> _keys;
        Dictionary<string, KeyList> _lists = new Dictionary<string, KeyList>();
        private List<Vector> _values = new List<Vector>();

        public Inputs(List<string> keys)
        {
            _keys = keys;
        }

        public void Add(RawDocument document)
        {
            var input = new Vector();
            for (var i = 0; i < _keys.Count; i++)
            {
                var key = _keys[i];
                var val = document.Get(key).FirstOrDefault();
                if (val != null && val.Any())
                {
                    input.Add(_lists[key].Input(val));
                }
                else
                {
                    input.Add(0);
                }
            }

            _values.Add(input);
        }

        public Vector[] Values => _values.ToArray();
    }
}