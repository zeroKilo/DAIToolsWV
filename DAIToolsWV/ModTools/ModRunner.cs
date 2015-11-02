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
                case 2:
                    sb.AppendLine("Affected ressource path: " + mj.respath);
                    sb.AppendLine("Affected bundle paths (" + mj.bundlePaths.Count + "):");                    
                    foreach (string p in mj.bundlePaths)
                        sb.AppendLine("\t" + p);
                    sb.AppendLine("Affected toc files (" + mj.tocPaths.Count + "):");
                    foreach (string p in mj.tocPaths)
                        sb.AppendLine("\t" + p);
                    break;
                case 1:
                    sb.AppendLine("Ressource type (" + mj.restype + "):");
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
            Application.DoEvents();
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
                case 1:
                    RunBinaryResJob(mj);
                    break;
                case 2:
                    RunBinaryEbxJob(mj);
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
                        string from = GlobalStuff.FindSetting("gamepath") + tocpath;
                        string to = outputPath + tocpath;
                        Directory.CreateDirectory(Path.GetDirectoryName(to) + "\\");
                        DbgPrint("TOC file not found, copying from base!");
                        try
                        {
                            File.Copy(from, to);
                        }
                        catch (Exception)
                        {
                            DbgPrint("Error: TOC file not found, can not copy from base!\n Tried to copy from:\n\t" + from + "\n to:\n\t" + to);
                            return;
                        }
                        from = GlobalStuff.FindSetting("gamepath") + tocpath.ToLower().Replace(".toc",".sb");
                        to = outputPath + tocpath.ToLower().Replace(".toc", ".sb");
                        Directory.CreateDirectory(Path.GetDirectoryName(to) + "\\");
                        try
                        {
                            File.Copy(from, to);
                        }
                        catch (Exception)
                        {
                            DbgPrint("Error: SB file not found, can not copy from base!\n Tried to copy from:\n\t" + from + "\n to:\n\t" + to);
                            return;
                        }
                        DbgPrint("Fixing layout.toc...");
                        TOCFile toc = new TOCFile(outputPath + "Data\\layout.toc");
                        BJSON.Entry root = toc.lines[0];
                        BJSON.Field sbun = root.FindField("superBundles");
                        List<BJSON.Entry> list = (List<BJSON.Entry>)sbun.data;
                        BJSON.Entry ne = new BJSON.Entry();
                        ne.type = 0x82;
                        ne.fields = new List<BJSON.Field>();
                        ne.fields.Add(new BJSON.Field(7, "name", tocpath.Replace(".toc", "")));
                        ne.fields.Add(new BJSON.Field(6, "delta", true));
                        list.Add(ne);
                        sbun.data = list;
                        toc.Save();
                        DbgPrint("SuperBundle added");
                    }
                }
                else
                {
                    if (!File.Exists(outputPath + Helpers.SkipSubFolder(tocpath, 2)))
                    {
                        string from = GlobalStuff.FindSetting("gamepath") + tocpath;
                        string to = outputPath + Helpers.SkipSubFolder(tocpath, 2);
                        Directory.CreateDirectory(Path.GetDirectoryName(to) + "\\");
                        DbgPrint("TOC file not found, copying from base!");
                        try
                        {
                            File.Copy(from, to);
                        }
                        catch (Exception)
                        {
                            DbgPrint("Error: TOC file not found, can not copy from base!\n Tried to copy from:\n\t" + from + "\n to:\n\t" + to);
                            return;
                        }
                        from = GlobalStuff.FindSetting("gamepath") + tocpath.ToLower().Replace(".toc", ".sb");
                        to = outputPath + Helpers.SkipSubFolder(tocpath, 2).ToLower().Replace(".toc", ".sb");
                        Directory.CreateDirectory(Path.GetDirectoryName(to) + "\\");
                        try
                        {
                            File.Copy(from, to);
                        }
                        catch (Exception)
                        {
                            DbgPrint("Error: SB file not found, can not copy from base!\n Tried to copy from:\n\t" + from + "\n to:\n\t" + to);
                            return;
                        }
                        DbgPrint("Fixing layout.toc...");
                        TOCFile toc = new TOCFile(outputPath + "Data\\layout.toc");
                        BJSON.Entry root = toc.lines[0];
                        BJSON.Field sbun = root.FindField("superBundles");
                        List<BJSON.Entry> list = (List<BJSON.Entry>)sbun.data;
                        BJSON.Entry ne = new BJSON.Entry();
                        ne.type = 0x82;
                        ne.fields = new List<BJSON.Field>();
                        ne.fields.Add(new BJSON.Field(7, "name", Helpers.SkipSubFolder(tocpath, 2).Replace(".toc", "")));
                        ne.fields.Add(new BJSON.Field(6, "delta", true));
                        list.Add(ne);
                        sbun.data = list;
                        toc.Save();
                        DbgPrint("SuperBundle added");
                    }
                }
            }
            DbgPrint("All found.");
            //create cas data
            int newcompressedsize = 0;
            byte[] newsha1 = CreateCASContainer(mj.data, out newcompressedsize);
            if (newsha1.Length != 0x14)
            {
                DbgPrint("Error: could not create CAS data, aborting!");
                return;
            }
            //walk through affected toc files
            foreach (string tocpath in mj.tocPaths)
                RunTextureResJob(mj, tocpath, newsha1, newcompressedsize);
        }

        public void RunBinaryResJob(Mod.ModJob mj)
        {
            DbgPrint("Running Binary Ressource Replacement Job for: " + mj.respath);
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
            int newcompressedsize = 0;
            byte[] newsha1 = CreateCASContainer(mj.data, out newcompressedsize);
            if (newsha1.Length != 0x14)
            {
                DbgPrint("Error: could not create CAS data, aborting!");
                return;
            }
            //walk through affected toc files
            foreach (string tocpath in mj.tocPaths)
                if (!tocpath.ToLower().Contains("\\patch\\"))
                    RunRessourceJob(mj, tocpath, newsha1, newcompressedsize);
        }

        public void RunBinaryEbxJob(Mod.ModJob mj)
        {
            DbgPrint("Running Binary EBX Replacement Job for: " + mj.respath);
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
            int newcompressedsize = 0;
            byte[] newsha1 = CreateCASContainer(mj.data, out newcompressedsize);
            if (newsha1.Length != 0x14)
            {
                DbgPrint("Error: could not create CAS data, aborting!");
                return;
            }
            //walk through affected toc files
            foreach (string tocpath in mj.tocPaths)
                if (!tocpath.ToLower().Contains("\\patch\\"))
                    RunEbxResJob(mj, tocpath, newsha1, newcompressedsize);
        }

        public void RunTextureResJob(Mod.ModJob mj, string tocpath, byte[] newsha1, int newcompressedsize)
        {
            DbgPrint("Loading : " + tocpath);
            TOCFile toc = null;
            if (!tocpath.ToLower().StartsWith("update"))
                toc = new TOCFile(outputPath + tocpath);
            else
                toc = new TOCFile(outputPath + Helpers.SkipSubFolder(tocpath, 2));
            //walk through affected bundles
            if (toc.iscas)
                foreach (string bpath in mj.bundlePaths)
                    RunTextureResJobOnBundle(mj, toc, tocpath, newsha1, bpath, newcompressedsize);
            else
                foreach (string bpath in mj.bundlePaths)
                    RunTextureResJobOnBundleBinary(mj, toc, tocpath, bpath);
            UpdateTOC(toc);
        }

        public void RunRessourceJob(Mod.ModJob mj, string tocpath, byte[] newsha1, int newcompressedsize)
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
                RunRessourceJobOnBundle(mj, toc, tocpath, newsha1, bpath, newcompressedsize);
            UpdateTOC(toc);
        }

        public void RunEbxResJob(Mod.ModJob mj, string tocpath, byte[] newsha1, int newcompressedsize)
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
                RunEbxResJobOnBundle(mj, toc, tocpath, newsha1, bpath, newcompressedsize);
            UpdateTOC(toc);
        }

        public void RunTextureResJobOnBundle(Mod.ModJob mj, TOCFile toc, string tocpath, byte[] newsha1, string bpath, int newcompressedsize)
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
                BJSON.Field bundles = root.FindField("bundles");
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
                bundles = root.FindField("bundles");
                List<BJSON.Entry> bundle_list =(List<BJSON.Entry>)bundles.data;
                //find right bundle
                for (int i = 0; i < bundle_list.Count; i++)
                {
                    bun = bundle_list[i];
                    BJSON.Field ebx = bun.FindField("ebx");
                    BJSON.Field res = bun.FindField("res");
                    BJSON.Field chunks = bun.FindField("chunks");
                    BJSON.Field path = bun.FindField("path");
                    if (!(path != null && (string)path.data == bpath) || res == null || chunks == null)
                        continue;
                    bool found = false;
                    byte[] chunkidbuff = new byte[16];
                    byte[] newchunkid = new byte[16];
                    //find right res entry
                    List<BJSON.Entry> res_list = (List<BJSON.Entry>)res.data;
                    for (int j = 0; j < res_list.Count; j++) 
                    {
                        BJSON.Entry res_e = res_list[j];
                        BJSON.Field f_sha1 = res_e.FindField("sha1");
                        BJSON.Field f_name = res_e.FindField("name");
                        BJSON.Field f_size = res_e.FindField("size");
                        BJSON.Field f_osize = res_e.FindField("originalSize");
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
                            for (int k = 0; k < 16; k++)
                                chunkidbuff[k] = resdata[k + 0x1C];
                            DbgPrint("  Found chunk id : " + Helpers.ByteArrayToHexString(chunkidbuff));
                            newchunkid = Guid.NewGuid().ToByteArray();
                            DbgPrint("  Creating new chunk id : " + Helpers.ByteArrayToHexString(newchunkid));
                            for (int k = 0; k < 16; k++)
                                resdata[k + 0x1C] = newchunkid[k];
                            int newrescompsize = 0;
                            byte[] newressha1 = CreateCASContainer(resdata, out newrescompsize, "  ");
                            DbgPrint("  Creating new res sha1 : " + Helpers.ByteArrayToHexString(newressha1));
                            f_sha1.data = newressha1;
                            DbgPrint("  Updating res size : " + resdata.Length);
                            f_size.data = BitConverter.GetBytes((long)newrescompsize);
                            f_osize.data = BitConverter.GetBytes((long)resdata.Length);
                            if (f_casPatchType != null)
                            {
                                if (BitConverter.ToInt32((byte[])f_casPatchType.data, 0) != 1)
                                {
                                    DbgPrint("  CasPatchType: found and set to 1!");
                                    f_casPatchType.data = BitConverter.GetBytes((int)1);
                                }
                                else
                                    DbgPrint("  CasPatchType: found and is fine!");
                            }
                            else
                            {
                                f_casPatchType = new BJSON.Field();
                                f_casPatchType.fieldname = "casPatchType";
                                f_casPatchType.type = 8;
                                f_casPatchType.data = BitConverter.GetBytes((int)1);
                                res_e.fields.Add(f_casPatchType);
                                DbgPrint("  CasPatchType: added and set to 1!");
                            }
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
                    List<BJSON.Entry> chunk_list = (List<BJSON.Entry>)chunks.data;
                    for (int j = 0; j < chunk_list.Count; j++) 
                    {
                        BJSON.Entry chunk_e = chunk_list[j];
                        BJSON.Field f_id = chunk_e.FindField("id");
                        BJSON.Field f_size = chunk_e.FindField("size");
                        BJSON.Field f_rangeStart = chunk_e.FindField("rangeStart");
                        BJSON.Field f_rangeEnd = chunk_e.FindField("rangeEnd");
                        BJSON.Field f_logicalOffset = chunk_e.FindField("logicalOffset");
                        BJSON.Field f_logicalSize = chunk_e.FindField("logicalSize");
                        BJSON.Field f2_sha1 = chunk_e.FindField("sha1");
                        BJSON.Field f_casPatchType2 = chunk_e.FindField("casPatchType");
                        if (f_id != null && Helpers.ByteArrayCompare((byte[])f_id.data, chunkidbuff))
                        {
                            DbgPrint("  Found chunk");
                            f_id.data = newchunkid;
                            found = true;
                            if (f_casPatchType2 != null)
                            {
                                if (BitConverter.ToInt32((byte[])f_casPatchType2.data, 0) != 1)
                                {
                                    DbgPrint("  CasPatchType: found and set to 1!");
                                    f_casPatchType2.data = BitConverter.GetBytes((int)1);
                                }
                                else
                                    DbgPrint("  CasPatchType: found and is fine!");
                            }
                            else
                            {
                                f_casPatchType2 = new BJSON.Field();
                                f_casPatchType2.fieldname = "casPatchType";
                                f_casPatchType2.type = 8;
                                f_casPatchType2.data = BitConverter.GetBytes((int)1);
                                chunk_e.fields.Add(f_casPatchType2);
                                DbgPrint("  CasPatchType: added and set to 1!");
                            }
                            f_size.data = BitConverter.GetBytes(newcompressedsize);
                            if (f_rangeStart != null)
                                f_rangeStart.data = BitConverter.GetBytes((int)0);
                            if (f_rangeEnd != null)
                                f_rangeEnd.data = BitConverter.GetBytes(newcompressedsize);
                            if (f_logicalOffset != null)
                                f_logicalOffset.data = BitConverter.GetBytes((int)0);
                            if (f_logicalSize != null)
                                f_logicalSize.data = BitConverter.GetBytes(mj.data.Length);
                            f2_sha1.data = newsha1;
                            DbgPrint("  Updated chunk size : " + mj.data.Length);
                            CalcTotalSize(bun);
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

        public void RunTextureResJobOnBundleBinary(Mod.ModJob mj, TOCFile toc, string tocpath, string bpath)
        {
            int count = 0;
            int index = -1;
            foreach (TOCFile.TOCBundleInfoStruct buni in toc.bundles)
                if (count++ > -1 && bpath.ToLower() == buni.id.ToLower())
                {
                    DbgPrint(" Found bundle : " + bpath);
                    index = count - 1;
                    break;
                }
            //if bundle found
            if (index != -1)
            {
                //find out if base, delta or nothing
                BJSON.Entry root = toc.lines[0];
                BJSON.Field bundles = root.FindField("bundles");
                BJSON.Entry bun = ((List<BJSON.Entry>)bundles.data)[index];
                BJSON.Field isDeltaField = bun.FindField("delta");
                BJSON.Field isBaseField = bun.FindField("base");
                //if has base or delta prop, still from patch
                if (isBaseField != null)
                    if (!ImportBundleBinaryFromBase(toc, tocpath, bpath))
                        return;
                toc = new TOCFile(toc.MyPath);//reload toc
                byte[] bundledataraw = toc.ExportBundleDataByPath(bpath);
                uint test = BitConverter.ToUInt32(bundledataraw, 4);
                if (test != 0xD58E799D)
                {
                    DbgPrint("  Its a real delta bundle, importing from base...");
                    if (!ImportBundleBinaryFromBase(toc, tocpath, bpath, false))
                        return;
                    toc.Save();
                    toc = new TOCFile(toc.MyPath);//reload toc
                }
                DbgPrint("  Updating SB file with new data...");
                bundledataraw = toc.ExportBundleDataByPath(bpath);
                BinaryBundle bundle = new BinaryBundle(new MemoryStream(bundledataraw));
                bool found = false;
                byte[] chunkidbuff = new byte[16];
                byte[] newchunkid = new byte[16];
                //find right res entry
                for (int j = 0; j < bundle.ResList.Count; j++)
                    if (bundle.ResList[j]._name.ToLower() == mj.respath.ToLower())
                    {
                        //get res data and extract chunk id
                        for (int k = 0; k < 16; k++)
                            chunkidbuff[k] = bundle.ResList[j]._data[k + 0x1C];
                        DbgPrint("  Found chunk id : " + Helpers.ByteArrayToHexString(chunkidbuff));
                        newchunkid = Guid.NewGuid().ToByteArray();
                        DbgPrint("  Creating new chunk id : " + Helpers.ByteArrayToHexString(newchunkid));
                        for (int k = 0; k < 16; k++)
                            bundle.ResList[j]._data[k + 0x1C] = newchunkid[k];
                        found = true;
                    }
                if (!found)
                {
                    DbgPrint("  Error: cant find res, skipping!");
                    return;
                }
                found = false;
                //find right chunk entry
                MemoryStream m3 = new MemoryStream();
                Helpers.WriteLEInt(m3, BitConverter.ToInt32(chunkidbuff, 0));
                Helpers.WriteLEUShort(m3, BitConverter.ToUInt16(chunkidbuff, 4));
                Helpers.WriteLEUShort(m3, BitConverter.ToUInt16(chunkidbuff, 6));
                m3.Write(chunkidbuff, 8, 8);
                byte[] chunkidswapped = m3.ToArray();
                for (int j = 0; j < bundle.ChunkList.Count; j++)
                    if (Helpers.ByteArrayCompare(bundle.ChunkList[j].id, chunkidswapped))
                    {
                        DbgPrint("  Found chunk");
                        found = true;
                        BinaryBundle.ChunkEntry chunk = bundle.ChunkList[j];
                        m3 = new MemoryStream();
                        Helpers.WriteLEInt(m3, BitConverter.ToInt32(newchunkid, 0));
                        Helpers.WriteLEUShort(m3, BitConverter.ToUInt16(newchunkid, 4));
                        Helpers.WriteLEUShort(m3, BitConverter.ToUInt16(newchunkid, 6));
                        m3.Write(newchunkid, 8, 8);
                        chunk.id = m3.ToArray();
                        chunk._data = mj.data;
                        bundle.ChunkList[j] = chunk;
                        break;
                    }
                if (!found)
                {
                    DbgPrint("  Error: Could not find Chunk by id");
                    return;
                }
                DbgPrint("  Recompiling bundle...");
                MemoryStream m = new MemoryStream();
                bundle.Save(m);
                DbgPrint("  Recompiling sb...");
                MemoryStream m2 = new MemoryStream();
                List<BJSON.Entry> list = ((List<BJSON.Entry>)bundles.data);
                foreach (TOCFile.TOCBundleInfoStruct buni in toc.bundles)
                    if (!buni.isbase)
                    {
                        byte[] buff = new byte[0];
                        if (bpath.ToLower() != buni.id.ToLower())
                            buff = toc.ExportBundleDataByPath(buni.id);
                        else
                            buff = m.ToArray();
                        for (int i = 0; i < list.Count; i++)
                            if ((string)list[i].FindField("id").data == buni.id)
                            {
                                BJSON.Entry e = list[i];
                                BJSON.Field f = e.fields[e.FindFieldIndex("offset")];
                                f.data = BitConverter.GetBytes(m2.Position);
                                e.fields[e.FindFieldIndex("offset")] = f;
                                f = e.fields[e.FindFieldIndex("size")];
                                f.data = BitConverter.GetBytes(buff.Length);
                                e.fields[e.FindFieldIndex("size")] = f;
                                list[i] = e;
                                break;
                            }
                        m2.Write(buff, 0, buff.Length);
                    }
                bundles.data = list;
                root.fields[root.FindFieldIndex("bundles")] = bundles;
                toc.lines[0] = root;
                DbgPrint("  Updating TOC...");
                toc.Save();
                toc = new TOCFile(toc.MyPath);//reload toc
                DbgPrint("  Saving sb...");
                if (tocpath.ToLower().Contains("update"))
                    File.WriteAllBytes(outputPath + Helpers.SkipSubFolder(tocpath.ToLower().Replace(".toc", ".sb"), 2), m2.ToArray());
                else
                    File.WriteAllBytes(outputPath + tocpath.ToLower().Replace(".toc", ".sb"), m2.ToArray());
                DbgPrint("  Job successfull!");
            }
        }

        public void RunRessourceJobOnBundle(Mod.ModJob mj, TOCFile toc, string tocpath, byte[] newsha1, string bpath, int newcompressedsize)
        {
            int count = 0;
            int index = -1;
            foreach (TOCFile.TOCBundleInfoStruct buni in toc.bundles)
                if (count++ > -1 && bpath.ToLower() == buni.id.ToLower())
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
                BJSON.Field bundles = root.FindField("bundles");
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
                bundles = root.FindField("bundles");
                List<BJSON.Entry> bundle_list = (List<BJSON.Entry>)bundles.data;
                //find right bundle
                for (int i = 0; i < bundle_list.Count; i++)
                {
                    bun = bundle_list[i];
                    BJSON.Field ebx = bun.FindField("ebx");
                    BJSON.Field res = bun.FindField("res");
                    BJSON.Field path = bun.FindField("path");
                    if (!(path != null && ((string)path.data).ToLower() == bpath.ToLower()) || res == null)
                        continue;
                    bool found = false;
                    //find right res entry
                    List<BJSON.Entry> res_list = (List<BJSON.Entry>)res.data;
                    for (int j = 0; j < res_list.Count; j++)
                    {
                        BJSON.Entry res_e = res_list[j];
                        BJSON.Field f_sha1 = res_e.FindField("sha1");
                        BJSON.Field f_name = res_e.FindField("name");
                        BJSON.Field f_size = res_e.FindField("size");
                        BJSON.Field f_osize = res_e.FindField("originalSize");
                        BJSON.Field f_casPatchType = res_e.FindField("casPatchType");
                        if (f_name != null && ((string)f_name.data).ToLower() == mj.respath.ToLower() && f_sha1 != null)
                        {
                            //get res data
                            byte[] sha1buff = (byte[])f_sha1.data;
                            DbgPrint("  Found res sha1 : " + Helpers.ByteArrayToHexString(sha1buff));
                            f_sha1.data = newsha1;
                            DbgPrint("  Replaced res sha1 with : " + Helpers.ByteArrayToHexString(newsha1));
                            DbgPrint("  Updating res size : " + mj.data.Length);
                            f_size.data = BitConverter.GetBytes((long)newcompressedsize);
                            f_osize.data = BitConverter.GetBytes((long)mj.data.Length);
                            if (f_casPatchType != null)
                            {
                                if (BitConverter.ToInt32((byte[])f_casPatchType.data, 0) != 1)
                                {
                                    DbgPrint("  CasPatchType: found and set to 1!");
                                    f_casPatchType.data = BitConverter.GetBytes((int)1);
                                }
                                else
                                    DbgPrint("  CasPatchType: found and is fine!");
                            }
                            else
                            {
                                f_casPatchType = new BJSON.Field();
                                f_casPatchType.fieldname = "casPatchType";
                                f_casPatchType.type = 8;
                                f_casPatchType.data = BitConverter.GetBytes((int)1);
                                res_e.fields.Add(f_casPatchType);
                                DbgPrint("  CasPatchType: added and set to 1!");
                            }
                            CalcTotalSize(bun);
                            sb.Save();
                            DbgPrint("  Job successfull!");
                            found = true;
                        }
                    }
                    if (!found)
                    {
                        DbgPrint("  cant find res, adding it!");
                        BJSON.Entry newres = new BJSON.Entry();
                        newres.type = 0x82;
                        newres.fields = new List<BJSON.Field>();
                        newres.fields.Add(new BJSON.Field(7, "name", mj.respath));
                        newres.fields.Add(new BJSON.Field(0x10, "sha1", newsha1));
                        newres.fields.Add(new BJSON.Field(9, "size", BitConverter.GetBytes((long)newcompressedsize)));
                        newres.fields.Add(new BJSON.Field(9, "originalSize", BitConverter.GetBytes((long)mj.data.Length)));
                        newres.fields.Add(new BJSON.Field(0x8, "resType", Helpers.HexStringToByteArray(mj.restype)));
                        newres.fields.Add(new BJSON.Field(0x13, "resMeta", new byte[0x10]));
                        newres.fields.Add(new BJSON.Field(9, "resRid", BitConverter.GetBytes((long)0)));
                        newres.fields.Add(new BJSON.Field(8, "casPatchType", BitConverter.GetBytes((int)1)));
                        ((List<BJSON.Entry>)res.data).Add(newres);
                        CalcTotalSize(bun);
                        sb.Save();
                        DbgPrint("  Job successfull!");
                        break;
                    }                    
                }
            }
        }

        public void RunEbxResJobOnBundle(Mod.ModJob mj, TOCFile toc, string tocpath, byte[] newsha1, string bpath, int newcompressedsize)
        {
            int count = 0;
            int index = -1;
            foreach (TOCFile.TOCBundleInfoStruct buni in toc.bundles)
                if (count++ > -1 && bpath.ToLower() == buni.id.ToLower())
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
                BJSON.Field bundles = root.FindField("bundles");
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
                bundles = root.FindField("bundles");
                List<BJSON.Entry> bundle_list = (List<BJSON.Entry>)bundles.data;
                //find right bundle
                for (int i = 0; i < bundle_list.Count; i++)
                {
                    bun = bundle_list[i];
                    BJSON.Field ebx = bun.FindField("ebx");
                    BJSON.Field path = bun.FindField("path");
                    if (!(path != null && ((string)path.data).ToLower() == bpath.ToLower()) || ebx == null)
                        continue;
                    bool found = false;
                    //find right res entry
                    List<BJSON.Entry> ebx_list = (List<BJSON.Entry>)ebx.data;
                    for (int j = 0; j < ebx_list.Count; j++)
                    {
                        BJSON.Entry ebx_e = ebx_list[j];
                        BJSON.Field f_name = ebx_e.FindField("name");
                        BJSON.Field f_sha1 = ebx_e.FindField("sha1");
                        BJSON.Field f_size = ebx_e.FindField("size");
                        BJSON.Field f_osize = ebx_e.FindField("originalSize");
                        BJSON.Field f_casPatchType = ebx_e.FindField("casPatchType");
                        if (f_name != null && ((string)f_name.data).ToLower() == mj.respath.ToLower() && f_sha1 != null)
                        {
                            //get res data and extract chunk id
                            byte[] sha1buff = (byte[])f_sha1.data;
                            DbgPrint("  Found ebx sha1 : " + Helpers.ByteArrayToHexString(sha1buff));
                            f_sha1.data = newsha1;
                            DbgPrint("  Replaced ebx sha1 with : " + Helpers.ByteArrayToHexString(newsha1));
                            DbgPrint("  Updating ebx size : " + mj.data.Length);
                            f_size.data = BitConverter.GetBytes((long)newcompressedsize);
                            f_osize.data = BitConverter.GetBytes((long)mj.data.Length);
                            if (f_casPatchType != null)
                            {
                                if (BitConverter.ToInt32((byte[])f_casPatchType.data, 0) != 1)
                                {
                                    DbgPrint("  CasPatchType: found and set to 1!");
                                    f_casPatchType.data = BitConverter.GetBytes((int)1);
                                }
                                else
                                    DbgPrint("  CasPatchType: found and is fine!");
                            }
                            else
                            {
                                f_casPatchType = new BJSON.Field();
                                f_casPatchType.fieldname = "casPatchType";
                                f_casPatchType.type = 8;
                                f_casPatchType.data = BitConverter.GetBytes((int)1);
                                ebx_e.fields.Add(f_casPatchType);
                                DbgPrint("  CasPatchType: added and set to 1!");
                            }
                            CalcTotalSize(bun);
                            sb.Save();
                            DbgPrint("  Job successfull!");
                            found = true;
                        }
                    }
                    if (!found)
                    {
                        DbgPrint("  cant find ebx, adding it!");
                        BJSON.Entry newebx = new BJSON.Entry();
                        newebx.type = 0x82;
                        newebx.fields = new List<BJSON.Field>();
                        newebx.fields.Add(new BJSON.Field(7, "name", mj.respath));
                        newebx.fields.Add(new BJSON.Field(0x10, "sha1", newsha1));
                        newebx.fields.Add(new BJSON.Field(9, "size", BitConverter.GetBytes((long)newcompressedsize)));
                        newebx.fields.Add(new BJSON.Field(9, "originalSize", BitConverter.GetBytes((long)mj.data.Length)));
                        newebx.fields.Add(new BJSON.Field(8, "casPatchType", BitConverter.GetBytes((int)1)));
                        ((List<BJSON.Entry>)ebx.data).Add(newebx);
                        CalcTotalSize(bun);
                        sb.Save();
                        DbgPrint("  Job successfull!");
                        break;
                    }
                }
            }
        }

        public void CalcTotalSize(BJSON.Entry bun)
        {
            BJSON.Field totalsize = bun.FindField("totalSize");
            BJSON.Field ebx = bun.FindField("ebx");
            BJSON.Field res = bun.FindField("res");
            BJSON.Field chunks = bun.FindField("chunks");
            long size = 0;
            if (ebx != null)
                foreach (BJSON.Entry e in (List<BJSON.Entry>)ebx.data)
                    size += BitConverter.ToInt64((byte[])e.FindField("size").data, 0);
            if (res != null)
                foreach (BJSON.Entry e in (List<BJSON.Entry>)res.data)
                    size += BitConverter.ToInt64((byte[])e.FindField("size").data, 0);
            if (chunks != null)
                foreach (BJSON.Entry e in (List<BJSON.Entry>)chunks.data)
                    size += BitConverter.ToInt32((byte[])e.FindField("size").data, 0);
            totalsize.data = BitConverter.GetBytes(size);
            DbgPrint("  Total size calculated: " + size.ToString("X"));
        }

        public void UpdateTOC(TOCFile toc)
        {
            string path = toc.MyPath;
            path = path.ToLower().Replace(".toc",".sb");
            if (!toc.iscas)
                return;
            SBFile sb = new SBFile(path);
            BJSON.Entry root = sb.lines[0];
            BJSON.Field bundles = root.FindField("bundles");
            BJSON.Entry tocroot = toc.lines[0];
            BJSON.Field tocbundles = tocroot.FindField("bundles");
            foreach (BJSON.Entry bun in (List<BJSON.Entry>)bundles.data)
            {
                BJSON.Field f = bun.FindField("path");
                string bunpath = (string)f.data;
                foreach (BJSON.Entry tocbun in (List<BJSON.Entry>)tocbundles.data)
                {
                    BJSON.Field f2 = tocbun.FindField("id");
                    string bunpath2 = (string)f2.data;
                    if (bunpath == bunpath2)
                    {
                        BJSON.Field offset = tocbun.FindField("offset");
                        offset.data = BitConverter.GetBytes(bun.offset);
                        MemoryStream m = new MemoryStream();
                        BJSON.WriteEntry(m, bun);
                        string p = Path.GetDirectoryName(path);
                        File.WriteAllBytes(p + "\\" + Path.GetFileName(bunpath) + ".bin", m.ToArray());
                        BJSON.Field size = tocbun.FindField("size");
                        size.data = BitConverter.GetBytes((uint)m.Length);
                    }
                }
            }
            toc.Save();
            DbgPrint("  TOC file offsets updated.");

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
            //removing idata
            DbgPrint("  removing idata...");
            SBFile sb = new SBFile(oldSBpath);
            BJSON.Entry newroot = sb.lines[0];
            BJSON.Field newbundles = newroot.FindField("bundles");
            List<BJSON.Entry> newbun_list = (List<BJSON.Entry>)newbundles.data;
            for (int i = 0; i < newbun_list.Count; i++)
            {
                BJSON.Field f_res = newbun_list[i].FindField("res");
                List<BJSON.Entry> newres_list = (List<BJSON.Entry>)f_res.data;
                for (int j = 0; j < newres_list.Count; j++)
                    newres_list[j].RemoveField("idata");
            }
            sb.Save();
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

        public bool ImportBundleBinaryFromBase(TOCFile toc, string tocpath, string bpath, bool isbase = true)
        {
            if(isbase)
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
            string ttocpath = tocpath;
            if (tocpath.ToLower().Contains("update"))
                ttocpath = Helpers.SkipSubFolder(tocpath, 2);
            string oldSBpath = outputPath + Path.GetDirectoryName(ttocpath) + "\\" + Path.GetFileNameWithoutExtension(ttocpath) + ".sb";
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
                BJSON.Field f_id = b.FindField("id");
                BJSON.Field f_offset = b.FindField("offset");
                BJSON.Field f_size = b.FindField("size");
                BJSON.Field f_isBase = b.FindField("base");
                BJSON.Field f_isDelta = b.FindField("delta");
                //if not our target and not copied from base, copy from old SB
                if (((string)f_id.data).ToLower() != bpath && f_isBase == null)
                {
                    int size = BitConverter.ToInt32((byte[])f_size.data, 0);
                    CopyFileStream(oldSB, newSB, BitConverter.ToInt64((byte[])f_offset.data, 0), size);
                    f_offset.data = BitConverter.GetBytes(glob_off);
                    glob_off += size;
                }
                //if target, replace data, make delta
                if (((string)f_id.data).ToLower() == bpath)
                {
                    f_offset.data = BitConverter.GetBytes(glob_off);
                    f_size.data = BitConverter.GetBytes(buff.Length);
                    if (f_isBase != null)
                        f_isBase.fieldname = "delta";
                    newSB.Write(buff, 0, buff.Length);
                    glob_off += buff.Length;
                }
            }
            oldSB.Close();
            //rebuilding new SB
            oldSB = new FileStream(oldSBpath, FileMode.Create, FileAccess.Write);
            oldSB.Write(newSB.ToArray(), 0, (int)newSB.Length);
            oldSB.Close();            
            DbgPrint("  Recompiling TOC...");            
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

        public byte[] CreateCASContainer(byte[] newdata, out int compressedsize, string tab= "")
        {
            //generating cas data
            DbgPrint(tab + "Creating CAS container for new data");
            byte[] data = CASFile.MakeHeaderAndContainer(newdata);
            compressedsize = data.Length - 0x20;
            DbgPrint(tab + "Finding free CAS...");
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
            DbgPrint(tab + "Choosing : cas_" + casindex.ToString("D2") + ".cas");
            fs = new FileStream(caspath, FileMode.Open, FileAccess.Read);
            //get new offset
            fs.Seek(0, SeekOrigin.End);
            pos = fs.Position;
            fs.Close();
            fs = new FileStream(caspath, FileMode.Append, FileAccess.Write);
            fs.Write(data, 0, data.Length);
            fs.Close();
            //creating new CAT entry with new SHA1
            DbgPrint(tab + "Appended Data, updating CAT file...");
            if (!File.Exists(outputPath + "Data\\cas.cat"))
            {
                DbgPrint(tab + "Error: cant find CAT file, skipping!");
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
