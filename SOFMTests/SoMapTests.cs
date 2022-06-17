using Microsoft.VisualStudio.TestTools.UnitTesting;
using SelfOrganizingMap;
using System;

namespace SOFM.Tests
{
    [TestClass()]
    public class SoMapTests
    {
        [TestMethod]
        public void TrainTest()
        {
            var testObject = new SoMap(
                width: 2,
                height: 2,
                inputDimension: 2,
                numberOfIterations: 100,
                learningRate: 0.1);

            var matrix = new Neuron[2, 2];
            matrix[0, 0] = new Neuron(2, 0, 0) { Weights = new Vector { 0.50, 0.51 } };
            matrix[1, 0] = new Neuron(2, 1, 0) { Weights = new Vector { 0.52, 0.53 } };
            matrix[0, 1] = new Neuron(2, 0, 1) { Weights = new Vector { 0.5, 0.55 } };
            matrix[1, 1] = new Neuron(2, 1, 1) { Weights = new Vector { 0.56, 0.56 } };

            testObject.Matrix = matrix;
            var A = new Vector { 3, 1 };
            var B = new Vector { 1, 3 };
            Console.Out.WriteLine("CalculateBMU(A)" + testObject.CalculateBestMatchingNeuron(A));
            Console.Out.WriteLine("CalculateBMU(B)" + testObject.CalculateBestMatchingNeuron(B));
            testObject.Train(new Vector[]
            {
                A, B
            });

            Console.Out.WriteLine("CalculateBMU(A)" + testObject.CalculateBestMatchingNeuron(A));
            Console.Out.WriteLine("CalculateBMU(B)" + testObject.CalculateBestMatchingNeuron(B));
            Assert.IsFalse(testObject.CalculateBestMatchingNeuron(A) == testObject.CalculateBestMatchingNeuron(B));
        }

        [TestMethod()]
        public void TrainSimpleTest()
        {
            // https://medium.com/analytics-vidhya/how-does-self-organizing-algorithm-works-f0664af9bf04
            //AB -
            //-  -
            var testObject = new SoMap(
                width: 2,
                height: 2,
                inputDimension: 2,
                numberOfIterations: 10,
                learningRate: 0.5);

            var matrix = new Neuron[2, 2];
            matrix[0, 0] = new Neuron(2, 0, 0) { Weights = new Vector { 0.45, 0.89 } };
            matrix[1, 0] = new Neuron(2, 0, 1) { Weights = new Vector { 0.55, 0.83 } };
            matrix[0, 1] = new Neuron(2, 1, 0) { Weights = new Vector { 0.62, 0.78 } };
            matrix[1, 1] = new Neuron(2, 1, 1) { Weights = new Vector { 0.95, 0.32 } };

            testObject.Matrix = matrix;
            var A = new Vector { 3, 1 };
            var B = new Vector { 1, 3 };
            Console.Out.WriteLine("CalculateBMU(A) :" + testObject.CalculateBestMatchingNeuron(A));
            Console.Out.WriteLine("CalculateBMU(B) :" + testObject.CalculateBestMatchingNeuron(B));
            Console.Out.WriteLine(A.EuclidianDistance(B));

            testObject.Train(new Vector[]
            {
                A, B
            });
            Console.Out.WriteLine(A.EuclidianDistance(B));

            Console.Out.WriteLine("CalculateBMU(A) :" + testObject.CalculateBestMatchingNeuron(A));
            Console.Out.WriteLine("CalculateBMU(B) :" + testObject.CalculateBestMatchingNeuron(B));

        }

        [TestMethod()]
        public void TrainNearTest()
        {
            // https://medium.com/analytics-vidhya/how-does-self-organizing-algorithm-works-f0664af9bf04
            //AB -
            //-  -
            var testObject = new SoMap(
                width: 2,
                height: 2,
                inputDimension: 2,
                numberOfIterations: 1000,
                learningRate: 0.5);

            var matrix = new Neuron[2, 2];
            matrix[0, 0] = new Neuron(2, 0, 0) { Weights = new Vector { 0.45, 0.89 }};
            matrix[1, 0] = new Neuron(2, 0, 1) { Weights = new Vector { 0.55, 0.83 }};
            matrix[0, 1] = new Neuron(2, 1, 0) { Weights = new Vector { 0.62, 0.78 }};
            matrix[1, 1] = new Neuron(2, 1, 1) { Weights = new Vector { 0.95, 0.32 }};

            testObject.Matrix = matrix;
            var A = new Vector { 3, 1 };
            var B = new Vector { 3.1, 1 };
            Console.Out.WriteLine("CalculateBMU(A) :" + testObject.CalculateBestMatchingNeuron(A));
            Console.Out.WriteLine("CalculateBMU(B) :" + testObject.CalculateBestMatchingNeuron(B));
            Console.Out.WriteLine(A.EuclidianDistance(B));

            testObject.Train(new Vector[]
            {
                A, B
            });
            Console.Out.WriteLine(A.EuclidianDistance(B));

            Console.Out.WriteLine("CalculateBMU(A) :" + testObject.CalculateBestMatchingNeuron(A));
            Console.Out.WriteLine("CalculateBMU(B) :" + testObject.CalculateBestMatchingNeuron(B));

        }

