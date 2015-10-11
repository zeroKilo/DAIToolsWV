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
using DAILibWV.Frostbite;
using Be.Windows.Forms;
using DAILibWV;

namespace DAIToolsWV.ContentTools
{
    public partial class TalktableTool : Form
    {
        public Talktable talk = new Talktable();
        public byte[] raw;
        List<int> NodeList;
        List<StringID> StringList;
        List<uint> BitStream;

        public class HuffNode
        {
            public HuffNode e0;
            public HuffNode e1;
            public char c;
            public long w;
            public int index;
            public bool hasIndex;
            public HuffNode(char chr, long weight)
            {
                e0 = e1 = null;
                c = chr;
                w = weight;
                hasIndex = false;
            }
        }

        public class AlphaEntry
        {
            public bool[] list;
            public char c;
        }

        public class StringID
        {
            public uint ID;
            public uint offset;
        }
        public TalktableTool()
        {
            InitializeComponent();
        }

        private void openHelperstripMenuItem_Click(object sender, EventArgs e)
        {
            OpenFileDialog d = new OpenFileDialog();
            d.Filter = "*.bin|*.bin";
            if (d.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                try
                {
                    LoadData(new MemoryStream(File.ReadAllBytes(d.FileName)));
                    MessageBox.Show("Done");
                }
                catch (Exception)
                {
                    MessageBox.Show("Error on loading!");
                }
            }
        }

        public void LoadData(MemoryStream m)
        {
            talk = new Talktable();
            talk.Read(m);
            raw = m.ToArray();
            RefreshMe();
        }

        public void RefreshMe()
        {
            if (talk == null)
                return;
            hb1.ByteProvider = new DynamicByteProvider(raw);
            rtb1.Text = "";
            rtb1.AppendText("0x00 Magic\t\t\t: 0x" + talk.magic.ToString("X8") + "\n");
            rtb1.AppendText("0x04 Unk01\t\t\t: 0x" + talk.unk01.ToString("X8") + "\n");
            rtb1.AppendText("0x08 Dataoffset\t\t: 0x" + talk.DataOffset.ToString("X8") + "\n");
            rtb1.AppendText("0x0C Unk02\t\t\t: 0x" + talk.unk02.ToString("X8") + "\n");
            rtb1.AppendText("0x10 Unk03\t\t\t: 0x" + talk.unk03.ToString("X8") + "\n");
            rtb1.AppendText("0x14 Unk04\t\t\t: 0x" + talk.unk04.ToString("X8") + "\n");
            rtb1.AppendText("0x18 Data 1 Count\t\t: 0x" + talk.Data1Count.ToString("X8") + "\n");
            rtb1.AppendText("0x1C Data 1 Offset\t: 0x" + talk.Data1Offset.ToString("X8") + "\n");
            rtb1.AppendText("0x20 Data 2 Count\t\t: 0x" + talk.Data2Count.ToString("X8") + "\n");
            rtb1.AppendText("0x24 Data 2 Offset\t: 0x" + talk.Data2Offset.ToString("X8") + "\n");
            rtb1.AppendText("0x28 Data 3 Count\t\t: 0x" + talk.Data3Count.ToString("X8") + "\n");
            rtb1.AppendText("0x2C Data 3 Offset\t: 0x" + talk.Data3Offset.ToString("X8") + "\n");
            rtb1.AppendText("0x30 Data 4 Count\t\t: 0x" + talk.Data4Count.ToString("X8") + "\n");
            rtb1.AppendText("0x34 Data 4 Offset\t: 0x" + talk.Data4Offset.ToString("X8") + "\n");
            if (talk.Data4Count != 0)
            {
                rtb1.AppendText("0x18 Data 5 Count\t\t: 0x" + talk.Data5Count.ToString("X8") + "\n");
                rtb1.AppendText("0x1C Data 5 Offset\t: 0x" + talk.Data5Offset.ToString("X8") + "\n");
            }
            listBox1.Items.Clear();
            listBox2.Items.Clear();
            listBox3.Items.Clear();
            listBox4.Items.Clear();
            int offset = talk.Data1Offset;
            int count = 0;
            foreach (int i in talk.Data1)
            {
                listBox1.Items.Add((count++).ToString("d4") + ". " + offset.ToString("X8") + " : " + i.ToString());
                offset += 4;
            }
            offset = talk.Data2Offset;
            count = 0;
            for (int i = 0; i < talk.StringIDs.Count; i++)
            {
                listBox2.Items.Add((count++).ToString("d4") + ". " + offset.ToString("X8") + " ID : 0x" + (talk.StringIDs[i]).ToString("X8") + " Offset : 0x" + (talk.StringData[i]).ToString("X8"));
                offset += 8;
            }
            count = 0;
            foreach (STR line in talk.Strings)
                listBox3.Items.Add((count++).ToString("d4") + ". " + line.ID.ToString("X8") + " : " + line.Value);
            offset = talk.DataOffset;
            count = 0;
            foreach (uint u in talk.Data)
            {
                listBox4.Items.Add((count++).ToString("d4") + ". " + offset.ToString("X8") + " : " + uint2String(u));
                offset += 4;
            }
            int e = (talk.Data1.Count / 2) - 1;
            TreeNode t = new TreeNode(e.ToString());
            t = MakeTree(t, e);
            t.ExpandAll();
            treeView1.Nodes.Clear();
            treeView1.Nodes.Add(t);
        }

