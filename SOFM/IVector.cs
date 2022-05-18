using System.Collections.Generic;

namespace SelfOrganizingMap
{
    public interface IVector : IList<double>
    {
        double EuclidianDistance(IVector vector);
        IEnumerable<double> Distances(IVector bmuWeights);
    }
}