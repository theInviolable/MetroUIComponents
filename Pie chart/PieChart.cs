using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace temp2
{
    public partial class PieChart : Control
    {
        PieData[] piedata;
        public PieData[] PieData
        {
            get { return piedata; }
            set { piedata = value; Invalidate(); }
        }

        float pieWidth;
        public float PieWidth
        {
            get { return pieWidth; }
            set { pieWidth = value; Invalidate(); }
        }

        float boundaryWidth;
        public float BoundaryWidth
        {
            get { return boundaryWidth; }
            set { boundaryWidth = value; Invalidate(); }
        }

        Color boundaryColor;
        public Color BoundaryColor
        {
            get { return boundaryColor; }
            set { boundaryColor = value; Invalidate(); }
        }

        bool fillPie;
        public bool FillPie
        {
            get { return fillPie; }
            set { fillPie = value; Invalidate(); }
        }

        bool showLegend;
        public bool ShowLegend
        {
            get { return showLegend; }
            set { showLegend = value; Invalidate(); }
        }

        bool showPieValues;
        public bool ShowPieValues
        {
            get { return showPieValues; }
            set { showPieValues = value; Invalidate(); }
        }

        Font legendFont;
        public Font LegendsFont
        {
            get { return legendFont; }
            set { legendFont = value; Invalidate(); }
        }

        public AnimationStyles AnimationStyle { get; set; }


        Pen BoundaryPen;
        int animationAngle = 360;    //  ALWAYS INITIALIZE TO 360;
        Thread animatorThread;
        bool isSum100 = false;

        public PieChart()
        {
            InitializeComponent();
            DoubleBuffered = true;
            CheckForIllegalCrossThreadCalls = false;

            LegendsFont = new Font(Font.FontFamily, 9, FontStyle.Regular);
            AnimationStyle = AnimationStyles.NON_UNIFORM;

            PieWidth = 12;
            BoundaryColor = Color.Black;
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);

            if (!FillPie)
            {
                if (PieWidth * 2 + 25 > Height)
                    Height = (int)(PieWidth * 2 + 30);

                //Height = Height;
            }
        }

        protected override void OnPaint(PaintEventArgs pe)
        {
            base.OnPaint(pe);

            if (PieData == null)
                return;

            pe.Graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
            pe.Graphics.SmoothingMode = SmoothingMode.AntiAlias;

            RectangleF baseRect = new RectangleF((PieWidth + BoundaryWidth) / 2, (PieWidth + BoundaryWidth) / 2, Height - 1 - PieWidth - BoundaryWidth, Height - 1 - PieWidth - BoundaryWidth);

            PointF startPoint, endPoint;
            float startAngle = 0;
            BoundaryPen = new Pen(BoundaryColor, BoundaryWidth);


            //  TO PREVENT ABSURD BEHAVIOUR WHILE ANIMATING
            if (isSum100)
            {
                //  LAST SEGMENT
                pe.Graphics.FillPie(new SolidBrush(PieData.Last().Color), BoundaryWidth / 2, BoundaryWidth / 2, Height - 1 - BoundaryWidth, Height - 1 - BoundaryWidth, 0, 360);
                pe.Graphics.DrawArc(BoundaryPen, GetOuterBoundaryRect(baseRect), 0, 360);

                if (!FillPie)
                    pe.Graphics.DrawArc(BoundaryPen, GetInnerBoundaryRect(baseRect), 0, 360);
            }


            //  DRAW PIE
            foreach (PieData pieData in PieData)
            {
                float sweepAngle = (pieData.Percent / 100) * animationAngle;

                if (startAngle + sweepAngle > 360)
                    sweepAngle = 360 - startAngle;

                pe.Graphics.FillPie(new SolidBrush(pieData.Color), BoundaryWidth / 2, BoundaryWidth / 2, Height - 1 - BoundaryWidth, Height - 1 - BoundaryWidth, startAngle, sweepAngle);

                startAngle += sweepAngle;
            }

            
            //  DRAW BOUNDARIES
            DrawAllBoundaries(pe.Graphics, baseRect);


            //  LAST SEGMENT INNER AND OUTER BOUNDARY CONNECTOR
            if (BoundaryWidth > 0 && !isSum100)
            {
                GetPieSegmentCornerPoints(startAngle, out startPoint, out endPoint);
                pe.Graphics.DrawLine(BoundaryPen, startPoint, endPoint);
            }


            //  MAKE CENTER WHITE
            if (!FillPie)
                pe.Graphics.FillPie(Brushes.White, PieWidth + BoundaryWidth, PieWidth + BoundaryWidth, 2 * ((Height - 1) / 2 - PieWidth - BoundaryWidth), 2 * ((Height - 1) / 2 - PieWidth - BoundaryWidth), 0, 360);


            DrawValuesOnPie(pe.Graphics, baseRect.Height / 2);

            DrawLegends(pe.Graphics);
        }

        private void DrawAllBoundaries(Graphics g, RectangleF baseRect)
        {
            if (BoundaryWidth > 0)
            {
                float startAngle = 0;
                PointF startPoint, endPoint;

                foreach (PieData pieData in PieData)
                {
                    float sweepAngle = (pieData.Percent / 100) * animationAngle;

                    GetPieSegmentCornerPoints(startAngle, out startPoint, out endPoint);
                    DrawBoundaries(g, startPoint, endPoint, baseRect, startAngle, sweepAngle);

                    startAngle += sweepAngle;
                }
            }
        }

        private void DrawLegends(Graphics g)
        {
            if (ShowLegend)
            {
                //  DRAW LEGENDS

                float legendsDivLeft = Height + 30;
                float legendsTop = 10;
                const float colorRectWidth = 20;
                const float colorRectHeight = 10;

                foreach (PieData pieData in PieData)
                {

                    //  DRAW TEXT
                    RectangleF textRectF = new RectangleF(legendsDivLeft + colorRectWidth + 10, legendsTop, Width - 1 - legendsDivLeft - colorRectWidth - 15, 14);
                    Size textSize = TextRenderer.MeasureText(pieData.Name, LegendsFont, new Size((int)textRectF.Width, (int)textRectF.Height));

                    //  BALANCE WIDTH
                    if (textSize.Width > textRectF.Width)
                    {
                        textRectF = new RectangleF(textRectF.X, textRectF.Y, textRectF.Width, (textSize.Width / textRectF.Width) * (textSize.Height + 8));
                        textSize = TextRenderer.MeasureText(pieData.Name, LegendsFont, new Size((int)textRectF.Width, (int)textRectF.Height));
                    }


                    //  BALANCE HEIGHT
                    textRectF = new RectangleF(textRectF.X, textRectF.Y, textRectF.Width, textRectF.Height < textSize.Height ? textSize.Height + 4 : textRectF.Height);

                    g.DrawString(pieData.Name, LegendsFont, new SolidBrush(ForeColor), textRectF, new StringFormat() { LineAlignment = StringAlignment.Center, Alignment = StringAlignment.Near });


                    //  DRAW COLOR RECTANGLE
                    RectangleF colorRectF = new RectangleF(legendsDivLeft, legendsTop + (textRectF.Height - colorRectHeight) / 2, colorRectWidth, colorRectHeight);
                    g.FillRectangle(new SolidBrush(pieData.Color), colorRectF);

                    legendsTop += textRectF.Height + 10;
                }
            }
        }

        private void DrawValuesOnPie(Graphics g, float radius)
        {
            float startAngle = 0;

            if (ShowPieValues)
            {
                //  DRAW TEXT
                foreach (PieData pieData in PieData)
                {
                    float sweepAngle = (pieData.Percent / 100) * animationAngle;
                    PointF rectCenterPoint = GetPointOnCircle(radius, startAngle + sweepAngle / 2);

                    g.DrawString(pieData.Percent.ToString("##0.###") + "%", Font, new SolidBrush(ForeColor), rectCenterPoint, new StringFormat() { LineAlignment = StringAlignment.Center, Alignment = StringAlignment.Center });

                    startAngle += sweepAngle;
                }
            }
        }

        private void GetPieSegmentCornerPoints(float startAngle, out PointF startPoint, out PointF endPoint)
        {
            float radians = ConvertToRadians(startAngle);
            float innerRadius = FillPie ? 0 : ((Height - 1) / 2) - PieWidth - BoundaryWidth;
            float outerRadius = ((Height - 1) / 2);

            float outerX = (float)((Height - 1) / 2 + outerRadius * Math.Cos(radians));
            float outerY = (float)((Height - 1) / 2 + outerRadius * Math.Sin(radians));

            float innerX = (float)((Height - 1) / 2 + innerRadius * Math.Cos(radians));
            float innerY = (float)((Height - 1) / 2 + innerRadius * Math.Sin(radians));

            startPoint = new PointF(innerX, innerY);
            endPoint = new PointF(outerX, outerY);
        }

        private float ConvertToRadians(float degree)
        {
            return (float)(degree * Math.PI / 180);
        }

        private void DrawBoundaries(Graphics g, PointF startPoint, PointF endPoint, RectangleF rectF, float startAngle, float sweepAngle)
        {
            //  OUTER BOUNDARY
            g.DrawArc(BoundaryPen, GetOuterBoundaryRect(rectF), startAngle, sweepAngle);

            //  INNER BOUNDARY
            if (!FillPie)
                g.DrawArc(BoundaryPen, GetInnerBoundaryRect(rectF), startAngle, sweepAngle);

            //  CONNECT INNER AND OUTER BOUNDARIES
            g.DrawLine(BoundaryPen, startPoint, endPoint);
        }

        private RectangleF GetOuterBoundaryRect(RectangleF baseRect)
        {
            return new RectangleF(baseRect.X - PieWidth / 2, baseRect.Y - PieWidth / 2, baseRect.Height + PieWidth, baseRect.Height + PieWidth);
        }

        private RectangleF GetInnerBoundaryRect(RectangleF baseRect)
        {
            return new RectangleF(baseRect.X + PieWidth / 2, baseRect.Y + PieWidth / 2, baseRect.Height - PieWidth, baseRect.Height - PieWidth);
        }

        private PointF GetPointOnCircle(float radius, float angle)
        {
            float radians = ConvertToRadians(angle);

            float X = (float)((Height - 1) / 2 + radius * Math.Cos(radians));
            float Y = (float)((Height - 1) / 2 + radius * Math.Sin(radians));

            return new PointF(X, Y);
        }

        public void AnimateAndLoad()
        {
            if (PieData == null)
                return;

            isSum100 = PieData.Sum(a => a.Percent) >= 100;

            if (animatorThread != null)
                animatorThread.Abort();

            //  DON"T CHANGE TO 'Tasks.Run'
            animatorThread = new Thread(delegate ()
            {
                for (animationAngle = 4; animationAngle < 361;)
                {
                    if (animationAngle + 4 > 360)
                        animationAngle = 360;
                    else
                        animationAngle += 4;

                    Invalidate();
                    Thread.Sleep(10);
                }

                animatorThread = null;
            });
            animatorThread.IsBackground = true;
            animatorThread.Start();
        }

        public void ChangePiePercent(int[] indices, float[] newPercents)
        {
            if (PieData.Length <= indices.Max())
                throw new Exception("Index out of range: " + indices.Max());

            if (indices.Length != newPercents.Length)
                throw new Exception("Number of indices must be equal to the number of percentage values");

            float sum = 0;
            for (int i = 0; i < PieData.Length; i++)
                sum += indices.Contains(i) ? newPercents[new List<int>(indices).IndexOf(i)] : PieData[i].Percent;
            isSum100 = sum >= 100;


            if (animatorThread != null)
                animatorThread.Abort();

            //  DON"T CHANGE TO 'Tasks.Run'
            animatorThread = new Thread(delegate ()
            {
                float increment = (float)0.05;
                List<bool> satisfiedList = new List<bool>(indices.Length);
                float maxDiff = 0;

                for (int i = 0; i < indices.Length; i++)
                {
                    if (AnimationStyle == AnimationStyles.NO_ANIMATION)
                    {
                        PieData[indices[i]].Percent = newPercents[i];
                        continue;
                    }

                    satisfiedList.Add(false);

                    float diff = PieData[indices[i]].Percent - newPercents[i];
                    if (diff < 0)
                        diff = -1 * diff;

                    if (maxDiff < diff)
                        maxDiff = diff;
                }

                if (AnimationStyle == AnimationStyles.NO_ANIMATION)
                {
                    Invalidate();
                    return;
                }

                if (AnimationStyle == AnimationStyles.UNIFORM)
                {
                    if (maxDiff > 30)
                        increment = 2;
                    else if (maxDiff > 10)
                        increment = 1;
                    else if (maxDiff > 1)
                        increment = 0.5F;
                }

                while (satisfiedList.FindIndex(a => a == false) > -1)
                {
                    for (int j = 0; j < indices.Length; j++)
                    {
                        int i = indices[j];

                        float oldPercent = PieData[i].Percent;
                        float newPercent = newPercents[j];

                        if (AnimationStyle == AnimationStyles.NON_UNIFORM)
                        {
                            maxDiff = oldPercent - newPercent;
                            maxDiff = maxDiff < 0 ? maxDiff * -1 : maxDiff;
                            if (maxDiff > 30)
                                increment = 2;
                            else if (maxDiff > 5)
                                increment = 1;
                            else if (maxDiff > 0.5)
                                increment = 0.1F;
                        }

                        if (oldPercent < newPercent)
                        {
                            if (PieData[i].Percent + increment > newPercent)
                                oldPercent = PieData[i].Percent = newPercent;
                            else
                                oldPercent = PieData[i].Percent += increment;
                        }
                        else if (oldPercent > newPercent)
                        {
                            if (PieData[i].Percent - increment < newPercent)
                                oldPercent = PieData[i].Percent = newPercent;
                            else
                                oldPercent = PieData[i].Percent -= increment;
                        }
                        else
                            satisfiedList[j] = true;
                    }

                    Invalidate();
                    Thread.Sleep(10);
                }

                animatorThread = null;
            });
            animatorThread.IsBackground = true;
            animatorThread.Start();
        }
    }

    public class PieData
    {
        public string Name { get; set; }
        public float Percent { get; set; }
        public Color Color { get; set; }
    }

    public enum AnimationStyles
    {
        NO_ANIMATION,
        UNIFORM,
        NON_UNIFORM,
    }
}
