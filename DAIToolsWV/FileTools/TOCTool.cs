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

namespace DAIToolsWV.FileTools
{
    public partial class TOCTool : Form
    {
        public TOCFile toc;
        public string basepath;

        public TOCTool()
        {
            InitializeComponent();
        }

        private void openSingleToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenFileDialog d = new OpenFileDialog();
            d.Filter = "*.toc|*.toc";
            if (d.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                toc = new TOCFile(d.FileName);
                RefreshTree();
            }
        }

        public void RefreshTree()
        {
            treeView1.Nodes.Clear();
            if (toc != null && toc.lines != null)
            {
                foreach (BJSON.Entry e in toc.lines)
                    treeView1.Nodes.Add(BJSON.MakeEntry(new TreeNode(e.type.ToString("X")), e));
                Debug.LockWindowUpdate(treeView1.Handle);
                Helpers.ExpandTreeByLevel(treeView1.Nodes[0], 1);
                Debug.LockWindowUpdate(System.IntPtr.Zero);
                Application.DoEvents();
            }
        }

        private void saveAsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SaveFileDialog d = new SaveFileDialog();
            d.Filter = "*.toc|*.toc";
            if (d.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                toc.Save(d.FileName);
                MessageBox.Show("Done.");
            }

        }

        private void treeView1_DoubleClick(object sender, EventArgs e)
        {
            TreeNode t = treeView1.SelectedNode;
            if (t == null || (t.Nodes != null && t.Nodes.Count != 0))
                return;
            List<int> Indices = GetIndices(t);
            BJSON.Entry entry = toc.lines[Indices[0]];
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

        private void toolStripButton1_Click(object sender, EventArgs e)
        {
            Helpers.SelectNext(toolStripTextBox1.Text, treeView1);
        }

        private void contextMenuStrip1_Paint(object sender, PaintEventArgs e)
        {
            TreeNode t = treeView1.SelectedNode;
            if (t == null || t.Parent == null || t.Parent.Text != "bundles")
            {
                keepOnlyThisBundleToolStripMenuItem.Visible = 
                deleteBundleToolStripMenuItem.Visible = false;
                nOPEToolStripMenuItem.Visible = true;
                return;
            }
            keepOnlyThisBundleToolStripMenuItem.Visible =
            deleteBundleToolStripMenuItem.Visible = true;
            nOPEToolStripMenuItem.Visible = false;
        }

        private void deleteBundleToolStripMenuItem_Click(object sender, EventArgs e)
        {
            TreeNode t = treeView1.SelectedNode;
            if (t == null || t.Parent == null || t.Parent.Text != "bundles")
                return;
            int n = t.Index;
            toc.bundles.RemoveAt(n);
            BJSON.Entry root = toc.lines[0];
            BJSON.Field bundles = root.fields[0];
            List<BJSON.Entry> list = (List<BJSON.Entry>)bundles.data;
            list.RemoveAt(n);
            bundles.data = list;
            RefreshTree();
        }

        private void keepOnlyThisBundleToolStripMenuItem_Click(object sender, EventArgs e)
        {
            TreeNode t = treeView1.SelectedNode;
            if (t == null || t.Parent == null || t.Parent.Text != "bundles")
                return;
            int n = t.Index;
            TOCFile.TOCBundleInfoStruct bunInfo = toc.bundles[n];
            toc.bundles.Clear();
            toc.bundles.Add(bunInfo);
            BJSON.Entry root = toc.lines[0];
            BJSON.Field bundles = root.fields[0];
            List<BJSON.Entry> list = (List<BJSON.Entry>)bundles.data;
            BJSON.Entry entry = list[n];
            list.Clear();
            list.Add(entry);
            bundles.data = list;
            RefreshTree();
        }

        private void expandAllSubNodesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            TreeNode t = treeView1.SelectedNode;
            if (t == null)
                return;
            Debug.LockWindowUpdate(treeView1.Handle);
            t.ExpandAll();
            Debug.LockWindowUpdate(System.IntPtr.Zero);
        }

        private void toolStripTextBox1_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == (char)13)
                Helpers.SelectNext(toolStripTextBox1.Text, treeView1);
        }
    }
    
}
