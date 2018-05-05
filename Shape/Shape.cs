using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace temp2
{
    public partial class Shape : Control
    {
        public PointF[] Points
        {
            get { return points; }
            set
            {
                if (null == value)
                    return;

                points = value;

                RefreshRatios();
                RefreshDrawingPoints();
            }
        }

        public bool ResizeShape
        {
            get { return resizeShape; }
            set { resizeShape = value; Invalidate(); }
        }

        public bool FillShape
        {
            get { return fillShape; }
            set { fillShape = value; Invalidate(); }
        }

        public float PenWidth
        {
            get { return penWidth; }
            set { penWidth = value; Invalidate(); }
        }

        PointF[] points;
        PointF[] DrawingPoints;
        KeyValuePair<float, float>[] Ratios;
        bool fillShape;
        bool resizeShape;
        float penWidth;

        public Shape()
        {
            InitializeComponent();
            DoubleBuffered = true;
            ResizeShape = true;

            SetStyle(ControlStyles.SupportsTransparentBackColor, true);
            BackColor = Color.FromArgb(0, BackColor);
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);

            if (ResizeShape)
                RefreshDrawingPoints();
            else
                RefreshRatios();
        }

        private void RefreshDrawingPoints()
        {
            if (Points == null || Ratios == null)
                return;

            DrawingPoints = new PointF[Points.Length];

            for (int i = 0; i < DrawingPoints.Length; i++)
            {
                KeyValuePair<float, float> ratio = Ratios[i];
                DrawingPoints[i] = new PointF(ratio.Key * Width, ratio.Value * Height);
            }

            Invalidate();
        }

        private void RefreshRatios()
        {
            Ratios = new KeyValuePair<float, float>[points.Length];

            for (int i = 0; i < Ratios.Length; i++)
            {
                PointF point = Points[i];

                if (Width == 0 || Height == 0)
                    Ratios[i] = new KeyValuePair<float, float>(1, 1);
                else
                    Ratios[i] = new KeyValuePair<float, float>(point.X / Width, point.Y / Height);
            }
        }

        protected override void OnPaint(PaintEventArgs pe)
        {
            base.OnPaint(pe);

            if (DrawingPoints == null)
                return;

            pe.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

            if (fillShape)
                pe.Graphics.FillPolygon(new SolidBrush(ForeColor), DrawingPoints);
            else
            {
                pe.Graphics.FillPolygon(new SolidBrush(ForeColor), DrawingPoints);
                //MessageBox.Show(this, string.Join(Environment.NewLine, Ratios));
                //MessageBox.Show(this, Name);
                //pe.Graphics.DrawLines(new Pen(ForeColor, penWidth), DrawingPoints);
            }
        }
    }
}