        public string uint2String(uint u)
        {
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < 32; i++)
                if ((u & (1 << i)) != 0)
                    sb.Append("1");
                else
                    sb.Append("0");
            return sb.ToString();
        }

        public TreeNode MakeTree(TreeNode t, int e)
        {
            int e1 = talk.Data1[e * 2];
            int e2 = talk.Data1[e * 2 + 1];
            TreeNode t1 = new TreeNode("0 : " + e1);
            if (e1 >= 0)
                t1 = MakeTree(t1, e1);
            else
                t1.Nodes.Add("'" + (char)(0xFFFF - e1) + "'");
            TreeNode t2 = new TreeNode("1 : " + e2);
            if (e2 >= 0)
                t2 = MakeTree(t2, e2);
            else
                t2.Nodes.Add("'" + (char)(0xFFFF - e2) + "'");
            t.Nodes.Add(t1);
            t.Nodes.Add(t2);
            return t;
        }

        private void generateHelperstripMenuItem_Click(object sender, EventArgs e)
        {
            Generate();
        }

        public void Generate()
        {
            long[] weights = new long[256];
            foreach (STR line in talk.Strings)
            {
                weights[0]++;   //string terminator for line
                foreach (char c in line.Value)
                    weights[(byte)c]++;
            }
            Dictionary<char, long> weighttable = new Dictionary<char, long>();
            for (int i = 0; i < 256; i++)
                if (weights[i] > 0)
                    weighttable.Add((char)i, weights[i]);
            List<HuffNode> nodes = new List<HuffNode>();
            foreach (KeyValuePair<char, long> w in weighttable)
                nodes.Add(new HuffNode(w.Key, w.Value));
            while (nodes.Count > 1)
            {
                bool run = true;
                while (run)
                {
                    run = false;
                    for (int i = 0; i < nodes.Count - 1; i++)
                        if (nodes[i].w > nodes[i + 1].w)
                        {
                            run = true;
                            HuffNode t = nodes[i];
                            nodes[i] = nodes[i + 1];
                            nodes[i + 1] = t;
                        }
                }
                HuffNode e0 = nodes[0];
                HuffNode e1 = nodes[1];
                HuffNode combine = new HuffNode(' ', e0.w + e1.w);
                combine.e0 = e0;
                combine.e1 = e1;
                nodes.RemoveAt(1);
                nodes.RemoveAt(0);
                nodes.Add(combine);
            }
            treeView2.Nodes.Clear();
            HuffNode h = nodes[0];
            TreeNode root = new TreeNode("root (generated from weights)");
            root = GenTree(root, h);
            treeView2.Nodes.Add(root);


            NodeList = new List<int>();
            while (!h.hasIndex)
                CalcIndex(h);
            listBox5.Items.Clear();
            int count = 0;
            foreach (int i in NodeList)
                listBox5.Items.Add((count++) + " : " + i.ToString("X4"));

            TreeNode root2 = new TreeNode("root (generated from flattening)");
            root2 = MakeTree2(root2, NodeList.Count / 2 - 1);
            root2.ExpandAll();
            treeView2.Nodes.Add(root2);

            AlphaEntry[] alphabet = GetAlphabet(h, new List<bool>());
            listBox6.Items.Clear();
            foreach (AlphaEntry a in alphabet)
            {
                StringBuilder sb = new StringBuilder();
                sb.Append("'");
                if ((byte)a.c != 0)
                    sb.Append(a.c);
                else
                    sb.Append("/0");
                sb.Append("' = ");
                foreach (bool b in a.list)
                    if (b)
                        sb.Append("1");
                    else
                        sb.Append("0");
                listBox6.Items.Add(sb.ToString());
            }
            BitStream = new List<uint>();
            StringList = new List<StringID>();
            uint curr = 0;
            uint index = 0;
            byte shift = 0;
            foreach (STR str in talk.Strings)
            {
                StringID t = new StringID();
                t.ID = str.ID;
                t.offset = index << 5;
                t.offset += shift;
                string s = str.Value + "\0";
                foreach (char c in s)
                {
                    AlphaEntry alpha = null;
                    foreach (AlphaEntry a in alphabet)
                        if ((byte)a.c == (byte)c)
                            alpha = a;
                    foreach (bool step in alpha.list)
                    {
                        byte b = 0;
                        if (step)
                            b = 1;
                        if (shift < 32)
                        {
                            curr += (uint)(b << shift);
                            shift++;
                        }
                        if (shift == 32)
                        {
                            BitStream.Add(curr);
                            index++;
                            shift = 0;
                            curr = 0;
                        }
                    }
                }
                StringList.Add(t);
            }
            BitStream.Add(curr);
            listBox7.Items.Clear();
            listBox8.Items.Clear();
            count = 0;
            for (int i = 0; i < StringList.Count; i++)
                listBox7.Items.Add((count++).ToString("d4") + ". ID : 0x" + (StringList[i].ID).ToString("X8") + " Offset : 0x" + StringList[i].offset.ToString("X8"));
            count = 0;
            foreach (uint u in BitStream)
                listBox8.Items.Add((count++).ToString("d4") + ". : " + uint2String(u));
        }

