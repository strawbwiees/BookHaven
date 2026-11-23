using System.Drawing.Drawing2D;
using BookHaven.Services;
using BookHaven.UI;

namespace BookHaven.Views
{
    public class DashboardView : UserControl
    {
        private readonly LibraryDataService _service = LibraryDataService.Instance;

        public event EventHandler<string>? NavigateRequested;

        // Enlarged, readable fonts
        private static readonly Font PageTitleFont = new("League Spartan", 20f, FontStyle.Bold);
        private static readonly Font DateBadgeFont = new("League Spartan", 10f, FontStyle.Bold);
        private static readonly Font StatValueFont = new("League Spartan", 24f, FontStyle.Bold);
        private static readonly Font StatDescFont = new("League Spartan", 9.5f, FontStyle.Regular);
        private static readonly Font CardHeaderFont = new("League Spartan", 13.5f, FontStyle.Bold);
        private static readonly Font CardSubFont = new("League Spartan", 9f, FontStyle.Regular);
        private static readonly Font TableHeaderFont = new("League Spartan", 8.8f, FontStyle.Bold);
        private static readonly Font RowBoldFont = new("League Spartan", 9.8f, FontStyle.Bold);
        private static readonly Font RowBodyFont = new("League Spartan", 9.5f, FontStyle.Regular);
        private static readonly Font BadgeFont = new("League Spartan", 8.5f, FontStyle.Bold);
        private static readonly Font FineFont = new("League Spartan", 11.5f, FontStyle.Bold);
        private static readonly Font QuickBtnFont = new("League Spartan", 10f, FontStyle.Bold);

        // Header controls
        private Label _lblTitle = null!;
        private Label _lblDate = null!;

        // Stat cards
        private CardPanel _cardTitles = null!;
        private CardPanel _cardLoans = null!;
        private CardPanel _cardOverdue = null!;
        private CardPanel _cardBorrowers = null!;
        private Label _lblTitlesCount = null!;
        private Label _lblLoansCount = null!;
        private Label _lblOverdueCount = null!;
        private Label _lblBorrowersCount = null!;
        private Label _lblTitlesDesc = null!;
        private Label _lblLoansDesc = null!;
        private Label _lblOverdueDesc = null!;
        private Label _lblBorrowersDesc = null!;

        // Middle section
        private CardPanel _recentCard = null!;
        private CardPanel _overdueCard = null!;
        private Panel _recentListPanel = null!;
        private Panel _overdueListPanel = null!;
        private LinkLabel _lblSeeAll = null!;

        // Bottom section
        private CardPanel _chartCard = null!;
        private CardPanel _quickCard = null!;
        private Panel _barChartPanel = null!;
        private PillButton _btnIssue = null!;
        private PillButton _btnReturn = null!;
        private PillButton _btnAddBook = null!;
        private PillButton _btnAddBorrower = null!;

        public DashboardView()
        {
            InitializeComponent();
        }

        protected override Point ScrollToControl(Control activeControl)
        {
            return new Point(0, AutoScrollPosition.Y);
        }

        private static int LineHeight(Font font, int extra = 6) =>
            TextRenderer.MeasureText("Ag", font).Height + extra;

