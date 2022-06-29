using ILGPU.Runtime;
using ILGPU;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SelfOrganizingMap
{
    public class GPU
    {
        private static Context _context;
        private static Accelerator _accelerator;
        private static Action<Index1D, ArrayView2D<double, Stride2D.DenseX>, ArrayView2D<double, Stride2D.DenseX>, ArrayView<int>> _loadedKernel;
        private static Action<
            Index1D,
            ArrayView2D<double, Stride2D.DenseX>,
            ArrayView2D<double, Stride2D.DenseX>,
            ArrayView<int>,
            ArrayView<double>> _trainKernel;
        private static int _inputsRowsCount;
        private static int _inputColumnsCount;
        private static MemoryBuffer2D<double, Stride2D.DenseX> _inputData;
        private static MemoryBuffer2D<double, Stride2D.DenseX> _weightData;
        private static int _width;
        private static int _height;
        private static MemoryBuffer1D<int, Stride1D.Dense> _bmuData;
        private static MemoryBuffer1D<double, Stride1D.Dense> _settings_w_h_lr_cr_Data;

        static GPU()
        {
            _context = Context.Create(b =>
                b
                .Default());
            _accelerator = _context.GetPreferredDevice(false).CreateAccelerator(_context);
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

        ~GPU()
        {
            if (_accelerator != null) _accelerator.Dispose();
            if (_inputData != null) _inputData.Dispose();
            if (_weightData != null) _weightData.Dispose();
            if (_bmuData != null) _bmuData.Dispose();
            if (_context != null) _context.Dispose();
        }

        public static void StoreInputs(double[,] inputs)
        {
            _inputsRowsCount = inputs.GetUpperBound(0) + 1;
            _inputColumnsCount = inputs.GetUpperBound(1) + 1;

            _inputData = _accelerator.Allocate2DDenseX<double>(new Index2D(_inputsRowsCount, _inputColumnsCount)); ;
            _inputData.CopyFromCPU(inputs);

            _bmuData = _accelerator.Allocate1D<int>(_inputsRowsCount);
        }

        public static void StoreNetwork(int width, int height)
        {
            _width = width;
            _height = height;

            var weights = new double[width * height, _inputColumnsCount];
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    for (var inputNr = 0; inputNr < _inputColumnsCount; inputNr++)
                    {
                        //Weights.Add(((i + 1) / numOfWeights));
                        //Weights.Add(random.NextDouble() * ((i + 1) / numOfWeights));
                        //Weights.Add(random.NextDouble());
                        weights[x + (y * _width), inputNr] = 0.5;
                    }
                }
            }

            _weightData = _accelerator.Allocate2DDenseX<double>(new Index2D(weights.GetUpperBound(0) + 1, weights.GetUpperBound(1) + 1));

            _weightData.CopyFromCPU(weights);

            _settings_w_h_lr_cr_Data = _accelerator.Allocate1D<double>(4);

        }

        public static (int x, int y)[] Bmus()
        {
            return _bmuData.GetAsArray1D().Select(w => (w % _width, w / _width)).ToArray();
        }

        public static void CalculateBestMatchingNeuronGpu()
        {
            _loadedKernel(_inputsRowsCount, _inputData.View, _weightData.View, _bmuData.View);
            _accelerator.Synchronize();
        }

        private static void CalculateBestMatchingNeuron(
            Index1D index,
            ArrayView2D<double, Stride2D.DenseX> inputs,
            ArrayView2D<double, Stride2D.DenseX> weights,
            ArrayView<int> output)
        {
            int inputNr = index;
            var min = double.MaxValue;
            for (int weightNr = 0; weightNr < weights.IntExtent.X; weightNr++)
            {
                double distance = 0;
                for (int input = 0; input < inputs.IntExtent.Y; input++)
                {
                    var d = inputs[new Index2D(inputNr, input)] - weights[new Index2D(weightNr, input)];
                    distance += d * d;
                    if (distance >= min) break;
                }

                if (distance < min)
                {
                    output[index] = weightNr;
                    min = distance;
                }
            }
        }

        public static void TrainGpu(
             double learningRate,
             double currentRadius)
        {
            var settings_w_h_lr_cr = new double[]
            {
                    _width, _height, learningRate, currentRadius
            };
            _settings_w_h_lr_cr_Data.CopyFromCPU(settings_w_h_lr_cr);

            _trainKernel(_inputsRowsCount, _inputData.View, _weightData.View, _bmuData.View, _settings_w_h_lr_cr_Data.View);
            _accelerator.Synchronize();
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
            double learningRate = settings_w_h_lr_cr_Data[new Index1D(2)];
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
                        // _currentRadius * _currentRadius can be static 
                        var distanceDrop = Math.Exp(-(distance * distance) / (currentRadius * currentRadius));
                        for (int i = 0; i < inputData.IntExtent.Y; i++)
                        {
                            weightData[new Index2D(x + y * width, i)] +=
                                distanceDrop * learningRate * (inputData[new Index2D(inputRow, i)] - weightData[new Index2D(x + y * width, i)]);
                        }
                    }
                }
            }
        }


    }
}
