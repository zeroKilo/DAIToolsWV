using System;
using System.IO;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
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
    public partial class EBXBrowser : Form
    {
        private DBAccess.EBXInformation[] ebxlist = null;
        private Thread TebxRefresh = null;
        private string lastpath = "";

        public EBXBrowser()
        {
            InitializeComponent();
        }

        private void EBXBrowser_Load(object sender, EventArgs e)
        {
            toolStripComboBox1.Items.Clear();
            toolStripComboBox1.Items.Add("CAS Patch Type - 0 (Normal/None)");
            toolStripComboBox1.Items.Add("CAS Patch Type - 1 (Replace)");
            toolStripComboBox1.Items.Add("CAS Patch Type - 2 (Delta)");
            toolStripComboBox1.SelectedIndex = 0;
            RefreshEBX();
        }

        private void EBXBrowser_FormClosing(object sender, FormClosingEventArgs e)
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
            GC.Collect();
        }

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

        private void toolStripTextBox2_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == (char)13)
                Helpers.SelectNext(toolStripTextBox2.Text, treeView4);
        }

        private void toolStripButton20_Click(object sender, EventArgs e)
        {
            Helpers.SelectNext(toolStripTextBox2.Text, treeView4);
        }

        private void toolStripButton1_Click(object sender, EventArgs e)
        {
            TreeNode t = treeView4.SelectedNode;
            if (t == null || t.Nodes == null || t.Nodes.Count != 0)
                return;
            string path = Helpers.GetPathFromNode(t, "/");
            path = path.Substring(1, path.Length - 1);
            MessageBox.Show("Please select replacement data");
            OpenFileDialog d = new OpenFileDialog();
            d.Filter = "*.bin|*.bin";
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
                    mj.type = 2;
                    mj.respath = path;
                    mj.data = data;
                    mj.bundlePaths = new List<string>();
                    mj.tocPaths = new List<string>();
                    int plen = GlobalStuff.FindSetting("gamepath").Length;
                    ebxstatus.Text = "Finding All References...";
                    DBAccess.EBXInformation[] ebxl = DBAccess.GetEBXInformationByPath(path);
                    ebxstatus.Text = "Creating Mod...";
                    foreach (DBAccess.EBXInformation ebx in ebxl)
                    {
                        bool found = false;
                        foreach (string p in mj.bundlePaths)
                            if (p == ebx.bundlepath)
                            {
                                found = true;
                                break;
                            }
                        if (!found)
                            mj.bundlePaths.Add(ebx.bundlepath);
                        found = false;
                        foreach(string p in mj.tocPaths)
                            if (p == ebx.tocfilepath.Substring(plen))
                            {
                                found = true;
                                break;
                            }
                        if (!found && !ebx.tocfilepath.ToLower().Contains("\\patch\\"))
                            mj.tocPaths.Add(ebx.tocfilepath.Substring(plen));
                    }
                    mod.jobs.Add(mj);
                    mod.Save(d2.FileName);
                    ebxstatus.Text = "Ready.";
                    MessageBox.Show("Done.");
                }
            }
        }
    }
}
