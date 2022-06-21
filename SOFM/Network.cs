using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SelfOrganizingMap
{
    public class Network
    {
        private readonly int _inputDimension;
        private readonly int _width;
        private readonly int _height;
        private Neuron[,] Matrix;

        public Network(int inputDimension, int width, int height)
        {
            _inputDimension = inputDimension;
            _width = width;
            _height = height;
            Matrix = new Neuron[width, height];
            for (int i = 0; i < width; i++)
            {
                for (int j = 0; j < height; j++)
                {
                    Matrix[i, j] = new Neuron(inputDimension, i, j);
                }
            }
        }

        public INeuron[] CalculateBestMatchingNeuron(IVector[] inputs)
        {
            var result = new List<INeuron>();
            foreach (var input in inputs)
            {
                result.Add(CalculateBestMatchingNeuron(input));
            }

            return result.ToArray();
        }

        private INeuron CalculateBestMatchingNeuron(IVector input)
        {
            var bmu = GetNeuron(0, 0);
            var bestDist = input.EuclidianDistance(bmu.Weights);

            for (var i = 0; i < _width; i++)
            {
                for (var j = 0; j < _height; j++)
                {
                    var neuron = GetNeuron(i, j);
                    var distance = input.EuclidianDistance(neuron.Weights);
                    if (distance < bestDist)
                    {
                        bmu = neuron;
                        bestDist = distance;
                    }
                }
            }
            return bmu;
        }

        public Neuron GetNeuron(in int indexX, in int indexY)
        {
            return Matrix[indexX, indexY];
        }
    }
}
