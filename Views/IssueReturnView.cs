using BookHaven.Models;
using BookHaven.Services;
using BookHaven.UI;

namespace BookHaven.Views
{
    public class IssueReturnView : UserControl
    {
        private readonly LibraryDataService _service = LibraryDataService.Instance;

        private Panel _contentPanel = null!;
        private Label _lblTitle = null!;

        // Cards
        private CardPanel _issueCard = null!;
        private CardPanel _returnCard = null!;
        private CardPanel _actCard = null!;

        // Issue controls
        private TextBox _txtBorrowerId = null!;
        private Label _lblBorrowerPreview = null!;
        private TextBox _txtBookIsbn = null!;
        private Label _lblBookPreview = null!;
        private DateTimePicker _dtpIssueDate = null!;
        private DateTimePicker _dtpDueDate = null!;
        private PillButton _btnIssue = null!;

        // Return controls
        private TextBox _txtReturnQuery = null!;
        private CardPanel _returnDetailsCard = null!;
        private Label _lblReturnBookTitle = null!;
        private Label _lblReturnBorrower = null!;
        private Label _lblReturnIssued = null!;
        private Label _lblReturnDue = null!;
        private Label _lblReturnReturned = null!;
        private Label _lblReturnFineCalc = null!;
        private Label _lblReturnFineTotal = null!;
        private PillButton _btnConfirmReturn = null!;
        private BorrowTransaction? _currentReturnTx;

        // Activity log panel
        private Panel _activityPanel = null!;

        public IssueReturnView()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            BackColor = UITheme.AppBackground;
            Dock = DockStyle.Fill;
            AutoScroll = true;

            _contentPanel = new Panel
            {
                Location = new Point(0, 0),
                BackColor = Color.Transparent,
                AutoScroll = false
            };
            Controls.Add(_contentPanel);

            // Title
            _lblTitle = new Label
            {
                Text = "ISSUE & RETURN!",
                Font = new Font("Segoe UI", 18f, FontStyle.Bold),
                ForeColor = UITheme.NavyDark,
                AutoSize = true,
                UseMnemonic = false
            };
            _contentPanel.Controls.Add(_lblTitle);

            // 1. Issue a book Card
            _issueCard = new CardPanel { BorderRadius = 16, Padding = new Padding(22) };

            var lblIssueHeader = new Label
            {
                Text = "📤  Issue a book",
                Font = UITheme.SubHeaderFont,
                ForeColor = UITheme.NavyDark,
                Location = new Point(20, 14),
                AutoSize = true
            };
            _issueCard.Controls.Add(lblIssueHeader);

            _issueCard.Controls.Add(new Label { Text = "Borrower ID", Font = UITheme.SmallFont, ForeColor = UITheme.TextMuted, Location = new Point(20, 44), AutoSize = true });
            _txtBorrowerId = new TextBox { Text = "BH-M-0142", Location = new Point(20, 64), Font = UITheme.BodyFont };
            _txtBorrowerId.TextChanged += (s, e) => UpdateBorrowerPreview();
            _issueCard.Controls.Add(_txtBorrowerId);

            _lblBorrowerPreview = new Label
            {
                Text = "👤  Aira Mendoza • 2 of 3 loans used",
                BackColor = Color.FromArgb(240, 246, 255),
                ForeColor = UITheme.NavyDark,
                Font = UITheme.BodyBold,
                Location = new Point(20, 98),
                Height = 32,
                TextAlign = ContentAlignment.MiddleLeft,
                Padding = new Padding(8, 0, 0, 0)
            };
            _issueCard.Controls.Add(_lblBorrowerPreview);

            _issueCard.Controls.Add(new Label { Text = "Book ISBN", Font = UITheme.SmallFont, ForeColor = UITheme.TextMuted, Location = new Point(20, 136), AutoSize = true });
            _txtBookIsbn = new TextBox { Text = "978-0-13-235088-4", Location = new Point(20, 154), Font = UITheme.BodyFont };
            _txtBookIsbn.TextChanged += (s, e) => UpdateBookPreview();
            _issueCard.Controls.Add(_txtBookIsbn);

