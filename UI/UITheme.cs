using System.Drawing.Drawing2D;
using BookHaven.Models;

namespace BookHaven.UI
{
    public static class UITheme
    {
        // Colors from PDF mockups
        public static readonly Color AppBackground = Color.FromArgb(234, 244, 253);       // Soft baby blue (#EAF4FD)
        public static readonly Color SidebarBackground = Color.FromArgb(226, 240, 253);   // Light blue sidebar (#E2F0FD)
        public static readonly Color BannerBackground = Color.FromArgb(17, 30, 56);       // Deep gradient navy (#111E38)
        public static readonly Color CardBackground = Color.White;
        public static readonly Color CardBorder = Color.FromArgb(222, 235, 248);

        public static readonly Color NavyDark = Color.FromArgb(16, 32, 64);               // #102040
        public static readonly Color HotPink = Color.FromArgb(255, 77, 148);             // #FF4D94
        public static readonly Color SkyBlue = Color.FromArgb(114, 196, 254);            // #72C4FE
        public static readonly Color SkyBlueLight = Color.FromArgb(219, 239, 254);        // #DBEFFE
        public static readonly Color TextMain = Color.FromArgb(16, 24, 40);
        public static readonly Color TextMuted = Color.FromArgb(107, 124, 150);

        // Badge / Status colors
        public static readonly Color StatusGreen = Color.FromArgb(34, 197, 94);
        public static readonly Color StatusGreenBg = Color.FromArgb(220, 252, 231);
        public static readonly Color StatusRed = Color.FromArgb(239, 68, 68);
        public static readonly Color StatusRedBg = Color.FromArgb(254, 226, 226);
        public static readonly Color StatusYellow = Color.FromArgb(245, 158, 11);
        public static readonly Color StatusYellowBg = Color.FromArgb(254, 243, 199);
        public static readonly Color StatusBlue = Color.FromArgb(2, 132, 199);
        public static readonly Color StatusBlueBg = Color.FromArgb(224, 242, 254);

        // Fonts
        public static readonly Font HeaderFont = new("League Spartan", 18f, FontStyle.Bold);
        public static readonly Font SubHeaderFont = new("League Spartan", 12.5f, FontStyle.Bold);
        public static readonly Font CardTitleFont = new("League Spartan", 10.5f, FontStyle.Bold);
        public static readonly Font BodyFont = new("League Spartan", 9.0f, FontStyle.Regular);
        public static readonly Font BodyBold = new("League Spartan", 9.0f, FontStyle.Bold);
        public static readonly Font SmallFont = new("League Spartan", 8.2f, FontStyle.Regular);
        public static readonly Font BigNumberFont = new("League Spartan", 22f, FontStyle.Bold);

        // ---- DPI helpers: keep spacing proportional to text when Windows scaling is above 100% ----
        private static float _dpiScale = -1f;

        public static float DpiScale
        {
            get
            {
                if (_dpiScale < 0)
                {
                    using var g = Graphics.FromHwnd(IntPtr.Zero);
                    _dpiScale = Math.Max(1f, g.DpiX / 96f);
                }
                return _dpiScale;
            }
        }

        /// <summary>Scales a design-time pixel value to the current display scaling.</summary>
        public static int S(int px) => (int)Math.Round(px * DpiScale);

        // ---- Shared brand banner (used by the main window and the login screen) ----
        private static Image? _shieldLogo;
        private static bool _shieldLogoTried;

        private static Image? GetShieldLogo()
        {
            if (_shieldLogoTried) return _shieldLogo;
            _shieldLogoTried = true;
            try
            {
                string path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources", "logo_shield.png");
                if (!File.Exists(path))
                    path = Path.Combine(Application.StartupPath, "..", "..", "..", "Resources", "logo_shield.png");
                if (File.Exists(path))
                {
                    using var fromFile = Image.FromFile(path);
                    _shieldLogo = new Bitmap(fromFile); // copy so the file is not locked
                }
            }
            catch { }
            return _shieldLogo;
        }

