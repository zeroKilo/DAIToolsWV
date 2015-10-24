namespace DAIToolsWV.FileTools
{
    partial class TOCTool
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(TOCTool));
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.fileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.openSingleToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.saveAsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.treeView1 = new System.Windows.Forms.TreeView();
            this.contextMenuStrip1 = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.deleteBundleToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.keepOnlyThisBundleToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.nOPEToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.expandAllSubNodesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStrip1 = new System.Windows.Forms.ToolStrip();
            this.toolStripTextBox1 = new System.Windows.Forms.ToolStripTextBox();
            this.toolStripButton1 = new System.Windows.Forms.ToolStripButton();
            this.menuStrip1.SuspendLayout();
            this.contextMenuStrip1.SuspendLayout();
            this.toolStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // menuStrip1
            // 
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.fileToolStripMenuItem});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Size = new System.Drawing.Size(583, 24);
            this.menuStrip1.TabIndex = 0;
            this.menuStrip1.Text = "menuStrip1";
            this.menuStrip1.Visible = false;
            // 
            // fileToolStripMenuItem
            // 
            this.fileToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.openSingleToolStripMenuItem,
            this.saveAsToolStripMenuItem});
            this.fileToolStripMenuItem.Name = "fileToolStripMenuItem";
            this.fileToolStripMenuItem.Size = new System.Drawing.Size(35, 20);
            this.fileToolStripMenuItem.Text = "File";
            // 
            // openSingleToolStripMenuItem
            // 
            this.openSingleToolStripMenuItem.Name = "openSingleToolStripMenuItem";
            this.openSingleToolStripMenuItem.Size = new System.Drawing.Size(152, 22);
            this.openSingleToolStripMenuItem.Text = "Open";
            this.openSingleToolStripMenuItem.Click += new System.EventHandler(this.openSingleToolStripMenuItem_Click);
            // 
            // saveAsToolStripMenuItem
            // 
            this.saveAsToolStripMenuItem.Name = "saveAsToolStripMenuItem";
            this.saveAsToolStripMenuItem.Size = new System.Drawing.Size(152, 22);
            this.saveAsToolStripMenuItem.Text = "Save as...";
            this.saveAsToolStripMenuItem.Click += new System.EventHandler(this.saveAsToolStripMenuItem_Click);
            // 
            // treeView1
            // 
            this.treeView1.ContextMenuStrip = this.contextMenuStrip1;
            this.treeView1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.treeView1.Font = new System.Drawing.Font("Courier New", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.treeView1.HideSelection = false;
            this.treeView1.Location = new System.Drawing.Point(0, 49);
            this.treeView1.Name = "treeView1";
            this.treeView1.Size = new System.Drawing.Size(583, 439);
            this.treeView1.TabIndex = 2;
            this.treeView1.DoubleClick += new System.EventHandler(this.treeView1_DoubleClick);
            // 
            // contextMenuStrip1
            // 
            this.contextMenuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.deleteBundleToolStripMenuItem,
            this.keepOnlyThisBundleToolStripMenuItem,
            this.nOPEToolStripMenuItem,
            this.expandAllSubNodesToolStripMenuItem});
            this.contextMenuStrip1.Name = "contextMenuStrip1";
            this.contextMenuStrip1.Size = new System.Drawing.Size(177, 92);
            this.contextMenuStrip1.Paint += new System.Windows.Forms.PaintEventHandler(this.contextMenuStrip1_Paint);
            // 
            // deleteBundleToolStripMenuItem
            // 
            this.deleteBundleToolStripMenuItem.Name = "deleteBundleToolStripMenuItem";
            this.deleteBundleToolStripMenuItem.Size = new System.Drawing.Size(176, 22);
            this.deleteBundleToolStripMenuItem.Text = "Delete bundle";
            this.deleteBundleToolStripMenuItem.Visible = false;
            this.deleteBundleToolStripMenuItem.Click += new System.EventHandler(this.deleteBundleToolStripMenuItem_Click);
            // 
            // keepOnlyThisBundleToolStripMenuItem
            // 
            this.keepOnlyThisBundleToolStripMenuItem.Name = "keepOnlyThisBundleToolStripMenuItem";
            this.keepOnlyThisBundleToolStripMenuItem.Size = new System.Drawing.Size(176, 22);
            this.keepOnlyThisBundleToolStripMenuItem.Text = "Keep only this bundle";
            this.keepOnlyThisBundleToolStripMenuItem.Visible = false;
            this.keepOnlyThisBundleToolStripMenuItem.Click += new System.EventHandler(this.keepOnlyThisBundleToolStripMenuItem_Click);
            // 
            // nOPEToolStripMenuItem
            // 
            this.nOPEToolStripMenuItem.Name = "nOPEToolStripMenuItem";
            this.nOPEToolStripMenuItem.Size = new System.Drawing.Size(176, 22);
            this.nOPEToolStripMenuItem.Text = "N.O.P.E.";
            // 
            // expandAllSubNodesToolStripMenuItem
            // 
            this.expandAllSubNodesToolStripMenuItem.Name = "expandAllSubNodesToolStripMenuItem";
            this.expandAllSubNodesToolStripMenuItem.Size = new System.Drawing.Size(176, 22);
            this.expandAllSubNodesToolStripMenuItem.Text = "Expand all sub nodes";
            this.expandAllSubNodesToolStripMenuItem.Click += new System.EventHandler(this.expandAllSubNodesToolStripMenuItem_Click);
            // 
            // toolStrip1
            // 
            this.toolStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripTextBox1,
            this.toolStripButton1});
            this.toolStrip1.Location = new System.Drawing.Point(0, 24);
            this.toolStrip1.Name = "toolStrip1";
            this.toolStrip1.Size = new System.Drawing.Size(583, 25);
            this.toolStrip1.TabIndex = 3;
            this.toolStrip1.Text = "toolStrip1";
            // 
            // toolStripTextBox1
            // 
            this.toolStripTextBox1.Name = "toolStripTextBox1";
            this.toolStripTextBox1.Size = new System.Drawing.Size(300, 25);
            this.toolStripTextBox1.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.toolStripTextBox1_KeyPress);
            // 
            // toolStripButton1
            // 
            this.toolStripButton1.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.toolStripButton1.Image = ((System.Drawing.Image)(resources.GetObject("toolStripButton1.Image")));
            this.toolStripButton1.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButton1.Name = "toolStripButton1";
            this.toolStripButton1.Size = new System.Drawing.Size(57, 22);
            this.toolStripButton1.Text = "Find Next";
            this.toolStripButton1.Click += new System.EventHandler(this.toolStripButton1_Click);
            // 
            // TOCTool
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(583, 488);
            this.Controls.Add(this.treeView1);
            this.Controls.Add(this.toolStrip1);
            this.Controls.Add(this.menuStrip1);
            this.MainMenuStrip = this.menuStrip1;
            this.Name = "TOCTool";
            this.Text = "TOC Tool";
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            this.contextMenuStrip1.ResumeLayout(false);
            this.toolStrip1.ResumeLayout(false);
            this.toolStrip1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.ToolStripMenuItem fileToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem openSingleToolStripMenuItem;
        private System.Windows.Forms.TreeView treeView1;
        private System.Windows.Forms.ToolStripMenuItem saveAsToolStripMenuItem;
        private System.Windows.Forms.ToolStrip toolStrip1;
        private System.Windows.Forms.ToolStripTextBox toolStripTextBox1;
        private System.Windows.Forms.ToolStripButton toolStripButton1;
        private System.Windows.Forms.ContextMenuStrip contextMenuStrip1;
        private System.Windows.Forms.ToolStripMenuItem deleteBundleToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem keepOnlyThisBundleToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem nOPEToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem expandAllSubNodesToolStripMenuItem;
    }
}