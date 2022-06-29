using ILGPU;
using ILGPU.Runtime;
using System;
using System.Diagnostics;

namespace SelfOrganizingMap
{
    public class NnMath
    {
        private static Accelerator _accelerator;
        private static Action<Index1D, ArrayView2D<double, Stride2D.DenseX>, ArrayView2D<double, Stride2D.DenseX>, ArrayView<int>> _loadedKernel;
        private static Action<
            Index1D,
            ArrayView2D<double, Stride2D.DenseX>,
            ArrayView2D<double, Stride2D.DenseX>,
            ArrayView<int>,
            ArrayView<double>> _trainKernel;

        static NnMath()
        {
            var context = Context.Create(b => b.Math(MathMode.Fast).Default()); // Context.CreateDefault();
            _accelerator = context.GetPreferredDevice(false).CreateAccelerator(context);
            _loadedKernel =
                _accelerator.LoadAutoGroupedStreamKernel<
                    Index1D,
                    ArrayView2D<double, Stride2D.DenseX>,
                    ArrayView2D<double, Stride2D.DenseX>,
                    ArrayView<int>>(
                    CalculateBestMatchingNeuron);
            _trainKernel =
                _accelerator.LoadAutoGroupedStreamKernel<
                    Index1D,
                    ArrayView2D<double, Stride2D.DenseX>,
                    ArrayView2D<double, Stride2D.DenseX>,
                    ArrayView<int>,
                    ArrayView<double>>(
                    TrainWeights);
        }


        ~NnMath()
        {
            if (_accelerator != null) _accelerator.Dispose();
        }


        public static int[] CalculateBestMatchingNeuronGpu(double[,] inputs, double[,] weights)
        {

            int inputsRowsCount = inputs.GetUpperBound(0) + 1;
            int inputColumnsCount = inputs.GetUpperBound(1) + 1;
            int outputCount = weights.GetUpperBound(0) + 1;
            using var inputData = _accelerator.Allocate2DDenseX<double>(new Index2D(inputsRowsCount, inputColumnsCount)); ;
            using var weightData = _accelerator.Allocate2DDenseX<double>(new Index2D(outputCount, inputColumnsCount));
            using var deviceOutput = _accelerator.Allocate1D<int>(inputsRowsCount);
            inputData.CopyFromCPU(inputs);
            weightData.CopyFromCPU(weights);
            _loadedKernel(inputsRowsCount, inputData.View, weightData.View, deviceOutput.View);
            _accelerator.Synchronize();

            return deviceOutput.GetAsArray1D();
        }

        private static void CalculateBestMatchingNeuron(
            Index1D index,
            ArrayView2D<double, Stride2D.DenseX> inputs,
            ArrayView2D<double, Stride2D.DenseX> weights,
            ArrayView<int> output)
        {
            int inputNr = index;
            float min = float.MaxValue;
            for (int weightNr = 0; weightNr < weights.IntExtent.X; weightNr++)
            {
                float distance = 0;
                for (int input = 0; input < inputs.IntExtent.Y; input++)
                {
                    distance += (float)Math.Pow(inputs[new Index2D(inputNr, input)] - weights[new Index2D(weightNr, input)], 2);
                    if (distance >= min) break;
                }

                if (distance < min)
                {
                    output[index] = weightNr;
                    min = distance;
                }
            }
        }

        public static int[] CalculateBestMatchingNeuron(double[,] inputs, double[,] weights)
        {
            var output = new int[inputs.GetUpperBound(0) + 1];
            for (var inputNr = 0; inputNr <= inputs.GetUpperBound(0); inputNr++)
            {
                float min = float.MaxValue;
                for (var weightNr = 0; weightNr <= weights.GetUpperBound(0); weightNr++)
                {
                    float distance = 0;
                    for (var input = 0; input <= inputs.GetUpperBound(1); input++)
                    {
                        distance += (float)Math.Pow(inputs[inputNr, input] - weights[weightNr, input], 2);
                    }

                    if (distance < min)
                    {
                        min = distance;
                        output[inputNr] = weightNr;
                    }
                }
            }

            return output;
        }