        public static void DrawBrandBanner(Graphics g, Rectangle bounds, bool compact, int sideInset = 0)
        {
            if (bounds.Width <= 0 || bounds.Height <= 0) return;

            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;

            using (var brush = new LinearGradientBrush(bounds, Color.FromArgb(17, 30, 56), Color.FromArgb(32, 54, 96), 45f))
                g.FillRectangle(brush, bounds);

            int w = bounds.Width, h = bounds.Height;
            using var titleFont = new Font("League Spartan", compact ? 15f : 20f, FontStyle.Bold);
            using var subFont = new Font("League Spartan", 8f, FontStyle.Bold);
            using var whiteBrush = new SolidBrush(Color.White);
            using var subBrush = new SolidBrush(Color.FromArgb(160, 195, 240));
            using var center = new StringFormat
            {
                Alignment = StringAlignment.Center,
                LineAlignment = StringAlignment.Center,
                FormatFlags = StringFormatFlags.NoWrap
            };

            if (compact)
            {
                g.DrawString("BOOKHAVEN", titleFont, whiteBrush,
                    new RectangleF(bounds.X + sideInset, bounds.Y, w - sideInset * 2, h), center);
                return;
            }

            const string title = "BOOKHAVEN";
            const string sub = "LIBRARY MANAGEMENT SYSTEM";
            SizeF tSize = g.MeasureString(title, titleFont);
            SizeF sSize = g.MeasureString(sub, subFont);

            int icon = S(52), gap = S(18);
            int textW = (int)Math.Ceiling(Math.Max(tSize.Width, sSize.Width));
            int groupW = icon + gap + textW + gap + icon;
            int x0 = bounds.X + (w - groupW) / 2;
            int cy = bounds.Y + h / 2;

            // Book badge on the left
            var badge = new Rectangle(x0, cy - icon / 2, icon, icon);
            using (var yellow = new SolidBrush(Color.FromArgb(254, 218, 56)))
                g.FillEllipse(yellow, badge);
            using (var ink = new Pen(Color.FromArgb(24, 38, 68), 2.5f))
                g.DrawEllipse(ink, badge);
            using (var iconFont = new Font("League Spartan", 18f))
            using (var inkBrush = new SolidBrush(Color.FromArgb(24, 38, 68)))
                g.DrawString("\U0001F4D6", iconFont, inkBrush, new RectangleF(badge.X, badge.Y - 1, badge.Width, badge.Height), center);

            // Title block
            float textX = x0 + icon + gap;
            float blockH = sSize.Height + tSize.Height - S(4);
            float top = cy - blockH / 2f;
            g.DrawString(sub, subFont, subBrush, new RectangleF(textX, top, textW, sSize.Height), center);
            g.DrawString(title, titleFont, whiteBrush, new RectangleF(textX, top + sSize.Height - S(4), textW, tSize.Height), center);

            // Shield on the right
            var logo = GetShieldLogo();
            if (logo != null)
            {
                int lw = (int)(icon * 48f / 52f);
                g.DrawImage(logo, (int)(textX + textW + gap), cy - icon / 2, lw, icon);
            }
        }

        public static GraphicsPath GetRoundedRectanglePath(Rectangle bounds, int radius)
        {
            var path = new GraphicsPath();
            if (radius <= 0)
            {
                path.AddRectangle(bounds);
                return path;
            }

            int diameter = radius * 2;
            var arc = new Rectangle(bounds.Location, new Size(diameter, diameter));

            // top left arc
            path.AddArc(arc, 180, 90);

            // top right arc
            arc.X = bounds.Right - diameter;
            path.AddArc(arc, 270, 90);

            // bottom right arc
            arc.Y = bounds.Bottom - diameter;
            path.AddArc(arc, 0, 90);

            // bottom left arc
            arc.X = bounds.Left;
            path.AddArc(arc, 90, 90);

            path.CloseFigure();
            return path;
        }

        public static void DrawSmiley(Graphics g, Color color, int x, int y, int size)
        {
            var oldMode = g.SmoothingMode;
            g.SmoothingMode = SmoothingMode.AntiAlias;

            using var face = new SolidBrush(color);
            g.FillEllipse(face, x, y, size, size);

            var ink = Color.FromArgb(30, 40, 60);
            using var inkBrush = new SolidBrush(ink);
            int eye = Math.Max(2, size / 9);
            g.FillEllipse(inkBrush, x + (int)(size * 0.28f), y + (int)(size * 0.32f), eye, eye);
            g.FillEllipse(inkBrush, x + (int)(size * 0.62f), y + (int)(size * 0.32f), eye, eye);

            using var pen = new Pen(ink, Math.Max(1.2f, size / 14f)) { StartCap = LineCap.Round, EndCap = LineCap.Round };
            g.DrawArc(pen, x + size * 0.25f, y + size * 0.35f, size * 0.5f, size * 0.35f, 20f, 140f);

            g.SmoothingMode = oldMode;
        }

