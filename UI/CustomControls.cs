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
        public int BorderRadius { get; set; } = 18;

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public Color CustomBackColor { get; set; } = UITheme.HotPink;

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public Color CustomForeColor { get; set; } = Color.White;

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public Color CustomBorderColor { get; set; } = Color.Transparent;

        public PillButton()
        {
            FlatStyle = FlatStyle.Flat;
            FlatAppearance.BorderSize = 0;
            DoubleBuffered = true;
            Cursor = Cursors.Hand;
            Font = new Font("Segoe UI", 9.5f, FontStyle.Bold);
            ForeColor = Color.White;
            BackColor = Color.Transparent;
            Size = new Size(130, 38);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;

            var rect = new Rectangle(0, 0, Width - 1, Height - 1);
            using var path = UITheme.GetRoundedRectanglePath(rect, BorderRadius);

            Color currentBg = Enabled ? CustomBackColor : Color.FromArgb(210, 220, 230);
            using var brush = new SolidBrush(currentBg);
            e.Graphics.FillPath(brush, path);

            if (CustomBorderColor != Color.Transparent)
            {
                using var pen = new Pen(CustomBorderColor, 1.4f);
                e.Graphics.DrawPath(pen, path);
            }

            TextRenderer.DrawText(e.Graphics, Text, Font, rect, CustomForeColor,
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

        public NavPillButton()
        {
            DoubleBuffered = true;
            Cursor = Cursors.Hand;
            Size = new Size(196, 44);
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
            var rect = new Rectangle(0, 0, Width - 1, Height - 1);

            // Container pill
            using (var path = UITheme.GetRoundedRectanglePath(rect, 20))
            {
                if (IsActive)
                {
                    using var activeBrush = new SolidBrush(UITheme.NavyDark);
                    g.FillPath(activeBrush, path);
                }
                else
                {
                    using var inactiveBrush = new SolidBrush(Color.White);
                    g.FillPath(inactiveBrush, path);

                    using var borderPen = new Pen(Color.FromArgb(214, 230, 246), 1.2f);
                    g.DrawPath(borderPen, path);
                }
            }

            // Circle badge on the left
            int circleSize = 28;
            int circleX = 8;
            int circleY = (Height - circleSize) / 2;

            using (var circleBrush = new SolidBrush(UITheme.SkyBlue))
            {
                g.FillEllipse(circleBrush, circleX, circleY, circleSize, circleSize);
            }

            using (var numFont = new Font("Segoe UI", 9.0f, FontStyle.Bold))
            using (var numBrush = new SolidBrush(UITheme.NavyDark))
            {
                var sf = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
                g.DrawString(NavNumber, numFont, numBrush, new RectangleF(circleX, circleY, circleSize, circleSize), sf);
            }

            // Text
            if (Width >= 70)
            {
                using var textFont = new Font("Segoe UI", 9.0f, FontStyle.Bold);
                using var textBrush = new SolidBrush(IsActive ? Color.White : UITheme.NavyDark);
                var sf = new StringFormat
                {
                    Alignment = StringAlignment.Near,
                    LineAlignment = StringAlignment.Center,
                    FormatFlags = StringFormatFlags.NoWrap,
                    Trimming = StringTrimming.EllipsisCharacter
                };
                g.DrawString(NavText, textFont, textBrush, new RectangleF(circleX + circleSize + 8, 0, Width - (circleX + circleSize + 12), Height), sf);
            }
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
            Height = 38;
            Padding = new Padding(12, 9, 12, 9);

            _textBox = new TextBox
            {
                BorderStyle = BorderStyle.None,
                Font = new Font("Segoe UI", 9.5f),
                ForeColor = UITheme.TextMain,
                BackColor = Color.White,
                Dock = DockStyle.Fill
            };

            _textBox.Enter += (s, e) => { _isFocused = true; Invalidate(); };
            _textBox.Leave += (s, e) => { _isFocused = false; Invalidate(); };
            _textBox.TextChanged += (s, e) => InnerTextChanged?.Invoke(this, e);

            Controls.Add(_textBox);
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
