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

        private static readonly Font CopiesFont = new("Segoe UI", 10.5f, FontStyle.Bold);
        private static readonly Font BadgeFont = new("Segoe UI", 8f, FontStyle.Bold);
        private static readonly Font RowButtonFont = new("Segoe UI", 8.5f, FontStyle.Bold);

        // Container
        private Panel _contentPanel = null!;

        // Header and mode tabs
        private Label _lblTitle = null!;
        private FlowLayoutPanel _tabHeaderPanel = null!;
        private PillButton _btnTabCatalog = null!;
        private PillButton _btnTabAdd = null!;
        private PillButton _btnTabEdit = null!;
        private PillButton _btnTabArchived = null!;

        // Search card
        private CardPanel _searchCard = null!;
        private RoundedTextBox _txtSearch = null!;
        private ComboBox _cmbCategory = null!;
        private PillButton _btnSearch = null!;
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
        private ComboBox _cmbSelectBookToEdit = null!;
        private Label _lblSelectBook = null!;

        private RoundedTextBox _txtBookTitle = null!;
        private RoundedTextBox _txtBookAuthor = null!;
        private RoundedTextBox _txtBookIsbn = null!;
        private ComboBox _cmbBookCategory = null!;
        private NumericUpDown _numBookCopies = null!;
        private Label _lblCopies = null!;
        private RoundedTextBox _txtBookShelf = null!;
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
                BackColor = Color.Transparent,
                AutoSize = true
            };
            _contentPanel.Controls.Add(_lblTitle);

            // Tabs: Catalog / Add / Edit
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

            // Build components
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
                Margin = new Padding(0, 0, UITheme.S(12), 0)
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
            btn.Size = new Size(textW + UITheme.S(tight ? 28 : 44), UITheme.S(42));
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

        #region SEARCH CARD
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

            _cmbCategory = new ComboBox
            {
                DropDownStyle = ComboBoxStyle.DropDownList,
                Font = UITheme.BodyFont
            };
            _cmbCategory.Items.Add("All categories");
            foreach (var cat in _service.Settings.Categories) _cmbCategory.Items.Add(cat);
            _cmbCategory.SelectedIndex = 0;
            _cmbCategory.SelectedIndexChanged += (s, e) =>
            {
                _activeCategoryFilter = _cmbCategory.SelectedIndex == 0 ? "All" : _cmbCategory.SelectedItem!.ToString()!;
                FilterAndRenderBooks();
            };

            _btnSearch = new PillButton
            {
                Text = "Search",
                CustomBackColor = UITheme.HotPink,
                CustomForeColor = Color.White
            };
            _btnSearch.Click += (s, e) => FilterAndRenderBooks();

            _searchCard.Controls.AddRange(new Control[] { _txtSearch, _cmbCategory, _btnSearch });
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
                Font = UITheme.SmallFont,
                ForeColor = UITheme.TextMuted,
                BackColor = Color.White,
                AutoSize = true
            };

            // Table header row
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
            int y = UITheme.S(18);

            _lblInventoryHeader.Location = new Point(px, y);
            _lblInventorySub.Location = new Point(px, y + UITheme.SubHeaderFont.Height + UITheme.S(2));

            int tableTop = _lblInventorySub.Top + UITheme.SmallFont.Height + UITheme.S(14);
            _tableHeaderRow.Bounds = new Rectangle(px, tableTop, cardW - px * 2, UITheme.S(28));

            int listTop = _tableHeaderRow.Bottom + UITheme.S(4);
            _inventoryListPanel.Bounds = new Rectangle(px, listTop, cardW - px * 2, Math.Max(UITheme.S(80), cardH - listTop - UITheme.S(16)));
        }

        private static bool IsCompact(int totalW) => totalW < UITheme.S(520);

        private static (int title, int cat, int copies, int status, int action) Columns(int totalW)
        {
            int title = (int)(totalW * 0.36f);
            int cat = (int)(totalW * 0.18f);
            int copies = (int)(totalW * 0.11f);
            int status = (int)(totalW * 0.20f);
            int action = totalW - title - cat - copies - status;
            return (title, cat, copies, status, action);
        }

        private void LayoutTableHeader(int totalWidth)
        {
            _tableHeaderRow.Controls.Clear();

            var (colTitle, colCat, colCopies, colStatus, colAction) = Columns(totalWidth);
            int lh = UITheme.SmallFont.Height + 2;
            int ly = Math.Max(0, (_tableHeaderRow.Height - lh) / 2);
            int titleX = UITheme.S(6) + UITheme.S(42) + UITheme.S(12);

            Label H(string text, int x, int w) => new Label
            {
                Text = text,
                Font = UITheme.SmallFont,
                ForeColor = UITheme.TextMuted,
                BackColor = Color.White,
                Location = new Point(x, ly),
                Size = new Size(Math.Max(10, w), lh)
            };

            _tableHeaderRow.Controls.Add(H("TITLE & AUTHOR", titleX, colTitle - titleX));
            _tableHeaderRow.Controls.Add(H("CATEGORY", colTitle, colCat));
            _tableHeaderRow.Controls.Add(H("COPIES", colTitle + colCat, colCopies));
            _tableHeaderRow.Controls.Add(H("STATUS", colTitle + colCat + colCopies, colStatus));
            _tableHeaderRow.Controls.Add(H("ACTION", colTitle + colCat + colCopies + colStatus, colAction));
        }

        public void FilterAndRenderBooks()
        {
            _inventoryListPanel.SuspendLayout();
            _inventoryListPanel.Controls.Clear();

            string search = _txtSearch.Text?.Trim().ToLower() ?? string.Empty;
            var filtered = _service.Books.Where(b =>
            {
                // Archived tab shows only archived books; catalog shows only active books
                if (_showArchivedOnly)
                    return b.Archived == true &&
                           (string.IsNullOrWhiteSpace(search) ||
                            b.Title.ToLower().Contains(search) ||
                            b.Author.ToLower().Contains(search) ||
                            b.ISBN.ToLower().Contains(search));
                else
                    return b.Archived != true &&
                           (string.IsNullOrWhiteSpace(search) ||
                            b.Title.ToLower().Contains(search) ||
                            b.Author.ToLower().Contains(search) ||
                            b.ISBN.ToLower().Contains(search)) &&
                           (_activeCategoryFilter == "All" || b.Category.Equals(_activeCategoryFilter, StringComparison.OrdinalIgnoreCase));
            }).ToList();

            string viewLabel = _showArchivedOnly ? "Archived" : "Inventory";
            _lblInventoryHeader.Text = viewLabel;
            _lblInventorySub.Text = $"{filtered.Count} {(filtered.Count == 1 ? "title" : "titles")} shown · click a book for details";

            int totalW = Math.Max(UITheme.S(240), _inventoryListPanel.ClientSize.Width - SystemInformation.VerticalScrollBarWidth);
            bool compact = IsCompact(totalW);
            _tableHeaderRow.Visible = !compact;
            if (!compact) LayoutTableHeader(totalW);

            int y = 0;
            int rowH = compact ? UITheme.S(100) : UITheme.S(76);

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
                    Font = UITheme.BodyFont,
                    ForeColor = UITheme.TextMuted,
                    BackColor = Color.White,
                    Location = new Point(0, UITheme.S(16)),
                    Size = new Size(totalW, UITheme.BodyFont.Height + UITheme.S(8))
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

            // Cover thumbnail
            int coverW = UITheme.S(42), coverH = UITheme.S(58);
            var picCover = new PictureBox
            {
                Location = new Point(UITheme.S(6), (rowH - coverH) / 2),
                Size = new Size(coverW, coverH),
                SizeMode = PictureBoxSizeMode.Zoom,
                Image = UITheme.GetBookCoverImage(book, coverW, coverH),
                Cursor = Cursors.Hand
            };
            picCover.Click += (s, e) => ShowBookDetails(book);

            // Title + author stacked and centred in the row
            int textX = picCover.Right + UITheme.S(12);
            int textW = Math.Max(UITheme.S(40), colTitle - textX - UITheme.S(8));
            int titleH = UITheme.BodyBold.Height + UITheme.S(2);
            int authorH = UITheme.SmallFont.Height + UITheme.S(2);
            int blockTop = (rowH - (titleH + authorH)) / 2;

            var lblTitle = new Label
            {
                Text = book.Title,
                Font = UITheme.BodyBold,
                ForeColor = UITheme.NavyDark,
                BackColor = Color.White,
                Location = new Point(textX, blockTop),
                Size = new Size(textW, titleH),
                AutoEllipsis = true
            };
            lblTitle.Click += (s, e) => ShowBookDetails(book);

            var lblAuthor = new Label
            {
                Text = $"{book.Author} · {book.ISBN}",
                Font = UITheme.SmallFont,
                ForeColor = UITheme.TextMuted,
                BackColor = Color.White,
                Location = new Point(textX, blockTop + titleH),
                Size = new Size(textW, authorH),
                AutoEllipsis = true
            };
            lblAuthor.Click += (s, e) => ShowBookDetails(book);

            // Category
            int catH = UITheme.BodyFont.Height + UITheme.S(4);
            var lblCategory = new Label
            {
                Text = book.Category,
                Font = UITheme.BodyFont,
                ForeColor = UITheme.TextMain,
                BackColor = Color.White,
                Location = new Point(colTitle, (rowH - catH) / 2),
                Size = new Size(Math.Max(10, colCat - UITheme.S(8)), catH),
                AutoEllipsis = true
            };

            // Copies
            int copiesH = CopiesFont.Height + UITheme.S(4);
            var lblCopies = new Label
            {
                Text = $"{book.AvailableCopies} / {book.TotalCopies}",
                Font = CopiesFont,
                ForeColor = UITheme.NavyDark,
                BackColor = Color.White,
                Location = new Point(colTitle + colCat, (rowH - copiesH) / 2),
                Size = new Size(Math.Max(10, colCopies - UITheme.S(6)), copiesH)
            };

            // Status badge
            var (status, bg, fg) = GetStatus(book);
            int badgeH = UITheme.S(26);
            var lblBadge = new Label
            {
                Text = status,
                Font = BadgeFont,
                BackColor = bg,
                ForeColor = fg,
                Location = new Point(colTitle + colCat + colCopies, (rowH - badgeH) / 2),
                Size = new Size(Math.Min(UITheme.S(96), Math.Max(10, colStatus - UITheme.S(8))), badgeH),
                TextAlign = ContentAlignment.MiddleCenter
            };

            // Action button
            int btnH = UITheme.S(32);
            int btnW = Math.Min(UITheme.S(90), Math.Max(UITheme.S(60), colAction - UITheme.S(8)));
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

        // Narrow layout: title/author on top, stock status + copies + Details underneath.
        private Panel CreateCompactInventoryRow(Book book, int totalW, int rowH)
        {
            var row = new Panel
            {
                Size = new Size(totalW, rowH),
                BackColor = Color.White,
                Cursor = Cursors.Hand
            };

            int coverW = UITheme.S(42), coverH = UITheme.S(60);
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
            int titleH = UITheme.BodyBold.Height + UITheme.S(2);
            int authorH = UITheme.SmallFont.Height + UITheme.S(2);
            int badgeH = UITheme.S(24);
            int blockH = titleH + authorH + UITheme.S(8) + badgeH;
            int top = (rowH - blockH) / 2;

            var lblTitle = new Label
            {
                Text = book.Title,
                Font = UITheme.BodyBold,
                ForeColor = UITheme.NavyDark,
                BackColor = Color.White,
                Location = new Point(textX, top),
                Size = new Size(textW, titleH),
                AutoEllipsis = true
            };
            lblTitle.Click += (s, e) => ShowBookDetails(book);

            var lblAuthor = new Label
            {
                Text = $"{book.Author} · {book.Category}",
                Font = UITheme.SmallFont,
                ForeColor = UITheme.TextMuted,
                BackColor = Color.White,
                Location = new Point(textX, top + titleH),
                Size = new Size(textW, authorH),
                AutoEllipsis = true
            };
            lblAuthor.Click += (s, e) => ShowBookDetails(book);

            var (status, bg, fg) = GetStatus(book);
            int badgeY = top + titleH + authorH + UITheme.S(8);
            int badgeW = UITheme.S(92);
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

            int btnW = UITheme.S(72), btnH = UITheme.S(30);
            var btnDetails = CreateDetailsButton(book,
                new Size(btnW, btnH),
                new Point(totalW - btnW - rightPad, badgeY + (badgeH - btnH) / 2));

            int copiesX = lblBadge.Right + UITheme.S(10);
            var lblCopies = new Label
            {
                Text = $"{book.AvailableCopies} / {book.TotalCopies} copies",
                Font = UITheme.BodyBold,
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
        private static Label FieldLabel(string text, int x, int y)
        {
            return new Label
            {
                Text = text,
                Font = UITheme.SmallFont,
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
            int lblGap = UITheme.S(20);
            int inputH = UITheme.S(42);
            int step = UITheme.S(72);

            _lblFormTitle = new Label
            {
                Text = "Add a book",
                Font = UITheme.SubHeaderFont,
                ForeColor = UITheme.NavyDark,
                BackColor = Color.White,
                Location = new Point(px, UITheme.S(18)),
                AutoSize = true
            };

            int y0 = UITheme.S(18) + UITheme.SubHeaderFont.Height + UITheme.S(20);

            // Book selector (visible only in Edit mode)
            _lblSelectBook = FieldLabel("Select book to edit", px, y0);
            _lblSelectBook.Visible = false;

            _cmbSelectBookToEdit = new ComboBox
            {
                Location = new Point(px, y0 + lblGap),
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

            // Input fields: each row is label, then input, with even spacing
            int y = y0;
            var lblT = FieldLabel("Title", px, y);
            _txtBookTitle = new RoundedTextBox { Location = new Point(px, y + lblGap), Height = inputH, BorderRadius = 12 };

            y += step;
            var lblA = FieldLabel("Author", px, y);
            _txtBookAuthor = new RoundedTextBox { Location = new Point(px, y + lblGap), Height = inputH, BorderRadius = 12 };

            y += step;
            var lblI = FieldLabel("ISBN", px, y);
            _txtBookIsbn = new RoundedTextBox { Location = new Point(px, y + lblGap), Height = inputH, BorderRadius = 12 };

            y += step;
            var lblC = FieldLabel("Category", px, y);
            _lblCopies = FieldLabel("Copies", px + UITheme.S(190), y);

            _cmbBookCategory = new ComboBox
            {
                Width = UITheme.S(170),
                DropDownStyle = ComboBoxStyle.DropDownList,
                Font = UITheme.BodyFont
            };
            _cmbBookCategory.Location = new Point(px, y + lblGap + (inputH - _cmbBookCategory.Height) / 2);
            foreach (var cat in _service.Settings.Categories) _cmbBookCategory.Items.Add(cat);
            if (_cmbBookCategory.Items.Count > 0) _cmbBookCategory.SelectedIndex = 0;

            _numBookCopies = new NumericUpDown
            {
                Width = UITheme.S(80),
                Font = UITheme.BodyFont,
                Minimum = 1,
                Maximum = 999,
                Value = 3
            };
            _numBookCopies.Location = new Point(px + UITheme.S(190), y + lblGap + (inputH - _numBookCopies.Height) / 2);

            y += step;
            var lblShelf = FieldLabel("Shelf location", px, y);
            _txtBookShelf = new RoundedTextBox { Location = new Point(px, y + lblGap), Height = inputH, BorderRadius = 12 };
            _txtBookShelf.InnerTextBox.PlaceholderText = "e.g. Shelf A-1";

            // Buttons
            y += step + UITheme.S(8);
            _btnSaveBook = new PillButton
            {
                Text = "Save book",
                CustomBackColor = UITheme.NavyDark,
                CustomForeColor = Color.White,
                Location = new Point(px, y),
                Size = new Size(UITheme.S(140), UITheme.S(44))
            };
            _btnSaveBook.Click += HandleSaveBook;

            _btnClearForm = new PillButton
            {
                Text = "Clear",
                CustomBackColor = Color.White,
                CustomForeColor = UITheme.NavyDark,
                CustomBorderColor = Color.FromArgb(205, 225, 245),
                Location = new Point(px + UITheme.S(152), y),
                Size = new Size(UITheme.S(110), UITheme.S(44))
            };
            _btnClearForm.Click += (s, e) => ClearForm();

            // Remember each field's resting position so the layout can shift them below the
            // "select book" row in Edit mode and pin the buttons to the card's bottom edge.
            _formFieldBase.Clear();
            foreach (var c in new Control[]
            {
                lblT, _txtBookTitle, lblA, _txtBookAuthor, lblI, _txtBookIsbn,
                lblC, _lblCopies, _cmbBookCategory, _numBookCopies,
                lblShelf, _txtBookShelf, _btnSaveBook, _btnClearForm
            })
            {
                _formFieldBase.Add((c, c.Top));
            }

            _formCard.Controls.AddRange(new Control[]
            {
                _lblFormTitle, _lblSelectBook, _cmbSelectBookToEdit,
                lblT, _txtBookTitle,
                lblA, _txtBookAuthor,
                lblI, _txtBookIsbn,
                lblC, _lblCopies, _cmbBookCategory, _numBookCopies,
                lblShelf, _txtBookShelf,
                _btnSaveBook, _btnClearForm
            });
        }

        private void SetFormMode(string mode, Book? bookToEdit = null)
        {
            _formMode = mode;
            _editingBook = bookToEdit;

            bool isEdit = mode == "Edit";

            _lblFormTitle.Text = isEdit ? "Edit a book" : "Add a book";
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
            string title = _txtBookTitle.Text?.Trim() ?? string.Empty;
            string author = _txtBookAuthor.Text?.Trim() ?? string.Empty;
            string isbn = _txtBookIsbn.Text?.Trim() ?? string.Empty;
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
                if (!_editingBook.AdjustTotalCopies(copies))
                {
                    MessageBox.Show($"'{title}' has {_editingBook.TotalCopies - _editingBook.AvailableCopies} copies on loan, so it cannot be reduced to {copies}.", "Copies On Loan", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
                _editingBook.ShelfLocation = shelf;

                _service.UpdateBook(_editingBook);
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

                _service.AddBook(newBook);
                MessageBox.Show($"Book '{title}' added to catalog!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                ClearForm();
            }

            FilterAndRenderBooks();
        }
        #endregion

        #region RESPONSIVE LAYOUT
        private void LayoutFormFields()
        {
            // Edit mode shows a "Select book to edit" combo above the Title field.
            int offset = _formMode == "Edit" ? UITheme.S(66) : 0;
            foreach (var (control, top) in _formFieldBase)
                control.Top = top + offset;

            // Buttons hug the bottom of the card when it is taller than the form needs.
            int btnTop = Math.Max(_formFieldBase[^1].top + offset, _formCard.Height - _btnSaveBook.Height - UITheme.S(24));
            _btnSaveBook.Top = btnTop;
            _btnClearForm.Top = btnTop;
        }

        private void LayoutFormCard(int cardW)
        {
            int px = UITheme.S(20);
            int fw = cardW - px * 2;

            _txtBookTitle.Width = fw;
            _txtBookAuthor.Width = fw;
            _txtBookIsbn.Width = fw;
            _txtBookShelf.Width = fw;
            _cmbSelectBookToEdit.Width = fw;

            int gap = UITheme.S(14);
            int halfW = (fw - gap) / 2;
            _cmbBookCategory.Left = px;
            _cmbBookCategory.Width = halfW;
            _numBookCopies.Left = px + halfW + gap;
            _numBookCopies.Width = halfW;
            _lblCopies.Left = px + halfW + gap;

            LayoutFormFields();
        }

        private void PerformResponsiveLayout()
        {
            _contentPanel.SuspendLayout();

            int pad = ClientSize.Width < UITheme.S(600) ? UITheme.S(14) : UITheme.S(28);
            int gap = UITheme.S(18);
            int availWidth = Math.Max(UITheme.S(280), ClientSize.Width - (pad * 2));

            _contentPanel.Location = new Point(0, 0);
            _contentPanel.Width = ClientSize.Width;

            // Shorter tab labels on phone widths so the three tabs stay on one row
            bool tightTabs = availWidth < UITheme.S(420);
            _btnTabCatalog.Text = tightTabs ? "Catalog" : "All Books (Catalog)";
            foreach (var tab in new[] { _btnTabCatalog, _btnTabAdd, _btnTabEdit })
                FitTab(tab, tightTabs);

            // Header: title, then tabs placed from the title's real height (no overlap)
            _lblTitle.Location = new Point(pad, UITheme.S(22));
            int tabsTop = _lblTitle.Top + UITheme.HeaderFont.Height + UITheme.S(14);
            _tabHeaderPanel.Bounds = new Rectangle(pad, tabsTop, availWidth, UITheme.S(46));

            int currentY = _tabHeaderPanel.Bottom + gap;

            // Search card: one clean row on wide windows, two rows on narrow ones
            int inner = UITheme.S(18);
            int ctrlH = UITheme.S(42);
            int btnW = UITheme.S(110);
            int comboW = UITheme.S(210);
            int g2 = UITheme.S(12);
            int cardH;

            if (availWidth >= UITheme.S(700))
            {
                cardH = UITheme.S(78);
                int rowY = (cardH - ctrlH) / 2;
                _btnSearch.Bounds = new Rectangle(availWidth - inner - btnW, rowY, btnW, ctrlH);
                _cmbCategory.Width = comboW;
                _cmbCategory.Location = new Point(_btnSearch.Left - g2 - comboW, (cardH - _cmbCategory.Height) / 2);
                _txtSearch.Bounds = new Rectangle(inner, rowY, _cmbCategory.Left - g2 - inner, ctrlH);
            }
            else
            {
                cardH = inner + ctrlH + g2 + ctrlH + inner;
                _txtSearch.Bounds = new Rectangle(inner, inner, availWidth - inner * 2, ctrlH);
                int row2 = inner + ctrlH + g2;
                _btnSearch.Bounds = new Rectangle(availWidth - inner - btnW, row2, btnW, ctrlH);
                _cmbCategory.Width = Math.Max(UITheme.S(100), _btnSearch.Left - g2 - inner);
                _cmbCategory.Location = new Point(inner, row2 + (ctrlH - _cmbCategory.Height) / 2);
            }

            _searchCard.Bounds = new Rectangle(pad, currentY, availWidth, cardH);
            currentY += cardH + gap;

            int contentH = Math.Max(UITheme.S(560), ClientSize.Height - currentY - UITheme.S(30));

            if (availWidth >= UITheme.S(880))
            {
                // DESKTOP: two columns
                int invW = (int)(availWidth * 0.64f) - (gap / 2);
                int formW = availWidth - invW - gap;

                _inventoryCard.Visible = true;
                _formCard.Visible = true;

                _inventoryCard.Bounds = new Rectangle(pad, currentY, invW, contentH);
                _formCard.Bounds = new Rectangle(pad + invW + gap, currentY, formW, contentH);

                LayoutInventoryCard(invW, contentH);
                FilterAndRenderBooks();
                LayoutFormCard(formW);

                currentY += contentH + UITheme.S(30);
            }
            else
            {
                // NARROW: one card at a time, switched by the tabs
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
                    LayoutFormCard(availWidth);
                }

                currentY += contentH + UITheme.S(30);
            }

            _contentPanel.Height = currentY;
            AutoScrollMinSize = new Size(0, currentY);
            _contentPanel.ResumeLayout();
        }
        #endregion
    }
}
