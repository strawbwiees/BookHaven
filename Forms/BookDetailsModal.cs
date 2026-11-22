using BookHaven.Models;
using BookHaven.Services;
using BookHaven.UI;

namespace BookHaven.Forms
{
    public class BookDetailsModal : Form
    {
        private readonly Book _book;
        public event EventHandler<Book>? RequestEdit;
        public event EventHandler<Book>? RequestIssue;

        public BookDetailsModal(Book book)
        {
            _book = book;
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            Text = $"BookHaven - {_book.Title}";
            Size = new Size(720, 560);
            StartPosition = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            BackColor = UITheme.AppBackground;

            // Main container card
            var mainCard = new CardPanel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(24),
                BorderRadius = 16
            };
            Controls.Add(mainCard);

            // Left side: Book Cover Preview
            var coverBox = new PictureBox
            {
                Size = new Size(190, 260),
                Location = new Point(24, 24),
                SizeMode = PictureBoxSizeMode.Zoom,
                Image = UITheme.GetBookCoverImage(_book, 190, 260)
            };
            mainCard.Controls.Add(coverBox);

            // Copies summary under cover
            var copyCard = new CardPanel
            {
                Location = new Point(24, 296),
                Size = new Size(190, 110),
                BorderRadius = 10,
                BackgroundFill = Color.FromArgb(245, 249, 255)
            };
            var lblCopiesTitle = new Label
            {
                Text = "INVENTORY",
                Font = UITheme.SmallFont,
                ForeColor = UITheme.TextMuted,
                Location = new Point(12, 10),
                AutoSize = true
            };
            var lblCopiesValue = new Label
            {
                Text = $"{_book.AvailableCopies} / {_book.TotalCopies}",
                Font = new Font("Segoe UI", 16f, FontStyle.Bold),
                ForeColor = UITheme.NavyDark,
                Location = new Point(10, 28),
                AutoSize = true
            };
            var lblCopiesSub = new Label
            {
                Text = "Copies Available",
                Font = UITheme.SmallFont,
                ForeColor = UITheme.TextMuted,
                Location = new Point(12, 60),
                AutoSize = true
            };
            var lblLocation = new Label
            {
                Text = $"📍 {_book.ShelfLocation}",
                Font = UITheme.SmallFont,
                ForeColor = UITheme.TextMain,
                Location = new Point(12, 80),
                AutoSize = true
            };
            copyCard.Controls.AddRange(new Control[] { lblCopiesTitle, lblCopiesValue, lblCopiesSub, lblLocation });
            mainCard.Controls.Add(copyCard);

            // Right side details
            int rightX = 236;
            int rightWidth = 440;

            var lblTitle = new Label
            {
                Text = _book.Title,
                Font = new Font("Segoe UI", 16f, FontStyle.Bold),
                ForeColor = UITheme.NavyDark,
                Location = new Point(rightX, 24),
                Size = new Size(rightWidth, 40)
            };
            mainCard.Controls.Add(lblTitle);

            var lblAuthor = new Label
            {
                Text = $"by {_book.Author}",
                Font = new Font("Segoe UI", 11f, FontStyle.Regular),
                ForeColor = UITheme.TextMuted,
                Location = new Point(rightX, 64),
                AutoSize = true
            };
            mainCard.Controls.Add(lblAuthor);

            // Badges row
            var panelBadges = new FlowLayoutPanel
            {
                Location = new Point(rightX, 94),
                Size = new Size(rightWidth, 34),
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents = false
            };

            var badgeCategory = CreatePillLabel(_book.Category, UITheme.SkyBlueLight, UITheme.NavyDark);
            panelBadges.Controls.Add(badgeCategory);

            string status = _book.GetStockStatus();
            Color statusBg = status == "Available" ? UITheme.StatusGreenBg : (status == "Low stock" ? UITheme.StatusYellowBg : UITheme.StatusRedBg);
            Color statusFg = status == "Available" ? UITheme.StatusGreen : (status == "Low stock" ? UITheme.StatusYellow : UITheme.StatusRed);
            var badgeStatus = CreatePillLabel(status, statusBg, statusFg);
            panelBadges.Controls.Add(badgeStatus);

            var badgeIsbn = CreatePillLabel($"ISBN: {_book.ISBN}", Color.FromArgb(240, 243, 248), UITheme.TextMuted);
            panelBadges.Controls.Add(badgeIsbn);

            mainCard.Controls.Add(panelBadges);

            // Details metadata
            var lblMeta = new Label
            {
                Text = $"Publisher: {_book.Publisher}   •   Year: {_book.PublicationYear}",
                Font = UITheme.SmallFont,
                ForeColor = UITheme.TextMuted,
                Location = new Point(rightX, 134),
                AutoSize = true
            };
            mainCard.Controls.Add(lblMeta);

