using BookHaven.Forms;
using BookHaven.Models;
using BookHaven.Services;
using BookHaven.UI;

namespace BookHaven.Views
{
    public class BooksView : UserControl
    {
        private readonly LibraryDataService _service = LibraryDataService.Instance;

        public event EventHandler<string>? NavigateRequested;

        // Container
        private Panel _contentPanel = null!;

        // Header and Mode Tabs
        private Label _lblTitle = null!;
        private FlowLayoutPanel _tabHeaderPanel = null!;
        private Button _btnTabCatalog = null!;
        private Button _btnTabAdd = null!;
        private Button _btnTabEdit = null!;

        // Top Search Card
        private CardPanel _searchCard = null!;
        private RoundedTextBox _txtSearch = null!;
        private ComboBox _cmbCategory = null!;
        private PillButton _btnSearch = null!;
        private FlowLayoutPanel _categoryPillsPanel = null!;
        private string _activeCategoryFilter = "All";

        // Main 2-Column Split (matches PDF Page 2)
        private CardPanel _inventoryCard = null!;
        private Label _lblInventoryHeader = null!;
        private Label _lblInventorySub = null!;
        private Panel _tableHeaderRow = null!;
        private Panel _inventoryListPanel = null!;

        // Add or Edit Book Card (Right column on desktop, tabbed on mobile)
        private CardPanel _formCard = null!;
        private Label _lblFormTitle = null!;
        private FlowLayoutPanel _formTabPanel = null!;
        private Button _btnFormModeAdd = null!;
        private Button _btnFormModeEdit = null!;
        private ComboBox _cmbSelectBookToEdit = null!;
        private Label _lblSelectBook = null!;

        private RoundedTextBox _txtBookTitle = null!;
        private RoundedTextBox _txtBookAuthor = null!;
        private RoundedTextBox _txtBookIsbn = null!;
        private ComboBox _cmbBookCategory = null!;
        private NumericUpDown _numBookCopies = null!;
        private RoundedTextBox _txtBookShelf = null!;
        private PillButton _btnSaveBook = null!;
        private PillButton _btnClearForm = null!;

        private string _activeTab = "Catalog";
        private string _formMode = "Add"; // "Add" or "Edit"
        private Book? _editingBook = null;

        public BooksView()
        {
            InitializeComponent();
        }

        protected override Point ScrollToControl(Control activeControl)
        {
            return new Point(0, AutoScrollPosition.Y);
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
                Text = "THE BOOK SHELF!",
                Font = UITheme.HeaderFont,
                ForeColor = UITheme.NavyDark,
                AutoSize = true
            };
            _contentPanel.Controls.Add(_lblTitle);

            // Tabs for Add / Edit
            _tabHeaderPanel = new FlowLayoutPanel
            {
                FlowDirection = FlowDirection.LeftToRight,
                AutoSize = true,
                BackColor = Color.Transparent
            };

            _btnTabCatalog = CreateTabButton("All Books (Catalog)", isDefault: true);
            _btnTabAdd = CreateTabButton("Add Book", isDefault: false);
            _btnTabEdit = CreateTabButton("Edit Book", isDefault: false);

            _btnTabCatalog.Click += (s, e) => SwitchTab("Catalog");
            _btnTabAdd.Click += (s, e) => SwitchTab("Add");
            _btnTabEdit.Click += (s, e) => SwitchTab("Edit");

            _tabHeaderPanel.Controls.AddRange(new Control[] { _btnTabCatalog, _btnTabAdd, _btnTabEdit });
            _contentPanel.Controls.Add(_tabHeaderPanel);

            // Build Components
            BuildSearchCard();
            BuildInventoryCard();
            BuildFormCard();

            _contentPanel.Controls.AddRange(new Control[] { _searchCard, _inventoryCard, _formCard });

            Resize += (s, e) => PerformResponsiveLayout();
            FilterAndRenderBooks();
            PerformResponsiveLayout();
        }

