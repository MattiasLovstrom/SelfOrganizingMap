using System;
using ResourceModel;

namespace SelfOrganizingMap
{
    public class Neuron : INeuron
    {
        public int X { get; set; }
        public int Y { get; set; }
        public Vector Weights { get; set; }
    }
}