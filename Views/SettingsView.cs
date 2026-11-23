using BookHaven.Services;
using BookHaven.UI;

namespace BookHaven.Views
{
    public class SettingsView : UserControl
    {
        private readonly LibraryDataService _service = LibraryDataService.Instance;

        private Panel _contentPanel = null!;
        private Label _lblTitle = null!;

        // Cards
        private CardPanel _rulesCard = null!;
        private CardPanel _catCard = null!;
        private CardPanel _adminCard = null!;

        // Rule inputs
        private NumericUpDown _numLoanPeriod = null!;
        private NumericUpDown _numFineRate = null!;
        private NumericUpDown _numMaxBooks = null!;
        private CheckBox _chkAutoFine = null!;
        private CheckBox _chkBlockOverdue = null!;
        private CardPanel _pnlAutoFine = null!;
        private CardPanel _pnlBlock = null!;
        private PillButton _btnSaveRules = null!;
        private int _saveRulesBaseTop;
        private Label _lblNewCategory = null!;

        // Categories
        private FlowLayoutPanel _categoriesPanel = null!;
        private TextBox _txtNewCategory = null!;
        private PillButton _btnAddCat = null!;

        // Admin account
        private TextBox _txtAdminName = null!;
        private TextBox _txtAdminPass = null!;
        private PillButton _btnUpdateAccount = null!;

        public SettingsView()
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
                Text = "SETTINGS!",
                Font = new Font("Segoe UI", 18f, FontStyle.Bold),
                ForeColor = UITheme.NavyDark,
                AutoSize = true
            };
            _contentPanel.Controls.Add(_lblTitle);

            // 1. Library rules Card
            _rulesCard = new CardPanel { BorderRadius = 16, Padding = new Padding(24) };

            var lblRulesTitle = new Label
            {
                Text = "Library rules",
                Font = UITheme.SubHeaderFont,
                ForeColor = UITheme.NavyDark,
                Location = new Point(20, 16),
                AutoSize = true
            };
            _rulesCard.Controls.Add(lblRulesTitle);

            int ry = 52;
            _rulesCard.Controls.Add(new Label { Text = "Loan period (days)", Font = UITheme.SmallFont, ForeColor = UITheme.TextMuted, Location = new Point(20, ry), AutoSize = true });
            _numLoanPeriod = new NumericUpDown { Location = new Point(20, ry + 20), Font = UITheme.BodyFont, Minimum = 1, Maximum = 60, Value = _service.Settings.LoanPeriodDays };
            _rulesCard.Controls.Add(_numLoanPeriod);

            ry += 58;
            _rulesCard.Controls.Add(new Label { Text = "Fine per day late (₱)", Font = UITheme.SmallFont, ForeColor = UITheme.TextMuted, Location = new Point(20, ry), AutoSize = true });
            _numFineRate = new NumericUpDown { Location = new Point(20, ry + 20), Font = UITheme.BodyFont, Minimum = 0, Maximum = 500, DecimalPlaces = 2, Value = _service.Settings.FinePerDayLate };
            _rulesCard.Controls.Add(_numFineRate);

            ry += 58;
            _rulesCard.Controls.Add(new Label { Text = "Max books per borrower", Font = UITheme.SmallFont, ForeColor = UITheme.TextMuted, Location = new Point(20, ry), AutoSize = true });
            _numMaxBooks = new NumericUpDown { Location = new Point(20, ry + 20), Font = UITheme.BodyFont, Minimum = 1, Maximum = 20, Value = _service.Settings.MaxBooksPerBorrower };
            _rulesCard.Controls.Add(_numMaxBooks);

            ry += 58;
            _pnlAutoFine = new CardPanel
            {
                Location = new Point(20, ry),
                Height = 44,
                BorderRadius = 10,
                BackgroundFill = Color.FromArgb(245, 249, 255),
                BorderColor = Color.FromArgb(220, 235, 250)
            };
            _chkAutoFine = new CheckBox
            {
                Text = "Calculate fines automatically on return",
                Font = UITheme.BodyBold,
                ForeColor = UITheme.NavyDark,
                Checked = _service.Settings.CalculateFinesAutomatically,
                Location = new Point(12, 10),
                AutoSize = true
            };
            _pnlAutoFine.Controls.Add(_chkAutoFine);
            _rulesCard.Controls.Add(_pnlAutoFine);

