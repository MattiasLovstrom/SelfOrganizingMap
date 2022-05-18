using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using SelfOrganizingMap;

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
                    var n  = new OutputNeuron();
                    n.Bounds = new Rectangle(topx + x * widthNeuron, topy + y * heightNeuron, widthNeuron - 2, heightNeuron - 2);
                    Controls.Add(n);
                    _map[y][x] = n;
                }
            }
        }

        
        public int count = 0;
        private SoMap _network;
        private Label neuron;

        private void TestMap()
        {
            _network = new SoMap(
                width: widthCount,
                height: heightCount,
                inputDimension: 2,
                numberOfIterations: 1000,
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

            _network.Iterate += (sender, eventArgs) =>
            {
                count++;
                if (count % 1 == 0)
                {
                    DisplayInternal(
                        ("0", inputVector0),
                        ("1", inputVector1),
                        ("2", inputVector2),
                        ("3", inputVector3),
                        ("4", inputVector4),
                        ("5", inputVector5),
                        ("6", inputVector6),
                        ("A", testVectorA),
                        ("B", testVectorB),
                        ("C", testVectorC)
                    );
                    Refresh();
                    Thread.Sleep(200);
                }

            };

            _network.Train(input);
            //Console.Out.WriteLine("input");
            DisplayInternal(
                ("0", inputVector0),
                ("1", inputVector1),
                ("2", inputVector2),
                ("3", inputVector3),
                ("4", inputVector4),
                ("5", inputVector5),
                ("6", inputVector6),
                ("A", testVectorA),
                ("B", testVectorB),
                ("C", testVectorC)
            );
        }


        public void DisplayInternal(params (string id, IVector input)[] selected)
        {
            var selectedNeurons = selected.Select(idInput => _network.CalculateBestMatchingNeuron(idInput.input));
            //foreach (var valueTuple in selected)
            //{
            //    var bmu = _network.CalculateBestMatchingNeuron(valueTuple.input);
            //    var dist = valueTuple.input.Distances(bmu.Weights).ToArray();
            //    //_map[bmu.Y][bmu.X].Selected.Add(bmu);
            //    //_map[bmu.Y][bmu.X].Id = valueTuple.id;
            //}

            for (var i = 0; i < widthCount; i++)
            {
                for (var j = 0; j < heightCount; j++)
                {
                    var neuron = _map[j][i];
                    var sel = selectedNeurons.Where(n => n.X == i && n.Y == j);
                    if (sel.Any())
                    {
                        neuron.Selected = new List<INeuron>(sel);
                    }
                    else
                    {
                        neuron.Selected = new List<INeuron>();
                    }

                }
            }

            
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            //var g = e.Graphics;
            //g.DrawString(count.ToString(), new Font("Arial", 8), new SolidBrush(Color.AntiqueWhite), 0, 0);
            //var topx = 20;
            //var topy = 20;
            //// 800
            //// 20 10 10 10 10 10 10 10 20
            //// 20 760 / 7              20
            //var widthNeuron = (Width - 40) / (widthCount);
            //var heightNeuron = (Height - 60) / (heightCount);

            //for (var y = 0; y < heightCount; y++)
            //{
            //    for (var x = 0; x < widthCount; x++)
            //    {
            //        g.FillRectangle(new SolidBrush(_map[y][x].GetColor()), topx + x * widthNeuron, topy + y * heightNeuron, widthNeuron - 2, heightNeuron - 2);
            //        g.DrawString(_map[y][x].Id, new Font("Arial", 8), new SolidBrush(Color.Black), topx + x * widthNeuron, topy + y * heightNeuron);
            //    }
            //}

            base.OnPaint(e);
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            Refresh();
            
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
            ForeColor =Color.LightGreen;
        }

        public List<INeuron> Selected { get; set; }
        public string Id { get; set; }

        protected override void OnPaint(PaintEventArgs e)
        {
            
            if (Selected.Count() == 1)
            {
                BackColor = Color.LightGreen;
                Text = Selected.First().ToString();
            }
            else if (Selected.Count() > 1)
            {
                BackColor = Color.Green;
                Text = string.Join(',', Selected.Select(x => x.ToString()).ToArray());
            }
            else
            {
                BackColor = Color.White;
                Text = "";
            }
            base.OnPaint(e);
        }
    }
}
