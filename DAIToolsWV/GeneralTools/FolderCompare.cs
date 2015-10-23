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

namespace DAIToolsWV.GeneralTools
{
    public partial class FolderCompare : Form
    {
        public FolderCompare()
        {
            InitializeComponent();
        }

        private void toolStripButton1_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog fbd = new FolderBrowserDialog();
            if (fbd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                toolStripTextBox1.Text = fbd.SelectedPath + "\\";
        }

        private void toolStripButton2_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog fbd = new FolderBrowserDialog();
            if (fbd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                toolStripTextBox2.Text = fbd.SelectedPath + "\\";
        }

        private void toolStripButton3_Click(object sender, EventArgs e)
        {
            string path1 = toolStripTextBox1.Text;
            string path2 = toolStripTextBox2.Text;
            if (path1 == "" || path2 == "")
                return;
            if (!path1.EndsWith("\\"))
                path1 += '\\';
            if (!path2.EndsWith("\\"))
                path2 += '\\';
            string[] files1 = Directory.GetFiles(path1, "*.toc", SearchOption.AllDirectories);
            string[] files2 = Directory.GetFiles(path1, "*.toc", SearchOption.AllDirectories);
            rtb1.Text = "";
            P("found " + files1.Length + " files in " + path1);
            P("found " + files2.Length + " files in " + path2);
            if (files1.Length == files2.Length)
                P("file count is identical");
            else
            {
                P("file count is different");
                if (files2.Length > files1.Length)
                {
                    string[] tmp = files1;
                    files1 = files2;
                    files2 = files1;
                    string p = path1;
                    path1 = path2;
                    path2 = p;
                }
            }
            foreach (string file in files1)
            {
                P(file.Substring(path1.Length));
                if (!File.Exists(path2 + file.Substring(path1.Length))) 
                {
                    P(" E:compare file doesnt exist");
                    continue;
                }
                CompareTocs(file, path2 + file.Substring(path1.Length));
            }
            P("\n\n=============\nCompare Done.\n");
        }

        private void CompareTocs(string file1, string file2)
        {
            TOCFile toc1 = new TOCFile(file1);
            TOCFile toc2 = new TOCFile(file2);
            foreach (TOCFile.TOCBundleInfoStruct b in toc1.bundles)
            {
                TOCFile.TOCBundleInfoStruct b2 = new TOCFile.TOCBundleInfoStruct();
                b2.id = "";
                foreach(TOCFile.TOCBundleInfoStruct compare in toc2.bundles)
                    if (b.id == compare.id)
                    {
                        b2 = compare;
                        break;
                    }
                if (b2.id == "")
                {
                    P(" E:bundle not found in compare file - " + b.id);
                    continue;
                }
                StringBuilder sb = new StringBuilder();
                sb.Append(" D:bundle " + b.id + "\n");                
                if (b.isbase != b2.isbase)
                    sb.AppendFormat(" -base is different ({0}) vs ({1})\n",b.isbase,b2.isbase);
                if (b.isdelta != b2.isdelta)
                    sb.AppendFormat(" -delta is different ({0}) vs ({1})\n",b.isdelta,b2.isdelta);
                if (b.size != b2.size)
                    sb.AppendFormat(" -size is different ({0}) vs ({1})\n",b.size, b2.size);
                if ((b.isbase != b2.isbase) || (b.isdelta != b2.isdelta) || (b.size != b2.size))
                    P(sb.ToString());
            }
            if (!file1.ToLower().Contains("chunk")) 
            foreach (TOCFile.TOCChunkInfoStruct b in toc1.chunks)
            {
                TOCFile.TOCChunkInfoStruct b2 = new TOCFile.TOCChunkInfoStruct();
                b2.id = new byte[0];
                foreach (TOCFile.TOCChunkInfoStruct compare in toc2.chunks)
                    if (Helpers.ByteArrayCompare(b.id, compare.id))
                    {
                        b2 = compare;
                        break;
                    }
                if (b2.id.Length == 0)
                {
                    P(" E:chunk not found in compare file - " + Helpers.ByteArrayToHexString(b.id));
                    continue;
                }
                StringBuilder sb = new StringBuilder();
                sb.Append(" D:chunk " + Helpers.ByteArrayToHexString(b.id) + "\n");
                if (b.sha1 != null && Helpers.ByteArrayCompare(b.sha1,b2.sha1))
                    sb.AppendFormat(" -sha1 is different ({0}) vs ({1})\n", Helpers.ByteArrayToHexString(b.sha1), Helpers.ByteArrayToHexString(b2.sha1));
                if (b.offset != b2.offset)
                    sb.AppendFormat(" -offset is different ({0}) vs ({1})\n", b.offset, b2.offset);
                if (b.size != b2.size)
                    sb.AppendFormat(" -size is different ({0}) vs ({1})\n", b.size, b2.size);
                if ((b.sha1 != null && Helpers.ByteArrayCompare(b.sha1, b2.sha1)) || (b.offset != b2.offset) || (b.size != b2.size))
                    P(sb.ToString());
            }
            CompareSB(file1.ToLower().Replace(".toc", ".sb"), file2.ToLower().Replace(".toc", ".sb"));
        }

