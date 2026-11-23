using System.Drawing.Drawing2D;
using BookHaven.Services;
using BookHaven.UI;
using BookHaven.Views;

namespace BookHaven.Forms
{
    public class MainForm : Form, IMessageFilter
    {
        private const int WM_LBUTTONDOWN = 0x0201;

        private enum LayoutMode { Full, Rail, Drawer }

        private BufferedPanel _sidebarPanel = null!;
        private Panel _rightContainer = null!;
        private BufferedPanel _bannerPanel = null!;
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
        private NavPillButton _logoutButton = null!;
        private readonly LibraryDataService _service = LibraryDataService.Instance;
        private readonly ToolTip _navToolTip = new();

        // Responsive state
        private LayoutMode _mode = LayoutMode.Full;
        private bool _railExpanded = false;
        private bool _drawerOpen = false;
        private bool _filterInstalled = false;
        private static int ExpandedSidebarWidth => UITheme.S(250);
        private static int CollapsedSidebarWidth => UITheme.S(72);
        private const int FullModeMinWidth = 960;   // sidebar + content side by side
        private const int RailModeMinWidth = 640;   // below this the sidebar becomes a slide-over drawer

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
            MinimumSize = new Size(360, 520);
            StartPosition = FormStartPosition.CenterScreen;
            BackColor = UITheme.AppBackground;

            // 1. Right Container (Holds Top Banner + Views)
            _rightContainer = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = UITheme.AppBackground
            };

            // 2. Left Sidebar (Docked Left)
            _sidebarPanel = new BufferedPanel
            {
                Dock = DockStyle.Left,
                Width = ExpandedSidebarWidth,
                BackColor = UITheme.SidebarBackground
            };
            _sidebarPanel.Paint += PaintSidebarEdge;
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

            _bannerPanel = new BufferedPanel
            {
                Dock = DockStyle.Top,
                Height = UITheme.S(88),
                BackColor = UITheme.BannerBackground
            };
            _bannerPanel.Paint += PaintBanner;

