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
using DAILibWV;

namespace DAIToolsWV.FileTools
{
    public partial class BundleBuilder : Form
    {
        public TOCFile toc;
        public List<bool> SelectForDeletion;

        public BundleBuilder()
        {
            InitializeComponent();
        }

        private void opnSingleToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenFileDialog d = new OpenFileDialog();
            d.Filter = "*.toc|*.toc";
            if(d.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                toc = new TOCFile(d.FileName);
                SelectForDeletion = new List<bool>();
                foreach (TOCFile.TOCBundleInfoStruct b in toc.bundles)
                    SelectForDeletion.Add(false);
                RefreshMe();
                rtb2.Text = "";
            }
        }

        public void RefreshMe()
        {
            listBox1.Items.Clear();
            int count = 0;
            foreach (TOCFile.TOCBundleInfoStruct bundle in toc.bundles)
                listBox1.Items.Add((SelectForDeletion[count++] ? "(TO DELETE)" : "") + bundle.id);
        }

        private void saveAsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (toc == null)
                return;
            SaveFileDialog d = new SaveFileDialog();
            d.Filter = "*.toc|*.toc";
            if (d.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                rtb2.Text = "Start Compiling...\n";
                if (!PrepareBJSON()) return;
                if (!BuildSB(d.FileName)) return;
                rtb2.AppendText("Writing " + Path.GetFileName(d.FileName) + " ...\n");
                toc.Save(d.FileName);
                rtb2.AppendText("Saved " + Path.GetFileName(d.FileName) + "\n");
                MessageBox.Show("Done");
            }
        }

        private bool PrepareBJSON()
        {
            rtb2.AppendText("Preparing BJSON...\n");
            try
            {
                BJSON.Entry root = toc.lines[0];
                BJSON.Field bundles = root.fields[0];
                BJSON.Field tbun = bundles;
                List<BJSON.Entry> list = (List<BJSON.Entry>)bundles.data;
                tbun.data = new List<BJSON.Entry>();
                int countcopy = 0;
                for (int i = 0; i < list.Count; i++)
                    if (!SelectForDeletion[i])
                    {
                        ((List<BJSON.Entry>)tbun.data).Add(list[i]);
                        countcopy++;
                    }
                rtb2.AppendText("Copied: " + countcopy + " / " + list.Count + "\n");
                root.fields[0] = tbun;
            }
            catch (Exception ex)
            {
                rtb2.AppendText("ERROR: " + ex.Message);
                return false;
            }
            return true;
        }

        private bool BuildSB(string tocname)
        {
            rtb2.AppendText("Building SB file...\n");
            string dir = Path.GetDirectoryName(tocname) + "\\";
            string filename = Path.GetFileNameWithoutExtension(tocname) + ".sb";
            string odir = Path.GetDirectoryName(toc.MyPath) + "\\";
            string ofilename = Path.GetFileNameWithoutExtension(toc.MyPath) + ".sb";
            rtb2.AppendText("Writing " + filename + " ...\n");
            try
            {
                BJSON.Entry root = toc.lines[0];
                BJSON.Field bundles = root.fields[0];
                BJSON.Field tbun = bundles;
                List<BJSON.Entry> list = (List<BJSON.Entry>)bundles.data;
                FileStream fs = new FileStream(dir + filename, FileMode.Create, FileAccess.Write);
                FileStream ofs = new FileStream(odir + ofilename, FileMode.Open, FileAccess.Read);
                MemoryStream m = new MemoryStream();
                long gl_off = 0;
                for (int i = 0; i < list.Count; i++)
                {
                    BJSON.Entry e = list[i];
                    long offset = 0;
                    int size = 0;
                    bool isbase = false;
                    BJSON.Field offset_field = new BJSON.Field();
                    foreach(BJSON.Field f in e.fields)
                        switch (f.fieldname)
                        {
                            case "offset":
                                offset = BitConverter.ToInt64((byte[])f.data, 0);
                                offset_field = f;
                                break;
                            case "size":
                                size = BitConverter.ToInt32((byte[])f.data, 0);
                                break;
                            case "base":
                                isbase = (bool)f.data;
                                break;
                        }
                    if (isbase)
                        continue;
                    offset_field.data = BitConverter.GetBytes(gl_off);
                    CopyFileStream(ofs, m, offset, size);
                    gl_off += size;
                }
                ofs.Close();
                MemoryStream t = new MemoryStream();
                Helpers.WriteLEB128(t, (int)m.Length);
                m.WriteByte(0);
                int varsize = (int)t.Length;
                fs.WriteByte(0x82);
                Helpers.WriteLEB128(fs, varsize + 9 + (int)m.Length);
                byte[] buff = { 0x01, 0x62, 0x75, 0x6E, 0x64, 0x6C, 0x65, 0x73, 0x00 };
                fs.Write(buff, 0, 9);
                fs.Write(t.ToArray(), 0, varsize);
                int rel_off = (int)fs.Position;
                fs.Write(m.ToArray(), 0, (int)m.Length);
                fs.Close();
                rtb2.AppendText("Saved " + filename + "\nUpdating TOC Entries...\n");
                for (int i = 0; i < list.Count; i++)
                {
                    BJSON.Entry e = list[i];
                    long offset = 0;
                    bool isbase = false;
                    BJSON.Field offset_field = new BJSON.Field();
                    foreach (BJSON.Field f in e.fields)
                        switch (f.fieldname)
                        {
                            case "offset":
                                offset = BitConverter.ToInt64((byte[])f.data, 0);
                                offset_field = f;
                                break;
                            case "base":
                                isbase = (bool)f.data;
                                break;
                        }
                    if (isbase)
                        continue;
                    offset_field.data = BitConverter.GetBytes(offset + rel_off);
                }
            }
            catch (Exception ex)
            {
                rtb2.AppendText("ERROR: " + ex.Message);
                return false;
            }
            rtb2.AppendText("");
            return true;
        }

        private void CopyFileStream(Stream s_in, Stream s_out, long offset, int size)
        {
            s_in.Seek(0, SeekOrigin.End);
            long len = s_in.Position;
            if (offset + size > len)
            {
                rtb2.AppendText("ERROR: Tried to read-copy outside of filesize");
                return;
            }
            s_in.Seek(offset, 0);
            byte[] buff = new byte[size];
            int bytes_read = 0;
            while ((bytes_read += s_in.Read(buff, bytes_read, size - bytes_read)) != size) Application.DoEvents();
            s_out.Write(buff, 0, size);
        }

        private void toolStripButton1_Click(object sender, EventArgs e)
        {
            int n = listBox1.SelectedIndex;
            if (n == -1 || toc == null)
                return;
            SelectForDeletion[n] = true;
            RefreshMe();
            listBox1.SelectedIndex = n;
        }

        private void toolStripButton2_Click(object sender, EventArgs e)
        {
            int n = listBox1.SelectedIndex;
            if (n == -1 || toc == null)
                return;
            SelectForDeletion[n] = false;
            RefreshMe();
            listBox1.SelectedIndex = n;
        }

        private void listBox1_SelectedIndexChanged_1(object sender, EventArgs e)
        {
            int n = listBox1.SelectedIndex;
            if (n == -1 || toc == null)
                return;
            TOCFile.TOCBundleInfoStruct b = toc.bundles[n];
            StringBuilder sb = new StringBuilder();
            sb.AppendFormat("TOC information\n===============\n\nPath     : {0}\nIs in CAS: {1}\n\n", toc.MyPath, toc.iscas);
            sb.AppendFormat("Bundle information\n==================\n\nId     : {0}\nOffset : 0x{1}\nSize   : 0x{2}\n", b.id, b.offset.ToString("X8"), b.size.ToString("X8"));
            sb.AppendFormat("IsDelta: {0}\nIsBase : {1}", b.isdelta, b.isbase);
            rtb1.Text = sb.ToString();
        }

        private void listBox1_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == (char)13)
            {
                int n = listBox1.SelectedIndex;
                if (n == -1 || toc == null)
                    return;
                SelectForDeletion[n] = !SelectForDeletion[n];
                RefreshMe();
                listBox1.SelectedIndex = n;
            }
        }
    }
}
