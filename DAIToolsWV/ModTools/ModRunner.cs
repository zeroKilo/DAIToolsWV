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

namespace DAIToolsWV.ModTools
{
    public partial class ModRunner : Form
    {
        public Mod mod;
        public string outputPath;
        public List<int> selectedJobs;

        public ModRunner()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            OpenFileDialog d = new OpenFileDialog();
            d.Filter = "*.DAIMWV|*.DAIMWV";
            if (d.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                textBox2.Text = d.FileName;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog fbd = new FolderBrowserDialog();
            if (fbd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                textBox1.Text = fbd.SelectedPath + "\\";
        }

        private void toolStripButton2_Click(object sender, EventArgs e)
        {
            if (radioButton1.Checked)
            {
                outputPath = GlobalStuff.FindSetting("gamepath") + "Update\\Patch\\";
            }
            else
            {
                if (textBox1.Text == "")
                    return;
                outputPath = textBox1.Text;
            }
            listBox1.Items.Clear();
            int count = 0;
            foreach (Mod.ModJob mj in mod.jobs)
                listBox1.Items.Add((count++) + " : Job Type(" + mod.GetTypeName(mj.type) + ")");
            panel3.BringToFront();
        }

        private void toolStripButton3_Click(object sender, EventArgs e)
        {
            if (textBox2.Text == "")
                return;
            mod = new Mod();
            mod.Load(textBox2.Text);
            panel2.BringToFront();
        }

        private void toolStripButton1_Click(object sender, EventArgs e)
        {
            selectedJobs = new List<int>();
            for (int i = 0; i < mod.jobs.Count; i++)
                selectedJobs.Add(i);
            RunJobs();
        }

        private void toolStripButton4_Click(object sender, EventArgs e)
        {
            selectedJobs = new List<int>();
            foreach (int i in listBox1.SelectedIndices)
                selectedJobs.Add(i);
            RunJobs();
        }

        private void toolStripButton5_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            rtb1.Text = "";
            int n = listBox1.SelectedIndex;
            if (n == -1)
                return;
            Mod.ModJob mj = mod.jobs[n];
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("Mod Type :" + mod.GetTypeName(mj.type));
            switch (mj.type)
            {
                case 0:
                    sb.AppendLine("Affected ressource path: " + mj.respath);
                    sb.AppendLine("Affected bundle paths (" + mj.paths.Count + "):");
                    foreach (string p in mj.paths)
                        sb.AppendLine("\tBundle: " + p);
                    break;
                default:
                    return;
            }
            rtb1.Text = sb.ToString();
        }

        private void RunJobs()
        {
            if (selectedJobs.Count == 0)
                return;
            rtb2.Text = "";
            panel4.BringToFront();
            DbgPrint("Start running " + selectedJobs.Count + " job(s):");
            toolStripButton5.Enabled = false;
            foreach (int i in selectedJobs)
                RunJob(i);
            toolStripButton5.Enabled = true;
        }

        private void DbgPrint(string s)
        {
            rtb2.AppendText(DateTime.Now.ToLongTimeString() + " : " + s + "\n");
            rtb2.SelectionStart = rtb2.Text.Length;
            rtb2.SelectionLength = 0;
            rtb2.ScrollToCaret();
        }

        public void RunJob(int i)
        {
            Mod.ModJob mj = mod.jobs[i];
            DbgPrint("Starting job of type : " + mod.GetTypeName(mj.type));
            switch (mj.type)
            {
                case 0:
                    RunTextureJob(mj);
                    break;
                default:
                    DbgPrint("Unknown mod type, did nothing");
                    break;
            }
            DbgPrint("End of job");
        }

        public void RunTextureJob(Mod.ModJob mj)
        {

        }
    }
}