        private void CompareSB(string file1, string file2)
        {
            if (!File.Exists(file1) || !File.Exists(file2))
                return;
            SBFile sb1 = new SBFile(file1);
            SBFile sb2 = new SBFile(file2);
            foreach (Bundle b in sb1.bundles)
            {
                Bundle b2 = null;
                foreach(Bundle c in sb2.bundles)
                    if (c.path == b.path)
                    {
                        b2 = c;
                        break;
                    }
                if (b2 == null)
                    continue;
                StringBuilder sb = new StringBuilder();
                sb.Append(" D:bundle " + b.path + "\n");
                bool diff = false;
                foreach (Bundle.ebxtype ebx in b.ebx)
                {
                    Bundle.ebxtype ebx2 = new Bundle.ebxtype();
                    ebx2.name = "";
                    foreach (Bundle.ebxtype compare in b2.ebx)
                        if (ebx.name == compare.name)
                        {
                            ebx2 = compare;
                            break;
                        }
                    if (ebx2.name == "")
                    {
                        sb.Append("  E:ebx not found - " + ebx.name + "\n");
                        continue;
                    }
                    sb.Append(CompareEBX(ebx, ebx2, diff, out diff));
                }
                foreach (Bundle.restype res in b.res)
                {
                    Bundle.restype res2 = new Bundle.restype();
                    res2.name = "";
                    foreach (Bundle.restype compare in b2.res)
                        if (res.name == compare.name)
                        {
                            res2 = compare;
                            break;
                        }
                    if (res2.name == "")
                    {
                        sb.Append("  E:res not found - " + res.name + "\n");
                        continue;
                    }
                    sb.Append(CompareRES(res, res2, diff, out diff));
                }
                foreach (Bundle.chunktype chunk in b.chunk)
                {
                    Bundle.chunktype chunk2 = new Bundle.chunktype();
                    chunk2.id = new byte[0];
                    foreach (Bundle.chunktype compare in b2.chunk)
                        if (Helpers.ByteArrayCompare(chunk.id,compare.id))
                        {
                            chunk2 = compare;
                            break;
                        }
                    if (chunk2.id.Length == 0)
                    {
                        sb.Append("  E:chunk not found - " + Helpers.ByteArrayToHexString(chunk.id) + "\n");
                        continue;
                    }
                    sb.Append(CompareCHUNK(chunk, chunk2, diff, out diff));
                }
                if (diff)
                    P(sb.ToString());
            }
        }

