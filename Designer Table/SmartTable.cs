using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Runtime.InteropServices;

namespace temp2
{
    public partial class SmartTable : UserControl
    {
        [Browsable(false)]
        public MyTable Table { get; set; }

        public MyTableDesignStyles TableStyles
        {
            get { return Table.DesignStyle; }
            set { Table.DesignStyle = value; }
        }

        public event EventHandler OnSelectionChanged
        {
            add { Table.OnSelectionChanged += value; }
            remove { Table.OnSelectionChanged -= value; }
        }

        public Color SelectionColor
        {
            get { return Table.SelectionColor; }
            set
            {
                if (value == Table.SelectionColor)
                    return;

                Table.SelectionColor = value;
                Invalidate();
            }
        }

        public ScrollBarVisibilityOptions ScrollBarVisibilityMode
        {
            get { return Table.ScrollBarVisibilityMode; }
            set { Table.ScrollBarVisibilityMode = value; Invalidate(); }
        }

        public ScrollBarStyle ScrollBarStyle
        {
            get { return Table.ScrollBarStyle; }
            set { Table.ScrollBarStyle = value; Invalidate(); }
        }

        public Color ScrollBarColor
        {
            get { return Table.ScrollBarColor; }
            set
            {
                Table.ScrollBarColor = value;
            }
        }

        public Color ScrollBarPlotColor
        {
            get { return Table.ScrollBarPlotColor; }
            set
            {
                Table.ScrollBarPlotColor = value;
            }
        }

        public new Image BackgroundImage
        {
            get { return Table.BackgroundImage; }
            set { Table.BackgroundImage = value; }
        }

        public float RowHeight
        {
            get { return Table.RowHeight; }
            set
            {
                Table.RowHeight = value;
            }
        }

        public float HeaderHeight
        {
            get { return Table.HeaderHeight; }
            set
            {
                Table.HeaderHeight = value;
            }
        }

        public MyTableSelectionStyles SelectionMode
        {
            get { return Table.SelectionMode; }
            set
            {
                Table.SelectionMode = value;
            }
        }

        public bool ContextMenuEnabled
        {
            get { return ContextMenuStrip != null; }
            set
            {
                ContextMenuStrip = value ? contextMenuStrip1 : null;
            }
        }



        MyTableCell RightClickCell;

        public SmartTable()
        {
            InitializeComponent();
            CheckForIllegalCrossThreadCalls = false;

            Table = new MyTable(new Size(470, 170));
            Controls.Add(Table);
            Table.Dock = DockStyle.Fill;

            Table.Font = new Font("Segoe UI", 12);
            Table.HeaderHeight = 40;
            Table.RowHeight = 35;
            Table.AddColumn("Col 1", 60).BackColor = Color.Orange;
            Table.AddColumn(new MyTableHeaderCell("Col 2", 595, false)
            {
                BackColor = Color.Orange,
                //FillTableSpace = true
                });

            Table.AddColumn("Col 3", 185).BackColor = Color.Orange;
            Table.Location = new Point(20, 10);
            Table.Visible = true;

            //Table.AddRow(0, "1", "Sachin 123456789 10 11 12 13 14", "sac@gmail");
            //Table.AddRow(1, true, "Rohit", "roh@gmail");
            //Table.AddRow(2, "3", "Ram", "ram@gmail");
            //Table.AddRow(5, "3", "Error waali row", "ram@gmail");

            //Table.AddRow(new MyTableCell[] {
            //    new MyTableCellText("3"),
            //    new MyTableCellText("Rohit"),
            //    new MyTableCellText("roh@gmail") });

            //for (int i = 5; i < 50; i++)
            //    Table.AddRow(-1, "" + i, "User " + i, "roh@gmail");

            MyTableCellText c1 = new MyTableCellText("Cell 1");
            MyTableCellText c2 = new MyTableCellText("Cell 2");
            MyTableCellText c3 = new MyTableCellText("Cell 3");

            Table.AddRow(new MyTableCell[] { c1, c2, c3 }, 0);

            Table.HotHighlightColor = Color.WhiteSmoke;

            //Table.AddRow(1, null, Image.FromFile(@"C:\Users\Sachin kushwaha\Pictures\mypic.PNG"), null);

            Table.SelectionMode = MyTableSelectionStyles.ENTIRE_ROW;
            Table.ScrollBarColor = Color.Blue;
            Table.ScrollBarPlotColor = Color.FromArgb(0x9B, 0x9B, 0xF9);
            Table.DesignStyle = MyTableDesignStyles.CUSTOM;
            Table.ScrollBarVisibilityMode = ScrollBarVisibilityOptions.PERMANENT_WITHOUT_PLOT;

            Table.Rows[0][1].ProgressPercent = 50;
        }

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        private static extern bool PeekMessage([In, Out] ref MSG msg, HandleRef hwnd, int msgMin, int msgMax, int remove);

