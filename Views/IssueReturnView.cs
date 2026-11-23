using BookHaven.Models;
using BookHaven.Services;
using BookHaven.UI;

namespace BookHaven.Views
{
    public class IssueReturnView : UserControl
    {
        private readonly LibraryDataService _service = LibraryDataService.Instance;

        // Larger, crisp fonts
        private static readonly Font PageTitleFont = new("League Spartan", 22f, FontStyle.Bold);
        private static readonly Font CardHeaderFont = new("League Spartan", 15f, FontStyle.Bold);
        private static readonly Font CardSubFont = new("League Spartan", 10f, FontStyle.Regular);
        private static readonly Font FieldLabelFont = new("League Spartan", 10f, FontStyle.Regular);
        private static readonly Font InputFont = new("League Spartan", 11f, FontStyle.Regular);
        private static readonly Font RowBoldFont = new("League Spartan", 10.8f, FontStyle.Bold);
        private static readonly Font RowBodyFont = new("League Spartan", 10.5f, FontStyle.Regular);
        private static readonly Font FineTotalFont = new("League Spartan", 13.5f, FontStyle.Bold);
        private static readonly Font BadgeFont = new("League Spartan", 9.5f, FontStyle.Bold);
        private static readonly Font ButtonFont = new("League Spartan", 11f, FontStyle.Bold);

        private Panel _contentPanel = null!;
        private Label _lblTitle = null!;

        // Cards
        private CardPanel _issueCard = null!;
        private CardPanel _returnCard = null!;
        private CardPanel _actCard = null!;

        // Issue controls
        private TextBox _txtBorrowerId = null!;
        private Label _lblBorrowerPreview = null!;
        private TextBox _txtBookTitle = null!;
        private Label _lblBookPreview = null!;
        private Label _lblDueDate = null!;
        private DateTimePicker _dtpIssueDate = null!;
        private DateTimePicker _dtpDueDate = null!;
        private PillButton _btnIssue = null!;

        // Return controls
        private TextBox _txtReturnQuery = null!;
        private CardPanel _returnDetailsCard = null!;
        private Label _lblReturnBookTitle = null!;
        private Label _lblReturnBorrower = null!;
        private Label _lblReturnIssued = null!;
        private Label _lblReturnDueCaption = null!;
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

        private static int LineHeight(Font font, int extra = 6) =>
            TextRenderer.MeasureText("Ag", font).Height + extra;

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
                Font = PageTitleFont,
                ForeColor = UITheme.NavyDark,
                BackColor = Color.Transparent,
                AutoSize = true,
                UseMnemonic = false
            };
            _contentPanel.Controls.Add(_lblTitle);

            // 1. Issue a book Card
            _issueCard = new CardPanel { BorderRadius = 16, Padding = new Padding(22) };

            var lblIssueHeader = new Label
            {
                Text = "📤  Issue a book",
                Font = CardHeaderFont,
                ForeColor = UITheme.NavyDark,
                BackColor = Color.White,
                Location = new Point(22, 18),
                AutoSize = true
            };
            _issueCard.Controls.Add(lblIssueHeader);

            _issueCard.Controls.Add(new Label { Text = "Borrower ID", Font = FieldLabelFont, ForeColor = UITheme.TextMuted, BackColor = Color.White, Location = new Point(22, 58), AutoSize = true });
            _txtBorrowerId = new TextBox { Text = "BH-M-0142", Location = new Point(22, 82), Font = InputFont };
            _txtBorrowerId.TextChanged += (s, e) => UpdateBorrowerPreview();
            _issueCard.Controls.Add(_txtBorrowerId);

            _lblBorrowerPreview = new Label
            {
                Text = "👤  Aira Mendoza • 2 of 3 loans used",
                BackColor = Color.FromArgb(240, 246, 255),
                ForeColor = UITheme.NavyDark,
                Font = RowBoldFont,
                Location = new Point(22, 124),
                Height = 40,
                TextAlign = ContentAlignment.MiddleLeft,
                Padding = new Padding(10, 0, 0, 0),
                AutoEllipsis = true
            };
            _issueCard.Controls.Add(_lblBorrowerPreview);

