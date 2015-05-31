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
            this.fileToolsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.tOCToolToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.sBToolToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.mFTToolToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.initFsToolToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.generalToolsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.contentBrowserToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.hexToolToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.contentToolsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.eBXToolToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.menuStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // menuStrip1
            // 
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.fileToolsToolStripMenuItem,
            this.generalToolsToolStripMenuItem,
            this.contentToolsToolStripMenuItem});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Size = new System.Drawing.Size(1119, 24);
            this.menuStrip1.TabIndex = 1;
            this.menuStrip1.Text = "menuStrip1";
            // 
            // fileToolsToolStripMenuItem
            // 
            this.fileToolsToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.tOCToolToolStripMenuItem,
            this.sBToolToolStripMenuItem,
            this.mFTToolToolStripMenuItem,
            this.initFsToolToolStripMenuItem});
            this.fileToolsToolStripMenuItem.Name = "fileToolsToolStripMenuItem";
            this.fileToolsToolStripMenuItem.Size = new System.Drawing.Size(63, 20);
            this.fileToolsToolStripMenuItem.Text = "File Tools";
            // 
            // tOCToolToolStripMenuItem
            // 
            this.tOCToolToolStripMenuItem.Name = "tOCToolToolStripMenuItem";
            this.tOCToolToolStripMenuItem.Size = new System.Drawing.Size(124, 22);
            this.tOCToolToolStripMenuItem.Text = "TOC Tool";
            this.tOCToolToolStripMenuItem.Click += new System.EventHandler(this.tOCToolToolStripMenuItem_Click);
            // 
            // sBToolToolStripMenuItem
            // 
            this.sBToolToolStripMenuItem.Name = "sBToolToolStripMenuItem";
            this.sBToolToolStripMenuItem.Size = new System.Drawing.Size(124, 22);
            this.sBToolToolStripMenuItem.Text = "SB Tool";
            this.sBToolToolStripMenuItem.Click += new System.EventHandler(this.sBToolToolStripMenuItem_Click);
            // 
            // mFTToolToolStripMenuItem
            // 
            this.mFTToolToolStripMenuItem.Name = "mFTToolToolStripMenuItem";
            this.mFTToolToolStripMenuItem.Size = new System.Drawing.Size(124, 22);
            this.mFTToolToolStripMenuItem.Text = "MFT Tool";
            this.mFTToolToolStripMenuItem.Click += new System.EventHandler(this.mFTToolToolStripMenuItem_Click);
            // 
            // initFsToolToolStripMenuItem
            // 
            this.initFsToolToolStripMenuItem.Name = "initFsToolToolStripMenuItem";
            this.initFsToolToolStripMenuItem.Size = new System.Drawing.Size(124, 22);
            this.initFsToolToolStripMenuItem.Text = "InitFs Tool";
            this.initFsToolToolStripMenuItem.Click += new System.EventHandler(this.initFsToolToolStripMenuItem_Click);
            // 
            // generalToolsToolStripMenuItem
            // 
            this.generalToolsToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.contentBrowserToolStripMenuItem,
            this.hexToolToolStripMenuItem});
            this.generalToolsToolStripMenuItem.Name = "generalToolsToolStripMenuItem";
            this.generalToolsToolStripMenuItem.Size = new System.Drawing.Size(84, 20);
            this.generalToolsToolStripMenuItem.Text = "General Tools";
            // 
            // contentBrowserToolStripMenuItem
            // 
            this.contentBrowserToolStripMenuItem.Name = "contentBrowserToolStripMenuItem";
            this.contentBrowserToolStripMenuItem.Size = new System.Drawing.Size(155, 22);
            this.contentBrowserToolStripMenuItem.Text = "Content Browser";
            this.contentBrowserToolStripMenuItem.Click += new System.EventHandler(this.contentBrowserToolStripMenuItem_Click_1);
            // 
            // hexToolToolStripMenuItem
            // 
            this.hexToolToolStripMenuItem.Name = "hexToolToolStripMenuItem";
            this.hexToolToolStripMenuItem.Size = new System.Drawing.Size(155, 22);
            this.hexToolToolStripMenuItem.Text = "Hex Tool";
            this.hexToolToolStripMenuItem.Click += new System.EventHandler(this.hexToolToolStripMenuItem_Click_1);
            // 
            // contentToolsToolStripMenuItem
            // 
            this.contentToolsToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.eBXToolToolStripMenuItem});
            this.contentToolsToolStripMenuItem.Name = "contentToolsToolStripMenuItem";
            this.contentToolsToolStripMenuItem.Size = new System.Drawing.Size(86, 20);
            this.contentToolsToolStripMenuItem.Text = "Content Tools";
            // 
            // eBXToolToolStripMenuItem
            // 
            this.eBXToolToolStripMenuItem.Name = "eBXToolToolStripMenuItem";
            this.eBXToolToolStripMenuItem.Size = new System.Drawing.Size(152, 22);
            this.eBXToolToolStripMenuItem.Text = "EBX Tool";
            this.eBXToolToolStripMenuItem.Click += new System.EventHandler(this.eBXToolToolStripMenuItem_Click);
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
        private ToolStripMenuItem contentBrowserToolStripMenuItem;
        private ToolStripMenuItem hexToolToolStripMenuItem;
        private ToolStripMenuItem initFsToolToolStripMenuItem;
        private ToolStripMenuItem contentToolsToolStripMenuItem;
        private ToolStripMenuItem eBXToolToolStripMenuItem;
    }
}