        private void InitializeComponent()
        {
            BackColor = UITheme.AppBackground;
            Dock = DockStyle.Fill;
            AutoScroll = true;

            // 1. Header
            _lblTitle = new Label
            {
                Text = "HELLO, LIBRARIAN!",
                Font = PageTitleFont,
                ForeColor = UITheme.NavyDark,
                BackColor = Color.Transparent,
                AutoSize = true
            };
            _lblDate = new Label
            {
                Text = DateTime.Now.ToString("ddd · MMM d, yyyy"),
                Font = DateBadgeFont,
                ForeColor = UITheme.NavyDark,
                BackColor = Color.White,
                Padding = new Padding(14, 8, 14, 8),
                AutoSize = true
            };
            Controls.AddRange(new Control[] { _lblTitle, _lblDate });

            // 2. Stat Cards
            var (titles, loans, overdue, borrowers) = _service.GetStatistics();

            _cardTitles = CreateStatCard("Titles in catalog", titles.ToString("N0"), "📚", out _lblTitlesCount, out _lblTitlesDesc);
            _cardLoans = CreateStatCard("Books on loan", loans.ToString("N0"), "📤", out _lblLoansCount, out _lblLoansDesc);
            _cardOverdue = CreateStatCard("Overdue returns", overdue.ToString("N0"), "⏰", out _lblOverdueCount, out _lblOverdueDesc);
            _cardBorrowers = CreateStatCard("Registered borrowers", borrowers.ToString("N0"), "👥", out _lblBorrowersCount, out _lblBorrowersDesc);
            Controls.AddRange(new Control[] { _cardTitles, _cardLoans, _cardOverdue, _cardBorrowers });

            // 3. Recent Transactions Card
            _recentCard = new CardPanel { BorderRadius = 16 };
            var lblRecentHeader = new Label
            {
                Text = "Recent transactions",
                Font = CardHeaderFont,
                ForeColor = UITheme.NavyDark,
                BackColor = Color.White,
                Location = new Point(20, 16),
                AutoSize = true
            };
            _lblSeeAll = new LinkLabel
            {
                Text = "See all",
                Font = CardSubFont,
                LinkColor = UITheme.NavyDark,
                BackColor = Color.White,
                AutoSize = true
            };
            _lblSeeAll.LinkClicked += (s, e) => NavigateRequested?.Invoke(this, "Borrowed Books");

            _recentListPanel = new Panel { AutoScroll = true, BackColor = Color.White };
            _recentCard.Controls.AddRange(new Control[] { lblRecentHeader, _lblSeeAll, _recentListPanel });
            Controls.Add(_recentCard);

            // 4. Most Overdue Card
            _overdueCard = new CardPanel { BorderRadius = 16 };
            var lblOverdueTitle = new Label
            {
                Text = "Most overdue",
                Font = CardHeaderFont,
                ForeColor = UITheme.NavyDark,
                BackColor = Color.White,
                Location = new Point(20, 16),
                AutoSize = true
            };
            var lblOverdueSub = new Label
            {
                Text = $"Fine: ₱{_service.Settings.FinePerDayLate:F2} per day late",
                Font = CardSubFont,
                ForeColor = UITheme.TextMuted,
                BackColor = Color.White,
                Location = new Point(20, 44),
                AutoSize = true
            };
            _overdueListPanel = new Panel { AutoScroll = true, BackColor = Color.White };
            _overdueCard.Controls.AddRange(new Control[] { lblOverdueTitle, lblOverdueSub, _overdueListPanel });
            Controls.Add(_overdueCard);

            // 5. Books Borrowed This Week (Chart) Card
            _chartCard = new CardPanel { BorderRadius = 16 };
            var lblChartHeader = new Label
            {
                Text = "Books borrowed this week",
                Font = CardHeaderFont,
                ForeColor = UITheme.NavyDark,
                BackColor = Color.White,
                Location = new Point(20, 16),
                AutoSize = true
            };
            var lblChartSub = new Label
            {
                Text = "Sample data for the mockup",
                Font = CardSubFont,
                ForeColor = UITheme.TextMuted,
                BackColor = Color.White,
                Location = new Point(20, 44),
                AutoSize = true
            };
            _barChartPanel = new Panel { BackColor = Color.White };
            _barChartPanel.Paint += DrawWeeklyChart;
            _chartCard.Controls.AddRange(new Control[] { lblChartHeader, lblChartSub, _barChartPanel });
            Controls.Add(_chartCard);

            // 6. Quick Actions Card
            _quickCard = new CardPanel { BorderRadius = 16 };
            var lblQuickHeader = new Label
            {
                Text = "Quick actions",
                Font = CardHeaderFont,
                ForeColor = UITheme.NavyDark,
                BackColor = Color.White,
                Location = new Point(20, 16),
                AutoSize = true
            };

            _btnIssue = new PillButton
            {
                Text = "Issue a book",
                Font = QuickBtnFont,
                CustomBackColor = UITheme.HotPink,
                CustomForeColor = Color.White
            };
            _btnIssue.Click += (s, e) => NavigateRequested?.Invoke(this, "Issue/Return Book");

            _btnReturn = new PillButton
            {
                Text = "Return a book",
                Font = QuickBtnFont,
                CustomBackColor = UITheme.SkyBlue,
                CustomForeColor = UITheme.NavyDark
            };
            _btnReturn.Click += (s, e) => NavigateRequested?.Invoke(this, "Issue/Return Book");

            _btnAddBook = new PillButton
            {
                Text = "Add a book",
                Font = QuickBtnFont,
                CustomBackColor = UITheme.SkyBlue,
                CustomForeColor = UITheme.NavyDark
            };
            _btnAddBook.Click += (s, e) => NavigateRequested?.Invoke(this, "Books-Add");

            _btnAddBorrower = new PillButton
            {
                Text = "Register a borrower",
                Font = QuickBtnFont,
                CustomBackColor = UITheme.SkyBlue,
                CustomForeColor = UITheme.NavyDark
            };
            _btnAddBorrower.Click += (s, e) => NavigateRequested?.Invoke(this, "Borrowers");

            _quickCard.Controls.AddRange(new Control[] { lblQuickHeader, _btnIssue, _btnReturn, _btnAddBook, _btnAddBorrower });
            Controls.Add(_quickCard);

            Resize += (s, e) => PerformResponsiveLayout();
            LoadDashboardData();
            PerformResponsiveLayout();
        }

