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

namespace DAIToolsWV.FileTools
{
    public partial class MFTTool : Form
    {
        byte[] key = File.ReadAllBytes(Path.GetDirectoryName(Application.ExecutablePath) + "\\ext\\keys\\mft_key.bin");
        bool isEncrypted = false;

        public MFTTool()
        {
            InitializeComponent();
        }

        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenFileDialog d = new OpenFileDialog();
            d.Filter = "*.mft|*.mft";
            if(d.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                LoadFile(d.FileName);
        }

        private void LoadFile(string path)
        {
            byte[] data = File.ReadAllBytes(path);
            uint magic = BitConverter.ToUInt32(data,0);
            isEncrypted = false;
            if(magic != 0x656D614E)
            {
                for(int i=0;i<data.Length;i++)
                    data[i] ^= key[i%0x80];
                isEncrypted = true;
            }
            rtb1.Text = Encoding.ASCII.GetString(data);
        }

        private void saveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SaveFileDialog d = new SaveFileDialog();
            d.Filter = "*.mft|*.mft";
            if (d.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                SaveFile(d.FileName);
                MessageBox.Show("Done.");
            }

        }

        private void SaveFile(string path)
        {
            byte[] data = Helpers.StringAsByteArray(rtb1.Text.Replace("\n","\r\n"));
            if (isEncrypted)
            {
                for (int i = 0; i < data.Length; i++)
                    data[i] ^= key[i % 0x80];
            }
            File.WriteAllBytes(path, data);
        }
    }
}