        public static void DrawLibrarianAvatar(Graphics g, Rectangle bounds)
        {
            g.SmoothingMode = SmoothingMode.AntiAlias;

            // Background circle
            using (var bgBrush = new SolidBrush(Color.FromArgb(179, 224, 255))) // Soft sky blue circle
            {
                g.FillEllipse(bgBrush, bounds);
            }

            // Calculations based on bounds
            float cx = bounds.X + bounds.Width / 2f;
            float cy = bounds.Y + bounds.Height / 2f;
            float r = bounds.Width / 2f;

            // Clip to circle
            var oldClip = g.Clip;
            using (var clipPath = new GraphicsPath())
            {
                clipPath.AddEllipse(bounds);
                g.SetClip(clipPath, CombineMode.Intersect);

                // 1. Shirt (pink/magenta collar)
                using (var shirtBrush = new SolidBrush(Color.FromArgb(209, 88, 222)))
                {
                    g.FillEllipse(shirtBrush, cx - r * 0.75f, cy + r * 0.35f, r * 1.5f, r * 1.2f);
                }

                // 2. Hair back (Dark warm brown)
                Color hairColor = Color.FromArgb(120, 65, 55);
                using (var hairBrush = new SolidBrush(hairColor))
                {
                    g.FillEllipse(hairBrush, cx - r * 0.62f, cy - r * 0.65f, r * 1.24f, r * 1.15f);
                    // Side locks
                    g.FillEllipse(hairBrush, cx - r * 0.68f, cy - r * 0.2f, r * 0.42f, r * 0.8f);
                    g.FillEllipse(hairBrush, cx + r * 0.26f, cy - r * 0.2f, r * 0.42f, r * 0.8f);
                }

                // 3. Neck & Face (Peach skin tone)
                using (var skinBrush = new SolidBrush(Color.FromArgb(254, 203, 175)))
                {
                    // Neck
                    g.FillRectangle(skinBrush, cx - r * 0.2f, cy + r * 0.15f, r * 0.4f, r * 0.35f);
                    // Face
                    g.FillEllipse(skinBrush, cx - r * 0.44f, cy - r * 0.45f, r * 0.88f, r * 0.9f);
                }

                // 4. Hair fringe / bangs
                using (var hairBrush = new SolidBrush(hairColor))
                {
                    g.FillEllipse(hairBrush, cx - r * 0.48f, cy - r * 0.68f, r * 0.96f, r * 0.46f);
                }

                g.Clip = oldClip;
            }
        }

        public static Bitmap GetBookCoverImage(Book book, int width = 110, int height = 150)
        {
            try
            {
                string? imgPath = book.CoverImagePath;
                if (string.IsNullOrEmpty(imgPath))
                {
                    string localDir = AppDomain.CurrentDomain.BaseDirectory;
                    string defaultPath = Path.Combine(localDir, "Resources", "BookCovers", GetDefaultCoverFileName(book.Title));
                    if (File.Exists(defaultPath)) imgPath = defaultPath;
                    else
                    {
                        string projPath = Path.Combine(Application.StartupPath, "..", "..", "..", "Resources", "BookCovers", GetDefaultCoverFileName(book.Title));
                        if (File.Exists(projPath)) imgPath = projPath;
                    }
                }

                if (!string.IsNullOrEmpty(imgPath) && File.Exists(imgPath))
                {
                    using var original = Image.FromFile(imgPath);
                    var resized = new Bitmap(width, height);
                    using var g = Graphics.FromImage(resized);
                    g.InterpolationMode = InterpolationMode.HighQualityBicubic;
                    g.SmoothingMode = SmoothingMode.AntiAlias;
                    g.PixelOffsetMode = PixelOffsetMode.HighQuality;
                    g.DrawImage(original, 0, 0, width, height);
                    return resized;
                }
            }
            catch { }

            return GenerateBookCover(book, width, height);
        }

