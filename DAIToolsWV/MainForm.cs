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
using System.Reflection;
using DAILibWV;

namespace DAIToolsWV
{
    public partial class MainForm : Form
    {
        public MainForm()
        {
            InitializeComponent();
        }

        DebugOutputWindow debug;
        bool init = false;

        private void MainForm_Load(object sender, EventArgs e)
        {
            Version v = Assembly.GetExecutingAssembly().GetName().Version;
            this.Text = "DAI Tools WV - build : " + v.ToString();
        }

        private void Init()
        {
            init = true;
            debug = new DebugOutputWindow();
            OpenMaxed(debug);
            Debug.SetBox(debug.rtb1);
            Debug.LogLn("Hello there! Im starting...");
            Application.DoEvents();
            bool exist = DBAccess.CheckIfDBExists();
            Debug.LogLn("Database file found : " + exist);
            if (!exist)
            {
                DialogResult result = MessageBox.Show("No database found, do you want to create a new one?", "Database", MessageBoxButtons.YesNo);
                if (result == System.Windows.Forms.DialogResult.Yes)
                {
                    DBAccess.CreateDataBase();
                    Debug.LogLn("Database file created");
                }
                else
                    this.Close();
            }
            DBAccess.LoadSettings();
            bool needsScan = DBAccess.CheckIfScanIsNeeded();
            Debug.LogLn("Initial Scan needed : " + needsScan);
            if (needsScan)
            {
                DialogResult result = MessageBox.Show("Database is empty, do you want to start initial scan?", "Database", MessageBoxButtons.YesNo);
                if (result == System.Windows.Forms.DialogResult.Yes)
                {
                    OpenFileDialog d = new OpenFileDialog();
                    d.Filter = "DragonAgeInquisition.exe|DragonAgeInquisition.exe";
                    if (d.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                        DBAccess.StartScan(Path.GetDirectoryName(d.FileName) + "\\");
                    Debug.LogLn("Initial Scan Done");
                }
                else
                    this.Close();
            }
            Debug.LogLn("I'm ready!");
        }

        private void OpenMaxed(Form f)
        {
            f.MdiParent = this;
            f.Show();
            f.WindowState = FormWindowState.Maximized;
        }

        private void MainForm_Activated(object sender, EventArgs e)
        {
            if(!init)
                Init();
        }

        private void tOCToolToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenMaxed(new FileTools.TOCTool());
        }

        private void sBToolToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenMaxed(new FileTools.SBTool());
        }

        private void mFTToolToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenMaxed(new FileTools.MFTTool());
        }

        private void hexToolToolStripMenuItem_Click_1(object sender, EventArgs e)
        {
            Form f = new FileTools.HexTool();
            f.Show();
        }

        private void initFsToolToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenMaxed(new FileTools.INITFSTool());
        }

        private void eBXToolToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenMaxed(new ContentTools.EBXTool());
        }

        private void rESETALLToolStripMenuItem_Click(object sender, EventArgs e)
        {
            DialogResult result = MessageBox.Show("Do you really want to reset the database and restart?", "Database", MessageBoxButtons.YesNo);
            if (result == System.Windows.Forms.DialogResult.Yes)
            { 
                if(File.Exists("database.sqlite"))
                    File.Delete("database.sqlite");
                Application.Restart();
            }
        }

        private void cASContainerCreatorToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenMaxed(new FileTools.CASContainerCreator());
        }

        private void textureToolToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenMaxed(new ContentTools.TextureTool());
        }

        private void modEditorToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenMaxed(new ModTools.ModEditor());
        }

        private void modRunnerToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenMaxed(new ModTools.ModRunner());
        }

        private void bundleBuilderToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenMaxed(new FileTools.BundleBuilder());
        }

        private void fileBrowserToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenMaxed(new Browser.FileBrowser());
        }

        private void bundleBrowserToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenMaxed(new Browser.BundleBrowser());
        }

        private void eBXBrowserToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenMaxed(new Browser.EBXBrowser());
        }

        private void textureBrowserToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenMaxed(new Browser.TextureBrowser());
        }

        private void talktableToolToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenMaxed(new ContentTools.TalktableTool());
        }

        private void sHA1LookupToolStripMenuItem_Click_1(object sender, EventArgs e)
        {
            OpenMaxed(new ContentTools.SHA1Lookup());
        }

        private void hexToStringToolToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenMaxed(new GeneralTools.HexToString());
        }

        private void cATRepairToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenMaxed(new GeneralTools.CATrepair());
        }

        private void folderCompareToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenMaxed(new GeneralTools.FolderCompare());
        }

        private void rESBrowserToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenMaxed(new Browser.RESBrowser());
        }

        private void meshBrowserToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenMaxed(new Browser.MeshBrowser());
        }

        private void meshToolToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenMaxed(new ContentTools.MeshTool());
        }
    }
}
