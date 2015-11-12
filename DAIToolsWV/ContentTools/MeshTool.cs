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

namespace DAIToolsWV.ContentTools
{
    public partial class MeshTool : Form
    {
        private List<DBAccess.RESInformation> ttlist = null;
        private List<DBAccess.RESInformation> ttprevlist = null;
        public DBAccess.BundleInformation[] bil;
        public DBAccess.TOCInformation[] tocil;
        private Thread TresRefresh = null;
        private bool hasLoaded = false;
        public Mesh mesh;
        public Renderer renderer;

        public MeshTool()
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
            string type = (string)obj;
            this.Invoke(new Action(delegate
            {
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
                status.Text = "Loaded " + ttlist.Count + " mesh information";
                hasLoaded = true;
                renderer = new Renderer();
                renderer.Init(pictureBox1.Handle, pictureBox1.Width, pictureBox1.Height);                
                timer1.Enabled = true;
            }
            ));
        }

        public void LoadById(string id)
        {
            hb1.ByteProvider = new DynamicByteProvider(new byte[0]);
            hb2.ByteProvider = new DynamicByteProvider(new byte[0]);
            toolStripTextBox1.Text = id;
            while (!hasLoaded)
                Application.DoEvents();
            listBox1.Items.Clear();
            ttprevlist = new List<DBAccess.RESInformation>();
            string DAIpath = GlobalStuff.FindSetting("gamepath");
            foreach (DBAccess.RESInformation tmp in ttlist)
                if (tmp.resname == id)
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
            listBox1.Items.AddRange(paths.ToArray());
            if (listBox1.Items.Count > 0)
                listBox1.SelectedIndex = 0;
        }

        private void toolStripButton1_Click(object sender, EventArgs e)
        {
            LoadById(toolStripTextBox1.Text);
        }

        private void RefreshPreview()
        {
            try
            {
                int n = listBox1.SelectedIndex;
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
                hb1.ByteProvider = new DynamicByteProvider(resdata);
                mesh = new Mesh(new MemoryStream(resdata));
                rtb1.Text = mesh.HeaderToStr();
                rtb2.Text = mesh.LODsToString();
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
                listBox2.Items.Clear();
                int count = 0;
                foreach (Mesh.MeshLOD l in mesh.header.LODs)
                    listBox2.Items.Add("LOD " + (count++) + " - Chunk-" + Helpers.ByteArrayToHexString(l.ChunkID));
                status.Text = "Ready";
            }
            catch (Exception ex)
            {
                status.Text = "General error, after state '" + status.Text + "' : " + ex.Message;
            }
        }

        private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            RefreshPreview();
        }

        private void listBox2_SelectedIndexChanged(object sender, EventArgs e)
        {
            int n = listBox2.SelectedIndex;
            if (n == -1)
                return;
            hb2.ByteProvider = new DynamicByteProvider(new byte[0]);
            Mesh.MeshLOD l = mesh.header.LODs[n];
            DBAccess.ChunkInformation ci = DBAccess.GetChunkInformationById(l.ChunkID);
            if (ci.sha1 == null)
                return;
            byte[] data = SHA1Access.GetDataBySha1(ci.sha1);
            hb2.ByteProvider = new DynamicByteProvider(data);
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

        private void toolStripButton2_Click(object sender, EventArgs e)
        {
            if (mesh == null)
                return;
            SaveFileDialog d = new SaveFileDialog();
            d.Filter = "*.psk|*.psk";
            if (d.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                PSKFile psk = new PSKFile(mesh);
                psk.Export(d.FileName);
                MessageBox.Show("Done.");
            }
        }
    }
}