            _issueCard.Controls.Add(new Label { Text = "Book title", Font = FieldLabelFont, ForeColor = UITheme.TextMuted, BackColor = Color.White, Location = new Point(22, 174), AutoSize = true });
            _txtBookTitle = new TextBox
            {
                Text = "Clean Code",
                Location = new Point(22, 198),
                Font = InputFont,
                AutoCompleteMode = AutoCompleteMode.SuggestAppend,
                AutoCompleteSource = AutoCompleteSource.CustomSource
            };

            // Show matching book titles while the user types.
            var bookTitleSuggestions = new AutoCompleteStringCollection();
            foreach (var bookTitle in _service.Books
                         .Select(b => b.Title)
                         .Where(title => !string.IsNullOrWhiteSpace(title))
                         .Distinct(StringComparer.OrdinalIgnoreCase)
                         .OrderBy(title => title))
            {
                bookTitleSuggestions.Add(bookTitle);
            }

            _txtBookTitle.AutoCompleteCustomSource = bookTitleSuggestions;
            _txtBookTitle.TextChanged += (s, e) => UpdateBookPreview();
            _issueCard.Controls.Add(_txtBookTitle);

            _lblBookPreview = new Label
            {
                Text = "📖  Clean Code • 3 copies available",
                BackColor = Color.FromArgb(240, 253, 244),
                ForeColor = UITheme.StatusGreen,
                Font = RowBoldFont,
                Location = new Point(22, 240),
                Height = 40,
                TextAlign = ContentAlignment.MiddleLeft,
                Padding = new Padding(10, 0, 0, 0),
                AutoEllipsis = true
            };
            _issueCard.Controls.Add(_lblBookPreview);

            _issueCard.Controls.Add(new Label { Text = "Issue date", Font = FieldLabelFont, ForeColor = UITheme.TextMuted, BackColor = Color.White, Location = new Point(22, 292), AutoSize = true });
            _dtpIssueDate = new DateTimePicker { Location = new Point(22, 316), Font = InputFont, Value = DateTime.Now, Format = DateTimePickerFormat.Short };
            _dtpIssueDate.ValueChanged += (s, e) => _dtpDueDate.Value = _dtpIssueDate.Value.AddDays(_service.Settings.LoanPeriodDays);
            _issueCard.Controls.Add(_dtpIssueDate);

            _lblDueDate = new Label { Text = $"Due date ({_service.Settings.LoanPeriodDays} days)", Font = FieldLabelFont, ForeColor = UITheme.TextMuted, BackColor = Color.White, Location = new Point(240, 292), AutoSize = true };
            _issueCard.Controls.Add(_lblDueDate);

            _dtpDueDate = new DateTimePicker { Location = new Point(240, 316), Font = InputFont, Value = DateTime.Now.AddDays(_service.Settings.LoanPeriodDays), Format = DateTimePickerFormat.Short };
            _issueCard.Controls.Add(_dtpDueDate);

            _btnIssue = new PillButton
            {
                Text = "Issue book",
                Font = ButtonFont,
                CustomBackColor = UITheme.HotPink,
                CustomForeColor = Color.White,
                Location = new Point(22, 376),
                Height = 48
            };
            _btnIssue.Click += HandleIssueBook;
            _issueCard.Controls.Add(_btnIssue);
            _contentPanel.Controls.Add(_issueCard);

            // 2. Return a book Card
            _returnCard = new CardPanel { BorderRadius = 16, Padding = new Padding(22) };

            var lblReturnHeader = new Label
            {
                Text = "📥  Return a book",
                Font = CardHeaderFont,
                ForeColor = UITheme.NavyDark,
                BackColor = Color.White,
                Location = new Point(22, 18),
                AutoSize = true
            };
            _returnCard.Controls.Add(lblReturnHeader);

