using Newtonsoft.Json;
using SelfOrganizingMap;
using SOFM.Tests;
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SofUi
{
    public partial class Form1 : Form
    {
        private int widthCount = 10;
        private int heightCount = 10;

        private OutputNeuron[][] _map;
        public Form1()
        {
            InitializeComponent();
            _map = new OutputNeuron[heightCount][];

            var topx = 20;
            var topy = 20;
            //// 800
            //// 20 10 10 10 10 10 10 10 20
            //// 20 760 / 7              20
            var widthNeuron = (Width - 40) / (widthCount);
            var heightNeuron = (Height - 60) / (heightCount);

            for (var y = 0; y < heightCount; y++)
            {
                _map[y] = new OutputNeuron[widthCount];
                for (var x = 0; x < widthCount; x++)
                {
                    var n = new OutputNeuron();
                    n.Parent = this;
                    n.Bounds = new Rectangle(topx + x * widthNeuron, topy + y * heightNeuron, widthNeuron - 2, heightNeuron - 2);
                    Controls.Add(n);
                    toolTip1.SetToolTip(n, "No match");
                    _map[y][x] = n;
                }
            }

            timer1.Interval = 1000;
            timer1.Tick += Timer1_Tick;

            // Enable timer.  
            timer1.Enabled = true;
        }

        private void Timer1_Tick(object sender, EventArgs e)
        {
            toolStripStatusLabel1.Text = status;
            Refresh();
        }

        public int count = 0;
        private SoMap _network;
        private Label neuron;
        int cnt = 0;
        private string status = "start";

        private void TestMap()
        {
            var stream = File.ReadAllText("c:\\temp\\10.txt");
            //var stream = File.ReadAllText("c:\\temp\\users.txt");
            var json = JsonConvert.DeserializeObject<RawSearchResult>(stream);

            var keys = new List<string> { "locationCountry", "locationCity", "locationAddress", "organization_title" };

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

            collector.SetList("locationCountry", new[]{
                "Sweden",
                "",
                "",
                "",
                "",
                "Norway",
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
            });


            int mapWidth = 10;
            int mapHeight = 10;
            var numberOfIterations = 10000;
            var LearningRate = 0.001;
            var map = new SoMap(
                width: widthCount,
                height: heightCount,
                inputDimension: keys.Count,
                numberOfIterations: numberOfIterations,
                learningRate: LearningRate);

            cnt = 0;
            //map.Iterate += Map_Iterate1;
            var vectors = collector.Documents.Select(doc => collector.ToNormalizedData(doc)).ToArray();

            map.Train(vectors);
            //var iteration = 0;
            //var learningRate = LearningRate;
            //var t = (iteration, learningRate);
            //while (t.iteration < numberOfIterations)
            //{
            //    t = map.TrainOne(vectors, t);
            //    if (cnt % 100 == 0)
            //    {
            //        toolStripStatusLabel1.Text = $"Train {cnt++}";
            //        Refresh();
            //    }
            //}


            cnt = 0;
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
            Refresh();
        }

        private void Map_Iterate1(object sender, EventArgs e)
        {
            cnt++;
            if (cnt % 100 != 0) return;

            status = $"Train {cnt}";
            Refresh();
        }


        private void Form1_Load(object sender, EventArgs e)
        {
            //Refresh();
        }

        private void Form1_Shown(object sender, EventArgs e)
        {
            TestMap();
        }
    }

    public class OutputNeuron : Label
    {
        public OutputNeuron()
        {
            Visible = true;

            Text = "";
            ForeColor = Color.LightGreen;
            BackColor = Color.Wheat;
        }

        public List<INeuron> Selected { get; set; }
        public string Id { get; set; }
        public List<INeuron> BestMatchingNeurons { get; set; } = new List<INeuron>();
        public List<IVector> BestMatchingVector { get; set; } = new List<IVector>();
        public List<RawDocument> BestMatchingDocs { get; set; } = new List<RawDocument>();

        internal void UpdateColors()
        {
            BackColor = Color.White;
            ForeColor = Color.Azure;
            if (BestMatchingVector.Any())
            {
                var c = BestMatchingVector.Last().Select(v => (int)(v * 256)).ToArray();
                BackColor = Color.FromArgb(c[0], c[1], c[2], c[3]);
                ForeColor = c[1] + c[2] + c[3] > 500 ? Color.Black : Color.Azure;
                System.Diagnostics.Trace.TraceInformation("BMU" + BestMatchingNeurons.Last().X + BestMatchingNeurons.Last().Y);
            }


            Text = BestMatchingNeurons.Count.ToString();
        }

        public string ToolTip()
        {
            var lines = new Dictionary<string, int>();
            foreach (var doc in BestMatchingDocs)
            {
                var line =
                    GetValue(doc, "locationCountry") + ", " +
                    GetValue(doc, "locationCity") + ", " +
                    GetValue(doc, "locationAddress") + ", " +
                    GetValue(doc, "organization_title");

                if (lines.ContainsKey(line))
                {
                    lines[line]++;
                }
                else
                {
                    lines.Add(line, 1);
                }
            }

            var toolTip = new StringBuilder();
            foreach (var line in lines.OrderByDescending(x => x.Value))
            {
                toolTip.AppendLine(line.Value + " : " + line.Key);
            }

            return toolTip.ToString();
        }

        private string GetValue(RawDocument doc, string key)
        {
            if (doc.TryGetValue(key, out var value)) return value as string;

            return "-";
        }
    }
}