        public void RefreshData()
        {
            var (titles, loans, overdue, borrowers) = _service.GetStatistics();
            _lblTitlesCount.Text = titles.ToString("N0");
            _lblLoansCount.Text = loans.ToString("N0");
            _lblOverdueCount.Text = overdue.ToString("N0");
            _lblBorrowersCount.Text = borrowers.ToString("N0");

            LoadDashboardData();
            PerformResponsiveLayout();
        }

        private void PerformResponsiveLayout()
        {
            SuspendLayout();

            int pad = ClientSize.Width < 600 ? 14 : 24;
            int gap = 16;
            int availWidth = Math.Max(280, ClientSize.Width - (pad * 2));

            // Header
            _lblTitle.Location = new Point(pad, 16);
            _lblDate.Text = DateTime.Now.ToString("ddd · MMM d, yyyy");
            bool stackedHeader = availWidth < 520;
            int currentY;
            if (stackedHeader)
            {
                _lblDate.Location = new Point(pad, _lblTitle.Bottom + 8);
                currentY = _lblDate.Bottom + 14;
            }
            else
            {
                _lblDate.Location = new Point(pad + availWidth - _lblDate.Width, 16);
                currentY = Math.Max(_lblTitle.Bottom, _lblDate.Bottom) + 16;
            }

            // Stat Cards: dynamically sized so description text is never clipped
            int cardH = Math.Max(132, _lblTitlesDesc.Bottom + 14);

            if (availWidth >= 860)
            {
                int cardW = (availWidth - (gap * 3)) / 4;
                _cardTitles.Bounds = new Rectangle(pad, currentY, cardW, cardH);
                _cardLoans.Bounds = new Rectangle(pad + (cardW + gap), currentY, cardW, cardH);
                _cardOverdue.Bounds = new Rectangle(pad + (cardW + gap) * 2, currentY, cardW, cardH);
                _cardBorrowers.Bounds = new Rectangle(pad + (cardW + gap) * 3, currentY, cardW, cardH);
                ResizeStatCardLabels(cardW);
                currentY += cardH + gap;
            }
            else if (availWidth >= 330)
            {
                int cardW = (availWidth - gap) / 2;
                _cardTitles.Bounds = new Rectangle(pad, currentY, cardW, cardH);
                _cardLoans.Bounds = new Rectangle(pad + cardW + gap, currentY, cardW, cardH);
                currentY += cardH + gap;
                _cardOverdue.Bounds = new Rectangle(pad, currentY, cardW, cardH);
                _cardBorrowers.Bounds = new Rectangle(pad + cardW + gap, currentY, cardW, cardH);
                ResizeStatCardLabels(cardW);
                currentY += cardH + gap;
            }
            else
            {
                int cardW = availWidth;
                _cardTitles.Bounds = new Rectangle(pad, currentY, cardW, cardH); currentY += cardH + 10;
                _cardLoans.Bounds = new Rectangle(pad, currentY, cardW, cardH); currentY += cardH + 10;
                _cardOverdue.Bounds = new Rectangle(pad, currentY, cardW, cardH); currentY += cardH + 10;
                _cardBorrowers.Bounds = new Rectangle(pad, currentY, cardW, cardH); currentY += cardH + gap;
                ResizeStatCardLabels(cardW);
            }

            // Middle & Bottom Sections
            int midH = 260;
            int botH = 236;

            if (availWidth >= 860)
            {
                int used = currentY + midH + gap + botH + 20;
                int spare = ClientSize.Height - used;
                if (spare > 0)
                {
                    int toMid = spare * 11 / 20;
                    midH += toMid;
                    botH += spare - toMid;
                }
            }

            if (availWidth >= 860)
            {
                int recentW = (int)(availWidth * 0.65f) - (gap / 2);
                int overdueW = availWidth - recentW - gap;

                _recentCard.Bounds = new Rectangle(pad, currentY, recentW, midH);
                _overdueCard.Bounds = new Rectangle(pad + recentW + gap, currentY, overdueW, midH);

                _lblSeeAll.Location = new Point(recentW - _lblSeeAll.Width - 20, 20);
                _recentListPanel.Bounds = new Rectangle(20, 52, recentW - 40, midH - 66);
                _overdueListPanel.Bounds = new Rectangle(20, 72, overdueW - 40, midH - 86);

                currentY += midH + gap;
            }
            else
            {
                _recentCard.Bounds = new Rectangle(pad, currentY, availWidth, midH);
                _lblSeeAll.Location = new Point(availWidth - _lblSeeAll.Width - 20, 20);
                _recentListPanel.Bounds = new Rectangle(20, 52, availWidth - 40, midH - 66);
                currentY += midH + gap;

                _overdueCard.Bounds = new Rectangle(pad, currentY, availWidth, 230);
                _overdueListPanel.Bounds = new Rectangle(20, 72, availWidth - 40, 144);
                currentY += 230 + gap;
            }

            // Bottom Section: Chart & Quick Actions
            if (availWidth >= 860)
            {
                int chartW = (int)(availWidth * 0.65f) - (gap / 2);
                int quickW = availWidth - chartW - gap;

                _chartCard.Bounds = new Rectangle(pad, currentY, chartW, botH);
                _quickCard.Bounds = new Rectangle(pad + chartW + gap, currentY, quickW, botH);

                _barChartPanel.Bounds = new Rectangle(20, 70, chartW - 40, botH - 84);

                int qBtnW = quickW - 40;
                int qGap = 10;
                int qTop = 52;
                int qH = Math.Clamp((botH - qTop - 18 - qGap * 3) / 4, 36, 56);
                var qButtons = new[] { _btnIssue, _btnReturn, _btnAddBook, _btnAddBorrower };
                for (int i = 0; i < qButtons.Length; i++)
                    qButtons[i].Bounds = new Rectangle(20, qTop + i * (qH + qGap), qBtnW, qH);

                currentY += botH + 20;
            }
            else
            {
                _chartCard.Bounds = new Rectangle(pad, currentY, availWidth, botH);
                _barChartPanel.Bounds = new Rectangle(20, 70, availWidth - 40, botH - 84);
                currentY += botH + gap;

                int quickCardH = 236;
                _quickCard.Bounds = new Rectangle(pad, currentY, availWidth, quickCardH);
                int qBtnW = availWidth - 40;
                int qTop = 52;
                int qH = 38;
                int qGap = 8;
                var qButtons = new[] { _btnIssue, _btnReturn, _btnAddBook, _btnAddBorrower };
                for (int i = 0; i < qButtons.Length; i++)
                    qButtons[i].Bounds = new Rectangle(20, qTop + i * (qH + qGap), qBtnW, qH);

                currentY += quickCardH + 20;
            }

            AutoScrollMinSize = new Size(0, currentY);
            ResumeLayout();

            RefreshRecentTableWidth(_recentListPanel.Width);
            _barChartPanel.Invalidate();
        }

