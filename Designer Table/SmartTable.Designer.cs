namespace temp2
{
    partial class SmartTable
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

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.contextMenuStrip1 = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.copyToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.pasteToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.editToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.rtlClkChangeString = new System.Windows.Forms.ToolStripTextBox();
            this.rtlClkChangeImage = new System.Windows.Forms.ToolStripMenuItem();
            this.rtlClkChangeBool = new System.Windows.Forms.ToolStripMenuItem();
            this.progressToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.colorToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.rtlTbProgressValue = new System.Windows.Forms.ToolStripTextBox();
            this.disableGlassEffectToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
            this.foreColorToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.backColorToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator3 = new System.Windows.Forms.ToolStripSeparator();
            this.addRowToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.addColumnToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.newColumnTextBox1 = new System.Windows.Forms.ToolStripTextBox();
            this.ImageChooserDialog = new System.Windows.Forms.OpenFileDialog();
            this.colorDialog1 = new System.Windows.Forms.ColorDialog();
            this.deleteRowToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.contextMenuStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // contextMenuStrip1
            // 
            this.contextMenuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.copyToolStripMenuItem,
            this.pasteToolStripMenuItem,
            this.toolStripSeparator1,
            this.editToolStripMenuItem,
            this.progressToolStripMenuItem,
            this.toolStripSeparator2,
            this.foreColorToolStripMenuItem,
            this.backColorToolStripMenuItem,
            this.toolStripSeparator3,
            this.addRowToolStripMenuItem,
            this.addColumnToolStripMenuItem,
            this.deleteRowToolStripMenuItem});
            this.contextMenuStrip1.Name = "contextMenuStrip1";
            this.contextMenuStrip1.Size = new System.Drawing.Size(153, 242);
            this.contextMenuStrip1.Opening += new System.ComponentModel.CancelEventHandler(this.contextMenuStrip1_Opening);
            this.contextMenuStrip1.Opened += new System.EventHandler(this.contextMenuStrip1_Opened);
            // 
            // copyToolStripMenuItem
            // 
            this.copyToolStripMenuItem.Name = "copyToolStripMenuItem";
            this.copyToolStripMenuItem.Size = new System.Drawing.Size(152, 22);
            this.copyToolStripMenuItem.Text = "Copy";
            this.copyToolStripMenuItem.Click += new System.EventHandler(this.copyToolStripMenuItem_Click);
            // 
            // pasteToolStripMenuItem
            // 
            this.pasteToolStripMenuItem.Name = "pasteToolStripMenuItem";
            this.pasteToolStripMenuItem.Size = new System.Drawing.Size(152, 22);
            this.pasteToolStripMenuItem.Text = "Paste";
            this.pasteToolStripMenuItem.Click += new System.EventHandler(this.pasteToolStripMenuItem_Click);
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(149, 6);
            // 
            // editToolStripMenuItem
            // 
            this.editToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.rtlClkChangeString,
            this.rtlClkChangeImage,
            this.rtlClkChangeBool});
            this.editToolStripMenuItem.Name = "editToolStripMenuItem";
            this.editToolStripMenuItem.Size = new System.Drawing.Size(152, 22);
            this.editToolStripMenuItem.Text = "Change";
            // 
            // rtlClkChangeString
            // 
            this.rtlClkChangeString.Name = "rtlClkChangeString";
            this.rtlClkChangeString.Size = new System.Drawing.Size(100, 23);
            this.rtlClkChangeString.KeyUp += new System.Windows.Forms.KeyEventHandler(this.rtlClkChangeString_KeyUp);
            // 
            // rtlClkChangeImage
            // 
            this.rtlClkChangeImage.Name = "rtlClkChangeImage";
            this.rtlClkChangeImage.Size = new System.Drawing.Size(160, 22);
            this.rtlClkChangeImage.Text = "Choose image...";
            this.rtlClkChangeImage.Click += new System.EventHandler(this.rtClkChangeImage_Click);
            // 
            // rtlClkChangeBool
            // 
            this.rtlClkChangeBool.Name = "rtlClkChangeBool";
            this.rtlClkChangeBool.Size = new System.Drawing.Size(160, 22);
            this.rtlClkChangeBool.Text = "Check";
            this.rtlClkChangeBool.Click += new System.EventHandler(this.rtClkChangeBool_Click);
            // 
            // progressToolStripMenuItem
            // 
            this.progressToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.colorToolStripMenuItem,
            this.rtlTbProgressValue,
            this.disableGlassEffectToolStripMenuItem});
            this.progressToolStripMenuItem.Name = "progressToolStripMenuItem";
            this.progressToolStripMenuItem.Size = new System.Drawing.Size(152, 22);
            this.progressToolStripMenuItem.Text = "Progress";
            // 
            // colorToolStripMenuItem
            // 
            this.colorToolStripMenuItem.Name = "colorToolStripMenuItem";
            this.colorToolStripMenuItem.Size = new System.Drawing.Size(174, 22);
            this.colorToolStripMenuItem.Text = "Color";
            this.colorToolStripMenuItem.Click += new System.EventHandler(this.colorToolStripMenuItem_Click);
            // 
            // rtlTbProgressValue
            // 
            this.rtlTbProgressValue.Name = "rtlTbProgressValue";
            this.rtlTbProgressValue.Size = new System.Drawing.Size(100, 23);
            this.rtlTbProgressValue.KeyDown += new System.Windows.Forms.KeyEventHandler(this.rtlTbProgressValue_KeyDown);
            // 
            // disableGlassEffectToolStripMenuItem
            // 
            this.disableGlassEffectToolStripMenuItem.Name = "disableGlassEffectToolStripMenuItem";
            this.disableGlassEffectToolStripMenuItem.Size = new System.Drawing.Size(174, 22);
            this.disableGlassEffectToolStripMenuItem.Text = "Disable glass effect";
            this.disableGlassEffectToolStripMenuItem.Click += new System.EventHandler(this.disableGlassEffectToolStripMenuItem_Click);
            // 
            // toolStripSeparator2
            // 
            this.toolStripSeparator2.Name = "toolStripSeparator2";
            this.toolStripSeparator2.Size = new System.Drawing.Size(149, 6);
            // 
            // foreColorToolStripMenuItem
            // 
            this.foreColorToolStripMenuItem.Name = "foreColorToolStripMenuItem";
            this.foreColorToolStripMenuItem.Size = new System.Drawing.Size(152, 22);
            this.foreColorToolStripMenuItem.Text = "Fore color";
            this.foreColorToolStripMenuItem.Click += new System.EventHandler(this.foreColorToolStripMenuItem_Click);
            // 
            // backColorToolStripMenuItem
            // 
            this.backColorToolStripMenuItem.Name = "backColorToolStripMenuItem";
            this.backColorToolStripMenuItem.Size = new System.Drawing.Size(152, 22);
            this.backColorToolStripMenuItem.Text = "Back color";
            this.backColorToolStripMenuItem.Click += new System.EventHandler(this.backColorToolStripMenuItem_Click);
            // 
            // toolStripSeparator3
            // 
            this.toolStripSeparator3.Name = "toolStripSeparator3";
            this.toolStripSeparator3.Size = new System.Drawing.Size(149, 6);
            // 
            // addRowToolStripMenuItem
            // 
            this.addRowToolStripMenuItem.Name = "addRowToolStripMenuItem";
            this.addRowToolStripMenuItem.Size = new System.Drawing.Size(152, 22);
            this.addRowToolStripMenuItem.Text = "Add row";
            this.addRowToolStripMenuItem.Click += new System.EventHandler(this.addRowToolStripMenuItem_Click);
            // 
            // addColumnToolStripMenuItem
            // 
            this.addColumnToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.newColumnTextBox1});
            this.addColumnToolStripMenuItem.Name = "addColumnToolStripMenuItem";
            this.addColumnToolStripMenuItem.Size = new System.Drawing.Size(152, 22);
            this.addColumnToolStripMenuItem.Text = "Add column";
            // 
            // newColumnTextBox1
            // 
            this.newColumnTextBox1.Name = "newColumnTextBox1";
            this.newColumnTextBox1.Size = new System.Drawing.Size(100, 23);
            this.newColumnTextBox1.KeyDown += new System.Windows.Forms.KeyEventHandler(this.newColumnTextBox1_KeyDown);
            // 
            // ImageChooserDialog
            // 
            this.ImageChooserDialog.Filter = "Image Files|*.jpg;*.jpeg;*.png;*.gif;";
            // 
            // deleteRowToolStripMenuItem
            // 
            this.deleteRowToolStripMenuItem.Name = "deleteRowToolStripMenuItem";
            this.deleteRowToolStripMenuItem.Size = new System.Drawing.Size(152, 22);
            this.deleteRowToolStripMenuItem.Text = "Delete row(s)";
            this.deleteRowToolStripMenuItem.Click += new System.EventHandler(this.deleteRowToolStripMenuItem_Click);
            // 
            // SmartTable
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoScroll = true;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(251)))), ((int)(((byte)(251)))), ((int)(((byte)(251)))));
            this.ContextMenuStrip = this.contextMenuStrip1;
            this.Name = "SmartTable";
            this.Size = new System.Drawing.Size(310, 226);
            this.contextMenuStrip1.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.ContextMenuStrip contextMenuStrip1;
        private System.Windows.Forms.ToolStripMenuItem editToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem copyToolStripMenuItem;
        public System.Windows.Forms.OpenFileDialog ImageChooserDialog;
        private System.Windows.Forms.ToolStripTextBox rtlClkChangeString;
        private System.Windows.Forms.ToolStripMenuItem rtlClkChangeImage;
        private System.Windows.Forms.ToolStripMenuItem rtlClkChangeBool;
        private System.Windows.Forms.ToolStripMenuItem progressToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem colorToolStripMenuItem;
        private System.Windows.Forms.ToolStripTextBox rtlTbProgressValue;
        private System.Windows.Forms.ColorDialog colorDialog1;
        private System.Windows.Forms.ToolStripMenuItem disableGlassEffectToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem pasteToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator2;
        private System.Windows.Forms.ToolStripMenuItem foreColorToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem backColorToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator3;
        private System.Windows.Forms.ToolStripMenuItem addRowToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem addColumnToolStripMenuItem;
        private System.Windows.Forms.ToolStripTextBox newColumnTextBox1;
        private System.Windows.Forms.ToolStripMenuItem deleteRowToolStripMenuItem;
    }
}
