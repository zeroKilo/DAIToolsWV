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

namespace DAIToolsWV.ModTools
{
    public partial class ModRunner : Form
    {
        public Mod mod;
        public string outputPath;
        public List<int> selectedJobs;

        public ModRunner()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            OpenFileDialog d = new OpenFileDialog();
            d.Filter = "*.DAIMWV|*.DAIMWV";
            if (d.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                textBox2.Text = d.FileName;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog fbd = new FolderBrowserDialog();
            if (fbd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                textBox1.Text = fbd.SelectedPath + "\\";
        }

        private void toolStripButton2_Click(object sender, EventArgs e)
        {
            if (radioButton1.Checked)
            {
                outputPath = GlobalStuff.FindSetting("gamepath") + "Update\\Patch\\";
            }
            else
            {
                if (textBox1.Text == "")
                    return;
                outputPath = textBox1.Text;
            }
            listBox1.Items.Clear();
            int count = 0;
            foreach (Mod.ModJob mj in mod.jobs)
                listBox1.Items.Add((count++) + " : Job Type(" + mod.GetTypeName(mj.type) + ")");
            panel3.BringToFront();
        }

        private void toolStripButton3_Click(object sender, EventArgs e)
        {
            if (textBox2.Text == "")
                return;
            mod = new Mod();
            mod.Load(textBox2.Text);
            panel2.BringToFront();
        }

        private void toolStripButton1_Click(object sender, EventArgs e)
        {
            selectedJobs = new List<int>();
            for (int i = 0; i < mod.jobs.Count; i++)
                selectedJobs.Add(i);
            RunJobs();
        }

        private void toolStripButton4_Click(object sender, EventArgs e)
        {
            selectedJobs = new List<int>();
            foreach (int i in listBox1.SelectedIndices)
                selectedJobs.Add(i);
            RunJobs();
        }

        private void toolStripButton5_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            rtb1.Text = "";
            int n = listBox1.SelectedIndex;
            if (n == -1)
                return;
            Mod.ModJob mj = mod.jobs[n];
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("Mod Type :" + mod.GetTypeName(mj.type));
            switch (mj.type)
            {
                case 0:
                    sb.AppendLine("Affected ressource path: " + mj.respath);
                    sb.AppendLine("Affected bundle paths (" + mj.bundlePaths.Count + "):");                    
                    foreach (string p in mj.bundlePaths)
                        sb.AppendLine("\t" + p);
                    sb.AppendLine("Affected toc files (" + mj.tocPaths.Count + "):");
                    foreach (string p in mj.tocPaths)
                        sb.AppendLine("\t" + p);
                    break;
                default:
                    return;
            }
            rtb1.Text = sb.ToString();
        }

        private void RunJobs()
        {
            if (selectedJobs.Count == 0)
                return;
            rtb2.Text = "";
            panel4.BringToFront();
            DbgPrint("Start running " + selectedJobs.Count + " job(s):");
            toolStripButton5.Enabled = false;
            foreach (int i in selectedJobs)
                RunJob(i);
            toolStripButton5.Enabled = true;
        }

        private void DbgPrint(string s)
        {
            rtb2.AppendText(DateTime.Now.ToLongTimeString() + " : " + s + "\n");
            rtb2.SelectionStart = rtb2.Text.Length;
            rtb2.SelectionLength = 0;
            rtb2.ScrollToCaret();
        }

        public void RunJob(int i)
        {
            Mod.ModJob mj = mod.jobs[i];
            DbgPrint("Starting job of type : " + mod.GetTypeName(mj.type));
            switch (mj.type)
            {
                case 0:
                    RunTextureJob(mj);
                    break;
                default:
                    DbgPrint("Unknown mod type, did nothing");
                    break;
            }
            DbgPrint("End of job");
        }

