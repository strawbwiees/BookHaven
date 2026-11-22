using System.Drawing.Drawing2D;
using BookHaven.Services;
using BookHaven.UI;
using BookHaven.Views;

namespace BookHaven.Forms
{
    public class MainForm : Form
    {
        private Panel _sidebarPanel = null!;
        private Panel _rightContainer = null!;
        private Panel _bannerPanel = null!;
        private Panel _contentContainer = null!;
        private Button _btnToggleSidebar = null!;

        // Views
        private DashboardView _dashboardView = null!;
        private BooksView _booksView = null!;
        private IssueReturnView _issueReturnView = null!;
        private BorrowedBooksView _borrowedBooksView = null!;
        private BorrowersView _borrowersView = null!;
        private SettingsView _settingsView = null!;

        // Nav buttons
        private readonly List<NavPillButton> _navButtons = new();
        private readonly LibraryDataService _service = LibraryDataService.Instance;

        private bool _isSidebarCollapsed = false;
        private const int ExpandedSidebarWidth = 240;
        private const int CollapsedSidebarWidth = 64;

        private PictureBox _avatarBox = null!;
        private Label _lblAdmin = null!;

        public MainForm()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            Text = "BookHaven - Library Management System";
            Size = new Size(1300, 860);
            MinimumSize = new Size(500, 560);
            StartPosition = FormStartPosition.CenterScreen;
            BackColor = UITheme.AppBackground;

            // 1. Right Container (Holds Top Banner + Views)
            _rightContainer = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = UITheme.AppBackground
            };

            // 2. Left Sidebar (Docked Left)
            _sidebarPanel = new Panel
            {
                Dock = DockStyle.Left,
                Width = ExpandedSidebarWidth,
                BackColor = UITheme.SidebarBackground
            };
            BuildSidebar();

            // Docking order: Right container first, then Sidebar docked left
            Controls.Add(_rightContainer);
            Controls.Add(_sidebarPanel);

