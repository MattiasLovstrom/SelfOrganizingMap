using System;
using ResourceModel;

namespace SelfOrganizingMap
{
    public class Neuron : INeuron
    {
        public int X { get; set; }
        public int Y { get; set; }
        public Vector Weights { get; set; }

        public Neuron(int numOfWeights, int x, int y)
        {
            X = x;
            Y = y;
            var random = new Random();
            Weights = new Vector();

            for (var i = 0; i < numOfWeights; i++)
            {
                Weights.Add(0.5);
                //Weights.Add(((i + 1) / numOfWeights));
                //Weights.Add(random.NextDouble() * ((i + 1) / numOfWeights));
                //Weights.Add(random.NextDouble());
            }
        }

        public double Distance(INeuron neuron)
        {
            return Math.Pow(X - neuron.X, 2) + Math.Pow(Y - neuron.Y, 2);
        }

        public void UpdateWeights(IVector input, double distanceDecay, double learningRate)
        {
            if (input.Count != Weights.Count)
                throw new ArgumentException("Wrong input!");

            for (int i = 0; i < Weights.Count; i++)
            {
                Weights[i] += distanceDecay * learningRate * (input[i] - Weights[i]);
            }
        }

        public override string ToString()
        {
            return $"{X}:{Y}";
        }
    }
}