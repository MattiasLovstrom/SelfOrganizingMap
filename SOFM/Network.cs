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



        public double[] GetWeights(int x, int y)
        {
            var result = new double[_inputDimension];
            for (int i = 0; i < _inputDimension; i++)
            {
                result[i] = _weights[x + y * _width, i];
            }

            return result;
        }

        public Neuron GetNeuron(int weightNr)
        {
            var v = new Vector();
            for (var i = 0; i < _inputDimension; i++)
            {
                v.Add(_weights[weightNr, i]);
            }
            return new Neuron
            {
                X = weightNr % _width,
                Y = weightNr / _width,
                Weights = v
            };
        }

        public Neuron GetNeuron(int x, int y)
        {
            var v = new Vector();
            foreach (var weight in GetWeights(x, y))
            {
                v.Add(weight);
            }
            return new Neuron
            {
                X = x,
                Y = y,
                Weights = v
            };
        }

        public void UpdateWeights(Neuron t, IVector input, double distanceDecay, double learningRate)
        {
            if (input.Count != t.Weights.Count)
                throw new ArgumentException("Wrong input!");

            for (int i = 0; i < t.Weights.Count; i++)
            {
                _weights[t.X + t.Y * _width, i] += distanceDecay * learningRate * (input[i] - _weights[t.X + t.Y * _width, i]);
            }
        }

        // i1  11 21
        // i2  12 22 
        //
        // 11 : i1 i2
        // 21 : i1 i2
        // 12 : i1 i2
        // 22 : i1 i2

       

        public INeuron[] CalculateBestMatchingNeuron(IVector[] inputs)
        {
            var inputData = new double[inputs.Length, _inputDimension];
            for (var i = 0; i < inputs.Length; i++)
            {
                for (var j = 0; j < _inputDimension; j++)
                {
                    inputData[i, j] = inputs[i][j];
                }
            }
            var weightNrs = NnMath.CalculateBestMatchingNeuronGpu(inputData, _weights);
            return weightNrs.Select(x => GetNeuron(x)).ToArray();
        }

        public double Distance(INeuron t, INeuron neuron)
        {
            return Math.Pow(t.X - neuron.X, 2) + Math.Pow(t.Y - neuron.Y, 2);
        }
    }
}