            // 3. Inside Right Container: Content container first, then Banner docked top
            _contentContainer = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = UITheme.AppBackground
            };

            _bannerPanel = new Panel
            {
                Dock = DockStyle.Top,
                Height = 88,
                BackColor = UITheme.BannerBackground
            };
            _bannerPanel.Paint += PaintBanner;

            // Sidebar toggle button (for mobile/tablet only)
            _btnToggleSidebar = new Button
            {
                Text = "☰",
                Font = new Font("Segoe UI", 12f, FontStyle.Bold),
                ForeColor = Color.White,
                BackColor = Color.FromArgb(32, 44, 68),
                FlatStyle = FlatStyle.Flat,
                Size = new Size(34, 34),
                Location = new Point(12, 26),
                Cursor = Cursors.Hand,
                Visible = false
            };
            _btnToggleSidebar.FlatAppearance.BorderSize = 0;
            _btnToggleSidebar.Click += (s, e) => ToggleSidebar();
            _bannerPanel.Controls.Add(_btnToggleSidebar);

            _rightContainer.Controls.Add(_contentContainer);
            _rightContainer.Controls.Add(_bannerPanel);

            // Instantiate Views
            _dashboardView = new DashboardView();
            _dashboardView.NavigateRequested += HandleNavigationRequest;

            _booksView = new BooksView();
            _booksView.NavigateRequested += HandleNavigationRequest;

            _issueReturnView = new IssueReturnView();
            _borrowedBooksView = new BorrowedBooksView();
            _borrowersView = new BorrowersView();
            _settingsView = new SettingsView();

            Resize += MainForm_Resize;

            // Start on Dashboard
            NavigateTo("Home");
            CheckResponsiveMode();
        }

        private void MainForm_Resize(object? sender, EventArgs e)
        {
            CheckResponsiveMode();
            _bannerPanel.Invalidate();
        }

        private void CheckResponsiveMode()
        {
            if (ClientSize.Width < 820 && !_isSidebarCollapsed)
            {
                CollapseSidebar(true);
            }
            else if (ClientSize.Width >= 960 && _isSidebarCollapsed)
            {
                CollapseSidebar(false);
            }
        }

        private void ToggleSidebar()
        {
            CollapseSidebar(!_isSidebarCollapsed);
        }

        private void CollapseSidebar(bool collapse)
        {
            _isSidebarCollapsed = collapse;
            _sidebarPanel.Width = collapse ? CollapsedSidebarWidth : ExpandedSidebarWidth;

            _avatarBox.Visible = !collapse;
            _lblAdmin.Visible = !collapse;
            _btnToggleSidebar.Visible = collapse || ClientSize.Width < 820;

            foreach (var btn in _navButtons)
            {
                btn.Width = collapse ? 46 : 216;
                btn.Invalidate();
            }
        }

        private void BuildSidebar()
        {
            // Admin Avatar matching PDF Page 8
            _avatarBox = new PictureBox
            {
                Size = new Size(68, 68),
                Location = new Point((ExpandedSidebarWidth - 68) / 2, 20),
                Image = DrawAdminAvatar(68)
            };
            _sidebarPanel.Controls.Add(_avatarBox);

            _lblAdmin = new Label
            {
                Text = "ADMIN",
                Font = new Font("Segoe UI", 13f, FontStyle.Bold),
                ForeColor = UITheme.NavyDark,
                TextAlign = ContentAlignment.MiddleCenter,
                Location = new Point(0, 92),
                Size = new Size(ExpandedSidebarWidth, 26)
            };
            _sidebarPanel.Controls.Add(_lblAdmin);

            // Navigation Pills matching PDF
            var navItems = new (string key, string num, string text)[]
            {
                ("Home", "H", "Home"),
                ("Books", "1", "Books"),
                ("Issue/Return Book", "2", "Issue/Return Book"),
                ("Borrowed Books", "3", "Borrowed Books"),
                ("Borrowers", "4", "Borrowers"),
                ("Settings", "5", "Settings"),
                ("Logout", "×", "Log out")
            };

            int btnY = 130;
            foreach (var (key, num, text) in navItems)
            {
                var btn = new NavPillButton
                {
                    NavNumber = num,
                    NavText = text,
                    Tag = key,
                    Location = new Point(12, btnY),
                    Size = new Size(216, 44)
                };

                btn.Selected += (s, e) =>
                {
                    if (key == "Logout")
                    {
                        var confirm = MessageBox.Show("Are you sure you want to log out?", "Confirm Log Out", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                        if (confirm == DialogResult.Yes)
                        {
                            Hide();
                            var login = new LoginForm();
                            login.FormClosed += (sender, args) => Close();
                            login.Show();
                        }
                    }
                    else
                    {
                        NavigateTo(key);
                        if (ClientSize.Width < 820) CollapseSidebar(true);
                    }
                };

                _navButtons.Add(btn);
                _sidebarPanel.Controls.Add(btn);
                btnY += 50;
            }
        }

        private static Bitmap DrawAdminAvatar(int size)
        {
            var bmp = new Bitmap(size, size);
            using var g = Graphics.FromImage(bmp);
            UITheme.DrawLibrarianAvatar(g, new Rectangle(0, 0, size, size));
            return bmp;
        }

        public void NavigateTo(string viewKey, object? argument = null)
        {
            foreach (var b in _navButtons)
            {
                b.IsActive = (b.Tag?.ToString() == viewKey);
                b.Invalidate();
            }

            _contentContainer.Controls.Clear();

            switch (viewKey)
            {
                case "Home":
                    _dashboardView.RefreshData();
                    _contentContainer.Controls.Add(_dashboardView);
                    break;
                case "Books":
                    _booksView.FilterAndRenderBooks();
                    _contentContainer.Controls.Add(_booksView);
                    break;
                case "Issue/Return Book":
                    if (argument is string isbn) _issueReturnView.PreloadIsbn(isbn);
                    _contentContainer.Controls.Add(_issueReturnView);
                    break;
                case "Borrowed Books":
                    _borrowedBooksView.LoadTransactionsData();
                    _borrowedBooksView.LoadReservations();
                    _contentContainer.Controls.Add(_borrowedBooksView);
                    break;
                case "Borrowers":
                    _borrowersView.RenderBorrowerCards();
                    _contentContainer.Controls.Add(_borrowersView);
                    break;
                case "Settings":
                    _contentContainer.Controls.Add(_settingsView);
                    break;
            }
        }

        private void HandleNavigationRequest(object? sender, string target)
        {
            if (target.StartsWith("Issue:"))
            {
                string isbn = target.Substring("Issue:".Length);
                NavigateTo("Issue/Return Book", isbn);
            }
            else if (target == "Books-Add")
            {
                NavigateTo("Books");
                _booksView.SwitchTab("Add");
            }
            else
            {
                NavigateTo(target);
            }
        }

        private void PaintBanner(object? sender, PaintEventArgs e)
        {
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;

            int w = _bannerPanel.Width;
            int h = _bannerPanel.Height;

            var rect = new Rectangle(0, 0, w, h);
            using (var brush = new LinearGradientBrush(rect, Color.FromArgb(17, 30, 56), Color.FromArgb(32, 54, 96), 45f))
            {
                g.FillRectangle(brush, rect);
            }

            // Subtle concentric decorative rings
            using (var ringPen = new Pen(Color.FromArgb(20, 255, 255, 255), 1.2f))
            {
                int cx = w / 2;
                int cy = h / 2;
                g.DrawEllipse(ringPen, cx - 180, cy - 180, 360, 360);
                g.DrawEllipse(ringPen, cx - 260, cy - 260, 520, 520);
                g.DrawEllipse(ringPen, cx - 340, cy - 340, 680, 680);
            }

            int centerX = w / 2;
            bool isNarrow = w < 680;

            if (isNarrow)
            {
                using var titleFont = new Font("Segoe UI", 16f, FontStyle.Bold);
                using var titleBrush = new SolidBrush(Color.White);
                var sf = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
                g.DrawString("BOOKHAVEN", titleFont, titleBrush, new RectangleF(48, 14, w - 56, 30), sf);

                using var subFont = new Font("Segoe UI", 8f, FontStyle.Bold);
                using var subBrush = new SolidBrush(UITheme.SkyBlue);
                g.DrawString("C# • WINDOWS FORMS • OOP", subFont, subBrush, new RectangleF(48, 48, w - 56, 20), sf);
                return;
            }

            // Desktop layout (matches PDF Page 9)
            // 1. Golden Yellow book circle
            int circleX = centerX - 190;
            int circleY = (h - 52) / 2;
            int circleSize = 52;
            using (var yellowBrush = new SolidBrush(Color.FromArgb(254, 218, 56)))
            {
                g.FillEllipse(yellowBrush, circleX, circleY, circleSize, circleSize);
            }
            using (var darkPen = new Pen(Color.FromArgb(24, 38, 68), 2.5f))
            {
                g.DrawEllipse(darkPen, circleX, circleY, circleSize, circleSize);
            }
            // Open book inside yellow circle
            using (var iconFont = new Font("Segoe UI Symbol", 20f))
            using (var bookBrush = new SolidBrush(Color.FromArgb(24, 38, 68)))
            {
                var sf = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
                g.DrawString("📖", iconFont, bookBrush, new RectangleF(circleX, circleY - 1, circleSize, circleSize), sf);
            }

            // 2. Subtitle: LIBRARY MANAGEMENT SYSTEM
            using (var subFont = new Font("Segoe UI", 7.8f, FontStyle.Bold))
            using (var subBrush = new SolidBrush(Color.FromArgb(160, 195, 240)))
            {
                var sf = new StringFormat { Alignment = StringAlignment.Center };
                g.DrawString("LIBRARY MANAGEMENT SYSTEM", subFont, subBrush, centerX, 12, sf);
            }

            // 3. Title: BOOKHAVEN
            using (var titleFont = new Font("Segoe UI", 20f, FontStyle.Bold))
            using (var titleBrush = new SolidBrush(Color.White))
            {
                var sf = new StringFormat { Alignment = StringAlignment.Center };
                g.DrawString("BOOKHAVEN", titleFont, titleBrush, centerX, 24, sf);
            }

            // 4. Pill badge: C# · WINDOWS FORMS · OOP
            var pillRect = new Rectangle(centerX - 110, 58, 220, 20);
            using (var path = UITheme.GetRoundedRectanglePath(pillRect, 10))
            {
                using var strokePen = new Pen(Color.FromArgb(88, 182, 252), 1.4f);
                g.DrawPath(strokePen, path);

                using var pillFont = new Font("Segoe UI", 7.5f, FontStyle.Bold);
                using var textBrush = new SolidBrush(Color.FromArgb(224, 242, 254));
                var sf = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
                g.DrawString("C#  ·  WINDOWS FORMS  ·  OOP", pillFont, textBrush, pillRect, sf);
            }

            // 5. Shield logo on right
            string logoPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources", "logo_shield.png");
            if (!File.Exists(logoPath))
            {
                logoPath = Path.Combine(Application.StartupPath, "..", "..", "..", "Resources", "logo_shield.png");
            }
            if (File.Exists(logoPath))
            {
                try
                {
                    using var img = Image.FromFile(logoPath);
                    g.DrawImage(img, centerX + 144, (h - 52) / 2, 48, 52);
                }
                catch { }
            }

            // 6. Cute decorative smiley faces (matches PDF Page 9)
            void DrawSmiley(Color color, int x, int y, int size, bool flip = false)
            {
                using var b = new SolidBrush(color);
                g.FillEllipse(b, x, y, size, size);
                using var p = new Pen(Color.FromArgb(30, 40, 60), 1.5f);
                int eyeY = y + (int)(size * 0.35f);
                g.FillEllipse(b, x + (int)(size * 0.25f), eyeY, 2, 2);
                g.FillEllipse(b, x + (int)(size * 0.65f), eyeY, 2, 2);
                g.DrawArc(p, x + (int)(size * 0.25f), y + (int)(size * 0.35f), (int)(size * 0.5f), (int)(size * 0.35f), 0, 180);
            }

            DrawSmiley(UITheme.SkyBlue, Math.Max(20, centerX - 320), 40, 24);
            DrawSmiley(Color.FromArgb(217, 70, 239), Math.Max(20, centerX - 240), 16, 20);
            DrawSmiley(UITheme.HotPink, Math.Min(w - 30, centerX + 230), 16, 20);
            DrawSmiley(Color.FromArgb(132, 204, 22), Math.Min(w - 36, centerX + 310), 40, 24);
        }
    }
}
