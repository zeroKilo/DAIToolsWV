using System;
using System.IO;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using DAILibWV.Frostbite;
using DAILibWV;
using Be.Windows.Forms;
using DevIL;

namespace DAIToolsWV
{
    public partial class ContentBrowser : Form
    {
        private DBAccess.BundleInformation[] blist = null;
        private DBAccess.EBXInformation[] ebxlist = null;
        private List<DBAccess.BundleInformation> tblist = null;
        private List<DBAccess.TextureInformation> ttlist = null;
        private List<DBAccess.TextureInformation> ttprevlist = null;

        private string lastpath = "";

        private Thread TbundleRefresh = null;
        private Thread TebxRefresh = null;
        private Thread TtexRefresh = null;

        public ContentBrowser()
        {
            InitializeComponent();
        }

        private void ContentBrowser_Load(object sender, EventArgs e)
        {
            toolStripComboBox1.Items.Clear();
            toolStripComboBox1.Items.Add("CAS Patch Type - 0 (Normal/None)");
            toolStripComboBox1.Items.Add("CAS Patch Type - 1 (Replace)");
            toolStripComboBox1.Items.Add("CAS Patch Type - 2 (Delta)");
            toolStripComboBox1.SelectedIndex = 0;
            RefreshReal();
            RefreshBundles();
            RefreshEBX();
            RefreshTextures();
        }

        private void ContentBrowser_FormClosing(object sender, FormClosingEventArgs e)
        {
            try
            {
                if (TbundleRefresh != null)
                {
                    TbundleRefresh.Abort();
                    TbundleRefresh.Join();
                }
            }
            catch (Exception)
            {
            }
            try
            {
                if (TebxRefresh != null)
                {
                    TebxRefresh.Abort();
                    TebxRefresh.Join();
                }
            }
            catch (Exception)
            {
            }
        }

        #region real file system

        private void RefreshReal()
        {
            bool pathlist = toolStripButton4.Checked;
            bool withBase = toolStripButton1.Checked;
            bool withDLC = toolStripButton2.Checked;
            bool withPatch = toolStripButton3.Checked;
            List<string> Files = new List<string>();
            Files.AddRange(DBAccess.GetFiles("tocfiles", withBase, withDLC, withPatch));
            Files.AddRange(DBAccess.GetFiles("sbfiles", withBase, withDLC, withPatch));
            Files.Sort();
            if (pathlist)
            {
                listBox1.Items.Clear();
                listBox1.Items.AddRange(Files.ToArray());
                listBox1.BringToFront();
            }
            else
            {
                treeView1.Nodes.Clear();
                TreeNode t = new TreeNode();
                foreach (string file in Files)
                    t = Helpers.AddPath(t, file, "", '\\');
                treeView1.Nodes.Add(t);
                t.Expand();
                treeView1.BringToFront();
            }
        }

        private void toolStripButton1_Click_1(object sender, EventArgs e)
        {
            RefreshReal();
        }

        private void toolStripButton2_Click(object sender, EventArgs e)
        {
            RefreshReal();
        }

        private void toolStripButton3_Click(object sender, EventArgs e)
        {
            RefreshReal();
        }

        private void toolStripButton4_Click_1(object sender, EventArgs e)
        {
            toolStripButton4.Checked = true;
            toolStripButton5.Checked = false;
            RefreshReal();
        }

        private void toolStripButton5_Click_1(object sender, EventArgs e)
        {
            toolStripButton5.Checked = true;
            toolStripButton4.Checked = false;
            RefreshReal();
        }

        private void treeView1_AfterSelect(object sender, TreeViewEventArgs e)
        {
            treeView2.Nodes.Clear();
            if (treeView1.SelectedNode == null)
                return;
            string path = Path.GetDirectoryName(GlobalStuff.FindSetting("gamepath")) + Helpers.GetPathFromNode(treeView1.SelectedNode);
            PreviewFile(path);
        }

        private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            int n = listBox1.SelectedIndex;
            if (n == -1)
                return;
            string path = GlobalStuff.FindSetting("gamepath") + listBox1.Items[n];
            PreviewFile(path);
        }