        public void RunTextureJob(Mod.ModJob mj)
        {
            DbgPrint("Running Texture Replacement Job for: " + mj.respath);
            //Check Toc Files
            List<string> reducedTocPaths = new List<string>();
            bool found = false;
            foreach (string p in mj.tocPaths)
            {
                found = false;
                for (int i = 0; i < reducedTocPaths.Count; i++)
                    if (p == reducedTocPaths[i])
                    {
                        found = true;
                        break;
                    }
                if (!found)
                    reducedTocPaths.Add(p);
            }
            mj.tocPaths = reducedTocPaths;
            List<string> reducedBundlePaths = new List<string>();            
            foreach (string p in mj.bundlePaths)
            {
                found = false;
                for (int i = 0; i < reducedBundlePaths.Count; i++)
                    if (p == reducedBundlePaths[i])
                    {
                        found = true;
                        break;
                    }
                if (!found)
                    reducedBundlePaths.Add(p);
            }
            mj.bundlePaths = reducedBundlePaths;
            foreach (string tocpath in mj.tocPaths)
            {
                if (tocpath.ToLower().Contains("\\patch\\"))
                    continue;
                DbgPrint("Checking for : " + tocpath);
                if (!tocpath.ToLower().StartsWith("update"))
                {
                    if (!File.Exists(outputPath + tocpath))
                    {
                        DbgPrint("Error: TOC file not found, aborting!");
                        return;
                    }
                }
                else
                {
                    if (!File.Exists(outputPath + Helpers.SkipSubFolder(tocpath, 2)))
                    {
                        DbgPrint("Error: TOC file not found, aborting!");
                        return;
                    }
                }
            }
            DbgPrint("All found.");
            //create cas data
            byte[] newsha1 = CreateCASContainer(mj);
            if (newsha1.Length != 0x14)
            {
                DbgPrint("Error: could not create CAS data, aborting!");
                return;
            }
            //walk through affected toc files
            foreach (string tocpath in mj.tocPaths)
                if (!tocpath.ToLower().Contains("\\patch\\"))
                    RunRessourceJob(mj, tocpath, newsha1);
        }

        public void RunRessourceJob(Mod.ModJob mj, string tocpath, byte[] newsha1)
        {
            if (!tocpath.ToLower().StartsWith("update"))
            DbgPrint("Loading : " + tocpath);
            TOCFile toc = null;
            if (!tocpath.ToLower().StartsWith("update"))
                toc = new TOCFile(outputPath + tocpath);
            else
                toc = new TOCFile(outputPath + Helpers.SkipSubFolder(tocpath, 2));
            //walk through affected bundles
            foreach (string bpath in mj.bundlePaths)
                RunRessourceJobOnBundle(mj, toc, tocpath, newsha1, bpath);
        }

