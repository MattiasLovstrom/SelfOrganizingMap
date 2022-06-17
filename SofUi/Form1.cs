using SelfOrganizingMap;
using SOFM.Tests;
using System;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace SofUi
{
    public partial class Form1 : Form
    {
        //private const string DataFolder = "c:\\temp\\Sof1000";
        //private const string DataFolder = "c:\\temp\\Sof10";
        private const string DataFolder = "c:\\temp\\somap10_country";
        //private const string DataFolder = "c:\\temp\\SofUnsorted10";
        //private const string DataFolder = "c:\\temp\\SofAll";

        private readonly int _widthCount = 10;
        private readonly int _heightCount = 10;

        private int _numberOfIterations = 1000;
        private double _learningRate = 0.1;
        private int stepCounter = 10;
        private int showNeuronsCounter = 100;

        private OutputNeuron[][] _outputNeurons;

        public Form1()
        {
            InitializeComponent();
        }

        private void TestMap()
        {
            var collector = new Collector(DataFolder);
            
            toolStripProgressBar1.Minimum = 1;
            toolStripProgressBar1.Maximum = _numberOfIterations;
            toolStripProgressBar1.Step = stepCounter;

            var map = SoMap.Load(DataFolder); 
            if (map == null)
            {
                map = TrainMap(collector);
                var resultFolder = $"{DataFolder}\\somap_{map.NumberOfIterations}_{map.Width}_{map.Height}_{DateTime.Now.ToString("yyyy-dd-M--HH-mm-ss")}";
                map.Save(resultFolder);
                collector.Save(resultFolder);
            }

            Test(collector, map);
            Refresh();
        }

        private SoMap TrainMap(Collector collector)
        {
            var map = new SoMap(
                width: _widthCount,
                height: _heightCount,
                inputDimension: collector.Keys.Count,
                numberOfIterations: _numberOfIterations,
                learningRate: _learningRate);

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

            return map;
        }


        protected override void OnResize(EventArgs e)
        {
            if (_outputNeurons == null)
            {
                _outputNeurons = new OutputNeuron[_heightCount][];
                for (var y = 0; y < _heightCount; y++)
                {
                    _outputNeurons[y] = new OutputNeuron[_widthCount];
                    for (var x = 0; x < _widthCount; x++)
                    {
                        var n = new OutputNeuron { Parent = this };
                        Controls.Add(n);
                        toolTip1.SetToolTip(n, "No match");
                        _outputNeurons[y][x] = n;
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
                    var n = _outputNeurons[y][x];
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
                    var n = _outputNeurons[y][x];
                    toolTip1.SetToolTip(n, "No match");
                    n.Clear();
                }
            }

            foreach (var doc in collector.Documents)
            {
                var currentNeuron = map.CalculateBestMatchingNeuron(collector.ToNormalizedData(doc));
                _outputNeurons[currentNeuron.Y][currentNeuron.X].BestMatchingDocs.Add(doc);
                _outputNeurons[currentNeuron.Y][currentNeuron.X].BestMatchingNeurons.Add(currentNeuron);
                _outputNeurons[currentNeuron.Y][currentNeuron.X].BestMatchingVector.Add(collector.ToNormalizedData(doc));
                _outputNeurons[currentNeuron.Y][currentNeuron.X].UpdateColors(collector.Keys);
                toolTip1.SetToolTip(
                    _outputNeurons[currentNeuron.Y][currentNeuron.X], _outputNeurons[currentNeuron.Y][currentNeuron.X]
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