        public void PreviewFile(string path)
        {
            string ext = Path.GetExtension(path).ToLower();
            TOCFile toc;
            switch (ext)
            {
                case ".toc":
                    toc = new TOCFile(path);
                    if (toc != null && toc.lines != null)
                        Helpers.FillTreeFast(treeView2, toc.lines);
                    break;
                case ".sb":
                    string tocpath = Helpers.GetFileNameWithOutExtension(path) + ".toc";
                    if (File.Exists(tocpath))
                    {
                        toc = new TOCFile(tocpath);
                        if (toc != null && toc.lines != null && toc.lines.Count != 0)
                            foreach (BJSON.Field f in toc.lines[0].fields)
                                if (f.fieldname == "cas" && (bool)f.data)
                                {
                                    SBFile sb = new SBFile(path);
                                    if (sb != null && sb.lines != null)
                                        Helpers.FillTreeFast(treeView2, sb.lines);
                                    return;
                                }
                    }
                    break;
            }
        }

        #endregion

        #region internal file system

        private void RefreshBundles()
        {
            TbundleRefresh = new Thread(RefreshBundlesThread);
            TbundleRefresh.Start();
        }

        private void RefreshBundlesThread(object obj)
        {
            bool withBase, withDLC, withPatch, withIsBase, withIsDelta;
            withBase = withDLC = withPatch = withIsBase = withIsDelta = false;
            this.Invoke(new Action(delegate
            {
                bundletext.Text = "Refreshing...";
                splitContainer2.Visible = false;
                treeView3.Nodes.Clear();
                withBase = toolStripButton6.Checked;
                withDLC = toolStripButton7.Checked;
                withPatch = toolStripButton8.Checked;
                withIsBase = toolStripButton9.Checked;
                withIsDelta = toolStripButton10.Checked;
            }
            ));
            if (blist == null)
                blist = DBAccess.GetBundleInformation();
            tblist = new List<DBAccess.BundleInformation>();
            for (int i = 0; i < blist.Length; i++)
            {
                if (blist[i].isbasegamefile && withBase)
                    if ((blist[i].isdelta == withIsDelta) &&
                         (blist[i].isbase == withIsBase))
                        tblist.Add(blist[i]);
                if (blist[i].isDLC && withDLC)
                    if ((blist[i].isdelta == withIsDelta) &&
                         (blist[i].isbase == withIsBase))
                        tblist.Add(blist[i]);
                if (blist[i].isPatch && withPatch)
                    if ((blist[i].isdelta == withIsDelta) &&
                         (blist[i].isbase == withIsBase))
                        tblist.Add(blist[i]);
            }
            this.Invoke(new Action(delegate
            {
                bundletext.Text = "Preparing...";
                treeView3.Nodes.Clear();
                TreeNode t = new TreeNode();
                int count = 0;
                foreach (DBAccess.BundleInformation bundle in tblist)
                {
                    t = Helpers.AddPath(t, bundle.bundlepath, "", '/');
                    if (count++ % 1000 == 0)
                        Application.DoEvents();
                }
                bundletext.Text = "Bundles loaded: " + tblist.Count();
                treeView3.Nodes.Add(t);
                t.Expand();
                treeView3.BringToFront();
                splitContainer2.Visible = true;
            }
            ));
        }

