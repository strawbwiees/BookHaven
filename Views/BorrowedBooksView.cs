using BookHaven.Models;
using BookHaven.Services;
using BookHaven.UI;

namespace BookHaven.Views
{
    public class BorrowedBooksView : UserControl
    {
        private readonly LibraryDataService _service = LibraryDataService.Instance;

        // Enlarged, readable fonts matching the rest of the app
        private static readonly Font PageTitleFont = new("League Spartan", 22f, FontStyle.Bold);
        private static readonly Font CardHeaderFont = new("League Spartan", 15f, FontStyle.Bold);
        private static readonly Font CardSubFont = new("League Spartan", 10f, FontStyle.Regular);
        private static readonly Font FilterPillFont = new("League Spartan", 9.5f, FontStyle.Bold);
        private static readonly Font GridHeaderFont = new("League Spartan", 9.5f, FontStyle.Bold);
        private static readonly Font GridCellFont = new("League Spartan", 10.5f, FontStyle.Regular);
        private static readonly Font GridBoldCellFont = new("League Spartan", 10.5f, FontStyle.Bold);
        private static readonly Font ButtonFont = new("League Spartan", 10.5f, FontStyle.Bold);
        private static readonly Font ResNumFont = new("League Spartan", 11f, FontStyle.Bold);
        private static readonly Font ResNameFont = new("League Spartan", 10.5f, FontStyle.Bold);
        private static readonly Font ResBookFont = new("League Spartan", 9.5f, FontStyle.Regular);

        private Panel _contentPanel = null!;
        private Label _lblTitle = null!;
        private CardPanel _tableCard = null!;
        private CardPanel _resCard = null!;
        private FlowLayoutPanel _filterPillsPanel = null!;
        private FlowLayoutPanel _reservationQueuePanel = null!;
        private DataGridView _gridTransactions = null!;
        private PillButton _btnExport = null!;
        private PillButton _btnAddReservation = null!;
        private string _activeFilter = "All";

        public BorrowedBooksView()
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
                Text = "BORROWED BOOKS!",
                Font = PageTitleFont,
                ForeColor = UITheme.NavyDark,
                BackColor = Color.Transparent,
                AutoSize = true
            };
            _contentPanel.Controls.Add(_lblTitle);

            // 1. Table Card
            _tableCard = new CardPanel { BorderRadius = 16, Padding = new Padding(22) };

            _filterPillsPanel = new FlowLayoutPanel
            {
                Location = new Point(22, 18),
                Height = 44,
                FlowDirection = FlowDirection.LeftToRight,
                BackColor = Color.White
            };
            BuildFilterPills();
            _tableCard.Controls.Add(_filterPillsPanel);

            _btnExport = new PillButton
            {
                Text = "Export CSV",
                Font = ButtonFont,
                CustomBackColor = Color.White,
                CustomForeColor = UITheme.NavyDark,
                CustomBorderColor = UITheme.CardBorder,
                Size = new Size(136, 40)
            };
            _btnExport.Click += HandleExportCsv;
            _tableCard.Controls.Add(_btnExport);

            _gridTransactions = new DataGridView
            {
                Location = new Point(22, 74),
                Height = 350,
                BackgroundColor = Color.White,
                BorderStyle = BorderStyle.None,
                CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal,
                GridColor = Color.FromArgb(240, 244, 250),
                RowHeadersVisible = false,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                ReadOnly = true,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                RowTemplate = { Height = 48 },
                Font = GridCellFont
            };

            _gridTransactions.EnableHeadersVisualStyles = false;
            _gridTransactions.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(248, 250, 253);
            _gridTransactions.ColumnHeadersDefaultCellStyle.ForeColor = UITheme.TextMuted;
            _gridTransactions.ColumnHeadersDefaultCellStyle.Font = GridHeaderFont;
            _gridTransactions.ColumnHeadersDefaultCellStyle.Padding = new Padding(6, 0, 6, 0);
            _gridTransactions.ColumnHeadersHeight = 44;

            _gridTransactions.DefaultCellStyle.Padding = new Padding(6, 4, 6, 4);
            _gridTransactions.DefaultCellStyle.SelectionBackColor = Color.FromArgb(234, 244, 253);
            _gridTransactions.DefaultCellStyle.SelectionForeColor = UITheme.NavyDark;

