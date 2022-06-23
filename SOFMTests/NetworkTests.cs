using Microsoft.VisualStudio.TestTools.UnitTesting;
using SelfOrganizingMap;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ResourceModel;

namespace SelfOrganizingMap.Tests
{
    [TestClass()]
    public class NetworkTests
    {
        [TestMethod()]
        public void CalculateBestMatchingNeuronTest()
        {
            var testObject = new Network(2, 2, 2);
            testObject._weights = new double[,] {
                {1.0,2.0},
                {2.0,4.0},
                {3.0,4.0},
                {4.0,4.0},
            };

            var inputs = new List<Vector>();
            var i = new Vector();
            i.Add(3.0);
            i.Add(4.0);
            inputs.Add(i);

            testObject.CalculateBestMatchingNeuron(inputs.ToArray());
        }

        [TestMethod()]
        public void CalculateBestMatchingNeuronTest1()
        {
            var testObject = new Network(2, 3, 3);
            testObject._weights = new double[,] {
                {1.0,2.0}, // 0 0
                {2.0,4.0}, // 1 0
                {3.0,5.0}, // 2 0
                {3.0,4.0}, // 0 1
                {3.0,4.0}, // 1 1
                {4.0,4.0}, // 2 1
                {4.0,4.0}, // 0 2
                {4.0,4.0}, // 1 2
                {4.0,4.0}, // 2 2
            };

            var inputs = new List<Vector>();
            var i = new Vector();
            i.Add(3.0);
            i.Add(4.0);
            inputs.Add(i);

            testObject.CalculateBestMatchingNeuron(inputs.ToArray());
        }

        [TestMethod()]
        public void CalculateBestMatchingNeuronTest2()
        {
            var testObject = new Network(2, 2, 2);
            testObject._weights = new double[,] {
                {1.0,2.0}, // 0 0
                {2.0,4.0}, // 1 0
                {2.0,4.0}, // 0 1
                {2.0,4.0}, // 1 1
                
            };

            var inputs = new List<Vector>();
            var i = new Vector();
            i.Add(2.0);
            i.Add(4.0);
            inputs.Add(i);

            testObject.CalculateBestMatchingNeuron(inputs.ToArray());
        }
    }
}