        private void ResizeStatCardLabels(int cardWidth)
        {
            int w = cardWidth - 36;
            _lblTitlesCount.Width = w;
            _lblLoansCount.Width = w;
            _lblOverdueCount.Width = w;
            _lblBorrowersCount.Width = w;

            _lblTitlesDesc.Width = w;
            _lblLoansDesc.Width = w;
            _lblOverdueDesc.Width = w;
            _lblBorrowersDesc.Width = w;
        }

        private static CardPanel CreateStatCard(string title, string value, string icon, out Label valueLabel, out Label descLabel)
        {
            var card = new CardPanel { BorderRadius = 16 };

            var iconCircle = new Label
            {
                Text = icon,
                Font = new Font("League Spartan Emoji", 14f),
                BackColor = UITheme.SkyBlueLight,
                Size = new Size(36, 36),
                Location = new Point(18, 12),
                TextAlign = ContentAlignment.MiddleCenter
            };

            int valH = LineHeight(StatValueFont, 0);
            valueLabel = new Label
            {
                Text = value,
                Font = StatValueFont,
                ForeColor = UITheme.NavyDark,
                BackColor = Color.White,
                Location = new Point(16, 50),
                Height = valH,
                TextAlign = ContentAlignment.MiddleLeft,
                AutoEllipsis = true
            };

            int descH = LineHeight(StatDescFont, 4);
            descLabel = new Label
            {
                Text = title,
                Font = StatDescFont,
                ForeColor = UITheme.TextMuted,
                BackColor = Color.White,
                Location = new Point(18, valueLabel.Bottom + 2),
                Height = descH,
                TextAlign = ContentAlignment.MiddleLeft,
                AutoEllipsis = true
            };

            card.Controls.AddRange(new Control[] { iconCircle, valueLabel, descLabel });
            return card;
        }

