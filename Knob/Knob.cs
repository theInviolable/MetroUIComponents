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
    public partial class Knob : Control
    {
        float percentValue;
        public float PercentValue
        {
            get { return percentValue; }
            set
            {
                if (value > 100)
                    value = 100;
                else if (value < 0)
                    value = 0;

                if (value == percentValue)
                    return;

                percentValue = value;

                Invalidate();
            }
        }

        bool showPercent;
        public bool ShowPercentage
        {
            get { return showPercent; }
            set { showPercent = value; Invalidate(); }
        }

        int nofMarks;
        public int NumberOfMarks
        {
            get { return nofMarks; }
            set { nofMarks = value; Invalidate(); }
        }

        bool showMarks;
        public bool ShowMarks
        {
            get { return showMarks; }
            set { showMarks = value; Invalidate(); }
        }

        float markLengthPercent;
        public float MarkLengthPercent
        {
            get { return markLengthPercent; }
            set { markLengthPercent = value; Invalidate(); }
        }

        float markInnerPadding;
        public float MarkInnerPadding
        {
            get { return markInnerPadding; }
            set { markInnerPadding = value; Invalidate(); }
        }

        float increment;
        public float Increment
        {
            get { return increment; }
            set { increment = value; }
        }

        bool circularPointer;
        public bool CircularPointer
        {
            get { return circularPointer; }
            set { circularPointer = value; Invalidate(); }
        }


        float TotalAngleSweeped = 270;
        float AngleStartOffset = 135;
        float radius = 30;
        PointF knobCenter;

        bool mouseEnter = false;
        bool mouseDown = false;
        Point mouseOffset;



        public Knob()
        {
            InitializeComponent();
            DoubleBuffered = true;
           
            PercentValue = 50;
            MarkLengthPercent = 100;
            NumberOfMarks = 10;
            Increment = 0.5F;
        }

        protected override void OnResize(EventArgs e)
        {
            Height = Width;

            radius = (Width / 2) - 10;
            base.OnResize(e);
        }

        protected override void OnPaint(PaintEventArgs pe)
        {
            base.OnPaint(pe);

            pe.Graphics.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
            pe.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

            try {
                DrawKnobBoundaryAndCenter(pe.Graphics);

                DrawKnobPointer(pe.Graphics);

                DrawText(pe.Graphics);

                DrawMarks(pe.Graphics);
            }
            catch(Exception ex)   { }
        }

        protected override void OnParentChanged(EventArgs e)
        {
            if (Parent != null)
                Parent.MouseWheel += Parent_MouseWheel;

            base.OnParentChanged(e);
        }

        private void Parent_MouseWheel(object sender, MouseEventArgs e)
        {
            Control parent = sender as Control;

            if (Parent.Controls.Contains(this))
                OnMouseWheel(e);
            else
                parent.MouseWheel -= Parent_MouseWheel;
        }

        private void DrawKnobBoundaryAndCenter(Graphics g)
        {
            knobCenter = new PointF(Width / 2, Height / 2);

            //  DRAW BOUNDARY
            g.DrawArc(Pens.Black, new RectangleF(knobCenter.X - radius, knobCenter.Y - radius, radius * 2, radius * 2), AngleStartOffset, TotalAngleSweeped);

            //  DRAW CENTER POINT
            //g.FillPie(Brushes.Black, knobCenter.X - 1, knobCenter.Y - 1, 2, 2, 0, 360);
        }

        private void DrawKnobPointer(Graphics g)
        {
            float radians = ConvertToRadians((PercentValue / 100) * TotalAngleSweeped + AngleStartOffset);

            if (CircularPointer)
            {
                float pointerPointRelative = 4 * (radius / 5);
                float pointerRadius = radius / 10;

                float pointX = (float)(knobCenter.X + pointerPointRelative * Math.Cos(radians));
                float pointY = (float)(knobCenter.Y + pointerPointRelative * Math.Sin(radians));

                g.FillPie(Brushes.Black, pointX - pointerRadius, pointY - pointerRadius, 2 * pointerRadius, 2 * pointerRadius, 0, 360);
            }

            else
            {
                float pointerOnCircleX = (float)(knobCenter.X + radius * Math.Cos(radians));
                float pointerOnCircleY = (float)(knobCenter.Y + radius * Math.Sin(radians));

                float pointerNearToCenterX = (float)(knobCenter.X + (2 * radius / 3) * Math.Cos(radians));
                float pointerNearToCenterY = (float)(knobCenter.Y + (2 * radius / 3) * Math.Sin(radians));

                g.DrawLine(new Pen(Color.Black, 2), new PointF(pointerNearToCenterX, pointerNearToCenterY), new PointF(pointerOnCircleX, pointerOnCircleY));
            }
        }

        private void DrawText(Graphics g)
        {
            RectangleF centerRect = new RectangleF(knobCenter.X - (radius), knobCenter.Y - (radius / 4), 2 * (radius), 2 * (radius / 4));

            StringFormat sf = new StringFormat();
            sf.LineAlignment = StringAlignment.Center;
            sf.Alignment = StringAlignment.Center;

           
            //  SHOW PERCENTAGE
            if (ShowPercentage)
                g.DrawString(PercentValue.ToString("##0.##") + "%", Font, new SolidBrush(ForeColor), centerRect, sf);
        }

        private void DrawMarks(Graphics g)
        {
            if (!ShowMarks || NumberOfMarks < 1)
                return;

            float anglePerMark = TotalAngleSweeped / (NumberOfMarks + 1);

            for (int i = 1; i <= NumberOfMarks; i++)
            {
                PointF start, end;

                GetMarkPoints(i * anglePerMark, out start, out end);

                g.DrawLine(Pens.Black, start, end);
            }
        }

        private void GetMarkPoints(float angle, out PointF startPoint, out PointF endPoint)
        {
            float radians = ConvertToRadians(angle + AngleStartOffset);
            float markStart = 3 + markInnerPadding, markLength = 7 * (markLengthPercent / 100);

            float nearToBoundaryX = (float)(knobCenter.X + (radius + markStart) * Math.Cos(radians));
            float nearToBoundaryY = (float)(knobCenter.Y + (radius + markStart) * Math.Sin(radians));

            float endX = (float)(knobCenter.X + (radius + markStart + markLength) * Math.Cos(radians));
            float endY = (float)(knobCenter.Y + (radius + markStart + markLength) * Math.Sin(radians));

            startPoint = new PointF(nearToBoundaryX, nearToBoundaryY);
            endPoint = new PointF(endX, endY);
        }

        private float ConvertToRadians(float degree)
        {
            return (float)(degree * Math.PI / 180);
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            mouseDown = true;
            mouseOffset = e.Location;

            base.OnMouseDown(e);
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            if (mouseDown)
            {
                float increment = (e.X - mouseOffset.X - e.Y + mouseOffset.Y) * Increment;

                PercentValue += increment;
                mouseOffset = e.Location;
            }

            base.OnMouseMove(e);
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            mouseDown = false;
            base.OnMouseUp(e);
        }

        protected override void OnMouseWheel(MouseEventArgs e)
        {
            if (!mouseEnter)
                return;

            float increment = (e.Delta > 0 ? 1 : -1) * Increment;

            PercentValue += increment;

            base.OnMouseWheel(e);
        }

        protected override void OnMouseEnter(EventArgs e)
        {
            mouseEnter = true;

            base.OnMouseEnter(e);
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            mouseEnter = false;
            base.OnMouseLeave(e);
        }
    }
}
