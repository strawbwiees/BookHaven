using BookHaven.Models;
using BookHaven.Services;
using BookHaven.UI;

namespace BookHaven.Views
{
    public class BorrowedBooksView : UserControl
    {
        private readonly LibraryDataService _service = LibraryDataService.Instance;

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
                Font = new Font("Segoe UI", 18f, FontStyle.Bold),
                ForeColor = UITheme.NavyDark,
                AutoSize = true
            };
            _contentPanel.Controls.Add(_lblTitle);

            // 1. Table Card
            _tableCard = new CardPanel { BorderRadius = 16, Padding = new Padding(20) };

            _filterPillsPanel = new FlowLayoutPanel
            {
                Location = new Point(20, 16),
                Height = 38,
                FlowDirection = FlowDirection.LeftToRight
            };
            BuildFilterPills();
            _tableCard.Controls.Add(_filterPillsPanel);

            _btnExport = new PillButton
            {
                Text = "Export CSV",
                CustomBackColor = Color.White,
                CustomForeColor = UITheme.NavyDark,
                CustomBorderColor = UITheme.CardBorder,
                Size = new Size(115, 34)
            };
            _btnExport.Click += HandleExportCsv;
            _tableCard.Controls.Add(_btnExport);

            _gridTransactions = new DataGridView
            {
                Location = new Point(20, 65),
                Height = 330,
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
                RowTemplate = { Height = 40 },
                Font = UITheme.BodyFont
            };

            _gridTransactions.EnableHeadersVisualStyles = false;
            _gridTransactions.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(248, 250, 253);
            _gridTransactions.ColumnHeadersDefaultCellStyle.ForeColor = UITheme.TextMuted;
            _gridTransactions.ColumnHeadersDefaultCellStyle.Font = UITheme.SmallFont;
            _gridTransactions.ColumnHeadersHeight = 36;

            _gridTransactions.Columns.Add("Borrower", "BORROWER");
            _gridTransactions.Columns.Add("Book", "BOOK");
            _gridTransactions.Columns.Add("Issued", "ISSUED");
            _gridTransactions.Columns.Add("Due", "DUE");
            _gridTransactions.Columns.Add("Status", "STATUS");
            _gridTransactions.Columns.Add("Fine", "FINE");
            foreach (DataGridViewColumn col in _gridTransactions.Columns) col.MinimumWidth = 72; // scroll sideways on phones instead of crushing columns

            _gridTransactions.CellFormatting += GridTransactions_CellFormatting;
            _tableCard.Controls.Add(_gridTransactions);
            _contentPanel.Controls.Add(_tableCard);

            // 2. Reservation Queue Card
            _resCard = new CardPanel { BorderRadius = 16, Padding = new Padding(20) };

            var lblResTitle = new Label
            {
                Text = "Reservation queue",
                Font = UITheme.SubHeaderFont,
                ForeColor = UITheme.NavyDark,
                Location = new Point(20, 16),
                AutoSize = true
            };
            var lblResSub = new Label
            {
                Text = "Members waiting for a book that is out right now",
                Font = UITheme.SmallFont,
                ForeColor = UITheme.TextMuted,
                Location = new Point(20, 42),
                AutoSize = true
            };

            _btnAddReservation = new PillButton
            {
                Text = "+ Reserve Book",
                CustomBackColor = UITheme.SkyBlue,
                CustomForeColor = UITheme.NavyDark,
                Size = new Size(130, 34)
            };
            _btnAddReservation.Click += HandleAddReservation;

