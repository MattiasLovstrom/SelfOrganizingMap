using Newtonsoft.Json;
using SelfOrganizingMap;
using SOFM.Tests;
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace SofUi
{
    public partial class Form1 : Form
    {
        public static List<string> keys = new List<string> { "locationCountry", "locationCity", "locationAddress", "organization_title" };

        private int _widthCount = 10;
        private int _heightCount = 10;

        private OutputNeuron[][] _map;

        public Form1()
        {
            InitializeComponent();
            _map = new OutputNeuron[_heightCount][];

            var topx = 20;
            var topy = 20;
            //// 800
            //// 20 10 10 10 10 10 10 10 20
            //// 20 760 / 7              20
            var widthNeuron = (Width - 40) / (_widthCount);
            var heightNeuron = (Height - 60) / (_heightCount);

            for (var y = 0; y < _heightCount; y++)
            {
                _map[y] = new OutputNeuron[_widthCount];
                for (var x = 0; x < _widthCount; x++)
                {
                    var n = new OutputNeuron();
                    n.Parent = this;
                    n.Bounds = new Rectangle(topx + x * widthNeuron, topy + y * heightNeuron, widthNeuron - 2, heightNeuron - 2);
                    Controls.Add(n);
                    toolTip1.SetToolTip(n, "No match");
                    _map[y][x] = n;
                }
            }
        }

        int cnt = 0;

        private void TestMap()
        {
            var stream = File.ReadAllText("c:\\temp\\10.txt");
            //var stream = File.ReadAllText("c:\\temp\\users.txt");
            var json = JsonConvert.DeserializeObject<RawSearchResult>(stream);

            var collector = new Collector(keys);

            foreach (var doclist in json.Components.Doclists)
            {
                foreach (var document in doclist.Documents)
                {
                    collector.Import(document);
                    toolStripStatusLabel1.Text = $"Reading {cnt++}";
                    if (cnt % 100 == 0) Refresh();
                }
            }

            collector.SetList("locationCountry", FieldsOrder10.LocationCountry);
            collector.SetList("locationCity", FieldsOrder10.LocationCity);
            collector.SetList("locationAddress", FieldsOrder10.LocationAddress);
            collector.SetList("organization_title", FieldsOrder10.OrganizationTitle);

            int mapWidth = 10;
            int mapHeight = 10;
            var numberOfIterations = 1000;
            var LearningRate = 0.01;
            var map = new SoMap(
                width: _widthCount,
                height: _heightCount,
                inputDimension: keys.Count,
                numberOfIterations: numberOfIterations,
                learningRate: LearningRate);

            cnt = 0;
            var vectors = collector.Documents.Select(doc => collector.ToNormalizedData(doc)).ToArray();

            //map.Train(vectors);

            var writer = new SoMapTrainer(map, vectors);
            while (writer.Train())
            {
                if (cnt % 1000 == 0)
                {
                    toolStripStatusLabel1.Text = $"Train {cnt}";
                    //Refresh();
                    Test(collector, map);
                }

                cnt++;
            }

            cnt = 0;
            Test(collector, map);
            Refresh();
        }

        private void Test(Collector collector, SoMap map)
        {
            for (var y = 0; y < _heightCount; y++)
            {
                for (var x = 0; x < _widthCount; x++)
                {
                    var n = _map[y][x];
                    toolTip1.SetToolTip(n, "No match");
                    n.Clear();
                }
            }

            foreach (var doc in collector.Documents)
            {
                var currentNeuron = map.CalculateBestMatchingNeuron(collector.ToNormalizedData(doc));
                _map[currentNeuron.Y][currentNeuron.X].BestMatchingDocs.Add(doc);
                _map[currentNeuron.Y][currentNeuron.X].BestMatchingNeurons.Add(currentNeuron);
                _map[currentNeuron.Y][currentNeuron.X].BestMatchingVector.Add(collector.ToNormalizedData(doc));
                _map[currentNeuron.Y][currentNeuron.X].UpdateColors();
                toolTip1.SetToolTip(_map[currentNeuron.Y][currentNeuron.X], _map[currentNeuron.Y][currentNeuron.X].ToolTip());
                cnt++;
                toolStripStatusLabel1.Text = $"Test {cnt}";
                Refresh();
            }
        }

        private void Form1_Shown(object sender, EventArgs e)
        {
            TestMap();
        }
    }
}