        protected override bool ProcessKeyPreview(ref Message m)
        {
            const int WM_KEYDOWN = 0x100;
            const int WM_KEYUP = 0x101;
            const int WM_CHAR = 0x102;
            const int WM_SYSCHAR = 0x106;
            const int WM_SYSKEYDOWN = 0x104;
            const int WM_SYSKEYUP = 0x105;
            const int WM_IME_CHAR = 0x286;

            KeyEventArgs e = null;

            if ((m.Msg != WM_CHAR) && (m.Msg != WM_SYSCHAR) && (m.Msg != WM_IME_CHAR))
            {
                e = new KeyEventArgs(((Keys)((int)((long)m.WParam))) | ModifierKeys);
                //if ((m.Msg == WM_KEYDOWN) || (m.Msg == WM_SYSKEYDOWN))
                //{
                 //   TrappedKeyDown(e);
                //}
                //else
                //{
                    TrappedKeyUp(e);
                //}

                // Remove any WM_CHAR type messages if supresskeypress is true.
                if (e.SuppressKeyPress)
                {
                    this.RemovePendingMessages(WM_CHAR, WM_CHAR);
                    this.RemovePendingMessages(WM_SYSCHAR, WM_SYSCHAR);
                    this.RemovePendingMessages(WM_IME_CHAR, WM_IME_CHAR);
                }

                if (e.Handled)
                {
                    return e.Handled;
                }
            }
            return base.ProcessKeyPreview(ref m);
        }

        private void RemovePendingMessages(int msgMin, int msgMax)
        {
            if (!this.IsDisposed)
            {
                MSG msg = new MSG();
                IntPtr handle = this.Handle;
                while (PeekMessage(ref msg,
                new HandleRef(this, handle), msgMin, msgMax, 1))
                {
                }
            }
        }

        private void TrappedKeyUp(KeyEventArgs e)
        {
            if (ContainsFocus)
                Table.OnKeyUp(e);
        }

        private void copyToolStripMenuItem_Click(object sender, EventArgs e)
        {
            object data = RightClickCell.GetData();

            if (data.GetType() == typeof(string) || data.GetType() == typeof(bool))
            {
                Clipboard.SetText(data.ToString());
            }
            else if (data.GetType() == typeof(Bitmap))
            {
                Clipboard.SetImage(data as Image);
            }
        }

        private void contextMenuStrip1_Opened(object sender, EventArgs e)
        {
            object data = RightClickCell.GetData();

            if (data.GetType() == typeof(string))
            {
                rtlClkChangeString.Text = data.ToString();
            }
            else if (data.GetType() == typeof(bool))
            {
                rtlClkChangeBool.Text = ((bool)data) ? "Uncheck" : "Check";
            }

            if (Clipboard.ContainsText())
            {
                pasteToolStripMenuItem.Enabled = true;
                pasteToolStripMenuItem.Text = "Paste text";
            }
            else if (Clipboard.ContainsImage())
            {
                pasteToolStripMenuItem.Enabled = true;
                pasteToolStripMenuItem.Text = "Paste image";
            }
            else
            {
                pasteToolStripMenuItem.Enabled = false;
            }

            rtlTbProgressValue.Text = RightClickCell.ProgressPercent.ToString();
            disableGlassEffectToolStripMenuItem.Text = RightClickCell.ProgressGlassEffect ? "Disable glass effect" : "Enabe glass effect";
        }

