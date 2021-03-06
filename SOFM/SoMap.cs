using System;
using System.IO;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using ResourceModel;

namespace SelfOrganizingMap
{
    public class SoMap
    {
        public Neuron[,] Matrix;
        public int Height;
        public int Width;
        public double MatrixRadius;
        public int NumberOfIterations;
        public double TimeConstant;
        public double LearningRate;

        public event EventHandler Iterate;

        public SoMap(
            int width, 
            int height, 
            int inputDimension, 
            int numberOfIterations, 
            double learningRate)
        {
            Width = width;
            Height = height;
            Matrix = new Neuron[Width, Height];
            NumberOfIterations = numberOfIterations;
            LearningRate = learningRate;

            MatrixRadius = Math.Max(Width, Height) / 2.0;
            //MatrixRadius = Math.Min(Width, Height);
            TimeConstant = NumberOfIterations / Math.Log(MatrixRadius);

            InitializeConnections(inputDimension);
        }

        public void Train(Vector[] input)
        {
            var iteration = 0;
            var learningRate = LearningRate;

            while (iteration < NumberOfIterations)
            {
                iteration = TrainIteration(input, iteration, ref learningRate);
            }
        }

        public int TrainIteration(Vector[] input, int iteration, ref double learningRate)
        {
            Iterate?.Invoke(this, EventArgs.Empty);
            var currentRadius = CalculateNeighborhoodRadius(iteration);

            foreach (var currentInput in input)
            {
                var bmu = CalculateBestMatchingNeuron(currentInput);
                var (xStart, xEnd, yStart, yEnd) = GetRadiusIndexes(bmu, currentRadius);

                for (var x = xStart; x < xEnd; x++)
                {
                    for (var y = yStart; y < yEnd; y++)
                    {
                        var processingNeuron = GetNeuron(x, y);
                        var distance = bmu.Distance(processingNeuron);
                        if (distance <= Math.Pow(currentRadius, 2.0))
                        {
                            var distanceDrop = GetDistanceDrop(distance, currentRadius);
                            processingNeuron.UpdateWeights(currentInput, learningRate, distanceDrop);
                        }
                    }
                }
            }

            iteration++;
            learningRate = LearningRate * Math.Exp(-iteration / NumberOfIterations);
            return iteration;
        }

        public (int xStart, int xEnd, int yStart, int yEnd) GetRadiusIndexes(INeuron bmu, double currentRadius)
        {
            var xStart = (int)(bmu.X - currentRadius - 1);
            xStart = xStart < 0 ? 0 : xStart;

            var xEnd = (int)(xStart + currentRadius * 2 + 1);
            if (xEnd > Width) xEnd = Width;

            var yStart = (int)(bmu.Y - currentRadius - 1);
            yStart = yStart < 0 ? 0 : yStart;

            var yEnd = (int)(yStart + currentRadius * 2 + 1);
            if (yEnd > Height) yEnd = Height;

            return (xStart, xEnd, yStart, yEnd);
        }

        public INeuron GetNeuron(int indexX, int indexY)
        {
            if (indexX > Width || indexY > Height)
                throw new ArgumentException("Wrong index!");

            return Matrix[indexX, indexY];
        }

        public double CalculateNeighborhoodRadius(double iteration)
        {
            return MatrixRadius * Math.Exp(-iteration / TimeConstant);
        }

        public double GetDistanceDrop(double distance, double radius)
        {
            return Math.Exp(-(Math.Pow(distance, 2.0) / Math.Pow(radius, 2.0)));
        }

        public INeuron CalculateBestMatchingNeuron(IVector input)
        {
            var bmu = Matrix[0, 0];
            var bestDist = input.EuclidianDistance(bmu.Weights);

            for (var i = 0; i < Width; i++)
            {
                for (var j = 0; j < Height; j++)
                {
                    var neuron = Matrix[i, j];
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

        private void InitializeConnections(int inputDimension)
        {
            for (int i = 0; i < Width; i++)
            {
                for (int j = 0; j < Height; j++)
                {
                    Matrix[i, j] = new Neuron(inputDimension, i, j);
                }
            }
        }

        public override string ToString()
        {
            var str = new StringBuilder();
            for (var i = 0; i < Width; i++)
            {
                for (var j = 0; j < Height; j++)
                {
                    str.Append(Matrix[i, j]).Append(" ");
                }

                str.AppendLine();
            }

            return str.ToString();
        }

        public void Display(params (string id, INeuron neuron)[] selected)
        {
            var str = new StringBuilder();
            for (var i = 0; i < Width; i++)
            {
                for (var j = 0; j < Height; j++)
                {
                    var sel = selected.Where(s => s.neuron.X == i && s.neuron.Y == j).ToArray();
                    if (sel.Length == 1)
                    {
                        str.Append(sel.First().id);
                    }
                    else if (sel.Length > 1)
                    {
                        str.Append("M");
                    }
                    else
                    {
                        str.Append("-");
                    }
                }

                str.AppendLine();
            }

            Console.Out.WriteLine(str);
        }

        public void Save(string folder)
        {
            if (!Directory.Exists(folder))
            {
                Directory.CreateDirectory(folder);
            }
            File.WriteAllText($"{folder}\\somap.json", JsonConvert.SerializeObject(this,Formatting.Indented));
        }

        public static SoMap Load(string folder)
        {
            var fileName = $"{folder}\\somap.json";
            if (!File.Exists(fileName)) return null;

            return JsonConvert.DeserializeObject<SoMap>(File.ReadAllText(fileName));
        }
    }
}