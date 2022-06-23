using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ILGPU;
using ILGPU.Runtime;

namespace SelfOrganizingMap
{
    public class NnMath
    {
        private static Accelerator _accelerator;
        private static Action<Index1D, ArrayView2D<double, Stride2D.DenseX>, ArrayView2D<double, Stride2D.DenseX>, ArrayView<int>> _loadedKernel;

        static NnMath()
        {
            var context = Context.CreateDefault();

            _accelerator = context.GetPreferredDevice(false).CreateAccelerator(context);
            _loadedKernel =
                _accelerator.LoadAutoGroupedStreamKernel<
                    Index1D,
                    ArrayView2D<double, Stride2D.DenseX>,
                    ArrayView2D<double, Stride2D.DenseX>,
                    ArrayView<int>>(
                    CalculateBestMatchingNeuron);
        }

        ~NnMath()
        {
            if (_accelerator != null) _accelerator.Dispose();
        }


        public static int[] CalculateBestMatchingNeuronGpu(double[,] inputs, double[,] weights)
        {
            
            int inputsRowsCount = inputs.GetUpperBound(0)+1;
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
    }
}
