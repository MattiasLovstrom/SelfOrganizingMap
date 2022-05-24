using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using SelfOrganizingMap;
using SOFM.Tests;

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

        public List<INeuron> BestMatchingNeurons { get; set; } = new List<INeuron>();
        public List<IVector> BestMatchingVector { get; set; } = new List<IVector>();
        public List<RawDocument> BestMatchingDocs { get; set; } = new List<RawDocument>();

        internal void UpdateColors()
        {
            BackColor = Color.White;
            ForeColor = Color.Azure;
            Text = BestMatchingNeurons.Count.ToString();
            if (BestMatchingVector.Any())
            {
                var c = BestMatchingVector.Last().Select(v => (int)(v * 256)).ToArray();
                BackColor = Color.FromArgb(c[0], c[1], c[2], c[3]);
                ForeColor = c[1] + c[2] + c[3] > 500 ? Color.Black : Color.Azure;
                System.Diagnostics.Trace.TraceInformation("BMU" + BestMatchingNeurons.Last().X + BestMatchingNeurons.Last().Y);
                var doc = BestMatchingDocs.First();
                foreach (var key in Form1.keys)
                {
                    if (doc.TryGetValue(key, out var value))
                    {
                        Text += ", " + value.ToString().PadRight(2).Substring(0, 2);
                    }
                }
            }
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