            ry += 52;
            _pnlBlock = new CardPanel
            {
                Location = new Point(20, ry),
                Height = 44,
                BorderRadius = 10,
                BackgroundFill = Color.FromArgb(245, 249, 255),
                BorderColor = Color.FromArgb(220, 235, 250)
            };
            _chkBlockOverdue = new CheckBox
            {
                Text = "Block new loans while a fine is unpaid",
                Font = UITheme.BodyBold,
                ForeColor = UITheme.NavyDark,
                Checked = _service.Settings.BlockNewLoansWhileFineUnpaid,
                Location = new Point(12, 10),
                AutoSize = true
            };
            _pnlBlock.Controls.Add(_chkBlockOverdue);
            _rulesCard.Controls.Add(_pnlBlock);

            ry += 54;
            _btnSaveRules = new PillButton
            {
                Text = "Save rules",
                CustomBackColor = UITheme.NavyDark,
                CustomForeColor = Color.White,
                Location = new Point(20, ry),
                Height = 40
            };
            _btnSaveRules.Click += HandleSaveRules;
            _rulesCard.Controls.Add(_btnSaveRules);
            _saveRulesBaseTop = ry;

            _contentPanel.Controls.Add(_rulesCard);

            // 2. Categories Card
            _catCard = new CardPanel { BorderRadius = 16, Padding = new Padding(24) };

            var lblCatTitle = new Label
            {
                Text = "Categories",
                Font = UITheme.SubHeaderFont,
                ForeColor = UITheme.NavyDark,
                Location = new Point(20, 16),
                AutoSize = true
            };
            _catCard.Controls.Add(lblCatTitle);

            _categoriesPanel = new FlowLayoutPanel
            {
                Location = new Point(20, 48),
                Height = 75,
                AutoScroll = true
            };
            RenderCategories();
            _catCard.Controls.Add(_categoriesPanel);

            _lblNewCategory = new Label { Text = "New category", Font = UITheme.SmallFont, ForeColor = UITheme.TextMuted, Location = new Point(20, 130), AutoSize = true };
            _catCard.Controls.Add(_lblNewCategory);
            _txtNewCategory = new TextBox { Location = new Point(20, 150), Font = UITheme.BodyFont };
            _catCard.Controls.Add(_txtNewCategory);

            _btnAddCat = new PillButton
            {
                Text = "Add",
                CustomBackColor = UITheme.HotPink,
                CustomForeColor = Color.White,
                Location = new Point(310, 148),
                Size = new Size(80, 32)
            };
            _btnAddCat.Click += HandleAddCategory;
            _catCard.Controls.Add(_btnAddCat);

            _contentPanel.Controls.Add(_catCard);

            // 3. Admin Account Card
            _adminCard = new CardPanel { BorderRadius = 16, Padding = new Padding(24) };

            var lblAdminTitle = new Label
            {
                Text = "Admin account",
                Font = UITheme.SubHeaderFont,
                ForeColor = UITheme.NavyDark,
                Location = new Point(20, 16),
                AutoSize = true
            };
            _adminCard.Controls.Add(lblAdminTitle);

            _adminCard.Controls.Add(new Label { Text = "Display name", Font = UITheme.SmallFont, ForeColor = UITheme.TextMuted, Location = new Point(20, 46), AutoSize = true });
            _txtAdminName = new TextBox { Text = _service.CurrentAdmin.DisplayName, Location = new Point(20, 66), Font = UITheme.BodyFont };
            _adminCard.Controls.Add(_txtAdminName);

            _adminCard.Controls.Add(new Label { Text = "New password", Font = UITheme.SmallFont, ForeColor = UITheme.TextMuted, Location = new Point(20, 104), AutoSize = true });
            _txtAdminPass = new TextBox { PlaceholderText = "••••••••", UseSystemPasswordChar = true, Location = new Point(20, 124), Font = UITheme.BodyFont };
            _adminCard.Controls.Add(_txtAdminPass);

            _btnUpdateAccount = new PillButton
            {
                Text = "Update account",
                CustomBackColor = Color.White,
                CustomForeColor = UITheme.NavyDark,
                CustomBorderColor = UITheme.CardBorder,
                Location = new Point(20, 166),
                Height = 36
            };
            _btnUpdateAccount.Click += HandleUpdateAdmin;
            _adminCard.Controls.Add(_btnUpdateAccount);

            _contentPanel.Controls.Add(_adminCard);

            Resize += (s, e) => PerformResponsiveLayout();
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

            int currentY = 56;