        public AlphaEntry[] GetAlphabet(HuffNode h, List<bool> list)
        {
            List<AlphaEntry> result = new List<AlphaEntry>();
            if (h.e0.e0 == null)
            {
                AlphaEntry e = new AlphaEntry();
                e.c = h.e0.c;
                List<bool> t = new List<bool>();
                t.AddRange(list);
                t.Add(false);
                e.list = t.ToArray();
                result.Add(e);
            }
            else
            {
                List<bool> t = new List<bool>();
                t.AddRange(list);
                t.Add(false);
                result.AddRange(GetAlphabet(h.e0, t));
            }
            if (h.e1.e0 == null)
            {
                AlphaEntry e = new AlphaEntry();
                e.c = h.e1.c;
                List<bool> t = new List<bool>();
                t.AddRange(list);
                t.Add(true);
                e.list = t.ToArray();
                result.Add(e);
            }
            else
            {
                List<bool> t = new List<bool>();
                t.AddRange(list);
                t.Add(true);
                result.AddRange(GetAlphabet(h.e1, t));
            }
            return result.ToArray();
        }

        public TreeNode MakeTree2(TreeNode t, int e)
        {
            int e1 = NodeList[e * 2];
            int e2 = NodeList[e * 2 + 1];
            TreeNode t1 = new TreeNode("0 : " + e1);
            if (e1 >= 0)
                t1 = MakeTree2(t1, e1);
            else
                t1.Nodes.Add("'" + (char)(0xFFFF - e1) + "'");
            TreeNode t2 = new TreeNode("1 : " + e2);
            if (e2 >= 0)
                t2 = MakeTree2(t2, e2);
            else
                t2.Nodes.Add("'" + (char)(0xFFFF - e2) + "'");
            t.Nodes.Add(t1);
            t.Nodes.Add(t2);
            return t;
        }