        private void treeView3_AfterSelect(object sender, TreeViewEventArgs e)
        {
            TreeNode t = treeView3.SelectedNode;
            if (t == null || t.Nodes == null || t.Nodes.Count != 0)
                return;
            string path = Helpers.GetPathFromNode(t, "/");
            for (int i = 0; i < tblist.Count; i++)
                if (path.Contains(tblist[i].bundlepath))
                {
                    DBAccess.BundleInformation bi = tblist[i];
                    if (bi.isbase)
                        return;
                    TOCFile toc = new TOCFile(bi.filepath);
                    byte[] data = toc.ExportBundleDataByPath(bi.bundlepath);
                    Bundle b = null;
                    if (bi.incas)
                    {
                        List<BJSON.Entry> tmp = new List<BJSON.Entry>();
                        BJSON.ReadEntries(new MemoryStream(data), tmp);
                        b = Bundle.Create(tmp[0]);
                    }
                    else
                        b = Bundle.Create(data, true);
                    if (b == null)
                        return;
                    if (b.ebx == null)
                        b.ebx = new List<Bundle.ebxtype>();
                    if (b.res == null)
                        b.res = new List<Bundle.restype>();
                    if (b.chunk == null)
                        b.chunk = new List<Bundle.chunktype>();
                    int total = b.ebx.Count + b.res.Count + b.chunk.Count;
                    StringBuilder sb = new StringBuilder();
                    sb.AppendFormat("Total count : {0}\n", total);
                    sb.AppendFormat("EBX count   : {0}\n", b.ebx.Count);
                    sb.AppendFormat("RES count   : {0}\n", b.res.Count);
                    sb.AppendFormat("CHUNK count : {0}\n", b.chunk.Count);
                    rtb1.Text = sb.ToString();
                    DBAccess.BundleInformation[] dupBun = DBAccess.GetBundleInformationById(bi.bundlepath);
                    listBox3.Items.Clear();
                    int count = 0;
                    int l = GlobalStuff.FindSetting("gamepath").Length;
                    foreach (DBAccess.BundleInformation dup in dupBun)
                    {
                        DBAccess.TOCInformation ti = DBAccess.GetTocInformationByIndex(dup.tocIndex);
                        listBox3.Items.Add((count++) + " : " + ti.path.Substring(l, ti.path.Length - l) + " -> Delta:" + dup.isdelta + " Base:" + dup.isbase);
                    }
                }
        }

        private void toolStripButton6_Click(object sender, EventArgs e)
        {
            RefreshBundles();
        }

        private void toolStripButton7_Click(object sender, EventArgs e)
        {
            RefreshBundles();
        }

        private void toolStripButton8_Click(object sender, EventArgs e)
        {
            RefreshBundles();
        }

        private void contextMenuStrip1_Paint(object sender, PaintEventArgs e)
        {
            bool istoc = false;
            if (toolStripButton4.Checked)
            {
                int n = listBox1.SelectedIndex;
                if (n == -1)
                    return;
                if (listBox1.Items[n].ToString().EndsWith(".toc"))
                    istoc = true;
            }
            else
            {
                TreeNode t = treeView1.SelectedNode;
                if (t == null)
                    return;
                if (t.Text.EndsWith(".toc"))
                    istoc = true;
            }
            if (istoc)
            {
                openInTOCToolToolStripMenuItem.Visible = true;
                openInSBToolToolStripMenuItem.Visible = false;
            }
            else
            {
                openInTOCToolToolStripMenuItem.Visible = false;
                openInSBToolToolStripMenuItem.Visible = true;
            }
        }