        private string CompareEBX(Bundle.ebxtype ebx, Bundle.ebxtype ebx2, bool cdiff, out bool diff)
        {
            StringBuilder sb = new StringBuilder();
            bool tdiff = false;
            sb.Append("  D:ebx " + ebx.name + "\n");
            if (ebx.baseSha1 != null && !Helpers.ByteArrayCompare(ebx.baseSha1, ebx2.baseSha1))
            {
                sb.AppendFormat("  D:-ebx basesha1 is different - ({0}) vs ({1})\n", Helpers.ByteArrayToHexString(ebx.baseSha1), Helpers.ByteArrayToHexString(ebx2.baseSha1));
                tdiff = true;
            }
            if (ebx.deltaSha1 != null && !Helpers.ByteArrayCompare(ebx.deltaSha1, ebx2.deltaSha1))
            {
                sb.AppendFormat("  D:-ebx deltasha1 is different - ({0}) vs ({1})\n", Helpers.ByteArrayToHexString(ebx.deltaSha1), Helpers.ByteArrayToHexString(ebx2.deltaSha1));
                tdiff = true;
            }
            if (ebx.Sha1 != null && !Helpers.ByteArrayCompare(ebx.Sha1, ebx2.Sha1))
            {
                sb.AppendFormat("  D:-ebx sha1 is different - ({0}) vs ({1})\n", Helpers.ByteArrayToHexString(ebx.Sha1), Helpers.ByteArrayToHexString(ebx2.Sha1));
                tdiff = true;
            }
            if (ebx.size != null && !Helpers.ByteArrayCompare(ebx.size, ebx2.size))
            {
                sb.AppendFormat("  D:-ebx size is different - ({0}) vs ({1})\n", BitConverter.ToInt32(ebx.size, 0), BitConverter.ToInt32(ebx2.size, 0));
                tdiff = true;
            }
            if (ebx.originalSize != null && !Helpers.ByteArrayCompare(ebx.originalSize, ebx2.originalSize))
            {
                sb.AppendFormat("  D:-ebx original size is different - ({0}) vs ({1})\n", BitConverter.ToInt32(ebx.originalSize, 0), BitConverter.ToInt32(ebx2.originalSize, 0));
                tdiff = true;
            }
            if (ebx.casPatchType != ebx2.casPatchType)
            {
                sb.AppendFormat("  D:-ebx caspathtype is different - ({0}) vs ({1})\n", ebx.casPatchType, ebx2.casPatchType);
                tdiff = true;
            }
            if (tdiff)
            {
                diff = true;
                return sb.ToString();
            }
            else
            {
                diff = cdiff;
                return "";
            }
        }

        private string CompareRES(Bundle.restype res, Bundle.restype res2, bool cdiff, out bool diff)
        {
            StringBuilder sb = new StringBuilder();
            bool tdiff = false;
            sb.Append("  D:res " + res.name + "\n");
            if (res.rtype != null && !Helpers.ByteArrayCompare(res.rtype, res2.rtype))
            {
                sb.AppendFormat("  D:-res ressource type is different - ({0}) vs ({1})\n", Helpers.ByteArrayToHexString(res.rtype), Helpers.ByteArrayToHexString(res2.rtype));
                tdiff = true;
            }
            if (res.baseSha1 != null && !Helpers.ByteArrayCompare(res.baseSha1, res2.baseSha1))
            {
                sb.AppendFormat("  D:-res basesha1 is different - ({0}) vs ({1})\n", Helpers.ByteArrayToHexString(res.baseSha1), Helpers.ByteArrayToHexString(res2.baseSha1));
                tdiff = true;
            }
            if (res.deltaSha1 != null && !Helpers.ByteArrayCompare(res.deltaSha1, res2.deltaSha1))
            {
                sb.AppendFormat("  D:-res deltasha1 is different - ({0}) vs ({1})\n", Helpers.ByteArrayToHexString(res.deltaSha1), Helpers.ByteArrayToHexString(res2.deltaSha1));
                tdiff = true;
            }
            if (res.SHA1 != null && !Helpers.ByteArrayCompare(res.SHA1, res2.SHA1))
            {
                sb.AppendFormat("  D:-res sha1 is different - ({0}) vs ({1})\n", Helpers.ByteArrayToHexString(res.SHA1), Helpers.ByteArrayToHexString(res2.SHA1));
                tdiff = true;
            }
            if (res.size != null && !Helpers.ByteArrayCompare(res.size, res2.size))
            {
                sb.AppendFormat("  D:-res size is different - ({0}) vs ({1})\n", BitConverter.ToInt32(res.size, 0), BitConverter.ToInt32(res2.size, 0));
                tdiff = true;
            }
            if (res.osize != null && !Helpers.ByteArrayCompare(res.osize, res2.osize))
            {
                sb.AppendFormat("  D:-res original size is different - ({0}) vs ({1})\n", BitConverter.ToInt32(res.osize, 0), BitConverter.ToInt32(res2.osize, 0));
                tdiff = true;
            }
            if (res.casPatchType != res2.casPatchType)
            {
                sb.AppendFormat("  D:-res caspathtype is different - ({0}) vs ({1})\n", res.casPatchType, res2.casPatchType);
                tdiff = true;
            }
            if (tdiff)
            {
                diff = true;
                return sb.ToString();
            }
            else
            {
                diff = cdiff;
                return "";
            }
        }