            _reservationQueuePanel = new FlowLayoutPanel
            {
                Location = new Point(20, 72),
                Height = 110,
                AutoScroll = true,
                FlowDirection = FlowDirection.LeftToRight
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

            int currentY = 56;
            int resH = 200;
            int gapY = 16;
            bool narrow = availWidth < 560;

            // The transactions table takes whatever height is left so the page fills the window.
            int tableH = Math.Max(420, ClientSize.Height - currentY - gapY - resH - 30);
            _tableCard.Bounds = new Rectangle(pad, currentY, availWidth, tableH);

            if (narrow)
            {
                // Filter pills wrap under the title row; export button drops below them.
                _filterPillsPanel.Width = availWidth - 40;
                _filterPillsPanel.Height = 68;
                _btnExport.Location = new Point(20, 16 + 70);
                _gridTransactions.Location = new Point(20, 16 + 70 + 44);
                _gridTransactions.Size = new Size(availWidth - 40, Math.Max(200, tableH - (16 + 70 + 44) - 20));
            }
            else
            {
                _filterPillsPanel.Height = 38;
                _filterPillsPanel.Width = Math.Max(150, availWidth - 170);
                _btnExport.Location = new Point(availWidth - 135, 14);
                _gridTransactions.Location = new Point(20, 65);
                _gridTransactions.Size = new Size(availWidth - 40, Math.Max(200, tableH - 65 - 25));
            }

            currentY += tableH + gapY;
            _resCard.Bounds = new Rectangle(pad, currentY, availWidth, resH);
            _btnAddReservation.Location = new Point(availWidth - 150, 16);
            _reservationQueuePanel.Width = availWidth - 40;

            currentY += resH + 30;
            _contentPanel.Height = currentY;
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
                var pill = new Button
                {
                    Text = text,
                    AutoSize = true,
                    Height = 30,
                    FlatStyle = FlatStyle.Flat,
                    Cursor = Cursors.Hand,
                    Font = new Font("Segoe UI", 8.5f, FontStyle.Bold),
                    Margin = new Padding(0, 0, 8, 0)
                };

                bool isSelected = _activeFilter == filterKey;
                pill.BackColor = isSelected ? UITheme.NavyDark : UITheme.SkyBlueLight;
                pill.ForeColor = isSelected ? Color.White : UITheme.NavyDark;
                pill.FlatAppearance.BorderSize = 0;

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
                e.CellStyle.Font = new Font("Segoe UI", 9.5f, FontStyle.Bold);
            }

            if (_gridTransactions.Columns[e.ColumnIndex].Name == "Fine" && e.Value is string fine && fine.Contains('₱'))
            {
                e.CellStyle!.ForeColor = UITheme.StatusRed;
                e.CellStyle.Font = new Font("Segoe UI", 9.5f, FontStyle.Bold);
            }
        }

        public void LoadReservations()
        {
            _reservationQueuePanel.Controls.Clear();

            foreach (var r in _service.Reservations)
            {
                var card = new CardPanel
                {
                    Size = new Size(280, 78),
                    BorderRadius = 12,
                    BackgroundFill = Color.FromArgb(249, 252, 255),
                    Margin = new Padding(0, 0, 12, 0)
                };

                var numCircle = new Label
                {
                    Text = $"#{r.QueueNumber}",
                    Font = new Font("Segoe UI", 9.5f, FontStyle.Bold),
                    BackColor = UITheme.SkyBlue,
                    ForeColor = UITheme.NavyDark,
                    Location = new Point(12, 18),
                    Size = new Size(42, 42),
                    TextAlign = ContentAlignment.MiddleCenter
                };

                var lblName = new Label
                {
                    Text = r.MemberName,
                    Font = UITheme.BodyBold,
                    ForeColor = UITheme.NavyDark,
                    Location = new Point(60, 14),
                    Size = new Size(210, 20),
                    AutoEllipsis = true
                };

                var lblBook = new Label
                {
                    Text = r.BookTitle,
                    Font = UITheme.SmallFont,
                    ForeColor = UITheme.TextMuted,
                    Location = new Point(60, 38),
                    Size = new Size(210, 20),
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
                Size = new Size(450, 300),
                StartPosition = FormStartPosition.CenterParent,
                FormBorderStyle = FormBorderStyle.FixedDialog,
                MaximizeBox = false,
                MinimizeBox = false,
                BackColor = UITheme.AppBackground
            };

            var lblMem = new Label { Text = "Select Member:", Location = new Point(24, 20), AutoSize = true, Font = UITheme.BodyBold };
            var cmbMem = new ComboBox { Location = new Point(24, 46), Size = new Size(380, 30), DropDownStyle = ComboBoxStyle.DropDownList, Font = UITheme.BodyFont };
            foreach (var m in _service.Members) cmbMem.Items.Add($"{m.FullName} ({m.MemberID})");
            if (cmbMem.Items.Count > 0) cmbMem.SelectedIndex = 0;

            var lblBk = new Label { Text = "Select Book:", Location = new Point(24, 90), AutoSize = true, Font = UITheme.BodyBold };
            var cmbBk = new ComboBox { Location = new Point(24, 116), Size = new Size(380, 30), DropDownStyle = ComboBoxStyle.DropDownList, Font = UITheme.BodyFont };
            foreach (var b in _service.Books) cmbBk.Items.Add($"{b.Title} (ISBN: {b.ISBN})");
            if (cmbBk.Items.Count > 0) cmbBk.SelectedIndex = 0;

            var btnSubmit = new PillButton
            {
                Text = "Confirm Reservation",
                CustomBackColor = UITheme.HotPink,
                CustomForeColor = Color.White,
                Location = new Point(24, 180),
                Size = new Size(200, 38)
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
