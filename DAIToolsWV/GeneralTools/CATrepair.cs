using System;
using System.IO;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using DAILibWV.Frostbite;
using DAILibWV;

namespace DAIToolsWV.GeneralTools
{
    public partial class CATrepair : Form
    {
        public CATFile cat;

        public struct DupEntry
        {
            public int firstIdx;
            public List<int> dupIdx;
        }

        public List<DupEntry> DupList = null;
        public bool[] isDup = new bool[0];

        public CATrepair()
        {
            InitializeComponent();
        }

        private void checkCATToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenFileDialog d = new OpenFileDialog();
            d.Filter = "*.cat|*.cat";
            if (d.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                cat = new CATFile(d.FileName);
                Check();
            }
        }

        public void Check()
        {
            DupList = new List<DupEntry>();
            int count = cat.lines.Count;
            isDup = new bool[count];
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < count - 1; i++)
                if (!isDup[i])
                {
                    if (i % 100 == 0)
                    {
                        rtb1.Text = "Checking " + i + " / " + count;
                        Application.DoEvents();
                    }
                    DupEntry d = new DupEntry();
                    d.firstIdx = i;
                    d.dupIdx = new List<int>();
                    for (int j = i + 1; j < count; j++)
                    {
                        if (cat.lines[i][0] == cat.lines[j][0] &&
                           cat.lines[i][1] == cat.lines[j][1] &&
                           cat.lines[i][2] == cat.lines[j][2] &&
                           cat.lines[i][3] == cat.lines[j][3] &&
                           cat.lines[i][4] == cat.lines[j][4])
                        {
                            isDup[i] = isDup[j] = true;
                            d.dupIdx.Add(j);
                        }
                    }
                    if (d.dupIdx.Count != 0)
                    {
                        sb.Append("Found duplicate times " + d.dupIdx.Count + " for Index " + d.firstIdx + "\n");
                        DupList.Add(d);
                    }
                }
            rtb1.Text = "Checking " + count + " / " + count + "\n";
            rtb1.AppendText(sb.ToString());
            Application.DoEvents();
        }

        private void repairCATToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (cat == null || DupList == null || DupList.Count == 0)
                return;
            rtb1.AppendText("\nRepairing CAT by only using the latest duplicate...\nWriting Header...\n");
            Application.DoEvents();
            MemoryStream m = new MemoryStream();
            Helpers.WriteLEInt(m, 0x4E79616E);
            Helpers.WriteLEInt(m, 0x4E79616E);
            Helpers.WriteLEInt(m, 0x4E79616E);
            Helpers.WriteLEInt(m, 0x4E79616E);            
            int count = cat.lines.Count;
            rtb1.AppendText("Writing non-duplicates...\n");
            Application.DoEvents();
            for (int i = 0; i < count; i++)
                if (!isDup[i])
                {
                    Helpers.WriteLEUInt(m, cat.lines[i][0]);
                    Helpers.WriteLEUInt(m, cat.lines[i][1]);
                    Helpers.WriteLEUInt(m, cat.lines[i][2]);
                    Helpers.WriteLEUInt(m, cat.lines[i][3]);
                    Helpers.WriteLEUInt(m, cat.lines[i][4]);
                    Helpers.WriteUInt(m, cat.lines[i][5]);
                    Helpers.WriteUInt(m, cat.lines[i][6]);
                    Helpers.WriteUInt(m, cat.lines[i][7]);
                }
            rtb1.AppendText("Writing latest-of-duplicates...\n");
            Application.DoEvents();
            foreach (DupEntry d in DupList)
            {
                int idx = d.dupIdx[d.dupIdx.Count - 1];
                Helpers.WriteLEUInt(m, cat.lines[idx][0]);
                Helpers.WriteLEUInt(m, cat.lines[idx][1]);
                Helpers.WriteLEUInt(m, cat.lines[idx][2]);
                Helpers.WriteLEUInt(m, cat.lines[idx][3]);
                Helpers.WriteLEUInt(m, cat.lines[idx][4]);
                Helpers.WriteUInt(m, cat.lines[idx][5]);
                Helpers.WriteUInt(m, cat.lines[idx][6]);
                Helpers.WriteUInt(m, cat.lines[idx][7]);
            }
            File.WriteAllBytes(cat.MyPath, m.ToArray());
            rtb1.AppendText("Done.");
        }

    }
}
