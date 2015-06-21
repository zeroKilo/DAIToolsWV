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
using Be.Windows.Forms;
using DAILibWV;
using DAILibWV.Frostbite;

namespace DAIToolsWV.ContentTools
{
    public partial class EBXTool : Form
    {
        public byte[] ebxdata = null;
        public EBXStream ebx = null;

        public EBXTool()
        {
            InitializeComponent();
        }

        public void LoadEbx(byte[] data)
        {
            ebxdata = data;
            ebx = new EBXStream(new MemoryStream(ebxdata), pb1);
            RefreshDisplay();
        }

        private void loadFromBinaryToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenFileDialog d = new OpenFileDialog();
            d.Filter = "*.bin|*.bin";
            if (d.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                LoadEbx(File.ReadAllBytes(d.FileName));
        }

        public void RefreshDisplay()
        {
            hb1.ByteProvider = new DynamicByteProvider(ebxdata);
            hb2.ByteProvider = new DynamicByteProvider(ebx.keywordarea);
            listBox1.Items.Clear();
            int count = 0;
            foreach (EBXStream.StreamingPartitionImportEntry exguid in ebx.imports)
                listBox1.Items.Add((count++).ToString("X4") + " : " + exguid.partitionGuid.ToString() + " -> " + exguid.instanceGuid.ToString());
            count = 0;
            listBox2.Items.Clear();
            foreach (EBXStream.KeyWordDicStruct entry in ebx.keyWordDic)
                listBox2.Items.Add((count++).ToString("X4") + " : Offset = 0x" + entry.offset.ToString("X8") + " Hash = 0x" + entry.hash.ToString("X8") + " Keyword = '" + entry.keyword + "'");
            count = 0;
            listBox3.Items.Clear();
            foreach (EBXStream.StreamingPartitionFieldDescriptor f in ebx.fieldDescriptors)
                listBox3.Items.Add(
                    (count++).ToString("X4")
                    + " : Hash = 0x"
                    + f.fieldNameHash.ToString("X8")
                    + " Type = 0x"
                    + f.flagBits.ToString("X4")
                    + " Reference = 0x"
                    + f.fieldTypeIndex.ToString("X4")
                    + " Offset = 0x"
                    + f.fieldOffset.ToString("X8")
                    + " Secondary Offset = 0x"
                    + f.secondaryOffset.ToString("X8")
                    + " Name ='"
                    + f._name + "'");
            count = 0;
            listBox4.Items.Clear();
            foreach (EBXStream.StreamingPartitionTypeDescriptor f in ebx.typeDescriptors)
                listBox4.Items.Add(
                    (count++).ToString("X4")
                    + " : Hash = 0x"
                    + f.typeNameHash.ToString("X8")
                    + " StartIndex = 0x"
                    + f.layoutDescriptorIndex.ToString("X8")
                    + " NumField = 0x"
                    + f.fieldCount.ToString("X2")
                    + " Alignment = 0x"
                    + f.alignment.ToString("X2")
                    + " Type = 0x"
                    + f.typeFlags.ToString("X4")
                    + " Size = 0x"
                    + f.instanceSize.ToString("X4")
                    + " Secondary Size = 0x"
                    + f.secondaryInstanceSize.ToString("X8")
                    + " Name ='"
                    + f._name + "'");
            count = 0;
            listBox5.Items.Clear();
            foreach (EBXStream.StreamingPartitionTypeEntry i in ebx.typeList)
                listBox5.Items.Add((count++).ToString("X4") + " : Type Desc Index = 0x" + i.typeDescriptorIndex.ToString("X4") + " Repeats = 0x" + i.repetitions.ToString("X4"));
            count = 0;
            listBox6.Items.Clear();
            foreach (EBXStream.StreamingPartitionArrayEntry a in ebx.arrayList)
                listBox6.Items.Add((count++).ToString("X4") + " : Offset = 0x" + a.offset.ToString("X8") + " Type Desc Index = 0x" + a.typeDescriptorIndex.ToString("X8") + " Repeats = 0x" + a.elementCount.ToString("X8"));
            treeView1.Nodes.Clear();
            count = 0;
            TreeNode t = new TreeNode("Types");
            if (ebx.typeEntryList != null)
                foreach (EBXStream.TypeEntryStruct typ in ebx.typeEntryList)
                    t.Nodes.Add(EBXStream.TypeToNode(typ.type));
            t.ExpandAll();
            treeView1.Nodes.Add(t);
            rtb2.Text = ebx.toXML();
        }
    }
}