            if (availWidth >= 880)
            {
                int leftW = (availWidth - gap) / 2;
                int rightW = leftW;

                // Fill the window: spare height goes to the rules card (left) and category card (right)
                int total = Math.Max(470, ClientSize.Height - currentY - 30);
                int spare = total - 470;

                _rulesCard.Bounds = new Rectangle(pad, currentY, leftW, 460 + spare);
                ResizeRulesControls(leftW);

                _catCard.Bounds = new Rectangle(pad + leftW + gap, currentY, rightW, 220 + spare);
                ResizeCatControls(rightW);

                _adminCard.Bounds = new Rectangle(pad + leftW + gap, currentY + 236 + spare, rightW, 224);
                ResizeAdminControls(rightW);

                currentY += total + 30;
            }
            else
            {
                _rulesCard.Bounds = new Rectangle(pad, currentY, availWidth, 460);
                ResizeRulesControls(availWidth);
                currentY += 460 + gap;

                _catCard.Bounds = new Rectangle(pad, currentY, availWidth, 220);
                ResizeCatControls(availWidth);
                currentY += 220 + gap;

                _adminCard.Bounds = new Rectangle(pad, currentY, availWidth, 224);
                ResizeAdminControls(availWidth);
                currentY += 224 + 30;
            }

            _contentPanel.Height = currentY;
            _contentPanel.ResumeLayout();
        }

        private void ResizeRulesControls(int width)
        {
            int innerW = width - 40;
            _numLoanPeriod.Width = innerW;
            _numFineRate.Width = innerW;
            _numMaxBooks.Width = innerW;
            _pnlAutoFine.Width = innerW;
            _pnlBlock.Width = innerW;
            _btnSaveRules.Width = innerW;

            // Save button hugs the bottom edge of the card
            _btnSaveRules.Top = Math.Max(_saveRulesBaseTop, _rulesCard.Height - _btnSaveRules.Height - 24);
        }

        private void ResizeCatControls(int width)
        {
            int innerW = width - 40;
            _categoriesPanel.Width = innerW;
            _categoriesPanel.Height = Math.Max(75, _catCard.Height - 48 - 100);   // grows with the card
            int below = _categoriesPanel.Bottom;
            _lblNewCategory.Location = new Point(20, below + 7);
            _txtNewCategory.Location = new Point(20, below + 27);
            _txtNewCategory.Width = Math.Max(120, innerW - 95);
            _btnAddCat.Location = new Point(innerW - 75, below + 25);
        }

        private void ResizeAdminControls(int width)
        {
            int innerW = width - 40;
            _txtAdminName.Width = innerW;
            _txtAdminPass.Width = innerW;
            _btnUpdateAccount.Width = innerW;
        }

        private void RenderCategories()
        {
            _categoriesPanel.Controls.Clear();
            foreach (var cat in _service.Settings.Categories)
            {
                var pill = new Label
                {
                    Text = cat,
                    BackColor = UITheme.SkyBlueLight,
                    ForeColor = UITheme.NavyDark,
                    Font = new Font("Segoe UI", 8.5f, FontStyle.Bold),
                    AutoSize = true,
                    Padding = new Padding(10, 5, 10, 5),
                    Margin = new Padding(0, 0, 8, 8)
                };
                _categoriesPanel.Controls.Add(pill);
            }
        }

        private void HandleSaveRules(object? sender, EventArgs e)
        {
            _service.Settings.LoanPeriodDays = (int)_numLoanPeriod.Value;
            _service.Settings.FinePerDayLate = _numFineRate.Value;
            _service.Settings.MaxBooksPerBorrower = (int)_numMaxBooks.Value;
            _service.Settings.CalculateFinesAutomatically = _chkAutoFine.Checked;
            _service.Settings.BlockNewLoansWhileFineUnpaid = _chkBlockOverdue.Checked;

            MessageBox.Show("Library rules updated successfully!", "Saved", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void HandleAddCategory(object? sender, EventArgs e)
        {
            string cat = _txtNewCategory.Text.Trim();
            if (string.IsNullOrWhiteSpace(cat)) return;

            if (_service.Settings.Categories.Any(c => c.Equals(cat, StringComparison.OrdinalIgnoreCase)))
            {
                MessageBox.Show("Category already exists.", "Duplicate", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            _service.Settings.Categories.Add(cat);
            _txtNewCategory.Clear();
            RenderCategories();
        }

        private void HandleUpdateAdmin(object? sender, EventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(_txtAdminName.Text)) _service.CurrentAdmin.DisplayName = _txtAdminName.Text.Trim();
            if (!string.IsNullOrWhiteSpace(_txtAdminPass.Text)) _service.CurrentAdmin.Password = _txtAdminPass.Text.Trim();

            MessageBox.Show("Admin account details updated!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
            _txtAdminPass.Clear();
        }
    }
}
