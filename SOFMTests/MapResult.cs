using System.Collections.Generic;
using System.Linq;
using SelfOrganizingMap;

namespace SOFM.Tests
{
    public class MapResults
    {
        private List<MapResult> _result = new List<MapResult>();

        public void Add(RawDocument doc, INeuron neuron)
        {
            _result.Add(new MapResult(doc, neuron));
        }

        public IEnumerable<MapResult> OrderBy(INeuron neuron)
        {
            return _result.OrderBy(r => r.Bmn.Distance(neuron));
        }
    }

    public class MapResult
    {
        public RawDocument Doc { get; }
        public INeuron Bmn { get; }

        public MapResult(RawDocument doc, INeuron bmn)
        {
            Doc = doc;
            Bmn = bmn;
        }

    }
}