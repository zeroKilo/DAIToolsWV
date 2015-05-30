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
    public partial class INITFSTool : Form
    {
        struct FileEntry
        {
            public byte[] data;
            public string name;
        }

        TOCFile initfs;
        List<FileEntry> list;

        public INITFSTool()
        {
            InitializeComponent();
        }

        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenFileDialog d = new OpenFileDialog();
            d.Filter = "initfs_Win32|initfs_Win32";
            if (d.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                LoadFile(d.FileName);
            }
        }

        public void LoadFile(string path)
        {
            initfs = new TOCFile(path);
            list = new List<FileEntry>();
            foreach (BJSON.Entry e in initfs.lines)
                if (e.fields != null && e.fields.Count != 0)
                {
                    BJSON.Field file = e.fields[0];
                    List<BJSON.Field> data = (List<BJSON.Field>)file.data;
                    FileEntry entry = new FileEntry();
                    foreach (BJSON.Field f in data)
                        switch (f.fieldname)
                        {
                            case "name":
                                entry.name = (string)f.data;
                                break;
                            case "payload":
                                entry.data = (byte[])f.data;
                                break;
                        }
                    list.Add(entry);
                }
            RefreshList();
        }

        public void RefreshList()
        {
            listBox1.Items.Clear();
            foreach (FileEntry e in list)
                listBox1.Items.Add(e.name);
        }

        private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            RefreshDisplay();
        }

        private void RefreshDisplay()
        {
            int n = listBox1.SelectedIndex;
            if (n == -1)
                return;
            FileEntry entry = list[n];
            if (toolStripButton1.Checked)
            {
                hb1.ByteProvider = new DynamicByteProvider(entry.data);
                hb1.BringToFront();
            }
            else
            {
                rtb1.Text = Encoding.Default.GetString(entry.data);
                rtb1.BringToFront();
            }
        }

        private void toolStripButton2_Click(object sender, EventArgs e)
        {
            toolStripButton1.Checked = false;
            toolStripButton2.Checked = true;
            RefreshDisplay();
        }

        private void toolStripButton1_Click(object sender, EventArgs e)
        {
            toolStripButton2.Checked = false;
            toolStripButton1.Checked = true;
            RefreshDisplay();
        }

        private void toolStripButton3_Click(object sender, EventArgs e)
        {
            int n = listBox1.SelectedIndex;
            if (n == -1)
                return;
            FileEntry entry = list[n];
            if (toolStripButton1.Checked)
            {
                MemoryStream m = new MemoryStream();
                for (long i = 0; i < hb1.ByteProvider.Length; i++)
                    m.WriteByte(hb1.ByteProvider.ReadByte(i));
                entry.data = m.ToArray();
            }
            else
                entry.data = Helpers.StringAsByteArray(rtb1.Text);
            list[n] = entry;
        }

        private void saveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SaveFileDialog d = new SaveFileDialog();
            d.Filter = "initfs_Win32|initfs_Win32";
            if (d.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                SaveFile(d.FileName);
                MessageBox.Show("Done");
            }
        }

        public void SaveFile(string path)
        {
            for (int i = 0; i < initfs.lines.Count; i++)
            {
                BJSON.Entry e = initfs.lines[i];
                if (e.fields != null && e.fields.Count != 0)
                {
                    BJSON.Field file = e.fields[0];
                    List<BJSON.Field> data = (List<BJSON.Field>)file.data;
                    foreach (BJSON.Field f in data)
                        if (f.fieldname=="payload")
                            f.data = list[i].data;
                }
                initfs.lines[i] = e;
            }
            initfs.Save(path);
            RefreshList();
        }
    }
}
