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
using DAILibWV.Frostbite;
using Be.Windows.Forms;
using DAILibWV;

namespace DAIToolsWV.Browser
{
    public partial class TextureBrowser : Form
    {
        private List<DBAccess.TextureInformation> ttlist = null;
        private List<DBAccess.TextureInformation> ttprevlist = null;
        private Thread TtexRefresh = null;

        public TextureBrowser()
        {
            InitializeComponent();
        }

        private void TextureBrowser_Load(object sender, EventArgs e)
        {
            RefreshTextures();
        }

        public void RefreshTextures()
        {
            try
            {
                if (TtexRefresh != null)
                {
                    TtexRefresh.Abort();
                    TtexRefresh.Join();
                }
            }
            catch (Exception)
            {
            }
            TtexRefresh = new Thread(RefreshtextureThread);
            TtexRefresh.Start();
        }

        public void RefreshtextureThread(object obj)
        {
            this.Invoke(new Action(delegate
            {
                splitContainer4.Visible = false;
                toolStrip4.Enabled = false;
                statustextures.Text = "Refreshing...";
            }
            ));
            ttlist = new List<DBAccess.TextureInformation>();
            ttlist.AddRange(DBAccess.GetTextureInformations());
            this.Invoke(new Action(delegate
            {
                statustextures.Text = "Preparing...";
                Application.DoEvents();
                MakeTextureTree();
            }
            ));
            this.Invoke(new Action(delegate
            {
                splitContainer4.Visible = true;
                toolStrip4.Enabled = true;
                statustextures.Text = "Loaded " + ttlist.Count + " textures";
            }
            ));
        }

        public void MakeTextureTree()
        {
            treeView5.Nodes.Clear();
            TreeNode t = new TreeNode("Textures");
            foreach (DBAccess.TextureInformation ti in ttlist)
                t = AddPath(t, ti.name);
            t.Expand();
            treeView5.Nodes.Add(t);
        }

        public TreeNode AddPath(TreeNode t, string path)
        {
            string[] parts = path.Split('/');
            TreeNode f = null;
            foreach (TreeNode c in t.Nodes)
                if (c.Text == parts[0])
                {
                    f = c;
                    break;
                }
            if (f == null)
            {
                f = new TreeNode(parts[0]);
                t.Nodes.Add(f);
            }
            if (parts.Length > 1)
            {
                string subpath = path.Substring(parts[0].Length + 1, path.Length - 1 - parts[0].Length);
                f = AddPath(f, subpath);
            }
            return t;
        }

        private void treeView5_AfterSelect(object sender, TreeViewEventArgs e)
        {
            TreeNode t = treeView5.SelectedNode;
            if (t == null || t.Nodes.Count != 0)
                return;
            string path = t.Text;
            while (t.Parent.Text != "Textures")
            {
                t = t.Parent;
                path = t.Text + "/" + path;
            }
            listBox2.Items.Clear();
            ttprevlist = new List<DBAccess.TextureInformation>();
            int count = 0;
            string DAIpath = GlobalStuff.FindSetting("gamepath");
            foreach (DBAccess.TextureInformation tmp in ttlist)
                if (tmp.name == path)
                {
                    ttprevlist.Add(tmp);
                    DBAccess.BundleInformation buni = DBAccess.GetBundleInformationByIndex(tmp.bundleIndex);
                    listBox2.Items.Add((count++) + " : " + buni.filepath.Substring(DAIpath.Length, buni.filepath.Length - DAIpath.Length) + " -> " + buni.bundlepath);
                }
            if (listBox2.Items.Count > 0)
                listBox2.SelectedIndex = 0;
        }

        private void listBox2_SelectedIndexChanged(object sender, EventArgs e)
        {
            RefreshPreview();
        }