        [TestMethod()]
        public void TrainNear4Test()
        {
            // https://medium.com/analytics-vidhya/how-does-self-organizing-algorithm-works-f0664af9bf04
            //AB -
            //-  -
            var testObject = new SoMap(
                width: 2,
                height: 2,
                inputDimension: 2,
                numberOfIterations: 1000,
                learningRate: 0.5);

            var matrix = new Neuron[2, 2];
            matrix[0, 0] = new Neuron(2, 0, 0) { Weights = new Vector { 0.45, 0.89 }};
            matrix[1, 0] = new Neuron(2, 0, 1) { Weights = new Vector { 0.55, 0.83 }};
            matrix[0, 1] = new Neuron(2, 1, 0) { Weights = new Vector { 0.62, 0.78 }};
            matrix[1, 1] = new Neuron(2, 1, 1) { Weights = new Vector { 0.95, 0.32 }};

            testObject.Matrix = matrix;
            var A = new Vector { 3, 1 };
            var B = new Vector { 3.1, 1 };
            var C = new Vector { 3, 1.1 };
            var D = new Vector { 3.1, 1.1 };
            Console.Out.WriteLine("CalculateBMU(A) :" + testObject.CalculateBestMatchingNeuron(A));
            Console.Out.WriteLine("CalculateBMU(B) :" + testObject.CalculateBestMatchingNeuron(B));
            Console.Out.WriteLine("CalculateBMU(C) :" + testObject.CalculateBestMatchingNeuron(C));
            Console.Out.WriteLine("CalculateBMU(D) :" + testObject.CalculateBestMatchingNeuron(D));
            Console.Out.WriteLine(A.EuclidianDistance(B));

            testObject.Train(new Vector[]
            {
                A, B, C, D
            });
            Console.Out.WriteLine(A.EuclidianDistance(B));

            Console.Out.WriteLine("CalculateBMU(A) :" + testObject.CalculateBestMatchingNeuron(A));
            Console.Out.WriteLine("CalculateBMU(B) :" + testObject.CalculateBestMatchingNeuron(B));
            Console.Out.WriteLine("CalculateBMU(C) :" + testObject.CalculateBestMatchingNeuron(C));
            Console.Out.WriteLine("CalculateBMU(D) :" + testObject.CalculateBestMatchingNeuron(D));

        }


        [TestMethod]
        public void TrainSimple1Test()
        {
            var testObject = new SoMap(
                width: 2,
                height: 2,
                inputDimension: 2,
                numberOfIterations: 1,
                learningRate: 0.5);

            var matrix = new Neuron[2, 2];
            matrix[0, 0] = new Neuron(2, 0, 0) { Weights = new Vector { 0.45, 0.89 }};
            matrix[1, 0] = new Neuron(2, 0, 1) { Weights = new Vector { 0.55, 0.83 }};
            matrix[0, 1] = new Neuron(2, 1, 0) { Weights = new Vector { 0.62, 0.78 }};
            matrix[1, 1] = new Neuron(2, 1, 1) { Weights = new Vector { 0.95, 0.32 }};

            testObject.Matrix = matrix;
            var A = new Vector { 3, 1 };
            var B = new Vector { 1, 3 };
            var C = new Vector { 3, 3 };
            Console.Out.WriteLine(testObject.CalculateBestMatchingNeuron(A));
            Console.Out.WriteLine(testObject.CalculateBestMatchingNeuron(B));
            Console.Out.WriteLine(A.EuclidianDistance(B));

            testObject.Train(new Vector[]
            {
                A, B, C
            });
            Console.Out.WriteLine(A.EuclidianDistance(B));

            Console.Out.WriteLine(testObject.CalculateBestMatchingNeuron(A));
            Console.Out.WriteLine(testObject.CalculateBestMatchingNeuron(B));
            Console.Out.WriteLine(testObject.CalculateBestMatchingNeuron(C));

        }
    }
}