            _gridTransactions.Columns.Add("Borrower", "BORROWER");
            _gridTransactions.Columns.Add("Book", "BOOK");
            _gridTransactions.Columns.Add("Issued", "ISSUED");
            _gridTransactions.Columns.Add("Due", "DUE");
            _gridTransactions.Columns.Add("Status", "STATUS");
            _gridTransactions.Columns.Add("Fine", "FINE");
            foreach (DataGridViewColumn col in _gridTransactions.Columns) col.MinimumWidth = 84;

            _gridTransactions.CellFormatting += GridTransactions_CellFormatting;
            _tableCard.Controls.Add(_gridTransactions);
            _contentPanel.Controls.Add(_tableCard);

            // 2. Reservation Queue Card
            _resCard = new CardPanel { BorderRadius = 16, Padding = new Padding(22) };

            var lblResTitle = new Label
            {
                Text = "Reservation queue",
                Font = CardHeaderFont,
                ForeColor = UITheme.NavyDark,
                BackColor = Color.White,
                Location = new Point(22, 18),
                AutoSize = true
            };
            var lblResSub = new Label
            {
                Text = "Members waiting for a book that is out right now",
                Font = CardSubFont,
                ForeColor = UITheme.TextMuted,
                BackColor = Color.White,
                Location = new Point(22, 48),
                AutoSize = true
            };

            _btnAddReservation = new PillButton
            {
                Text = "+ Reserve Book",
                Font = ButtonFont,
                CustomBackColor = UITheme.SkyBlue,
                CustomForeColor = UITheme.NavyDark,
                Size = new Size(164, 40)
            };
            _btnAddReservation.Click += HandleAddReservation;

            _reservationQueuePanel = new FlowLayoutPanel
            {
                Location = new Point(22, 84),
                Height = 122,
                AutoScroll = true,
                FlowDirection = FlowDirection.LeftToRight,
                BackColor = Color.White
            };

            _resCard.Controls.AddRange(new Control[] { lblResTitle, lblResSub, _btnAddReservation, _reservationQueuePanel });
            _contentPanel.Controls.Add(_resCard);

