using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace temp2
{
    [ToolboxItem(false)]
    public partial class MyTable : Control
    {
        public List<MyTableCell[]> Rows { get; set; }
        public List<MyTableHeaderCell> HeaderRow { get; set; }
        public bool HotHighlight { get; set; }
        public bool MultiSelect { get; set; }

        public Color HotHighlightColor { get; set; }
        private List<MyTableCell> HotHighlightedCells;

        public MyTableDesignStyles DesignStyle
        {
            get
            {
                return _designStyle;
            }
            set
            {
                if (Rows != null)
                {
                    Rows.ForEach(delegate (MyTableCell[] cells)
                    {
                        foreach (MyTableCell cell in cells)
                            cell.SuppressThemeColor = false;
                    });
                }

                if (value == _designStyle)
                {
                    Invalidate();
                    return;
                }

                _designStyle = value;
                Invalidate();
            }
        }

        public Color SelectionColor
        {
            get { return selectionColor; }
            set
            {
                if (value == selectionColor)
                    return;

                selectionColor = value;
                Invalidate();
            }
        }

        public ScrollBarVisibilityOptions ScrollBarVisibilityMode
        {
            get { return _scrollBarVisibilityMode; }
            set { _scrollBarVisibilityMode = value; Invalidate(); }
        }

        public ScrollBarStyle ScrollBarStyle
        {
            get { return _scrollBarStyle; }
            set { _scrollBarStyle = value; Invalidate(); }
        }

        public Color ScrollBarColor
        {
            get { return MyTableScrollBar.ScrollBarColor; }
            set
            {
                if (value == MyTableScrollBar.ScrollBarColor)
                    return;

                MyTableScrollBar.ScrollBarColor = value;

                if (ScrollBarVisibilityMode != ScrollBarVisibilityOptions.ONHOVER_WITHOUT_PLOT
                   && ScrollBarVisibilityMode != ScrollBarVisibilityOptions.ONHOVER_WITH_PLOT)
                    Invalidate();
            }
        }

        public Color ScrollBarPlotColor
        {
            get { return MyTableScrollBar.ScrollBarPlotColor; }
            set
            {
                if (value == MyTableScrollBar.ScrollBarPlotColor)
                    return;

                MyTableScrollBar.ScrollBarPlotColor = value;

                if (ScrollBarVisibilityMode == ScrollBarVisibilityOptions.PERMANENT_WITH_PLOT)
                    Invalidate();
            }
        }

        public new Image BackgroundImage
        {
            get { return backgroundImage; }
            set
            {
                if (value == backgroundImage)
                    return;

                backgroundImage = value;
                Invalidate();
            }
        }

        public float RowHeight
        {
            get { return rowHeight; }
            set
            {
                rowHeight = value;

                int rowIndex = 0;
                Rows.ForEach(delegate (MyTableCell[] cells)
                {
                    float y = HeaderHeight + (rowIndex * rowHeight);
                    foreach (MyTableCell cell in cells)
                    {
                        cell.SetY(y);
                        cell.SetHeight(rowHeight);
                    }

                    rowIndex++;
                });

                RefreshScrollBar();

                Invalidate();
            }
        }

        public float HeaderHeight
        {
            get { return headerHeight; }
            set
            {
                headerHeight = value;
                HeaderRow.ForEach(delegate (MyTableHeaderCell cell)
                {
                    cell.SetHeight(headerHeight);
                });

                int rowIndex = 0;
                Rows.ForEach(delegate (MyTableCell[] cells)
                {
                    float y = HeaderHeight + (rowIndex * rowHeight);
                    foreach (MyTableCell cell in cells)
                    {
                        cell.SetY(y);
                    }

                    rowIndex++;
                });

                RefreshScrollBar();

                Invalidate();
            }
        }

        //public float ZoomFactor
        //{
        //    get { return zoomFactor; }
        //    set
        //    {
        //        value = value > 4 ? 4 : value;
        //        value = value < 0.2F ? 0.2F : value;

        //        if (!ZoomEnabled || zoomFactor == value)
        //            return;

        //        zoomFactor = value;
        //        Invalidate();
        //    }
        //}

        //public bool ZoomEnabled {  get; set;   }


        public int RowCount { get { return Rows.Count; } }
        public int ColumnCount { get { return HeaderRow.Count; } }

        float headerHeight;
        float rowHeight;
        ScrollBarVisibilityOptions _scrollBarVisibilityMode;
        ScrollBarStyle _scrollBarStyle;
        MyTableDesignStyles _designStyle;
        MyTableSelectionStyles _mtss;

        private bool activateHeaderResizing = false;
        private MyTableHeaderCell resizingHeaderCell = null;
        private Point mouseOffset;
        private Point currentMouseLocation;
        private bool doHScroll = false;
        private bool doVScroll = false;
        private float zoomFactor = 1;
        private Color selectionColor;
        private Image backgroundImage;
        private List<MyTableCell> prevSelectedCells;
        internal MyTableVerticalScrollBar VscrollBar;
        internal MyTableHorizontalScrollBar HscrollBar;
        internal MyTableCell cellClicked;

        public List<MyTableCell[]> SelectedRows { get; set; }
        public List<MyTableCell[]> SelectedColumns { get; set; }
        public List<MyTableCell> SelectedCells { get; set; }
        public MyTableSelectionStyles SelectionMode
        {
            get { return _mtss; }
            set
            {
                if (value == _mtss)
                    return;

                _mtss = value;
                //  SELECT ENTIRE ROW OR COULMN ACCRODINGLY IF PREV STYLE WAS CELL_ONLY
                Invalidate();
            }
        }


        public event EventHandler OnSelectionChanged;
        public event EventHandler<MyTableCell> OnCellContentChanged;
        public event EventHandler<MyTableCell> OnCellClicked;
        public event EventHandler<int> OnRowAdded;
        public event EventHandler OnScrolled;


        public MyTable(Size size)
        {
            InitializeComponent();
            DoubleBuffered = true;
            HotHighlight = true;
            MultiSelect = true;
            //ZoomEnabled = true;
            HotHighlightColor = Color.WhiteSmoke;
            SelectionColor = Color.DodgerBlue;
            DesignStyle = MyTableDesignStyles.CUSTOM;
            ScrollBarVisibilityMode = ScrollBarVisibilityOptions.PERMANENT_WITHOUT_PLOT;
            ScrollBarStyle = ScrollBarStyle.ROUNDED;
            VscrollBar = new MyTableVerticalScrollBar(this);
            HscrollBar = new MyTableHorizontalScrollBar(this);
            Size = size;
            
            Rows = new List<MyTableCell[]>();
            HeaderRow = new List<MyTableHeaderCell>();
            SelectedRows = new List<MyTableCell[]>();
            SelectedColumns = new List<MyTableCell[]>();
            SelectedCells = new List<MyTableCell>();
            HotHighlightedCells = new List<MyTableCell>();

            HeaderHeight = 20;
            RowHeight = 20;
        }

        protected override void OnPaint(PaintEventArgs pe)
        {
            base.OnPaint(pe);
            pe.Graphics.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
            pe.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

            //if (ZoomFactor != 0)
            //    pe.Graphics.ScaleTransform(ZoomFactor, ZoomFactor);

            float VscrollCoordinateShift = VscrollBar.GetEquivalentCoordinateShift();
            float HscrollCoordinateShift = HscrollBar.GetEquivalentCoordinateShift();

            if (BackgroundImage != null)
            {
                Rectangle rect = HelperFunctions.FitImage(BackgroundImage, new SizeF(Width, Height - HeaderHeight));
                pe.Graphics.DrawImage(BackgroundImage, rect.X, rect.Y + HeaderHeight, rect.Width, rect.Height);
            }


            //  DRAW ROWS BEFORE HEADERS
            pe.Graphics.TranslateTransform(-HscrollCoordinateShift, -VscrollCoordinateShift);
            Rows.ForEach(delegate (MyTableCell[] cells)
            {
                foreach (MyTableCell cell in cells)
                    cell.Paint(pe.Graphics);
            });

            //  DRAW HEADERS
            pe.Graphics.TranslateTransform(0, VscrollCoordinateShift);
            HeaderRow.ForEach(delegate (MyTableHeaderCell headerCell)
            {
                headerCell.Paint(pe.Graphics);
            });

            pe.Graphics.TranslateTransform(HscrollCoordinateShift, 0);

            //  DRAW SCROLLBARS
            DrawScrollBars(pe.Graphics);
        }

        private void DrawScrollBars(Graphics g)
        {
            switch(ScrollBarVisibilityMode)
            {
                case ScrollBarVisibilityOptions.PERMANENT_WITHOUT_PLOT:
                    VscrollBar.Paint(g, Width - 1, HeaderHeight, Height - HeaderHeight - 1, 
                        false, ScrollBarStyle == ScrollBarStyle.ROUNDED);

                    HscrollBar.Paint(g, 0, Height - 1, Width - 1, 
                        false, ScrollBarStyle == ScrollBarStyle.ROUNDED);
                    break;

                case ScrollBarVisibilityOptions.PERMANENT_WITH_PLOT:
                    VscrollBar.Paint(g, Width - 1, HeaderHeight, Height - HeaderHeight - 1,
                        true, ScrollBarStyle == ScrollBarStyle.ROUNDED);

                    HscrollBar.Paint(g, 0, Height - 1, Width - 1, 
                        true, ScrollBarStyle == ScrollBarStyle.ROUNDED);
                    break;

                case ScrollBarVisibilityOptions.ONHOVER_WITHOUT_PLOT:
                    if (doVScroll || VscrollBar.IsMouseInPlot(currentMouseLocation))
                        VscrollBar.Paint(g, Width - 1, HeaderHeight, Height - HeaderHeight - 1,
                            false, ScrollBarStyle == ScrollBarStyle.ROUNDED);

                    if (doHScroll || HscrollBar.IsMouseInPlot(currentMouseLocation))
                        HscrollBar.Paint(g, 0, Height - 1, Width - 1,
                            false, ScrollBarStyle == ScrollBarStyle.ROUNDED);
                    break;

                case ScrollBarVisibilityOptions.ONHOVER_WITH_PLOT:
                    if (doVScroll || VscrollBar.IsMouseInPlot(currentMouseLocation))
                        VscrollBar.Paint(g, Width - 1, HeaderHeight, Height - HeaderHeight - 1,
                            true, ScrollBarStyle == ScrollBarStyle.ROUNDED);

                    if (doHScroll || HscrollBar.IsMouseInPlot(currentMouseLocation))
                        HscrollBar.Paint(g, 0, Height - 1, Width - 1,
                            true, ScrollBarStyle == ScrollBarStyle.ROUNDED);
                    break;
            }
        }

        public MyTableHeaderCell AddColumn(string text, float width, int position = -1)
        {
            MyTableHeaderCell headerCell = (MyTableHeaderCell) GetInitializedTableHeaderCell(new MyTableHeaderCell(text, width), position > -1 ? position : ColumnCount);

            if (position > -1)
            {
                ShiftColumns(position, headerCell.CellRectangle.Width);
                HeaderRow.Insert(position, headerCell);
            }
            else
                HeaderRow.Add(headerCell);


            position = position > -1 ? position : ColumnCount;

            if (Rows != null)
                for (int i = 0; i < Rows.Count; i++)
                {
                    List<MyTableCell> cellArray = new List<MyTableCell>(Rows[i]);
                    cellArray.Insert(position, GetInitializedTableCell(new MyTableCellText(), headerCell, i, position));
                    Rows[i] = cellArray.ToArray();
                }

            RefreshRowColIndices(); //  ALWAYS AFTER ADDING CELLS TO THE NEW COLUMN
            AdjustColsWithFillToSpaceAttribute();

            headerCell.TextAlign = ContentAlignment.MiddleCenter;

            Invalidate();
            return headerCell;
        }

        public MyTableHeaderCell AddColumn(MyTableHeaderCell headerCell, int position = -1)
        {
            if (headerCell == null)
                throw new NullReferenceException();

            if (position > -1)
            {
                headerCell = (MyTableHeaderCell)GetInitializedTableHeaderCell(headerCell, position);
                ShiftColumns(position, headerCell.CellRectangle.Width);
                HeaderRow.Insert(position, headerCell);
            }
            else
            {
                headerCell = (MyTableHeaderCell)GetInitializedTableHeaderCell(headerCell, ColumnCount);
                HeaderRow.Add(headerCell);
            }

            position = position > -1 ? position : ColumnCount;

            if (Rows != null)
                for (int i = 0; i < Rows.Count; i++)
                {
                    List<MyTableCell> cellArray = new List<MyTableCell>(Rows[i]);
                    cellArray.Insert(position, GetInitializedTableCell(new MyTableCellText(), headerCell, i, position));
                    Rows[i] = cellArray.ToArray();
                }

            RefreshRowColIndices(); //  ALWAYS AFTER ADDING CELLS TO THE NEW COLUMN
            AdjustColsWithFillToSpaceAttribute();

            headerCell.SetHeight(HeaderHeight);

            Invalidate();
            return headerCell;
        }

        public void SetAlternateRowColors(Color[] colors)
        {
            int i = 0;
            foreach (MyTableCell[] cells in Rows)
            {
                foreach (MyTableCell cell in cells)
                    cell.BackColor = colors[i];

                i++;
                i = i % colors.Length;
            }
        }

        /// <summary>
        /// <para>Resizes the columns with 'FillTableSpace' attribute to fill the empty space</para>
        /// </summary>
        internal void AdjustColsWithFillToSpaceAttribute()
        {
            float occupiedWidth = 0, colsWithFillSpaceAttribute = 0;

            if (HeaderRow == null)
                return;

            HeaderRow.ForEach(delegate (MyTableHeaderCell cell)
            {
                if (!cell.FillTableSpace)
                    occupiedWidth += cell.CellRectangle.Width;
                else
                    colsWithFillSpaceAttribute++;
            });
            float emptyWidth = Width - 1 - occupiedWidth;
            if (emptyWidth > 0)
            {
                HeaderRow.ForEach(delegate (MyTableHeaderCell cell)
                {
                    if (cell.FillTableSpace)
                        ResizeColumnWidth(cell, (int)(cell.CellRectangle.Width - (emptyWidth / colsWithFillSpaceAttribute)));
                });
            }
        }

        public void AddRow(MyTableCell[] cells, int position = -1)
        {
            MyTableCell[] cellsArray = new MyTableCell[ColumnCount];

            for (int i = 0; i < ColumnCount; i++)
            {
                if (i < cells.Length)
                    cellsArray[i] = GetInitializedTableCell(cells[i], HeaderRow[i], -1, i);
                else
                    cellsArray[i] = GetInitializedTableCell(new MyTableCellText(), HeaderRow[i], -1, i);
            }

            if (position > -1)
            {
                ShiftRows(position, RowHeight);
                Rows.Insert(position > RowCount ? RowCount : position, cellsArray);
            }
            else
            {
                Rows.Add(cellsArray);
                position = RowCount - 1;
            }

            CheckColumnsForFitToContent();
            RefreshRowColIndices();
            RefreshScrollBar();

            OnRowAdded?.Invoke(this, position);

            Invalidate();
        }

        public void AddRow(int position = -1, params object[] data)
        {
            if (data == null)
                return;

            int pos = position == -1 ? RowCount : position;
            MyTableCell[] cells = new MyTableCell[ColumnCount];

            for (int i = 0; i < ColumnCount; i++)
            {
                if (i >= data.Length)
                {
                    MyTableCellText textCell;
                    cells[i] = textCell = (MyTableCellText) GetInitializedTableCell(new MyTableCellText(), HeaderRow[i], pos, i);
                    textCell.Text = "";
                    continue;
                }

                object obj = data[i];

                if (obj == null)
                {
                    MyTableCellText textCell = new MyTableCellText();
                    cells[i] = textCell = (MyTableCellText)GetInitializedTableCell(textCell, HeaderRow[i], pos, i);
                    textCell.Text = "";
                }
                else if (obj.GetType() == typeof(string))
                {
                    MyTableCellText textCell = new MyTableCellText();
                    cells[i] = textCell = (MyTableCellText)GetInitializedTableCell(textCell, HeaderRow[i], pos, i);
                    textCell.Text = (string)obj;
                }
                else if (obj.GetType() == typeof(bool))
                {
                    MyTableCellCheck checkCell = new MyTableCellCheck();
                    cells[i] = checkCell = (MyTableCellCheck)GetInitializedTableCell(checkCell, HeaderRow[i], pos, i);
                    checkCell.Checked = (bool)obj;
                }
                else if (obj.GetType() == typeof(Bitmap))
                {
                    MyTableCellImage imgCell = new MyTableCellImage();
                    cells[i] = imgCell = (MyTableCellImage)GetInitializedTableCell(imgCell, HeaderRow[i], pos, i);
                    imgCell.Image = (Image)obj;
                }
            }

            if (position > -1)
            {
                ShiftRows(position, RowHeight);
                Rows.Insert(position > RowCount ? RowCount : position, cells);
            }
            else
            {
                Rows.Add(cells);
                position = RowCount - 1;
            }

            CheckColumnsForFitToContent();
            RefreshRowColIndices();
            RefreshScrollBar();
            OnRowAdded?.Invoke(this, position);

            Invalidate();
        }

        public void DeleteSelectedRows()
        {
            int total = SelectedRows.Count;

            for (int j = 0; j < total; j++)
            {
                int rowIndex = SelectedRows[0][0].RowIndex - j;
                DeleteRowBase(rowIndex);
            }

            RefreshRowColIndices();
            RefreshScrollBar();

            Invalidate();
        }

        private void DeleteRowBase(int rowIndex)
        {
            if (rowIndex < 0 || rowIndex > RowCount - 1)
                return;

            //  DELETE ROW FROM SELECTED ROWS COLLECTION
            SelectedRows.Remove(Rows[rowIndex]);

            //  DELETE ROW CELLS FROM COLUMNS COLLECTION
            for (int i = 0; i < SelectedColumns.Count; i++)
            {
                List<MyTableCell> list = SelectedColumns[i].ToList();
                list.RemoveAt(rowIndex);
                SelectedColumns[i] = list.ToArray();
            }

            //  DELETE ROW CELLS FROM GENERAL SELECTED CELLS LIST
            foreach (MyTableCell cell in Rows[rowIndex])
                SelectedCells.Remove(cell);

            ShiftRows(rowIndex, -RowHeight);

            //  DELETE ROW
            Rows.RemoveAt(rowIndex);
        }

        public void DeleteRow(int rowIndex)
        {
            DeleteRowBase(rowIndex);

            RefreshRowColIndices();
            RefreshScrollBar();

            Invalidate();
        }

        private void RefreshScrollBar()
        {
            if (HeaderRow == null || HeaderRow.Count == 0)
                return;

            VscrollBar.SetPlotArea(Width - 1, HeaderHeight, Height - HeaderHeight - 1);

            float totalRowsHeight = RowHeight * Rows.Count;
            VscrollBar.SetScrollBarHeight(totalRowsHeight);


            HscrollBar.SetPlotArea(0, Height - 1, Width - 1);

            float totalColsWidth = 0;
            foreach (MyTableCell cell in HeaderRow)
                totalColsWidth += cell.CellRectangle.Width;

            HscrollBar.SetScrollBarWidth(totalColsWidth);
        }

        private void RefreshRowColIndices()
        {
            for (int rowIndex = 0; rowIndex < Rows.Count; rowIndex++)
                for (int colIndex = 0; colIndex < HeaderRow.Count; colIndex++)
                {
                    Rows[rowIndex][colIndex].RowIndex = rowIndex;
                    Rows[rowIndex][colIndex].ColumnIndex = colIndex;
                }
        }

        public void SetRow(int rowIndex, MyTableCell[] cells)
        {
            for (int i = 0; i < cells.Length; i++)
            {
                MyTableCell cell = cells[i];
                cell.OwnerHeader = HeaderRow[i];
                cell.OwnerTable = this;
                cell.CellRectangle = Rows[rowIndex][i].CellRectangle;
                if (cell.Font == null)
                    cell.Font = new Font(HeaderRow[i].Font.FontFamily, HeaderRow[i].Font.Size - 2, FontStyle.Regular);
                cell.RowIndex = rowIndex;
                cell.ColumnIndex = i;
                Rows[rowIndex][i] = cell;
            }

            CheckColumnsForFitToContent();
            Invalidate();
        }

        public void SetRow(int rowIndex, params object[] data)
        {
            for (int i = 0; i < data.Length; i++)
            {
                object obj = data[i];
                MyTableCell cell = null;

                if (obj.GetType() == typeof(string))
                {
                    MyTableCellText textCell = new MyTableCellText();
                    cell = textCell = (MyTableCellText) GetInitializedTableCell(textCell, HeaderRow[i], rowIndex, i);
                    textCell.Text = (string) obj;
                }
                else if (obj.GetType() == typeof(bool))
                {
                    MyTableCellCheck checkCell = new MyTableCellCheck();
                    cell = checkCell = (MyTableCellCheck)GetInitializedTableCell(checkCell, HeaderRow[i], rowIndex, i);
                    checkCell.Checked = (bool) obj;
                }
                else if (obj.GetType() == typeof(Bitmap))
                {
                    MyTableCellImage imgCell = new MyTableCellImage();
                    cell = imgCell = (MyTableCellImage)GetInitializedTableCell(imgCell, HeaderRow[i], rowIndex, i);
                    imgCell.Image = (Image) obj;
                }

                Rows[rowIndex][i] = cell;
            }

            CheckColumnsForFitToContent();
            Invalidate();
        }

        private void ShiftRows(int startRowIndex, float distance)
        {
            for (int i = startRowIndex; i < RowCount; i++)
                for (int j = 0; j < ColumnCount; j++)
                    Rows[i][j].ShiftCellVertically(distance);
        }

        private void ShiftColumns(int startColIndex, float distance)
        {
            for (int i = startColIndex; i < ColumnCount; i++)
            {
                HeaderRow[i].ShiftCellHorizontally(distance);

                for (int j = 0; j < RowCount; j++)
                    Rows[j][i].ShiftCellHorizontally(distance);
            }
        }

        internal void CheckColumnsForFitToContent(MyTableHeaderCell targetHeaderCell = null)
        {
            int startColIndex = targetHeaderCell == null ? 0 : HeaderRow.IndexOf(targetHeaderCell);

            for (int i = startColIndex; i < HeaderRow.Count; i++)
            {
                MyTableHeaderCell headerCell = HeaderRow[i];
                if (headerCell.ColumnExpansionOptions == MyTableHeaderColumnExpansions.NONE)
                    continue;

                float desiredWidth =
                    headerCell.ColumnExpansionOptions == MyTableHeaderColumnExpansions.EXPAND_ONLY
                    ? headerCell.CellRectangle.Width : -1, val2 = -1;

                Rows.ForEach(delegate (MyTableCell[] cells)
                {
                    if ((val2 = cells[i].WidthRequiredToFitContent()) > desiredWidth)
                        desiredWidth = val2;
                });

                if (desiredWidth != headerCell.CellRectangle.Width)
                    ResizeColumnWidth(headerCell, (int)(headerCell.CellRectangle.Width - desiredWidth));
            }
        }

        internal void ResizeColumnWidth(MyTableHeaderCell headerCell, float diff)
        {
            headerCell.SetWidth(headerCell.CellRectangle.Width - diff);

            if (headerCell.CellRectangle.Width < 20)
            {
                headerCell.SetWidth(20);
                return;
            }

            int colIndex = HeaderRow.IndexOf(headerCell);
            for (int i = colIndex + 1; i < HeaderRow.Count; i++)
                HeaderRow[i].SetX(HeaderRow[i].CellRectangle.X - diff);

            Rows.ForEach(delegate (MyTableCell[] cells)
            {
                cells[colIndex].SetWidth(headerCell.CellRectangle.Width);

                for (int i = colIndex + 1; i < HeaderRow.Count; i++)
                    cells[i].SetX(cells[i].CellRectangle.X - diff);
            });

            //  DON'T REFRESH SCROLLBAR SIZE HERE

            Invalidate();
        }

        /// <summary>
        /// <para>Checks the type of cell and if it is not compatible with obj type, then makes a new cell and returns true, else returns false. It also fills the cell with the new obj value</para>
        /// </summary>
        internal bool CheckAndChangeCellTypeIfRequired(object obj, MyTableCell targetCell)
        {
            MyTableCell newCell = null;

            if (obj.GetType() == typeof(string))
            {
                if (targetCell.GetType() == typeof(MyTableCellText) || targetCell.GetType() == typeof(MyTableCellButton))
                {
                    (targetCell as MyTableCellText).Text = (string)obj;
                    return false;
                }

                MyTableCellText textCell = new MyTableCellText();
                newCell = textCell = (MyTableCellText)GetInitializedTableCell(textCell, HeaderRow[targetCell.ColumnIndex], targetCell.RowIndex, targetCell.ColumnIndex);
                targetCell.CloneTo(newCell);
                textCell.Text = (string)obj;
            }
            else if (obj.GetType() == typeof(bool))
            {
                if (targetCell.GetType() == typeof(MyTableCellCheck))
                {
                    (targetCell as MyTableCellCheck).Checked = (bool)obj;
                    return false;
                }

                MyTableCellCheck checkCell = new MyTableCellCheck();
                newCell = checkCell = (MyTableCellCheck)GetInitializedTableCell(checkCell, HeaderRow[targetCell.ColumnIndex], targetCell.RowIndex, targetCell.ColumnIndex);
                targetCell.CloneTo(newCell);
                checkCell.Checked = (bool)obj;
            }
            else if (obj.GetType() == typeof(Bitmap))
            {
                if (targetCell.GetType() == typeof(MyTableCellImage))
                {
                    (targetCell as MyTableCellImage).Image = (Image)obj;
                    return false;
                }

                MyTableCellImage imgCell = new MyTableCellImage();
                newCell = imgCell = (MyTableCellImage)GetInitializedTableCell(imgCell, HeaderRow[targetCell.ColumnIndex], targetCell.RowIndex, targetCell.ColumnIndex);
                targetCell.CloneTo(newCell);
                imgCell.Image = (Image)obj;
            }

            Rows[targetCell.RowIndex][targetCell.ColumnIndex] = newCell;

            ReplaceCellInSelection(targetCell, newCell);

            CheckColumnsForFitToContent();
            Invalidate();
            return true;
        }

        private void ReplaceCellInSelection(MyTableCell oldCell, MyTableCell newCell)
        {
            foreach (MyTableCell[] cells in SelectedRows)
            {
                int index = cells.ToList().IndexOf(oldCell);
                if (index > -1)
                {
                    cells[index] = newCell;
                    break;
                }
            }
            foreach (MyTableCell[] cells in SelectedColumns)
            {
                int index = cells.ToList().IndexOf(oldCell);
                if (index > -1)
                {
                    cells[index] = newCell;
                    break;
                }
            }
            for (int i = 0; i < SelectedCells.Count; i++)
            {
                if (SelectedCells[i] == oldCell)
                {
                    SelectedCells[i] = newCell;
                    break;
                }
            }
        }

        private MyTableCell GetInitializedTableCell(MyTableCell cell, MyTableHeaderCell header, int rowIndex = -1, int colIndex = -1)
        {
            cell.OwnerHeader = header;
            cell.OwnerTable = this;
            cell.IsSelectable = true;
            cell.Font = Font;
            cell.RowIndex = rowIndex;
            cell.ColumnIndex = colIndex;

            if (rowIndex > -1)  //  CHECK REQUIRED IF 'cell' HAS 'Y' ALREADY SET
            {
                float Y = HeaderHeight + (RowHeight * rowIndex);
                cell.SetDimensions(0, Y, header.CellRectangle.Width, RowHeight);
            }

            if (colIndex > -1)   //  CHECK REQUIRED IF 'cell' HAS 'X' ALREADY SET
            {
                float X = header.CellRectangle.X;
                cell.SetDimensions(X, cell.CellRectangle.Y, header.CellRectangle.Width, RowHeight);
            }

            return cell;
        }

        private MyTableCell GetInitializedTableHeaderCell(MyTableHeaderCell cell, int colIndex)
        {
            cell.OwnerHeader = null;
            cell.OwnerTable = this;
            cell.IsSelectable = false;
            cell.Font = this.Font;
            cell.RowIndex = -1;
            cell.ColumnIndex = colIndex;

            if (colIndex > -1)
            {
                float X = 0;
                
                for (int i = 0; i < colIndex; i++)
                { X += HeaderRow[i].CellRectangle.Width; };

                cell.SetDimensions(X, -1, cell.CellRectangle.Width, HeaderHeight + 1);
            }
            return cell;
        }

        private void ClearHotSelection()
        {
            foreach (MyTableCell cell in HotHighlightedCells)
                cell.Highlight = false;
            HotHighlightedCells.Clear();
        }

        private void ClearSelection()
        {
            //  'SELCTEDCELLS' WILL DESELECT CELLS IN 'SELECTEDROWS' AS WELL AS 'SELECTEDCOLUMNS'
        
            SelectedCells.ForEach(delegate (MyTableCell cell)
            {
                cell.Selected = false;
            });

            SelectedRows.Clear();
            SelectedColumns.Clear();
            SelectedCells.Clear();
        }

        public MyTableCell GetCellAt(Point location, bool isScreenCoordinates = false)
        {
            if (isScreenCoordinates)
                location = PointToClient(location);

            foreach (MyTableCell[] cells in Rows)
                foreach (MyTableCell cell in cells)
                    if (VscrollBar.ScrollTransform(HscrollBar.ScrollTransform(cell.CellRectangle)).Contains(location))
                        return cell;

            return null;
        }

        private void InvokeEventOnSelectionChanged()
        {
            if (prevSelectedCells == null || prevSelectedCells.Count != SelectedCells.Count)
            {
                OnSelectionChanged?.Invoke(this, new EventArgs());
                return;
            }

            bool invoke = false;
            foreach (MyTableCell cell in SelectedCells)
                if (!prevSelectedCells.Contains(cell))
                {
                    invoke = true;
                    break;
                }

            if (invoke)
                OnSelectionChanged?.Invoke(this, new EventArgs());
        }

        internal void InvokeEventOnScrolled()
        {
            OnScrolled?.Invoke(this, new EventArgs());
        }

        internal void InvokeEventOnCellContentChanged(MyTableCell cell)
        {
            OnCellContentChanged?.Invoke(this, cell);
        }

        private void HotHighlightCells(Point mouseLocation, bool selectAlso = false)
        {
            ClearHotSelection();
            prevSelectedCells = new List<MyTableCell>(SelectedCells);

            if (selectAlso && ModifierKeys != Keys.Control)
            {
                ClearSelection();
            }

            switch (SelectionMode)
            {
                case MyTableSelectionStyles.CELL_ONLY:
                    Rows.ForEach(delegate (MyTableCell[] cells)
                    {
                        foreach (MyTableCell cell in cells)
                            if (VscrollBar.ScrollTransform(HscrollBar.ScrollTransform(cell.CellRectangle)).Contains(mouseLocation))
                            {
                                cell.Highlight = HotHighlight;
                                HotHighlightedCells.Add(cell);

                                if (selectAlso && cell.IsSelectable)
                                {
                                    cell.Selected = true;
                                }

                                InvokeEventOnSelectionChanged();
                                return;
                            }
                    });
                    break;

                case MyTableSelectionStyles.ENTIRE_COLUMN:
                    int colIndex = 0;
                    List<MyTableCell> colCells = new List<MyTableCell>();

                    HeaderRow.ForEach(delegate (MyTableHeaderCell headerCell)
                    {
                        colCells.Clear();

                        RectangleF colRect = new RectangleF(headerCell.CellRectangle.Left, HeaderHeight, headerCell.CellRectangle.Width, Height);
                        if (colRect.Contains(mouseLocation))
                        {
                            Rows.ForEach(delegate (MyTableCell[] cells)
                            {
                                cells[colIndex].Highlight = HotHighlight;
                                HotHighlightedCells.Add(cells[colIndex]);

                                if (selectAlso && cells[colIndex].IsSelectable)
                                {
                                    cells[colIndex].Selected = true;
                                    colCells.Add(cells[colIndex]);
                                }
                            });

                            if (selectAlso)
                            {
                                if (!SelectedColumns.Contains(colCells.ToArray()))
                                    SelectedColumns.Add(colCells.ToArray());
                                InvokeEventOnSelectionChanged();
                            }

                            return;
                        }

                        colIndex++;
                    });
                    break;

                case MyTableSelectionStyles.ENTIRE_ROW:
                    Rows.ForEach(delegate (MyTableCell[] cells)
                    {
                        RectangleF rowRect = new RectangleF(0, cells.First().CellRectangle.Y, cells.Last().CellRectangle.Right, RowHeight);
                        if (VscrollBar.ScrollTransform(HscrollBar.ScrollTransform(rowRect)).Contains(mouseLocation))
                        {
                            foreach (MyTableCell cell in cells)
                            {
                                cell.Highlight = HotHighlight;
                                HotHighlightedCells.Add(cell);

                                if (selectAlso && cell.IsSelectable)
                                {
                                    cell.Selected = true;
                                }
                            }

                            if (selectAlso)
                            {
                                if (!SelectedRows.Contains(cells))
                                    SelectedRows.Add(cells);
                                InvokeEventOnSelectionChanged();
                            }

                            return;
                        }
                    });
                    break;

                case MyTableSelectionStyles.ROW_AND_COLUMN:
                    colCells = new List<MyTableCell>();
                    colIndex = 0;

                    HeaderRow.ForEach(delegate (MyTableHeaderCell headerCell)
                    {
                        colCells.Clear();

                        RectangleF colRect = new RectangleF(headerCell.CellRectangle.Left, HeaderHeight, headerCell.CellRectangle.Width, Height);
                        if (VscrollBar.ScrollTransform(HscrollBar.ScrollTransform(colRect)).Contains(mouseLocation))
                        {
                            Rows.ForEach(delegate (MyTableCell[] cells)
                            {
                                cells[colIndex].Highlight = HotHighlight;
                                HotHighlightedCells.Add(cells[colIndex]);

                                if (selectAlso && cells[colIndex].IsSelectable)
                                {
                                    cells[colIndex].Selected = true;
                                    colCells.Add(cells[colIndex]);
                                }
                            });

                            if (selectAlso)
                            {
                                if (!SelectedColumns.Contains(colCells.ToArray()))
                                    SelectedColumns.Add(colCells.ToArray());
                                InvokeEventOnSelectionChanged();
                            }

                            return;
                        }
                        colIndex++;
                    });
                    Rows.ForEach(delegate (MyTableCell[] cells)
                    {
                        RectangleF rowRect = new RectangleF(0, cells.First().CellRectangle.Y, cells.Last().CellRectangle.Right, RowHeight);
                        if (VscrollBar.ScrollTransform(HscrollBar.ScrollTransform(rowRect)).Contains(mouseLocation))
                        {
                            foreach (MyTableCell cell in cells)
                            {
                                cell.Highlight = HotHighlight;
                                HotHighlightedCells.Add(cell);

                                if (selectAlso && cell.IsSelectable)
                                {
                                    cell.Selected = true;
                                }
                            }

                            if (selectAlso)
                            {
                                if (!SelectedRows.Contains(cells))
                                    SelectedRows.Add(cells);
                                InvokeEventOnSelectionChanged();
                            }

                            return;
                        }
                    });
                    break;
            }
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            base.OnMouseLeave(e);
            ClearHotSelection();
            Invalidate();
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);
            mouseOffset = e.Location;
            cellClicked = GetCellAt(currentMouseLocation);
            Focus();

            if (VscrollBar.IsMouseInPlot(e.Location))
            {
                if (VscrollBar.IsMouseOnScrollBar(e.Location))
                {
                    doVScroll = true;
                }
                else
                {
                    if (e.Location.Y > VscrollBar.ScrollBar.Y)
                        VscrollBar.DoScroll(e.Location.Y - VscrollBar.ScrollBar.Bottom);
                    else
                        VscrollBar.DoScroll(e.Location.Y - VscrollBar.ScrollBar.Y);

                    Invalidate();
                }

                return;
            }

            if (HscrollBar.IsMouseInPlot(e.Location))
            {
                if (HscrollBar.IsMouseOnScrollBar(e.Location))
                {
                    doHScroll = true;
                }
                else
                {
                    if (e.Location.X > HscrollBar.ScrollBar.X)
                        HscrollBar.DoScroll(e.Location.X - HscrollBar.ScrollBar.Right);
                    else
                        HscrollBar.DoScroll(e.Location.X - HscrollBar.ScrollBar.X);

                    Invalidate();
                }

                return;
            }

            if (resizingHeaderCell != null)
            {
                activateHeaderResizing = true;
            }

            HotHighlightCells(e.Location, true);

            OnCellClicked?.Invoke(this, cellClicked);

            Invalidate();
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            base.OnMouseUp(e);

            resizingHeaderCell = null;
            doVScroll = false;
            doHScroll = false;
            mouseOffset = Point.Empty;

            if (cellClicked is MyTableCellButton)
            {
                cellClicked = null;
                Invalidate();
            }

            if (activateHeaderResizing)
            {
                RefreshScrollBar();
                Invalidate();
                activateHeaderResizing = false;
            }
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);
            currentMouseLocation = e.Location;

            if (activateHeaderResizing)
            {
                int diff = mouseOffset.X - e.X;
                ResizeColumnWidth(resizingHeaderCell, diff);
                mouseOffset = new Point(e.X, e.Y);
                return;
            }

            resizingHeaderCell = null;

            RectangleF headerRect = new RectangleF(0, 0, HeaderRow.Last().CellRectangle.Right, HeaderHeight);
            if (headerRect.Contains(e.Location))
            {
                foreach (MyTableHeaderCell cell in HeaderRow)
                {
                    if (cell.FillTableSpace)
                        continue;

                    RectangleF rightBorder = new RectangleF(HscrollBar.ScrollTransform(cell.CellRectangle).Right - 5, 0, 5, HeaderHeight);
                    if (rightBorder.Contains(e.Location))
                    {
                        Cursor = Cursors.VSplit;
                        resizingHeaderCell = cell;
                        return;
                    }
                }
                Cursor = Cursors.Default;
            }
            else if (doVScroll)
            {
                VscrollBar.DoScroll(e.Y - mouseOffset.Y);
                mouseOffset = e.Location;
            }
            else if (doHScroll)
            {
                HscrollBar.DoScroll(e.X - mouseOffset.X);
                mouseOffset = e.Location;
            }
            else if (HotHighlight)
            {
                Cursor = Cursors.Default;
                HotHighlightCells(e.Location);
            }

            Invalidate();
        }

        protected override void OnMouseWheel(MouseEventArgs e)
        {
            base.OnMouseWheel(e);

            //if (ModifierKeys == Keys.Control)
            //{ 
            //    ZoomFactor += (e.Delta < 0 ? 1 : -1) * 0.1F;
            //    return;
            //}

            VscrollBar.DoScroll((e.Delta < 0 ? 1 : -1) * (SystemInformation.MouseWheelScrollLines + 2));

            //  CODE FOR HORIZONTAL SCROLL

            Invalidate();
        }

        public new void OnKeyUp(KeyEventArgs e)
        {
            base.OnKeyDown(e);

            ChangeSelection(e);
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);

            AdjustColsWithFillToSpaceAttribute();
            RefreshScrollBar();
        }

        private void ChangeSelection(KeyEventArgs e)
        {
            if (SelectionMode == MyTableSelectionStyles.DISABLED
                || SelectedCells.Count == 0)
                return;

            MyTableCell refCell = SelectedCells[0], cellToBeSelected = null;
            prevSelectedCells = new List<MyTableCell>(SelectedCells);

            switch (e.KeyCode)
            {
                case Keys.Up:
                    if (SelectionMode == MyTableSelectionStyles.ENTIRE_COLUMN
                        || refCell.RowIndex == 0)
                            return;

                    if (!e.Shift)
                        ClearSelection();

                    cellToBeSelected = GetAdjacentCell(refCell, up: true);
                    break;

                case Keys.Down:
                    if (SelectionMode == MyTableSelectionStyles.ENTIRE_COLUMN
                        || refCell.RowIndex + 1 >= Rows.Count)
                        return;

                    if (!e.Shift)
                        ClearSelection();

                    cellToBeSelected = GetAdjacentCell(refCell, down: true);
                    break;

                case Keys.Left:
                    if (SelectionMode == MyTableSelectionStyles.ENTIRE_ROW
                        || refCell.ColumnIndex == 0)
                        return;

                    if (!e.Shift)
                        ClearSelection();

                    cellToBeSelected = GetAdjacentCell(refCell, left: true);
                    break;

                case Keys.Right:
                    if (SelectionMode == MyTableSelectionStyles.ENTIRE_ROW
                        || refCell.ColumnIndex + 1 >= HeaderRow.Count)
                        return;

                    if (!e.Shift)
                        ClearSelection();

                    cellToBeSelected = GetAdjacentCell(refCell, right: true);
                    break;
            }

            switch (SelectionMode)
            {
                case MyTableSelectionStyles.CELL_ONLY:
                    if (cellToBeSelected != null)
                    {
                        cellToBeSelected.Selected = true;
                        SelectedCells.Insert(0, cellToBeSelected);
                        InvokeEventOnSelectionChanged();
                    }
                    break;

                case MyTableSelectionStyles.ENTIRE_ROW:
                    if (cellToBeSelected != null)
                    //  SELECT ENTIRE ROW
                    {
                        MyTableCell[] cellsArr = new MyTableCell[HeaderRow.Count];

                        for (int i = 0; i < HeaderRow.Count; i++)
                        {
                            Rows[cellToBeSelected.RowIndex][i].Selected = true;
                            cellsArr[i] = Rows[cellToBeSelected.RowIndex][i];
                        }

                        if (!SelectedRows.Contains(cellsArr))
                            SelectedRows.Insert(0, cellsArr);

                        InvokeEventOnSelectionChanged();
                    }
                    break;

                case MyTableSelectionStyles.ENTIRE_COLUMN:
                    if (cellToBeSelected != null)
                    {//  SELECT ENTIRE COLUMN

                        MyTableCell[] cellsArr = new MyTableCell[Rows.Count];

                        for (int i = 0; i < Rows.Count; i++)
                        {
                            Rows[i][cellToBeSelected.ColumnIndex].Selected = true;
                            cellsArr[i] = Rows[i][cellToBeSelected.ColumnIndex];
                        }

                        if (!SelectedColumns.Contains(cellsArr))
                            SelectedColumns.Insert(0, cellsArr);

                        InvokeEventOnSelectionChanged();
                    }
                    break;

                case MyTableSelectionStyles.ROW_AND_COLUMN:
                    if (cellToBeSelected != null)
                    {
                        MyTableCell[] cellsArr = new MyTableCell[HeaderRow.Count];

                        //  SELECT ENTIRE ROW
                        for (int i = 0; i < HeaderRow.Count; i++)
                        {
                            Rows[cellToBeSelected.RowIndex][i].Selected = true;
                            cellsArr[i] = Rows[cellToBeSelected.RowIndex][i];
                        }

                        if (!SelectedRows.Contains(cellsArr))
                            SelectedRows.Insert(0, cellsArr);

                        cellsArr = new MyTableCell[Rows.Count];

                        //  SELECT ENTIRE COLUMN
                        for (int i = 0; i < Rows.Count; i++)
                        {
                            Rows[i][cellToBeSelected.ColumnIndex].Selected = true;
                            cellsArr[i] = Rows[i][cellToBeSelected.ColumnIndex];
                        }

                        if (!SelectedColumns.Contains(cellsArr))
                            SelectedColumns.Insert(0, cellsArr);

                        InvokeEventOnSelectionChanged();
                    }

                    break;
            }

            if (cellToBeSelected != null)
            {
                //  SCROLL TABLE TO BRING SELECTED CELL INTO VIEW

                RectangleF visibleRect = new RectangleF(0, HeaderHeight, Width - 1, Height - HeaderHeight - 1);

                if (SelectionMode != MyTableSelectionStyles.ENTIRE_COLUMN)
                {
                    RectangleF scrollTransformed = VscrollBar.ScrollTransform(cellToBeSelected.CellRectangle);
                    bool scrollrequired = scrollTransformed.Y < visibleRect.Y || scrollTransformed.Bottom > visibleRect.Bottom;

                    if (scrollrequired)
                    {
                        float ratio = (cellToBeSelected.CellRectangle.Y - (scrollTransformed.Y < visibleRect.Y ? HeaderHeight : Height - RowHeight)) / (RowCount * RowHeight);
                        float scrollbarY = (ratio * VscrollBar.Plot.Height) + HeaderHeight;
                        VscrollBar.DoScroll(scrollbarY - VscrollBar.ScrollBar.Y);
                    }
                }

                if (SelectionMode != MyTableSelectionStyles.ENTIRE_ROW)
                {
                    RectangleF scrollTransformed = HscrollBar.ScrollTransform(cellToBeSelected.CellRectangle);
                    bool scrollrequired = scrollTransformed.X < visibleRect.X || scrollTransformed.Right > visibleRect.Right;

                    if (scrollrequired)
                    {
                        float ratio = ((scrollTransformed.X < visibleRect.X ? cellToBeSelected.CellRectangle.X : cellToBeSelected.CellRectangle.Right)) / GetColsTotalWidth();
                        float scrollbarX = (ratio * HscrollBar.Plot.Width);
                        HscrollBar.DoScroll(scrollbarX - HscrollBar.ScrollBar.X);
                    }
                }
            }

            Invalidate();
        }

        private float GetColsTotalWidth()
        {
            float width = 0;

            foreach (MyTableCell header in HeaderRow)
                width += header.CellRectangle.Width;

            return width;
        }

        private MyTableCell GetAdjacentCell(MyTableCell cell, bool up = false, bool down = false, bool left = false, bool right = false)
        {
            if (up)
            {
                if (cell.RowIndex <= 0)
                    return null;

                return Rows[cell.RowIndex - 1][cell.ColumnIndex];
            }
            else if (down)
            {
                if (cell.RowIndex + 1 >= Rows.Count)
                    return null;

                return Rows[cell.RowIndex + 1][cell.ColumnIndex];
            }
            else if (right)
            {
                if (cell.ColumnIndex + 1 >= HeaderRow.Count)
                    return null;

                return Rows[cell.RowIndex][cell.ColumnIndex + 1];
            }
            else if (left)
            {
                if (cell.ColumnIndex <= 0)
                    return null;

                return Rows[cell.RowIndex][cell.ColumnIndex - 1];
            }

            return null;
        }
    }

    public abstract class MyTableCell : IDisposable
    {
        public Color BorderColor { get; set; }
        public bool SuppressThemeColor { get; set; }
        public bool Highlight { get; set; }
        public bool BorderVisible { get; set; }
        public Font Font { get; set; }
        public bool IsSelectable { get; set; }
        public int RowIndex { get; set; }
        public int ColumnIndex { get; set; }

        Color _backColor;
        public Color BackColor
        {
            get { return _backColor; }
            set
            {
                if (_backColor == value)
                    return;

                _backColor = value;
                OwnerTable?.Invalidate(new Region(CellRectangle));
            }
        }

        Color _foreColor;
        public Color ForeColor
        {
            get { return _foreColor; }
            set
            {
                if (_foreColor == value)
                    return;

                _foreColor = value;
                OwnerTable?.Invalidate(new Region(CellRectangle));
            }
        }

        float _progPercent = 0;
        public float ProgressPercent
        {
            get { return _progPercent; }
            set
            {
                _progPercent = value;
                OwnerTable?.Invalidate(new Region(CellRectangle));
            }
        }

        bool selected;
        public bool Selected
        {
            get { return selected; }
            set
            {
                selected = value;
                if (OwnerTable != null && !OwnerTable.SelectedCells.Contains(this))
                    OwnerTable.SelectedCells.Insert(0, this);
            }
        }

        Color _progColor = Color.LimeGreen;
        public Color ProgressColor
        {
            get { return _progColor; }
            set
            {
                _progColor = value;
                OwnerTable?.Invalidate(new Region(CellRectangle));
            }
        }

        bool _progGlass = false;
        public bool ProgressGlassEffect
        {
            get { return _progGlass; }
            set
            {
                _progGlass = value;
                OwnerTable?.Invalidate(new Region(CellRectangle));
            }
        }

        public MyTableHeaderCell OwnerHeader;
        public MyTable OwnerTable;

        public RectangleF CellRectangle = new RectangleF();

        public abstract event EventHandler<object> OnContentChanged;

        internal virtual void Paint(Graphics g)
        {
            if (OwnerTable != null && OwnerTable.DesignStyle != MyTableDesignStyles.CUSTOM)
            {
                PaintDesign(g);

                if (ProgressPercent > 0)
                    DrawProgressBar(g);

                return;
            }

            if (GetType() == typeof(MyTableCellButton))
                g.FillRectangle(new SolidBrush(Selected ? OwnerTable.SelectionColor : 
                    Highlight ? OwnerTable.HotHighlightColor :
                    OwnerTable.BackgroundImage == null ? OwnerTable.BackColor : Color.Transparent), CellRectangle);
            else
                g.FillRectangle(new SolidBrush(Selected ? OwnerTable.SelectionColor : Highlight ? OwnerTable.HotHighlightColor : OwnerTable.BackgroundImage == null ? BackColor :
                    Color.Transparent), CellRectangle);

            if (ProgressPercent > 0)
                DrawProgressBar(g);

            if (BorderVisible)
                g.DrawRectangle(new Pen(BorderColor), CellRectangle.X, CellRectangle.Y, CellRectangle.Width, CellRectangle.Height);
        }

        protected void PaintDesign(Graphics g)
        {
            g.InterpolationMode = InterpolationMode.HighQualityBicubic;
            g.SmoothingMode = SmoothingMode.AntiAlias;

            switch (OwnerTable.DesignStyle)
            {
                case MyTableDesignStyles.ELEGANT:
                    g.FillRectangle(new SolidBrush(Selected ? OwnerTable.SelectionColor : Highlight ? OwnerTable.HotHighlightColor : this is MyTableHeaderCell ? Color.LightGray : OwnerTable.BackgroundImage == null ? SuppressThemeColor ? BackColor : Color.White : Color.Transparent), CellRectangle);

                    PointF BL = new PointF(CellRectangle.Left, CellRectangle.Bottom);
                    PointF BR = new PointF(CellRectangle.Right, CellRectangle.Bottom);

                    Pen pen = new Pen(Color.LightGray, 1.5F);
                    g.DrawLine(pen, BL, BR);
                    break;

                case MyTableDesignStyles.CLASSIC:
                    Color color;
                    if (this is MyTableHeaderCell)
                    {
                        color = BackColor;
                        g.FillRectangle(new SolidBrush(color), CellRectangle.X, CellRectangle.Y, CellRectangle.Width, CellRectangle.Height);
                    }
                    else
                    {
                        if (Highlight)
                            g.FillRectangle(new SolidBrush(Selected ? OwnerTable.SelectionColor : OwnerTable.BackgroundImage == null ? OwnerTable.HotHighlightColor : SuppressThemeColor ? BackColor : Color.Transparent), CellRectangle);

                        color = OwnerHeader.BackColor;
                        g.DrawRectangle(new Pen(Selected ? OwnerTable.SelectionColor : color), CellRectangle.X, CellRectangle.Y, CellRectangle.Width, CellRectangle.Height);
                    }
                    break;
            }
        }

        private void DrawProgressBar(Graphics g)
        {
            if (ProgressPercent > 100)
                ProgressPercent = 100;

            RectangleF upperRect = new RectangleF(CellRectangle.X, CellRectangle.Y, (ProgressPercent * CellRectangle.Width) / 100, CellRectangle.Height / 2);
            RectangleF lowerRect = new RectangleF(CellRectangle.X, upperRect.Bottom, (ProgressPercent * CellRectangle.Width) / 100, CellRectangle.Height / 2);

            if (ProgressGlassEffect)
            {
                int offset = 30;
                Color lightShade = Color.FromArgb(
                    ProgressColor.R + offset > 255 ? ProgressColor.R - offset : ProgressColor.R + offset,
                    ProgressColor.G + offset > 255 ? ProgressColor.G - offset : ProgressColor.G + offset,
                    ProgressColor.B + offset > 255 ? ProgressColor.B - offset : ProgressColor.B + offset);

                g.FillRectangle(new LinearGradientBrush(upperRect, Color.FromArgb(240, 240, 240), lightShade, 90F), upperRect);
            }
            else
                g.FillRectangle(new SolidBrush(ProgressColor), upperRect);

            g.FillRectangle(new SolidBrush(ProgressColor), lowerRect);
        }

        internal void SetDimensions(float x, float y, float width, float height)
        {
            CellRectangle = new RectangleF(x, y, width, height);
            OnDimensionsChanged();
        }

        internal void SetHeight(float height)
        {
            CellRectangle.Height = height;
            OnDimensionsChanged();
        }

        internal void SetWidth(float width)
        {
            CellRectangle.Width = width;
            OnDimensionsChanged();
        }

        internal void SetX(float x)
        {
            CellRectangle.X = x;
            OnDimensionsChanged();
        }

        internal void SetY(float y)
        {
            CellRectangle.Y = y;
            OnDimensionsChanged();
        }

        internal void ShiftCellVertically(float addDistance)
        {
            SetY(CellRectangle.Y + addDistance);
        }

        internal void ShiftCellHorizontally(float addDistance)
        {
            SetX(CellRectangle.X + addDistance);
        }

        public void Dispose()
        {
            if (Font != null)
            {
                Font.Dispose();
                Font = null;
            }
            if (OwnerHeader != null)
            {
                OwnerHeader.Dispose();
                OwnerHeader = null;
            }
            if (OwnerTable != null)
            {
                OwnerTable.Dispose();
                OwnerTable = null;
            }
        }

        internal void CloneTo(MyTableCell cell)
        {
            foreach (PropertyInfo pi in typeof(MyTableCell).GetProperties())
                pi.SetValue(cell, pi.GetValue(this));

            cell.CellRectangle = CellRectangle;
        }

        protected virtual void OnDimensionsChanged() { }

        internal abstract int WidthRequiredToFitContent();

        public abstract object GetData();

        public void SetData(object value)
        {
            if (OwnerTable != null)
                OwnerTable.CheckAndChangeCellTypeIfRequired(value, this);
            else
            {
                if (value.GetType() == typeof(string) && (GetType() == typeof(MyTableCellText) || GetType() == typeof(MyTableCellButton)))
                {
                    (this as MyTableCellText).Text = (string)value;
                }
                else if (value.GetType() == typeof(bool))
                {
                    (this as MyTableCellCheck).Checked = (bool)value;
                }
                else if (value.GetType() == typeof(Bitmap))
                {
                    (this as MyTableCellImage).Image = (Image)value;
                }
            }
        }
    }

    public class MyTableCellText : MyTableCell
    {
        private string shortenedText;
        string _text;

        public override event EventHandler<object> OnContentChanged;

        public string Text
        {
            get { return _text; }
            set
            {
                if (_text == value)
                    return;

                _text = value;
                SetShortText();

                OwnerTable?.Invalidate(new Region(CellRectangle));
                OwnerTable?.CheckColumnsForFitToContent(OwnerHeader);
                OnContentChanged?.Invoke(this, Text);
                OwnerTable?.InvokeEventOnCellContentChanged(this);
            }
        }

        public ContentAlignment TextAlign { get; set; }
        public bool UsePadding { get; set; }
        public PointF Padding { get; set; }

        public MyTableCellText(string text = "")
        {
            Text = text;
            BackColor = Color.White;
            ForeColor = Color.Black;
            //BorderVisible = true;
            Highlight = false;
            BorderColor = Color.WhiteSmoke;
            Font = new Font("Segoe UI", 12);
            TextAlign = ContentAlignment.MiddleCenter;
        }

        internal override void Paint(Graphics g)
        {
            base.Paint(g);

            if (UsePadding)
                g.DrawString(shortenedText, Font, new SolidBrush(Selected ? Color.Black : ForeColor), CellRectangle.X + Padding.X, CellRectangle.Y + Padding.Y);
            else
                g.DrawString(shortenedText, Font, new SolidBrush(Selected ? Color.Black : ForeColor), CellRectangle, GetStringAlignment());
        }

        protected override void OnDimensionsChanged()
        {
            base.OnDimensionsChanged();
            SetShortText();
        }

        private StringFormat GetStringAlignment()
        {
            StringFormat sf = new StringFormat();

            switch (TextAlign)
            {
                case ContentAlignment.BottomCenter:
                    sf.LineAlignment = StringAlignment.Far;
                    sf.Alignment = StringAlignment.Center;
                    break;

                case ContentAlignment.BottomLeft:
                    sf.LineAlignment = StringAlignment.Far;
                    sf.Alignment = StringAlignment.Near;
                    break;

                case ContentAlignment.BottomRight:
                    sf.LineAlignment = StringAlignment.Far;
                    sf.Alignment = StringAlignment.Far;
                    break;

                case ContentAlignment.MiddleCenter:
                    sf.LineAlignment = StringAlignment.Center;
                    sf.Alignment = StringAlignment.Center;
                    break;

                case ContentAlignment.MiddleLeft:
                    sf.LineAlignment = StringAlignment.Center;
                    sf.Alignment = StringAlignment.Near;
                    break;

                case ContentAlignment.MiddleRight:
                    sf.LineAlignment = StringAlignment.Center;
                    sf.Alignment = StringAlignment.Far;
                    break;

                case ContentAlignment.TopCenter:
                    sf.LineAlignment = StringAlignment.Near;
                    sf.Alignment = StringAlignment.Center;
                    break;

                case ContentAlignment.TopLeft:
                    sf.LineAlignment = StringAlignment.Near;
                    sf.Alignment = StringAlignment.Near;
                    break;

                case ContentAlignment.TopRight:
                    sf.LineAlignment = StringAlignment.Near;
                    sf.Alignment = StringAlignment.Far;
                    break;
            }
            return sf;
        } 

        private void SetShortText()
        {
            shortenedText = Text;

            if (Text == null || Text == "" || CellRectangle.Width < 5)
                return;

            Size s2 = TextRenderer.MeasureText(Text, Font);
            int heightFactor = (int) (CellRectangle.Height / (s2.Height));

            float width = TextRenderer.MeasureText(Text, Font).Width;
            while (width + 4 > CellRectangle.Width * heightFactor && shortenedText.Length > 3)
            {
                shortenedText = shortenedText.Substring(0, shortenedText.Length - 4) + "...";
                width = TextRenderer.MeasureText(shortenedText, Font).Width;
            }
        }

        internal override int WidthRequiredToFitContent()
        {
            int width = TextRenderer.MeasureText(Text, Font).Width;

            if (UsePadding)
                return width + (int)Padding.X + 10;

            return width + 8;
        }

        public override object GetData()
        {
            return Text;
        }
    }

    public class MyTableCellCheck : MyTableCell
    {
        public bool Checked
        {
            get { return _checked; }
            set
            {
                if (value == _checked)
                    return;

                _checked = value;
                OwnerTable?.Invalidate(new Region(CellRectangle));
                OnContentChanged?.Invoke(this, Checked);
                OwnerTable?.InvokeEventOnCellContentChanged(this);

            }
        }

        public bool ShowCrossAsUnchecked
        {
            get { return _showCross; }
            set
            {
                if (value == _showCross)
                    return;

                _showCross = value;
                OwnerTable?.Invalidate(new Region(CellRectangle));
            }
        }

        public float FigureSize
        {
            get { return _figSize; }
            set
            {
                if (value == _figSize)
                    return;

                _figSize = value;
                OwnerTable?.Invalidate(new Region(CellRectangle));
            }
        }

        bool _showCross;
        bool _checked;
        float _figSize;

        public override event EventHandler<object> OnContentChanged;

        public MyTableCellCheck(bool defaultValue = false)
        {
            Checked = defaultValue;
            ForeColor = Color.Black;
            ShowCrossAsUnchecked = true;
            FigureSize = 14;
        }

        internal override int WidthRequiredToFitContent()
        {
            throw new NotImplementedException();
        }

        public override object GetData()
        {
            return Checked;
        }

        internal override void Paint(Graphics g)
        {
            base.Paint(g);

            Pen pen = new Pen(ForeColor, 2);

            RectangleF rect = new RectangleF(CellRectangle.X + (CellRectangle.Width - FigureSize) / 2, CellRectangle.Y + (CellRectangle.Height - FigureSize) / 2, FigureSize, FigureSize);

            if (Checked)
            {
                g.DrawLine(pen, rect.X, rect.Y + 7, rect.X + 5, rect.Y + 11);
                g.DrawLine(pen, rect.X + 4, rect.Y + 11, rect.X + 13, rect.Y + 2);
            }
            else
            {
                g.DrawLine(pen, rect.X, rect.Y, rect.Right, rect.Bottom);
                g.DrawLine(pen, rect.Right, rect.Y, rect.X, rect.Bottom);
            }
        }
    }

    public class MyTableCellImage : MyTableCell
    {
        Image _img;
        public Image Image
        {
            get { return _img; }
            set
            {
                if (value == _img)
                    return;
                _img = value;
                SetImageSizeLocation();
                OwnerTable?.Invalidate();
                OnContentChanged?.Invoke(this, Image);
                OwnerTable?.InvokeEventOnCellContentChanged(this);
            }
        }

        private int posX, posY, Width, Height;

        public override event EventHandler<object> OnContentChanged;

        public MyTableCellImage(Image img = null)
        {
            Image = img;
        }

        private void SetImageSizeLocation()
        {
            if (Image == null)
                return;

            Rectangle rect = HelperFunctions.FitImage(Image, new SizeF(CellRectangle.Width - 4, CellRectangle.Height - 4));
            Height = rect.Height;
            Width = rect.Width;
            posX = rect.X;
            posY = rect.Y;
        }

        internal override int WidthRequiredToFitContent()
        {
            throw new NotImplementedException();
        }

        internal override void Paint(Graphics g)
        {
            base.Paint(g);

            if (Image == null)
                return;

            g.DrawImage(Image, CellRectangle.X + posX + 1, CellRectangle.Y + posY + 1, Width, Height);
        }

        protected override void OnDimensionsChanged()
        {
            base.OnDimensionsChanged();
            SetImageSizeLocation();
        }

        public override object GetData()
        {
            return Image;
        }
    }

    public class MyTableCellButton : MyTableCell
    {
        private string shortenedText;
        StringFormat sf;

        string _text;

        public override event EventHandler<object> OnContentChanged;
        public event EventHandler OnClick;

        public string Text
        {
            get { return _text; }
            set
            {
                if (_text == value)
                    return;

                _text = value;
                SetShortText();

                OwnerTable?.Invalidate(new Region(CellRectangle));
                OwnerTable?.CheckColumnsForFitToContent(OwnerHeader);
                OnContentChanged?.Invoke(this, Text);
                OwnerTable?.InvokeEventOnCellContentChanged(this);
            }
        }

        public MyTableCellButton(string Text = "")
        {
            sf = new StringFormat();
            sf.Alignment = StringAlignment.Center;
            sf.LineAlignment = StringAlignment.Center;
            ForeColor = Color.Black;
            this.Text = Text;
            SetShortText();
        }

        internal override void Paint(Graphics g)
        {
            base.Paint(g);

            int padd = 2;

            if (OwnerTable != null && OwnerTable.cellClicked == this)
                padd = 5;

            RectangleF buttonRect = new RectangleF(CellRectangle.X + padd, CellRectangle.Y + padd, CellRectangle.Width - (2 * padd), CellRectangle.Height - (2 * padd));
            g.FillRectangle(new SolidBrush(BackColor), buttonRect);
            g.DrawString(shortenedText, Font, new SolidBrush(ForeColor), buttonRect, sf);
        }

        internal override int WidthRequiredToFitContent()
        {
            return (int)CellRectangle.Width;
        }

        private void SetShortText()
        {
            shortenedText = Text;

            if (Text == null || Text == "" || CellRectangle.Width < 5)
                return;

            float width = TextRenderer.MeasureText(Text, Font).Width;
            while (width + 4 > CellRectangle.Width && shortenedText.Length > 3)
            {
                shortenedText = shortenedText.Substring(0, shortenedText.Length - 4) + "...";
                width = TextRenderer.MeasureText(shortenedText, Font).Width;
            }
        }

        public override object GetData()
        {
            return Text; 
        }
    }

    [Serializable]
    public class MyTableHeaderCell : MyTableCellText, ISerializable
    {
        MyTableHeaderColumnExpansions _ectfc;
        public MyTableHeaderColumnExpansions ColumnExpansionOptions
        {
            get { return _ectfc; }
            set
            {
                _ectfc = value;
                OwnerTable?.CheckColumnsForFitToContent(this);
            }
        }

        bool _fts;
        public bool FillTableSpace
        {
            get { return _fts; }
            set
            {
                if (_fts == value)
                    return;
                _fts = value;
                OwnerTable?.AdjustColsWithFillToSpaceAttribute();
            }
        }

        public MyTableHeaderCell(string text = "Column", float width = 20, bool fillTableSpace = false)
        {
            IsSelectable = false;
            Text = text;
            ForeColor = Color.Black;
            BackColor = Color.LightGray;
            CellRectangle.Width = width;
            Font = new Font("Segoe UI", 12);
            TextAlign = ContentAlignment.MiddleCenter;
            ColumnExpansionOptions = MyTableHeaderColumnExpansions.NONE;
            FillTableSpace = fillTableSpace;
        }

        MyTableHeaderCell(SerializationInfo info, StreamingContext context)
        {
            IsSelectable = false;
            ForeColor = Color.Black;
            BackColor = Color.LightGray;
            Font = new Font("Segoe UI", 12);

            Text = info.GetString("Text");
            Padding = new PointF(1, 1);
        }

        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("Text", Text);
            info.AddValue("Padding", Padding);
        }
    }




    internal abstract class MyTableScrollBar
    {
        public RectangleF Plot { get; protected set; }
        internal RectangleF ScrollBar;
        protected float scrollBarWidth = 6;
        protected float scrollBarHeight = 40;
        public float ScrollValue;
        public static Color ScrollBarColor;
        public static Color ScrollBarPlotColor;

        protected MyTable OwnerTable;

        public MyTableScrollBar(MyTable owner)
        {
            ScrollValue = 0;
            ScrollBarPlotColor = Color.WhiteSmoke;
            ScrollBarColor = Color.Silver;
            OwnerTable = owner;
        }

        ///<summary>
        ///<para>Returns plot dimensions as Rectangle instead of RectangleF</para>
        ///</summary>
        public Rectangle GetPlot()
        {
            return new Rectangle((int)Plot.X, (int)Plot.Y, (int)Plot.Width, (int)Plot.Height);
        }

        public bool IsMouseInPlot(Point point)
        {
            if (Plot == null)
                return false;

            return Plot.Contains(point);
        }

        public bool IsMouseOnScrollBar(Point point)
        {
            if (ScrollBar == null)
                return false;

            return ScrollBar.Contains(point);
        }

        public abstract void DoScroll(float value);
        
        public abstract void SetPlotArea(float X, float Y, float extendedDimension);

        public abstract float GetEquivalentCoordinateShift();

        public abstract void Paint(Graphics g, float X, float Y, float extendedDimension, bool plotVisible, bool rounded);

        public abstract RectangleF ScrollTransform(RectangleF originalRect);

        public float ScrollTransform(float value)
        {
            return value - GetEquivalentCoordinateShift();
        }
    }

    internal class MyTableVerticalScrollBar : MyTableScrollBar
    {
        protected float targetWindowHeight;

        public MyTableVerticalScrollBar(MyTable owner)
            : base(owner)
        {
            scrollBarWidth = 6;
            scrollBarHeight = 40;
        }

        public override void DoScroll(float value)
        {
            if (value > 0 && ScrollBar.Bottom + value > Plot.Bottom)
            {
                float gapLeft = Plot.Bottom - ScrollBar.Bottom;

                if (gapLeft == 0)
                    return;
                else
                    value = gapLeft;
            }
            else if (value < 0 && ScrollBar.Y - value < Plot.Y)
                return;

            ScrollValue += value;

            if (ScrollValue < 0)
                ScrollValue = 0;

            ScrollBar = new RectangleF(Plot.X, Plot.Y + ScrollValue, Plot.Width, scrollBarHeight);

            OwnerTable?.InvokeEventOnScrolled();
        }

        public void SetScrollBarHeight(float totalRowsHeight)
        {
            targetWindowHeight = totalRowsHeight;
            scrollBarHeight = (Plot.Height * Plot.Height) / totalRowsHeight;
            if (scrollBarHeight > Plot.Height)
                scrollBarHeight = Plot.Height;

            if (ScrollBar.Y + scrollBarHeight > Plot.Bottom)
                DoScroll(Plot.Bottom - ScrollBar.Y - scrollBarHeight);
        }

        public override void SetPlotArea(float X, float Y, float height)
        {
            Plot = new RectangleF(X - scrollBarWidth, Y, scrollBarWidth, height);
            ScrollBar = new RectangleF(Plot.X, Plot.Y + ScrollValue, Plot.Width, scrollBarHeight);
        }

        public override RectangleF ScrollTransform(RectangleF originalRect)
        {
            return new RectangleF(originalRect.X, originalRect.Y - GetEquivalentCoordinateShift(),
                originalRect.Width, originalRect.Height);
        }

        public override float GetEquivalentCoordinateShift()
        {
            if (Plot.Height == scrollBarHeight)
                return 0;

            float percent = ScrollValue / (Plot.Height - scrollBarHeight);
            return percent * (targetWindowHeight - Plot.Height);
        }

        public override void Paint(Graphics g, float X, float Y, float height, bool plotVisible, bool rounded)
        {
            Plot = new RectangleF(X - scrollBarWidth, Y, scrollBarWidth, height);
            if (plotVisible)
            {
                if (rounded)
                    HelperFunctions.FillRoundedRectangle(g, new SolidBrush(Color.FromArgb(220, ScrollBarPlotColor)), Plot, (int)scrollBarWidth / 2, true, true, true, true);
                else
                    g.FillRectangle(new SolidBrush(Color.FromArgb(220, ScrollBarPlotColor)), Plot);
            }

            ScrollBar = new RectangleF(Plot.X, Plot.Y + ScrollValue, Plot.Width, scrollBarHeight);
            if (rounded)
                HelperFunctions.FillRoundedRectangle(g, new SolidBrush(ScrollBarColor), ScrollBar, (int)scrollBarWidth / 2, true, true, true, true);
            else
                g.FillRectangle(new SolidBrush(ScrollBarColor), ScrollBar);
        }
    }

    internal class MyTableHorizontalScrollBar : MyTableScrollBar
    {
        protected float targetWindowWidth;

        public MyTableHorizontalScrollBar(MyTable owner)
            : base(owner)
        {
            scrollBarWidth = 40;
            scrollBarHeight = 6;
        }

        public override void DoScroll(float value)
        {
            if (value > 0 && ScrollBar.Right + value > Plot.Right)
            {
                float gapLeft = Plot.Right - ScrollBar.Right;

                if (gapLeft == 0)
                    return;
                else
                    value = (int)gapLeft;
            }
            else if (value < 0 && ScrollBar.X - value < Plot.X)
                return;

            ScrollValue += value;

            if (ScrollValue < 0)
                ScrollValue = 0;

            ScrollBar = new RectangleF(Plot.X + ScrollValue, Plot.Y, scrollBarWidth, Plot.Height);

            OwnerTable?.InvokeEventOnScrolled();
        }

        public void SetScrollBarWidth(float totalColsWidth)
        {
            targetWindowWidth = totalColsWidth;
            scrollBarWidth = (Plot.Width * Plot.Width) / totalColsWidth;
            if (scrollBarWidth > Plot.Width)
                scrollBarWidth = Plot.Width;

            if (ScrollBar.X + scrollBarWidth > Plot.Right)
                DoScroll(Plot.Right - ScrollBar.X - scrollBarWidth);
        }

        public override void SetPlotArea(float X, float Y, float width)
        {
            Plot = new RectangleF(X, Y - scrollBarHeight, width, scrollBarHeight);
            ScrollBar = new RectangleF(Plot.X + ScrollValue, Plot.Y, scrollBarWidth, Plot.Height);
        }

        public override RectangleF ScrollTransform(RectangleF originalRect)
        {
            return new RectangleF(originalRect.X - GetEquivalentCoordinateShift(), originalRect.Y,
                originalRect.Width, originalRect.Height);
        }

        public override float GetEquivalentCoordinateShift()
        {
            if (Plot.Width == scrollBarWidth)
                return 0;

            float percent = ScrollValue / (Plot.Width - scrollBarWidth);
            return percent * (targetWindowWidth - Plot.Width);
        }

        public override void Paint(Graphics g, float X, float Y, float width, bool plotVisible, bool rounded)
        {
            Plot = new RectangleF(X, Y - scrollBarHeight, width, scrollBarHeight);
            if (plotVisible)
            {
                if (rounded)
                    HelperFunctions.FillRoundedRectangle(g, new SolidBrush(Color.FromArgb(200, ScrollBarPlotColor)), Plot, (int)scrollBarHeight / 2, true, true, true, true);
                else
                    g.FillRectangle(new SolidBrush(Color.FromArgb(200, ScrollBarPlotColor)), Plot);
            }

            ScrollBar = new RectangleF(Plot.X + ScrollValue, Plot.Y, scrollBarWidth, Plot.Height);
            if (rounded)
                HelperFunctions.FillRoundedRectangle(g, new SolidBrush(ScrollBarColor), ScrollBar, (int)scrollBarHeight / 2, true, true, true, true);
            else
                g.FillRectangle(new SolidBrush(ScrollBarColor), ScrollBar);
        }
    }


    public enum MyTableCellType
    {
        Check,
        Text,
        Image,
        Button
    }
    
    public enum MyTableHeaderColumnExpansions
    {
        NONE,
        EXPAND_ONLY,
        EXPAND_AND_SHRINK
    }

    public enum MyTableSelectionStyles
    {
        DISABLED,
        CELL_ONLY,
        ENTIRE_ROW,
        ENTIRE_COLUMN,
        ROW_AND_COLUMN
    }

    public enum MyTableDesignStyles
    {
        CLASSIC,
        MATERIAL,
        ELEGANT,
        CUSTOM
    }

    public enum ScrollBarVisibilityOptions
    {
        ///<summary>
        ///<para>Shows scroll bar with the entire background scolling area</para>
        ///</summary>
        PERMANENT_WITH_PLOT,
        ///<summary>
        ///<para>Shows scroll bar only</para>
        ///</summary>
        PERMANENT_WITHOUT_PLOT,
        ///<summary>
        ///<para>Shows scroll bar with the entire background scrolling area when mouse is brought over the scrolling area</para>
        ///</summary>
        ONHOVER_WITH_PLOT,
        ///<summary>
        ///<para>Shows scroll bar only when mouse is brought over the scrolling area</para>
        ///</summary>
        ONHOVER_WITHOUT_PLOT,
    }

    public enum ScrollBarStyle
    {
        ///<summary>
        ///<para>Scroll bar is of normal rectangle shape</para>
        ///</summary>
        NORMAL,
        ///<summary>
        ///<para>Scroll bar has rounded upper and bottom edges</para>
        ///</summary>
        ROUNDED
    }
}