        private static string GetDefaultCoverFileName(string title)
        {
            string t = title.ToLower();
            if (t.Contains("clean code")) return "clean_code.png";
            if (t.Contains("pragmatic")) return "pragmatic_programmer.png";
            if (t.Contains("algorithm")) return "intro_algorithms.png";
            if (t.Contains("database")) return "database_systems.png";
            if (t.Contains("noli")) return "noli_me_tangere.png";
            if (t.Contains("calculus")) return "calculus.png";
            return "clean_code.png";
        }

        public static Bitmap GenerateBookCover(Book book, int width = 110, int height = 150)
        {
            var bmp = new Bitmap(width, height);
            using var g = Graphics.FromImage(bmp);
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.InterpolationMode = InterpolationMode.HighQualityBicubic;
            g.PixelOffsetMode = PixelOffsetMode.HighQuality;

            Color baseColor;
            try { baseColor = ColorTranslator.FromHtml(book.CoverColorHex); }
            catch { baseColor = Color.FromArgb(37, 99, 235); }

            Color lighterColor = ControlPaint.Light(baseColor, 0.4f);
            Color darkerColor = ControlPaint.Dark(baseColor, 0.3f);

            var fullRect = new Rectangle(0, 0, width, height);

            using (var path = GetRoundedRectanglePath(fullRect, 6))
            {
                using var brush = new LinearGradientBrush(fullRect, lighterColor, darkerColor, 45f);
                g.FillPath(brush, path);

                using var spinePen = new Pen(Color.FromArgb(80, Color.Black), 3f);
                g.DrawLine(spinePen, 7, 3, 7, height - 3);

                using var highlightPen = new Pen(Color.FromArgb(60, Color.White), 1.5f);
                g.DrawLine(highlightPen, 9, 3, 9, height - 3);

                using var borderPen = new Pen(Color.FromArgb(80, darkerColor), 1f);
                g.DrawPath(borderPen, path);
            }

            using (var catBrush = new SolidBrush(Color.FromArgb(180, Color.Black)))
            {
                g.FillRectangle(catBrush, 10, 8, width - 20, 18);
            }
            using (var catFont = new Font("League Spartan", 6.8f, FontStyle.Bold))
            using (var whiteBrush = new SolidBrush(Color.White))
            {
                var sf = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
                g.DrawString(book.Category.ToUpper(), catFont, whiteBrush, new RectangleF(10, 8, width - 20, 18), sf);
            }

            using (var titleFont = new Font("League Spartan", 8.5f, FontStyle.Bold))
            using (var whiteBrush = new SolidBrush(Color.White))
            {
                var sf = new StringFormat
                {
                    Alignment = StringAlignment.Center,
                    LineAlignment = StringAlignment.Center,
                    Trimming = StringTrimming.EllipsisWord
                };
                g.DrawString(book.Title, titleFont, whiteBrush, new RectangleF(10, 32, width - 20, 58), sf);
            }

            using (var authorFont = new Font("League Spartan", 7.5f, FontStyle.Italic))
            using (var authorBrush = new SolidBrush(Color.FromArgb(230, 240, 255)))
            {
                var sf = new StringFormat
                {
                    Alignment = StringAlignment.Center,
                    LineAlignment = StringAlignment.Far,
                    Trimming = StringTrimming.EllipsisWord
                };
                g.DrawString(book.Author, authorFont, authorBrush, new RectangleF(10, 100, width - 20, 42), sf);
            }

            return bmp;
        }

        public static Bitmap GenerateAvatar(string initials, int size = 44, Color? bg = null)
        {
            var bmp = new Bitmap(size, size);
            using var g = Graphics.FromImage(bmp);
            g.SmoothingMode = SmoothingMode.AntiAlias;

            Color bgColor = bg ?? SkyBlue;
            using (var brush = new SolidBrush(bgColor))
            {
                g.FillEllipse(brush, 0, 0, size, size);
            }

            using var textBrush = new SolidBrush(NavyDark);
            using var font = new Font("League Spartan", size * 0.32f, FontStyle.Bold);
            var sf = new StringFormat
            {
                Alignment = StringAlignment.Center,
                LineAlignment = StringAlignment.Center,
                FormatFlags = StringFormatFlags.NoWrap
            };
            g.DrawString(initials, font, textBrush, new RectangleF(0, 0, size, size), sf);

            return bmp;
        }
    }
}