        private Button CreateTabButton(string text, bool isDefault)
        {
            var btn = new Button
            {
                Text = text,
                AutoSize = true,
                Height = 36,
                Padding = new Padding(16, 4, 16, 4),
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9f, FontStyle.Bold),
                Cursor = Cursors.Hand,
                BackColor = isDefault ? UITheme.NavyDark : Color.White,
                ForeColor = isDefault ? Color.White : UITheme.NavyDark,
                Margin = new Padding(0, 0, 8, 8)
            };
            btn.FlatAppearance.BorderColor = Color.FromArgb(205, 225, 245);
            return btn;
        }

        public void SwitchTab(string tabName, Book? targetBook = null)
        {
            _activeTab = tabName;

            _btnTabCatalog.BackColor = tabName == "Catalog" ? UITheme.NavyDark : Color.White;
            _btnTabCatalog.ForeColor = tabName == "Catalog" ? Color.White : UITheme.NavyDark;

            _btnTabAdd.BackColor = tabName == "Add" ? UITheme.NavyDark : Color.White;
            _btnTabAdd.ForeColor = tabName == "Add" ? Color.White : UITheme.NavyDark;

            _btnTabEdit.BackColor = tabName == "Edit" ? UITheme.NavyDark : Color.White;
            _btnTabEdit.ForeColor = tabName == "Edit" ? Color.White : UITheme.NavyDark;

            if (tabName == "Add")
            {
                SetFormMode("Add");
            }
            else if (tabName == "Edit")
            {
                SetFormMode("Edit", targetBook);
            }

            PerformResponsiveLayout();
        }

        #region TOP SEARCH CARD
        private void BuildSearchCard()
        {
            _searchCard = new CardPanel
            {
                Height = 110,
                BorderRadius = 14
            };

            var lblSearch = new Label
            {
                Text = "Search the catalog",
                Font = UITheme.SmallFont,
                ForeColor = UITheme.TextMuted,
                Location = new Point(18, 10),
                AutoSize = true
            };

            _txtSearch = new RoundedTextBox
            {
                Location = new Point(18, 28),
                Height = 36,
                BorderRadius = 10
            };
            _txtSearch.InnerTextBox.PlaceholderText = "Title, author, ISBN";
            _txtSearch.InnerTextChanged += (s, e) => FilterAndRenderBooks();

            var lblCat = new Label
            {
                Text = "Category",
                Font = UITheme.SmallFont,
                ForeColor = UITheme.TextMuted,
                Location = new Point(410, 10),
                AutoSize = true
            };

            _cmbCategory = new ComboBox
            {
                Location = new Point(410, 30),
                Size = new Size(180, 32),
                DropDownStyle = ComboBoxStyle.DropDownList,
                Font = UITheme.BodyFont
            };
            _cmbCategory.Items.Add("All categories");
            foreach (var cat in _service.Settings.Categories) _cmbCategory.Items.Add(cat);
            _cmbCategory.SelectedIndex = 0;
            _cmbCategory.SelectedIndexChanged += (s, e) =>
            {
                _activeCategoryFilter = _cmbCategory.SelectedIndex == 0 ? "All" : _cmbCategory.SelectedItem!.ToString()!;
                BuildCategoryPills();
                FilterAndRenderBooks();
            };

            _btnSearch = new PillButton
            {
                Text = "Search",
                CustomBackColor = UITheme.HotPink,
                CustomForeColor = Color.White,
                Size = new Size(95, 34),
                Location = new Point(604, 28)
            };
            _btnSearch.Click += (s, e) => FilterAndRenderBooks();

            _categoryPillsPanel = new FlowLayoutPanel
            {
                Location = new Point(18, 68),
                Height = 34,
                FlowDirection = FlowDirection.LeftToRight,
                BackColor = Color.Transparent
            };
            BuildCategoryPills();

            _searchCard.Controls.AddRange(new Control[] { lblSearch, _txtSearch, lblCat, _cmbCategory, _btnSearch, _categoryPillsPanel });
        }

        private void BuildCategoryPills()
        {
            _categoryPillsPanel.Controls.Clear();
            var list = new List<string> { "All" };
            list.AddRange(_service.Settings.Categories);

            foreach (var cat in list)
            {
                var pill = new Button
                {
                    Text = cat,
                    AutoSize = true,
                    Height = 26,
                    FlatStyle = FlatStyle.Flat,
                    Cursor = Cursors.Hand,
                    Font = new Font("Segoe UI", 8.2f, FontStyle.Bold),
                    Margin = new Padding(0, 0, 6, 0)
                };

                bool isSelected = _activeCategoryFilter.Equals(cat, StringComparison.OrdinalIgnoreCase);
                pill.BackColor = isSelected ? UITheme.NavyDark : UITheme.SkyBlueLight;
                pill.ForeColor = isSelected ? Color.White : UITheme.NavyDark;
                pill.FlatAppearance.BorderSize = 0;

                pill.Click += (s, e) =>
                {
                    _activeCategoryFilter = cat;
                    if (cat == "All") _cmbCategory.SelectedIndex = 0;
                    else
                    {
                        int idx = _cmbCategory.Items.IndexOf(cat);
                        if (idx >= 0) _cmbCategory.SelectedIndex = idx;
                    }
                    BuildCategoryPills();
                    FilterAndRenderBooks();
                };

                _categoryPillsPanel.Controls.Add(pill);
            }
        }
        #endregion

        #region LEFT COLUMN: INVENTORY CARD
        private void BuildInventoryCard()
        {
            _inventoryCard = new CardPanel
            {
                BorderRadius = 16
            };

            _lblInventoryHeader = new Label
            {
                Text = "Inventory",
                Font = UITheme.SubHeaderFont,
                ForeColor = UITheme.NavyDark,
                Location = new Point(18, 14),
                AutoSize = true
            };

            _lblInventorySub = new Label
            {
                Text = "6 titles shown · (Click any book for details)",
                Font = UITheme.SmallFont,
                ForeColor = UITheme.TextMuted,
                Location = new Point(130, 18),
                AutoSize = true
            };

            // Table Header Row
            _tableHeaderRow = new Panel
            {
                Location = new Point(18, 44),
                Height = 22,
                BackColor = Color.Transparent
            };
            _tableHeaderRow.Paint += (s, e) =>
            {
                // Muted divider line under header
                using var p = new Pen(Color.FromArgb(235, 242, 250), 1f);
                e.Graphics.DrawLine(p, 0, _tableHeaderRow.Height - 1, _tableHeaderRow.Width, _tableHeaderRow.Height - 1);
            };

            _inventoryListPanel = new Panel
            {
                Location = new Point(18, 70),
                AutoScroll = true,
                BackColor = Color.Transparent
            };

            _inventoryCard.Controls.AddRange(new Control[] { _lblInventoryHeader, _lblInventorySub, _tableHeaderRow, _inventoryListPanel });
        }

        private void LayoutTableHeader(int totalWidth)
        {
            _tableHeaderRow.Controls.Clear();
            _tableHeaderRow.Width = totalWidth;

            int colTitle = (int)(totalWidth * 0.40f);
            int colCat = (int)(totalWidth * 0.20f);
            int colCopies = (int)(totalWidth * 0.12f);
            int colStatus = (int)(totalWidth * 0.14f);
            int colAction = totalWidth - colTitle - colCat - colCopies - colStatus;

            _tableHeaderRow.Controls.Add(new Label { Text = "TITLE & AUTHOR", Font = UITheme.SmallFont, ForeColor = UITheme.TextMuted, Location = new Point(54, 2), Size = new Size(colTitle - 54, 18) });
            _tableHeaderRow.Controls.Add(new Label { Text = "CATEGORY", Font = UITheme.SmallFont, ForeColor = UITheme.TextMuted, Location = new Point(colTitle, 2), Size = new Size(colCat, 18) });
            _tableHeaderRow.Controls.Add(new Label { Text = "COPIES", Font = UITheme.SmallFont, ForeColor = UITheme.TextMuted, Location = new Point(colTitle + colCat, 2), Size = new Size(colCopies, 18) });
            _tableHeaderRow.Controls.Add(new Label { Text = "STATUS", Font = UITheme.SmallFont, ForeColor = UITheme.TextMuted, Location = new Point(colTitle + colCat + colCopies, 2), Size = new Size(colStatus, 18) });
            _tableHeaderRow.Controls.Add(new Label { Text = "ACTION", Font = UITheme.SmallFont, ForeColor = UITheme.TextMuted, Location = new Point(colTitle + colCat + colCopies + colStatus, 2), Size = new Size(colAction, 18) });
        }

        public void FilterAndRenderBooks()
        {
            _inventoryListPanel.SuspendLayout();
            _inventoryListPanel.Controls.Clear();

            string search = _txtSearch.Text.Trim().ToLower();
            var filtered = _service.Books.Where(b =>
            {
                bool matchesCat = _activeCategoryFilter == "All" || b.Category.Equals(_activeCategoryFilter, StringComparison.OrdinalIgnoreCase);
                bool matchesSearch = string.IsNullOrWhiteSpace(search) ||
                                     b.Title.ToLower().Contains(search) ||
                                     b.Author.ToLower().Contains(search) ||
                                     b.ISBN.ToLower().Contains(search);
                return matchesCat && matchesSearch;
            }).ToList();

            _lblInventorySub.Text = $"{filtered.Count} titles shown · (Click any book for details)";

            int totalW = Math.Max(320, _inventoryListPanel.Width - 10);
            LayoutTableHeader(totalW);

            int y = 0;
            int rowH = 68;

            foreach (var book in filtered)
            {
                var row = CreateInventoryRow(book, totalW, rowH);
                row.Location = new Point(0, y);
                _inventoryListPanel.Controls.Add(row);
                y += rowH + 6;
            }

            _inventoryListPanel.ResumeLayout();
        }

        private Panel CreateInventoryRow(Book book, int totalW, int rowH)
        {
            var row = new Panel
            {
                Size = new Size(totalW, rowH),
                BackColor = Color.White,
                Cursor = Cursors.Hand
            };

            int colTitle = (int)(totalW * 0.40f);
            int colCat = (int)(totalW * 0.20f);
            int colCopies = (int)(totalW * 0.12f);
            int colStatus = (int)(totalW * 0.14f);
            int colAction = totalW - colTitle - colCat - colCopies - colStatus;

            // 1. Real Book Cover Thumbnail (Matches user requirement)
            var picCover = new PictureBox
            {
                Location = new Point(4, 6),
                Size = new Size(40, 56),
                SizeMode = PictureBoxSizeMode.Zoom,
                Image = UITheme.GetBookCoverImage(book, 40, 56),
                Cursor = Cursors.Hand
            };
            picCover.Click += (s, e) => ShowBookDetails(book);

            // 2. Title & Author
            var lblTitle = new Label
            {
                Text = book.Title,
                Font = UITheme.BodyBold,
                ForeColor = UITheme.NavyDark,
                Location = new Point(50, 10),
                Size = new Size(colTitle - 54, 20),
                AutoEllipsis = true
            };
            lblTitle.Click += (s, e) => ShowBookDetails(book);

            var lblAuthor = new Label
            {
                Text = $"{book.Author} · {book.ISBN}",
                Font = UITheme.SmallFont,
                ForeColor = UITheme.TextMuted,
                Location = new Point(50, 32),
                Size = new Size(colTitle - 54, 18),
                AutoEllipsis = true
            };
            lblAuthor.Click += (s, e) => ShowBookDetails(book);

            // 3. Category
            var lblCategory = new Label
            {
                Text = book.Category,
                Font = UITheme.BodyFont,
                ForeColor = UITheme.TextMain,
                Location = new Point(colTitle, 22),
                Size = new Size(colCat - 6, 22),
                AutoEllipsis = true
            };

            // 4. Copies
            var lblCopies = new Label
            {
                Text = $"{book.AvailableCopies} / {book.TotalCopies}",
                Font = new Font("Segoe UI", 10.5f, FontStyle.Bold),
                ForeColor = UITheme.NavyDark,
                Location = new Point(colTitle + colCat, 20),
                Size = new Size(colCopies - 6, 24)
            };

            // 5. Status Badge
            Color bg, fg;
            string status;
            if (book.AvailableCopies <= 0)
            {
                status = "All borrowed";
                bg = UITheme.StatusRedBg;
                fg = UITheme.StatusRed;
            }
            else if (book.AvailableCopies <= 1)
            {
                status = "Low stock";
                bg = UITheme.StatusYellowBg;
                fg = UITheme.StatusYellow;
            }
            else
            {
                status = "Available";
                bg = UITheme.StatusGreenBg;
                fg = UITheme.StatusGreen;
            }

            var lblBadge = new Label
            {
                Text = status,
                Font = new Font("Segoe UI", 7.5f, FontStyle.Bold),
                BackColor = bg,
                ForeColor = fg,
                Location = new Point(colTitle + colCat + colCopies, 20),
                Size = new Size(Math.Min(82, colStatus - 4), 24),
                TextAlign = ContentAlignment.MiddleCenter
            };

            // 6. Action Button (Details / Edit)
            var btnDetails = new Button
            {
                Text = "Details",
                Font = new Font("Segoe UI", 7.5f, FontStyle.Bold),
                BackColor = UITheme.SkyBlueLight,
                ForeColor = UITheme.NavyDark,
                FlatStyle = FlatStyle.Flat,
                Size = new Size(Math.Max(50, colAction - 4), 26),
                Location = new Point(colTitle + colCat + colCopies + colStatus + 2, 19),
                Cursor = Cursors.Hand
            };
            btnDetails.FlatAppearance.BorderSize = 0;
            btnDetails.Click += (s, e) => ShowBookDetails(book);

            row.Controls.AddRange(new Control[] { picCover, lblTitle, lblAuthor, lblCategory, lblCopies, lblBadge, btnDetails });
            row.Click += (s, e) => ShowBookDetails(book);

            // Subtle bottom separator
            row.Paint += (s, e) =>
            {
                using var p = new Pen(Color.FromArgb(240, 246, 253), 1f);
                e.Graphics.DrawLine(p, 4, rowH - 1, totalW - 4, rowH - 1);
            };

            return row;
        }

        private void ShowBookDetails(Book book)
        {
            using var modal = new BookDetailsModal(book);
            modal.RequestEdit += (s, b) =>
            {
                SwitchTab("Edit", b);
            };
            modal.RequestIssue += (s, b) =>
            {
                NavigateRequested?.Invoke(this, $"Issue:{b.ISBN}");
            };
            modal.ShowDialog(FindForm());
        }
        #endregion

        #region RIGHT COLUMN: ADD OR EDIT BOOK CARD
        private void BuildFormCard()
        {
            _formCard = new CardPanel
            {
                BorderRadius = 16
            };

            _lblFormTitle = new Label
            {
                Text = "Add or edit a book",
                Font = UITheme.SubHeaderFont,
                ForeColor = UITheme.NavyDark,
                Location = new Point(18, 14),
                AutoSize = true
            };

            // Mode Selector Pills inside card
            _formTabPanel = new FlowLayoutPanel
            {
                Location = new Point(18, 44),
                Height = 32,
                FlowDirection = FlowDirection.LeftToRight,
                BackColor = Color.Transparent
            };

            _btnFormModeAdd = new Button
            {
                Text = "+ Add Book",
                Font = new Font("Segoe UI", 8.2f, FontStyle.Bold),
                Size = new Size(100, 28),
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand,
                BackColor = UITheme.NavyDark,
                ForeColor = Color.White,
                Margin = new Padding(0, 0, 8, 0)
            };
            _btnFormModeAdd.FlatAppearance.BorderSize = 0;
            _btnFormModeAdd.Click += (s, e) => SetFormMode("Add");

            _btnFormModeEdit = new Button
            {
                Text = "✏ Edit Book",
                Font = new Font("Segoe UI", 8.2f, FontStyle.Bold),
                Size = new Size(100, 28),
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand,
                BackColor = UITheme.SkyBlueLight,
                ForeColor = UITheme.NavyDark,
                Margin = new Padding(0, 0, 8, 0)
            };
            _btnFormModeEdit.FlatAppearance.BorderSize = 0;
            _btnFormModeEdit.Click += (s, e) => SetFormMode("Edit");

            _formTabPanel.Controls.AddRange(new Control[] { _btnFormModeAdd, _btnFormModeEdit });

            // Book Selector (Visible only in Edit Mode)
            _lblSelectBook = new Label
            {
                Text = "Select book to edit",
                Font = UITheme.SmallFont,
                ForeColor = UITheme.TextMuted,
                Location = new Point(18, 80),
                AutoSize = true,
                Visible = false
            };

            _cmbSelectBookToEdit = new ComboBox
            {
                Location = new Point(18, 100),
                Height = 30,
                DropDownStyle = ComboBoxStyle.DropDownList,
                Font = UITheme.BodyFont,
                Visible = false
            };
            _cmbSelectBookToEdit.SelectedIndexChanged += (s, e) =>
            {
                if (_cmbSelectBookToEdit.SelectedItem is Book b)
                {
                    PopulateFieldsWithBook(b);
                }
            };

            // Input Fields
            int y = 84;

            var lblT = new Label { Text = "Title", Font = UITheme.SmallFont, ForeColor = UITheme.TextMuted, Location = new Point(18, y), AutoSize = true };
            _txtBookTitle = new RoundedTextBox { Location = new Point(18, y + 18), Height = 36, BorderRadius = 10 };

            y += 58;
            var lblA = new Label { Text = "Author", Font = UITheme.SmallFont, ForeColor = UITheme.TextMuted, Location = new Point(18, y), AutoSize = true };
            _txtBookAuthor = new RoundedTextBox { Location = new Point(18, y + 18), Height = 36, BorderRadius = 10 };

            y += 58;
            var lblI = new Label { Text = "ISBN", Font = UITheme.SmallFont, ForeColor = UITheme.TextMuted, Location = new Point(18, y), AutoSize = true };
            _txtBookIsbn = new RoundedTextBox { Location = new Point(18, y + 18), Height = 36, BorderRadius = 10 };

            y += 58;
            var lblC = new Label { Text = "Category", Font = UITheme.SmallFont, ForeColor = UITheme.TextMuted, Location = new Point(18, y), AutoSize = true };
            var lblCop = new Label { Text = "Copies", Font = UITheme.SmallFont, ForeColor = UITheme.TextMuted, Location = new Point(200, y), AutoSize = true };

            _cmbBookCategory = new ComboBox
            {
                Location = new Point(18, y + 20),
                Size = new Size(170, 32),
                DropDownStyle = ComboBoxStyle.DropDownList,
                Font = UITheme.BodyFont
            };
            foreach (var cat in _service.Settings.Categories) _cmbBookCategory.Items.Add(cat);
            if (_cmbBookCategory.Items.Count > 0) _cmbBookCategory.SelectedIndex = 0;

            _numBookCopies = new NumericUpDown
            {
                Location = new Point(200, y + 20),
                Size = new Size(80, 32),
                Font = UITheme.BodyFont,
                Minimum = 1,
                Maximum = 999,
                Value = 3
            };

            y += 58;
            var lblShelf = new Label { Text = "Shelf location", Font = UITheme.SmallFont, ForeColor = UITheme.TextMuted, Location = new Point(18, y), AutoSize = true };
            _txtBookShelf = new RoundedTextBox { Location = new Point(18, y + 18), Height = 36, BorderRadius = 10 };
            _txtBookShelf.InnerTextBox.PlaceholderText = "e.g. Shelf A-1";

            // Buttons
            y += 66;
            _btnSaveBook = new PillButton
            {
                Text = "Save book",
                CustomBackColor = UITheme.NavyDark,
                CustomForeColor = Color.White,
                Location = new Point(18, y),
                Size = new Size(130, 38)
            };
            _btnSaveBook.Click += HandleSaveBook;

            _btnClearForm = new PillButton
            {
                Text = "Clear",
                CustomBackColor = Color.White,
                CustomForeColor = UITheme.NavyDark,
                CustomBorderColor = Color.FromArgb(205, 225, 245),
                Location = new Point(156, y),
                Size = new Size(100, 38)
            };
            _btnClearForm.Click += (s, e) => ClearForm();

            _formCard.Controls.AddRange(new Control[]
            {
                _lblFormTitle, _formTabPanel, _lblSelectBook, _cmbSelectBookToEdit,
                lblT, _txtBookTitle,
                lblA, _txtBookAuthor,
                lblI, _txtBookIsbn,
                lblC, lblCop, _cmbBookCategory, _numBookCopies,
                lblShelf, _txtBookShelf,
                _btnSaveBook, _btnClearForm
            });
        }

        private void SetFormMode(string mode, Book? bookToEdit = null)
        {
            _formMode = mode;
            _editingBook = bookToEdit;

            bool isEdit = mode == "Edit";

            _btnFormModeAdd.BackColor = !isEdit ? UITheme.NavyDark : UITheme.SkyBlueLight;
            _btnFormModeAdd.ForeColor = !isEdit ? Color.White : UITheme.NavyDark;

            _btnFormModeEdit.BackColor = isEdit ? UITheme.NavyDark : UITheme.SkyBlueLight;
            _btnFormModeEdit.ForeColor = isEdit ? Color.White : UITheme.NavyDark;

            _lblFormTitle.Text = isEdit ? "Edit a book" : "Add or edit a book";
            _btnSaveBook.Text = isEdit ? "Update book" : "Save book";

            _lblSelectBook.Visible = isEdit;
            _cmbSelectBookToEdit.Visible = isEdit;

            if (isEdit)
            {
                _cmbSelectBookToEdit.Items.Clear();
                foreach (var b in _service.Books) _cmbSelectBookToEdit.Items.Add(b);

                if (bookToEdit != null)
                {
                    _cmbSelectBookToEdit.SelectedItem = bookToEdit;
                    PopulateFieldsWithBook(bookToEdit);
                }
                else if (_cmbSelectBookToEdit.Items.Count > 0)
                {
                    _cmbSelectBookToEdit.SelectedIndex = 0;
                }
            }
            else
            {
                ClearForm();
            }

            PerformResponsiveLayout();
        }

        private void PopulateFieldsWithBook(Book book)
        {
            _editingBook = book;
            _txtBookTitle.Text = book.Title;
            _txtBookAuthor.Text = book.Author;
            _txtBookIsbn.Text = book.ISBN;
            _txtBookShelf.Text = book.ShelfLocation;
            _numBookCopies.Value = Math.Max(1, book.TotalCopies);

            int idx = _cmbBookCategory.Items.IndexOf(book.Category);
            if (idx >= 0) _cmbBookCategory.SelectedIndex = idx;
        }

        private void ClearForm()
        {
            _editingBook = null;
            _txtBookTitle.Text = "";
            _txtBookAuthor.Text = "";
            _txtBookIsbn.Text = "";
            _txtBookShelf.Text = "";
            _numBookCopies.Value = 3;
            if (_cmbBookCategory.Items.Count > 0) _cmbBookCategory.SelectedIndex = 0;
        }

        private void HandleSaveBook(object? sender, EventArgs e)
        {
            string title = _txtBookTitle.Text.Trim();
            string author = _txtBookAuthor.Text.Trim();
            string isbn = _txtBookIsbn.Text.Trim();
            string category = _cmbBookCategory.SelectedItem?.ToString() ?? "General";
            int copies = (int)_numBookCopies.Value;
            string shelf = string.IsNullOrWhiteSpace(_txtBookShelf.Text) ? "Shelf A-1" : _txtBookShelf.Text.Trim();

            if (string.IsNullOrWhiteSpace(title) || string.IsNullOrWhiteSpace(author) || string.IsNullOrWhiteSpace(isbn))
            {
                MessageBox.Show("Please enter Title, Author, and ISBN.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (_formMode == "Edit" && _editingBook != null)
            {
                _editingBook.Title = title;
                _editingBook.Author = author;
                _editingBook.ISBN = isbn;
                _editingBook.Category = category;
                _editingBook.TotalCopies = copies;
                _editingBook.ShelfLocation = shelf;

                MessageBox.Show($"Book '{title}' updated successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            else
            {
                int nextId = _service.GetNextBookId();
                var newBook = new Book(nextId, title, author, isbn, category, copies)
                {
                    ShelfLocation = shelf,
                    Publisher = "BookHaven Publishing",
                    PublicationYear = DateTime.Now.Year,
                    Description = "Added to BookHaven collection."
                };

                _service.Books.Add(newBook);
                MessageBox.Show($"Book '{title}' added to catalog!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                ClearForm();
            }

            FilterAndRenderBooks();
        }
        #endregion

        #region RESPONSIVE LAYOUT
        private void PerformResponsiveLayout()
        {
            _contentPanel.SuspendLayout();

            int pad = 24;
            int gap = 16;
            int availWidth = Math.Max(320, ClientSize.Width - (pad * 2));

            _contentPanel.Location = new Point(0, 0);
            _contentPanel.Width = ClientSize.Width;

            _lblTitle.Location = new Point(pad, 14);
            _tabHeaderPanel.Location = new Point(pad, 48);
            _tabHeaderPanel.Width = availWidth;

            int currentY = 90;

            // Search Card
            _searchCard.Bounds = new Rectangle(pad, currentY, availWidth, 110);
            if (availWidth >= 740)
            {
                _txtSearch.Width = availWidth - 360;
                _cmbCategory.Location = new Point(availWidth - 330, 28);
                _cmbCategory.Width = 190;
                _btnSearch.Location = new Point(availWidth - 120, 28);
            }
            else
            {
                _txtSearch.Width = availWidth - 36;
                _cmbCategory.Location = new Point(18, 68);
                _cmbCategory.Width = availWidth - 146;
                _btnSearch.Location = new Point(availWidth - 110, 68);
            }
            _categoryPillsPanel.Width = availWidth - 36;

            currentY += 110 + gap;

            int contentH = 560;

            if (availWidth >= 880)
            {
                // DESKTOP: 2 COLUMNS (Matches PDF Page 2)
                int invW = (int)(availWidth * 0.64f) - (gap / 2);
                int formW = availWidth - invW - gap;

                _inventoryCard.Visible = true;
                _formCard.Visible = true;

                _inventoryCard.Bounds = new Rectangle(pad, currentY, invW, contentH);
                _formCard.Bounds = new Rectangle(pad + invW + gap, currentY, formW, contentH);

                _inventoryListPanel.Bounds = new Rectangle(18, 70, invW - 36, contentH - 85);
                FilterAndRenderBooks();

                // Layout Form Inputs inside _formCard
                int fw = formW - 36;
                _txtBookTitle.Width = fw;
                _txtBookAuthor.Width = fw;
                _txtBookIsbn.Width = fw;
                _txtBookShelf.Width = fw;
                _cmbSelectBookToEdit.Width = fw;

                int halfW = (fw - 12) / 2;
                _cmbBookCategory.Width = halfW;
                _numBookCopies.Location = new Point(18 + halfW + 12, _numBookCopies.Top);
                _numBookCopies.Width = halfW;

                currentY += contentH + 30;
            }
            else
            {
                // MOBILE / NARROW: Stacked or Tabbed
                if (_activeTab == "Catalog")
                {
                    _inventoryCard.Visible = true;
                    _formCard.Visible = false;

                    _inventoryCard.Bounds = new Rectangle(pad, currentY, availWidth, contentH);
                    _inventoryListPanel.Bounds = new Rectangle(18, 70, availWidth - 36, contentH - 85);
                    FilterAndRenderBooks();
                    currentY += contentH + 30;
                }
                else
                {
                    // Add or Edit Tab active
                    _inventoryCard.Visible = false;
                    _formCard.Visible = true;

                    _formCard.Bounds = new Rectangle(pad, currentY, availWidth, contentH);
                    int fw = availWidth - 36;
                    _txtBookTitle.Width = fw;
                    _txtBookAuthor.Width = fw;
                    _txtBookIsbn.Width = fw;
                    _txtBookShelf.Width = fw;
                    _cmbSelectBookToEdit.Width = fw;

                    int halfW = (fw - 12) / 2;
                    _cmbBookCategory.Width = halfW;
                    _numBookCopies.Location = new Point(18 + halfW + 12, _numBookCopies.Top);
                    _numBookCopies.Width = halfW;
                    currentY += contentH + 30;
                }
            }

            _contentPanel.Height = currentY;
            AutoScrollMinSize = new Size(0, currentY);
            _contentPanel.ResumeLayout();
        }
        #endregion
    }
}
