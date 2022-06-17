using SOFM;

namespace SelfOrganizingMap
{
    public interface INeuron
    {
        int X { get; set; }
        int Y { get; set; }
        Vector Weights { get; }

        double Distance(INeuron neuron);
        void UpdateWeights(IVector input, double distanceDecay, double learningRate);
    }
}