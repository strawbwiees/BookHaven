using System.Drawing.Drawing2D;

namespace BookHaven.Services
{
    public static class CoverImageGenerator
    {
        public static void EnsureCoversGenerated(string baseDir)
        {
            string coversDir = Path.Combine(baseDir, "Resources", "BookCovers");
            if (!Directory.Exists(coversDir))
            {
                Directory.CreateDirectory(coversDir);
            }

            var covers = new (string filename, string title, string subtitle, string author, string category, string colorHex, string icon)[]
            {
                ("clean_code.png", "CLEAN\nCODE", "A Handbook of Agile Software Craftsmanship", "Robert C. Martin", "Programming", "#1D4ED8", "</>"),
                ("pragmatic_programmer.png", "THE PRAGMATIC\nPROGRAMMER", "Your Journey to Mastery", "David Thomas & Andrew Hunt", "Programming", "#6D28D9", "⚙️"),
                ("intro_algorithms.png", "INTRODUCTION TO\nALGORITHMS", "Fourth Edition", "Thomas H. Cormen et al.", "Computer Science", "#0F172A", "📐"),
                ("database_systems.png", "DATABASE SYSTEM\nCONCEPTS", "Seventh Edition", "Silberschatz • Korth • Sudarshan", "Databases", "#047857", "🗄️"),
                ("noli_me_tangere.png", "NOLI ME\nTANGERE", "Unabridged English Translation", "José Rizal", "Filipiana", "#92400E", "📜"),
                ("calculus.png", "CALCULUS\nEarly Transcendentals", "Metric Version • 9th Edition", "James Stewart", "Mathematics", "#B91C1C", "∫ dx")
            };

            foreach (var c in covers)
            {
                string filePath = Path.Combine(coversDir, c.filename);
                if (!File.Exists(filePath))
                {
                    using var bmp = CreateCoverBitmap(c.title, c.subtitle, c.author, c.category, c.colorHex, c.icon);
                    bmp.Save(filePath, System.Drawing.Imaging.ImageFormat.Png);
                }
            }
        }

        public static Bitmap CreateCoverBitmap(string title, string subtitle, string author, string category, string colorHex, string icon, int width = 160, int height = 230)
        {
            var bmp = new Bitmap(width, height);
            using var g = Graphics.FromImage(bmp);
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.InterpolationMode = InterpolationMode.HighQualityBicubic;
            g.PixelOffsetMode = PixelOffsetMode.HighQuality;

            Color baseColor;
            try { baseColor = ColorTranslator.FromHtml(colorHex); }
            catch { baseColor = Color.FromArgb(30, 64, 175); }

            Color darkColor = ControlPaint.Dark(baseColor, 0.4f);
            Color lightColor = ControlPaint.Light(baseColor, 0.3f);

            var rect = new Rectangle(0, 0, width, height);

            // 1. Rich gradient background
            using (var brush = new LinearGradientBrush(rect, lightColor, darkColor, 55f))
            {
                g.FillRectangle(brush, rect);
            }

            // 2. Book spine 3D shading on left
            using (var spineBrush = new LinearGradientBrush(new Rectangle(0, 0, 14, height), Color.FromArgb(160, Color.Black), Color.FromArgb(10, Color.Black), 0f))
            {
                g.FillRectangle(spineBrush, 0, 0, 14, height);
            }
            using (var highlightPen = new Pen(Color.FromArgb(70, Color.White), 1.5f))
            {
                g.DrawLine(highlightPen, 13, 0, 13, height);
            }

            // 3. Top Category Tag Bar
            using (var barBrush = new SolidBrush(Color.FromArgb(60, Color.Black)))
            {
                g.FillRectangle(barBrush, 18, 14, width - 30, 20);
            }
            using (var barFont = new Font("League Spartan", 7.5f, FontStyle.Bold))
            using (var whiteBrush = new SolidBrush(Color.FromArgb(240, 245, 255)))
            {
                var sf = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
                g.DrawString(category.ToUpper(), barFont, whiteBrush, new RectangleF(18, 14, width - 30, 20), sf);
            }

            // 4. Center Title
            using (var titleFont = new Font("League Spartan", 10.5f, FontStyle.Bold))
            using (var whiteBrush = new SolidBrush(Color.White))
            {
                var sf = new StringFormat
                {
                    Alignment = StringAlignment.Center,
                    LineAlignment = StringAlignment.Center,
                    Trimming = StringTrimming.EllipsisWord
                };
                g.DrawString(title, titleFont, whiteBrush, new RectangleF(18, 42, width - 30, 75), sf);
            }

            // 5. Center Icon / Emblem
            using (var iconFont = new Font("League Spartan Symbol", 16f, FontStyle.Bold))
            using (var iconBrush = new SolidBrush(Color.FromArgb(220, 240, 255)))
            {
                var sf = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
                g.DrawString(icon, iconFont, iconBrush, new RectangleF(18, 118, width - 30, 36), sf);
            }

            // 6. Subtitle
            using (var subFont = new Font("League Spartan", 6.8f, FontStyle.Regular))
            using (var subBrush = new SolidBrush(Color.FromArgb(210, 230, 255)))
            {
                var sf = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
                g.DrawString(subtitle, subFont, subBrush, new RectangleF(18, 154, width - 30, 32), sf);
            }

            // 7. Author at bottom
            using (var authFont = new Font("League Spartan", 8f, FontStyle.Bold))
            using (var authBrush = new SolidBrush(Color.White))
            {
                var sf = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
                g.DrawString(author, authFont, authBrush, new RectangleF(18, 192, width - 30, 26), sf);
            }

            // 8. Subtle Outer Border
            using (var borderPen = new Pen(Color.FromArgb(90, Color.Black), 1.2f))
            {
                g.DrawRectangle(borderPen, 0, 0, width - 1, height - 1);
            }

            return bmp;
        }
    }
}
