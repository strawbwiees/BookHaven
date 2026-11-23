using System.ComponentModel;
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
        public int BorderRadius { get; set; } = 22;

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public Color CustomBackColor { get; set; } = UITheme.HotPink;

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public Color CustomForeColor { get; set; } = Color.White;

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public Color CustomBorderColor { get; set; } = Color.Transparent;

        private bool _hover;
        private bool _down;

        public PillButton()
        {
            FlatStyle = FlatStyle.Flat;
            FlatAppearance.BorderSize = 0;
            DoubleBuffered = true;
            Cursor = Cursors.Hand;
            Font = new Font("Segoe UI", 10f, FontStyle.Bold);
            ForeColor = Color.White;
            BackColor = Color.Transparent;
            Size = new Size(UITheme.S(130), UITheme.S(42));
        }

        protected override void OnMouseEnter(EventArgs e) { _hover = true; Invalidate(); base.OnMouseEnter(e); }
        protected override void OnMouseLeave(EventArgs e) { _hover = false; _down = false; Invalidate(); base.OnMouseLeave(e); }
        protected override void OnMouseDown(MouseEventArgs mevent) { _down = true; Invalidate(); base.OnMouseDown(mevent); }
        protected override void OnMouseUp(MouseEventArgs mevent) { _down = false; Invalidate(); base.OnMouseUp(mevent); }

        // White buttons get a soft blue tint on hover; coloured buttons get slightly darker.
        private static Color HoverShade(Color c, bool pressed)
        {
            if (c.GetBrightness() > 0.97f)
                return pressed ? Color.FromArgb(222, 236, 250) : Color.FromArgb(240, 247, 254);
            return ControlPaint.Dark(c, pressed ? 0.12f : 0.06f);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;

            var rect = new Rectangle(0, 0, Width - 1, Height - 1);
            int radius = Math.Max(1, Math.Min(BorderRadius, Height / 2));
            using var path = UITheme.GetRoundedRectanglePath(rect, radius);

            Color currentBg;
            if (!Enabled) currentBg = Color.FromArgb(210, 220, 230);
            else if (_down) currentBg = HoverShade(CustomBackColor, true);
            else if (_hover) currentBg = HoverShade(CustomBackColor, false);
            else currentBg = CustomBackColor;

            using var brush = new SolidBrush(currentBg);
            g.FillPath(brush, path);

            if (CustomBorderColor != Color.Transparent)
            {
                using var pen = new Pen(CustomBorderColor, 1.4f);
                g.DrawPath(pen, path);
            }

            TextRenderer.DrawText(g, Text, Font, rect, CustomForeColor,
                TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter |
                TextFormatFlags.NoPrefix | TextFormatFlags.EndEllipsis);
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

            using (var numFont = new Font("Segoe UI", 9.5f, FontStyle.Bold))
            using (var numBrush = new SolidBrush(UITheme.NavyDark))
            {
                var sf = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
                g.DrawString(NavNumber, numFont, numBrush, new RectangleF(circleX, circleY, circleSize, circleSize), sf);
            }

            // Text
            if (!iconOnly)
            {
                int textX = circleX + circleSize + UITheme.S(12);
                using var textFont = new Font("Segoe UI", 9.5f, FontStyle.Bold);
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
        public override string? Text
        {
            get => _textBox.Text;
            set => _textBox.Text = value ?? string.Empty;
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
            Height = UITheme.S(42);

            _textBox = new TextBox
            {
                BorderStyle = BorderStyle.None,
                Font = new Font("Segoe UI", 10f),
                ForeColor = UITheme.TextMain,
                BackColor = Color.White
            };

            _textBox.Enter += (s, e) => { _isFocused = true; Invalidate(); };
            _textBox.Leave += (s, e) => { _isFocused = false; Invalidate(); };
            _textBox.TextChanged += (s, e) => InnerTextChanged?.Invoke(this, e);

            Controls.Add(_textBox);
            Click += (s, e) => _textBox.Focus();
            Resize += (s, e) => LayoutInner();
            LayoutInner();
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
