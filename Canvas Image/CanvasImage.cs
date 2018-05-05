using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace temp2
{
    public partial class CanvasImage : Control
    {
        public Image Image
        {
            get { return _img; }
            set
            {
                if (_img == value)
                    return;

                _img = value;
                Invalidate();
            }
        }

        public bool ShowBorder
        {
            get { return _showBorder; }
            set
            {
                if (value == _showBorder)
                    return;

                _showBorder = value;
                Invalidate();
            }
        }

        public Color BorderColor
        {
            get { return _borderColor; }
            set
            {
                if (value == _borderColor)
                    return;

                _borderColor = value;
                Invalidate();
            }
        }

        public float BorderWidth
        {
            get { return _borderWidth; }
            set
            {
                if (value == _borderWidth)
                    return;

                _borderWidth = value;
                Invalidate();
            }
        }

        public float ImageX
        {
            get { return _imgX; }
            set
            {
                if (value == _imgX)
                    return;

                _imgX = value;
                Invalidate();
            }
        }

        public float ImageY
        {
            get { return _imgY; }
            set
            {
                if (value == _imgY)
                    return;

                _imgY = value;
                Invalidate();
            }
        }

        public float ZoomFactor
        {
            get { return _zoomFactor; }
            set
            {
                if (value == _zoomFactor)
                    return;

                _zoomFactor = value;
                Invalidate();
            }
        }

        public MyTable st { get; set; }

        bool _showBorder;
        Image _img;
        float _zoomFactor;
        float _imgX;
        float _imgY;
        float _borderWidth;
        Color _borderColor;

        public CanvasImage()
        {
            InitializeComponent();
            DoubleBuffered = true;
            CheckForIllegalCrossThreadCalls = false;

            BorderColor = Color.Black;
            BackColor = Color.White;
            BorderWidth = 1;
            ZoomFactor = 0;
            ImageX = ImageY = 0;
            ShowBorder = true;
            st = new MyTable(new Size(450,450));
        }

        protected override void OnPaint(PaintEventArgs pe)
        {
            base.OnPaint(pe);

            pe.Graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
            pe.Graphics.SmoothingMode = SmoothingMode.AntiAlias;

            //  DRAW IMAGE
            if (Image != null)
            {
                RectangleF rectF = new RectangleF(ImageX - ZoomFactor, ImageY - ZoomFactor, Width - 1 + (ZoomFactor * 2), Height - 1 + (ZoomFactor * 2));
                pe.Graphics.DrawImage(Image, rectF);
            }


            //  DRAW CANVAS
            int val = 150;
            pe.Graphics.DrawEllipse(new Pen(BackColor, val), new Rectangle(-val / 2, -val / 2, Width + val - 1, Height + val - 1));


            //  DRAW BORDER
            if (ShowBorder)
                pe.Graphics.DrawEllipse(new Pen(BorderColor, BorderWidth), new RectangleF(BorderWidth / 2, BorderWidth / 2, Width - 0.6F - BorderWidth, Height - 0.6F - BorderWidth));
        }
    }
}
