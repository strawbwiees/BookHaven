using BookHaven.Models;
using BookHaven.Services;
using BookHaven.UI;

namespace BookHaven.Views
{
    public class BorrowersView : UserControl
    {
        private readonly LibraryDataService _service = LibraryDataService.Instance;

        private Panel _contentPanel = null!;
        private Label _lblTitle = null!;

        // Left section
        private CardPanel _searchCard = null!;
        private TextBox _txtSearch = null!;
        private Label _lblCount = null!;
        private FlowLayoutPanel _cardsContainer = null!;

        // Right registration card
        private CardPanel _regCard = null!;
        private TextBox _txtRegName = null!;
        private TextBox _txtRegStudentId = null!;
        private TextBox _txtRegEmail = null!;
        private TextBox _txtRegCourse = null!;
        private PillButton _btnRegister = null!;
        private PillButton _btnClear = null!;

        public BorrowersView()
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
                Text = "OUR BORROWERS!",
                Font = new Font("Segoe UI", 18f, FontStyle.Bold),
                ForeColor = UITheme.NavyDark,
                AutoSize = true
            };
            _contentPanel.Controls.Add(_lblTitle);

            // 1. Search Bar Card
            _searchCard = new CardPanel { BorderRadius = 14 };

            var lblSearch = new Label
            {
                Text = "Find a borrower",
                Font = UITheme.SmallFont,
                ForeColor = UITheme.TextMuted,
                Location = new Point(16, 10),
                AutoSize = true
            };
            _txtSearch = new TextBox
            {
                PlaceholderText = "Name, ID or email...",
                Location = new Point(16, 32),
                Font = UITheme.BodyFont
            };
            _txtSearch.TextChanged += (s, e) => RenderBorrowerCards();

            _lblCount = new Label
            {
                Text = $"{_service.Members.Count} registered",
                Font = new Font("Segoe UI", 8.5f, FontStyle.Bold),
                BackColor = UITheme.SkyBlueLight,
                ForeColor = UITheme.NavyDark,
                Height = 28,
                TextAlign = ContentAlignment.MiddleCenter
            };
            _searchCard.Controls.AddRange(new Control[] { lblSearch, _txtSearch, _lblCount });
            _contentPanel.Controls.Add(_searchCard);

            // 2. Borrower Cards Grid Container
            _cardsContainer = new FlowLayoutPanel
            {
                AutoScroll = true,
                FlowDirection = FlowDirection.LeftToRight,
                BackColor = Color.Transparent
            };
            _contentPanel.Controls.Add(_cardsContainer);

            // 3. Register Borrower Card
            _regCard = new CardPanel { BorderRadius = 16, Padding = new Padding(22) };

            var lblRegTitle = new Label
            {
                Text = "Register a borrower",
                Font = UITheme.SubHeaderFont,
                ForeColor = UITheme.NavyDark,
                Location = new Point(18, 16),
                AutoSize = true
            };
            _regCard.Controls.Add(lblRegTitle);

            int ry = 52;
            _regCard.Controls.Add(new Label { Text = "Full name *", Font = UITheme.SmallFont, ForeColor = UITheme.TextMuted, Location = new Point(18, ry), AutoSize = true });
            _txtRegName = new TextBox { Location = new Point(18, ry + 20), Font = UITheme.BodyFont };
            _regCard.Controls.Add(_txtRegName);

            ry += 58;
            _regCard.Controls.Add(new Label { Text = "Student ID *", Font = UITheme.SmallFont, ForeColor = UITheme.TextMuted, Location = new Point(18, ry), AutoSize = true });
            _txtRegStudentId = new TextBox { Location = new Point(18, ry + 20), Font = UITheme.BodyFont };
            _regCard.Controls.Add(_txtRegStudentId);

            ry += 58;
            _regCard.Controls.Add(new Label { Text = "Email *", Font = UITheme.SmallFont, ForeColor = UITheme.TextMuted, Location = new Point(18, ry), AutoSize = true });
            _txtRegEmail = new TextBox { Location = new Point(18, ry + 20), Font = UITheme.BodyFont };
            _regCard.Controls.Add(_txtRegEmail);

