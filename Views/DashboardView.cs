using System.Drawing.Drawing2D;
using BookHaven.Services;
using BookHaven.UI;

namespace BookHaven.Views
{
    public class DashboardView : UserControl
    {
        private readonly LibraryDataService _service = LibraryDataService.Instance;

        public event EventHandler<string>? NavigateRequested;

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
            // Only scroll vertically, NEVER scroll horizontally!
            return new Point(0, AutoScrollPosition.Y);
        }

        private void InitializeComponent()
        {
            BackColor = UITheme.AppBackground;
            Dock = DockStyle.Fill;
            AutoScroll = true;

            // 1. Header
            _lblTitle = new Label
            {
                Text = "HELLO, LIBRARIAN!",
                Font = new Font("Segoe UI", 18f, FontStyle.Bold),
                ForeColor = UITheme.NavyDark,
                AutoSize = true
            };
            _lblDate = new Label
            {
                Text = DateTime.Now.ToString("ddd · MMM d, yyyy"),
                Font = new Font("Segoe UI", 9.5f, FontStyle.Bold),
                ForeColor = UITheme.NavyDark,
                BackColor = Color.White,
                Padding = new Padding(12, 6, 12, 6),
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
                Font = UITheme.SubHeaderFont,
                ForeColor = UITheme.NavyDark,
                Location = new Point(18, 16),
                AutoSize = true
            };
            _lblSeeAll = new LinkLabel
            {
                Text = "See all",
                Font = UITheme.SmallFont,
                LinkColor = UITheme.NavyDark,
                AutoSize = true
            };
            _lblSeeAll.LinkClicked += (s, e) => NavigateRequested?.Invoke(this, "Borrowed Books");

            _recentListPanel = new Panel { AutoScroll = true, BackColor = Color.Transparent };
            _recentCard.Controls.AddRange(new Control[] { lblRecentHeader, _lblSeeAll, _recentListPanel });
            Controls.Add(_recentCard);

            // 4. Most Overdue Card
            _overdueCard = new CardPanel { BorderRadius = 16 };
            var lblOverdueTitle = new Label
            {
                Text = "Most overdue",
                Font = UITheme.SubHeaderFont,
                ForeColor = UITheme.NavyDark,
                Location = new Point(18, 16),
                AutoSize = true
            };
            var lblOverdueSub = new Label
            {
                Text = $"Fine: ₱{_service.Settings.FinePerDayLate:F2} per day late",
                Font = UITheme.SmallFont,
                ForeColor = UITheme.TextMuted,
                Location = new Point(18, 42),
                AutoSize = true
            };
            _overdueListPanel = new Panel { AutoScroll = true, BackColor = Color.Transparent };
            _overdueCard.Controls.AddRange(new Control[] { lblOverdueTitle, lblOverdueSub, _overdueListPanel });
            Controls.Add(_overdueCard);

            // 5. Books Borrowed This Week (Chart) Card
            _chartCard = new CardPanel { BorderRadius = 16 };
            var lblChartHeader = new Label
            {
                Text = "Books borrowed this week",
                Font = UITheme.SubHeaderFont,
                ForeColor = UITheme.NavyDark,
                Location = new Point(18, 14),
                AutoSize = true
            };
            var lblChartSub = new Label
            {
                Text = "Sample data for the mockup",
                Font = UITheme.SmallFont,
                ForeColor = UITheme.TextMuted,
                Location = new Point(18, 38),
                AutoSize = true
            };
            _barChartPanel = new Panel { BackColor = Color.Transparent };
            _barChartPanel.Paint += DrawWeeklyChart;
            _chartCard.Controls.AddRange(new Control[] { lblChartHeader, lblChartSub, _barChartPanel });
            Controls.Add(_chartCard);

            // 6. Quick Actions Card
            _quickCard = new CardPanel { BorderRadius = 16 };
            var lblQuickHeader = new Label
            {
                Text = "Quick actions",
                Font = UITheme.SubHeaderFont,
                ForeColor = UITheme.NavyDark,
                Location = new Point(18, 14),
                AutoSize = true
            };

            _btnIssue = new PillButton
            {
                Text = "Issue a book",
                CustomBackColor = UITheme.HotPink,
                CustomForeColor = Color.White
            };
            _btnIssue.Click += (s, e) => NavigateRequested?.Invoke(this, "Issue/Return Book");

            _btnReturn = new PillButton
            {
                Text = "Return a book",
                CustomBackColor = UITheme.SkyBlue,
                CustomForeColor = UITheme.NavyDark
            };
            _btnReturn.Click += (s, e) => NavigateRequested?.Invoke(this, "Issue/Return Book");

            _btnAddBook = new PillButton
            {
                Text = "Add a book",
                CustomBackColor = UITheme.SkyBlue,
                CustomForeColor = UITheme.NavyDark
            };
            _btnAddBook.Click += (s, e) => NavigateRequested?.Invoke(this, "Books-Add");

            _btnAddBorrower = new PillButton
            {
                Text = "Register a borrower",
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
                _lblDate.Location = new Point(pad, 54);
                currentY = 94;
            }
            else
            {
                _lblDate.Location = new Point(pad + availWidth - _lblDate.Width, 18);
                currentY = 56;
            }
            int cardH = 118;

            // Stat Cards Layout
            if (availWidth >= 860)
            {
                // 4 cards in 1 row
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
                // 2x2 grid
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
                // 1 card per row (mobile)
                int cardW = availWidth;
                _cardTitles.Bounds = new Rectangle(pad, currentY, cardW, cardH); currentY += cardH + 10;
                _cardLoans.Bounds = new Rectangle(pad, currentY, cardW, cardH); currentY += cardH + 10;
                _cardOverdue.Bounds = new Rectangle(pad, currentY, cardW, cardH); currentY += cardH + 10;
                _cardBorrowers.Bounds = new Rectangle(pad, currentY, cardW, cardH); currentY += cardH + gap;
                ResizeStatCardLabels(cardW);
            }

            // Middle Section: Recent Transactions & Most Overdue
            int midH = 236;
            int botH = 210;

            // On wide layouts, hand any spare viewport height to the two card rows
            // so the dashboard fills the window instead of leaving an empty band at the bottom.
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

                _lblSeeAll.Location = new Point(recentW - _lblSeeAll.Width - 18, 16);
                _recentListPanel.Bounds = new Rectangle(18, 44, recentW - 36, midH - 52);
                _overdueListPanel.Bounds = new Rectangle(18, 62, overdueW - 36, midH - 70);

                currentY += midH + gap;
            }
            else
            {
                // Stack vertically on mobile
                _recentCard.Bounds = new Rectangle(pad, currentY, availWidth, midH);
                _lblSeeAll.Location = new Point(availWidth - _lblSeeAll.Width - 18, 16);
                _recentListPanel.Bounds = new Rectangle(18, 44, availWidth - 36, midH - 52);
                currentY += midH + gap;

                _overdueCard.Bounds = new Rectangle(pad, currentY, availWidth, 210);
                _overdueListPanel.Bounds = new Rectangle(18, 62, availWidth - 36, 140);
                currentY += 210 + gap;
            }

