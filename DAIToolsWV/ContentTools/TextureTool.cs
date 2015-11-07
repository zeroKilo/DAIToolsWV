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
using DAILibWV;
using DAILibWV.Frostbite;
using Be.Windows.Forms;

namespace DAIToolsWV.ContentTools
{
    public partial class TextureTool : Form
    {

        public DBAccess.TextureInformation[] til;
        public DBAccess.BundleInformation[] bil;
        public DBAccess.TOCInformation[] tocil;
        bool allInCas, hasBase, hasPatch, hasDLC;

        public TextureTool()
        {
            InitializeComponent();
        }

        private void toolStripButton1_Click(object sender, EventArgs e)
        {
            LoadById(toolStripTextBox1.Text);
        }

        public void LoadById(string id)
        {
            toolStripTextBox1.Text = id;
            til = DBAccess.GetTextureInformationsById(id);
            List<DBAccess.BundleInformation> tbil = new List<DBAccess.BundleInformation>();
            foreach (DBAccess.TextureInformation ti in til)
            {
                bool found = false;
                for (int i = 0; i < tbil.Count; i++)
                    if (tbil[i].index == ti.bundleIndex)
                    {
                        found = true;
                        break;
                    }
                if (!found)
                    tbil.Add(DBAccess.GetBundleInformationByIndex(ti.bundleIndex));
            }
            bil = tbil.ToArray();
            List<DBAccess.TOCInformation> ttocil = new List<DBAccess.TOCInformation>();
            foreach (DBAccess.BundleInformation bi in bil)
            {
                bool found = false;
                for (int i = 0; i < ttocil.Count; i++)
                    if (ttocil[i].index == bi.tocIndex)
                    {
                        found = true;
                        break;
                    }
                if (!found)
                    ttocil.Add(DBAccess.GetTocInformationByIndex(bi.tocIndex));
            }
            tocil = ttocil.ToArray();
            hasBase = hasPatch = hasDLC = false;
            allInCas = true;
            foreach (DBAccess.TOCInformation toci in tocil)
            {
                if (!toci.incas)
                    allInCas = false;
                if (!toci.path.ToLower().Contains("update"))
                    hasBase = true;
                else
                {
                    if (toci.path.ToLower().Contains("patch"))
                        hasPatch = true;
                    else
                        hasDLC = true;
                }
            }
            listBox1.Items.Clear();
            int count = 0;
            foreach (DBAccess.BundleInformation bi in bil)
            {
                DBAccess.TOCInformation ti = new DBAccess.TOCInformation();
                ti.path = "";
                foreach (DBAccess.TOCInformation t in tocil)
                    if (t.index == bi.tocIndex)
                        ti = t;
                listBox1.Items.Add((count++) + " : " + ti.path + " -> " + bi.bundlepath);
            }
            rtb1.Text = "";
            rtb1.AppendText("Name                : " + id + "\n");
            rtb1.AppendText("Bundle occurenes    : " + bil.Length + "\n");
            rtb1.AppendText("TOC occurenes       : " + tocil.Length + "\n");
            rtb1.AppendText("All in CAS          : " + allInCas + "\n");
            rtb1.AppendText("Occurs in base game : " + hasBase + "\n");
            rtb1.AppendText("Occurs in DLC       : " + hasDLC + "\n");
            rtb1.AppendText("Occurs in Patch     : " + hasPatch + "\n");
            if (bil.Length != 0)
                listBox1.SelectedIndex = 0;
        }