            ry += 58;
            _regCard.Controls.Add(new Label { Text = "Course & year", Font = UITheme.SmallFont, ForeColor = UITheme.TextMuted, Location = new Point(18, ry), AutoSize = true });
            _txtRegCourse = new TextBox { Location = new Point(18, ry + 20), Font = UITheme.BodyFont };
            _regCard.Controls.Add(_txtRegCourse);

            ry += 70;
            _btnRegister = new PillButton
            {
                Text = "Register",
                CustomBackColor = UITheme.NavyDark,
                CustomForeColor = Color.White,
                Location = new Point(18, ry),
                Height = 40
            };
            _btnRegister.Click += HandleRegisterBorrower;

            _btnClear = new PillButton
            {
                Text = "Clear",
                CustomBackColor = Color.White,
                CustomForeColor = UITheme.NavyDark,
                CustomBorderColor = UITheme.CardBorder,
                Height = 40
            };
            _btnClear.Click += (s, e) => ClearRegForm();

            _regCard.Controls.AddRange(new Control[] { _btnRegister, _btnClear });
            _contentPanel.Controls.Add(_regCard);

            Resize += (s, e) => PerformResponsiveLayout();
            RenderBorrowerCards();
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

            int currentY = 56;

            if (availWidth >= 880)
            {
                int leftW = (int)(availWidth * 0.68f) - (gap / 2);
                int rightW = availWidth - leftW - gap;

                _searchCard.Bounds = new Rectangle(pad, currentY, leftW, 76);
                _txtSearch.Width = leftW - 230;
                _lblCount.Location = new Point(leftW - 200, 30);
                _lblCount.Width = 185;

                _cardsContainer.Bounds = new Rectangle(pad, currentY + 88, leftW, 540);
                _regCard.Bounds = new Rectangle(pad + leftW + gap, currentY, rightW, 470);
                ResizeRegControls(rightW);

                currentY += 640;
            }
            else
            {
                // Stack vertically
                _searchCard.Bounds = new Rectangle(pad, currentY, availWidth, 76);
                _txtSearch.Width = Math.Max(120, availWidth - 210);
                _lblCount.Location = new Point(availWidth - 190, 30);
                _lblCount.Width = 175;
                currentY += 88;

                _cardsContainer.Bounds = new Rectangle(pad, currentY, availWidth, 420);
                currentY += 430;

                _regCard.Bounds = new Rectangle(pad, currentY, availWidth, 470);
                ResizeRegControls(availWidth);
                currentY += 480 + 30;
            }