            // Bottom Section: Books borrowed this week & Quick actions
            if (availWidth >= 860)
            {
                int chartW = (int)(availWidth * 0.65f) - (gap / 2);
                int quickW = availWidth - chartW - gap;

                _chartCard.Bounds = new Rectangle(pad, currentY, chartW, botH);
                _quickCard.Bounds = new Rectangle(pad + chartW + gap, currentY, quickW, botH);

                _barChartPanel.Bounds = new Rectangle(18, 54, chartW - 36, botH - 62);

                // Quick action buttons grow with the card
                int qBtnW = quickW - 36;
                int qGap = 8;
                int qH = Math.Clamp((botH - 44 - 18 - qGap * 3) / 4, 32, 60);
                var qButtons = new[] { _btnIssue, _btnReturn, _btnAddBook, _btnAddBorrower };
                for (int i = 0; i < qButtons.Length; i++)
                    qButtons[i].Bounds = new Rectangle(18, 44 + i * (qH + qGap), qBtnW, qH);

                currentY += botH + 20;
            }
            else
            {
                // Stack vertically on mobile
                _chartCard.Bounds = new Rectangle(pad, currentY, availWidth, botH);
                _barChartPanel.Bounds = new Rectangle(18, 54, availWidth - 36, botH - 62);
                currentY += botH + gap;

                _quickCard.Bounds = new Rectangle(pad, currentY, availWidth, 210);
                int qBtnW = availWidth - 36;
                _btnIssue.Bounds = new Rectangle(18, 44, qBtnW, 34);
                _btnReturn.Bounds = new Rectangle(18, 84, qBtnW, 32);
                _btnAddBook.Bounds = new Rectangle(18, 122, qBtnW, 32);
                _btnAddBorrower.Bounds = new Rectangle(18, 160, qBtnW, 32);
                currentY += 210 + 20;
            }

