using Newtonsoft.Json;
using System;
using System.IO;

namespace SelfOrganizingMap
{
    public class SoMap
    {
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
            //Nn = new Network(inputDimension, width, height);
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

            GPU.CalculateBestMatchingNeuronGpu();
            GPU.TrainGpu(learningRate, currentRadius);

            iteration++;
            learningRate = LearningRate * Math.Exp(-iteration / NumberOfIterations);
            return iteration;
        }

        public double CalculateNeighborhoodRadius(double iteration)
        {
            return MatrixRadius * Math.Exp(-iteration / TimeConstant);
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