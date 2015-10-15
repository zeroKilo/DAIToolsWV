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

namespace DAIToolsWV.GeneralTools
{
    public partial class HexToString : Form
    {
        public HexToString()
        {
            InitializeComponent();
            hb4.ByteProvider = new DynamicByteProvider(new byte[0]);
        }

        private void toolStripButton1_Click(object sender, EventArgs e)
        {
            MemoryStream m = new MemoryStream();
            for (long i = 0; i < hb4.ByteProvider.Length; i++)
                m.WriteByte(hb4.ByteProvider.ReadByte(i));
            StringBuilder sb = new StringBuilder();
            m.Seek(0, 0);
            if (withSpaces.Checked)
                for (long i = 0; i < m.Length; i++)
                    sb.Append(((byte)m.ReadByte()).ToString("X2") + " ");
            else
                for (long i = 0; i < m.Length; i++)
                    sb.Append(((byte)m.ReadByte()).ToString("X2"));
            rtb2.Text = sb.ToString();
        }
    }
}