        private int RecentTableWidth() =>
            Math.Max(240, _recentListPanel.ClientSize.Width - SystemInformation.VerticalScrollBarWidth);

        private static (int borrower, int book, int due, int status) GetRecentColumns(int totalW)
        {
            if (totalW >= 420)
            {
                int b = (int)(totalW * 0.25f);
                int bk = (int)(totalW * 0.39f);
                int d = (int)(totalW * 0.16f);
                return (b, bk, d, Math.Max(92, totalW - b - bk - d - 8));
            }
            int b2 = (int)(totalW * 0.34f);
            int st2 = 84;
            return (b2, Math.Max(40, totalW - b2 - st2 - 4), 0, st2);
        }

        private void RefreshRecentTableWidth(int totalWidth)
        {
            totalWidth = Math.Max(240, totalWidth - SystemInformation.VerticalScrollBarWidth);
            foreach (Control row in _recentListPanel.Controls)
            {
                row.Width = totalWidth;
                if (row.Controls.Count >= 4)
                {
                    var (borrowerW, bookW, dueW, statusW) = GetRecentColumns(totalWidth);

                    row.Controls[0].Bounds = new Rectangle(0, row.Controls[0].Top, borrowerW, row.Controls[0].Height);
                    row.Controls[1].Bounds = new Rectangle(borrowerW, row.Controls[1].Top, bookW, row.Controls[1].Height);
                    row.Controls[2].Bounds = new Rectangle(borrowerW + bookW, row.Controls[2].Top, dueW, row.Controls[2].Height);
                    row.Controls[3].Bounds = new Rectangle(borrowerW + bookW + dueW, row.Controls[3].Top, row.Top == 0 ? statusW : Math.Min(92, statusW), row.Controls[3].Height);
                }
            }
        }