        private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            int n = listBox1.SelectedIndex;
            if (n == -1)
                return;
            try
            {
                statustext.Text = "Getting header infos from db...";
                Application.DoEvents();
                hb1.ByteProvider = new DynamicByteProvider(new byte[0]);
                if (File.Exists("tmp\\tmp.dds"))
                    File.Delete("tmp\\tmp.dds");

                DBAccess.BundleInformation buni = bil[n];
                DBAccess.TextureInformation ti = new DBAccess.TextureInformation();
                foreach (DBAccess.TextureInformation t in til)
                    if (t.bundleIndex == buni.index)
                        ti = t;
                DBAccess.TOCInformation toci = DBAccess.GetTocInformationByIndex(buni.tocIndex);
                byte[] resdata = new byte[0];
                if (toci.incas)
                {
                    statustext.Text = "Getting header data from sha1...";
                    Application.DoEvents();
                    resdata = SHA1Access.GetDataBySha1(ti.sha1);
                }
                else
                {
                    statustext.Text = "Getting header data from binary bundle...";
                    Application.DoEvents();
                    TOCFile toc = new TOCFile(toci.path);
                    byte[] bundledata = toc.ExportBundleDataByPath(buni.bundlepath);
                    BinaryBundle b = new BinaryBundle(new MemoryStream(bundledata));
                    foreach (BinaryBundle.ResEntry res in b.ResList)
                        if (res._name == ti.name)
                        {
                            resdata = res._data;
                            break;
                        }
                }
                hb1.ByteProvider = new DynamicByteProvider(resdata);
                statustext.Text = "Getting texture infos from db...";
                Application.DoEvents();
                TextureMetaResource tmr = new TextureMetaResource(resdata);
                DBAccess.ChunkInformation ci = DBAccess.GetChunkInformationById(tmr.chunkid);
                if (ci.bundleIndex == -1)
                    throw new Exception("no chunk info found in db");
                DBAccess.BundleInformation buni2 = DBAccess.GetBundleInformationByIndex(ci.bundleIndex);
                DBAccess.TOCInformation toci2 = DBAccess.GetTocInformationByIndex(buni2.tocIndex);
                byte[] texdata = new byte[0];
                if (toci2.incas)
                {
                    statustext.Text = "Getting texture data from sha1...";
                    Application.DoEvents();
                    texdata = SHA1Access.GetDataBySha1(ci.sha1);
                }
                else
                {
                    statustext.Text = "Getting texture data from binary bundle...";
                    Application.DoEvents();
                    TOCFile toc = new TOCFile(toci2.path);
                    byte[] bundledata = toc.ExportBundleDataByPath(buni2.bundlepath);
                    BinaryBundle b = new BinaryBundle(new MemoryStream(bundledata));
                    foreach (BinaryBundle.ChunkEntry chunk in b.ChunkList)
                        if (Helpers.MatchByteArray(chunk.id, ci.id))
                        {
                            texdata = chunk._data;
                            break;
                        }
                }
                hb2.ByteProvider = new DynamicByteProvider(texdata);
                statustext.Text = "Making Preview...";
                Application.DoEvents();
                MemoryStream m = new MemoryStream();
                tmr.WriteTextureHeader(m);
                m.Write(texdata, 0, texdata.Length);
                if (!Directory.Exists("tmp"))
                    Directory.CreateDirectory("tmp");
                File.WriteAllBytes("tmp\\tmp.dds", m.ToArray());
                try
                {
                    pb1.Image = DevIL.DevIL.LoadBitmap("tmp\\tmp.dds");
                    pb1.BringToFront();
                }
                catch (Exception)
                {
                    statustext.Text = "Error loading dds, after state '" + statustext.Text + "'";
                }
                statustext.Text = "Ready";
            }
            catch (Exception)
            {
                statustext.Text = "General error, after state '" + statustext.Text + "'";
            }
        }

        private void toolStripButton2_Click(object sender, EventArgs e)
        {
            if (toolStripButton2.Checked)
                pb1.SizeMode = PictureBoxSizeMode.StretchImage;
            else
                pb1.SizeMode = PictureBoxSizeMode.Normal;
        }

