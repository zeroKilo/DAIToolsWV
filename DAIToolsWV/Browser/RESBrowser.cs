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
    public partial class RESBrowser : Form
    {
        private List<DBAccess.RESInformation> ttlist = null;
        private List<DBAccess.RESInformation> ttprevlist = null;
        private Thread TresRefresh = null;
        private string[] types;

        public RESBrowser()
        {
            InitializeComponent();
            Init();
        }

        public void Init()
        {
            types = DBAccess.GetUsedRESTypes();
            List<string> tmp = new List<string>();
            foreach (string type in types)
                if (type != "")
                    tmp.Add(type);
            types = tmp.ToArray();
            toolStripComboBox1.Items.Clear();
            foreach (string type in types)                
                toolStripComboBox1.Items.Add(type + " - " + Helpers.GetResType(BitConverter.ToUInt32(Helpers.HexStringToByteArray(type), 0)));
            toolStripComboBox1.SelectedIndex = 0;
        }

        public void RefreshRES()
        {
            try
            {
                if (TresRefresh != null)
                {
                    TresRefresh.Abort();
                    TresRefresh.Join();
                }
            }
            catch (Exception)
            {
            }
            TresRefresh = new Thread(RefreshRESThread);
            TresRefresh.Start(types[toolStripComboBox1.SelectedIndex]);
        }

        public void RefreshRESThread(object obj)
        {
            string type = (string)obj;
            this.Invoke(new Action(delegate
            {
                splitContainer4.Visible = false;
                toolStrip4.Enabled = false;
                status.Text = "Refreshing...";
            }
            ));
            ttlist = new List<DBAccess.RESInformation>();
            ttlist.AddRange(DBAccess.GetRESInformationsByType(type));
            this.Invoke(new Action(delegate
            {
                status.Text = "Preparing...";
                Application.DoEvents();
                MakeTextureTree();
            }
            ));
            this.Invoke(new Action(delegate
            {
                splitContainer4.Visible = true;
                toolStrip4.Enabled = true;
                status.Text = "Loaded " + ttlist.Count + " ressources";
                toolStripComboBox1.Focus();
            }
            ));
        }

        public void MakeTextureTree()
        {
            treeView5.Nodes.Clear();
            TreeNode t = new TreeNode();
            foreach (DBAccess.RESInformation ti in ttlist)
                t = AddPath(t, ti.resname);
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

        private void RefreshPreview()
        {
            try
            {
                int n = listBox2.SelectedIndex;
                if (n == -1)
                    return;
                status.Text = "Getting header infos from db...";
                Application.DoEvents();
                if (!Directory.Exists("tmp"))
                    Directory.CreateDirectory("tmp");
                if (File.Exists("tmp\\tmp.dds"))
                    File.Delete("tmp\\tmp.dds");
                DBAccess.RESInformation ti = ttprevlist[n];
                DBAccess.BundleInformation buni = DBAccess.GetBundleInformationById(ti.bundlepath)[0];
                DBAccess.TOCInformation toci = DBAccess.GetTocInformationByIndex(buni.tocIndex);
                byte[] resdata = new byte[0];
                if (toci.incas)
                {
                    status.Text = "Getting header data from sha1...";
                    Application.DoEvents();
                    resdata = SHA1Access.GetDataBySha1(Helpers.HexStringToByteArray(ti.sha1));
                }
                else
                {
                    status.Text = "Getting header data from binary bundle...";
                    Application.DoEvents();
                    TOCFile toc = new TOCFile(toci.path);
                    byte[] bundledata = toc.ExportBundleDataByPath(buni.bundlepath);
                    BinaryBundle b = new BinaryBundle(new MemoryStream(bundledata));
                    foreach (BinaryBundle.ResEntry res in b.ResList)
                        if (res._name == ti.resname)
                        {
                            resdata = res._data;
                            break;
                        }
                }
                hb2.ByteProvider = new DynamicByteProvider(resdata);
                status.Text = "Ready";
            }
            catch (Exception ex)
            {
                status.Text = "General error, after state '" + status.Text + "' : " + ex.Message;
            }
        }

        private void toolStripComboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            RefreshRES();
        }

        private void treeView5_AfterSelect(object sender, TreeViewEventArgs e)
        {
            TreeNode t = treeView5.SelectedNode;
            if (t == null || t.Nodes.Count != 0)
                return;
            string path = t.Text;
            while (t.Parent.Text != "")
            {
                t = t.Parent;
                path = t.Text + "/" + path;
            }
            listBox2.Items.Clear();
            ttprevlist = new List<DBAccess.RESInformation>();
            string DAIpath = GlobalStuff.FindSetting("gamepath");
            foreach (DBAccess.RESInformation tmp in ttlist)
                if (tmp.resname == path)
                    ttprevlist.Add(tmp);
            List<DBAccess.RESInformation> tmplist = new List<DBAccess.RESInformation>();
            for (int i = 0; i < ttprevlist.Count; i++) 
            {
                if (ttprevlist[i].isPatch)
                    continue;
                bool found = false;
                for (int j = 0; j < tmplist.Count; j++)
                    if (tmplist[j].bundlepath == ttprevlist[i].bundlepath &&
                        tmplist[j].tocfilepath == ttprevlist[i].tocfilepath)
                    {
                        found = true;
                        break;
                    }
                if (!found)
                    tmplist.Add(ttprevlist[i]);
            }
            int count = 0;
            ttprevlist = tmplist;
            List<string> paths = new List<string>();
            foreach (DBAccess.RESInformation tmp in ttprevlist)
                paths.Add((count++) + " : " + tmp.tocfilepath.Substring(DAIpath.Length, tmp.tocfilepath.Length - DAIpath.Length) + " -> " + tmp.bundlepath);
            listBox2.Items.AddRange(paths.ToArray());
            if (listBox2.Items.Count > 0)
                listBox2.SelectedIndex = 0;
        }

        private void listBox2_SelectedIndexChanged(object sender, EventArgs e)
        {
            RefreshPreview();
        }

        private void toolStripButton18_Click(object sender, EventArgs e)
        {
            int n = listBox2.SelectedIndex;
            if (n == -1)
                return;
            DBAccess.RESInformation ti = ttprevlist[n];
            SaveFileDialog d = new SaveFileDialog();
            d.Filter = "*.bin|*.bin";
            d.FileName = Path.GetFileName(ti.resname.Replace("/", "\\"));
            if (d.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                MemoryStream m = new MemoryStream();
                for (long i = 0; i < hb2.ByteProvider.Length; i++)
                    m.WriteByte(hb2.ByteProvider.ReadByte(i));
                File.WriteAllBytes(d.FileName, m.ToArray());
                MessageBox.Show("Done.");
            }
        }

        private void createSingleModToolStripMenuItem_Click(object sender, EventArgs e)
        {
            int n = listBox2.SelectedIndex;
            if (n == -1)
                return;
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
                    mj.type = 1;
                    mj.bundlePaths = new List<string>();
                    mj.tocPaths = new List<string>();
                    int plen = GlobalStuff.FindSetting("gamepath").Length;
                    DBAccess.RESInformation resi = ttprevlist[n];                    
                    mj.respath = resi.resname;
                    mj.bundlePaths.Add(resi.bundlepath);
                    mj.tocPaths.Add(resi.tocfilepath.Substring(plen));
                    MemoryStream m = new MemoryStream();
                    m.Write(data, 0x80, data.Length - 0x80);
                    mj.data = m.ToArray();
                    mj.restype = types[toolStripComboBox1.SelectedIndex];                    
                    mod.jobs.Add(mj);
                    mod.Save(d2.FileName);
                    MessageBox.Show("Done.");
                }
            }
        }

        private void createForAllDupsModToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MessageBox.Show("Please select replacement data");
            OpenFileDialog d = new OpenFileDialog();
            d.Filter = "*.bin|*.bin";
            if (d.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                Mod mod = new Mod();
                mod.jobs = new List<Mod.ModJob>();
                Mod.ModJob mj = new Mod.ModJob();
                mj.data = File.ReadAllBytes(d.FileName);
                MessageBox.Show("Please select mod save location");
                SaveFileDialog d2 = new SaveFileDialog();
                d2.Filter = "*.DAIMWV|*.DAIMWV";
                if (d2.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    int plen = GlobalStuff.FindSetting("gamepath").Length;
                    mj.type = 1;
                    mj.bundlePaths = new List<string>();
                    mj.tocPaths = new List<string>();
                    bool skip = false;
                    for (int i = 0; i < ttprevlist.Count; i++)
                    {
                        DBAccess.RESInformation resi = ttprevlist[i];
                        mj.respath = resi.resname;
                        skip = false;
                        foreach (string p in mj.bundlePaths)
                            if (p == resi.bundlepath)
                            {
                                skip = true;
                                break;
                            }
                        if (!skip)
                            mj.bundlePaths.Add(resi.bundlepath);
                        skip = false;
                        foreach (string p in mj.tocPaths)
                            if (p == resi.tocfilepath.Substring(plen))
                            {
                                skip = true;
                                break;
                            }
                        if (!skip && !resi.tocfilepath.ToLower().Contains("\\patch\\"))
                            mj.tocPaths.Add(resi.tocfilepath.Substring(plen));
                    }
                    mj.restype = types[toolStripComboBox1.SelectedIndex];
                    mod.jobs.Add(mj);
                    mod.Save(d2.FileName);
                    MessageBox.Show("Done.");
                }
            }
        }
    }
}