        public void RunRessourceJobOnBundle(Mod.ModJob mj, TOCFile toc, string tocpath, byte[] newsha1, string bpath)
        {
            int count = 0;
            int index = -1;
            foreach (TOCFile.TOCBundleInfoStruct buni in toc.bundles)
                if (count++ > -1 && bpath == buni.id)
                {
                    DbgPrint(" Found bundle : " + bpath);
                    index = count - 1;
                    break;
                }
            //if bundle found
            if (index != -1)
            {
                if (!toc.iscas)
                {
                    DbgPrint(" Warning: binary bundles not supported yet, skipping!");
                    return;
                }
                //find out if base or delta
                BJSON.Entry root = toc.lines[0];
                BJSON.Field bundles = root.fields[0];
                BJSON.Entry bun = ((List<BJSON.Entry>)bundles.data)[index];
                BJSON.Field isDeltaField = bun.FindField("delta");
                BJSON.Field isBaseField = bun.FindField("base");
                //if is base, copy from base, make delta and recompile
                if (isBaseField != null && (bool)isBaseField.data == true)
                    if (!ImportBundleFromBase(toc, tocpath, index, bpath))
                        return;
                //check if already is in sb
                if (isDeltaField != null && (bool)isDeltaField.data == true)
                    DbgPrint("  Its already a delta");
                DbgPrint("  Updating SB file with new SHA1...");//yeah, pretty much
                string SBpath = outputPath + Path.GetDirectoryName(tocpath) + "\\" + Path.GetFileNameWithoutExtension(tocpath) + ".sb";
                SBFile sb = new SBFile(SBpath);
                root = sb.lines[0];
                bundles = root.fields[0];
                count = ((List<BJSON.Entry>)bundles.data).Count;
                //find right bundle
                for (int i = 0; i < count; i++)
                {
                    bun = ((List<BJSON.Entry>)bundles.data)[i];
                    BJSON.Field ebx = bun.FindField("ebx");
                    BJSON.Field res = bun.FindField("res");
                    BJSON.Field chunks = bun.FindField("chunks");
                    BJSON.Field path = bun.FindField("path");
                    if (!(path != null && (string)path.data == bpath) || res == null || chunks == null)
                        continue;
                    bool found = false;
                    ////find right ebx entry
                    //foreach (BJSON.Entry ebx_e in ((List<BJSON.Entry>)ebx.data))
                    //{
                    //    BJSON.Field f_name = ebx_e.FindField("name");
                    //    BJSON.Field f_casPatchType = ebx_e.FindField("casPatchType");
                    //    if (f_name != null && (string)f_name.data == mj.respath)
                    //    {
                    //        DbgPrint("  Found ebx");
                    //        found = true;
                    //        if (f_casPatchType != null)
                    //            f_casPatchType.data = BitConverter.GetBytes((int)1);
                    //        else
                    //        {
                    //            f_casPatchType = new BJSON.Field();
                    //            f_casPatchType.fieldname = "casPatchType";
                    //            f_casPatchType.type = 8;
                    //            f_casPatchType.data = BitConverter.GetBytes((int)1);
                    //            ebx_e.fields.Add(f_casPatchType);
                    //        }
                    //        DbgPrint("  casPatchType set to 1");
                    //    }
                    //}
                    //if (!found)
                    //{
                    //    DbgPrint("  Error: cant find ebx, skipping!");
                    //    break;
                    //}
                    //
                    //found = false;
                    byte[] chunkidbuff = new byte[16];
                    //find right res entry
                    foreach (BJSON.Entry res_e in ((List<BJSON.Entry>)res.data))
                    {
                        BJSON.Field f_sha1 = res_e.FindField("sha1");
                        BJSON.Field f_name = res_e.FindField("name");
                        BJSON.Field f_casPatchType = res_e.FindField("casPatchType");
                        if (f_name != null && (string)f_name.data == mj.respath && f_sha1 != null)
                        {
                            //get res data and extract chunk id
                            byte[] sha1buff = (byte[])f_sha1.data;
                            DbgPrint("  Found res sha1 : " + Helpers.ByteArrayToHexString(sha1buff));
                            byte[] resdata = SHA1Access.GetDataBySha1(sha1buff);
                            if (resdata.Length == 0)
                            {
                                DbgPrint("  Error: cant find res data, skipping!");
                                break;
                            }
                            for (int j = 0; j < 16; j++)
                                chunkidbuff[j] = resdata[j + 0x1C];
                            DbgPrint("  Found chunk id : " + Helpers.ByteArrayToHexString(chunkidbuff));
                            //if (f_casPatchType != null)
                            //    f_casPatchType.data = BitConverter.GetBytes((int)1);
                            //else
                            //{
                            //    f_casPatchType = new BJSON.Field();
                            //    f_casPatchType.fieldname = "casPatchType";
                            //    f_casPatchType.type = 8;
                            //    f_casPatchType.data = BitConverter.GetBytes((int)1);
                            //    res_e.fields.Add(f_casPatchType);
                            //}
                            //DbgPrint("  casPatchType set to 1");
                            found = true;
                        }
                    }
                    if (!found)
                    {
                        DbgPrint("  Error: cant find res, skipping!");
                        break;
                    }
                    found = false;
                    //find right chunk entry
                    foreach (BJSON.Entry chunk_e in ((List<BJSON.Entry>)chunks.data))
                    {
                        BJSON.Field f_id = chunk_e.FindField("id");
                        BJSON.Field f2_sha1 = chunk_e.FindField("sha1");
                        BJSON.Field f_casPatchType2 = chunk_e.FindField("casPatchType");
                        if (f_id != null && Helpers.ByteArrayCompare((byte[])f_id.data, chunkidbuff))
                        {
                            DbgPrint("  Found chunk");
                            found = true;
                            if (f_casPatchType2 != null)
                                f_casPatchType2.data = BitConverter.GetBytes((int)1);
                            else
                            {
                                f_casPatchType2 = new BJSON.Field();
                                f_casPatchType2.fieldname = "casPatchType";
                                f_casPatchType2.type = 8;
                                f_casPatchType2.data = BitConverter.GetBytes((int)1);
                                chunk_e.fields.Add(f_casPatchType2);
                            }
                            DbgPrint("  casPatchType set to 1");
                            f2_sha1.data = newsha1;
                            sb.Save();
                            found = true;
                            DbgPrint("  Replaced chunk sha1 and saved SB file");
                            DbgPrint("  Job successfull!");
                            break;
                        }
                    }
                    if (!found)
                        DbgPrint("  Error: Could not find Chunk by id");
                }
            }
        }

