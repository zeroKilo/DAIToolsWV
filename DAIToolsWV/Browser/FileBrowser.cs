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

namespace DAIToolsWV.Browser
{
    public partial class FileBrowser : Form
    {
        public FileBrowser()
        {
            InitializeComponent();
        }

        private void FileBrowser_Load(object sender, EventArgs e)
        {
            RefreshReal();
        }

        private void RefreshReal()
        {
            bool pathlist = toolStripButton4.Checked;
            bool withBase = toolStripButton1.Checked;
            bool withDLC = toolStripButton2.Checked;
            bool withPatch = toolStripButton3.Checked;
            List<string> Files = new List<string>();
            Files.AddRange(DBAccess.GetFiles("tocfiles", withBase, withDLC, withPatch));
            Files.AddRange(DBAccess.GetFiles("sbfiles", withBase, withDLC, withPatch));
            Files.Sort();
            if (pathlist)
            {
                listBox1.Items.Clear();
                listBox1.Items.AddRange(Files.ToArray());
                listBox1.BringToFront();
            }
            else
            {
                treeView1.Nodes.Clear();
                TreeNode t = new TreeNode();
                foreach (string file in Files)
                    t = Helpers.AddPath(t, file, "", '\\');
                treeView1.Nodes.Add(t);
                t.Expand();
                treeView1.BringToFront();
            }
        }

        private void toolStripButton1_Click(object sender, EventArgs e)
        {
            RefreshReal();
        }

        private void toolStripButton2_Click(object sender, EventArgs e)
        {
            RefreshReal();
        }

        private void toolStripButton3_Click(object sender, EventArgs e)
        {
            RefreshReal();
        }

        private void toolStripButton4_Click(object sender, EventArgs e)
        {
            toolStripButton4.Checked = true;
            toolStripButton5.Checked = false;
            RefreshReal();
        }

        private void toolStripButton5_Click(object sender, EventArgs e)
        {
            toolStripButton5.Checked = true;
            toolStripButton4.Checked = false;
            RefreshReal();
        }

        private void treeView1_AfterSelect(object sender, TreeViewEventArgs e)
        {
            treeView2.Nodes.Clear();
            if (treeView1.SelectedNode == null)
                return;
            string path = Path.GetDirectoryName(GlobalStuff.FindSetting("gamepath")) + Helpers.GetPathFromNode(treeView1.SelectedNode);
            PreviewFile(path);
        }

        public void PreviewFile(string path)
        {
            string ext = Path.GetExtension(path).ToLower();
            TOCFile toc;
            switch (ext)
            {
                case ".toc":
                    toc = new TOCFile(path);
                    if (toc != null && toc.lines != null)
                        Helpers.FillTreeFast(treeView2, toc.lines);
                    break;
                case ".sb":
                    string tocpath = Helpers.GetFileNameWithOutExtension(path) + ".toc";
                    if (File.Exists(tocpath))
                    {
                        toc = new TOCFile(tocpath);
                        if (toc != null && toc.lines != null && toc.lines.Count != 0)
                            foreach (BJSON.Field f in toc.lines[0].fields)
                                if (f.fieldname == "cas" && (bool)f.data)
                                {
                                    SBFile sb = new SBFile(path);
                                    if (sb != null && sb.lines != null)
                                        Helpers.FillTreeFast(treeView2, sb.lines);
                                    return;
                                }
                    }
                    break;
            }
        }

        private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            int n = listBox1.SelectedIndex;
            if (n == -1)
                return;
            string path = GlobalStuff.FindSetting("gamepath") + listBox1.Items[n];
            PreviewFile(path);
        }

        private void openInTOCToolToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string path = Path.GetDirectoryName(GlobalStuff.FindSetting("gamepath"));
            string path2 = "";
            if (toolStripButton4.Checked)
            {
                int n = listBox1.SelectedIndex;
                if (n == -1)
                    return;
                path2 = listBox1.Items[n].ToString();
            }
            else
            {
                TreeNode t = treeView1.SelectedNode;
                if (t == null)
                    return;
                path2 = t.Text;
                while (t.Parent != null)
                {
                    t = t.Parent;
                    path2 = t.Text + "\\" + path2;
                }
            }
            if (!path2.StartsWith("\\"))
                path += "\\";
            path += path2;
            FileTools.TOCTool toc = new FileTools.TOCTool();
            toc.toc = new TOCFile(path);
            toc.RefreshTree();
            toc.MdiParent = this.MdiParent;
            toc.WindowState = FormWindowState.Maximized;
            toc.Show();
        }

        private void openInSBToolToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string path = Path.GetDirectoryName(GlobalStuff.FindSetting("gamepath"));
            string path2 = "";
            if (toolStripButton4.Checked)
            {
                int n = listBox1.SelectedIndex;
                if (n == -1)
                    return;
                path2 = listBox1.Items[n].ToString();
            }
            else
            {
                TreeNode t = treeView1.SelectedNode;
                if (t == null)
                    return;
                path2 = t.Text;
                while (t.Parent != null)
                {
                    t = t.Parent;
                    path2 = t.Text + "\\" + path2;
                }
            }
            if (!path2.StartsWith("\\"))
                path += "\\";
            path += path2;
            FileTools.SBTool sb = new FileTools.SBTool();
            sb.LoadFile(path);
            sb.MdiParent = this.MdiParent;
            sb.WindowState = FormWindowState.Maximized;
            sb.Show();
        }

        private void contextMenuStrip1_Paint(object sender, PaintEventArgs e)
        {
            bool istoc = false;
            if (toolStripButton4.Checked)
            {
                int n = listBox1.SelectedIndex;
                if (n == -1)
                    return;
                if (listBox1.Items[n].ToString().EndsWith(".toc"))
                    istoc = true;
            }
            else
            {
                TreeNode t = treeView1.SelectedNode;
                if (t == null)
                    return;
                if (t.Text.EndsWith(".toc"))
                    istoc = true;
            }
            if (istoc)
            {
                openInTOCToolToolStripMenuItem.Visible = true;
                openInSBToolToolStripMenuItem.Visible = false;
            }
            else
            {
                openInTOCToolToolStripMenuItem.Visible = false;
                openInSBToolToolStripMenuItem.Visible = true;
            }
        }

    }
}