            AutoScrollMinSize = new Size(0, currentY);
            ResumeLayout();

            RefreshRecentTableWidth(_recentListPanel.Width);
            _barChartPanel.Invalidate();
        }

        private void ResizeStatCardLabels(int cardWidth)
        {
            int w = cardWidth - 32;
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
                Font = new Font("Segoe UI Emoji", 14f),
                BackColor = UITheme.SkyBlueLight,
                Size = new Size(38, 38),
                Location = new Point(18, 14),
                TextAlign = ContentAlignment.MiddleCenter
            };

            valueLabel = new Label
            {
                Text = value,
                Font = new Font("Segoe UI", 20f, FontStyle.Bold),
                ForeColor = UITheme.NavyDark,
                Location = new Point(16, 48),
                Height = 38,
                AutoEllipsis = true
            };

            descLabel = new Label
            {
                Text = title,
                Font = UITheme.SmallFont,
                ForeColor = UITheme.TextMuted,
                Location = new Point(18, 88),
                Height = 20,
                AutoEllipsis = true
            };

            card.Controls.AddRange(new Control[] { iconCircle, valueLabel, descLabel });
            return card;
        }

        private int RecentTableWidth() =>
            Math.Max(240, _recentListPanel.ClientSize.Width - SystemInformation.VerticalScrollBarWidth);

        // Column widths for the recent-transactions table. Very narrow (phone) widths drop the DUE column.
        private static (int borrower, int book, int due, int status) GetRecentColumns(int totalW)
        {
            if (totalW >= 420)
            {
                int b = (int)(totalW * 0.26f);
                int bk = (int)(totalW * 0.40f);
                int d = (int)(totalW * 0.16f);
                return (b, bk, d, Math.Max(78, totalW - b - bk - d - 10));
            }
            int b2 = (int)(totalW * 0.34f);
            int st2 = 74;
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
                    row.Controls[3].Bounds = new Rectangle(borrowerW + bookW + dueW, row.Controls[3].Top, row.Top == 0 ? statusW : Math.Min(84, statusW), row.Controls[3].Height);
                }
            }
        }

        private void LoadDashboardData()
        {
            // Recent Transactions List
            _recentListPanel.Controls.Clear();

            int totalW = RecentTableWidth();
            var (borrowerW, bookW, dueW, statusW) = GetRecentColumns(totalW);

            var headerRow = new Panel { Size = new Size(totalW, 24), Location = new Point(0, 0) };
            headerRow.Controls.Add(new Label { Text = "BORROWER", Font = UITheme.SmallFont, ForeColor = UITheme.TextMuted, Location = new Point(0, 4), Size = new Size(borrowerW, 20) });
            headerRow.Controls.Add(new Label { Text = "BOOK", Font = UITheme.SmallFont, ForeColor = UITheme.TextMuted, Location = new Point(borrowerW, 4), Size = new Size(bookW, 20) });
            headerRow.Controls.Add(new Label { Text = "DUE", Font = UITheme.SmallFont, ForeColor = UITheme.TextMuted, Location = new Point(borrowerW + bookW, 4), Size = new Size(dueW, 20) });
            headerRow.Controls.Add(new Label { Text = "STATUS", Font = UITheme.SmallFont, ForeColor = UITheme.TextMuted, Location = new Point(borrowerW + bookW + dueW, 4), Size = new Size(statusW, 20) });
            _recentListPanel.Controls.Add(headerRow);

            int y = 28;
            foreach (var tx in _service.Transactions.Take(8))
            {
                var row = new Panel { Size = new Size(totalW, 34), Location = new Point(0, y) };
                var lblBorrower = new Label { Text = tx.BorrowerName, Font = UITheme.BodyBold, ForeColor = UITheme.TextMain, Location = new Point(0, 6), Size = new Size(borrowerW, 20), AutoEllipsis = true };
                var lblBook = new Label { Text = tx.BookTitle, Font = UITheme.BodyFont, ForeColor = UITheme.TextMain, Location = new Point(borrowerW, 6), Size = new Size(bookW, 20), AutoEllipsis = true };
                var lblDue = new Label { Text = tx.DueDate.ToString("MMM dd"), Font = UITheme.BodyFont, ForeColor = UITheme.TextMuted, Location = new Point(borrowerW + bookW, 6), Size = new Size(dueW, 20) };

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

                var lblStatus = new Label
                {
                    Text = tx.Status,
                    Font = new Font("Segoe UI", 7.5f, FontStyle.Bold),
                    BackColor = statusBg,
                    ForeColor = statusFg,
                    Location = new Point(borrowerW + bookW + dueW, 5),
                    Size = new Size(Math.Min(84, statusW), 22),
                    TextAlign = ContentAlignment.MiddleCenter
                };

                row.Controls.AddRange(new Control[] { lblBorrower, lblBook, lblDue, lblStatus });
                _recentListPanel.Controls.Add(row);
                y += 35;
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
                    Font = UITheme.BodyFont,
                    ForeColor = UITheme.TextMuted,
                    Location = new Point(0, 8),
                    AutoSize = true
                });
            }
            foreach (var ov in overdues)
            {
                var ovRow = new CardPanel
                {
                    Location = new Point(0, oy),
                    Size = new Size(Math.Max(240, _overdueListPanel.Width - 10), 72),
                    BorderRadius = 10,
                    BackgroundFill = Color.FromArgb(249, 252, 255),
                    Anchor = AnchorStyles.Left | AnchorStyles.Top | AnchorStyles.Right
                };

                var avatar = new PictureBox
                {
                    Location = new Point(8, 12),
                    Size = new Size(42, 42),
                    Image = UITheme.GenerateAvatar(GetInitials(ov.BorrowerName), 42)
                };

                var lblName = new Label
                {
                    Text = ov.BorrowerName,
                    Font = UITheme.BodyBold,
                    ForeColor = UITheme.NavyDark,
                    Location = new Point(56, 8),
                    Size = new Size(ovRow.Width - 150, 18),
                    AutoEllipsis = true,
                    Anchor = AnchorStyles.Left | AnchorStyles.Top | AnchorStyles.Right
                };

                var lblLate = new Label
                {
                    Text = $"{ov.DaysLate} days late • {ov.BookTitle}",
                    Font = UITheme.SmallFont,
                    ForeColor = UITheme.TextMuted,
                    Location = new Point(56, 28),
                    Size = new Size(ovRow.Width - 150, 36),
                    AutoEllipsis = true,
                    Anchor = AnchorStyles.Left | AnchorStyles.Top | AnchorStyles.Right
                };

                var lblFine = new Label
                {
                    Text = $"₱{ov.FineAmount:N2}",
                    Font = new Font("Segoe UI", 11f, FontStyle.Bold),
                    ForeColor = UITheme.NavyDark,
                    Location = new Point(ovRow.Width - 85, 14),
                    Size = new Size(80, 24),
                    TextAlign = ContentAlignment.TopRight,
                    Anchor = AnchorStyles.Top | AnchorStyles.Right
                };

                ovRow.Controls.AddRange(new Control[] { avatar, lblName, lblLate, lblFine });
                _overdueListPanel.Controls.Add(ovRow);
                oy += 78;
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

            string[] days = { "Mon", "Tue", "Wed", "Thu", "Fri", "Sat", "Sun" };
            int[] values = { 14, 22, 18, 30, 26, 9, 5 };
            int maxVal = 35;

            int panelW = _barChartPanel.Width;
            int panelH = _barChartPanel.Height;
            if (panelW < 100 || panelH < 50) return;

            int chartHeight = panelH - 45;
            int step = panelW / 7;
            int barWidth = Math.Max(18, Math.Min(42, step - 14));

            for (int i = 0; i < 7; i++)
            {
                int val = values[i];
                int bh = (int)((float)val / maxVal * chartHeight);
                int bx = i * step + (step - barWidth) / 2;
                int by = chartHeight - bh + 14;

                // Value label on top
                using (var valFont = new Font("Segoe UI", 8.5f, FontStyle.Bold))
                using (var valBrush = new SolidBrush(UITheme.NavyDark))
                {
                    var sf = new StringFormat { Alignment = StringAlignment.Center };
                    g.DrawString(val.ToString(), valFont, valBrush, bx + barWidth / 2, by - 16, sf);
                }

                // Bar
                var rect = new Rectangle(bx, by, barWidth, bh);
                using (var path = UITheme.GetRoundedRectanglePath(rect, 8))
                using (var barBrush = new SolidBrush(UITheme.SkyBlue))
                {
                    g.FillPath(barBrush, path);
                }

                // Day label
                using (var dayFont = new Font("Segoe UI", 8f, FontStyle.Regular))
                using (var dayBrush = new SolidBrush(UITheme.TextMuted))
                {
                    var sf = new StringFormat { Alignment = StringAlignment.Center };
                    g.DrawString(days[i], dayFont, dayBrush, bx + barWidth / 2, chartHeight + 18, sf);
                }
            }
        }
    }
}
