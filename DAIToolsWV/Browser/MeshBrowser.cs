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
using DAIToolsWV.Render;

namespace DAIToolsWV.Browser
{
    public partial class MeshBrowser : Form
    {
        private List<DBAccess.RESInformation> ttlist = null;
        private List<DBAccess.RESInformation> ttprevlist = null;
        private Thread TresRefresh = null;
        public Render.Renderer renderer;

        public MeshBrowser()
        {
            InitializeComponent();
            RefreshRES();
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
            TresRefresh.Start("D456B149");
        }

        public void RefreshRESThread(object obj)
        {
            try
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
                List<DBAccess.RESInformation> tmp = new List<DBAccess.RESInformation>();
                foreach (DBAccess.RESInformation res in ttlist)
                    if (!res.isPatch)
                        tmp.Add(res);
                ttlist = tmp;
                this.Invoke(new Action(delegate
                {
                    status.Text = "Preparing...";
                    Application.DoEvents();
                    MakeMeshTree();
                }
                ));
                this.Invoke(new Action(delegate
                {
                    splitContainer4.Visible = true;
                    toolStrip4.Enabled = true;
                    status.Text = "Loaded " + ttlist.Count + " ressources";
                    renderer = new Render.Renderer();
                    renderer.Init(pictureBox1.Handle, pictureBox1.Width, pictureBox1.Height);
                    timer1.Enabled = true;
                }
                ));
            }
            catch (Exception)
            {
            }
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
                DBAccess.RESInformation ti = ttprevlist[n];
                DBAccess.BundleInformation buni = DBAccess.GetBundleInformationById(ti.bundlepath)[0];
                DBAccess.TOCInformation toci = DBAccess.GetTocInformationByIndex(buni.tocIndex);
                BinaryBundle b = new BinaryBundle();
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
                    b = new BinaryBundle(new MemoryStream(bundledata));
                    foreach (BinaryBundle.ResEntry res in b.ResList)
                        if (res._name == ti.resname)
                        {
                            resdata = res._data;
                            break;
                        }
                }
                hb2.ByteProvider = new DynamicByteProvider(resdata);
                Mesh mesh = new Mesh(new MemoryStream(resdata));
                foreach (Mesh.MeshLOD lod in mesh.header.LODs)
                {
                    byte[] id = lod.ChunkID;
                    byte[] data = new byte[0];
                    if (toci.incas)
                    {
                        DBAccess.ChunkInformation ci = DBAccess.GetChunkInformationById(id);
                        if (ci.sha1 == null)
                            continue;
                        data = SHA1Access.GetDataBySha1(ci.sha1);
                    }
                    else
                    {
                        byte t = id[0];
                        id[0] = id[3];
                        id[3] = t;
                        t = id[1];
                        id[1] = id[2];
                        id[2] = t;
                        t = id[6];
                        id[6] = id[7];
                        id[7] = t;
                        t = id[4];
                        id[4] = id[5];
                        id[5] = t;
                        foreach (BinaryBundle.ChunkEntry c in b.ChunkList)
                            if (Helpers.ByteArrayCompare(id, c.id))
                                data = c._data;
                        if (data.Length == 0)
                        {
                            DBAccess.ChunkInformation ci = DBAccess.GetChunkInformationById(id);
                            if (ci.sha1 == null)
                                continue;
                            data = SHA1Access.GetDataBySha1(ci.sha1);
                        }
                    }
                    mesh.LoadChunkData(lod, new MemoryStream(data));
                }
                MeshRenderObject mro = new MeshRenderObject(mesh);
                renderer.list.Clear();
                renderer.list.Add(mro);
                renderer.worldoffset = -mro.center;
                renderer.CamDistance = mro.min.Length() + mro.max.Length();
                status.Text = "Ready";
            }
            catch (Exception ex)
            {
                status.Text = "General error, after state '" + status.Text + "' : " + ex.Message;
            }
        }



        public void MakeMeshTree()
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

        private void toolStripButton19_Click(object sender, EventArgs e)
        {
            Helpers.SelectNext(toolStripTextBox1.Text, treeView5);
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

        private void context_Paint(object sender, PaintEventArgs e)
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
            while (t.Parent.Text != "")
            {
                t = t.Parent;
                path = t.Text + "/" + path;
            }
            timer1.Enabled = false;
            renderer.device.Dispose();
            renderer = null;
            Application.DoEvents();
            ContentTools.MeshTool ttool = new ContentTools.MeshTool();
            ttool.MdiParent = this.MdiParent;
            ttool.WindowState = FormWindowState.Maximized;
            ttool.Show();
            ttool.LoadById(path);
            this.Close();
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            renderer.Render();
        }

        private void pictureBox1_Resize(object sender, EventArgs e)
        {
            if (renderer == null)
                return;
            timer1.Enabled = false;
            renderer.Init(pictureBox1.Handle, pictureBox1.Width, pictureBox1.Height);
            timer1.Enabled = true;
        }
    }
}