            _lblBookPreview = new Label
            {
                Text = "📖  Clean Code • 3 copies available",
                BackColor = Color.FromArgb(240, 253, 244),
                ForeColor = UITheme.StatusGreen,
                Font = UITheme.BodyBold,
                Location = new Point(20, 188),
                Height = 32,
                TextAlign = ContentAlignment.MiddleLeft,
                Padding = new Padding(8, 0, 0, 0)
            };
            _issueCard.Controls.Add(_lblBookPreview);

            _issueCard.Controls.Add(new Label { Text = "Issue date", Font = UITheme.SmallFont, ForeColor = UITheme.TextMuted, Location = new Point(20, 226), AutoSize = true });
            _dtpIssueDate = new DateTimePicker { Location = new Point(20, 246), Value = DateTime.Now, Format = DateTimePickerFormat.Short };
            _dtpIssueDate.ValueChanged += (s, e) => _dtpDueDate.Value = _dtpIssueDate.Value.AddDays(_service.Settings.LoanPeriodDays);
            _issueCard.Controls.Add(_dtpIssueDate);

            _issueCard.Controls.Add(new Label { Text = $"Due date ({_service.Settings.LoanPeriodDays} days)", Font = UITheme.SmallFont, ForeColor = UITheme.TextMuted, Location = new Point(240, 226), AutoSize = true });
            _dtpDueDate = new DateTimePicker { Location = new Point(240, 246), Value = DateTime.Now.AddDays(_service.Settings.LoanPeriodDays), Format = DateTimePickerFormat.Short };
            _issueCard.Controls.Add(_dtpDueDate);

            _btnIssue = new PillButton
            {
                Text = "Issue book",
                CustomBackColor = UITheme.HotPink,
                CustomForeColor = Color.White,
                Location = new Point(20, 288),
                Height = 40
            };
            _btnIssue.Click += HandleIssueBook;
            _issueCard.Controls.Add(_btnIssue);
            _contentPanel.Controls.Add(_issueCard);

            // 2. Return a book Card
            _returnCard = new CardPanel { BorderRadius = 16, Padding = new Padding(22) };

            var lblReturnHeader = new Label
            {
                Text = "📥  Return a book",
                Font = UITheme.SubHeaderFont,
                ForeColor = UITheme.NavyDark,
                Location = new Point(20, 14),
                AutoSize = true
            };
            _returnCard.Controls.Add(lblReturnHeader);

            _returnCard.Controls.Add(new Label { Text = "Transaction ID or ISBN", Font = UITheme.SmallFont, ForeColor = UITheme.TextMuted, Location = new Point(20, 44), AutoSize = true });
            _txtReturnQuery = new TextBox { Text = "TX-2026-0315", Location = new Point(20, 64), Font = UITheme.BodyFont };
            _txtReturnQuery.TextChanged += (s, e) => UpdateReturnDetails();
            _returnCard.Controls.Add(_txtReturnQuery);

            _returnDetailsCard = new CardPanel
            {
                Location = new Point(20, 98),
                Height = 175,
                BorderRadius = 12,
                BackgroundFill = Color.FromArgb(249, 252, 255)
            };

            int ry = 8;
            _returnDetailsCard.Controls.Add(new Label { Text = "Book", Font = UITheme.SmallFont, ForeColor = UITheme.TextMuted, Location = new Point(14, ry), AutoSize = true });
            _lblReturnBookTitle = new Label { Text = "Database System Concepts", Font = UITheme.BodyBold, ForeColor = UITheme.NavyDark, Location = new Point(90, ry), Size = new Size(300, 18), AutoEllipsis = true };
            _returnDetailsCard.Controls.Add(_lblReturnBookTitle);

            ry += 22;
            _returnDetailsCard.Controls.Add(new Label { Text = "Borrower", Font = UITheme.SmallFont, ForeColor = UITheme.TextMuted, Location = new Point(14, ry), AutoSize = true });
            _lblReturnBorrower = new Label { Text = "Jun Dela Cruz", Font = UITheme.BodyFont, ForeColor = UITheme.TextMain, Location = new Point(90, ry), Size = new Size(300, 18) };
            _returnDetailsCard.Controls.Add(_lblReturnBorrower);

            ry += 22;
            _returnDetailsCard.Controls.Add(new Label { Text = "Issued", Font = UITheme.SmallFont, ForeColor = UITheme.TextMuted, Location = new Point(14, ry), AutoSize = true });
            _lblReturnIssued = new Label { Text = "Nov 8, 2026", Font = UITheme.BodyFont, ForeColor = UITheme.TextMain, Location = new Point(90, ry), Size = new Size(110, 18) };
            _returnDetailsCard.Controls.Add(_lblReturnIssued);

