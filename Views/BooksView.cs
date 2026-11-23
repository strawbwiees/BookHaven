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

        // Slightly enlarged, crisp fonts for better readability
        private static readonly Font TableHeaderFont = new("Segoe UI", 8.5f, FontStyle.Bold);
        private static readonly Font RowTitleFont = new("Segoe UI", 10f, FontStyle.Bold);
        private static readonly Font RowSubFont = new("Segoe UI", 8.8f, FontStyle.Regular);
        private static readonly Font RowCategoryFont = new("Segoe UI", 9.5f, FontStyle.Regular);
        private static readonly Font CopiesFont = new("Segoe UI", 10.5f, FontStyle.Bold);
        private static readonly Font BadgeFont = new("Segoe UI", 8.5f, FontStyle.Bold);
        private static readonly Font RowButtonFont = new("Segoe UI", 9f, FontStyle.Bold);
        private static readonly Font FieldLabelFont = new("Segoe UI", 9f, FontStyle.Regular);

        // Container
        private Panel _contentPanel = null!;

        // Header and mode tabs
        private Label _lblTitle = null!;
        private FlowLayoutPanel _tabHeaderPanel = null!;
        private PillButton _btnTabCatalog = null!;
        private PillButton _btnTabAdd = null!;
        private PillButton _btnTabEdit = null!;
        private PillButton _btnTabArchived = null!;

        // Search & Filter card
        private CardPanel _searchCard = null!;
        private RoundedTextBox _txtSearch = null!;
        private ComboBox _cmbAuthor = null!;
        private ComboBox _cmbCategory = null!;
        private ComboBox _cmbSort = null!;
        private PillButton _btnSearch = null!;
        private string _activeAuthorFilter = "All";
        private string _activeCategoryFilter = "All";

        // Inventory (left column)
        private CardPanel _inventoryCard = null!;
        private Label _lblInventoryHeader = null!;
        private Label _lblInventorySub = null!;
        private Panel _tableHeaderRow = null!;
        private Panel _inventoryListPanel = null!;

        // Add / edit form (right column on desktop, its own tab on narrow windows)
        private CardPanel _formCard = null!;
        private Label _lblFormTitle = null!;
        private Panel _formScrollPanel = null!;
        private ComboBox _cmbSelectBookToEdit = null!;
        private Label _lblSelectBook = null!;

        private RoundedTextBox _txtBookTitle = null!;
        private RoundedTextBox _txtBookAuthor = null!;
        private RoundedTextBox _txtBookIsbn = null!;
        private ComboBox _cmbBookCategory = null!;
        private NumericUpDown _numBookCopies = null!;
        private Label _lblCopies = null!;
        private RoundedTextBox _txtBookShelf = null!;
        private PictureBox _picCoverPreview = null!;
        private PillButton _btnUploadCover = null!;
        private string? _selectedCoverPath = null;

        private readonly List<(Control control, int top)> _formFieldBase = new();
        private PillButton _btnSaveBook = null!;
        private PillButton _btnClearForm = null!;

        private string _activeTab = "Catalog";
        private string _formMode = "Add"; // "Add" or "Edit"
        private Book? _editingBook = null;
        private bool _showArchivedOnly = false;

        public BooksView()
        {
            InitializeComponent();
        }

        protected override Point ScrollToControl(Control activeControl)
        {
            return new Point(0, AutoScrollPosition.Y);
        }

        private static int LineHeight(Font font, int extra = 6) =>
            TextRenderer.MeasureText("Ag", font).Height + UITheme.S(extra);

        private void InitializeComponent()
        {
            BackColor = UITheme.AppBackground;
            Dock = DockStyle.Fill;
            AutoScroll = false;

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
                BackColor = Color.Transparent,
                AutoSize = true
            };
            _contentPanel.Controls.Add(_lblTitle);

            // Tabs: Catalog / Add / Edit / Archived
            _tabHeaderPanel = new FlowLayoutPanel
            {
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents = false,
                AutoSize = false,
                BackColor = Color.Transparent
            };

            _btnTabCatalog = CreateTabButton("All Books (Catalog)", isActive: true);
            _btnTabAdd = CreateTabButton("Add Book", isActive: false);
            _btnTabEdit = CreateTabButton("Edit Book", isActive: false);
            _btnTabArchived = CreateTabButton("Archived", isActive: false);

            _btnTabCatalog.Click += (s, e) => SwitchTab("Catalog");
            _btnTabAdd.Click += (s, e) => SwitchTab("Add");
            _btnTabEdit.Click += (s, e) => SwitchTab("Edit");
            _btnTabArchived.Click += (s, e) => SwitchTab("Archived");

            _tabHeaderPanel.Controls.AddRange(new Control[] { _btnTabCatalog, _btnTabAdd, _btnTabEdit, _btnTabArchived });
            _contentPanel.Controls.Add(_tabHeaderPanel);

            BuildSearchCard();
            BuildInventoryCard();
            BuildFormCard();

            _contentPanel.Controls.AddRange(new Control[] { _searchCard, _inventoryCard, _formCard });

            Resize += (s, e) => PerformResponsiveLayout();
            FilterAndRenderBooks();
            PerformResponsiveLayout();
        }

        #region TABS
        private PillButton CreateTabButton(string text, bool isActive)
        {
            var btn = new PillButton
            {
                Text = text,
                Font = new Font("Segoe UI", 9.5f, FontStyle.Bold),
                Margin = new Padding(0, 0, UITheme.S(10), 0)
            };
            StyleTab(btn, isActive);
            FitTab(btn, false);
            return btn;
        }

        private static void StyleTab(PillButton btn, bool active)
        {
            btn.CustomBackColor = active ? UITheme.NavyDark : Color.White;
            btn.CustomForeColor = active ? Color.White : UITheme.NavyDark;
            btn.CustomBorderColor = active ? UITheme.NavyDark : Color.FromArgb(205, 225, 245);
            btn.Invalidate();
        }

        private static void FitTab(PillButton btn, bool tight)
        {
            int textW = TextRenderer.MeasureText(btn.Text, btn.Font).Width;
            btn.Size = new Size(textW + UITheme.S(tight ? 26 : 40), UITheme.S(40));
        }

        public void SwitchTab(string tabName, Book? targetBook = null)
        {
            _activeTab = tabName;

            StyleTab(_btnTabCatalog, tabName == "Catalog");
            StyleTab(_btnTabAdd, tabName == "Add");
            StyleTab(_btnTabEdit, tabName == "Edit");
            StyleTab(_btnTabArchived, tabName == "Archived");

            _showArchivedOnly = tabName == "Archived";

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
        #endregion

        #region SEARCH & FILTER CARD
        private void BuildSearchCard()
        {
            _searchCard = new CardPanel
            {
                BorderRadius = 14
            };

            _txtSearch = new RoundedTextBox
            {
                BorderRadius = 12
            };
            _txtSearch.InnerTextBox.PlaceholderText = "Search by title, author or ISBN";
            _txtSearch.InnerTextChanged += (s, e) => FilterAndRenderBooks();

            _cmbAuthor = new ComboBox
            {
                DropDownStyle = ComboBoxStyle.DropDownList,
                Font = RowCategoryFont
            };
            RefreshAuthorFilterOptions();
            _cmbAuthor.SelectedIndexChanged += (s, e) =>
            {
                _activeAuthorFilter = _cmbAuthor.SelectedIndex <= 0 ? "All" : _cmbAuthor.SelectedItem!.ToString()!;
                FilterAndRenderBooks();
            };

            _cmbCategory = new ComboBox
            {
                DropDownStyle = ComboBoxStyle.DropDownList,
                Font = RowCategoryFont
            };
            RefreshGenreOptions();
            _cmbCategory.SelectedIndexChanged += (s, e) =>
            {
                _activeCategoryFilter = _cmbCategory.SelectedIndex <= 0 ? "All" : _cmbCategory.SelectedItem!.ToString()!;
                FilterAndRenderBooks();
            };

            _cmbSort = new ComboBox
            {
                DropDownStyle = ComboBoxStyle.DropDownList,
                Font = RowCategoryFont
            };
            _cmbSort.Items.AddRange(new object[]
            {
                "Title (A–Z)",
                "Title (Z–A)",
                "Author (A–Z)",
                "Genre (A–Z)"
            });
            _cmbSort.SelectedIndex = 0;
            _cmbSort.SelectedIndexChanged += (s, e) => FilterAndRenderBooks();

            _btnSearch = new PillButton
            {
                Text = "Search",
                CustomBackColor = UITheme.HotPink,
                CustomForeColor = Color.White
            };
            _btnSearch.Click += (s, e) => FilterAndRenderBooks();

            _searchCard.Controls.AddRange(new Control[] { _txtSearch, _cmbAuthor, _cmbCategory, _cmbSort, _btnSearch });
        }

        private void RefreshAuthorFilterOptions()
        {
            string current = _cmbAuthor.SelectedItem?.ToString() ?? "All authors";
            _cmbAuthor.Items.Clear();
            _cmbAuthor.Items.Add("All authors");

            foreach (var author in _service.Books
                .Select(b => b.Author)
                .Where(a => !string.IsNullOrWhiteSpace(a))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .OrderBy(a => a))
            {
                _cmbAuthor.Items.Add(author);
            }

            int idx = _cmbAuthor.Items.IndexOf(current);
            _cmbAuthor.SelectedIndex = idx >= 0 ? idx : 0;
        }

        private void RefreshGenreOptions()
        {
            string currentFilter = _cmbCategory.SelectedItem?.ToString() ?? "All genres";
            _cmbCategory.Items.Clear();
            _cmbCategory.Items.Add("All genres");

            var allGenres = _service.Settings.Categories
                .Concat(_service.Books.Select(b => b.Category))
                .Where(c => !string.IsNullOrWhiteSpace(c))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .OrderBy(c => c)
                .ToList();

            foreach (var genre in allGenres)
                _cmbCategory.Items.Add(genre);

            int idx = _cmbCategory.Items.IndexOf(currentFilter);
            _cmbCategory.SelectedIndex = idx >= 0 ? idx : 0;

            if (_cmbBookCategory != null)
            {
                string currentInput = _cmbBookCategory.Text;
                _cmbBookCategory.Items.Clear();
                foreach (var genre in allGenres)
                    _cmbBookCategory.Items.Add(genre);
                _cmbBookCategory.Text = currentInput;
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
                BackColor = Color.White,
                AutoSize = true
            };

            _lblInventorySub = new Label
            {
                Text = "",
                Font = RowSubFont,
                ForeColor = UITheme.TextMuted,
                BackColor = Color.White,
                AutoSize = true
            };

            _tableHeaderRow = new Panel
            {
                BackColor = Color.White
            };
            _tableHeaderRow.Paint += (s, e) =>
            {
                using var p = new Pen(Color.FromArgb(235, 242, 250), 1f);
                e.Graphics.DrawLine(p, 0, _tableHeaderRow.Height - 1, _tableHeaderRow.Width, _tableHeaderRow.Height - 1);
            };

            _inventoryListPanel = new Panel
            {
                AutoScroll = true,
                BackColor = Color.White
            };

            _inventoryCard.Controls.AddRange(new Control[] { _lblInventoryHeader, _lblInventorySub, _tableHeaderRow, _inventoryListPanel });
        }

        private void LayoutInventoryCard(int cardW, int cardH)
        {
            int px = UITheme.S(20);
            int y = UITheme.S(16);

            _lblInventoryHeader.Location = new Point(px, y);
            _lblInventorySub.Location = new Point(px, _lblInventoryHeader.Bottom + UITheme.S(4));

            int tableTop = _lblInventorySub.Bottom + UITheme.S(14);
            int headerH = LineHeight(TableHeaderFont, 10);
            _tableHeaderRow.Bounds = new Rectangle(px, tableTop, cardW - px * 2, headerH);

            int listTop = _tableHeaderRow.Bottom + UITheme.S(6);
            _inventoryListPanel.Bounds = new Rectangle(px, listTop, cardW - px * 2, Math.Max(UITheme.S(80), cardH - listTop - UITheme.S(16)));
        }

        private static bool IsCompact(int totalW) => totalW < UITheme.S(520);

        private static (int title, int cat, int copies, int status, int action) Columns(int totalW)
        {
            int title = (int)(totalW * 0.36f);
            int cat = (int)(totalW * 0.18f);
            int copies = (int)(totalW * 0.11f);
            int status = (int)(totalW * 0.19f);
            int action = totalW - title - cat - copies - status;
            return (title, cat, copies, status, action);
        }

        private void LayoutTableHeader(int totalWidth)
        {
            _tableHeaderRow.Controls.Clear();

            var (colTitle, colCat, colCopies, colStatus, colAction) = Columns(totalWidth);
            int lh = _tableHeaderRow.Height - UITheme.S(2);
            int titleX = UITheme.S(6) + UITheme.S(44) + UITheme.S(12);

            Label H(string text, int x, int w) => new Label
            {
                Text = text,
                Font = TableHeaderFont,
                ForeColor = UITheme.TextMuted,
                BackColor = Color.White,
                Location = new Point(x, 0),
                Size = new Size(Math.Max(10, w), lh),
                TextAlign = ContentAlignment.MiddleLeft
            };

            _tableHeaderRow.Controls.Add(H("TITLE & AUTHOR", titleX, colTitle - titleX));
            _tableHeaderRow.Controls.Add(H("GENRE", colTitle, colCat));
            _tableHeaderRow.Controls.Add(H("COPIES", colTitle + colCat, colCopies));
            _tableHeaderRow.Controls.Add(H("STATUS", colTitle + colCat + colCopies, colStatus));
            _tableHeaderRow.Controls.Add(H("ACTION", colTitle + colCat + colCopies + colStatus, colAction));
        }

        public void FilterAndRenderBooks()
        {
            _inventoryListPanel.SuspendLayout();
            _inventoryListPanel.Controls.Clear();

            string search = _txtSearch.Text?.Trim().ToLower() ?? string.Empty;

            var query = _service.Books.Where(b =>
            {
                bool matchesArchive = _showArchivedOnly ? b.Archived == true : b.Archived != true;
                if (!matchesArchive) return false;

                bool matchesSearch = string.IsNullOrWhiteSpace(search) ||
                                     b.Title.ToLower().Contains(search) ||
                                     b.Author.ToLower().Contains(search) ||
                                     b.ISBN.ToLower().Contains(search);

                bool matchesAuthor = _activeAuthorFilter == "All" ||
                                     b.Author.Equals(_activeAuthorFilter, StringComparison.OrdinalIgnoreCase);

                bool matchesGenre = _activeCategoryFilter == "All" ||
                                    b.Category.Equals(_activeCategoryFilter, StringComparison.OrdinalIgnoreCase);

                return matchesSearch && matchesAuthor && matchesGenre;
            });

            string sortOption = _cmbSort?.SelectedItem?.ToString() ?? "Title (A–Z)";
            var filtered = sortOption switch
            {
                "Title (Z–A)" => query.OrderByDescending(b => b.Title).ToList(),
                "Author (A–Z)" => query.OrderBy(b => b.Author).ThenBy(b => b.Title).ToList(),
                "Genre (A–Z)" => query.OrderBy(b => b.Category).ThenBy(b => b.Title).ToList(),
                _ => query.OrderBy(b => b.Title).ToList()
            };

            string viewLabel = _showArchivedOnly ? "Archived" : "Inventory";
            _lblInventoryHeader.Text = viewLabel;
            _lblInventorySub.Text = $"{filtered.Count} {(filtered.Count == 1 ? "title" : "titles")} shown · click a book for details";

            int totalW = Math.Max(UITheme.S(240), _inventoryListPanel.ClientSize.Width - SystemInformation.VerticalScrollBarWidth);
            bool compact = IsCompact(totalW);
            _tableHeaderRow.Visible = !compact;
            if (!compact) LayoutTableHeader(totalW);

            int y = 0;
            int rowH = compact ? UITheme.S(108) : UITheme.S(84);

            foreach (var book in filtered)
            {
                var row = CreateInventoryRow(book, totalW, rowH);
                row.Location = new Point(0, y);
                _inventoryListPanel.Controls.Add(row);
                y += rowH + UITheme.S(4);
            }

            if (filtered.Count == 0)
            {
                _inventoryListPanel.Controls.Add(new Label
                {
                    Text = "No books match your search.",
                    Font = RowCategoryFont,
                    ForeColor = UITheme.TextMuted,
                    BackColor = Color.White,
                    Location = new Point(0, UITheme.S(16)),
                    Size = new Size(totalW, LineHeight(RowCategoryFont, 8))
                });
            }

            _inventoryListPanel.ResumeLayout();
        }

        private static (string status, Color bg, Color fg) GetStatus(Book book)
        {
            if (book.AvailableCopies <= 0) return ("All borrowed", UITheme.StatusRedBg, UITheme.StatusRed);
            if (book.AvailableCopies <= 1) return ("Low stock", UITheme.StatusYellowBg, UITheme.StatusYellow);
            return ("Available", UITheme.StatusGreenBg, UITheme.StatusGreen);
        }

        private PillButton CreateDetailsButton(Book book, Size size, Point location)
        {
            var btn = new PillButton
            {
                Text = "Details",
                Font = RowButtonFont,
                CustomBackColor = UITheme.SkyBlueLight,
                CustomForeColor = UITheme.NavyDark,
                Size = size,
                Location = location
            };
            btn.Click += (s, e) => ShowBookDetails(book);
            return btn;
        }

        private Panel CreateInventoryRow(Book book, int totalW, int rowH)
        {
            if (IsCompact(totalW)) return CreateCompactInventoryRow(book, totalW, rowH);

            var row = new Panel
            {
                Size = new Size(totalW, rowH),
                BackColor = Color.White,
                Cursor = Cursors.Hand
            };

            var (colTitle, colCat, colCopies, colStatus, colAction) = Columns(totalW);

            int coverW = UITheme.S(44), coverH = UITheme.S(62);
            var picCover = new PictureBox
            {
                Location = new Point(UITheme.S(6), (rowH - coverH) / 2),
                Size = new Size(coverW, coverH),
                SizeMode = PictureBoxSizeMode.Zoom,
                Image = UITheme.GetBookCoverImage(book, coverW, coverH),
                Cursor = Cursors.Hand
            };
            picCover.Click += (s, e) => ShowBookDetails(book);

            int textX = picCover.Right + UITheme.S(12);
            int textW = Math.Max(UITheme.S(40), colTitle - textX - UITheme.S(8));
            int titleH = LineHeight(RowTitleFont, 4);
            int authorH = LineHeight(RowSubFont, 4);
            int blockTop = Math.Max(UITheme.S(6), (rowH - (titleH + authorH)) / 2);

            var lblTitle = new Label
            {
                Text = book.Title,
                Font = RowTitleFont,
                ForeColor = UITheme.NavyDark,
                BackColor = Color.White,
                Location = new Point(textX, blockTop),
                Size = new Size(textW, titleH),
                TextAlign = ContentAlignment.MiddleLeft,
                AutoEllipsis = true
            };
            lblTitle.Click += (s, e) => ShowBookDetails(book);

            var lblAuthor = new Label
            {
                Text = $"{book.Author} · {book.ISBN}",
                Font = RowSubFont,
                ForeColor = UITheme.TextMuted,
                BackColor = Color.White,
                Location = new Point(textX, blockTop + titleH),
                Size = new Size(textW, authorH),
                TextAlign = ContentAlignment.MiddleLeft,
                AutoEllipsis = true
            };
            lblAuthor.Click += (s, e) => ShowBookDetails(book);

            int catH = LineHeight(RowCategoryFont, 6);
            var lblCategory = new Label
            {
                Text = book.Category,
                Font = RowCategoryFont,
                ForeColor = UITheme.TextMain,
                BackColor = Color.White,
                Location = new Point(colTitle, (rowH - catH) / 2),
                Size = new Size(Math.Max(10, colCat - UITheme.S(8)), catH),
                TextAlign = ContentAlignment.MiddleLeft,
                AutoEllipsis = true
            };

            int copiesH = LineHeight(CopiesFont, 6);
            var lblCopies = new Label
            {
                Text = $"{book.AvailableCopies} / {book.TotalCopies}",
                Font = CopiesFont,
                ForeColor = UITheme.NavyDark,
                BackColor = Color.White,
                Location = new Point(colTitle + colCat, (rowH - copiesH) / 2),
                Size = new Size(Math.Max(10, colCopies - UITheme.S(6)), copiesH),
                TextAlign = ContentAlignment.MiddleLeft
            };

            var (status, bg, fg) = GetStatus(book);
            int badgeH = UITheme.S(28);
            var lblBadge = new Label
            {
                Text = status,
                Font = BadgeFont,
                BackColor = bg,
                ForeColor = fg,
                Location = new Point(colTitle + colCat + colCopies, (rowH - badgeH) / 2),
                Size = new Size(Math.Min(UITheme.S(102), Math.Max(10, colStatus - UITheme.S(8))), badgeH),
                TextAlign = ContentAlignment.MiddleCenter
            };

            int btnH = UITheme.S(34);
            int btnW = Math.Min(UITheme.S(94), Math.Max(UITheme.S(64), colAction - UITheme.S(8)));
            var btnDetails = CreateDetailsButton(book,
                new Size(btnW, btnH),
                new Point(colTitle + colCat + colCopies + colStatus, (rowH - btnH) / 2));

            row.Controls.AddRange(new Control[] { picCover, lblTitle, lblAuthor, lblCategory, lblCopies, lblBadge, btnDetails });
            row.Click += (s, e) => ShowBookDetails(book);

            row.Paint += (s, e) =>
            {
                using var p = new Pen(Color.FromArgb(240, 246, 253), 1f);
                e.Graphics.DrawLine(p, UITheme.S(4), rowH - 1, totalW - UITheme.S(4), rowH - 1);
            };

            return row;
        }

        private Panel CreateCompactInventoryRow(Book book, int totalW, int rowH)
        {
            var row = new Panel
            {
                Size = new Size(totalW, rowH),
                BackColor = Color.White,
                Cursor = Cursors.Hand
            };

            int coverW = UITheme.S(44), coverH = UITheme.S(62);
            var picCover = new PictureBox
            {
                Location = new Point(UITheme.S(6), (rowH - coverH) / 2),
                Size = new Size(coverW, coverH),
                SizeMode = PictureBoxSizeMode.Zoom,
                Image = UITheme.GetBookCoverImage(book, coverW, coverH),
                Cursor = Cursors.Hand
            };
            picCover.Click += (s, e) => ShowBookDetails(book);

            int textX = picCover.Right + UITheme.S(12);
            int rightPad = UITheme.S(8);
            int textW = Math.Max(UITheme.S(40), totalW - textX - rightPad);
            int titleH = LineHeight(RowTitleFont, 4);
            int authorH = LineHeight(RowSubFont, 4);
            int badgeH = UITheme.S(26);
            int blockH = titleH + authorH + UITheme.S(8) + badgeH;
            int top = Math.Max(UITheme.S(6), (rowH - blockH) / 2);

            var lblTitle = new Label
            {
                Text = book.Title,
                Font = RowTitleFont,
                ForeColor = UITheme.NavyDark,
                BackColor = Color.White,
                Location = new Point(textX, top),
                Size = new Size(textW, titleH),
                TextAlign = ContentAlignment.MiddleLeft,
                AutoEllipsis = true
            };
            lblTitle.Click += (s, e) => ShowBookDetails(book);

            var lblAuthor = new Label
            {
                Text = $"{book.Author} · {book.Category}",
                Font = RowSubFont,
                ForeColor = UITheme.TextMuted,
                BackColor = Color.White,
                Location = new Point(textX, top + titleH),
                Size = new Size(textW, authorH),
                TextAlign = ContentAlignment.MiddleLeft,
                AutoEllipsis = true
            };
            lblAuthor.Click += (s, e) => ShowBookDetails(book);

            var (status, bg, fg) = GetStatus(book);
            int badgeY = top + titleH + authorH + UITheme.S(8);
            int badgeW = UITheme.S(96);
            var lblBadge = new Label
            {
                Text = status,
                Font = BadgeFont,
                BackColor = bg,
                ForeColor = fg,
                Location = new Point(textX, badgeY),
                Size = new Size(badgeW, badgeH),
                TextAlign = ContentAlignment.MiddleCenter
            };

            int btnW = UITheme.S(76), btnH = UITheme.S(32);
            var btnDetails = CreateDetailsButton(book,
                new Size(btnW, btnH),
                new Point(totalW - btnW - rightPad, badgeY + (badgeH - btnH) / 2));

            int copiesX = lblBadge.Right + UITheme.S(10);
            var lblCopies = new Label
            {
                Text = $"{book.AvailableCopies} / {book.TotalCopies} copies",
                Font = RowTitleFont,
                ForeColor = UITheme.NavyDark,
                BackColor = Color.White,
                Location = new Point(copiesX, badgeY),
                Size = new Size(Math.Max(UITheme.S(40), btnDetails.Left - copiesX - UITheme.S(6)), badgeH),
                TextAlign = ContentAlignment.MiddleLeft,
                AutoEllipsis = true
            };

            row.Controls.AddRange(new Control[] { picCover, lblTitle, lblAuthor, lblBadge, lblCopies, btnDetails });
            row.Click += (s, e) => ShowBookDetails(book);
            row.Paint += (s, e) =>
            {
                using var p = new Pen(Color.FromArgb(240, 246, 253), 1f);
                e.Graphics.DrawLine(p, UITheme.S(4), rowH - 1, totalW - UITheme.S(4), rowH - 1);
            };
            return row;
        }

        private void ShowBookDetails(Book book)
        {
            using var modal = new BookDetailsModal(book);
            modal.RequestEdit += (s, b) => SwitchTab("Edit", b);
            modal.RequestIssue += (s, b) => NavigateRequested?.Invoke(this, $"Issue:{b.ISBN}");
            modal.ShowDialog(FindForm());
        }
        #endregion

        #region RIGHT COLUMN: ADD OR EDIT BOOK CARD
        private static Label FieldLabel(string text, int x, int y)
        {
            return new Label
            {
                Text = text,
                Font = FieldLabelFont,
                ForeColor = UITheme.TextMuted,
                BackColor = Color.White,
                Location = new Point(x, y),
                AutoSize = true
            };
        }

        private void BuildFormCard()
        {
            _formCard = new CardPanel
            {
                BorderRadius = 16
            };

            int px = UITheme.S(20);
            int lblGap = UITheme.S(22);
            int inputH = UITheme.S(42);
            int step = UITheme.S(70);

            _lblFormTitle = new Label
            {
                Text = "Add a book",
                Font = UITheme.SubHeaderFont,
                ForeColor = UITheme.NavyDark,
                BackColor = Color.White,
                Location = new Point(px, UITheme.S(16)),
                AutoSize = true
            };

            // Scrollable inner area so the form never pushes the whole page to scroll
            _formScrollPanel = new Panel
            {
                BackColor = Color.White,
                AutoScroll = true
            };

            int y0 = UITheme.S(4);

            // Book selector (visible only in Edit mode)
            _lblSelectBook = FieldLabel("Select book to edit", 0, y0);
            _lblSelectBook.Visible = false;

            _cmbSelectBookToEdit = new ComboBox
            {
                Location = new Point(0, y0 + lblGap),
                DropDownStyle = ComboBoxStyle.DropDownList,
                DisplayMember = "Title",
                Font = RowCategoryFont,
                Visible = false
            };
            _cmbSelectBookToEdit.SelectedIndexChanged += (s, e) =>
            {
                if (_cmbSelectBookToEdit.SelectedItem is Book b)
                    PopulateFieldsWithBook(b);
            };

            // Input fields
            int y = y0;
            var lblT = FieldLabel("Title", 0, y);
            _txtBookTitle = new RoundedTextBox { Location = new Point(0, y + lblGap), Height = inputH, BorderRadius = 12 };

            y += step;
            var lblA = FieldLabel("Author", 0, y);
            _txtBookAuthor = new RoundedTextBox { Location = new Point(0, y + lblGap), Height = inputH, BorderRadius = 12 };

            y += step;
            var lblI = FieldLabel("ISBN", 0, y);
            _txtBookIsbn = new RoundedTextBox { Location = new Point(0, y + lblGap), Height = inputH, BorderRadius = 12 };

            y += step;
            var lblC = FieldLabel("Genre", 0, y);
            _lblCopies = FieldLabel("Copies", UITheme.S(180), y);

            _cmbBookCategory = new ComboBox
            {
                Width = UITheme.S(165),
                DropDownStyle = ComboBoxStyle.DropDown,
                AutoCompleteMode = AutoCompleteMode.SuggestAppend,
                AutoCompleteSource = AutoCompleteSource.ListItems,
                Font = RowCategoryFont
            };
            _cmbBookCategory.Location = new Point(0, y + lblGap + (inputH - _cmbBookCategory.Height) / 2);
            RefreshGenreOptions();
            if (_cmbBookCategory.Items.Count > 0) _cmbBookCategory.SelectedIndex = 0;

            _numBookCopies = new NumericUpDown
            {
                Width = UITheme.S(80),
                Font = RowCategoryFont,
                Minimum = 1,
                Maximum = 999,
                Value = 3
            };
            _numBookCopies.Location = new Point(UITheme.S(180), y + lblGap + (inputH - _numBookCopies.Height) / 2);

            y += step;
            var lblShelf = FieldLabel("Shelf location", 0, y);
            _txtBookShelf = new RoundedTextBox { Location = new Point(0, y + lblGap), Height = inputH, BorderRadius = 12 };
            _txtBookShelf.InnerTextBox.PlaceholderText = "e.g. Shelf A-1";

            // Book cover photo upload
            y += step;
            var lblCover = FieldLabel("Book cover", 0, y);
            _picCoverPreview = new PictureBox
            {
                Location = new Point(0, y + lblGap),
                Size = new Size(UITheme.S(50), UITheme.S(68)),
                SizeMode = PictureBoxSizeMode.Zoom,
                BackColor = Color.FromArgb(242, 247, 253),
                BorderStyle = BorderStyle.FixedSingle
            };

            _btnUploadCover = new PillButton
            {
                Text = "Upload Photo",
                CustomBackColor = UITheme.SkyBlueLight,
                CustomForeColor = UITheme.NavyDark,
                Location = new Point(UITheme.S(64), y + lblGap + UITheme.S(15)),
                Size = new Size(UITheme.S(136), UITheme.S(38))
            };
            _btnUploadCover.Click += HandleUploadCover;

            _formFieldBase.Clear();
            foreach (var c in new Control[]
            {
                lblT, _txtBookTitle, lblA, _txtBookAuthor, lblI, _txtBookIsbn,
                lblC, _lblCopies, _cmbBookCategory, _numBookCopies,
                lblShelf, _txtBookShelf,
                lblCover, _picCoverPreview, _btnUploadCover
            })
            {
                _formFieldBase.Add((c, c.Top));
            }

            _formScrollPanel.Controls.AddRange(new Control[]
            {
                _lblSelectBook, _cmbSelectBookToEdit,
                lblT, _txtBookTitle,
                lblA, _txtBookAuthor,
                lblI, _txtBookIsbn,
                lblC, _lblCopies, _cmbBookCategory, _numBookCopies,
                lblShelf, _txtBookShelf,
                lblCover, _picCoverPreview, _btnUploadCover
            });

            // Footer buttons pinned at bottom of _formCard
            _btnSaveBook = new PillButton
            {
                Text = "Save book",
                CustomBackColor = UITheme.NavyDark,
                CustomForeColor = Color.White,
                Size = new Size(UITheme.S(136), UITheme.S(42))
            };
            _btnSaveBook.Click += HandleSaveBook;

            _btnClearForm = new PillButton
            {
                Text = "Clear",
                CustomBackColor = Color.White,
                CustomForeColor = UITheme.NavyDark,
                CustomBorderColor = Color.FromArgb(205, 225, 245),
                Size = new Size(UITheme.S(104), UITheme.S(42))
            };
            _btnClearForm.Click += (s, e) => ClearForm();

            _formCard.Controls.AddRange(new Control[] { _lblFormTitle, _formScrollPanel, _btnSaveBook, _btnClearForm });
        }

        private void HandleUploadCover(object? sender, EventArgs e)
        {
            using var ofd = new OpenFileDialog
            {
                Title = "Select Book Cover Photo",
                Filter = "Image Files|*.png;*.jpg;*.jpeg;*.bmp;*.webp"
            };

            if (ofd.ShowDialog() != DialogResult.OK) return;

            try
            {
                string coversDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources", "BookCovers");
                Directory.CreateDirectory(coversDir);

                string fileName = $"cover_{Guid.NewGuid():N}{Path.GetExtension(ofd.FileName)}";
                string destPath = Path.Combine(coversDir, fileName);
                File.Copy(ofd.FileName, destPath, true);

                _selectedCoverPath = destPath;

                using var img = Image.FromFile(destPath);
                _picCoverPreview.Image?.Dispose();
                _picCoverPreview.Image = new Bitmap(img);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Could not load image: {ex.Message}", "Upload Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void SetFormMode(string mode, Book? bookToEdit = null)
        {
            _formMode = mode;
            _editingBook = bookToEdit;

            bool isEdit = mode == "Edit";

            _lblFormTitle.Text = isEdit ? "Edit a book" : "Add a book";
            _btnSaveBook.Text = isEdit ? "Update book" : "Save book";
            _btnSaveBook.Invalidate();

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
            _cmbBookCategory.Text = book.Category;

            _selectedCoverPath = book.CoverImagePath;
            _picCoverPreview.Image?.Dispose();
            _picCoverPreview.Image = UITheme.GetBookCoverImage(book, UITheme.S(50), UITheme.S(68));
        }

        private void ClearForm()
        {
            _editingBook = null;
            _txtBookTitle.Text = "";
            _txtBookAuthor.Text = "";
            _txtBookIsbn.Text = "";
            _txtBookShelf.Text = "";
            _numBookCopies.Value = 3;
            _cmbBookCategory.Text = _cmbBookCategory.Items.Count > 0 ? _cmbBookCategory.Items[0]?.ToString() ?? "" : "";
            _selectedCoverPath = null;
            _picCoverPreview.Image?.Dispose();
            _picCoverPreview.Image = null;
        }

        private void HandleSaveBook(object? sender, EventArgs e)
        {
            string title = _txtBookTitle.Text?.Trim() ?? string.Empty;
            string author = _txtBookAuthor.Text?.Trim() ?? string.Empty;
            string isbn = _txtBookIsbn.Text?.Trim() ?? string.Empty;
            string category = string.IsNullOrWhiteSpace(_cmbBookCategory.Text) ? "General" : _cmbBookCategory.Text.Trim();
            int copies = (int)_numBookCopies.Value;
            string shelf = string.IsNullOrWhiteSpace(_txtBookShelf.Text) ? "Shelf A-1" : _txtBookShelf.Text.Trim();

            if (string.IsNullOrWhiteSpace(title) || string.IsNullOrWhiteSpace(author) || string.IsNullOrWhiteSpace(isbn))
            {
                MessageBox.Show("Please enter Title, Author, and ISBN.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (!_service.Settings.Categories.Any(c => c.Equals(category, StringComparison.OrdinalIgnoreCase)))
            {
                _service.Settings.Categories.Add(category);
            }

            if (_formMode == "Edit" && _editingBook != null)
            {
                _editingBook.Title = title;
                _editingBook.Author = author;
                _editingBook.ISBN = isbn;
                _editingBook.Category = category;
                if (!_editingBook.AdjustTotalCopies(copies))
                {
                    MessageBox.Show($"'{title}' has {_editingBook.TotalCopies - _editingBook.AvailableCopies} copies on loan, so it cannot be reduced to {copies}.", "Copies On Loan", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
                _editingBook.ShelfLocation = shelf;
                _editingBook.CoverImagePath = _selectedCoverPath;

                _service.UpdateBook(_editingBook);
                MessageBox.Show($"Book '{title}' updated successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            else
            {
                int nextId = _service.GetNextBookId();
                var newBook = new Book(nextId, title, author, isbn, category, copies)
                {
                    ShelfLocation = shelf,
                    CoverImagePath = _selectedCoverPath,
                    Publisher = "BookHaven Publishing",
                    PublicationYear = DateTime.Now.Year,
                    Description = "Added to BookHaven collection."
                };

                _service.AddBook(newBook);
                MessageBox.Show($"Book '{title}' added to catalog!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                ClearForm();
            }

            RefreshAuthorFilterOptions();
            RefreshGenreOptions();
            FilterAndRenderBooks();
        }
        #endregion

        #region RESPONSIVE LAYOUT
        private void LayoutFormCard(int cardW, int cardH)
        {
            int px = UITheme.S(20);
            int topY = _lblFormTitle.Bottom + UITheme.S(12);
            int footerH = _btnSaveBook.Height + UITheme.S(28);
            int scrollH = Math.Max(UITheme.S(120), cardH - topY - footerH);

            _formScrollPanel.Bounds = new Rectangle(px, topY, cardW - px * 2, scrollH);

            // Pin Save & Clear buttons cleanly at the bottom of the form card
            int btnY = cardH - _btnSaveBook.Height - UITheme.S(16);
            _btnSaveBook.Location = new Point(px, btnY);
            _btnClearForm.Location = new Point(_btnSaveBook.Right + UITheme.S(10), btnY);

            int fw = Math.Max(UITheme.S(160), _formScrollPanel.ClientSize.Width - SystemInformation.VerticalScrollBarWidth - UITheme.S(4));

            _txtBookTitle.Width = fw;
            _txtBookAuthor.Width = fw;
            _txtBookIsbn.Width = fw;
            _txtBookShelf.Width = fw;
            _cmbSelectBookToEdit.Width = fw;

            int gap = UITheme.S(12);
            int halfW = (fw - gap) / 2;
            _cmbBookCategory.Left = 0;
            _cmbBookCategory.Width = halfW;
            _numBookCopies.Left = halfW + gap;
            _numBookCopies.Width = halfW;
            _lblCopies.Left = halfW + gap;

            int offset = _formMode == "Edit" ? UITheme.S(66) : 0;
            foreach (var (control, top) in _formFieldBase)
                control.Top = top + offset;
        }

        private void PerformResponsiveLayout()
        {
            _contentPanel.SuspendLayout();

            int pad = ClientSize.Width < UITheme.S(600) ? UITheme.S(14) : UITheme.S(24);
            int gap = UITheme.S(14);
            int availWidth = Math.Max(UITheme.S(280), ClientSize.Width - (pad * 2));

            _contentPanel.Location = new Point(0, 0);
            _contentPanel.Size = new Size(ClientSize.Width, ClientSize.Height);

            bool tightTabs = availWidth < UITheme.S(460);
            _btnTabCatalog.Text = tightTabs ? "Catalog" : "All Books (Catalog)";
            foreach (var tab in new[] { _btnTabCatalog, _btnTabAdd, _btnTabEdit, _btnTabArchived })
                FitTab(tab, tightTabs);

            // Compact, clean top header
            _lblTitle.Location = new Point(pad, UITheme.S(16));
            int tabsTop = _lblTitle.Bottom + UITheme.S(10);
            _tabHeaderPanel.Bounds = new Rectangle(pad, tabsTop, availWidth, UITheme.S(44));

            int currentY = _tabHeaderPanel.Bottom + gap;

            // Search + Filters card
            int inner = UITheme.S(16);
            int ctrlH = UITheme.S(40);
            int btnW = UITheme.S(100);
            int g2 = UITheme.S(10);
            int cardH;

            if (availWidth >= UITheme.S(900))
            {
                cardH = UITheme.S(70);
                int rowY = (cardH - ctrlH) / 2;
                int authorW = UITheme.S(150);
                int genreW = UITheme.S(150);
                int sortW = UITheme.S(130);

                _btnSearch.Bounds = new Rectangle(availWidth - inner - btnW, rowY, btnW, ctrlH);
                _cmbSort.Width = sortW;
                _cmbSort.Location = new Point(_btnSearch.Left - g2 - sortW, (cardH - _cmbSort.Height) / 2);
                _cmbCategory.Width = genreW;
                _cmbCategory.Location = new Point(_cmbSort.Left - g2 - genreW, (cardH - _cmbCategory.Height) / 2);
                _cmbAuthor.Width = authorW;
                _cmbAuthor.Location = new Point(_cmbCategory.Left - g2 - authorW, (cardH - _cmbAuthor.Height) / 2);
                _txtSearch.Bounds = new Rectangle(inner, rowY, Math.Max(UITheme.S(140), _cmbAuthor.Left - g2 - inner), ctrlH);
            }
            else
            {
                cardH = inner + ctrlH + g2 + ctrlH + inner;
                _txtSearch.Bounds = new Rectangle(inner, inner, availWidth - inner * 2 - g2 - btnW, ctrlH);
                _btnSearch.Bounds = new Rectangle(availWidth - inner - btnW, inner, btnW, ctrlH);

                int row2 = inner + ctrlH + g2;
                int filterW = Math.Max(UITheme.S(80), (availWidth - inner * 2 - g2 * 2) / 3);
                _cmbAuthor.Width = filterW;
                _cmbAuthor.Location = new Point(inner, row2 + (ctrlH - _cmbAuthor.Height) / 2);
                _cmbCategory.Width = filterW;
                _cmbCategory.Location = new Point(_cmbAuthor.Right + g2, row2 + (ctrlH - _cmbCategory.Height) / 2);
                _cmbSort.Width = availWidth - inner - (_cmbCategory.Right + g2);
                _cmbSort.Location = new Point(_cmbCategory.Right + g2, row2 + (ctrlH - _cmbSort.Height) / 2);
            }

            _searchCard.Bounds = new Rectangle(pad, currentY, availWidth, cardH);
            currentY += cardH + gap;

            // Fill remaining viewport height cleanly — no outer page scrollbar needed
            int contentH = Math.Max(UITheme.S(340), ClientSize.Height - currentY - UITheme.S(18));

            if (availWidth >= UITheme.S(880))
            {
                int invW = (int)(availWidth * 0.64f) - (gap / 2);
                int formW = availWidth - invW - gap;

                _inventoryCard.Visible = true;
                _formCard.Visible = true;

                _inventoryCard.Bounds = new Rectangle(pad, currentY, invW, contentH);
                _formCard.Bounds = new Rectangle(pad + invW + gap, currentY, formW, contentH);

                LayoutInventoryCard(invW, contentH);
                FilterAndRenderBooks();
                LayoutFormCard(formW, contentH);
            }
            else
            {
                if (_activeTab == "Catalog" || _activeTab == "Archived")
                {
                    _inventoryCard.Visible = true;
                    _formCard.Visible = false;

                    _inventoryCard.Bounds = new Rectangle(pad, currentY, availWidth, contentH);
                    LayoutInventoryCard(availWidth, contentH);
                    FilterAndRenderBooks();
                }
                else
                {
                    _inventoryCard.Visible = false;
                    _formCard.Visible = true;

                    _formCard.Bounds = new Rectangle(pad, currentY, availWidth, contentH);
                    LayoutFormCard(availWidth, contentH);
                }
            }

            _contentPanel.ResumeLayout();
        }
        #endregion
    }
}