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

namespace DAIToolsWV.FileTools
{
    public partial class SBTool : Form
    {
        public TOCFile toc;
        public SBFile sb;
        public CATFile cat_base, cat_patch;
        public CASFile cas;
        public BinaryBundle binBundle;
        public string basepath;

        public SBTool()
        {
            InitializeComponent();
        }

        private void opnSingleToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenFileDialog d = new OpenFileDialog();
            d.Filter = "*.sb|*.sb";
            if (d.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                LoadFile(d.FileName);
        }

        public void LoadFile(string path)
        {
            toc = new TOCFile(Helpers.GetFileNameWithOutExtension(path) + ".toc");
            if (toc.iscas)
            {
                toolStrip1.Visible = true;
                splitContainer2.BringToFront();
                sb = new SBFile(path);
                RefreshTree();
            }
            else
            {
                toolStrip1.Visible = false;
                tabControl1.BringToFront();
                RefreshBinary();
            }
        }

        public void RefreshBinary()
        {
            listBox1.Items.Clear();
            foreach(TOCFile.TOCBundleInfoStruct info in toc.bundles)
                listBox1.Items.Add(info.id + (info.isbase ? " (B)" : "") + (info.isdelta ? " (D)" : ""));
        }
        public void RefreshBinaryBundle()
        {
            if (binBundle == null)
                return;
            rtb2.Text = "";
            rtb2.AppendText("Magic             : 0x" + binBundle.Header.magic.ToString("X8") + "\n");
            rtb2.AppendText("Total Count       : " + binBundle.Header.totalCount + "\n");
            rtb2.AppendText("EBX Count         : " + binBundle.Header.ebxCount + "\n");
            rtb2.AppendText("RES Count         : " + binBundle.Header.resCount + "\n");
            rtb2.AppendText("CHUNK Count       : " + binBundle.Header.chunkCount + "\n");
            rtb2.AppendText("String Offset     : " + binBundle.Header.stringOffset + "\n");
            rtb2.AppendText("Chunk Meta Offset : " + binBundle.Header.chunkMetaOffset + "\n");
            rtb2.AppendText("Chunk Meta Size   : " + binBundle.Header.chunkMetaSize + "\n");
            listBox2.Items.Clear();
            foreach (BinaryBundle.EbxEntry ebx in binBundle.EbxList)
                listBox2.Items.Add(ebx._name);
            listBox3.Items.Clear();
            foreach (BinaryBundle.ResEntry res in binBundle.ResList)
                listBox3.Items.Add(res._name);
            listBox4.Items.Clear();
            foreach (BinaryBundle.ChunkEntry chunk in binBundle.ChunkList)
                listBox4.Items.Add(Helpers.ByteArrayToString(chunk.id));
            treeView2.Nodes.Clear();
            TreeNode t = new TreeNode("Meta");
            if (binBundle.ChunkMeta != null)
                t = BJSON.MakeField(t, binBundle.ChunkMeta);
            treeView2.Nodes.Add(t);
        }

        public void RefreshTree()
        {
            if (sb == null)
                return;
            treeView1.Nodes.Clear();
            foreach (BJSON.Entry e in sb.lines)
                treeView1.Nodes.Add(BJSON.MakeEntry(new TreeNode(e.type.ToString("X")), e));
            Debug.LockWindowUpdate(treeView1.Handle);
            Helpers.ExpandTreeByLevel(treeView1.Nodes[0], 1);
            Debug.LockWindowUpdate(System.IntPtr.Zero);
            Application.DoEvents();
        }

        private void toolStripButton1_Click(object sender, EventArgs e)
        {
            string text = toolStripTextBox1.Text;
            SelectNext(text);
        }

        private void SelectNext(string text)
        {
            if (sb == null)
                return;
            TreeNode t = treeView1.SelectedNode;
            if (t == null && treeView1.Nodes.Count != 0)
                t = treeView1.Nodes[0];
            while (true)
            {
                TreeNode t2 = FindNext(t, text);
                if (t2 != null)
                {
                    treeView1.SelectedNode = t2;
                    return;
                }
                else if (t.NextNode != null)
                    t = t.NextNode;
                else if (t.Parent != null && t.Parent.NextNode != null)
                    t = t.Parent.NextNode;
                else if (t.Parent != null && t.Parent.NextNode == null)
                    while (t.Parent != null)
                    {
                        t = t.Parent;
                        if (t.Parent != null && t.Parent.NextNode != null)
                        {
                            t = t.Parent.NextNode;
                            break;
                        }
                    }
                else
                    return;
            }
        }

        private TreeNode FindNext(TreeNode t, string text)
        {
            foreach (TreeNode t2 in t.Nodes)
            {
                if (t2.Text.Contains(text))
                    return t2;
                if (t2.Nodes.Count != 0)
                {
                    TreeNode t3 = FindNext(t2, text);
                    if (t3 != null)
                        return t3;
                }
            }
            return null;
        }

        private void toolStripButton2_Click(object sender, EventArgs e)
        {
            int n = toolStripComboBox1.SelectedIndex;
            if (n == -1)
                return;
            SelectNext(toolStripComboBox1.Items[n].ToString());
        }

        private void SBTool_Load(object sender, EventArgs e)
        {
            toolStripComboBox1.Items.Clear();
            foreach (KeyValuePair<uint, string> entry in Helpers.ResTypes)
                toolStripComboBox1.Items.Add("0x" + entry.Key.ToString("X8") + " " + entry.Value);
            toolStripComboBox1.SelectedIndex = 0;
        }