            _returnDetailsCard.Controls.Add(new Label { Text = "Due", Font = UITheme.SmallFont, ForeColor = UITheme.TextMuted, Location = new Point(210, ry), AutoSize = true });
            _lblReturnDue = new Label { Text = "Nov 15, 2026", Font = UITheme.BodyFont, ForeColor = UITheme.TextMain, Location = new Point(250, ry), Size = new Size(110, 18) };
            _returnDetailsCard.Controls.Add(_lblReturnDue);

            ry += 22;
            _returnDetailsCard.Controls.Add(new Label { Text = "Returned", Font = UITheme.SmallFont, ForeColor = UITheme.TextMuted, Location = new Point(14, ry), AutoSize = true });
            _lblReturnReturned = new Label { Text = DateTime.Now.ToString("MMM dd, yyyy"), Font = UITheme.BodyFont, ForeColor = UITheme.TextMain, Location = new Point(90, ry), Size = new Size(180, 18) };
            _returnDetailsCard.Controls.Add(_lblReturnReturned);

            ry += 32;
            _lblReturnFineCalc = new Label { Text = "7 days late × ₱5.00", Font = UITheme.BodyBold, ForeColor = UITheme.TextMuted, Location = new Point(14, ry + 6), AutoSize = true };
            _lblReturnFineTotal = new Label
            {
                Text = "₱35.00",
                Font = new Font("Segoe UI", 12f, FontStyle.Bold),
                ForeColor = UITheme.StatusRed,
                BackColor = UITheme.StatusRedBg,
                Location = new Point(290, ry),
                Size = new Size(95, 30),
                TextAlign = ContentAlignment.MiddleCenter
            };
            _returnDetailsCard.Controls.AddRange(new Control[] { _lblReturnFineCalc, _lblReturnFineTotal });
            _returnCard.Controls.Add(_returnDetailsCard);

            _btnConfirmReturn = new PillButton
            {
                Text = "Confirm return & collect fine",
                CustomBackColor = UITheme.HotPink,
                CustomForeColor = Color.White,
                Location = new Point(20, 290),
                Height = 42
            };
            _btnConfirmReturn.Click += HandleConfirmReturn;
            _returnCard.Controls.Add(_btnConfirmReturn);
            _contentPanel.Controls.Add(_returnCard);

            // 3. Today's Activity Card
            _actCard = new CardPanel { BorderRadius = 16, Padding = new Padding(22) };

            var lblActHeader = new Label
            {
                Text = "Today's activity",
                Font = UITheme.SubHeaderFont,
                ForeColor = UITheme.NavyDark,
                Location = new Point(20, 16),
                AutoSize = true
            };
            var lblActSub = new Label
            {
                Text = "Recent issue, return, and fine events",
                Font = UITheme.SmallFont,
                ForeColor = UITheme.TextMuted,
                Location = new Point(20, 40),
                AutoSize = true
            };

            _activityPanel = new Panel { Location = new Point(20, 68), Height = 160, AutoScroll = true };
            _actCard.Controls.AddRange(new Control[] { lblActHeader, lblActSub, _activityPanel });
            _contentPanel.Controls.Add(_actCard);

            Resize += (s, e) => PerformResponsiveLayout();
            UpdateBorrowerPreview();
            UpdateBookPreview();
            UpdateReturnDetails();
            RenderActivities();
            PerformResponsiveLayout();
        }

