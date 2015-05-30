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

        private void contentBrowserToolStripMenuItem_Click_1(object sender, EventArgs e)
        {
            OpenMaxed(new ContentBrowser());
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
    }
}