            // Synopsis / Description
            var lblDescHeader = new Label
            {
                Text = "SYNOPSIS & OVERVIEW",
                Font = UITheme.SmallFont,
                ForeColor = UITheme.TextMuted,
                Location = new Point(rightX, 160),
                AutoSize = true
            };
            mainCard.Controls.Add(lblDescHeader);

            var txtDesc = new TextBox
            {
                Text = string.IsNullOrWhiteSpace(_book.Description) ? "No synopsis provided for this title." : _book.Description,
                Font = UITheme.BodyFont,
                ForeColor = UITheme.TextMain,
                BackColor = Color.FromArgb(248, 250, 253),
                BorderStyle = BorderStyle.None,
                Multiline = true,
                ReadOnly = true,
                ScrollBars = ScrollBars.Vertical,
                Location = new Point(rightX, 180),
                Size = new Size(rightWidth, 90)
            };
            mainCard.Controls.Add(txtDesc);

            // Loan Status / Active Borrowers
            var service = LibraryDataService.Instance;
            var activeLoans = service.Transactions
                .Where(t => t.BookISBN == _book.ISBN && t.Status != "Returned")
                .ToList();

            var lblLoanHeader = new Label
            {
                Text = $"CURRENT LOANS ({activeLoans.Count})",
                Font = UITheme.SmallFont,
                ForeColor = UITheme.TextMuted,
                Location = new Point(rightX, 280),
                AutoSize = true
            };
            mainCard.Controls.Add(lblLoanHeader);

            var listLoans = new ListView
            {
                Location = new Point(rightX, 300),
                Size = new Size(rightWidth, 105),
                View = View.Details,
                FullRowSelect = true,
                GridLines = true,
                BorderStyle = BorderStyle.FixedSingle,
                Font = UITheme.SmallFont
            };
            listLoans.Columns.Add("Borrower", 150);
            listLoans.Columns.Add("Issued", 85);
            listLoans.Columns.Add("Due Date", 85);
            listLoans.Columns.Add("Status", 90);

            if (activeLoans.Any())
            {
                foreach (var loan in activeLoans)
                {
                    var item = new ListViewItem(loan.BorrowerName);
                    item.SubItems.Add(loan.IssueDate.ToString("MMM dd"));
                    item.SubItems.Add(loan.DueDate.ToString("MMM dd"));
                    item.SubItems.Add(loan.Status);
                    if (loan.Status == "Overdue") item.ForeColor = Color.Red;
                    listLoans.Items.Add(item);
                }
            }
            else
            {
                var item = new ListViewItem("All loaned copies are currently on shelves.");
                item.SubItems.Add("-");
                item.SubItems.Add("-");
                item.SubItems.Add("Available");
                listLoans.Items.Add(item);
            }
            mainCard.Controls.Add(listLoans);

            // Action Buttons at bottom
            var btnIssue = new PillButton
            {
                Text = "Issue Book",
                CustomBackColor = UITheme.HotPink,
                CustomForeColor = Color.White,
                Location = new Point(rightX, 425),
                Size = new Size(130, 40),
                Enabled = _book.AvailableCopies > 0
            };
            btnIssue.Click += (s, e) =>
            {
                Close();
                RequestIssue?.Invoke(this, _book);
            };
            mainCard.Controls.Add(btnIssue);

            var btnEdit = new PillButton
            {
                Text = "Edit Book",
                CustomBackColor = UITheme.NavyDark,
                CustomForeColor = Color.White,
                Location = new Point(rightX + 140, 425),
                Size = new Size(130, 40)
            };
            btnEdit.Click += (s, e) =>
            {
                Close();
                RequestEdit?.Invoke(this, _book);
            };
            mainCard.Controls.Add(btnEdit);

            var btnClose = new PillButton
            {
                Text = "Close",
                CustomBackColor = Color.White,
                CustomForeColor = UITheme.TextMain,
                CustomBorderColor = UITheme.CardBorder,
                Location = new Point(rightX + 280, 425),
                Size = new Size(100, 40)
            };
            btnClose.Click += (s, e) => Close();
            mainCard.Controls.Add(btnClose);
        }

        private static Label CreatePillLabel(string text, Color bg, Color fg)
        {
            var lbl = new Label
            {
                Text = text,
                BackColor = bg,
                ForeColor = fg,
                Font = new Font("Segoe UI", 8f, FontStyle.Bold),
                AutoSize = true,
                Padding = new Padding(8, 4, 8, 4),
                Margin = new Padding(0, 0, 6, 0)
            };
            return lbl;
        }
    }
}
