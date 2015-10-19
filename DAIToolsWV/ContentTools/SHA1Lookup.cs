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
using DAILibWV;
using DAILibWV.Frostbite;
using Be.Windows.Forms;

namespace DAIToolsWV.ContentTools
{
    public partial class SHA1Lookup : Form
    {
        public SHA1Lookup()
        {
            InitializeComponent();
        }

        public DBAccess.EBXInformation[] el;
        public DBAccess.RESInformation[] rl;
        public DBAccess.ChunkInformation[] cl;
        

        private void toolStripButton1_Click(object sender, EventArgs e)
        {
            Search();
        }

        private void Search()
        {
            rtb1.Text = rtb2.Text = rtb3.Text = "";
            string sha1 = toolStripTextBox1.Text.Trim().ToLower();
            if (sha1.Length != 40)
            {
                MessageBox.Show("Not valid SHA1!");
                return;
            }
            byte[] data = SHA1Access.GetDataBySha1(Helpers.HexStringToByteArray(sha1));
            if (data.Length == 0)
            {
                MessageBox.Show("SHA1 not found!");
                return;
            }
            hb1.ByteProvider = new DynamicByteProvider(data);
            el = DBAccess.GetEBXInformationBySHA1(sha1);
            rl = DBAccess.GetRESInformationBySHA1(sha1);
            cl = DBAccess.GetChunkInformationBySHA1(sha1);
            listBox1.Items.Clear();
            int count = 0;
            int l = GlobalStuff.FindSetting("gamepath").Length;
            foreach (DBAccess.EBXInformation ei in el)
                listBox1.Items.Add((count++) + " : " + ei.ebxname);
            listBox2.Items.Clear();
            count = 0;
            foreach (DBAccess.RESInformation ri in rl)
                listBox2.Items.Add((count++) + " : " + ri.resname);
            listBox3.Items.Clear();
            count = 0;
            foreach (DBAccess.ChunkInformation ci in cl)
                listBox3.Items.Add((count++) + " : id = " + Helpers.ByteArrayToHexString(ci.id));
        }

        private void toolStripButton2_Click(object sender, EventArgs e)
        {
            SaveFileDialog d = new SaveFileDialog();
            d.Filter = "*.bin|*.bin";
            if (d.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                MemoryStream m = new MemoryStream();
                for (int i = 0; i < (int)hb1.ByteProvider.Length; i++)
                    m.WriteByte(hb1.ByteProvider.ReadByte(i));
                File.WriteAllBytes(d.FileName, m.ToArray());
                MessageBox.Show("Done.");
            }

        }

        private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            rtb1.Text = "";
            int n = listBox1.SelectedIndex;
            if (n == -1)
                return;
            DBAccess.EBXInformation ebx = el[n];
            rtb1.AppendText("EBX Path    : " + ebx.ebxname + "\n");
            rtb1.AppendText("Bundle Path : " + ebx.bundlepath + "\n");
            rtb1.AppendText("TOC Path    : " + ebx.tocfilepath + "\n");
            rtb1.AppendText("In CAS      : " + ebx.incas + "\n");
            rtb1.AppendText("SHA1        : " + ebx.sha1 + "\n");
            rtb1.AppendText("BaseSHA1    : " + ebx.basesha1 + "\n");
            rtb1.AppendText("DeltaSHA1   : " + ebx.deltasha1 + "\n");
        }

        private void listBox2_SelectedIndexChanged(object sender, EventArgs e)
        {
            rtb2.Text = "";
            int n = listBox2.SelectedIndex;
            if (n == -1)
                return;
            DBAccess.RESInformation res = rl[n];
            rtb2.AppendText("EBX Path    : " + res.resname + "\n");
            rtb2.AppendText("Bundle Path : " + res.bundlepath + "\n");
            rtb2.AppendText("TOC Path    : " + res.tocfilepath + "\n");
            rtb2.AppendText("In CAS      : " + res.incas + "\n");
            rtb2.AppendText("SHA1        : " + res.sha1 + "\n");
            rtb2.AppendText("Res Type    : " + res.rtype + " = ");
            byte[] buff = Helpers.HexStringToByteArray(res.rtype);
            rtb2.AppendText(Helpers.GetResType(BitConverter.ToUInt32(buff, 0)) + "\n");
        }

        private void listBox3_SelectedIndexChanged(object sender, EventArgs e)
        {
            rtb3.Text = "";
            int n = listBox3.SelectedIndex;
            if (n == -1)
                return;
            DBAccess.ChunkInformation ci = cl[n];
            DBAccess.BundleInformation bi = DBAccess.GetBundleInformationByIndex(ci.bundleIndex);
            DBAccess.TOCInformation ti = DBAccess.GetTocInformationByIndex(bi.tocIndex);
            rtb3.AppendText("Bundle Path : " + bi.bundlepath + "\n");
            rtb3.AppendText("TOC Path    : " + ti.path + "\n");
            rtb3.AppendText("In CAS      : " + ti.incas + "\n");
            rtb3.AppendText("SHA1        : " + Helpers.ByteArrayToHexString(ci.sha1) + "\n");
        }

        private void toolStripButton3_Click(object sender, EventArgs e)
        {

            try
            {
                string input = Microsoft.VisualBasic.Interaction.InputBox("Please enter offset in decimal", "Find by offset and cas number", "");
                uint offset = Convert.ToUInt32(input);
                input = Microsoft.VisualBasic.Interaction.InputBox("Please enter cas number decimal", "Find by offset and cas number", "");
                uint casnr = Convert.ToUInt32(input);
                input = Microsoft.VisualBasic.Interaction.InputBox("Search base(b) or patch(p)?", "Find by offset and cas number", "b");
                CATFile cat = null;
                if (input != "p")
                    cat = new CATFile(GlobalStuff.FindSetting("gamepath") + "Data\\cas.cat");
                else
                    cat = new CATFile(GlobalStuff.FindSetting("gamepath") + "Update\\Patch\\Data\\cas.cat");
                byte[] sha1 = new byte[0];
                for (int i = 0; i < cat.lines.Count; i++)
                    if (cat.lines[i][7] == casnr && cat.lines[i][5] <= offset && cat.lines[i][5] + cat.lines[i][6] > offset)
                    {
                        MemoryStream m = new MemoryStream();
                        for (int j = 0; j < 5; j++)
                            Helpers.WriteLEUInt(m, cat.lines[i][j]);
                        sha1 = m.ToArray();
                        break;
                    }
                cat = null;
                if(sha1.Length == 0)
                {
                    MessageBox.Show("SHA1 not found!");
                    return;
                }
                toolStripTextBox1.Text = Helpers.ByteArrayToHexString(sha1);
                Thread t = new Thread(ThreadedSearch);
                t.Start();
                MessageBox.Show("Done.");
                return;
            }
            catch (Exception)
            {
            }
        }

        private void ThreadedSearch(object obj)
        {
            this.Invoke((MethodInvoker)delegate() { Search(); });
        }
    }
}
