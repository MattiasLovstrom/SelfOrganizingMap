using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SOFM.Tests
{
    public class KeyList
    {
        private List<string> _values = new List<string>();

        public void Add(IEnumerable<string> values)
        {
            if (values == null) return;

            foreach (var value in values)
            {
                if (_values.Contains(value)) continue;
                _values.Add(value);
            }
        }

        public double Input(string value)
        {
            return (_values.IndexOf(value) + 1.0) / (_values.Count + 1.0);
        }

        public override string ToString()
        {
            var str = new StringBuilder();
            for (int i = 0; i < _values.Count; i++)
            {
                str.AppendLine($"{_values[i]}\t{Input(_values[i]).ToString("0.00")}");
            }

            return str.ToString();

        }
    }
}