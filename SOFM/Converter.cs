using System.Collections.Generic;
using System.Text;

namespace SelfOrganizingMap
{
    public class Converter
    {
        readonly List<List<string>> _lists = new List<List<string>>();

        public Converter()
        {
            _lists.Add(new List<string>());
        }

        public void Add(IEnumerable<string> inData)
        {
            var usedLists = new List<int>();
            foreach (var data in inData)
            {
                Insert(data, usedLists);
            }
        }

        private void Insert(string data, List<int> usedLists)
        {
            var list = ListFor(data);
            if (list > 0)
            {
                if (usedLists.Contains(list))
                {
                    Split(list, data);
                    return;
                }
                usedLists.Add(list);
                return;
            }

            for (list = 0; list < _lists.Count; list++)
            {
                if (usedLists.Contains(list)) continue;
                _lists[list].Add(data);
                usedLists.Add(list);
                return;
            }

            _lists.Add(new List<string> { data });
            usedLists.Add(_lists.Count - 1);
        }

        private int ListFor(string data)
        {
            for (var list = 0; list < _lists.Count; list++)
            {
                if (_lists[list].Contains(data))
                {
                    return list;
                }
            }

            return -1;
        }

        private void Split(int list, string data)
        {
            _lists[list].Remove(data);
            _lists.Add(new List<string> { data });
        }

        public IEnumerable<double> Read(IEnumerable<string> query)
        {
            var result = new double[_lists.Count];
            foreach (var data in query)
            {
                var listFor = ListFor(data);
                result[listFor] = GetValue(data, _lists[listFor]);
            }

            return result;
        }

        private double GetValue(string data, List<string> list)
        {
            var lastIndexOf = list.LastIndexOf(data);
            if (lastIndexOf < 0) return 0;

            return (double)(lastIndexOf + 1) / (list.Count + 1);
        }


        public override string ToString()
        {
            var display = new StringBuilder();
            foreach (var list in _lists)
            {
                foreach (var row in list)
                {
                    display.AppendLine(row);
                }

                display.AppendLine();
            }

            return display.ToString();
        }
    }
}