        public bool ImportBundleFromBase(TOCFile toc, string tocpath, int index, string bpath)
        {
            DbgPrint("  Its a base reference! Copying in from base...");
            //Find base toc
            string basepath = GlobalStuff.FindSetting("gamepath");
            if (!File.Exists(basepath + tocpath))
            {
                DbgPrint("Error: base TOC file not found, skipping!");
                return false;
            }
            TOCFile otoc = new TOCFile(basepath + tocpath);
            //get base bundle data
            byte[] buff = otoc.ExportBundleDataByPath(bpath);
            if (buff.Length == 0)
            {
                DbgPrint("Error: base bundle not found, skipping!");
                return false;
            }
            //get old sb file
            string oldSBpath = outputPath + Path.GetDirectoryName(tocpath) + "\\" + Path.GetFileNameWithoutExtension(tocpath) + ".sb";
            if (!File.Exists(oldSBpath))
            {
                DbgPrint("Error: patch SB file not found, skipping!");
                return false;
            }
            DbgPrint("  Got copy, recompiling...");
            //recompiling new sb in memory
            MemoryStream newSB = new MemoryStream();
            FileStream oldSB = new FileStream(oldSBpath, FileMode.Open, FileAccess.Read);
            long glob_off = 0;
            BJSON.Entry root = toc.lines[0];
            BJSON.Field bundles = root.fields[0];
            int count = ((List<BJSON.Entry>)bundles.data).Count();
            DbgPrint("  Recompiling SB...");
            //put one bundle after another that is not base as defined in toc
            for (int i = 0; i < count; i++)
            {
                //get entry infos
                BJSON.Entry b = ((List<BJSON.Entry>)bundles.data)[i];
                BJSON.Field f_offset = b.FindField("offset");
                BJSON.Field f_size = b.FindField("size");
                BJSON.Field f_isBase = b.FindField("base");
                //if not our target and not copied from base, copy from old SB
                if (i != index && f_isBase == null)
                {
                    int size = BitConverter.ToInt32((byte[])f_size.data, 0);
                    CopyFileStream(oldSB, newSB, BitConverter.ToInt64((byte[])f_offset.data, 0), size);
                    f_offset.data = BitConverter.GetBytes(glob_off);
                    glob_off += size;
                }
                //if target, replace data, make delta
                if (i == index)
                {
                    f_offset.data = BitConverter.GetBytes(glob_off);
                    f_size.data = BitConverter.GetBytes(buff.Length);
                    f_isBase.fieldname = "delta";
                    newSB.Write(buff, 0, buff.Length);
                    glob_off += buff.Length;
                }
            }
            oldSB.Close();
            //rebuilding new SB
            oldSB = new FileStream(oldSBpath, FileMode.Create, FileAccess.Write);
            //creating bundle header field
            MemoryStream t = new MemoryStream();
            Helpers.WriteLEB128(t, (int)newSB.Length);
            newSB.WriteByte(0);
            int varsize = (int)t.Length;
            //add root entry
            oldSB.WriteByte(0x82);
            Helpers.WriteLEB128(oldSB, varsize + 9 + (int)newSB.Length);
            byte[] buff2 = { 0x01, 0x62, 0x75, 0x6E, 0x64, 0x6C, 0x65, 0x73, 0x00 };
            oldSB.Write(buff2, 0, 9);
            oldSB.Write(t.ToArray(), 0, varsize);
            //header done, grab header offset and put all bundles
            int rel_off = (int)oldSB.Position;
            oldSB.Write(newSB.ToArray(), 0, (int)newSB.Length);
            oldSB.Close();
            DbgPrint("  Recompiling TOC...");
            //correct offsets in toc by new adding header offset
            count = ((List<BJSON.Entry>)bundles.data).Count();
            for (int i = 0; i < count; i++)
            {
                BJSON.Entry b = ((List<BJSON.Entry>)bundles.data)[i];
                BJSON.Field f_offset = b.FindField("offset");
                BJSON.Field f_isBase = b.FindField("base");
                //if is in sb file, update
                if (f_isBase == null)
                {
                    long off = BitConverter.ToInt64((byte[])f_offset.data, 0);
                    off += rel_off;
                    f_offset.data = BitConverter.GetBytes(off);
                }
            }
            toc.Save();
            DbgPrint("  Bundle imported");
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

        public byte[] CreateCASContainer(Mod.ModJob mj)
        {
            //generating cas data
            DbgPrint("Creating CAS container for new data");
            byte[] data = CASFile.MakeHeaderAndContainer(mj.data);
            DbgPrint("Finding free CAS...");
            int casindex = 99;
            FileStream fs;
            long pos;
            while (File.Exists(outputPath + "Data\\cas_" + casindex.ToString("D2") + ".cas"))
            {
                fs = new FileStream(outputPath + "Data\\cas_" + casindex.ToString("D2") + ".cas", FileMode.Open, FileAccess.Read);
                fs.Seek(0, SeekOrigin.End);
                pos = fs.Position;
                fs.Close();
                if (pos < 0x70000000)
                    break;
                casindex--;
            }
            string caspath = outputPath + "Data\\cas_" + casindex.ToString("D2") + ".cas";
            if (!File.Exists(caspath))
                File.WriteAllBytes(caspath, new byte[0]);
            DbgPrint("Choosing : cas_" + casindex.ToString("D2") + ".cas");
            fs = new FileStream(caspath, FileMode.Open, FileAccess.Read);
            //get new offset
            fs.Seek(0, SeekOrigin.End);
            pos = fs.Position;
            fs.Close();
            fs = new FileStream(caspath, FileMode.Append, FileAccess.Write);
            fs.Write(data, 0, data.Length);
            fs.Close();
            //creating new CAT entry with new SHA1
            DbgPrint("Appended Data, updating CAT file...");
            if (!File.Exists(outputPath + "Data\\cas.cat"))
            {
                DbgPrint("Error: cant find CAT file, skipping!");
                return new byte[0];
            }
            fs = new FileStream(outputPath + "Data\\cas.cat", FileMode.Append, FileAccess.Write);
            fs.Write(data, 4, 0x14);
            byte[] newsha1 = new byte[0x14];
            for (int i = 0; i < 0x14; i++)
                newsha1[i] = data[i + 4];
            fs.Write(BitConverter.GetBytes((int)pos + 0x20), 0, 4);
            fs.Write(BitConverter.GetBytes((int)data.Length - 0x20), 0, 4);
            fs.Write(BitConverter.GetBytes(casindex), 0, 4);
            fs.Close();
            return newsha1;
        }
    }
}
