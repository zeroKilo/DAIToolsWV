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
    public partial class HexTool : Form
    {
        public HexTool()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            string t = textBox1.Text.Replace(" ", "");
            textBox2.Text = "";
            try
            {
                textBox1.Text = t;
                byte[] buff = Helpers.HexStringToByteArray(t);
                ulong l = Helpers.ReadLEB128(new MemoryStream(buff));
                buff = BitConverter.GetBytes(l);
                textBox2.Text = l.ToString("X");
            }
            catch (Exception)
            {
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            string t = textBox2.Text.Replace(" ", "");
            textBox1.Text = "";
            try
            {
                textBox2.Text = t;
                int v = Convert.ToInt32(t, 16);
                MemoryStream m = new MemoryStream();
                Helpers.WriteLEB128(m, v);
                byte[] buff = m.ToArray();
                foreach (byte b in buff)
                    textBox1.Text += b.ToString("X2");
            }
            catch (Exception)
            {
            }
        }
    }
}
