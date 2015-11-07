using System.Windows.Forms;
namespace DAIToolsWV
{
    partial class MainForm
    {
        /// <summary>
        /// Erforderliche Designervariable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Verwendete Ressourcen bereinigen.
        /// </summary>
        /// <param name="disposing">True, wenn verwaltete Ressourcen gelöscht werden sollen; andernfalls False.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Vom Windows Form-Designer generierter Code

        /// <summary>
        /// Erforderliche Methode für die Designerunterstützung.
        /// Der Inhalt der Methode darf nicht mit dem Code-Editor geändert werden.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.toolTip = new System.Windows.Forms.ToolTip(this.components);
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.generalToolsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.hexToolToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.sHA1LookupToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.cASContainerCreatorToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.hexToStringToolToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.cATRepairToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.folderCompareToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.fileToolsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.tOCToolToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.sBToolToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.mFTToolToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.initFsToolToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.bundleBuilderToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.contentToolsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.eBXToolToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.textureToolToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.talktableToolToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.meshToolToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.browserToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.fileBrowserToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.bundleBrowserToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.eBXBrowserToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.rESBrowserToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.textureBrowserToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.meshBrowserToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.modToolsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.modEditorToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.modRunnerToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.rESETALLToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.supportDAIMODToolToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.menuStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // menuStrip1
            // 
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.generalToolsToolStripMenuItem,
            this.fileToolsToolStripMenuItem,
            this.contentToolsToolStripMenuItem,
            this.browserToolStripMenuItem,
            this.modToolsToolStripMenuItem,
            this.rESETALLToolStripMenuItem});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Size = new System.Drawing.Size(1119, 24);
            this.menuStrip1.TabIndex = 1;
            this.menuStrip1.Text = "menuStrip1";
            // 
            // generalToolsToolStripMenuItem
            // 
            this.generalToolsToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.hexToolToolStripMenuItem,
            this.sHA1LookupToolStripMenuItem,
            this.cASContainerCreatorToolStripMenuItem,
            this.hexToStringToolToolStripMenuItem,
            this.cATRepairToolStripMenuItem,
            this.folderCompareToolStripMenuItem});
            this.generalToolsToolStripMenuItem.Name = "generalToolsToolStripMenuItem";
            this.generalToolsToolStripMenuItem.Size = new System.Drawing.Size(84, 20);
            this.generalToolsToolStripMenuItem.Text = "General Tools";
            // 
            // hexToolToolStripMenuItem
            // 
            this.hexToolToolStripMenuItem.Name = "hexToolToolStripMenuItem";
            this.hexToolToolStripMenuItem.Size = new System.Drawing.Size(184, 22);
            this.hexToolToolStripMenuItem.Text = "Hex Tool";
            this.hexToolToolStripMenuItem.Click += new System.EventHandler(this.hexToolToolStripMenuItem_Click_1);
            // 
            // sHA1LookupToolStripMenuItem
            // 
            this.sHA1LookupToolStripMenuItem.Name = "sHA1LookupToolStripMenuItem";
            this.sHA1LookupToolStripMenuItem.Size = new System.Drawing.Size(184, 22);
            this.sHA1LookupToolStripMenuItem.Text = "SHA1 Lookup";
            this.sHA1LookupToolStripMenuItem.Click += new System.EventHandler(this.sHA1LookupToolStripMenuItem_Click_1);
            // 
            // cASContainerCreatorToolStripMenuItem
            // 
            this.cASContainerCreatorToolStripMenuItem.Name = "cASContainerCreatorToolStripMenuItem";
            this.cASContainerCreatorToolStripMenuItem.Size = new System.Drawing.Size(184, 22);
            this.cASContainerCreatorToolStripMenuItem.Text = "CAS Container Creator";
            this.cASContainerCreatorToolStripMenuItem.Click += new System.EventHandler(this.cASContainerCreatorToolStripMenuItem_Click);
            // 
            // hexToStringToolToolStripMenuItem
            // 
            this.hexToStringToolToolStripMenuItem.Name = "hexToStringToolToolStripMenuItem";
            this.hexToStringToolToolStripMenuItem.Size = new System.Drawing.Size(184, 22);
            this.hexToStringToolToolStripMenuItem.Text = "Hex To String Tool";
            this.hexToStringToolToolStripMenuItem.Click += new System.EventHandler(this.hexToStringToolToolStripMenuItem_Click);
            // 
            // cATRepairToolStripMenuItem
            // 
            this.cATRepairToolStripMenuItem.Name = "cATRepairToolStripMenuItem";
            this.cATRepairToolStripMenuItem.Size = new System.Drawing.Size(184, 22);
            this.cATRepairToolStripMenuItem.Text = "CAT Repair";
            this.cATRepairToolStripMenuItem.Click += new System.EventHandler(this.cATRepairToolStripMenuItem_Click);
            // 
            // folderCompareToolStripMenuItem
            // 
            this.folderCompareToolStripMenuItem.Name = "folderCompareToolStripMenuItem";
            this.folderCompareToolStripMenuItem.Size = new System.Drawing.Size(184, 22);
            this.folderCompareToolStripMenuItem.Text = "Folder Compare";
            this.folderCompareToolStripMenuItem.Click += new System.EventHandler(this.folderCompareToolStripMenuItem_Click);
            // 
            // fileToolsToolStripMenuItem
            // 
            this.fileToolsToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.tOCToolToolStripMenuItem,
            this.sBToolToolStripMenuItem,
            this.mFTToolToolStripMenuItem,
            this.initFsToolToolStripMenuItem,
            this.bundleBuilderToolStripMenuItem,
            this.supportDAIMODToolToolStripMenuItem});
            this.fileToolsToolStripMenuItem.Name = "fileToolsToolStripMenuItem";
            this.fileToolsToolStripMenuItem.Size = new System.Drawing.Size(63, 20);
            this.fileToolsToolStripMenuItem.Text = "File Tools";
            // 
            // tOCToolToolStripMenuItem
            // 
            this.tOCToolToolStripMenuItem.Name = "tOCToolToolStripMenuItem";
            this.tOCToolToolStripMenuItem.Size = new System.Drawing.Size(179, 22);
            this.tOCToolToolStripMenuItem.Text = "TOC Tool";
            this.tOCToolToolStripMenuItem.Click += new System.EventHandler(this.tOCToolToolStripMenuItem_Click);
            // 
            // sBToolToolStripMenuItem
            // 
            this.sBToolToolStripMenuItem.Name = "sBToolToolStripMenuItem";
            this.sBToolToolStripMenuItem.Size = new System.Drawing.Size(179, 22);
            this.sBToolToolStripMenuItem.Text = "SB Tool";
            this.sBToolToolStripMenuItem.Click += new System.EventHandler(this.sBToolToolStripMenuItem_Click);
            // 
            // mFTToolToolStripMenuItem
            // 
            this.mFTToolToolStripMenuItem.Name = "mFTToolToolStripMenuItem";
            this.mFTToolToolStripMenuItem.Size = new System.Drawing.Size(179, 22);
            this.mFTToolToolStripMenuItem.Text = "MFT Tool";
            this.mFTToolToolStripMenuItem.Click += new System.EventHandler(this.mFTToolToolStripMenuItem_Click);
            // 
            // initFsToolToolStripMenuItem
            // 
            this.initFsToolToolStripMenuItem.Name = "initFsToolToolStripMenuItem";
            this.initFsToolToolStripMenuItem.Size = new System.Drawing.Size(179, 22);
            this.initFsToolToolStripMenuItem.Text = "InitFs Tool";
            this.initFsToolToolStripMenuItem.Click += new System.EventHandler(this.initFsToolToolStripMenuItem_Click);
            // 
            // bundleBuilderToolStripMenuItem
            // 
            this.bundleBuilderToolStripMenuItem.Name = "bundleBuilderToolStripMenuItem";
            this.bundleBuilderToolStripMenuItem.Size = new System.Drawing.Size(179, 22);
            this.bundleBuilderToolStripMenuItem.Text = "Bundle Builder";
            this.bundleBuilderToolStripMenuItem.Click += new System.EventHandler(this.bundleBuilderToolStripMenuItem_Click);
            // 
            // contentToolsToolStripMenuItem
            // 
            this.contentToolsToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.eBXToolToolStripMenuItem,
            this.textureToolToolStripMenuItem,
            this.talktableToolToolStripMenuItem,
            this.meshToolToolStripMenuItem});
            this.contentToolsToolStripMenuItem.Name = "contentToolsToolStripMenuItem";
            this.contentToolsToolStripMenuItem.Size = new System.Drawing.Size(86, 20);
            this.contentToolsToolStripMenuItem.Text = "Content Tools";
            // 
            // eBXToolToolStripMenuItem
            // 
            this.eBXToolToolStripMenuItem.Name = "eBXToolToolStripMenuItem";
            this.eBXToolToolStripMenuItem.Size = new System.Drawing.Size(140, 22);
            this.eBXToolToolStripMenuItem.Text = "EBX Tool";
            this.eBXToolToolStripMenuItem.Click += new System.EventHandler(this.eBXToolToolStripMenuItem_Click);
            // 
            // textureToolToolStripMenuItem
            // 
            this.textureToolToolStripMenuItem.Name = "textureToolToolStripMenuItem";
            this.textureToolToolStripMenuItem.Size = new System.Drawing.Size(140, 22);
            this.textureToolToolStripMenuItem.Text = "Texture Tool";
            this.textureToolToolStripMenuItem.Click += new System.EventHandler(this.textureToolToolStripMenuItem_Click);
            // 
            // talktableToolToolStripMenuItem
            // 
            this.talktableToolToolStripMenuItem.Name = "talktableToolToolStripMenuItem";
            this.talktableToolToolStripMenuItem.Size = new System.Drawing.Size(140, 22);
            this.talktableToolToolStripMenuItem.Text = "Talktable Tool";
            this.talktableToolToolStripMenuItem.Click += new System.EventHandler(this.talktableToolToolStripMenuItem_Click);
            // 
            // meshToolToolStripMenuItem
            // 
            this.meshToolToolStripMenuItem.Name = "meshToolToolStripMenuItem";
            this.meshToolToolStripMenuItem.Size = new System.Drawing.Size(140, 22);
            this.meshToolToolStripMenuItem.Text = "Mesh Tool";
            this.meshToolToolStripMenuItem.Click += new System.EventHandler(this.meshToolToolStripMenuItem_Click);
            // 
            // browserToolStripMenuItem
            // 
            this.browserToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.fileBrowserToolStripMenuItem,
            this.bundleBrowserToolStripMenuItem,
            this.eBXBrowserToolStripMenuItem,
            this.rESBrowserToolStripMenuItem,
            this.textureBrowserToolStripMenuItem,
            this.meshBrowserToolStripMenuItem});
            this.browserToolStripMenuItem.Name = "browserToolStripMenuItem";
            this.browserToolStripMenuItem.Size = new System.Drawing.Size(58, 20);
            this.browserToolStripMenuItem.Text = "Browser";
            // 
            // fileBrowserToolStripMenuItem
            // 
            this.fileBrowserToolStripMenuItem.Name = "fileBrowserToolStripMenuItem";
            this.fileBrowserToolStripMenuItem.Size = new System.Drawing.Size(154, 22);
            this.fileBrowserToolStripMenuItem.Text = "File Browser";
            this.fileBrowserToolStripMenuItem.Click += new System.EventHandler(this.fileBrowserToolStripMenuItem_Click);
            // 
            // bundleBrowserToolStripMenuItem
            // 
            this.bundleBrowserToolStripMenuItem.Name = "bundleBrowserToolStripMenuItem";
            this.bundleBrowserToolStripMenuItem.Size = new System.Drawing.Size(154, 22);
            this.bundleBrowserToolStripMenuItem.Text = "Bundle Browser";
            this.bundleBrowserToolStripMenuItem.Click += new System.EventHandler(this.bundleBrowserToolStripMenuItem_Click);
            // 
            // eBXBrowserToolStripMenuItem
            // 
            this.eBXBrowserToolStripMenuItem.Name = "eBXBrowserToolStripMenuItem";
            this.eBXBrowserToolStripMenuItem.Size = new System.Drawing.Size(154, 22);
            this.eBXBrowserToolStripMenuItem.Text = "EBX Browser";
            this.eBXBrowserToolStripMenuItem.Click += new System.EventHandler(this.eBXBrowserToolStripMenuItem_Click);
            // 
            // rESBrowserToolStripMenuItem
            // 
            this.rESBrowserToolStripMenuItem.Name = "rESBrowserToolStripMenuItem";
            this.rESBrowserToolStripMenuItem.Size = new System.Drawing.Size(154, 22);
            this.rESBrowserToolStripMenuItem.Text = "RES Browser";
            this.rESBrowserToolStripMenuItem.Click += new System.EventHandler(this.rESBrowserToolStripMenuItem_Click);
            // 
            // textureBrowserToolStripMenuItem
            // 
            this.textureBrowserToolStripMenuItem.Name = "textureBrowserToolStripMenuItem";
            this.textureBrowserToolStripMenuItem.Size = new System.Drawing.Size(154, 22);
            this.textureBrowserToolStripMenuItem.Text = "Texture Browser";
            this.textureBrowserToolStripMenuItem.Click += new System.EventHandler(this.textureBrowserToolStripMenuItem_Click);
            // 
            // meshBrowserToolStripMenuItem
            // 
            this.meshBrowserToolStripMenuItem.Name = "meshBrowserToolStripMenuItem";
            this.meshBrowserToolStripMenuItem.Size = new System.Drawing.Size(154, 22);
            this.meshBrowserToolStripMenuItem.Text = "Mesh Browser";
            this.meshBrowserToolStripMenuItem.Click += new System.EventHandler(this.meshBrowserToolStripMenuItem_Click);
            // 
            // modToolsToolStripMenuItem
            // 
            this.modToolsToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.modEditorToolStripMenuItem,
            this.modRunnerToolStripMenuItem});
            this.modToolsToolStripMenuItem.Name = "modToolsToolStripMenuItem";
            this.modToolsToolStripMenuItem.Size = new System.Drawing.Size(67, 20);
            this.modToolsToolStripMenuItem.Text = "Mod Tools";
            // 
            // modEditorToolStripMenuItem
            // 
            this.modEditorToolStripMenuItem.Name = "modEditorToolStripMenuItem";
            this.modEditorToolStripMenuItem.Size = new System.Drawing.Size(132, 22);
            this.modEditorToolStripMenuItem.Text = "Mod Editor";
            this.modEditorToolStripMenuItem.Click += new System.EventHandler(this.modEditorToolStripMenuItem_Click);
            // 
            // modRunnerToolStripMenuItem
            // 
            this.modRunnerToolStripMenuItem.Name = "modRunnerToolStripMenuItem";
            this.modRunnerToolStripMenuItem.Size = new System.Drawing.Size(132, 22);
            this.modRunnerToolStripMenuItem.Text = "Mod Runner";
            this.modRunnerToolStripMenuItem.Click += new System.EventHandler(this.modRunnerToolStripMenuItem_Click);
            // 
            // rESETALLToolStripMenuItem
            // 
            this.rESETALLToolStripMenuItem.ForeColor = System.Drawing.Color.Red;
            this.rESETALLToolStripMenuItem.Name = "rESETALLToolStripMenuItem";
            this.rESETALLToolStripMenuItem.Size = new System.Drawing.Size(70, 20);
            this.rESETALLToolStripMenuItem.Text = "RESET ALL";
            this.rESETALLToolStripMenuItem.Click += new System.EventHandler(this.rESETALLToolStripMenuItem_Click);
            // 
            // supportDAIMODToolToolStripMenuItem
            // 
            this.supportDAIMODToolToolStripMenuItem.Name = "supportDAIMODToolToolStripMenuItem";
            this.supportDAIMODToolToolStripMenuItem.Size = new System.Drawing.Size(179, 22);
            this.supportDAIMODToolToolStripMenuItem.Text = "Support DAIMOD Tool";
            this.supportDAIMODToolToolStripMenuItem.Click += new System.EventHandler(this.supportDAIMODToolToolStripMenuItem_Click);
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1119, 559);
            this.Controls.Add(this.menuStrip1);
            this.IsMdiContainer = true;
            this.MainMenuStrip = this.menuStrip1;
            this.Name = "MainForm";
            this.Text = "MainForm";
            this.Activated += new System.EventHandler(this.MainForm_Activated);
            this.Load += new System.EventHandler(this.MainForm_Load);
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }
        #endregion

        private System.Windows.Forms.ToolTip toolTip;
        private MenuStrip menuStrip1;
        private ToolStripMenuItem fileToolsToolStripMenuItem;
        private ToolStripMenuItem tOCToolToolStripMenuItem;
        private ToolStripMenuItem sBToolToolStripMenuItem;
        private ToolStripMenuItem mFTToolToolStripMenuItem;
        private ToolStripMenuItem generalToolsToolStripMenuItem;
        private ToolStripMenuItem hexToolToolStripMenuItem;
        private ToolStripMenuItem initFsToolToolStripMenuItem;
        private ToolStripMenuItem contentToolsToolStripMenuItem;
        private ToolStripMenuItem eBXToolToolStripMenuItem;
        private ToolStripMenuItem rESETALLToolStripMenuItem;
        private ToolStripMenuItem cASContainerCreatorToolStripMenuItem;
        private ToolStripMenuItem textureToolToolStripMenuItem;
        private ToolStripMenuItem modToolsToolStripMenuItem;
        private ToolStripMenuItem modEditorToolStripMenuItem;
        private ToolStripMenuItem modRunnerToolStripMenuItem;
        private ToolStripMenuItem bundleBuilderToolStripMenuItem;
        private ToolStripMenuItem browserToolStripMenuItem;
        private ToolStripMenuItem fileBrowserToolStripMenuItem;
        private ToolStripMenuItem bundleBrowserToolStripMenuItem;
        private ToolStripMenuItem eBXBrowserToolStripMenuItem;
        private ToolStripMenuItem textureBrowserToolStripMenuItem;
        private ToolStripMenuItem talktableToolToolStripMenuItem;
        private ToolStripMenuItem sHA1LookupToolStripMenuItem;
        private ToolStripMenuItem hexToStringToolToolStripMenuItem;
        private ToolStripMenuItem cATRepairToolStripMenuItem;
        private ToolStripMenuItem folderCompareToolStripMenuItem;
        private ToolStripMenuItem rESBrowserToolStripMenuItem;
        private ToolStripMenuItem meshBrowserToolStripMenuItem;
        private ToolStripMenuItem meshToolToolStripMenuItem;
        private ToolStripMenuItem supportDAIMODToolToolStripMenuItem;
    }
}



