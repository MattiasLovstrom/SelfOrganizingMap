using Microsoft.VisualStudio.TestTools.UnitTesting;
using SOFM;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Newtonsoft.Json;
using SelfOrganizingMap;
using JsonSerializer = System.Text.Json.JsonSerializer;
using System.Linq;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.DataCollection;
using ResourceModel;

namespace SOFM.Tests
{
    [TestClass()]
    public class IntegrationTests
    {

        public const double Sverige = 1;
        public const double Norge = 2;

        public const double Lulea = 10;
        public const double Stockholm = 7;
        public const double Goteborg = 4;
        public const double Malmo = 2;
        public const double Lund = 1;
        public const double Oslo = 6;


        [TestMethod]
        public void TestUsers()
        {
            var stream = File.ReadAllText("c:\\temp\\10.txt");
            //var stream = File.ReadAllText("c:\\temp\\users.txt");
            var json = JsonConvert.DeserializeObject<RawSearchResult>(stream);

            var keys = new List<string> { "locationCountry", "locationCity", "locationAddress", "organization_title" };

            var collector = new Collector(keys);

            foreach (var doclist in json.Components.DocLists)
            {
                foreach (var document in doclist.Documents)
                {
                    collector.Import(document);
                }
            }

            int mapWidth = 15;
            int mapHeight = 15;
            var map = new SoMap(
                width: mapWidth,
                height: mapHeight,
                inputDimension: keys.Count,
                numberOfIterations: 1000,
                learningRate: 0.01);

            map.Train(collector.Documents.Select(doc => collector.ToNormalizedData(doc)).ToArray());

            var result = new MapResults();
            foreach (var doc in collector.Documents)
            {
                result.Add(doc, map.CalculateBestMatchingNeuron(collector.ToNormalizedData(doc)));
            }

            foreach (var doc in collector.Documents)
            {
                Console.Out.WriteLine("-----");
                var currentNeuron = map.CalculateBestMatchingNeuron(collector.ToNormalizedData(doc));
                DisplayDoc(doc, keys, currentNeuron);
                Console.Out.WriteLine("=>");
                var ordered = result.OrderBy(currentNeuron);
                foreach (var mapResult in ordered.Take(5))
                {
                    DisplayDoc(mapResult.Doc, keys, mapResult.Bmn);
                }
            }

        }

        [TestMethod]
        public void TestUsersSortedLists()
        {
            var stream = File.ReadAllText("10.txt");
            var json = JsonConvert.DeserializeObject<RawSearchResult>(stream);

            var keys = new List<string> { "locationCountry", "locationCity", "locationAddress", "organization_title" };

            var collector = new Collector(keys);

            foreach (var doclist in json.Components.DocLists)
            {
                foreach (var document in doclist.Documents)
                {
                    collector.Import(document);
                }
            }

            collector.SetList("locationCountry", new[]{
                "Sweden",
                "",
                "",
                "Norway",
                "",
                "",
                "",
                "Estonia",
                "Latvia",
                "Lithuania"
                });

            collector.SetList("locationCity", new[]{
                "",
                "",
                "",
                "",
                "",
                "",
                "",
                "Stockholm",
                "Göteborg",
                "Malmö",
                "",
                "",
                "",
                "",
                "Oslo",
                "",
                "",
                "",
                "",
                "Tallinn",
                "Rīga",
                "Vilnius",
                "",
                "",
                "",
                "",
                "",
            });

            collector.SetList("locationAddress", new[]
            {
                "",
                "",
                "",
                "",
                "",
                "Stjärntorget 4", //Stockholm
                "Sankt Eriksgatan 32",//Stockholm
                "Östra Hamngatan 24",//Göteborg
                "Östergatan 39", // malmö
                "",
                "Filipstad Brygge 1", //Oslo
                "Tornimäe 2",           //Tornimäe
                "Vienības gatve 109",  //Rīga
                "J.Balčikonio g.3", //Vilnius
                "",
                "",
                "",
                "",
            });

            collector.SetList("organization_title", new[]
            {
                "",
                "",
                "",
                "",
                "",
                "",
                "",
                "",
                "",
                "",
                "",
                "Group Staff, Control & Support",
                "Corporate and Private Customer",
                "Large Corporates and Financial",
                "Investment Management",
                "Technology",
                "",
                "",
                "",
                "SEB Baltic",
                "",
                "",
                "",
                "",
                "",
                "",
            });


            int mapWidth = 15;
            int mapHeight = 15;
            var map = new SoMap(
                width: mapWidth,
                height: mapHeight,
                inputDimension: keys.Count,
                numberOfIterations: 10000,
                learningRate: 0.001);

            map.Train(collector.Documents.Select(doc => collector.ToNormalizedData(doc)).ToArray());

            var result = new MapResults();
            foreach (var doc in collector.Documents)
            {
                result.Add(doc, map.CalculateBestMatchingNeuron(collector.ToNormalizedData(doc)));
            }

            foreach (var doc in collector.Documents)
            {
                Console.Out.WriteLine("-----");
                var currentNeuron = map.CalculateBestMatchingNeuron(collector.ToNormalizedData(doc));
                DisplayDoc(doc, keys, currentNeuron);
                Console.Out.WriteLine("=>");
                var ordered = result.OrderBy(currentNeuron);
                foreach (var mapResult in ordered)
                {
                    DisplayDoc(mapResult.Doc, keys, mapResult.Bmn);
                }
            }

        }


