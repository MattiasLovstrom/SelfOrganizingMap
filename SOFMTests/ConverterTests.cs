using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;

namespace SelfOrganizingMap.Tests
{
    [TestClass()]
    public class ConverterTests
    {
        [TestMethod]
        public void SimpleTest()
        {
            var testObject = new Converter();
            
            testObject.Add(new []{ "c#", "c++"});
            var result = testObject.Read(new[] { "c#", "c++" }).ToArray();

            Assert.AreEqual(2, result.Length);
            Assert.AreEqual(0.5, result[0]);
            Assert.AreEqual(0.5, result[1]);
        }

        [TestMethod]
        public void ComplexTest()
        {
            var testObject = new Converter();

            testObject.Add(new[] { "c#", "c++" });
            Console.Out.WriteLine(testObject);
            testObject.Add(new[] { "c#", "java" });
            Console.Out.WriteLine(testObject);
            testObject.Add(new[] { "c++", "java" });
            Console.Out.WriteLine(testObject);
            // c#      c++
            //    java
            //         
            var result = testObject.Read(new[] { "c#", "c++" }).ToArray();
            // 0.5  0  0.5
            Assert.AreEqual(3, result.Length);
        }

        [TestMethod]
        public void IndexTest()
        {
            var testObject = new Converter();

            testObject.Add(new[] { "c#", "c++" });
            Console.Out.WriteLine(testObject);
            testObject.Add(new[] { "c#", "java" });
            Console.Out.WriteLine(testObject);
            testObject.Add(new[] { "c++", "java" });
            Console.Out.WriteLine(testObject);
            // c#      c++
            //    java
            //         
            var result = testObject.Read(new[] { "c#", "c++" }).ToArray();
            // 0.5  0  0.5
            Assert.AreEqual(3, result.Length);
        }
    }
}