        private void createSingleModToolStripMenuItem_Click(object sender, EventArgs e)
        {
            int n = listBox1.SelectedIndex;
            if (n == -1)
                return;
            MessageBox.Show("Please select replacement texture (same size)");
            OpenFileDialog d = new OpenFileDialog();
            d.Filter = "*.dds|*.dds";
            if (d.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                byte[] data = File.ReadAllBytes(d.FileName);
                MessageBox.Show("Please select mod save location");
                SaveFileDialog d2 = new SaveFileDialog();
                d2.Filter = "*.DAIMWV|*.DAIMWV";
                if (d2.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    Mod mod = new Mod();
                    mod.jobs = new List<Mod.ModJob>();
                    Mod.ModJob mj = new Mod.ModJob();
                    mj.type = 0;
                    mj.bundlePaths = new List<string>();
                    mj.tocPaths = new List<string>();
                    int plen = GlobalStuff.FindSetting("gamepath").Length;
                    DBAccess.BundleInformation buni = bil[n];
                    DBAccess.TextureInformation ti = new DBAccess.TextureInformation();
                    foreach (DBAccess.TextureInformation t in til)
                        if (t.bundleIndex == buni.index)
                            ti = t;
                    DBAccess.TOCInformation toci = DBAccess.GetTocInformationByIndex(buni.tocIndex);
                    mj.respath = ti.name;
                    mj.bundlePaths.Add(buni.bundlepath);
                    mj.tocPaths.Add(toci.path.Substring(plen, toci.path.Length - plen));
                    MemoryStream m = new MemoryStream();
                    m.Write(data, 0x80, data.Length - 0x80);
                    mj.data = m.ToArray();
                    mod.jobs.Add(mj);
                    mod.Save(d2.FileName);
                    MessageBox.Show("Done.");
                }
            }
        }

        private void createForAllDupsModToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MessageBox.Show("Please select replacement texture (same size)");
            OpenFileDialog d = new OpenFileDialog();
            d.Filter = "*.dds|*.dds";
            if (d.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                byte[] data = File.ReadAllBytes(d.FileName);
                MessageBox.Show("Please select mod save location");
                SaveFileDialog d2 = new SaveFileDialog();
                d2.Filter = "*.DAIMWV|*.DAIMWV";
                if (d2.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    Mod mod = new Mod();
                    mod.jobs = new List<Mod.ModJob>();
                    Mod.ModJob mj = new Mod.ModJob();
                    mj.type = 0;
                    mj.bundlePaths = new List<string>();
                    mj.tocPaths = new List<string>();
                    int plen = GlobalStuff.FindSetting("gamepath").Length;
                    bool skip = false;
                    for (int i = 0; i < bil.Length; i++)
                    {
                        DBAccess.BundleInformation buni = bil[i];
                        DBAccess.TextureInformation ti = new DBAccess.TextureInformation();
                        foreach (DBAccess.TextureInformation t in til)
                            if (t.bundleIndex == buni.index)
                                ti = t;
                        DBAccess.TOCInformation toci = DBAccess.GetTocInformationByIndex(buni.tocIndex);
                        mj.respath = ti.name;
                        skip = false;
                        foreach (string p in mj.bundlePaths)
                            if (p == buni.bundlepath)
                            {
                                skip = true;
                                break;
                            }
                        if (!skip)
                            mj.bundlePaths.Add(buni.bundlepath);
                        string tpath = toci.path.Substring(plen, toci.path.Length - plen);
                        skip = false;
                        foreach (string p in mj.tocPaths)
                            if (p == tpath)
                            {
                                skip = true;
                                break;
                            }
                        if (tpath.ToLower().Contains("\\patch\\"))
                            skip = true;
                        if (!skip)
                            mj.tocPaths.Add(tpath);
                    }
                    MemoryStream m = new MemoryStream();
                    m.Write(data, 0x80, data.Length - 0x80);
                    mj.data = m.ToArray();
                    mod.jobs.Add(mj);
                    mod.Save(d2.FileName);
                    MessageBox.Show("Done.");
                }
            }
        }

        private void toolStripButton3_Click(object sender, EventArgs e)
        {
            if (File.Exists("tmp\\tmp.dds"))
            {
                SaveFileDialog d = new SaveFileDialog();
                d.Filter = "*.dds|*.dds";
                d.FileName = Path.GetFileName(toolStripTextBox1.Text.Replace("/", "\\")) + ".dds";
                if (d.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    File.Copy("tmp\\tmp.dds", d.FileName, true);
                    MessageBox.Show("Done.");
                }
            }
        }
    }
}