            _returnCard.Controls.Add(new Label { Text = "Borrower ID / Student ID", Font = FieldLabelFont, ForeColor = UITheme.TextMuted, BackColor = Color.White, Location = new Point(22, 58), AutoSize = true });
            _txtReturnQuery = new TextBox { Text = "BH-M-0142", Location = new Point(22, 82), Font = InputFont };
            _txtReturnQuery.TextChanged += (s, e) => UpdateReturnDetails();
            _returnCard.Controls.Add(_txtReturnQuery);

            Color detailBg = Color.FromArgb(249, 252, 255);
            _returnDetailsCard = new CardPanel
            {
                Location = new Point(22, 126),
                Height = 232,
                BorderRadius = 12,
                BackgroundFill = detailBg
            };

            int rowH = LineHeight(RowBoldFont, 4);
            int ry = 14;
            _returnDetailsCard.Controls.Add(new Label { Text = "Book", Font = FieldLabelFont, ForeColor = UITheme.TextMuted, BackColor = detailBg, Location = new Point(16, ry), Size = new Size(88, rowH), TextAlign = ContentAlignment.MiddleLeft });
            _lblReturnBookTitle = new Label { Text = "Database System Concepts", Font = RowBoldFont, ForeColor = UITheme.NavyDark, BackColor = detailBg, Location = new Point(108, ry), Size = new Size(300, rowH), TextAlign = ContentAlignment.MiddleLeft, AutoEllipsis = true };
            _returnDetailsCard.Controls.Add(_lblReturnBookTitle);

            ry += rowH + 8;
            _returnDetailsCard.Controls.Add(new Label { Text = "Borrower", Font = FieldLabelFont, ForeColor = UITheme.TextMuted, BackColor = detailBg, Location = new Point(16, ry), Size = new Size(88, rowH), TextAlign = ContentAlignment.MiddleLeft });
            _lblReturnBorrower = new Label { Text = "Jun Dela Cruz", Font = RowBodyFont, ForeColor = UITheme.TextMain, BackColor = detailBg, Location = new Point(108, ry), Size = new Size(300, rowH), TextAlign = ContentAlignment.MiddleLeft, AutoEllipsis = true };
            _returnDetailsCard.Controls.Add(_lblReturnBorrower);

            ry += rowH + 8;
            _returnDetailsCard.Controls.Add(new Label { Text = "Issued", Font = FieldLabelFont, ForeColor = UITheme.TextMuted, BackColor = detailBg, Location = new Point(16, ry), Size = new Size(88, rowH), TextAlign = ContentAlignment.MiddleLeft });
            _lblReturnIssued = new Label { Text = "Nov 8, 2026", Font = RowBodyFont, ForeColor = UITheme.TextMain, BackColor = detailBg, Location = new Point(108, ry), Size = new Size(125, rowH), TextAlign = ContentAlignment.MiddleLeft };
            _returnDetailsCard.Controls.Add(_lblReturnIssued);

            _lblReturnDueCaption = new Label { Text = "Due", Font = FieldLabelFont, ForeColor = UITheme.TextMuted, BackColor = detailBg, Location = new Point(236, ry), Size = new Size(42, rowH), TextAlign = ContentAlignment.MiddleLeft };
            _returnDetailsCard.Controls.Add(_lblReturnDueCaption);

            _lblReturnDue = new Label { Text = "Nov 15, 2026", Font = RowBodyFont, ForeColor = UITheme.TextMain, BackColor = detailBg, Location = new Point(282, ry), Size = new Size(125, rowH), TextAlign = ContentAlignment.MiddleLeft };
            _returnDetailsCard.Controls.Add(_lblReturnDue);

            ry += rowH + 8;
            _returnDetailsCard.Controls.Add(new Label { Text = "Returned", Font = FieldLabelFont, ForeColor = UITheme.TextMuted, BackColor = detailBg, Location = new Point(16, ry), Size = new Size(88, rowH), TextAlign = ContentAlignment.MiddleLeft });
            _lblReturnReturned = new Label { Text = DateTime.Now.ToString("MMM dd, yyyy"), Font = RowBodyFont, ForeColor = UITheme.TextMain, BackColor = detailBg, Location = new Point(108, ry), Size = new Size(190, rowH), TextAlign = ContentAlignment.MiddleLeft };
            _returnDetailsCard.Controls.Add(_lblReturnReturned);

