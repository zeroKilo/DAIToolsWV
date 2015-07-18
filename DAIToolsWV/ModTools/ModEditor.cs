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
    }
}
