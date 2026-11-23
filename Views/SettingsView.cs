using BookHaven.Services;
using BookHaven.UI;

namespace BookHaven.Views
{
    public class SettingsView : UserControl
    {
        private readonly LibraryDataService _service = LibraryDataService.Instance;

        // Enlarged, readable fonts matching the rest of the app
        private static readonly Font PageTitleFont = new("League Spartan", 22f, FontStyle.Bold);
        private static readonly Font CardHeaderFont = new("League Spartan", 15f, FontStyle.Bold);
        private static readonly Font FieldLabelFont = new("League Spartan", 10f, FontStyle.Regular);
        private static readonly Font InputFont = new("League Spartan", 11f, FontStyle.Regular);
        private static readonly Font CheckboxFont = new("League Spartan", 10.5f, FontStyle.Bold);
        private static readonly Font CategoryPillFont = new("League Spartan", 9.5f, FontStyle.Bold);
        private static readonly Font ButtonFont = new("League Spartan", 10.5f, FontStyle.Bold);

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
                Font = PageTitleFont,
                ForeColor = UITheme.NavyDark,
                BackColor = Color.Transparent,
                AutoSize = true
            };
            _contentPanel.Controls.Add(_lblTitle);

            // 1. Library rules Card
            _rulesCard = new CardPanel { BorderRadius = 16, Padding = new Padding(24) };

            var lblRulesTitle = new Label
            {
                Text = "Library rules",
                Font = CardHeaderFont,
                ForeColor = UITheme.NavyDark,
                BackColor = Color.White,
                Location = new Point(22, 18),
                AutoSize = true
            };
            _rulesCard.Controls.Add(lblRulesTitle);

            int ry = 60;
            _rulesCard.Controls.Add(new Label { Text = "Loan period (days)", Font = FieldLabelFont, ForeColor = UITheme.TextMuted, BackColor = Color.White, Location = new Point(22, ry), AutoSize = true });
            _numLoanPeriod = new NumericUpDown { Location = new Point(22, ry + 24), Font = InputFont, Minimum = 1, Maximum = 60, Value = _service.Settings.LoanPeriodDays };
            _rulesCard.Controls.Add(_numLoanPeriod);

            ry += 70;
            _rulesCard.Controls.Add(new Label { Text = "Fine per day late (₱)", Font = FieldLabelFont, ForeColor = UITheme.TextMuted, BackColor = Color.White, Location = new Point(22, ry), AutoSize = true });
            _numFineRate = new NumericUpDown { Location = new Point(22, ry + 24), Font = InputFont, Minimum = 0, Maximum = 500, DecimalPlaces = 2, Value = _service.Settings.FinePerDayLate };
            _rulesCard.Controls.Add(_numFineRate);

            ry += 70;
            _rulesCard.Controls.Add(new Label { Text = "Max books per borrower", Font = FieldLabelFont, ForeColor = UITheme.TextMuted, BackColor = Color.White, Location = new Point(22, ry), AutoSize = true });
            _numMaxBooks = new NumericUpDown { Location = new Point(22, ry + 24), Font = InputFont, Minimum = 1, Maximum = 20, Value = _service.Settings.MaxBooksPerBorrower };
            _rulesCard.Controls.Add(_numMaxBooks);

            ry += 74;
            Color toggleBg = Color.FromArgb(245, 249, 255);
            _pnlAutoFine = new CardPanel
            {
                Location = new Point(22, ry),
                Height = 52,
                BorderRadius = 10,
                BackgroundFill = toggleBg,
                BorderColor = Color.FromArgb(220, 235, 250)
            };
            _chkAutoFine = new CheckBox
            {
                Text = "Calculate fines automatically on return",
                Font = CheckboxFont,
                ForeColor = UITheme.NavyDark,
                BackColor = toggleBg,
                Checked = _service.Settings.CalculateFinesAutomatically,
                Location = new Point(14, 13),
                AutoSize = true
            };
            _pnlAutoFine.Controls.Add(_chkAutoFine);
            _rulesCard.Controls.Add(_pnlAutoFine);

            ry += 64;
            _pnlBlock = new CardPanel
            {
                Location = new Point(22, ry),
                Height = 52,
                BorderRadius = 10,
                BackgroundFill = toggleBg,
                BorderColor = Color.FromArgb(220, 235, 250)
            };
            _chkBlockOverdue = new CheckBox
            {
                Text = "Block new loans while a fine is unpaid",
                Font = CheckboxFont,
                ForeColor = UITheme.NavyDark,
                BackColor = toggleBg,
                Checked = _service.Settings.BlockNewLoansWhileFineUnpaid,
                Location = new Point(14, 13),
                AutoSize = true
            };
            _pnlBlock.Controls.Add(_chkBlockOverdue);
            _rulesCard.Controls.Add(_pnlBlock);

            ry += 68;
            _btnSaveRules = new PillButton
            {
                Text = "Save rules",
                Font = ButtonFont,
                CustomBackColor = UITheme.NavyDark,
                CustomForeColor = Color.White,
                Location = new Point(22, ry),
                Height = 46
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
                Font = CardHeaderFont,
                ForeColor = UITheme.NavyDark,
                BackColor = Color.White,
                Location = new Point(22, 18),
                AutoSize = true
            };
            _catCard.Controls.Add(lblCatTitle);

            _categoriesPanel = new FlowLayoutPanel
            {
                Location = new Point(22, 56),
                Height = 90,
                AutoScroll = true,
                BackColor = Color.White
            };
            RenderCategories();
            _catCard.Controls.Add(_categoriesPanel);

