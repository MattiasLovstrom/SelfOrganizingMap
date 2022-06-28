using ResourceModel;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace SelfOrganizingMap
{
    public class Network
    {
        private readonly int _inputDimension;
        private readonly int _width;
        private readonly int _height;
        public double[,] _weights;

        public Network(int inputDimension, int width, int height)
        {
            _inputDimension = inputDimension;
            _width = width;
            _height = height;
            _weights = new double[width * height, inputDimension];
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    for (var inputNr = 0; inputNr < inputDimension; inputNr++)
                    {
                        //Weights.Add(((i + 1) / numOfWeights));
                        //Weights.Add(random.NextDouble() * ((i + 1) / numOfWeights));
                        //Weights.Add(random.NextDouble());
                        _weights[x + (y * _width), inputNr] = 0.5;
                    }
                }
            }
        }

        public void UpdateWeights(int x, int y, double[,] input, int inputRow, double distanceDecay, double learningRate)
        {
            for (int i = 0; i < _inputDimension; i++)
            {
                _weights[x + y * _width, i] += distanceDecay * learningRate * (input[inputRow, i] - _weights[x + y * _width, i]);
            }

        }

        // i1  11 21
        // i2  12 22 
        //
        // 11 : i1 i2
        // 21 : i1 i2
        // 12 : i1 i2
        // 22 : i1 i2

        public void Train(
             double[,] input,
             double learningRate,
             (int x, int y)[] bmus,
             double currentRadius)
        {
            //NnMath.Train(_weights, _width, _height, input, learningRate, bmus, currentRadius);
            _weights = NnMath.TrainGpu(_weights, _width, _height, input, learningRate, bmus.Select(u => u.y * _width + u.x).ToArray(), currentRadius);
        }

        public (int x, int y)[] CalculateBestMatchingNeuron(double[,] inputData)
        {
            var weightNrs = NnMath.CalculateBestMatchingNeuronGpu(inputData, _weights);
            return weightNrs.Select(w => (w % _width,w / _width)).ToArray();
        }
    }
}
