using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using ResourceModel;
using SelfOrganizingMap;

namespace SofUi
{
    public class OutputNeuron : Label
    {
        public OutputNeuron()
        {
            Visible = true;

            Clear();
        }

        public void Clear()
        {
            Text = "";
            ForeColor = Color.LightGreen;
            BackColor = Color.Wheat;
            BestMatchingDocs.Clear();
            BestMatchingVector.Clear();
            BestMatchingNeurons.Clear();
        }

        public List<(int x, int y)> BestMatchingNeurons { get; set; } = new List<(int x, int y)>();
        public List<double[]> BestMatchingVector { get; set; } = new List<double[]>();
        public List<RawDocument> BestMatchingDocs { get; set; } = new List<RawDocument>();

        internal void UpdateColors(IEnumerable<string> keys)
        {
            BackColor = Color.White;
            ForeColor = Color.Azure;
            Text = BestMatchingNeurons.Count.ToString();
            if (BestMatchingVector.Any())
            {
                var c = BestMatchingVector.Last().Select(v => (int)(v * 256)).ToArray();
                if (c.Length >= 4)
                {
                    BackColor = Color.FromArgb(c[0], c[1], c[2], c[3]);
                    ForeColor = c[1] + c[2] + c[3] > 800 ? Color.Black : Color.Azure;
                }
                else if (c.Length >= 3)
                {
                    BackColor = Color.FromArgb(c[0], c[1], c[2], 0);
                    ForeColor = c[1] + c[2] > 256 ? Color.Black : Color.Azure;
                }
                else if (c.Length >= 2)
                {
                    BackColor = Color.FromArgb(c[0], c[1], 0, 0);
                    ForeColor = c[1] > 128 ? Color.Black : Color.Azure;
                }
                else
                {
                    BackColor = Color.FromArgb(c[0], 0, 0, 0);
                    ForeColor = Color.Azure;
                }

                System.Diagnostics.Trace.TraceInformation("BMU" + BestMatchingNeurons.Last().x + BestMatchingNeurons.Last().y);
                var doc = BestMatchingDocs.First();
                foreach (var key in keys)
                {
                    if (doc.TryGetValue(key, out var value))
                    {
                        Text += ", " + value.ToString().PadRight(2).Substring(0, 2);
                    }
                }
            }
        }

        public string ToolTip(Collector collector)
        {
            var lines = new Dictionary<string, int>();
            foreach (var doc in BestMatchingDocs)
            {
                var line = new StringBuilder();
                foreach (var key in collector.Keys)
                {
                    line.Append(GetValue(doc, key)).Append(", ");
                }

                if (lines.ContainsKey(line.ToString()))
                {
                    lines[line.ToString()]++;
                }
                else
                {
                    lines.Add(line.ToString(), 1);
                }
            }

            var toolTip = new StringBuilder();
            foreach (var (key, value) in lines.OrderByDescending(x => x.Value))
            {
                toolTip.AppendLine(value + " : " + key);
            }

            return toolTip.ToString();
        }

        private static string GetValue(RawDocument doc, string key)
        {
            if (doc.TryGetValue(key, out var value)) return value as string;

            return "-";
        }
    }
}