        public void CalcIndex(HuffNode h)
        {
            if (h.e0 == null && h.e1 == null && !h.hasIndex)
            {
                short u = (short)(0xFFFF - (short)h.c);
                h.index = u;
                h.hasIndex = true;
            }
            else
            {
                CalcIndex(h.e0);
                CalcIndex(h.e1);
                if (h.e0.hasIndex && h.e1.hasIndex)
                {
                    h.index = NodeList.Count / 2;
                    h.hasIndex = true;
                    NodeList.Add(h.e0.index);
                    NodeList.Add(h.e1.index);
                }
            }
        }

        public TreeNode GenTree(TreeNode t, HuffNode h)
        {
            if (h.e0 == null && h.e1 == null)
                t.Nodes.Add("'" + h.c + "'");
            else
            {
                if (h.e0 != null)
                {
                    TreeNode t1 = new TreeNode("0");
                    t1 = GenTree(t1, h.e0);
                    t.Nodes.Add(t1);
                }
                if (h.e1 != null)
                {
                    TreeNode t2 = new TreeNode("1");
                    t2 = GenTree(t2, h.e1);
                    t.Nodes.Add(t2);
                }
            }
            return t;
        }

        private void saveHelperstripMenuItem_Click(object sender, EventArgs e)
        {
            Generate();
            SaveFileDialog d = new SaveFileDialog();
            d.Filter = "*.bin|*.bin";
            if (d.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                Save(d.FileName);
                MessageBox.Show("Done");
            }
        }

        public void Save(string path)
        {
            FileStream fs = new FileStream(path, FileMode.Create, FileAccess.Write);
            Helpers.WriteInt(fs, (int)talk.magic);                        //0
            Helpers.WriteInt(fs, (int)talk.unk01);
            Helpers.WriteInt(fs, 0x38 + NodeList.Count * 4 + StringList.Count * 8);
            Helpers.WriteInt(fs, (int)talk.unk02);
            Helpers.WriteInt(fs, (int)talk.unk03);                        //10
            Helpers.WriteInt(fs, (int)talk.unk04);
            Helpers.WriteInt(fs, NodeList.Count);//Data1Count = Helpers.ReadInt(s);
            Helpers.WriteInt(fs, 0x38);//Data1Offset = Helpers.ReadInt(s);
            Helpers.WriteInt(fs, StringList.Count);//Data2Count = Helpers.ReadInt(s);      //20
            Helpers.WriteInt(fs, 0x38 + NodeList.Count * 4);//Data2Offset = Helpers.ReadInt(s);
            Helpers.WriteInt(fs, 0);//Data3Count = Helpers.ReadInt(s);
            Helpers.WriteInt(fs, 0x38 + NodeList.Count * 4 + StringList.Count * 8);//Data3Offset = Helpers.ReadInt(s);
            Helpers.WriteInt(fs, 0);//Data4Count = Helpers.ReadInt(s);      //30
            Helpers.WriteInt(fs, 0x38 + NodeList.Count * 4 + StringList.Count * 8);//Data4Offset = Helpers.ReadInt(s);
            foreach (int i in NodeList)
                Helpers.WriteInt(fs, i);
            foreach (StringID sid in StringList)
            {
                Helpers.WriteInt(fs, (int)sid.ID);
                Helpers.WriteInt(fs, (int)sid.offset);
            }
            foreach (int i in BitStream)
                Helpers.WriteInt(fs, i);
            fs.Close();
        }

        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenFileDialog d = new OpenFileDialog();
            d.Filter = "*.bin|*.bin";
            if (d.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                try
                {
                    LoadData(new MemoryStream(File.ReadAllBytes(d.FileName)));
                    MessageBox.Show("Done");
                }
                catch (Exception)
                {
                    MessageBox.Show("Error on loading!");
                }
            }
        }

        private void generateToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Generate();
        }

        private void saveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Generate();
            SaveFileDialog d = new SaveFileDialog();
            d.Filter = "*.bin|*.bin";
            if (d.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                Save(d.FileName);
                MessageBox.Show("Done");
            }
        }

        private void listBox3_DoubleClick(object sender, EventArgs e)
        {
            int n = listBox3.SelectedIndex;
            if (n == -1)
                return;
            STR s = talk.Strings[n];
            string input = Microsoft.VisualBasic.Interaction.InputBox("Please enter new value", "Edit", s.Value);
            if (input != "")
            {
                s.Value = input;
                talk.Strings[n] = s;
                RefreshMe();
            }
        }

    }
}