            ry += rowH + 16;
            _lblReturnFineCalc = new Label { Text = "7 days late × ₱5.00", Font = RowBoldFont, ForeColor = UITheme.TextMuted, BackColor = detailBg, Location = new Point(16, ry + 4), Height = rowH, AutoSize = true };
            _lblReturnFineTotal = new Label
            {
                Text = "₱35.00",
                Font = FineTotalFont,
                ForeColor = UITheme.StatusRed,
                BackColor = UITheme.StatusRedBg,
                Location = new Point(290, ry),
                Size = new Size(116, 38),
                TextAlign = ContentAlignment.MiddleCenter
            };
            _returnDetailsCard.Controls.AddRange(new Control[] { _lblReturnFineCalc, _lblReturnFineTotal });
            _returnCard.Controls.Add(_returnDetailsCard);

            _btnConfirmReturn = new PillButton
            {
                Text = "Confirm return & collect fine",
                Font = ButtonFont,
                CustomBackColor = UITheme.HotPink,
                CustomForeColor = Color.White,
                Location = new Point(22, 376),
                Height = 48
            };
            _btnConfirmReturn.Click += HandleConfirmReturn;
            _returnCard.Controls.Add(_btnConfirmReturn);
            _contentPanel.Controls.Add(_returnCard);

            // 3. Today's Activity Card
            _actCard = new CardPanel { BorderRadius = 16, Padding = new Padding(22) };

            var lblActHeader = new Label
            {
                Text = "Today's activity",
                Font = CardHeaderFont,
                ForeColor = UITheme.NavyDark,
                BackColor = Color.White,
                Location = new Point(22, 18),
                AutoSize = true
            };
            var lblActSub = new Label
            {
                Text = "Recent issue, return, and fine events",
                Font = CardSubFont,
                ForeColor = UITheme.TextMuted,
                BackColor = Color.White,
                Location = new Point(22, 48),
                AutoSize = true
            };

            _activityPanel = new Panel { Location = new Point(22, 82), Height = 160, AutoScroll = true, BackColor = Color.White };
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

            int pad = ClientSize.Width < 600 ? 14 : 24;
            int gap = 16;
            int availWidth = Math.Max(280, ClientSize.Width - (pad * 2));

            _contentPanel.Location = new Point(0, 0);
            _contentPanel.Width = ClientSize.Width;

            _lblTitle.Location = new Point(pad, 16);

            int currentY = _lblTitle.Bottom + 16;
            int cardH = 448;

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

            // Activity card fills remaining viewport height
            int actH = Math.Max(250, ClientSize.Height - currentY - 24);
            _actCard.Bounds = new Rectangle(pad, currentY, availWidth, actH);
            _activityPanel.Bounds = new Rectangle(22, 82, availWidth - 44, Math.Max(130, actH - 98));
            RenderActivities();
            currentY += actH + 24;

