using System;
using System.Collections.Generic;
using System.Linq;
using SelfOrganizingMap;

namespace SOFM
{
    public class Vector : List<double>, IVector
    {
        public double EuclidianDistance(IVector vector)
        {
            if (vector.Count != Count)
                throw new ArgumentException("Not the same size");

            var all = Distances(vector);
            var euclidianDistance = all.Sum();
            return euclidianDistance;
        }

        public IEnumerable<double> Distances(IVector vector)
        {
            return this.Select(x => Math.Pow(x - vector[IndexOf(x)], 2));
        }

        public override string ToString()
        {
            return "[" + string.Join(", ", this.Select(x => x.ToString("0.0"))) + "]";
        }
    }
}