        public static double[,] TrainGpu(
            double[,] weights,
            int width,
            int height,
             double[,] inputs,
             double learningRate,
             int[] bmus,
             double currentRadius)
        {
            var settings_w_h_lr_cr = new double[]
            {
                width, height, learningRate, currentRadius
            };

            int inputsRowsCount = inputs.GetUpperBound(0) + 1;
            int inputColumnsCount = inputs.GetUpperBound(1) + 1;

            using var inputData = _accelerator.Allocate2DDenseX<double>(new Index2D(inputsRowsCount, inputColumnsCount)); ;
            using var weightData = _accelerator.Allocate2DDenseX<double>(new Index2D(weights.GetUpperBound(0) + 1, weights.GetUpperBound(1) + 1));
            using var bmusData = _accelerator.Allocate1D<int>(inputsRowsCount);
            using var settings_w_h_lr_cr_Data = _accelerator.Allocate1D<double>(4);


            inputData.CopyFromCPU(inputs);
            weightData.CopyFromCPU(weights);
            bmusData.CopyFromCPU(bmus);
            settings_w_h_lr_cr_Data.CopyFromCPU(settings_w_h_lr_cr);

            _trainKernel(inputsRowsCount, inputData.View, weightData.View, bmusData.View, settings_w_h_lr_cr_Data.View);
            _accelerator.Synchronize();

            return weightData.GetAsArray2D();
        }

        private static void TrainWeights(
            Index1D index,
            ArrayView2D<double, Stride2D.DenseX> inputData,
            ArrayView2D<double, Stride2D.DenseX> weightData,
            ArrayView<int> bmusData,
            ArrayView<double> settings_w_h_lr_cr_Data)
        {
            int inputRow = index;
            int width = (int)settings_w_h_lr_cr_Data[new Index1D(0)];
            int height = (int)settings_w_h_lr_cr_Data[new Index1D(1)];
            double leraningRate = settings_w_h_lr_cr_Data[new Index1D(2)];
            double currentRadius = settings_w_h_lr_cr_Data[new Index1D(3)];
            var bmu = bmusData[index];
            var bmux = bmu % width;
            var bmuy = bmu % width;
            var xStart = (int)(bmux - currentRadius - 1);
            xStart = xStart < 0 ? 0 : xStart;

            var xEnd = (int)(xStart + currentRadius * 2 + 1);
            if (xEnd > width) xEnd = width;

            var yStart = (int)(bmuy - currentRadius - 1);
            yStart = yStart < 0 ? 0 : yStart;

            var yEnd = (int)(yStart + currentRadius * 2 + 1);
            if (yEnd > height) yEnd = height;

            for (var x = xStart; x < xEnd; x++)
            {
                for (var y = yStart; y < yEnd; y++)
                {
                    var distance = Math.Pow(bmux - x, 2) + Math.Pow(bmuy - y, 2);
                    if (distance <= Math.Pow(currentRadius, 2.0))
                    {
                        var distanceDrop = Math.Exp(-(Math.Pow(distance, 2.0) / Math.Pow(currentRadius, 2.0)));
                        for (int i = 0; i < inputData.IntExtent.Y; i++)
                        {
                            weightData[new Index2D(x + y * width, i)] += 
                                distanceDrop * leraningRate * (inputData[new Index2D(inputRow, i)] - weightData[new Index2D(x + y * width, i)]);
                        }
                    }
                }
            }
        }


        public static void Train(
            double[,] _weights,
            int _width,
            int _height,
             double[,] input,
             double learningRate,
             (int x, int y)[] bmus,
             double currentRadius)
        {
            var cnt = 0;
            var _inputDimension = input.GetUpperBound(1) + 1;
            for (var inputRow = 0; inputRow <= input.GetUpperBound(0); inputRow++)
            {
                var bmu = bmus[cnt++];
                var xStart = (int)(bmu.x - currentRadius - 1);
                xStart = xStart < 0 ? 0 : xStart;
                
                var xEnd = (int)(xStart + currentRadius * 2 + 1);
                if (xEnd > _width) xEnd = _width;
                Trace.TraceInformation("n:" + xEnd);

                var yStart = (int)(bmu.y - currentRadius - 1);
                yStart = yStart < 0 ? 0 : yStart;

                var yEnd = (int)(yStart + currentRadius * 2 + 1);
                if (yEnd > _height) yEnd = _height;

                for (var x = xStart; x < xEnd; x++)
                {
                    for (var y = yStart; y < yEnd; y++)
                    {
                        var distance = Math.Pow(bmu.x - x, 2) + Math.Pow(bmu.y - y, 2);
                        if (distance <= Math.Pow(currentRadius, 2.0))
                        {
                            var distanceDrop = Math.Exp(-(Math.Pow(distance, 2.0) / Math.Pow(currentRadius, 2.0)));
                            for (int i = 0; i < _inputDimension; i++)
                            {
                                _weights[x + y * _width, i] += distanceDrop * learningRate * (input[inputRow, i] - _weights[x + y * _width, i]);
                            }
                        }
                    }
                }
            }
        }
    }
}