        private void LoadDashboardData()
        {
            // Recent Transactions List
            _recentListPanel.Controls.Clear();

            int totalW = RecentTableWidth();
            var (borrowerW, bookW, dueW, statusW) = GetRecentColumns(totalW);

            int headerH = LineHeight(TableHeaderFont, 8);
            var headerRow = new Panel { Size = new Size(totalW, headerH), Location = new Point(0, 0), BackColor = Color.White };
            headerRow.Controls.Add(new Label { Text = "BORROWER", Font = TableHeaderFont, ForeColor = UITheme.TextMuted, BackColor = Color.White, Location = new Point(0, 0), Size = new Size(borrowerW, headerH), TextAlign = ContentAlignment.MiddleLeft });
            headerRow.Controls.Add(new Label { Text = "BOOK", Font = TableHeaderFont, ForeColor = UITheme.TextMuted, BackColor = Color.White, Location = new Point(borrowerW, 0), Size = new Size(bookW, headerH), TextAlign = ContentAlignment.MiddleLeft });
            headerRow.Controls.Add(new Label { Text = "DUE", Font = TableHeaderFont, ForeColor = UITheme.TextMuted, BackColor = Color.White, Location = new Point(borrowerW + bookW, 0), Size = new Size(dueW, headerH), TextAlign = ContentAlignment.MiddleLeft });
            headerRow.Controls.Add(new Label { Text = "STATUS", Font = TableHeaderFont, ForeColor = UITheme.TextMuted, BackColor = Color.White, Location = new Point(borrowerW + bookW + dueW, 0), Size = new Size(statusW, headerH), TextAlign = ContentAlignment.MiddleLeft });
            _recentListPanel.Controls.Add(headerRow);

            int y = headerH + 6;
            int rowH = 42;
            int textH = LineHeight(RowBoldFont, 4);

            foreach (var tx in _service.Transactions.Take(8))
            {
                var row = new Panel { Size = new Size(totalW, rowH), Location = new Point(0, y), BackColor = Color.White };
                int textY = (rowH - textH) / 2;

                var lblBorrower = new Label { Text = tx.BorrowerName, Font = RowBoldFont, ForeColor = UITheme.TextMain, BackColor = Color.White, Location = new Point(0, textY), Size = new Size(borrowerW, textH), TextAlign = ContentAlignment.MiddleLeft, AutoEllipsis = true };
                var lblBook = new Label { Text = tx.BookTitle, Font = RowBodyFont, ForeColor = UITheme.TextMain, BackColor = Color.White, Location = new Point(borrowerW, textY), Size = new Size(bookW, textH), TextAlign = ContentAlignment.MiddleLeft, AutoEllipsis = true };
                var lblDue = new Label { Text = tx.DueDate.ToString("MMM dd"), Font = RowBodyFont, ForeColor = UITheme.TextMuted, BackColor = Color.White, Location = new Point(borrowerW + bookW, textY), Size = new Size(dueW, textH), TextAlign = ContentAlignment.MiddleLeft };

                Color statusBg = tx.Status switch
                {
                    "Returned" => UITheme.StatusGreenBg,
                    "Overdue" => UITheme.StatusRedBg,
                    _ => UITheme.StatusBlueBg
                };
                Color statusFg = tx.Status switch
                {
                    "Returned" => UITheme.StatusGreen,
                    "Overdue" => UITheme.StatusRed,
                    _ => UITheme.StatusBlue
                };

                int badgeH = 26;
                var lblStatus = new Label
                {
                    Text = tx.Status,
                    Font = BadgeFont,
                    BackColor = statusBg,
                    ForeColor = statusFg,
                    Location = new Point(borrowerW + bookW + dueW, (rowH - badgeH) / 2),
                    Size = new Size(Math.Min(92, statusW), badgeH),
                    TextAlign = ContentAlignment.MiddleCenter
                };

                row.Controls.AddRange(new Control[] { lblBorrower, lblBook, lblDue, lblStatus });
                _recentListPanel.Controls.Add(row);
                y += rowH + 2;
            }

            // Overdue List
            _overdueListPanel.Controls.Clear();
            var overdues = _service.Transactions
                .Where(t => t.Status == "Overdue")
                .OrderByDescending(t => t.DaysLate)
                .ToList();
            int oy = 0;
            if (overdues.Count == 0)
            {
                _overdueListPanel.Controls.Add(new Label
                {
                    Text = "Nothing overdue — all caught up!",
                    Font = RowBodyFont,
                    ForeColor = UITheme.TextMuted,
                    BackColor = Color.White,
                    Location = new Point(0, 8),
                    AutoSize = true
                });
            }

            int ovCardH = 84;
            int nameH = LineHeight(RowBoldFont, 4);
            int fineH = LineHeight(FineFont, 4);

            foreach (var ov in overdues)
            {
                var ovRow = new CardPanel
                {
                    Location = new Point(0, oy),
                    Size = new Size(Math.Max(240, _overdueListPanel.Width - 10), ovCardH),
                    BorderRadius = 10,
                    BackgroundFill = Color.FromArgb(249, 252, 255),
                    Anchor = AnchorStyles.Left | AnchorStyles.Top | AnchorStyles.Right
                };

                var avatar = new PictureBox
                {
                    Location = new Point(10, (ovCardH - 44) / 2),
                    Size = new Size(44, 44),
                    Image = UITheme.GenerateAvatar(GetInitials(ov.BorrowerName), 44)
                };

                var lblName = new Label
                {
                    Text = ov.BorrowerName,
                    Font = RowBoldFont,
                    ForeColor = UITheme.NavyDark,
                    BackColor = Color.FromArgb(249, 252, 255),
                    Location = new Point(62, 10),
                    Size = new Size(ovRow.Width - 160, nameH),
                    TextAlign = ContentAlignment.MiddleLeft,
                    AutoEllipsis = true,
                    Anchor = AnchorStyles.Left | AnchorStyles.Top | AnchorStyles.Right
                };

                var lblLate = new Label
                {
                    Text = $"{ov.DaysLate} days late • {ov.BookTitle}",
                    Font = CardSubFont,
                    ForeColor = UITheme.TextMuted,
                    BackColor = Color.FromArgb(249, 252, 255),
                    Location = new Point(62, lblName.Bottom + 2),
                    Size = new Size(ovRow.Width - 160, ovCardH - lblName.Bottom - 10),
                    AutoEllipsis = true,
                    Anchor = AnchorStyles.Left | AnchorStyles.Top | AnchorStyles.Right
                };

                var lblFine = new Label
                {
                    Text = $"₱{ov.FineAmount:N2}",
                    Font = FineFont,
                    ForeColor = UITheme.NavyDark,
                    BackColor = Color.FromArgb(249, 252, 255),
                    Location = new Point(ovRow.Width - 92, 14),
                    Size = new Size(82, fineH),
                    TextAlign = ContentAlignment.MiddleRight,
                    Anchor = AnchorStyles.Top | AnchorStyles.Right
                };

                ovRow.Controls.AddRange(new Control[] { avatar, lblName, lblLate, lblFine });
                _overdueListPanel.Controls.Add(ovRow);
                oy += ovCardH + 8;
            }
        }