        private void RefreshPreview()
        {
            try
            {
                int n = listBox2.SelectedIndex;
                if (n == -1)
                    return;
                statustextures.Text = "Getting header infos from db...";
                Application.DoEvents();
                hb3.ByteProvider = new DynamicByteProvider(new byte[0]);
                hb3.BringToFront();
                if (!Directory.Exists("tmp"))
                    Directory.CreateDirectory("tmp");
                if (File.Exists("tmp\\tmp.dds"))
                    File.Delete("tmp\\tmp.dds");
                DBAccess.TextureInformation ti = ttprevlist[n];
                DBAccess.BundleInformation buni = DBAccess.GetBundleInformationByIndex(ti.bundleIndex);
                DBAccess.TOCInformation toci = DBAccess.GetTocInformationByIndex(buni.tocIndex);
                byte[] resdata = new byte[0];
                if (toci.incas)
                {
                    statustextures.Text = "Getting header data from sha1...";
                    Application.DoEvents();
                    resdata = SHA1Access.GetDataBySha1(ti.sha1);
                }
                else
                {
                    statustextures.Text = "Getting header data from binary bundle...";
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
                hb2.ByteProvider = new DynamicByteProvider(resdata);
                statustextures.Text = "Getting texture infos from db...";
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
                    statustextures.Text = "Getting texture data from sha1...";
                    Application.DoEvents();
                    texdata = SHA1Access.GetDataBySha1(ci.sha1);
                }
                else
                {
                    statustextures.Text = "Getting texture data from binary bundle...";
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
                if (toolStripButton16.Checked)
                {
                    hb3.ByteProvider = new DynamicByteProvider(texdata);
                    hb3.BringToFront();
                }
                else
                {
                    statustextures.Text = "Making Preview...";
                    Application.DoEvents();
                    MemoryStream m = new MemoryStream();
                    tmr.WriteTextureHeader(m);
                    m.Write(texdata, 0, texdata.Length);
                    File.WriteAllBytes("tmp\\tmp.dds", m.ToArray());
                    try
                    {
                        pb1.Image = DevIL.DevIL.LoadBitmap("tmp\\tmp.dds");
                        pb1.BringToFront();
                    }
                    catch (Exception)
                    {
                        statustextures.Text = "Error loading dds, after state '" + statustextures.Text + "'";
                        hb3.ByteProvider = new DynamicByteProvider(texdata);
                        hb3.BringToFront();
                    }
                }
                statustextures.Text = "Ready";
            }
            catch (Exception)
            {
                statustextures.Text = "General error, after state '" + statustextures.Text + "'";
            }
        }

        private void toolStripButton17_Click(object sender, EventArgs e)
        {
            if (toolStripButton17.Checked)
                pb1.SizeMode = PictureBoxSizeMode.StretchImage;
            else
                pb1.SizeMode = PictureBoxSizeMode.Normal;
        }

        private void toolStripButton18_Click(object sender, EventArgs e)
        {
            int n = listBox2.SelectedIndex;
            if (n == -1)
                return;
            DBAccess.TextureInformation ti = ttprevlist[n];
            if (File.Exists("tmp\\tmp.dds"))
            {
                SaveFileDialog d = new SaveFileDialog();
                d.Filter = "*.dds|*.dds";
                d.FileName = Path.GetFileName(ti.name.Replace("/", "\\")) + ".dds";
                if (d.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    File.Copy("tmp\\tmp.dds", d.FileName, true);
                    MessageBox.Show("Done.");
                }
            }
        }

        private void toolStripButton19_Click(object sender, EventArgs e)
        {
            Helpers.SelectNext(toolStripTextBox1.Text, treeView5);
        }

        private void toolStripTextBox1_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == (char)13)
                Helpers.SelectNext(toolStripTextBox1.Text, treeView5);
        }

        private void openInTextureToolToolStripMenuItem_Click(object sender, EventArgs e)
        {
            TreeNode t = treeView5.SelectedNode;
            if (t == null || t.Nodes.Count != 0)
                return;
            string path = t.Text;
            while (t.Parent.Text != "Textures")
            {
                t = t.Parent;
                path = t.Text + "/" + path;
            }
            ContentTools.TextureTool ttool = new ContentTools.TextureTool();
            ttool.MdiParent = this.MdiParent;
            ttool.WindowState = FormWindowState.Maximized;
            ttool.Show();
            ttool.LoadById(path);
        }

        private void toolStripButton16_Click(object sender, EventArgs e)
        {
            RefreshPreview();           
        }

        private void texcontext_Paint(object sender, PaintEventArgs e)
        {
            TreeNode t = treeView5.SelectedNode;
            if (t == null || t.Nodes.Count != 0)
            {
                openInTextureToolToolStripMenuItem.Visible = false;
                nOPEToolStripMenuItem.Visible = true;
            }
            else
            {
                openInTextureToolToolStripMenuItem.Visible = true;
                nOPEToolStripMenuItem.Visible = false;
            }
        }
    }
}
