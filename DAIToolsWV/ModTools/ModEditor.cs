using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using DAILibWV;
using Be.Windows.Forms;

namespace DAIToolsWV.ModTools
{
    public partial class ModEditor : Form
    {
        public Mod mod;
        public List<byte[]> dataList;

        public ModEditor()
        {
            InitializeComponent();
        }

        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenFileDialog d = new OpenFileDialog();
            d.Filter = "*.DAIMWV|*.DAIMWV";
            if (d.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                mod = new Mod();
                mod.Load(d.FileName);
                RefreshMe();
            }
        }

        public void RefreshMe()
        {
            if (mod == null)
                return;
            rtb1.Text = mod.headerXML;
            dataList = new List<byte[]>();
            listBox1.Items.Clear();
            int count = 0;
            foreach (Mod.ModJob mj in mod.jobs)
                switch (mj.type)
                {
                    case 0:
                        dataList.Add(mj.data);
                        listBox1.Items.Add("Data Blob #" + (count++));
                        break;
                }
        }

        private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            int n = listBox1.SelectedIndex;
            if (n == -1)
                return;
            hb1.ByteProvider = new DynamicByteProvider(dataList[n]);
        }

        private void importToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (mod == null)
                return;
            OpenFileDialog d = new OpenFileDialog();
            d.Filter = "*.DAIMWV|*.DAIMWV";
            if (d.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                Mod mi = new Mod();
                mi.Load(d.FileName);
                foreach (Mod.ModJob mj in mi.jobs)
                    mod.jobs.Add(mj);
                mod.CreateHeader();
                RefreshMe();
                MessageBox.Show("Done.");
            }
        }

        private void saveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (mod == null)
                return;
            SaveFileDialog d = new SaveFileDialog();
            d.Filter = "*.DAIMWV|*.DAIMWV";
            if (d.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                mod.headerXML = rtb1.Text;
                mod.Save(d.FileName, false);
                MessageBox.Show("Done.");
            }
        }
    }
}