            _contentPanel.Height = currentY;
            _contentPanel.ResumeLayout();
        }

        private void ResizeRegControls(int cardWidth)
        {
            int innerW = cardWidth - 36;
            _txtRegName.Width = innerW;
            _txtRegStudentId.Width = innerW;
            _txtRegEmail.Width = innerW;
            _txtRegCourse.Width = innerW;

            int btnW = (innerW - 12) / 2;
            _btnRegister.Width = btnW;
            _btnClear.Location = new Point(18 + btnW + 12, _btnRegister.Top);
            _btnClear.Width = btnW;
        }

        public void RenderBorrowerCards()
        {
            _cardsContainer.SuspendLayout();
            _cardsContainer.Controls.Clear();

            string q = _txtSearch.Text.Trim().ToLower();
            var filtered = _service.Members.Where(m =>
                string.IsNullOrWhiteSpace(q) ||
                m.FullName.ToLower().Contains(q) ||
                m.MemberID.ToLower().Contains(q) ||
                m.StudentID.ToLower().Contains(q) ||
                m.Email.ToLower().Contains(q)
            ).ToList();

            _lblCount.Text = string.IsNullOrWhiteSpace(q) ? $"{_service.Members.Count} registered" : $"{filtered.Count} found";

            int cardW = Math.Max(260, Math.Min(310, (_cardsContainer.Width - 30) / 2));
            if (_cardsContainer.Width < 560) cardW = Math.Max(260, _cardsContainer.Width - 15);

            foreach (var m in filtered)
            {
                var card = new CardPanel
                {
                    Size = new Size(cardW, 126),
                    BorderRadius = 14,
                    Margin = new Padding(0, 0, 10, 10),
                    BackgroundFill = Color.White,
                    Cursor = Cursors.Hand
                };

                var picAvatar = new PictureBox
                {
                    Location = new Point(12, 14),
                    Size = new Size(44, 44),
                    Image = UITheme.GenerateAvatar(m.Initials, 44)
                };

                var lblName = new Label
                {
                    Text = m.FullName,
                    Font = UITheme.BodyBold,
                    ForeColor = UITheme.NavyDark,
                    Location = new Point(64, 12),
                    Size = new Size(cardW - 74, 18),
                    AutoEllipsis = true
                };

                var lblId = new Label
                {
                    Text = m.MemberID,
                    Font = UITheme.SmallFont,
                    ForeColor = UITheme.TextMuted,
                    Location = new Point(64, 32),
                    Size = new Size(cardW - 74, 16)
                };

                var lblEmail = new Label
                {
                    Text = m.Email,
                    Font = UITheme.SmallFont,
                    ForeColor = UITheme.TextMuted,
                    Location = new Point(12, 64),
                    Size = new Size(cardW - 24, 16),
                    AutoEllipsis = true
                };

                var lblLoans = new Label
                {
                    Text = $"{m.CurrentLoansCount} on loan",
                    Font = UITheme.SmallFont,
                    ForeColor = UITheme.TextMain,
                    Location = new Point(12, 90),
                    AutoSize = true
                };

                bool hasFine = m.OutstandingFines > 0;
                var badgeStatus = new Label
                {
                    Text = hasFine ? "Fine due" : "Active",
                    Font = new Font("Segoe UI", 7.5f, FontStyle.Bold),
                    BackColor = hasFine ? UITheme.StatusRedBg : UITheme.StatusGreenBg,
                    ForeColor = hasFine ? UITheme.StatusRed : UITheme.StatusGreen,
                    Location = new Point(cardW - 84, 86),
                    Size = new Size(72, 22),
                    TextAlign = ContentAlignment.MiddleCenter
                };

                void OpenBorrower(object? s, EventArgs e) => ShowBorrowerDetails(m);
                card.Click += OpenBorrower;
                picAvatar.Click += OpenBorrower;
                lblName.Click += OpenBorrower;

                card.Controls.AddRange(new Control[] { picAvatar, lblName, lblId, lblEmail, lblLoans, badgeStatus });
                _cardsContainer.Controls.Add(card);
            }

            _cardsContainer.ResumeLayout();
        }

        private void ShowBorrowerDetails(Member m)
        {
            var loans = _service.Transactions.Where(t => t.BorrowerID == m.MemberID).ToList();
            string loanDetails = loans.Any()
                ? string.Join("\n", loans.Select(l => $"• {l.BookTitle} ({l.Status}, Due: {l.DueDate:MMM dd})"))
                : "No active borrowing transactions.";

            MessageBox.Show(
                $"Member: {m.FullName}\nID: {m.MemberID} | Student ID: {m.StudentID}\nEmail: {m.Email}\nCourse: {m.CourseAndYear}\nOutstanding Fines: ₱{m.OutstandingFines:N2}\n\nLoans History:\n{loanDetails}",
                $"Borrower Profile - {m.FullName}",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);
        }

        private void HandleRegisterBorrower(object? sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(_txtRegName.Text) || string.IsNullOrWhiteSpace(_txtRegStudentId.Text) || string.IsNullOrWhiteSpace(_txtRegEmail.Text))
            {
                MessageBox.Show("Please fill in Name, Student ID, and Email.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string studentId = _txtRegStudentId.Text.Trim();
            if (_service.Members.Any(m => m.StudentID.Equals(studentId, StringComparison.OrdinalIgnoreCase)))
            {
                MessageBox.Show("A student with this ID is already registered.", "Duplicate Student", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string memberId = _service.GetNextMemberId();
            var newMember = new Member(memberId, studentId, _txtRegName.Text.Trim(), _txtRegEmail.Text.Trim(), _txtRegCourse.Text.Trim());

            _service.AddMember(newMember);
            MessageBox.Show($"Borrower '{newMember.FullName}' registered successfully! Assigned ID: {memberId}", "Registered", MessageBoxButtons.OK, MessageBoxIcon.Information);

            ClearRegForm();
            RenderBorrowerCards();
        }

        private void ClearRegForm()
        {
            _txtRegName.Clear();
            _txtRegStudentId.Clear();
            _txtRegEmail.Clear();
            _txtRegCourse.Clear();
        }
    }
}