            // Sidebar toggle button (rail + drawer modes)
            _btnToggleSidebar = new Button
            {
                Text = "☰",
                Font = new Font("League Spartan", 12f, FontStyle.Bold),
                ForeColor = Color.White,
                BackColor = Color.FromArgb(32, 44, 68),
                FlatStyle = FlatStyle.Flat,
                Size = new Size(UITheme.S(40), UITheme.S(40)),
                Location = new Point(UITheme.S(14), UITheme.S(24)),
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
            _sidebarPanel.Resize += (s, e) => LayoutSidebar();

            // Start on Dashboard
            NavigateTo("Home");
            ApplyLayoutMode();
        }

        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            RemoveDrawerFilter();
            base.OnFormClosed(e);
        }

        private void MainForm_Resize(object? sender, EventArgs e)
        {
            ApplyLayoutMode();
        }

        // ---------------------------------------------------------------
        // RESPONSIVE MODES
        //   Full   (>= 960px): expanded sidebar docked left
        //   Rail   (>= 640px): icon rail docked left, hamburger expands it
        //   Drawer (<  640px): sidebar hidden, hamburger slides it over the content
        // ---------------------------------------------------------------
        private LayoutMode DetermineMode()
        {
            int w = ClientSize.Width;
            if (w >= FullModeMinWidth) return LayoutMode.Full;
            if (w >= RailModeMinWidth) return LayoutMode.Rail;
            return LayoutMode.Drawer;
        }

        private bool IsExpandedLook =>
            _mode == LayoutMode.Full ||
            _mode == LayoutMode.Drawer ||
            (_mode == LayoutMode.Rail && _railExpanded);

        private void ApplyLayoutMode()
        {
            if (_sidebarPanel == null || _bannerPanel == null) return;

            _mode = DetermineMode();

            SuspendLayout();
            switch (_mode)
            {
                case LayoutMode.Full:
                    _railExpanded = false;
                    _drawerOpen = false;
                    RemoveDrawerFilter();
                    DockSidebar(ExpandedSidebarWidth);
                    _btnToggleSidebar.Visible = false;
                    _bannerPanel.Height = UITheme.S(88);
                    break;

                case LayoutMode.Rail:
                    _drawerOpen = false;
                    RemoveDrawerFilter();
                    DockSidebar(_railExpanded ? ExpandedSidebarWidth : CollapsedSidebarWidth);
                    _btnToggleSidebar.Visible = true;
                    _bannerPanel.Height = UITheme.S(88);
                    break;

                case LayoutMode.Drawer:
                    _railExpanded = false;
                    _btnToggleSidebar.Visible = true;
                    _bannerPanel.Height = UITheme.S(64);
                    ShowSidebarAsDrawer();
                    break;
            }

            _btnToggleSidebar.Top = (_bannerPanel.Height - _btnToggleSidebar.Height) / 2;
            ResumeLayout(true);

            LayoutSidebar();
            _bannerPanel.Invalidate();
        }

        private void DockSidebar(int width)
        {
            if (_sidebarPanel.Dock != DockStyle.Left)
            {
                _sidebarPanel.Dock = DockStyle.Left;
                _sidebarPanel.SendToBack(); // docks before the Fill container
            }
            _sidebarPanel.Visible = true;
            _sidebarPanel.Width = width;
        }

        private void ShowSidebarAsDrawer()
        {
            if (_sidebarPanel.Dock != DockStyle.None)
            {
                _sidebarPanel.Dock = DockStyle.None;
                _sidebarPanel.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left;
            }

            _sidebarPanel.Bounds = new Rectangle(0, 0, ExpandedSidebarWidth, ClientSize.Height);
            _sidebarPanel.Visible = _drawerOpen;

            if (_drawerOpen)
            {
                _sidebarPanel.BringToFront();
                AddDrawerFilter();
            }
            else
            {
                RemoveDrawerFilter();
            }
        }

        private void ToggleSidebar()
        {
            if (_mode == LayoutMode.Rail)
            {
                _railExpanded = !_railExpanded;
                ApplyLayoutMode();
            }
            else if (_mode == LayoutMode.Drawer)
            {
                _drawerOpen = !_drawerOpen;
                ApplyLayoutMode();
            }
        }

        private void CloseTransientSidebar()
        {
            if (_mode == LayoutMode.Rail && _railExpanded)
            {
                _railExpanded = false;
                ApplyLayoutMode();
            }
            else if (_mode == LayoutMode.Drawer && _drawerOpen)
            {
                _drawerOpen = false;
                ApplyLayoutMode();
            }
        }

        // Tap/click anywhere outside the open drawer closes it.
        private void AddDrawerFilter()
        {
            if (_filterInstalled) return;
            Application.AddMessageFilter(this);
            _filterInstalled = true;
        }

        private void RemoveDrawerFilter()
        {
            if (!_filterInstalled) return;
            Application.RemoveMessageFilter(this);
            _filterInstalled = false;
        }

        public bool PreFilterMessage(ref Message m)
        {
            if (m.Msg != WM_LBUTTONDOWN || _mode != LayoutMode.Drawer || !_drawerOpen) return false;

            Control? c = Control.FromHandle(m.HWnd);
            if (c == null || c.FindForm() != this) return false;

            for (Control? p = c; p != null; p = p.Parent)
            {
                if (p == _sidebarPanel || p == _btnToggleSidebar) return false;
            }

            BeginInvoke(new Action(CloseTransientSidebar));
            return false; // let the click through to whatever was tapped
        }

        // ---------------------------------------------------------------
        // SIDEBAR
        // ---------------------------------------------------------------
        private void PaintSidebarEdge(object? sender, PaintEventArgs e)
        {
            using var pen = new Pen(Color.FromArgb(205, 224, 244), 1f);
            e.Graphics.DrawLine(pen, _sidebarPanel.Width - 1, 0, _sidebarPanel.Width - 1, _sidebarPanel.Height);
        }

        private void BuildSidebar()
        {
            // Admin Avatar matching PDF Page 8
            _avatarBox = new PictureBox
            {
                Size = new Size(68, 68),
                Location = new Point((ExpandedSidebarWidth - 68) / 2, 18),
                Image = DrawAdminAvatar(68),
                SizeMode = PictureBoxSizeMode.Zoom,
                BackColor = Color.Transparent
            };
            _sidebarPanel.Controls.Add(_avatarBox);

            _lblAdmin = new Label
            {
                Text = "ADMIN",
                Font = new Font("League Spartan", 13f, FontStyle.Bold),
                ForeColor = UITheme.NavyDark,
                TextAlign = ContentAlignment.MiddleCenter,
                Location = new Point(0, 90),
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

            foreach (var (key, num, text) in navItems)
            {
                var btn = new NavPillButton
                {
                    NavNumber = num,
                    NavText = text,
                    Tag = key,
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
                        CloseTransientSidebar();
                    }
                };

                if (key == "Logout") _logoutButton = btn;
                _navButtons.Add(btn);
                _sidebarPanel.Controls.Add(btn);
            }

        }

        /// <summary>
        /// Lays the sidebar out against its real height: avatar and ADMIN label on top,
        /// evenly spaced nav pills below, and Log out pinned to the bottom.
        /// </summary>
        private void LayoutSidebar()
        {
            if (_avatarBox == null || _lblAdmin == null || _logoutButton == null) return;

            bool expanded = IsExpandedLook;
            int sw = _sidebarPanel.Width;
            int sh = _sidebarPanel.Height;
            if (sw <= 0 || sh <= 0) return;

            int edge = UITheme.S(16);
            int btnW = Math.Max(UITheme.S(40), sw - edge * 2 - 1);
            int btnX = edge;
            if (!expanded)
            {
                btnW = UITheme.S(48);
                btnX = (sw - btnW) / 2;
            }

            // Header: avatar, then the ADMIN label with breathing room underneath
            int avatar = expanded ? UITheme.S(sh < 620 ? 60 : 76) : UITheme.S(40);
            if (_avatarBox.Image == null || _avatarBox.Image.Width != avatar)
            {
                var old = _avatarBox.Image;
                _avatarBox.Image = DrawAdminAvatar(avatar);
                old?.Dispose();
            }
            _avatarBox.Size = new Size(avatar, avatar);
            _avatarBox.Location = new Point((sw - avatar) / 2, UITheme.S(expanded ? 28 : 18));
            _avatarBox.Visible = true;

            int navTop;
            _lblAdmin.Visible = expanded;
            if (expanded)
            {
                _lblAdmin.Bounds = new Rectangle(0, _avatarBox.Bottom + UITheme.S(10), sw, _lblAdmin.Font.Height + UITheme.S(6));
                navTop = _lblAdmin.Bottom + UITheme.S(28);
            }
            else
            {
                navTop = _avatarBox.Bottom + UITheme.S(24);
            }

            // Footer: Log out pinned to the bottom
            int logoutH = UITheme.S(48);
            int logoutTop = sh - UITheme.S(24) - logoutH;
            _logoutButton.Bounds = new Rectangle(btnX, logoutTop, btnW, logoutH);

            // Middle: nav pills with generous spacing, spread over the remaining height
            var mainButtons = _navButtons.Where(b => b != _logoutButton).ToList();
            int n = mainButtons.Count;
            int h = UITheme.S(48), gap = UITheme.S(12);
            int region = logoutTop - UITheme.S(24) - navTop;
            int extra = region - (n * h + (n - 1) * gap);

            if (extra > 0)
            {
                int addH = Math.Min(UITheme.S(6), extra / n);
                h += addH;
                extra -= addH * n;
                gap += Math.Min(UITheme.S(12), extra / Math.Max(1, n - 1));
            }
            else if (extra < 0)
            {
                gap = UITheme.S(6);
                h = Math.Max(UITheme.S(36), (region - (n - 1) * gap) / n);
            }

            int y = navTop;
            foreach (var b in mainButtons)
            {
                b.Bounds = new Rectangle(btnX, y, btnW, h);
                y += h + gap;
            }

            // Icon-only rail gets tooltips; the expanded sidebar doesn't need them.
            foreach (var b in _navButtons)
                _navToolTip.SetToolTip(b, expanded ? string.Empty : b.NavText);

            foreach (var b in _navButtons) b.Invalidate();
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
            bool compact = _bannerPanel.Width < UITheme.S(680);
            UITheme.DrawBrandBanner(e.Graphics, new Rectangle(0, 0, _bannerPanel.Width, _bannerPanel.Height), compact, UITheme.S(60));
        }
    }
}
