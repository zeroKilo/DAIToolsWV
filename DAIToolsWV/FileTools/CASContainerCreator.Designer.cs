namespace DAIToolsWV.FileTools
{
    partial class CASContainerCreator
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
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.fileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.loadBinaryToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.hb1 = new Be.Windows.Forms.HexBox();
            this.splitContainer2 = new System.Windows.Forms.SplitContainer();
            this.hb2 = new Be.Windows.Forms.HexBox();
            this.hb3 = new Be.Windows.Forms.HexBox();
            this.appendToCASToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.menuStrip1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer2)).BeginInit();
            this.splitContainer2.Panel1.SuspendLayout();
            this.splitContainer2.Panel2.SuspendLayout();
            this.splitContainer2.SuspendLayout();
            this.SuspendLayout();
            // 
            // menuStrip1
            // 
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.fileToolStripMenuItem});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Size = new System.Drawing.Size(666, 24);
            this.menuStrip1.TabIndex = 0;
            this.menuStrip1.Text = "menuStrip1";
            this.menuStrip1.Visible = false;
            // 
            // fileToolStripMenuItem
            // 
            this.fileToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.loadBinaryToolStripMenuItem,
            this.appendToCASToolStripMenuItem});
            this.fileToolStripMenuItem.Name = "fileToolStripMenuItem";
            this.fileToolStripMenuItem.Size = new System.Drawing.Size(35, 20);
            this.fileToolStripMenuItem.Text = "File";
            // 
            // loadBinaryToolStripMenuItem
            // 
            this.loadBinaryToolStripMenuItem.Name = "loadBinaryToolStripMenuItem";
            this.loadBinaryToolStripMenuItem.Size = new System.Drawing.Size(130, 22);
            this.loadBinaryToolStripMenuItem.Text = "Load Binary";
            this.loadBinaryToolStripMenuItem.Click += new System.EventHandler(this.loadBinaryToolStripMenuItem_Click);
            // 
            // splitContainer1
            // 
            this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer1.Location = new System.Drawing.Point(0, 24);
            this.splitContainer1.Name = "splitContainer1";
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.hb1);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.splitContainer2);
            this.splitContainer1.Size = new System.Drawing.Size(666, 401);
            this.splitContainer1.SplitterDistance = 222;
            this.splitContainer1.TabIndex = 1;
            // 
            // hb1
            // 
            this.hb1.BoldFont = null;
            this.hb1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.hb1.Font = new System.Drawing.Font("Courier New", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.hb1.LineInfoForeColor = System.Drawing.Color.Empty;
            this.hb1.LineInfoVisible = true;
            this.hb1.Location = new System.Drawing.Point(0, 0);
            this.hb1.Name = "hb1";
            this.hb1.ShadowSelectionColor = System.Drawing.Color.FromArgb(((int)(((byte)(100)))), ((int)(((byte)(60)))), ((int)(((byte)(188)))), ((int)(((byte)(255)))));
            this.hb1.Size = new System.Drawing.Size(222, 401);
            this.hb1.StringViewVisible = true;
            this.hb1.TabIndex = 4;
            this.hb1.UseFixedBytesPerLine = true;
            this.hb1.VScrollBarVisible = true;
            // 
            // splitContainer2
            // 
            this.splitContainer2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer2.Location = new System.Drawing.Point(0, 0);
            this.splitContainer2.Name = "splitContainer2";
            this.splitContainer2.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainer2.Panel1
            // 
            this.splitContainer2.Panel1.Controls.Add(this.hb2);
            // 
            // splitContainer2.Panel2
            // 
            this.splitContainer2.Panel2.Controls.Add(this.hb3);
            this.splitContainer2.Size = new System.Drawing.Size(440, 401);
            this.splitContainer2.SplitterDistance = 201;
            this.splitContainer2.TabIndex = 2;
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
            this.hb2.Size = new System.Drawing.Size(440, 201);
            this.hb2.StringViewVisible = true;
            this.hb2.TabIndex = 3;
            this.hb2.UseFixedBytesPerLine = true;
            this.hb2.VScrollBarVisible = true;
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
            this.hb3.Size = new System.Drawing.Size(440, 196);
            this.hb3.StringViewVisible = true;
            this.hb3.TabIndex = 3;
            this.hb3.UseFixedBytesPerLine = true;
            this.hb3.VScrollBarVisible = true;
            // 
            // appendToCASToolStripMenuItem
            // 
            this.appendToCASToolStripMenuItem.Name = "appendToCASToolStripMenuItem";
            this.appendToCASToolStripMenuItem.Size = new System.Drawing.Size(152, 22);
            this.appendToCASToolStripMenuItem.Text = "Append to CAS";
            this.appendToCASToolStripMenuItem.Click += new System.EventHandler(this.appendToCASToolStripMenuItem_Click);
            // 
            // CASContainerCreator
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(666, 425);
            this.Controls.Add(this.splitContainer1);
            this.Controls.Add(this.menuStrip1);
            this.MainMenuStrip = this.menuStrip1;
            this.Name = "CASContainerCreator";
            this.Text = "CAS Container Creator";
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
            this.splitContainer2.Panel1.ResumeLayout(false);
            this.splitContainer2.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer2)).EndInit();
            this.splitContainer2.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.ToolStripMenuItem fileToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem loadBinaryToolStripMenuItem;
        private System.Windows.Forms.SplitContainer splitContainer1;
        private Be.Windows.Forms.HexBox hb1;
        private System.Windows.Forms.SplitContainer splitContainer2;
        private Be.Windows.Forms.HexBox hb2;
        private Be.Windows.Forms.HexBox hb3;
        private System.Windows.Forms.ToolStripMenuItem appendToCASToolStripMenuItem;
    }
}