        private string CompareCHUNK(Bundle.chunktype chunk, Bundle.chunktype chunk2, bool cdiff, out bool diff)
        {
            StringBuilder sb = new StringBuilder();
            bool tdiff = false;
            sb.Append("  D:chunk " + Helpers.ByteArrayToHexString(chunk.id) + "\n");
            if (chunk.baseSha1 != null && !Helpers.ByteArrayCompare(chunk.baseSha1, chunk2.baseSha1))
            {
                sb.AppendFormat("  D:-chunk basesha1 is different - ({0}) vs ({1})\n", Helpers.ByteArrayToHexString(chunk.baseSha1), Helpers.ByteArrayToHexString(chunk2.baseSha1));
                tdiff = true;
            }
            if (chunk.deltaSha1 != null && !Helpers.ByteArrayCompare(chunk.deltaSha1, chunk2.deltaSha1))
            {
                sb.AppendFormat("  D:-chunk deltasha1 is different - ({0}) vs ({1})\n", Helpers.ByteArrayToHexString(chunk.deltaSha1), Helpers.ByteArrayToHexString(chunk2.deltaSha1));
                tdiff = true;
            }
            if (chunk.SHA1 != null && !Helpers.ByteArrayCompare(chunk.SHA1, chunk2.SHA1))
            {
                sb.AppendFormat("  D:-chunk sha1 is different - ({0}) vs ({1})\n", Helpers.ByteArrayToHexString(chunk.SHA1), Helpers.ByteArrayToHexString(chunk2.SHA1));
                tdiff = true;
            }
            if (chunk.size != null && !Helpers.ByteArrayCompare(chunk.size, chunk2.size))
            {
                sb.AppendFormat("  D:-chunk size is different - ({0}) vs ({1})\n", BitConverter.ToInt32(chunk.size, 0), BitConverter.ToInt32(chunk2.size, 0));
                tdiff = true;
            }
            if (chunk.casPatchType != chunk2.casPatchType)
            {
                sb.AppendFormat("  D:-chunk caspathtype is different - ({0}) vs ({1})\n", chunk.casPatchType, chunk2.casPatchType);
                tdiff = true;
            }
            if (tdiff)
            {
                diff = true;
                return sb.ToString();
            }
            else
            {
                diff = cdiff;
                return "";
            }
        }


        private void P(string s)
        {
            rtb1.AppendText(s + "\n");
            rtb1.SelectionStart = rtb1.Text.Length;
            rtb1.SelectionLength = 0;
            rtb1.ScrollToCaret();
            Application.DoEvents();
        }
    }
}