            Resize += (s, e) => PerformResponsiveLayout();
            LoadTransactionsData();
            LoadReservations();
            PerformResponsiveLayout();
        }

        private void PerformResponsiveLayout()
        {
            _contentPanel.SuspendLayout();

            int pad = ClientSize.Width < 600 ? 14 : 24;
            int availWidth = Math.Max(280, ClientSize.Width - (pad * 2));

            _contentPanel.Location = new Point(0, 0);
            _contentPanel.Width = ClientSize.Width;

            _lblTitle.Location = new Point(pad, 16);

            int currentY = _lblTitle.Bottom + 16;
            int resH = 224;
            int gapY = 16;
            bool narrow = availWidth < 640;

            int tableH = Math.Max(440, ClientSize.Height - currentY - gapY - resH - 24);
            _tableCard.Bounds = new Rectangle(pad, currentY, availWidth, tableH);

            if (narrow)
            {
                _filterPillsPanel.Width = availWidth - 44;
                _filterPillsPanel.Height = 82;
                _btnExport.Location = new Point(22, _filterPillsPanel.Bottom + 8);
                int gridTop = _btnExport.Bottom + 12;
                _gridTransactions.Location = new Point(22, gridTop);
                _gridTransactions.Size = new Size(availWidth - 44, Math.Max(220, tableH - gridTop - 22));
            }
            else
            {
                _filterPillsPanel.Height = 44;
                _filterPillsPanel.Width = Math.Max(180, availWidth - 190);
                _btnExport.Location = new Point(availWidth - _btnExport.Width - 22, 18);
                _gridTransactions.Location = new Point(22, 74);
                _gridTransactions.Size = new Size(availWidth - 44, Math.Max(220, tableH - 96));
            }

            currentY += tableH + gapY;
            _resCard.Bounds = new Rectangle(pad, currentY, availWidth, resH);
            _btnAddReservation.Location = new Point(availWidth - _btnAddReservation.Width - 22, 18);
            _reservationQueuePanel.Width = availWidth - 44;

            currentY += resH + 24;
            _contentPanel.Height = currentY;
            AutoScrollMinSize = new Size(0, currentY);
            _contentPanel.ResumeLayout();
        }

        private void BuildFilterPills()
        {
            _filterPillsPanel.Controls.Clear();

            int onLoanCount = _service.Transactions.Count(t => t.Status == "Borrowed" || t.Status == "Overdue");
            int overdueCount = _service.Transactions.Count(t => t.Status == "Overdue");
            int returnedCount = _service.Transactions.Count(t => t.Status == "Returned");

            var tabs = new (string text, string filterKey)[]
            {
                ($"All • {_service.Transactions.Count}", "All"),
                ($"On loan • {onLoanCount}", "OnLoan"),
                ($"Overdue • {overdueCount}", "Overdue"),
                ($"Returned • {returnedCount}", "Returned")
            };

            foreach (var (text, filterKey) in tabs)
            {
                bool isSelected = _activeFilter == filterKey;
                int pillW = TextRenderer.MeasureText(text, FilterPillFont).Width + 32;

                var pill = new PillButton
                {
                    Text = text,
                    Font = FilterPillFont,
                    Size = new Size(pillW, 36),
                    BorderRadius = 10,
                    CustomBackColor = isSelected ? UITheme.NavyDark : UITheme.SkyBlueLight,
                    CustomForeColor = isSelected ? Color.White : UITheme.NavyDark,
                    Margin = new Padding(0, 2, 10, 6)
                };

                pill.Click += (s, e) =>
                {
                    _activeFilter = filterKey;
                    BuildFilterPills();
                    LoadTransactionsData();
                };

                _filterPillsPanel.Controls.Add(pill);
            }
        }

        public void LoadTransactionsData()
        {
            _gridTransactions.Rows.Clear();

            var list = _service.Transactions.AsEnumerable();
            if (_activeFilter == "OnLoan")
                list = list.Where(t => t.Status == "Borrowed" || t.Status == "Overdue");
            else if (_activeFilter == "Overdue")
                list = list.Where(t => t.Status == "Overdue");
            else if (_activeFilter == "Returned")
                list = list.Where(t => t.Status == "Returned");

            foreach (var tx in list)
            {
                string fineText = tx.FineAmount > 0 ? $"₱{tx.FineAmount:N2}" : "—";
                _gridTransactions.Rows.Add(
                    tx.BorrowerName,
                    tx.BookTitle,
                    tx.IssueDate.ToString("MMM dd"),
                    tx.DueDate.ToString("MMM dd"),
                    tx.Status,
                    fineText
                );
            }
        }

        private void GridTransactions_CellFormatting(object? sender, DataGridViewCellFormattingEventArgs e)
        {
            if (e.RowIndex < 0) return;

            if (_gridTransactions.Columns[e.ColumnIndex].Name == "Status" && e.Value is string status)
            {
                if (status == "Overdue") e.CellStyle!.ForeColor = UITheme.StatusRed;
                else if (status == "Returned") e.CellStyle!.ForeColor = UITheme.StatusGreen;
                else e.CellStyle!.ForeColor = Color.FromArgb(2, 132, 199);
                e.CellStyle.Font = GridBoldCellFont;
            }

            if (_gridTransactions.Columns[e.ColumnIndex].Name == "Fine" && e.Value is string fine && fine.Contains('₱'))
            {
                e.CellStyle!.ForeColor = UITheme.StatusRed;
                e.CellStyle.Font = GridBoldCellFont;
            }
        }

        public void LoadReservations()
        {
            _reservationQueuePanel.Controls.Clear();

            Color cardBg = Color.FromArgb(249, 252, 255);
            int nameH = LineHeight(ResNameFont, 4);
            int bookH = LineHeight(ResBookFont, 4);

            foreach (var r in _service.Reservations)
            {
                var card = new CardPanel
                {
                    Size = new Size(310, 92),
                    BorderRadius = 12,
                    BackgroundFill = cardBg,
                    Margin = new Padding(0, 0, 14, 0)
                };

                var numCircle = new Label
                {
                    Text = $"#{r.QueueNumber}",
                    Font = ResNumFont,
                    BackColor = UITheme.SkyBlue,
                    ForeColor = UITheme.NavyDark,
                    Location = new Point(14, (card.Height - 46) / 2),
                    Size = new Size(46, 46),
                    TextAlign = ContentAlignment.MiddleCenter
                };

                int blockTop = (card.Height - (nameH + bookH + 2)) / 2;
                var lblName = new Label
                {
                    Text = r.MemberName,
                    Font = ResNameFont,
                    ForeColor = UITheme.NavyDark,
                    BackColor = cardBg,
                    Location = new Point(70, blockTop),
                    Size = new Size(224, nameH),
                    TextAlign = ContentAlignment.MiddleLeft,
                    AutoEllipsis = true
                };

                var lblBook = new Label
                {
                    Text = r.BookTitle,
                    Font = ResBookFont,
                    ForeColor = UITheme.TextMuted,
                    BackColor = cardBg,
                    Location = new Point(70, blockTop + nameH + 2),
                    Size = new Size(224, bookH),
                    TextAlign = ContentAlignment.MiddleLeft,
                    AutoEllipsis = true
                };

                card.Controls.AddRange(new Control[] { numCircle, lblName, lblBook });
                _reservationQueuePanel.Controls.Add(card);
            }
        }

        private void HandleAddReservation(object? sender, EventArgs e)
        {
            using var dlg = new Form
            {
                Text = "Add Book Reservation",
                Size = new Size(480, 330),
                StartPosition = FormStartPosition.CenterParent,
                FormBorderStyle = FormBorderStyle.FixedDialog,
                MaximizeBox = false,
                MinimizeBox = false,
                BackColor = UITheme.AppBackground
            };

            var lblMem = new Label { Text = "Select Member:", Location = new Point(26, 22), AutoSize = true, Font = ResNameFont, ForeColor = UITheme.NavyDark };
            var cmbMem = new ComboBox { Location = new Point(26, 52), Size = new Size(408, 34), DropDownStyle = ComboBoxStyle.DropDownList, Font = GridCellFont };
            foreach (var m in _service.Members) cmbMem.Items.Add($"{m.FullName} ({m.MemberID})");
            if (cmbMem.Items.Count > 0) cmbMem.SelectedIndex = 0;

            var lblBk = new Label { Text = "Select Book:", Location = new Point(26, 104), AutoSize = true, Font = ResNameFont, ForeColor = UITheme.NavyDark };
            var cmbBk = new ComboBox { Location = new Point(26, 134), Size = new Size(408, 34), DropDownStyle = ComboBoxStyle.DropDownList, Font = GridCellFont };
            foreach (var b in _service.Books) cmbBk.Items.Add($"{b.Title} (ISBN: {b.ISBN})");
            if (cmbBk.Items.Count > 0) cmbBk.SelectedIndex = 0;

            var btnSubmit = new PillButton
            {
                Text = "Confirm Reservation",
                Font = ButtonFont,
                CustomBackColor = UITheme.HotPink,
                CustomForeColor = Color.White,
                Location = new Point(26, 204),
                Size = new Size(220, 44)
            };
            btnSubmit.Click += (s, args) =>
            {
                if (cmbMem.SelectedIndex < 0 || cmbBk.SelectedIndex < 0) return;
                var mem = _service.Members[cmbMem.SelectedIndex];
                var book = _service.Books[cmbBk.SelectedIndex];

                string res = _service.ReserveBook(mem.MemberID, book.ISBN);
                MessageBox.Show(res, "Reservation", MessageBoxButtons.OK, MessageBoxIcon.Information);
                dlg.Close();
                LoadReservations();
                BuildFilterPills();
            };

            dlg.Controls.AddRange(new Control[] { lblMem, cmbMem, lblBk, cmbBk, btnSubmit });
            dlg.ShowDialog(FindForm());
        }

        private void HandleExportCsv(object? sender, EventArgs e)
        {
            using var sfd = new SaveFileDialog
            {
                Filter = "CSV Files (*.csv)|*.csv",
                FileName = $"BookHaven_Transactions_{DateTime.Now:yyyyMMdd}.csv"
            };

            if (sfd.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    using var writer = new StreamWriter(sfd.FileName);
                    writer.WriteLine("TransactionID,BorrowerID,BorrowerName,ISBN,BookTitle,IssueDate,DueDate,ReturnDate,Status,FineAmount");
                    foreach (var tx in _service.Transactions)
                    {
                        writer.WriteLine($"\"{tx.TransactionID}\",\"{tx.BorrowerID}\",\"{tx.BorrowerName}\",\"{tx.BookISBN}\",\"{tx.BookTitle}\",\"{tx.IssueDate:yyyy-MM-dd}\",\"{tx.DueDate:yyyy-MM-dd}\",\"{(tx.ReturnDate.HasValue ? tx.ReturnDate.Value.ToString("yyyy-MM-dd") : "")}\",\"{tx.Status}\",{tx.FineAmount}");
                    }
                    MessageBox.Show("Transactions exported successfully!", "Export CSV", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Failed to export: {ex.Message}", "Export Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }
    }
}