        private void openInTOCToolToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string path = Path.GetDirectoryName(GlobalStuff.FindSetting("gamepath"));
            string path2 = "";
            if (toolStripButton4.Checked)
            {
                int n = listBox1.SelectedIndex;
                if (n == -1)
                    return;
                path2 = listBox1.Items[n].ToString();
            }
            else
            {
                TreeNode t = treeView1.SelectedNode;
                if (t == null)
                    return;
                path2 = t.Text;
                while (t.Parent != null)
                {
                    t = t.Parent;
                    path2 = t.Text + "\\" + path2;
                }
            }
            if (!path2.StartsWith("\\"))
                path += "\\";
            path += path2;
            FileTools.TOCTool toc = new FileTools.TOCTool();
            toc.toc = new TOCFile(path);
            toc.RefreshTree();
            toc.MdiParent = this.MdiParent;
            toc.WindowState = FormWindowState.Maximized;
            toc.Show();
        }

        private void openInSBToolToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string path = Path.GetDirectoryName(GlobalStuff.FindSetting("gamepath"));
            string path2 = "";
            if (toolStripButton4.Checked)
            {
                int n = listBox1.SelectedIndex;
                if (n == -1)
                    return;
                path2 = listBox1.Items[n].ToString();
            }
            else
            {
                TreeNode t = treeView1.SelectedNode;
                if (t == null)
                    return;
                path2 = t.Text;
                while (t.Parent != null)
                {
                    t = t.Parent;
                    path2 = t.Text + "\\" + path2;
                }
            }
            if (!path2.StartsWith("\\"))
                path += "\\";
            path += path2;
            FileTools.SBTool sb = new FileTools.SBTool();
            sb.LoadFile(path);
            sb.MdiParent = this.MdiParent;
            sb.WindowState = FormWindowState.Maximized;
            sb.Show();

        }

        private void toolStripButton9_Click(object sender, EventArgs e)
        {
            RefreshBundles();
        }

        private void toolStripButton10_Click(object sender, EventArgs e)
        {
            RefreshBundles();
        }

        #endregion

        #region ebx

        public void RefreshEBX()
        {
            try
            {
                if (TebxRefresh != null)
                {
                    TebxRefresh.Abort();
                    TebxRefresh.Join();
                }
            }
            catch (Exception)
            {
            }
            TebxRefresh = new Thread(RefreshEBXThread);
            TebxRefresh.Start();
        }

        public void RefreshEBXThread(object obj)
        {
            Stopwatch sp = new Stopwatch();
            sp.Start();
            int casptype = 0;
            int waitcounter = 0;
            this.Invoke(new Action(delegate
            {
                splitContainer3.Visible = false;
                casptype = toolStripComboBox1.SelectedIndex;
                toolStrip3.Enabled = false;
            }
            ));
            if (ebxlist == null)
                ebxlist = DBAccess.GetEBXInformation(ebxstatus);
            this.Invoke(new Action(delegate
            {
                treeView4.Enabled = true;
                hb1.Enabled = true;
                treeView4.Nodes.Clear();
                TreeNode t = new TreeNode();
                int count = 0;
                foreach (DBAccess.EBXInformation ebx in ebxlist)
                    if (ebx.casPatchType == casptype)
                    {
                        t = Helpers.AddPath(t, ebx.ebxname, "", '/');
                        if (count++ % 10000 == 0)
                        {
                            ebxstatus.Text = "Preparing... " + Helpers.GetWaiter(waitcounter++);
                            Application.DoEvents();
                        }
                    }
                treeView4.Nodes.Add(t);
                t.Expand();
                sp.Stop();
                ebxstatus.Text = "Loaded " + count + " ebx in " + sp.ElapsedMilliseconds + " ms";
                splitContainer3.Visible = true;
                toolStrip3.Enabled = true;
            }
            ));
            sp.Stop();
        }

        private void CheckSelectionEBX()
        {
            TreeNode t = treeView4.SelectedNode;
            if (t == null || t.Nodes == null || t.Nodes.Count != 0)
                return;
            string path = Helpers.GetPathFromNode(t, "/");            
            path = path.Substring(1, path.Length - 1);
            if (path == lastpath)
                return;
            lastpath = path;
            foreach (DBAccess.EBXInformation ebx in ebxlist)
                if (path == ebx.ebxname && ebx.casPatchType != 2 && !ebx.isbase)
                {
                    string c = "b";
                    if (ebx.isDLC)
                        c = "u";
                    if (ebx.isPatch)
                        c = "p";
                    byte[] data = new byte[0];
                    if (ebx.incas)
                        data = SHA1Access.GetDataBySha1(Helpers.HexStringToByteArray(ebx.sha1));
                    else
                    {
                        TOCFile toc = new TOCFile(ebx.tocfilepath);
                        byte[] bundledata = toc.ExportBundleDataByPath(ebx.bundlepath);
                        BinaryBundle b = new BinaryBundle(new MemoryStream(bundledata));
                        foreach (BinaryBundle.EbxEntry ebx2 in b.EbxList)
                            if (path.Contains(ebx2._name))
                                data = ebx2._data;
                    }
                    if (toolStripButton11.Checked)
                    {
                        hb1.BringToFront();
                        hb1.ByteProvider = new DynamicByteProvider(data);
                    }
                    else if (toolStripComboBox1.SelectedIndex < 2)
                    {
                        rtb2.BringToFront();
                        rtb2.Text = "";
                        rtb2.Visible = false;
                        try
                        {
                            ebxstatus.Text = "Processing...";
                            Application.DoEvents();
                            EBXStream ebxf = new EBXStream(new MemoryStream(data), ebxpb1);
                            ebxstatus.Text = "Displaying...";
                            rtb2.Text = ebxf.toXML();
                            ebxstatus.Text = "Ready (Type is " + c + ")";
                        }
                        catch (Exception ex)
                        {
                            rtb2.Text = "Error:\n" + ex.Message;
                        }
                        rtb2.Visible = true;
                    }
                    return;
                }
        }

        private void treeView4_AfterSelect(object sender, TreeViewEventArgs e)
        {
            CheckSelectionEBX();
        }

        private void openInEBXToolToolStripMenuItem_Click(object sender, EventArgs e)
        {
            TreeNode t = treeView4.SelectedNode;
            if (t == null || t.Nodes == null || t.Nodes.Count != 0)
                return;
            string path = Helpers.GetPathFromNode(t, "/");
            path = path.Substring(1, path.Length - 1);
            foreach (DBAccess.EBXInformation ebx in ebxlist)
                if (path == ebx.ebxname && ebx.casPatchType == 0 && !ebx.isbase)
                {
                    byte[] data = new byte[0];
                    if (ebx.incas)
                        data = SHA1Access.GetDataBySha1(Helpers.HexStringToByteArray(ebx.sha1));
                    else
                    {
                        TOCFile toc = new TOCFile(ebx.tocfilepath);
                        byte[] bundledata = toc.ExportBundleDataByPath(ebx.bundlepath);
                        BinaryBundle b = new BinaryBundle(new MemoryStream(bundledata));
                        foreach (BinaryBundle.EbxEntry ebx2 in b.EbxList)
                            if (path == ebx2._name)
                                data = ebx2._data;
                    }
                    ContentTools.EBXTool ebxtool = new ContentTools.EBXTool();
                    ebxtool.MdiParent = this.MdiParent;
                    ebxtool.Show();
                    ebxtool.WindowState = FormWindowState.Maximized;
                    ebxtool.LoadEbx(data);
                    break;
                }
        }

        private void toolStripButton11_Click(object sender, EventArgs e)
        {
            toolStripButton11.Checked = true;
            toolStripButton12.Checked = false;
            CheckSelectionEBX();
        }

        private void toolStripButton12_Click(object sender, EventArgs e)
        {
            toolStripButton11.Checked = false;
            toolStripButton12.Checked = true;
            CheckSelectionEBX();
        }

        private void toolStripButton13_Click(object sender, EventArgs e)
        {
            RefreshEBX();
        }

        private void toolStripButton14_Click(object sender, EventArgs e)
        {
            SaveFileDialog d = new SaveFileDialog();
            d.Filter = "*.bin|*.bin";
            TreeNode t = treeView4.SelectedNode;
            if (t == null || t.Nodes == null || t.Nodes.Count != 0)
                return;
            string path = Helpers.GetPathFromNode(t, "/");
            path = path.Substring(1, path.Length - 1);
            d.FileName = t.Text + ".bin";
            if (d.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                foreach (DBAccess.EBXInformation ebx in ebxlist)
                    if (path == ebx.ebxname && ebx.casPatchType == 0 && !ebx.isbase)
                    {
                        byte[] data = new byte[0];
                        if (ebx.incas)
                            data = SHA1Access.GetDataBySha1(Helpers.HexStringToByteArray(ebx.sha1));
                        else
                        {
                            TOCFile toc = new TOCFile(ebx.tocfilepath);
                            byte[] bundledata = toc.ExportBundleDataByPath(ebx.bundlepath);
                            BinaryBundle b = new BinaryBundle(new MemoryStream(bundledata));
                            foreach (BinaryBundle.EbxEntry ebx2 in b.EbxList)
                                if (path == ebx2._name)
                                    data = ebx2._data;
                        }
                        File.WriteAllBytes(d.FileName, data);
                        MessageBox.Show("Done.");
                        return;
                    }
            }
        }

        private void toolStripButton15_Click(object sender, EventArgs e)
        {
            int errorcount = 0;
            FolderBrowserDialog fbd = new FolderBrowserDialog();
            if (fbd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                string basepath = fbd.SelectedPath + "\\";
                int casptype = toolStripComboBox1.SelectedIndex;
                int count = 0;
                foreach (DBAccess.EBXInformation ebx in ebxlist)
                    if (ebx.casPatchType == casptype)
                    {
                        byte[] data = new byte[0];
                        if (ebx.incas)
                            data = SHA1Access.GetDataBySha1(Helpers.HexStringToByteArray(ebx.sha1), 0x40);
                        else
                        {
                            TOCFile toc = new TOCFile(ebx.tocfilepath);
                            byte[] bundledata = toc.ExportBundleDataByPath(ebx.bundlepath);
                            BinaryBundle b = new BinaryBundle(new MemoryStream(bundledata));
                            foreach (BinaryBundle.EbxEntry ebx2 in b.EbxList)
                                if (ebx.ebxname == ebx2._name)
                                    data = ebx2._data;
                        }
                        EBXStream ex = new EBXStream(new MemoryStream(data));
                        if (!ex.ErrorLoading)
                        {
                            string subfilename = ebx.ebxname.Replace("/", "\\").Replace("'", "") + ".xml";
                            string subpath = Path.GetDirectoryName(subfilename) + "\\";
                            if (!Directory.Exists(basepath + subpath))
                                Directory.CreateDirectory(basepath + subpath);
                            File.WriteAllText(basepath + subfilename, ex.toXML());
                            if (count++ % 123 == 0)
                            {
                                ebxstatus.Text = "Writing #" + count + " " + basepath + subfilename + " ...";
                                Application.DoEvents();
                            }
                        }
                        else
                        {
                            errorcount++;
                            if (count++ % 123 == 0)
                            {
                                this.Text = "Errors exporting : " + errorcount.ToString() + " =(" + ((float)errorcount / (float)count) * 100f + "%) Last:" + ebx.ebxname;
                                Application.DoEvents();
                            }
                        }
                    }
                this.Text = "Content Browser";
                ebxstatus.Text = "Ready. Processed Ebx:" + count + " Errors: " + errorcount + " (" + ((float)errorcount / (float)count) * 100f + "%)";
            }
        }

        #endregion

        #region texture browser

        public void RefreshTextures()
        {
            try
            {
                if (TebxRefresh != null)
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
                statustextures.Text = "Loaded " + ttlist.Count +" textures";
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
            try
            {
                int n = listBox2.SelectedIndex;
                if (n == -1)
                    return;
                statustextures.Text = "Getting header infos from db...";
                Application.DoEvents();
                hb3.ByteProvider = new DynamicByteProvider(new byte[0]);
                hb3.BringToFront();
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

        #endregion

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

        private void toolStripButton20_Click(object sender, EventArgs e)
        {
            Helpers.SelectNext(toolStripTextBox2.Text, treeView4);
        }

        private void toolStripButton21_Click(object sender, EventArgs e)
        {
            Helpers.SelectNext(toolStripTextBox3.Text, treeView3);
        }

        private void toolStripTextBox1_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == (char)13)
                Helpers.SelectNext(toolStripTextBox1.Text, treeView5);
        }

        private void toolStripTextBox2_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == (char)13)
                Helpers.SelectNext(toolStripTextBox2.Text, treeView4);
        }

        private void toolStripTextBox3_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == (char)13)
                Helpers.SelectNext(toolStripTextBox3.Text, treeView3);
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
    }
}