            _lblNewCategory = new Label { Text = "New category", Font = FieldLabelFont, ForeColor = UITheme.TextMuted, BackColor = Color.White, Location = new Point(22, 154), AutoSize = true };
            _catCard.Controls.Add(_lblNewCategory);
            _txtNewCategory = new TextBox { Location = new Point(22, 178), Font = InputFont };
            _catCard.Controls.Add(_txtNewCategory);

            _btnAddCat = new PillButton
            {
                Text = "Add",
                Font = ButtonFont,
                CustomBackColor = UITheme.HotPink,
                CustomForeColor = Color.White,
                Size = new Size(92, 38)
            };
            _btnAddCat.Click += HandleAddCategory;
            _catCard.Controls.Add(_btnAddCat);

            _contentPanel.Controls.Add(_catCard);

            // 3. Admin Account Card
            _adminCard = new CardPanel { BorderRadius = 16, Padding = new Padding(24) };

            var lblAdminTitle = new Label
            {
                Text = "Admin account",
                Font = CardHeaderFont,
                ForeColor = UITheme.NavyDark,
                BackColor = Color.White,
                Location = new Point(22, 18),
                AutoSize = true
            };
            _adminCard.Controls.Add(lblAdminTitle);

            _adminCard.Controls.Add(new Label { Text = "Display name", Font = FieldLabelFont, ForeColor = UITheme.TextMuted, BackColor = Color.White, Location = new Point(22, 58), AutoSize = true });
            _txtAdminName = new TextBox { Text = _service.CurrentAdmin.DisplayName, Location = new Point(22, 82), Font = InputFont };
            _adminCard.Controls.Add(_txtAdminName);

            _adminCard.Controls.Add(new Label { Text = "New password", Font = FieldLabelFont, ForeColor = UITheme.TextMuted, BackColor = Color.White, Location = new Point(22, 128), AutoSize = true });
            _txtAdminPass = new TextBox { PlaceholderText = "••••••••", UseSystemPasswordChar = true, Location = new Point(22, 152), Font = InputFont };
            _adminCard.Controls.Add(_txtAdminPass);

            _btnUpdateAccount = new PillButton
            {
                Text = "Update account",
                Font = ButtonFont,
                CustomBackColor = Color.White,
                CustomForeColor = UITheme.NavyDark,
                CustomBorderColor = UITheme.CardBorder,
                Location = new Point(22, 204),
                Height = 44
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

            int currentY = _lblTitle.Bottom + 16;
            int minTotalH = 540;
            int adminH = 268;

            if (availWidth >= 880)
            {
                int leftW = (availWidth - gap) / 2;
                int rightW = availWidth - leftW - gap;

                int total = Math.Max(minTotalH, ClientSize.Height - currentY - 24);
                int catH = Math.Max(256, total - adminH - gap);

                _rulesCard.Bounds = new Rectangle(pad, currentY, leftW, total);
                ResizeRulesControls(leftW);

                _catCard.Bounds = new Rectangle(pad + leftW + gap, currentY, rightW, catH);
                ResizeCatControls(rightW);

                _adminCard.Bounds = new Rectangle(pad + leftW + gap, currentY + catH + gap, rightW, adminH);
                ResizeAdminControls(rightW);

                currentY += total + 24;
            }
            else
            {
                _rulesCard.Bounds = new Rectangle(pad, currentY, availWidth, minTotalH);
                ResizeRulesControls(availWidth);
                currentY += minTotalH + gap;

                _catCard.Bounds = new Rectangle(pad, currentY, availWidth, 260);
                ResizeCatControls(availWidth);
                currentY += 260 + gap;

                _adminCard.Bounds = new Rectangle(pad, currentY, availWidth, adminH);
                ResizeAdminControls(availWidth);
                currentY += adminH + 24;
            }

            _contentPanel.Height = currentY;
            AutoScrollMinSize = new Size(0, currentY);
            _contentPanel.ResumeLayout();
        }

        private void ResizeRulesControls(int width)
        {
            int innerW = width - 44;
            _numLoanPeriod.Width = innerW;
            _numFineRate.Width = innerW;
            _numMaxBooks.Width = innerW;
            _pnlAutoFine.Width = innerW;
            _pnlBlock.Width = innerW;
            _btnSaveRules.Width = innerW;

            _btnSaveRules.Top = Math.Max(_saveRulesBaseTop, _rulesCard.Height - _btnSaveRules.Height - 22);
        }

        private void ResizeCatControls(int width)
        {
            int innerW = width - 44;
            _categoriesPanel.Width = innerW;
            _categoriesPanel.Height = Math.Max(84, _catCard.Height - 56 - 98);

            int below = _categoriesPanel.Bottom + 10;
            _lblNewCategory.Location = new Point(22, below);
            _txtNewCategory.Location = new Point(22, below + 24);
            _txtNewCategory.Width = Math.Max(120, innerW - _btnAddCat.Width - 12);
            _btnAddCat.Location = new Point(22 + _txtNewCategory.Width + 12, below + 20);
        }

        private void ResizeAdminControls(int width)
        {
            int innerW = width - 44;
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
                    Font = CategoryPillFont,
                    AutoSize = true,
                    Padding = new Padding(12, 6, 12, 6),
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
            _service.SaveSettings();

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

            _service.AddCategory(cat);
            _txtNewCategory.Clear();
            RenderCategories();
        }

        private void HandleUpdateAdmin(object? sender, EventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(_txtAdminName.Text)) _service.CurrentAdmin.DisplayName = _txtAdminName.Text.Trim();
            if (!string.IsNullOrWhiteSpace(_txtAdminPass.Text)) _service.CurrentAdmin.Password = _txtAdminPass.Text.Trim();

            _service.SaveCurrentAdmin();

            MessageBox.Show("Admin account details updated!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
            _txtAdminPass.Clear();
        }
    }
}