        private void treeView1_AfterSelect_1(object sender, TreeViewEventArgs e)
        {
            TreeNode t = treeView1.SelectedNode;
            hb1.BringToFront();
            hb1.ByteProvider = new DynamicByteProvider(new byte[0]);
            if (t == null) return;
            if (t.Text.ToLower().Contains("sha1"))
            {
                TreeNode t2 = t;
                if (t2 != null)
                {
                    string sha1 = t2.Nodes[0].Text;
                    byte[] sha1buff = Helpers.HexStringToByteArray(sha1);
                    byte[] data = SHA1Access.GetDataBySha1(sha1buff);
                    hb1.ByteProvider = new DynamicByteProvider(data);
                    hb1.BringToFront();
                    rtb1.Text = Helpers.DecompileLUAC(data);
                    if (rtb1.Text != "")
                        rtb1.BringToFront();
                }
            }
        }

        private void saveAsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (!toc.iscas)
                return;
            SaveFileDialog d = new SaveFileDialog();
            d.Filter = "*.sb|*.sb";
            if (d.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                sb.Save(d.FileName);
                MessageBox.Show("Done.");
            }
        }

        private void treeView1_DoubleClick(object sender, EventArgs e)
        {
            TreeNode t = treeView1.SelectedNode;
            if (t == null || (t.Nodes != null && t.Nodes.Count != 0))
                return;
            List<int> Indices = GetIndices(t);
            BJSON.Entry entry = sb.lines[Indices[0]];
            BJSON.Field field = null;
            byte swap = 0;
            for (int i = 1; i < Indices.Count - 1; i++)
            {
                if (i % 2 == swap)
                {
                    if (field.data is List<BJSON.Entry>)
                    {
                        List<BJSON.Entry> list = (List<BJSON.Entry>)field.data;
                        entry = list[Indices[i]];
                    }
                    if (field.data is List<BJSON.Field>)
                    {
                        List<BJSON.Field> list = (List<BJSON.Field>)field.data;
                        field = list[Indices[i]];
                        if (swap == 0)
                            swap = 1;
                        else
                            swap = 0;
                    }
                }
                else
                {
                    field = entry.fields[Indices[i]];
                }
            }
            if (field != null)
            {
                TOCTool_InputForm input = new TOCTool_InputForm();
                switch (field.type)
                {
                    case 1:
                        return;
                    case 7:
                        input.hb1.Enabled = false;
                        input.rtb1.Text = (string)field.data;
                        break;
                    case 6:
                        byte[] tmp = new byte[1];
                        if ((bool)field.data)
                            tmp[0] = 1;
                        input.hb1.ByteProvider = new FixedByteProvider(tmp);
                        input.rtb1.Enabled = false;
                        break;
                    default:
                        if (field.data is byte[])
                        {
                            input.hb1.ByteProvider = new DynamicByteProvider((byte[])field.data);
                            input.rtb1.Enabled = false;
                        }
                        break;
                }
                DialogResult res = input.ShowDialog();
                MemoryStream m;
                if (res == System.Windows.Forms.DialogResult.OK)
                {
                    switch (field.type)
                    {
                        case 7:
                            field.data = input.rtb1.Text;
                            break;
                        case 6:
                            m = new MemoryStream();
                            for (long i = 0; i < input.hb1.ByteProvider.Length; i++)
                                m.WriteByte(input.hb1.ByteProvider.ReadByte(i));
                            if (m.Length == 1)
                            {
                                m.Seek(0, 0);
                                if ((byte)m.ReadByte() == 0)
                                    field.data = false;
                                else
                                    field.data = true;
                            }
                            break;
                        default:
                            if (field.data is byte[])
                            {
                                m = new MemoryStream();
                                for (long i = 0; i < input.hb1.ByteProvider.Length; i++)
                                    m.WriteByte(input.hb1.ByteProvider.ReadByte(i));
                                field.data = m.ToArray();
                            }
                            break;
                    }
                    RefreshTree();
                }
            }
        }

        private List<int> GetIndices(TreeNode t)
        {
            List<int> result = new List<int>();
            if (t.Parent != null)
                result.AddRange(GetIndices(t.Parent));
            result.Add(t.Index);
            return result;
        }

        private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            int n = listBox1.SelectedIndex;
            if (n == -1)
                return;
            TOCFile.TOCBundleInfoStruct info = toc.bundles[n];
            if (info.isbase)
                return;
            byte[] data = toc.ExportBinaryBundle(info);
            binBundle = new BinaryBundle(new MemoryStream(data));
            RefreshBinaryBundle();
        }

        private void listBox2_SelectedIndexChanged(object sender, EventArgs e)
        {
            int n = listBox2.SelectedIndex;
            if (n == -1)
                return;
            BinaryBundle.EbxEntry ebx = binBundle.EbxList[n];
            hb2.ByteProvider = new DynamicByteProvider(ebx._data);
        }

        private void listBox3_SelectedIndexChanged(object sender, EventArgs e)
        {            
            int n = listBox3.SelectedIndex;
            if (n == -1)
                return;
            BinaryBundle.ResEntry res = binBundle.ResList[n];
            hb3.ByteProvider = new DynamicByteProvider(res._data);
        }

        private void listBox4_SelectedIndexChanged(object sender, EventArgs e)
        {

            int n = listBox4.SelectedIndex;
            if (n == -1)
                return;
            BinaryBundle.ChunkEntry chunk = binBundle.ChunkList[n];
            hb4.ByteProvider = new DynamicByteProvider(chunk._data);
        }
    }
}
