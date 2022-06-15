using SelfOrganizingMap;
using SOFM.Tests;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace SofUi
{
    public partial class Form1 : Form
    {
        private readonly int _widthCount = 10;
        private readonly int _heightCount = 10;

        private OutputNeuron[][] _map;

        public Form1()
        {
            InitializeComponent();
        }

        private void TestMap()
        {
            //var collector = new Collector("c:\\temp\\Sof10");
            var collector = new Collector("c:\\temp\\Sof1000");
            //var collector = new Collector("c:\\temp\\SofUnsorted10");
            //var collector = new Collector("c:\\temp\\SofAll");

            var numberOfIterations = 1000;
            var LearningRate = 0.1;
            var stepCounter = 10;
            var showNeuronsCounter = 100;

            toolStripProgressBar1.Minimum = 1;
            toolStripProgressBar1.Maximum = numberOfIterations;
            toolStripProgressBar1.Step = stepCounter;
            var map = new SoMap(
                width: _widthCount,
                height: _heightCount,
                inputDimension: collector.Keys.Count,
                numberOfIterations: numberOfIterations,
                learningRate: LearningRate);

            var cnt = 0;
            var vectors = collector.Documents.Select(doc => collector.ToNormalizedData(doc)).ToArray();

            var writer = new SoMapTrainer(map, vectors);
            while (writer.Train())
            {
                if (cnt % stepCounter == 0)
                {
                    toolStripStatusLabel1.Text = $"Train {cnt}";
                    toolStripProgressBar1.PerformStep();
                    Refresh();
                }
                if (cnt % showNeuronsCounter == 0)
                {
                    Test(collector, map);
                    Refresh();
                }
                Application.DoEvents();

                cnt++;
            }

            Test(collector, map);
            Refresh();
            //ShowCollector(collector);
        }


        protected override void OnResize(EventArgs e)
        {
            if (_map == null)
            {
                _map = new OutputNeuron[_heightCount][];
                for (var y = 0; y < _heightCount; y++)
                {
                    _map[y] = new OutputNeuron[_widthCount];
                    for (var x = 0; x < _widthCount; x++)
                    {
                        var n = new OutputNeuron { Parent = this };
                        Controls.Add(n);
                        toolTip1.SetToolTip(n, "No match");
                        _map[y][x] = n;
                    }
                }
            }

            var topx = 20;
            var topy = 20;
            var widthNeuron = (Width - 40) / (_widthCount);
            var heightNeuron = (Height - 80) / (_heightCount);

            for (var y = 0; y < _heightCount; y++)
            {
                for (var x = 0; x < _widthCount; x++)
                {
                    var n = _map[y][x];
                    n.Bounds = new Rectangle(topx + x * widthNeuron, topy + y * heightNeuron, widthNeuron - 2, heightNeuron - 2);
                }
            }
            base.OnResize(e);
        }

        private static void ShowCollector(Collector collector)
        {

            foreach (var key in collector.Keys)
            {
                var values = new StringBuilder();
                values.Append(key);
                values.AppendLine("[");
                foreach (var value in collector.GetList(key))
                {
                    var example = "";
                    try
                    {
                        RawDocument exampleDoc = null;
                        foreach (var doc in collector.Documents)
                        {
                            if (doc.TryGetValue(key, out var v1) && (string)v1 == value)
                            {
                                exampleDoc = doc;
                                break;
                            }
                        }

                        if (exampleDoc != null)
                        {
                            foreach (var key1 in collector.Keys)
                            {
                                if (exampleDoc.TryGetValue(key1, out var value1))
                                {
                                    example += $"{value1}, ";
                                }
                            }
                        }
                        else
                        {
                            example = "";
                        }
                    }
                    catch (Exception ex)
                    {
                        example = ex.Message;
                    }

                    values.Append($"\"{value}\", ");
                    if (!string.IsNullOrEmpty(example))
                    {
                        values.Append($"/* {example} */");
                    }
                    values.AppendLine();
                }
                values.AppendLine("]");
                values.AppendLine();
                MessageBox.Show(values.ToString());
            }
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
                _map[currentNeuron.Y][currentNeuron.X].UpdateColors(collector.Keys);
                toolTip1.SetToolTip(
                    _map[currentNeuron.Y][currentNeuron.X], _map[currentNeuron.Y][currentNeuron.X]
                        .ToolTip(collector));
            }
        }

        private void Form1_Shown(object sender, EventArgs e)
        {
            TestMap();
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            Application.Exit();
            base.OnClosing(e);
        }
    }
}
