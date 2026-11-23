using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Drawing.Drawing2D;

namespace BookHaven.UI
{
    public class CardPanel : Panel
    {
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public int BorderRadius { get; set; } = 16;

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public Color BorderColor { get; set; } = UITheme.CardBorder;

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public Color BackgroundFill { get; set; } = Color.White;

        public CardPanel()
        {
            DoubleBuffered = true;
            BackColor = Color.Transparent;
            Padding = new Padding(14);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;

            var rect = new Rectangle(0, 0, Width - 1, Height - 1);
            using var path = UITheme.GetRoundedRectanglePath(rect, BorderRadius);

            using var brush = new SolidBrush(BackgroundFill);
            e.Graphics.FillPath(brush, path);

            using var pen = new Pen(BorderColor, 1.2f);
            e.Graphics.DrawPath(pen, path);
        }
    }

    public class PillButton : Button
    {
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public int BorderRadius { get; set; } = 12;

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public Color CustomBackColor { get; set; } = UITheme.HotPink;

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public Color CustomForeColor { get; set; } = Color.White;

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public Color CustomBorderColor { get; set; } = Color.Transparent;

        private bool _hover;
        private bool _pressed;

        public PillButton()
        {
            SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint |
                     ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);
            FlatStyle = FlatStyle.Flat;
            FlatAppearance.BorderSize = 0;
            Cursor = Cursors.Hand;
            Font = new Font("League Spartan", 9.5f, FontStyle.Bold);
            ForeColor = Color.White;
            Size = new Size(130, 40);
        }

        // Prevent WinForms from painting the default rectangular button background
        protected override void OnPaintBackground(PaintEventArgs pevent) { }

        protected override void OnMouseEnter(EventArgs e)
        {
            base.OnMouseEnter(e);
            _hover = true;
            Invalidate();
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            base.OnMouseLeave(e);
            _hover = false;
            _pressed = false;
            Invalidate();
        }

        protected override void OnMouseDown(MouseEventArgs mevent)
        {
            base.OnMouseDown(mevent);
            _pressed = true;
            Invalidate();
        }

        protected override void OnMouseUp(MouseEventArgs mevent)
        {
            base.OnMouseUp(mevent);
            _pressed = false;
            Invalidate();
        }

