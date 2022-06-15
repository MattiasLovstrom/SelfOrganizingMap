using System;

namespace SelfOrganizingMap
{
    class Program
    {
        static void Main(string[] args)
        {
            var testObject = new SoMap(
                width: 7,
                height: 7,
                inputDimension: 2,
                numberOfIterations: 10000,
                learningRate: 0.01);

            var inputVector0 = new SOFM.Vector { 0, 0 };   // 1:1
            var inputVector1 = new SOFM.Vector { 1, 1 };   // 3:5
            var inputVector2 = new SOFM.Vector { 2, 2 };   // 9:5
            var inputVector3 = new SOFM.Vector { 3, 3 };   // 5:0
            var inputVector4 = new SOFM.Vector { 4, 4 };   // 7:0
            var inputVector5 = new SOFM.Vector { 5, 5 };   // 7:2
            var inputVector6 = new SOFM.Vector { 6, 6 };   // 9:2
            var testVectorA = new SOFM.Vector { 2.1, 2.1 };// 8:5
            var testVectorB = new SOFM.Vector { 2.2, 2.2 };// 4:3
            var testVectorC = new SOFM.Vector { 1.9, 1.9 };// 9:5
            // 00 --
            var input = new[]
            {
                inputVector0,
                inputVector1,
                inputVector2,
                inputVector3,
                inputVector4,
                inputVector5,
                inputVector6
            };
            int i = 0;

            testObject.Iterate += (sender, eventArgs) =>
            {
                Console.SetCursorPosition(0,0);
                Console.Out.WriteLine(i++);
                testObject.Display(
                    ("0", testObject.CalculateBestMatchingNeuron(inputVector0)),
                    ("1", testObject.CalculateBestMatchingNeuron(inputVector1)),
                    ("2", testObject.CalculateBestMatchingNeuron(inputVector2)),
                    ("3", testObject.CalculateBestMatchingNeuron(inputVector3)),
                    ("4", testObject.CalculateBestMatchingNeuron(inputVector4)),
                    ("5", testObject.CalculateBestMatchingNeuron(inputVector5)),
                    ("6", testObject.CalculateBestMatchingNeuron(inputVector6)),
                    ("A", testObject.CalculateBestMatchingNeuron(testVectorA)),
                    ("B", testObject.CalculateBestMatchingNeuron(testVectorB)),
                    ("C", testObject.CalculateBestMatchingNeuron(testVectorC))
                );
                //Thread.Sleep(2);
            };

            testObject.Train(input);
            //Console.Out.WriteLine("input");
            testObject.Display(
                ("0", testObject.CalculateBestMatchingNeuron(inputVector0)),
                ("1", testObject.CalculateBestMatchingNeuron(inputVector1)),
                ("2", testObject.CalculateBestMatchingNeuron(inputVector2)),
                ("3", testObject.CalculateBestMatchingNeuron(inputVector3)),
                ("4", testObject.CalculateBestMatchingNeuron(inputVector4)),
                ("5", testObject.CalculateBestMatchingNeuron(inputVector5)),
                ("6", testObject.CalculateBestMatchingNeuron(inputVector6)),
                ("A", testObject.CalculateBestMatchingNeuron(testVectorA)),
                ("B", testObject.CalculateBestMatchingNeuron(testVectorB)),
                ("C", testObject.CalculateBestMatchingNeuron(testVectorC))
            );
        }
    }
}