        private void PerformResponsiveLayout()
        {
            _contentPanel.SuspendLayout();

            int pad = 24;
            int gap = 16;
            int availWidth = Math.Max(320, ClientSize.Width - (pad * 2));

            _contentPanel.Location = new Point(0, 0);
            _contentPanel.Width = ClientSize.Width;

            _lblTitle.Location = new Point(pad, 16);

            int currentY = 58;
            int cardH = 350;

            if (availWidth >= 860)
            {
                int cardW = (availWidth - gap) / 2;
                _issueCard.Bounds = new Rectangle(pad, currentY, cardW, cardH);
                _returnCard.Bounds = new Rectangle(pad + cardW + gap, currentY, cardW, cardH);

                ResizeCardControls(_issueCard, cardW);
                ResizeCardControls(_returnCard, cardW);

                currentY += cardH + gap;
            }
            else
            {
                _issueCard.Bounds = new Rectangle(pad, currentY, availWidth, cardH);
                ResizeCardControls(_issueCard, availWidth);
                currentY += cardH + gap;

                _returnCard.Bounds = new Rectangle(pad, currentY, availWidth, cardH);
                ResizeCardControls(_returnCard, availWidth);
                currentY += cardH + gap;
            }

            // Activity card 100% width
            _actCard.Bounds = new Rectangle(pad, currentY, availWidth, 240);
            _activityPanel.Width = availWidth - 40;
            RenderActivities();
            currentY += 240 + 30;

            _contentPanel.Height = currentY;
            _contentPanel.ResumeLayout();
        }

        private void ResizeCardControls(CardPanel card, int width)
        {
            int innerW = width - 40;
            if (card == _issueCard)
            {
                _txtBorrowerId.Width = innerW;
                _lblBorrowerPreview.Width = innerW;
                _txtBookIsbn.Width = innerW;
                _lblBookPreview.Width = innerW;
                int dateW = (innerW - 16) / 2;
                _dtpIssueDate.Width = dateW;
                _dtpDueDate.Location = new Point(20 + dateW + 16, 246);
                _dtpDueDate.Width = dateW;
                _btnIssue.Width = innerW;
            }
            else if (card == _returnCard)
            {
                _txtReturnQuery.Width = innerW;
                _returnDetailsCard.Width = innerW;
                _lblReturnFineTotal.Location = new Point(innerW - 105, 126);
                _btnConfirmReturn.Width = innerW;
            }
        }

        public void PreloadIsbn(string isbn)
        {
            _txtBookIsbn.Text = isbn;
            UpdateBookPreview();
        }

        private void UpdateBorrowerPreview()
        {
            string id = _txtBorrowerId.Text.Trim();
            var member = _service.Members.FirstOrDefault(m =>
                m.MemberID.Equals(id, StringComparison.OrdinalIgnoreCase) ||
                m.StudentID.Equals(id, StringComparison.OrdinalIgnoreCase));

            if (member != null)
            {
                _lblBorrowerPreview.Text = $"👤  {member.FullName} • {member.CurrentLoansCount} of {_service.Settings.MaxBooksPerBorrower} loans used";
                _lblBorrowerPreview.ForeColor = UITheme.NavyDark;
                _lblBorrowerPreview.BackColor = Color.FromArgb(240, 246, 255);
            }
            else
            {
                _lblBorrowerPreview.Text = "⚠️  Borrower not found";
                _lblBorrowerPreview.ForeColor = Color.Red;
                _lblBorrowerPreview.BackColor = Color.FromArgb(254, 242, 242);
            }
        }

        private void UpdateBookPreview()
        {
            string isbn = _txtBookIsbn.Text.Trim();
            var book = _service.Books.FirstOrDefault(b => b.ISBN.Equals(isbn, StringComparison.OrdinalIgnoreCase));

            if (book != null)
            {
                if (book.AvailableCopies > 0)
                {
                    _lblBookPreview.Text = $"📖  {book.Title} • {book.AvailableCopies} copies available";
                    _lblBookPreview.ForeColor = UITheme.StatusGreen;
                    _lblBookPreview.BackColor = UITheme.StatusGreenBg;
                }
                else
                {
                    _lblBookPreview.Text = $"❌  {book.Title} • All copies borrowed";
                    _lblBookPreview.ForeColor = UITheme.StatusRed;
                    _lblBookPreview.BackColor = UITheme.StatusRedBg;
                }
            }
            else
            {
                _lblBookPreview.Text = "⚠️  Book ISBN not found in catalog";
                _lblBookPreview.ForeColor = Color.Red;
                _lblBookPreview.BackColor = Color.FromArgb(254, 242, 242);
            }
        }

