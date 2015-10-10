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
using DAILibWV;

namespace DAIToolsWV.Browser
{
    public partial class BundleBrowser : Form
    {
        private Thread TbundleRefresh = null;
        private DBAccess.BundleInformation[] blist = null;
        private List<DBAccess.BundleInformation> tblist = null;

        public BundleBrowser()
        {
            InitializeComponent();
        }

        private void BundleBrowser_Load(object sender, EventArgs e)
        {
            RefreshBundles();
        }

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

        private void BundleBrowser_FormClosing(object sender, FormClosingEventArgs e)
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

        private void toolStripButton9_Click(object sender, EventArgs e)
        {
            RefreshBundles();
        }

        private void toolStripButton10_Click(object sender, EventArgs e)
        {
            RefreshBundles();
        }

        private void toolStripButton21_Click(object sender, EventArgs e)
        {
            Helpers.SelectNext(toolStripTextBox3.Text, treeView3);
        }

        private void toolStripTextBox3_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == (char)13)
                Helpers.SelectNext(toolStripTextBox3.Text, treeView3);
        }

    }
}
