namespace DAIToolsWV.Browser
{
    partial class TextureBrowser
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(TextureBrowser));
            this.splitContainer4 = new System.Windows.Forms.SplitContainer();
            this.treeView5 = new System.Windows.Forms.TreeView();
            this.splitContainer5 = new System.Windows.Forms.SplitContainer();
            this.listBox2 = new System.Windows.Forms.ListBox();
            this.splitContainer6 = new System.Windows.Forms.SplitContainer();
            this.hb2 = new Be.Windows.Forms.HexBox();
            this.pb1 = new System.Windows.Forms.PictureBox();
            this.hb3 = new Be.Windows.Forms.HexBox();
            this.statusStrip3 = new System.Windows.Forms.StatusStrip();
            this.statustextures = new System.Windows.Forms.ToolStripStatusLabel();
            this.toolStrip4 = new System.Windows.Forms.ToolStrip();
            this.toolStripTextBox1 = new System.Windows.Forms.ToolStripTextBox();
            this.toolStripButton19 = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator3 = new System.Windows.Forms.ToolStripSeparator();
            this.toolStripButton18 = new System.Windows.Forms.ToolStripButton();
            this.toolStripButton17 = new System.Windows.Forms.ToolStripButton();
            this.toolStripButton16 = new System.Windows.Forms.ToolStripButton();
            this.texcontext = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.openInTextureToolToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.nOPEToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer4)).BeginInit();
            this.splitContainer4.Panel1.SuspendLayout();
            this.splitContainer4.Panel2.SuspendLayout();
            this.splitContainer4.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer5)).BeginInit();
            this.splitContainer5.Panel1.SuspendLayout();
            this.splitContainer5.Panel2.SuspendLayout();
            this.splitContainer5.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer6)).BeginInit();
            this.splitContainer6.Panel1.SuspendLayout();
            this.splitContainer6.Panel2.SuspendLayout();
            this.splitContainer6.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pb1)).BeginInit();
            this.statusStrip3.SuspendLayout();
            this.toolStrip4.SuspendLayout();
            this.texcontext.SuspendLayout();
            this.SuspendLayout();
            // 
            // splitContainer4
            // 
            this.splitContainer4.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer4.Location = new System.Drawing.Point(0, 25);
            this.splitContainer4.Name = "splitContainer4";
            // 
            // splitContainer4.Panel1
            // 
            this.splitContainer4.Panel1.Controls.Add(this.treeView5);
            // 
            // splitContainer4.Panel2
            // 
            this.splitContainer4.Panel2.Controls.Add(this.splitContainer5);
            this.splitContainer4.Size = new System.Drawing.Size(756, 439);
            this.splitContainer4.SplitterDistance = 369;
            this.splitContainer4.TabIndex = 5;
            // 
            // treeView5
            // 
            this.treeView5.ContextMenuStrip = this.texcontext;
            this.treeView5.Dock = System.Windows.Forms.DockStyle.Fill;
            this.treeView5.Font = new System.Drawing.Font("Courier New", 10F);
            this.treeView5.HideSelection = false;
            this.treeView5.Location = new System.Drawing.Point(0, 0);
            this.treeView5.Name = "treeView5";
            this.treeView5.Size = new System.Drawing.Size(369, 439);
            this.treeView5.TabIndex = 1;
            this.treeView5.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.treeView5_AfterSelect);
            // 
            // splitContainer5
            // 
            this.splitContainer5.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer5.Location = new System.Drawing.Point(0, 0);
            this.splitContainer5.Name = "splitContainer5";
            this.splitContainer5.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainer5.Panel1
            // 
            this.splitContainer5.Panel1.Controls.Add(this.listBox2);
            // 
            // splitContainer5.Panel2
            // 
            this.splitContainer5.Panel2.Controls.Add(this.splitContainer6);
            this.splitContainer5.Size = new System.Drawing.Size(383, 439);
            this.splitContainer5.SplitterDistance = 129;
            this.splitContainer5.TabIndex = 0;
            // 
            // listBox2
            // 
            this.listBox2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.listBox2.Font = new System.Drawing.Font("Courier New", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.listBox2.FormattingEnabled = true;
            this.listBox2.HorizontalScrollbar = true;
            this.listBox2.IntegralHeight = false;
            this.listBox2.ItemHeight = 16;
            this.listBox2.Location = new System.Drawing.Point(0, 0);
            this.listBox2.Name = "listBox2";
            this.listBox2.ScrollAlwaysVisible = true;
            this.listBox2.Size = new System.Drawing.Size(383, 129);
            this.listBox2.TabIndex = 2;
            this.listBox2.SelectedIndexChanged += new System.EventHandler(this.listBox2_SelectedIndexChanged);
            // 
            // splitContainer6
            // 
            this.splitContainer6.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer6.Location = new System.Drawing.Point(0, 0);
            this.splitContainer6.Name = "splitContainer6";
            this.splitContainer6.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainer6.Panel1
            // 
            this.splitContainer6.Panel1.Controls.Add(this.hb2);
            // 
            // splitContainer6.Panel2
            // 
            this.splitContainer6.Panel2.Controls.Add(this.pb1);
            this.splitContainer6.Panel2.Controls.Add(this.hb3);
            this.splitContainer6.Size = new System.Drawing.Size(383, 306);
            this.splitContainer6.SplitterDistance = 125;
            this.splitContainer6.TabIndex = 0;
            // 
            // hb2
            // 
            this.hb2.BoldFont = null;
            this.hb2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.hb2.Font = new System.Drawing.Font("Courier New", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.hb2.LineInfoForeColor = System.Drawing.Color.Empty;
            this.hb2.LineInfoVisible = true;
            this.hb2.Location = new System.Drawing.Point(0, 0);
            this.hb2.Name = "hb2";
            this.hb2.ShadowSelectionColor = System.Drawing.Color.FromArgb(((int)(((byte)(100)))), ((int)(((byte)(60)))), ((int)(((byte)(188)))), ((int)(((byte)(255)))));
            this.hb2.Size = new System.Drawing.Size(383, 125);
            this.hb2.StringViewVisible = true;
            this.hb2.TabIndex = 2;
            this.hb2.UseFixedBytesPerLine = true;
            this.hb2.VScrollBarVisible = true;
            // 
            // pb1
            // 
            this.pb1.BackColor = System.Drawing.Color.White;
            this.pb1.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.pb1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pb1.Location = new System.Drawing.Point(0, 0);
            this.pb1.Name = "pb1";
            this.pb1.Size = new System.Drawing.Size(383, 177);
            this.pb1.TabIndex = 3;
            this.pb1.TabStop = false;
            // 
            // hb3
            // 
            this.hb3.BoldFont = null;
            this.hb3.Dock = System.Windows.Forms.DockStyle.Fill;
            this.hb3.Font = new System.Drawing.Font("Courier New", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.hb3.LineInfoForeColor = System.Drawing.Color.Empty;
            this.hb3.LineInfoVisible = true;
            this.hb3.Location = new System.Drawing.Point(0, 0);
            this.hb3.Name = "hb3";
            this.hb3.ShadowSelectionColor = System.Drawing.Color.FromArgb(((int)(((byte)(100)))), ((int)(((byte)(60)))), ((int)(((byte)(188)))), ((int)(((byte)(255)))));
            this.hb3.Size = new System.Drawing.Size(383, 177);
            this.hb3.StringViewVisible = true;
            this.hb3.TabIndex = 2;
            this.hb3.UseFixedBytesPerLine = true;
            this.hb3.VScrollBarVisible = true;
            // 
            // statusStrip3
            // 
            this.statusStrip3.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.statustextures});
            this.statusStrip3.Location = new System.Drawing.Point(0, 464);
            this.statusStrip3.Name = "statusStrip3";
            this.statusStrip3.Size = new System.Drawing.Size(756, 22);
            this.statusStrip3.TabIndex = 4;
            this.statusStrip3.Text = "statusStrip3";
            // 
            // statustextures
            // 
            this.statustextures.Font = new System.Drawing.Font("Courier New", 10F);
            this.statustextures.Name = "statustextures";
            this.statustextures.Size = new System.Drawing.Size(176, 17);
            this.statustextures.Text = "toolStripStatusLabel1";
            // 
            // toolStrip4
            // 
            this.toolStrip4.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripTextBox1,
            this.toolStripButton19,
            this.toolStripSeparator3,
            this.toolStripButton18,
            this.toolStripButton17,
            this.toolStripButton16});
            this.toolStrip4.Location = new System.Drawing.Point(0, 0);
            this.toolStrip4.Name = "toolStrip4";
            this.toolStrip4.Size = new System.Drawing.Size(756, 25);
            this.toolStrip4.TabIndex = 3;
            this.toolStrip4.Text = "toolStrip4";
            // 
            // toolStripTextBox1
            // 
            this.toolStripTextBox1.Name = "toolStripTextBox1";
            this.toolStripTextBox1.Size = new System.Drawing.Size(300, 25);
            this.toolStripTextBox1.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.toolStripTextBox1_KeyPress);
            // 
            // toolStripButton19
            // 
            this.toolStripButton19.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.toolStripButton19.Image = ((System.Drawing.Image)(resources.GetObject("toolStripButton19.Image")));
            this.toolStripButton19.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButton19.Name = "toolStripButton19";
            this.toolStripButton19.Size = new System.Drawing.Size(57, 22);
            this.toolStripButton19.Text = "Find Next";
            this.toolStripButton19.Click += new System.EventHandler(this.toolStripButton19_Click);
            // 
            // toolStripSeparator3
            // 
            this.toolStripSeparator3.Name = "toolStripSeparator3";
            this.toolStripSeparator3.Size = new System.Drawing.Size(6, 25);
            // 
            // toolStripButton18
            // 
            this.toolStripButton18.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.toolStripButton18.Image = ((System.Drawing.Image)(resources.GetObject("toolStripButton18.Image")));
            this.toolStripButton18.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButton18.Name = "toolStripButton18";
            this.toolStripButton18.Size = new System.Drawing.Size(84, 22);
            this.toolStripButton18.Text = "Export Texture";
            this.toolStripButton18.Click += new System.EventHandler(this.toolStripButton18_Click);
            // 
            // toolStripButton17
            // 
            this.toolStripButton17.CheckOnClick = true;
            this.toolStripButton17.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.toolStripButton17.Image = ((System.Drawing.Image)(resources.GetObject("toolStripButton17.Image")));
            this.toolStripButton17.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButton17.Name = "toolStripButton17";
            this.toolStripButton17.Size = new System.Drawing.Size(75, 22);
            this.toolStripButton17.Text = "Fit to window";
            this.toolStripButton17.Click += new System.EventHandler(this.toolStripButton17_Click);
            // 
            // toolStripButton16
            // 
            this.toolStripButton16.CheckOnClick = true;
            this.toolStripButton16.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.toolStripButton16.Image = ((System.Drawing.Image)(resources.GetObject("toolStripButton16.Image")));
            this.toolStripButton16.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButton16.Name = "toolStripButton16";
            this.toolStripButton16.Size = new System.Drawing.Size(71, 22);
            this.toolStripButton16.Text = "HEX Preview";
            this.toolStripButton16.Click += new System.EventHandler(this.toolStripButton16_Click);
            // 
            // texcontext
            // 
            this.texcontext.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.openInTextureToolToolStripMenuItem,
            this.nOPEToolStripMenuItem});
            this.texcontext.Name = "texcontext";
            this.texcontext.Size = new System.Drawing.Size(176, 70);
            this.texcontext.Paint += new System.Windows.Forms.PaintEventHandler(this.texcontext_Paint);
            // 
            // openInTextureToolToolStripMenuItem
            // 
            this.openInTextureToolToolStripMenuItem.Name = "openInTextureToolToolStripMenuItem";
            this.openInTextureToolToolStripMenuItem.Size = new System.Drawing.Size(175, 22);
            this.openInTextureToolToolStripMenuItem.Text = "Open in Texture Tool";
            this.openInTextureToolToolStripMenuItem.Click += new System.EventHandler(this.openInTextureToolToolStripMenuItem_Click);
            // 
            // nOPEToolStripMenuItem
            // 
            this.nOPEToolStripMenuItem.Name = "nOPEToolStripMenuItem";
            this.nOPEToolStripMenuItem.Size = new System.Drawing.Size(175, 22);
            this.nOPEToolStripMenuItem.Text = "N.O.P.E.";
            // 
            // TextureBrowser
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(756, 486);
            this.Controls.Add(this.splitContainer4);
            this.Controls.Add(this.statusStrip3);
            this.Controls.Add(this.toolStrip4);
            this.Name = "TextureBrowser";
            this.Text = "Texture Browser";
            this.Load += new System.EventHandler(this.TextureBrowser_Load);
            this.splitContainer4.Panel1.ResumeLayout(false);
            this.splitContainer4.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer4)).EndInit();
            this.splitContainer4.ResumeLayout(false);
            this.splitContainer5.Panel1.ResumeLayout(false);
            this.splitContainer5.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer5)).EndInit();
            this.splitContainer5.ResumeLayout(false);
            this.splitContainer6.Panel1.ResumeLayout(false);
            this.splitContainer6.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer6)).EndInit();
            this.splitContainer6.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.pb1)).EndInit();
            this.statusStrip3.ResumeLayout(false);
            this.statusStrip3.PerformLayout();
            this.toolStrip4.ResumeLayout(false);
            this.toolStrip4.PerformLayout();
            this.texcontext.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.SplitContainer splitContainer4;
        private System.Windows.Forms.TreeView treeView5;
        private System.Windows.Forms.SplitContainer splitContainer5;
        private System.Windows.Forms.ListBox listBox2;
        private System.Windows.Forms.SplitContainer splitContainer6;
        private Be.Windows.Forms.HexBox hb2;
        private System.Windows.Forms.PictureBox pb1;
        private Be.Windows.Forms.HexBox hb3;
        private System.Windows.Forms.StatusStrip statusStrip3;
        private System.Windows.Forms.ToolStripStatusLabel statustextures;
        private System.Windows.Forms.ToolStrip toolStrip4;
        private System.Windows.Forms.ToolStripTextBox toolStripTextBox1;
        private System.Windows.Forms.ToolStripButton toolStripButton19;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator3;
        private System.Windows.Forms.ToolStripButton toolStripButton18;
        private System.Windows.Forms.ToolStripButton toolStripButton17;
        private System.Windows.Forms.ToolStripButton toolStripButton16;
        private System.Windows.Forms.ContextMenuStrip texcontext;
        private System.Windows.Forms.ToolStripMenuItem openInTextureToolToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem nOPEToolStripMenuItem;
    }
}