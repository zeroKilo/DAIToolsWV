using System;
using System.IO;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using DAILibWV.OldSupport;
using Be.Windows.Forms;
using DAILibWV;
namespace DAIToolsWV.FileTools
{
    public partial class DAIMODSupportTool : Form
    {
        public DAIMODFile mod;

        public DAIMODSupportTool()
        {
            InitializeComponent();
        }

        private void openOldDAIMODToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenFileDialog d = new OpenFileDialog();
            d.Filter = "*.DAIMOD|*.DAIMOD";
            if (d.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                mod = new DAIMODFile(d.FileName);
                RefreshPreview();
            }
        }

        public void RefreshPreview()
        {
            rtb1.Text = mod.XML;
            rtb2.Text = mod.Script;
            listBox1.Items.Clear();
            for (int i = 0; i < mod.Data.Count; i++)
                listBox1.Items.Add("Data[" + i + "] : Size 0x" + mod.Data[i].Length.ToString("X8"));
        }

        private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            int n = listBox1.SelectedIndex;
            if (n == -1)
                return;
            hb1.ByteProvider = new DynamicByteProvider(mod.Data[n]);
        }

        private void toolStripButton1_Click(object sender, EventArgs e)
        {
            int n = listBox1.SelectedIndex;
            if (n == -1)
                return;
            SaveFileDialog d = new SaveFileDialog();
            d.Filter = "*.bin|*.bin";
            if (d.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                File.WriteAllBytes(d.FileName, mod.Data[n]);
                MessageBox.Show("Done.");
            }
        }
    }
}