        private void UpdateReturnDetails()
        {
            string q = _txtReturnQuery.Text.Trim();
            _currentReturnTx = _service.Transactions.FirstOrDefault(t =>
                t.Status != "Returned" &&
                (t.TransactionID.Equals(q, StringComparison.OrdinalIgnoreCase) ||
                 t.BookISBN.Equals(q, StringComparison.OrdinalIgnoreCase)));

            if (_currentReturnTx != null)
            {
                _lblReturnBookTitle.Text = _currentReturnTx.BookTitle;
                _lblReturnBorrower.Text = _currentReturnTx.BorrowerName;
                _lblReturnIssued.Text = _currentReturnTx.IssueDate.ToString("MMM dd, yyyy");
                _lblReturnDue.Text = _currentReturnTx.DueDate.ToString("MMM dd, yyyy");
                _lblReturnReturned.Text = DateTime.Now.ToString("MMM dd, yyyy");

                int lateDays = Math.Max(0, (int)(DateTime.Now.Date - _currentReturnTx.DueDate.Date).TotalDays);
                decimal fine = lateDays * _service.Settings.FinePerDayLate;

                if (lateDays > 0)
                {
                    _lblReturnFineCalc.Text = $"{lateDays} days late × ₱{_service.Settings.FinePerDayLate:F2}";
                    _lblReturnFineTotal.Text = $"₱{fine:N2}";
                    _lblReturnFineTotal.Visible = true;
                }
                else
                {
                    _lblReturnFineCalc.Text = "On time (No late fees)";
                    _lblReturnFineTotal.Text = "₱0.00";
                    _lblReturnFineTotal.Visible = false;
                }
            }
            else
            {
                _lblReturnBookTitle.Text = "No active loan found for this ID/ISBN";
                _lblReturnBorrower.Text = "-";
                _lblReturnIssued.Text = "-";
                _lblReturnDue.Text = "-";
                _lblReturnReturned.Text = "-";
                _lblReturnFineCalc.Text = "-";
                _lblReturnFineTotal.Visible = false;
            }
        }

        private void HandleIssueBook(object? sender, EventArgs e)
        {
            string msg = _service.IssueBook(_txtBorrowerId.Text, _txtBookIsbn.Text);
            if (msg.StartsWith("Error"))
            {
                MessageBox.Show(msg, "Issue Book", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            else
            {
                MessageBox.Show(msg, "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                UpdateBorrowerPreview();
                UpdateBookPreview();
                RenderActivities();
            }
        }

        private void HandleConfirmReturn(object? sender, EventArgs e)
        {
            var (success, msg, fine) = _service.ReturnBook(_txtReturnQuery.Text);
            if (!success)
            {
                MessageBox.Show(msg, "Return Book", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            else
            {
                MessageBox.Show(msg, "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                UpdateReturnDetails();
                RenderActivities();
            }
        }

        private void RenderActivities()
        {
            _activityPanel.Controls.Clear();
            int y = 0;

            foreach (var act in _service.Activities.Take(6))
            {
                var row = new Panel { Size = new Size(Math.Max(300, _activityPanel.Width - 10), 30), Location = new Point(0, y) };

                var lblTime = new Label { Text = act.TimeText, Font = UITheme.BodyBold, ForeColor = UITheme.NavyDark, Location = new Point(0, 5), Size = new Size(75, 20) };
                var lblDesc = new Label { Text = act.Description, Font = UITheme.BodyFont, ForeColor = UITheme.TextMain, Location = new Point(80, 5), Size = new Size(Math.Max(120, row.Width - 210), 20), AutoEllipsis = true };

                Color tagBg = act.ActionType switch
                {
                    "Returned" => UITheme.StatusGreenBg,
                    "Issued" => UITheme.StatusBlueBg,
                    "Reserved" => UITheme.SkyBlueLight,
                    _ => UITheme.StatusYellowBg
                };
                Color tagFg = act.ActionType switch
                {
                    "Returned" => UITheme.StatusGreen,
                    "Issued" => UITheme.StatusBlue,
                    "Reserved" => UITheme.NavyDark,
                    _ => Color.FromArgb(180, 100, 0)
                };

                var lblTag = new Label
                {
                    Text = act.TagText,
                    Font = new Font("Segoe UI", 7.5f, FontStyle.Bold),
                    BackColor = tagBg,
                    ForeColor = tagFg,
                    Location = new Point(row.Width - 115, 4),
                    Size = new Size(110, 22),
                    TextAlign = ContentAlignment.MiddleCenter
                };

                row.Controls.AddRange(new Control[] { lblTime, lblDesc, lblTag });
                _activityPanel.Controls.Add(row);
                y += 32;
            }
        }
    }
}