        [TestMethod]
        public void TestCollector()
        {
            //var stream = File.ReadAllText("10.txt");
            var stream = File.ReadAllText("c:\\temp\\sofall\\data.json");
            var json = JsonConvert.DeserializeObject<RawSearchResult>(stream);

            var keys = new List<string> { "locationCountry", "locationCity", "locationAddress", "organization_title" };

            var collector = new Collector(keys);

            foreach (var doclist in json.Components.DocLists)
            {
                foreach (var document in doclist.Documents)
                {
                    collector.Import(document);
                }
            }

            foreach (var key in keys)
            {
                Console.Out.WriteLine(key);
                Console.Out.WriteLine("[");
                foreach (var value in collector.GetList(key))
                {
                    RawDocument exampleDoc = null;
                    string example = "";
                    try
                    {
                        exampleDoc = null;
                        foreach (var doc in collector.Documents)
                        {
                            if (doc.TryGetValue(key, out var v1) && (string)v1 == value)
                            {
                                exampleDoc = doc;
                                break;
                            }
                        }

                        foreach (var key1 in keys)
                        {
                            if (exampleDoc.TryGetValue(key1, out var value1))
                            {
                                example += $"{value1}, ";
                            }
                        }
                    }
                    catch(Exception ex)
                    {
                        example = ex.Message;
                    }
                    Console.Out.WriteLine($"\"{value}\", /* {example} */");
                }
                Console.Out.WriteLine("]");
                Console.Out.WriteLine();
            }

        }


        private void DisplayDoc(RawDocument doc, List<string> keys, INeuron bmu)
        {
            Console.Out.Write(bmu.X + ":" + bmu.Y + " ");
            foreach (var key in keys)
            {
                var value = "";
                if (doc.TryGetValue(key, out var v))
                {
                    value = (string)v;
                }

                Console.Out.Write(value + " ");
            }
            Console.Out.WriteLine();
        }

        [TestMethod]
        public void TestLocation()
        {
            var testObject = new SoMap(
                width: 5,
                height: 5,
                inputDimension: 2,
                numberOfIterations: 1000,
                learningRate: 0.01);

            var inputVector0 = new Vector { Sverige, Lulea };
            var inputVector1 = new Vector { Sverige, Stockholm };
            var inputVector2 = new Vector { Sverige, Goteborg };
            var inputVector3 = new Vector { Sverige, Malmo };
            var inputVector4 = new Vector { Sverige, Lund };
            var inputVector5 = new Vector { Norge, Oslo };
            var input = new[]
            {
                inputVector0,
                inputVector1,
                inputVector2,
                inputVector3,
                inputVector4,
                inputVector5
            };

            testObject.Train(input);


            //    testObject.CalculateBMU(inputVector1).SetId(1),
            //    testObject.CalculateBMU(inputVector2).SetId(2),
            //    testObject.CalculateBMU(inputVector3).SetId(3),
            //    testObject.CalculateBMU(inputVector4).SetId(4),
            //    testObject.CalculateBMU(inputVector5).SetId(5),
            //    testObject.CalculateBMU(inputVector6).SetId(6));

            //testObject.Display(
            //    testObject.CalculateBMU(testVector0).SetId(0),
            //    testObject.CalculateBMU(testVector1).SetId(1),
            //    testObject.CalculateBMU(testVector2).SetId(2)
            //);
        }

