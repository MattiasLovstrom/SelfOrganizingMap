using System;
using ResourceModel;

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

            
            // 00 --
            var input = new[,]
            {
                { 0, 0 },   // 1:1
                { 1, 1 },   // 3:5
                { 2, 2 },   // 9:5
                { 3, 3 },   // 5:0
                { 4, 4 },   // 7:0
                { 5, 5 },   // 7:2
                { 6, 6 },   // 9:2
                { 2.1, 2.1 },// 8:5
                { 2.2, 2.2 },// 4:3
                { 1.9, 1.9 }// 9:5
        };
            int i = 0;

            //testObject.Iterate += (sender, eventArgs) =>
            //{
            //    Console.SetCursorPosition(0, 0);
            //    Console.Out.WriteLine(i++);
            //    testObject.Display(
            //        ("0", testObject.Nn.CalculateBestMatchingNeuron(input[0)[0]),
            //        ("1", testObject.Nn.CalculateBestMatchingNeuron(input[1)[0]),
            //        ("2", testObject.Nn.CalculateBestMatchingNeuron(input[2)[0]),
            //        ("3", testObject.Nn.CalculateBestMatchingNeuron(input[3)[0]),
            //        ("4", testObject.Nn.CalculateBestMatchingNeuron(input[4)[0]),
            //        ("5", testObject.Nn.CalculateBestMatchingNeuron(input[5)[0]),
            //        ("6", testObject.Nn.CalculateBestMatchingNeuron(input[6)[0]),
            //        ("A", testObject.Nn.CalculateBestMatchingNeuron(testVectorA)[0]),
            //        ("B", testObject.Nn.CalculateBestMatchingNeuron(testVectorB)[0]),
            //        ("C", testObject.Nn.CalculateBestMatchingNeuron(testVectorC)[0])
            //    );
            //    //Thread.Sleep(2);
            //};

            //testObject.Train(input);
            //Console.Out.WriteLine("input");
            //testObject.Display(
            //    ("0", testObject.Nn.CalculateBestMatchingNeuron(new[] { inputVector0})[0]),
            //    ("1", testObject.Nn.CalculateBestMatchingNeuron(new[] { inputVector1})[0]),
            //    ("2", testObject.Nn.CalculateBestMatchingNeuron(new[] { inputVector2})[0]),
            //    ("3", testObject.Nn.CalculateBestMatchingNeuron(new[] { inputVector3})[0]),
            //    ("4", testObject.Nn.CalculateBestMatchingNeuron(new[] { inputVector4})[0]),
            //    ("5", testObject.Nn.CalculateBestMatchingNeuron(new[] { inputVector5})[0]),
            //    ("6", testObject.Nn.CalculateBestMatchingNeuron(new[] { inputVector6})[0]),
            //    ("A", testObject.Nn.CalculateBestMatchingNeuron(new[] { testVectorA })[0]),
            //    ("B", testObject.Nn.CalculateBestMatchingNeuron(new[] { testVectorB })[0]),
            //    ("C", testObject.Nn.CalculateBestMatchingNeuron(new[] { testVectorC })[0])
            //);
        }
    }
}