        // Walk up the parent chain to find the actual visible background color
        private Color GetEffectiveParentBackColor()
        {
            for (Control? p = Parent; p != null; p = p.Parent)
            {
                if (p is CardPanel cp)
                    return cp.BackgroundFill;
                if (p.BackColor != Color.Transparent && p.BackColor.A == 255)
                    return p.BackColor;
            }
            return UITheme.AppBackground;
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;

            // Fill corners with the true parent background color
            using (var clearBrush = new SolidBrush(GetEffectiveParentBackColor()))
            {
                g.FillRectangle(clearBrush, ClientRectangle);
            }

            var rect = new Rectangle(1, 1, Width - 3, Height - 3);
            int radius = Math.Min(BorderRadius, rect.Height / 2);
            using var path = UITheme.GetRoundedRectanglePath(rect, radius);

            Color currentBg;
            if (!Enabled)
            {
                currentBg = Color.FromArgb(215, 224, 234);
            }
            else if (_pressed)
            {
                currentBg = ControlPaint.Dark(CustomBackColor, 0.08f);
            }
            else if (_hover)
            {
                currentBg = CustomBackColor == Color.White
                    ? Color.FromArgb(242, 248, 255)
                    : ControlPaint.Light(CustomBackColor, 0.08f);
            }
            else
            {
                currentBg = CustomBackColor;
            }

            using (var brush = new SolidBrush(currentBg))
            {
                g.FillPath(brush, path);
            }

            if (CustomBorderColor != Color.Transparent)
            {
                using var pen = new Pen(CustomBorderColor, 1.3f);
                g.DrawPath(pen, path);
            }

            Color textColor = Enabled ? CustomForeColor : Color.FromArgb(130, 145, 165);
            TextRenderer.DrawText(g, Text, Font, rect, textColor,
                TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPrefix);
        }
    }

    public class NavPillButton : Control
    {
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public string NavNumber { get; set; } = "1";

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public string NavText { get; set; } = "Home";

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool IsActive { get; set; } = false;

        public event EventHandler? Selected;

        private bool _hover;

        public NavPillButton()
        {
            DoubleBuffered = true;
            ResizeRedraw = true;
            Cursor = Cursors.Hand;
            Size = new Size(196, 44);
        }

        protected override void OnMouseEnter(EventArgs e)
        {
            base.OnMouseEnter(e);
            _hover = true;
            Invalidate();
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            base.OnMouseLeave(e);
            _hover = false;
            Invalidate();
        }

        protected override void OnClick(EventArgs e)
        {
            base.OnClick(e);
            Selected?.Invoke(this, EventArgs.Empty);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;
            var rect = new Rectangle(0, 0, Width - 1, Height - 1);

            // Container pill
            using (var path = UITheme.GetRoundedRectanglePath(rect, Math.Max(1, Math.Min(UITheme.S(24), Height / 2))))
            {
                if (IsActive)
                {
                    using var activeBrush = new SolidBrush(UITheme.NavyDark);
                    g.FillPath(activeBrush, path);
                }
                else
                {
                    using var inactiveBrush = new SolidBrush(_hover ? Color.FromArgb(240, 248, 255) : Color.White);
                    g.FillPath(inactiveBrush, path);

                    using var borderPen = new Pen(Color.FromArgb(214, 230, 246), 1.2f);
                    g.DrawPath(borderPen, path);
                }
            }

            bool iconOnly = Width < UITheme.S(70);

            // Circle badge on the left
            int circleSize = Math.Min(UITheme.S(30), Height - UITheme.S(14));
            int circleX = iconOnly ? (Width - circleSize) / 2 : UITheme.S(10);
            int circleY = (Height - circleSize) / 2;

            using (var circleBrush = new SolidBrush(UITheme.SkyBlue))
            {
                g.FillEllipse(circleBrush, circleX, circleY, circleSize, circleSize);
            }

            using (var numFont = new Font("League Spartan", 9.5f, FontStyle.Bold))
            using (var numBrush = new SolidBrush(UITheme.NavyDark))
            {
                var sf = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
                g.DrawString(NavNumber, numFont, numBrush, new RectangleF(circleX, circleY, circleSize, circleSize), sf);
            }

            // Text
            if (!iconOnly)
            {
                int textX = circleX + circleSize + UITheme.S(12);
                using var textFont = new Font("League Spartan", 9.5f, FontStyle.Bold);
                using var textBrush = new SolidBrush(IsActive ? Color.White : UITheme.NavyDark);
                var sf = new StringFormat
                {
                    Alignment = StringAlignment.Near,
                    LineAlignment = StringAlignment.Center,
                    FormatFlags = StringFormatFlags.NoWrap,
                    Trimming = StringTrimming.EllipsisCharacter
                };
                g.DrawString(NavText, textFont, textBrush, new RectangleF(textX, 0, Width - textX - UITheme.S(10), Height), sf);
            }
        }
    }

    public class BufferedPanel : Panel
    {
        public BufferedPanel()
        {
            DoubleBuffered = true;
            ResizeRedraw = true;
        }
    }

    public class RoundedTextBox : Panel
    {
        private readonly TextBox _textBox;

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [AllowNull]
        public override string Text
        {
            get => _textBox.Text;
            set => _textBox.Text = value;
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public TextBox InnerTextBox => _textBox;

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool UseSystemPasswordChar
        {
            get => _textBox.UseSystemPasswordChar;
            set => _textBox.UseSystemPasswordChar = value;
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public int BorderRadius { get; set; } = 12;

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public Color BorderColor { get; set; } = Color.FromArgb(205, 225, 245);

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public Color FocusBorderColor { get; set; } = UITheme.SkyBlue;

        private bool _isFocused = false;

        public event EventHandler? InnerTextChanged;

        public RoundedTextBox()
        {
            DoubleBuffered = true;
            BackColor = Color.White;
            Height = 40; // slightly taller for breathing room

            _textBox = new TextBox
            {
                BorderStyle = BorderStyle.None,
                Font = new Font("Segoe UI", 9.5f), // <-- Change input font size here
                ForeColor = UITheme.TextMain,
                BackColor = Color.White
            };

            _textBox.Enter += (s, e) => { _isFocused = true; Invalidate(); };
            _textBox.Leave += (s, e) => { _isFocused = false; Invalidate(); };
            _textBox.TextChanged += (s, e) => InnerTextChanged?.Invoke(this, e);

            Controls.Add(_textBox);
            CenterInnerTextBox();
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            CenterInnerTextBox();
        }

        private void CenterInnerTextBox()
        {
            if (_textBox == null) return;
            int padX = 12;
            int textH = _textBox.PreferredHeight;
            _textBox.Location = new Point(padX, Math.Max(4, (Height - textH) / 2));
            _textBox.Width = Math.Max(10, Width - padX * 2);
        }

        // The text box is centred vertically and the control never gets shorter than its text,
        // so the typed text can no longer be clipped at higher display scaling.
        private void LayoutInner()
        {
            int padX = UITheme.S(14);
            int minHeight = _textBox.PreferredHeight + UITheme.S(16);
            if (Height < minHeight)
            {
                Height = minHeight; // raises Resize again, which lays out below
                return;
            }

            _textBox.Width = Math.Max(10, Width - padX * 2);
            _textBox.Location = new Point(padX, (Height - _textBox.Height) / 2);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;

            var rect = new Rectangle(0, 0, Width - 1, Height - 1);
            using var path = UITheme.GetRoundedRectanglePath(rect, BorderRadius);

            using var bgBrush = new SolidBrush(BackColor);
            e.Graphics.FillPath(bgBrush, path);

            Color currentBorder = _isFocused ? FocusBorderColor : BorderColor;
            float borderWidth = _isFocused ? 1.6f : 1.2f;
            using var pen = new Pen(currentBorder, borderWidth);
            e.Graphics.DrawPath(pen, path);
        }
    }
}
