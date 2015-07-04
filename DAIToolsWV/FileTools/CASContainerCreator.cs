using System;
using System.IO;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Security.Cryptography;
using System.Threading.Tasks;
using System.Windows.Forms;
using Be.Windows.Forms;
using DAILibWV;

namespace DAIToolsWV.FileTools
{
    public partial class CASContainerCreator : Form
    {
        public byte[] SHA1;
        public byte[] data;
        public byte[] header;
        public byte[] container;

        public CASContainerCreator()
        {
            InitializeComponent();
        }

        private void loadBinaryToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenFileDialog d = new OpenFileDialog();
            d.Filter = "*.bin|*.bin";
            if (d.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                data = File.ReadAllBytes(d.FileName);
                ReFresh();
            }
        }

        public void ReFresh()
        {
            hb1.ByteProvider = new DynamicByteProvider(data);

            MemoryStream m = new MemoryStream();
            int pos = 0;
            while (pos < data.Length)
            {
                if (data.Length - pos > 0xFFFF)
                {
                    Helpers.WriteLEInt(m, 0xFFFF);
                    m.WriteByte(0);
                    m.WriteByte(0x70);
                    m.WriteByte(0xFF);
                    m.WriteByte(0xFF);
                    m.Write(data, pos, 0xFFFF);
                    pos += 0xFFFF;
                }
                else
                {
                    int rest = data.Length - pos;
                    Helpers.WriteLEInt(m, rest);
                    m.WriteByte(0);
                    m.WriteByte(0x70);
                    m.WriteByte((byte)(rest >> 8));
                    m.WriteByte((byte)(rest & 0xFF));
                    m.Write(data, pos, rest);
                    pos += rest;
                }
            }
            container = m.ToArray();
            hb3.ByteProvider = new DynamicByteProvider(container);

            m = new MemoryStream();
            Helpers.WriteUInt(m, 0xF00FCEFA);
            SHA1CryptoServiceProvider sha1 = new SHA1CryptoServiceProvider();
            SHA1 = sha1.ComputeHash(container);
            m.Write(SHA1, 0, 0x14);
            Helpers.WriteInt(m, container.Length);
            Helpers.WriteUInt(m, 0);
            header = m.ToArray();
            hb2.ByteProvider = new DynamicByteProvider(header);
        }

        private void appendToCASToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenFileDialog d = new OpenFileDialog();
            d.Filter = "*.cas|*.cas";
            if (d.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                FileStream fs = new FileStream(d.FileName, FileMode.Append, FileAccess.Write);
                fs.Seek(0, SeekOrigin.End);
                fs.Write(header, 0, 0x20);
                fs.Write(container, 0, container.Length);
                fs.Close();
                MessageBox.Show("Done.");
            }
        }
    }
}
