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
        public Network Nn;
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
            Nn = new Network(inputDimension, width, height);
            NumberOfIterations = numberOfIterations;
            LearningRate = learningRate;

            MatrixRadius = Math.Max(Width, Height) / 2.0;
            //MatrixRadius = Math.Min(Width, Height);
            TimeConstant = NumberOfIterations / Math.Log(MatrixRadius);
        }

        public void Train(double[,] input)
        {
            var iteration = 0;
            var learningRate = LearningRate;

            while (iteration < NumberOfIterations)
            {
                iteration = TrainIteration(input, iteration, ref learningRate);
            }
        }

        public int TrainIteration(double[,] input, int iteration, ref double learningRate)
        {
            Iterate?.Invoke(this, EventArgs.Empty);
            var currentRadius = CalculateNeighborhoodRadius(iteration);

            var bmus = Nn.CalculateBestMatchingNeuron(input);
            Train(input, learningRate, bmus, currentRadius);

            iteration++;
            learningRate = LearningRate * Math.Exp(-iteration / NumberOfIterations);
            return iteration;
        }

        private void Train(
            double[,] input, 
            double learningRate, 
            (int x, int y)[] bmus, 
            double currentRadius)
        {
            var cnt = 0;
            for (var inputRow = 0; inputRow <= input.GetUpperBound(0); inputRow++)
            {
                var bmu = bmus[cnt++];
                var (xStart, xEnd, yStart, yEnd) = GetRadiusIndexes(bmu, currentRadius);

                for (var x = xStart; x < xEnd; x++)
                {
                    for (var y = yStart; y < yEnd; y++)
                    {
                        var distance = Nn.Distance(bmu.x, bmu.y, x, y);
                        if (distance <= Math.Pow(currentRadius, 2.0))
                        {
                            var distanceDrop = GetDistanceDrop(distance, currentRadius);
                            Nn.UpdateWeights(x, y, input, inputRow, learningRate, distanceDrop);
                        }
                    }
                }
            }
        }

        public (int xStart, int xEnd, int yStart, int yEnd) GetRadiusIndexes((int x, int y) bmu, double currentRadius)
        {
            var xStart = (int)(bmu.x - currentRadius - 1);
            xStart = xStart < 0 ? 0 : xStart;

            var xEnd = (int)(xStart + currentRadius * 2 + 1);
            if (xEnd > Width) xEnd = Width;

            var yStart = (int)(bmu.y - currentRadius - 1);
            yStart = yStart < 0 ? 0 : yStart;

            var yEnd = (int)(yStart + currentRadius * 2 + 1);
            if (yEnd > Height) yEnd = Height;

            return (xStart, xEnd, yStart, yEnd);
        }

        public double CalculateNeighborhoodRadius(double iteration)
        {
            return MatrixRadius * Math.Exp(-iteration / TimeConstant);
        }

        public double GetDistanceDrop(double distance, double radius)
        {
            return Math.Exp(-(Math.Pow(distance, 2.0) / Math.Pow(radius, 2.0)));
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