        private void rtClkChangeImage_Click(object sender, EventArgs e)
        {
            if (DialogResult.OK != ImageChooserDialog.ShowDialog(this))
                return;

            //if (ImageChooserDialog.FileName.Trim() != "")
                RightClickCell.SetData(Image.FromFile(ImageChooserDialog.FileName));
        }

        private void rtClkChangeBool_Click(object sender, EventArgs e)
        {
            bool val = (sender as ToolStripMenuItem).Text == "Check";
            RightClickCell.SetData(val);
        }

        private void rtlClkChangeString_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                RightClickCell.SetData((sender as ToolStripTextBox).TextBox.Text);
                contextMenuStrip1.Close();
            }
        }

        private void contextMenuStrip1_Opening(object sender, CancelEventArgs e)
        {
            RightClickCell = Table.GetCellAt(new Point(contextMenuStrip1.Left, contextMenuStrip1.Top), true);
            if (RightClickCell == null)
            {
                e.Cancel = true;
                return;
            }
        }

        private void rtlTbProgressValue_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                RightClickCell.ProgressPercent = float.Parse(rtlTbProgressValue.Text);
                contextMenuStrip1.Close();
            }

            if ((e.KeyValue < 48 || e.KeyValue > 57 )
                && (e.KeyCode != Keys.Left && e.KeyCode != Keys.Right &&
                    e.KeyCode != Keys.Back && e.KeyCode != Keys.Delete)
                && (!e.Shift && (e.KeyValue < 96 || e.KeyValue > 105)))
            {
                e.Handled = true;
                e.SuppressKeyPress = true;
                return;
            }
        }

        private void colorToolStripMenuItem_Click(object sender, EventArgs e)
        {
            colorDialog1.Color = RightClickCell.ProgressColor;

            if (DialogResult.OK != colorDialog1.ShowDialog(this))
                return;

            RightClickCell.ProgressColor = colorDialog1.Color;
        }

        private void disableGlassEffectToolStripMenuItem_Click(object sender, EventArgs e)
        {
            RightClickCell.ProgressGlassEffect = ! RightClickCell.ProgressGlassEffect;
        }

        private void pasteToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (Clipboard.ContainsText())
            {
                RightClickCell.SetData(Clipboard.GetText());
            }
            else if (Clipboard.ContainsImage())
            {
                RightClickCell.SetData(Clipboard.GetImage());
            }
        }

        private void foreColorToolStripMenuItem_Click(object sender, EventArgs e)
        {
            colorDialog1.Color = RightClickCell.ForeColor;

            if (DialogResult.OK != colorDialog1.ShowDialog(this))
                return;

            RightClickCell.ForeColor = colorDialog1.Color;
        }

        private void backColorToolStripMenuItem_Click(object sender, EventArgs e)
        {
            colorDialog1.Color = RightClickCell.BackColor;

            if (DialogResult.OK != colorDialog1.ShowDialog(this))
                return;

            RightClickCell.SuppressThemeColor = true;
            RightClickCell.BackColor = colorDialog1.Color;
        }

        private void addRowToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Table.AddRow(RightClickCell.RowIndex, " ");
        }

        private void newColumnTextBox1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter && newColumnTextBox1.TextBox.Text != "")
            {
                Table.AddColumn(newColumnTextBox1.TextBox.Text, 84, RightClickCell.ColumnIndex);
                contextMenuStrip1.Close();
            }
        }

        private void deleteRowToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Table.DeleteSelectedRows();
        }
    }

    internal struct MSG
    {
        public IntPtr hwnd;
        public int message;
        public IntPtr wParam;
        public IntPtr lParam;
        public int time;
        public int pt_x;
        public int pt_y;
    }

}
