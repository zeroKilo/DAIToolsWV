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
            string text = toolStripTextBox1.Text;
            SelectNext(text);
        }

        private void SelectNext(string text)
        {
            if (toc == null)
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
    }
    
}