            _contentPanel.Height = currentY;
            AutoScrollMinSize = new Size(0, currentY);
            _contentPanel.ResumeLayout();
        }

        private void ResizeCardControls(CardPanel card, int width)
        {
            int innerW = width - 44;
            if (card == _issueCard)
            {
                _txtBorrowerId.Width = innerW;
                _lblBorrowerPreview.Width = innerW;
                _txtBookTitle.Width = innerW;
                _lblBookPreview.Width = innerW;

                int dateGap = 16;
                int dateW = (innerW - dateGap) / 2;
                _dtpIssueDate.Width = dateW;
                _lblDueDate.Location = new Point(22 + dateW + dateGap, 292);
                _dtpDueDate.Location = new Point(22 + dateW + dateGap, 316);
                _dtpDueDate.Width = dateW;

                _btnIssue.Width = innerW;
            }
            else if (card == _returnCard)
            {
                _txtReturnQuery.Width = innerW;
                _returnDetailsCard.Width = innerW;

                int valW = Math.Max(120, innerW - 124);
                _lblReturnBookTitle.Width = valW;
                _lblReturnBorrower.Width = valW;
                _lblReturnReturned.Width = valW;

                int dueStart = Math.Max(225, innerW / 2 + 6);
                _lblReturnIssued.Width = Math.Max(80, dueStart - 114);
                _lblReturnDueCaption.Left = dueStart;
                _lblReturnDue.Left = dueStart + 46;
                _lblReturnDue.Width = Math.Max(80, innerW - _lblReturnDue.Left - 14);

                _lblReturnFineTotal.Location = new Point(innerW - _lblReturnFineTotal.Width - 16, _lblReturnFineTotal.Top);
                _btnConfirmReturn.Width = innerW;
            }
        }

        public void PreloadBookTitle(string title)
        {
            _txtBookTitle.Text = title;
            UpdateBookPreview();
        }

        // Kept for existing navigation code that still passes an ISBN.
        public void PreloadIsbn(string isbn)
        {
            var book = _service.Books.FirstOrDefault(b =>
                b.ISBN.Equals(isbn.Trim(), StringComparison.OrdinalIgnoreCase));

            _txtBookTitle.Text = book?.Title ?? isbn;
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
            string title = _txtBookTitle.Text.Trim();
            var book = _service.Books.FirstOrDefault(b =>
                b.Title.Equals(title, StringComparison.OrdinalIgnoreCase));

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
                _lblBookPreview.Text = "⚠️  Book title not found in catalog";
                _lblBookPreview.ForeColor = Color.Red;
                _lblBookPreview.BackColor = Color.FromArgb(254, 242, 242);
            }
        }

        private void UpdateReturnDetails()
        {
            string q = _txtReturnQuery.Text.Trim();
            var member = _service.Members.FirstOrDefault(m =>
                m.MemberID.Equals(q, StringComparison.OrdinalIgnoreCase) ||
                m.StudentID.Equals(q, StringComparison.OrdinalIgnoreCase));

            _currentReturnTx = member == null
                ? null
                : _service.Transactions
                    .Where(t => t.Status != "Returned" &&
                                t.BorrowerID.Equals(member.MemberID, StringComparison.OrdinalIgnoreCase))
                    .OrderBy(t => t.IssueDate)
                    .FirstOrDefault();

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
                _lblReturnBookTitle.Text = "No active book found for this borrower ID";
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
            string msg = _service.IssueBook(_txtBorrowerId.Text, _txtBookTitle.Text);
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
            int rowH = 44;
            int textH = LineHeight(RowBoldFont, 4);
            int rowW = Math.Max(240, _activityPanel.ClientSize.Width - SystemInformation.VerticalScrollBarWidth);

            foreach (var act in _service.Activities.Take(12))
            {
                var row = new Panel { Size = new Size(rowW, rowH), Location = new Point(0, y), BackColor = Color.White };
                int textY = (rowH - textH) / 2;

                var lblTime = new Label { Text = act.TimeText, Font = RowBoldFont, ForeColor = UITheme.NavyDark, BackColor = Color.White, Location = new Point(0, textY), Size = new Size(92, textH), TextAlign = ContentAlignment.MiddleLeft };
                var lblDesc = new Label { Text = act.Description, Font = RowBodyFont, ForeColor = UITheme.TextMain, BackColor = Color.White, Location = new Point(98, textY), Size = new Size(Math.Max(120, rowW - 236), textH), TextAlign = ContentAlignment.MiddleLeft, AutoEllipsis = true };

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

                int tagH = 30;
                var lblTag = new Label
                {
                    Text = act.TagText,
                    Font = BadgeFont,
                    BackColor = tagBg,
                    ForeColor = tagFg,
                    Location = new Point(rowW - 128, (rowH - tagH) / 2),
                    Size = new Size(124, tagH),
                    TextAlign = ContentAlignment.MiddleCenter
                };

                row.Controls.AddRange(new Control[] { lblTime, lblDesc, lblTag });
                _activityPanel.Controls.Add(row);
                y += rowH + 2;
            }
        }
    }
}