        [TestMethod()]
        public void SoMapTest1()
        {
            var testObject = new SoMap(
                width: 2,
                height: 2,
                inputDimension: 2,
                numberOfIterations: 1000,
                learningRate: 0.01);

            var inputVector0 = new Vector { 0, 0 };
            var inputVector1 = new Vector { 1, 0 };
            var inputVector2 = new Vector { 2, 0 };
            var inputVector3 = new Vector { 3, 0 };
            var inputVector4 = new Vector { 4, 0 };
            var inputVector5 = new Vector { 5, 0 };
            var inputVector6 = new Vector { 6, 0 };
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

            testObject.Train(input);
            for (var x = 0; x < 6; x++)
            {
                //Console.Out.WriteLine(testObject.CalculateBMU(new Vector {x, 0}).SetId($"{x}") + " ");
            }
            //testObject.Display(
            //    testObject.CalculateBMU(inputVector0).SetId(0),
            //    testObject.CalculateBMU(inputVector1).SetId(1),
            //    testObject.CalculateBMU(inputVector2).SetId(2),
            //    testObject.CalculateBMU(inputVector3).SetId(3),
            //    testObject.CalculateBMU(inputVector4).SetId(4),
            //    testObject.CalculateBMU(inputVector5).SetId(5),
            //    testObject.CalculateBMU(inputVector6).SetId(6));

            //testObject.Display(
            //    testObject.CalculateBMU(testVector0).SetId(0),
            //    testObject.CalculateBMU(testVector1).SetId(1),
            //    testObject.CalculateBMU(testVector2).SetId(2)
            //);
        }

        [TestMethod()]
        public void SoMapTest()
        {
            var testObject = new SoMap(
                width: 10,
                height: 10,
                inputDimension: 2,
                numberOfIterations: 1000,
                learningRate: 0.01);

            var inputVector0 = new Vector { 0, 0 };   // 1:1
            var inputVector1 = new Vector { 1, 1 };   // 3:5
            var inputVector2 = new Vector { 2, 2 };   // 9:5
            var inputVector3 = new Vector { 3, 3 };   // 5:0
            var inputVector4 = new Vector { 4, 4 };   // 7:0
            var inputVector5 = new Vector { 5, 5 };   // 7:2
            var inputVector6 = new Vector { 6, 6 };   // 9:2
            var testVectorA = new Vector { 2.1, 2.1 };// 8:5
            var testVectorB = new Vector { 2.2, 2.2 };// 4:3
            var testVectorC = new Vector { 1.9, 1.9 };// 9:5
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

            testObject.Train(input);
            Console.Out.WriteLine("input");
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
            Console.Out.WriteLine("tests");
            Console.Out.WriteLine(testObject.CalculateBestMatchingNeuron(testVectorA));
            Console.Out.WriteLine(testObject.CalculateBestMatchingNeuron(testVectorB));
            Console.Out.WriteLine(testObject.CalculateBestMatchingNeuron(testVectorC));
        }

        [TestMethod]
        public void SoMapSmallTest()
        {
            var testObject = new SoMap(
                width: 2,
                height: 2,
                inputDimension: 2,
                numberOfIterations: 1000,
                learningRate: 0.01);

            var inputVector0 = new Vector { 0, 0 };
            var inputVector1 = new Vector { 1, 1 };
            var inputVector2 = new Vector { 2, 2 };
            var inputVector3 = new Vector { 3, 3 };
            var inputVector4 = new Vector { 4, 4 };
            var inputVector5 = new Vector { 5, 5 };
            var inputVector6 = new Vector { 6, 6 };
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

            testObject.Train(input);

            Console.Out.WriteLine(testObject.CalculateBestMatchingNeuron(inputVector0));
            Console.Out.WriteLine(testObject.CalculateBestMatchingNeuron(inputVector1));
            Console.Out.WriteLine(testObject.CalculateBestMatchingNeuron(inputVector2));
            Console.Out.WriteLine(testObject.CalculateBestMatchingNeuron(inputVector3));
            Console.Out.WriteLine(testObject.CalculateBestMatchingNeuron(inputVector4));
            Console.Out.WriteLine(testObject.CalculateBestMatchingNeuron(inputVector5));
            Console.Out.WriteLine(testObject.CalculateBestMatchingNeuron(inputVector6));
        }
    }
}