        private static string GetInitials(string name)
        {
            var parts = name.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length == 1) return parts[0][..1].ToUpper();
            return $"{parts[0][0]}{parts[^1][0]}".ToUpper();
        }

        private void DrawWeeklyChart(object? sender, PaintEventArgs e)
        {
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;

            string[] days = { "Mon", "Tue", "Wed", "Thu", "Fri", "Sat", "Sun" };
            int[] values = { 14, 22, 18, 30, 26, 9, 5 };
            int maxVal = 35;

            int panelW = _barChartPanel.Width;
            int panelH = _barChartPanel.Height;
            if (panelW < 100 || panelH < 60) return;

            int topPad = 24;
            int bottomPad = 28;
            int chartHeight = Math.Max(20, panelH - topPad - bottomPad);
            int step = panelW / 7;
            int barWidth = Math.Max(20, Math.Min(44, step - 14));

            using var valFont = new Font("League Spartan", 9.5f, FontStyle.Bold);
            using var dayFont = new Font("League Spartan", 9f, FontStyle.Regular);
            using var valBrush = new SolidBrush(UITheme.NavyDark);
            using var dayBrush = new SolidBrush(UITheme.TextMuted);
            using var barBrush = new SolidBrush(UITheme.SkyBlue);
            using var sf = new StringFormat { Alignment = StringAlignment.Center };

            for (int i = 0; i < 7; i++)
            {
                int val = values[i];
                int bh = (int)((float)val / maxVal * chartHeight);
                int bx = i * step + (step - barWidth) / 2;
                int by = topPad + (chartHeight - bh);

                // Value label on top
                g.DrawString(val.ToString(), valFont, valBrush, bx + barWidth / 2f, by - 20, sf);

                // Bar
                if (bh > 0)
                {
                    var rect = new Rectangle(bx, by, barWidth, bh);
                    using var path = UITheme.GetRoundedRectanglePath(rect, 8);
                    g.FillPath(barBrush, path);
                }

                // Day label
                g.DrawString(days[i], dayFont, dayBrush, bx + barWidth / 2f, topPad + chartHeight + 6, sf);
            }
        }
    }
}