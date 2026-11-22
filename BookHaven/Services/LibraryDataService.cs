using BookHaven.Models;
using Microsoft.Data.SqlClient;

namespace BookHaven.Services
{
    public class LibraryDataService
    {
        private static LibraryDataService? _instance;
        public static LibraryDataService Instance => _instance ??= new LibraryDataService();

        public List<Book> Books { get; } = new();
        public List<Member> Members { get; } = new();
        public List<BorrowTransaction> Transactions { get; } = new();
        public List<Reservation> Reservations { get; } = new();
        public List<ActivityLog> Activities { get; } = new();
        public LibrarySettings Settings { get; } = new();
        public Librarian CurrentAdmin { get; } = new();

        public LibraryDataService()
        {
            try
            {
                CoverImageGenerator.EnsureCoversGenerated(AppDomain.CurrentDomain.BaseDirectory);
                string projDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..", "..");
                if (Directory.Exists(projDir)) CoverImageGenerator.EnsureCoversGenerated(projDir);
            }
            catch { }

            Database.EnsureSchema();
            LoadFromDatabase();
        }

        private void LoadFromDatabase()
        {
            LoadBooks();
            LoadMembers();
            LoadTransactions();
            LoadReservations();
            LoadActivities();
            LoadSettings();
            LoadAdmin();
        }

        private void LoadBooks()
        {
            using var conn = Database.Open();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = "SELECT ItemID, Title, Author, ISBN, Category, TotalCopies, AvailableCopies, Publisher, PublicationYear, ShelfLocation, CoverColorHex, Description FROM dbo.Books";
            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                var book = new Book(
                    reader.GetInt32(0),
                    reader.GetString(1),
                    reader.GetString(2),
                    reader.GetString(3),
                    reader.GetString(4),
                    reader.GetInt32(5),
                    reader.GetInt32(6))
                {
                    Publisher = reader.GetString(7),
                    PublicationYear = reader.GetInt32(8),
                    ShelfLocation = reader.GetString(9),
                    CoverColorHex = reader.GetString(10),
                    Description = reader.GetString(11)
                };
                Books.Add(book);
            }
        }

        private void LoadMembers()
        {
            using var conn = Database.Open();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = "SELECT MemberID, StudentID, FullName, Email, CourseAndYear, RegisteredDate, MaxLoans, CurrentLoansCount, OutstandingFines FROM dbo.Members";
            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                var regDate = reader.IsDBNull(5) ? DateTime.Now : DateTime.Parse(reader.GetString(5));
                Members.Add(new Member
                {
                    MemberID = reader.GetString(0),
                    StudentID = reader.GetString(1),
                    FullName = reader.GetString(2),
                    Email = reader.GetString(3),
                    CourseAndYear = reader.GetString(4),
                    RegisteredDate = regDate,
                    MaxLoans = reader.GetInt32(6),
                    CurrentLoansCount = reader.GetInt32(7),
                    OutstandingFines = (decimal)reader.GetFloat(8)
                });
            }
        }

        private void LoadTransactions()
        {
            using var conn = Database.Open();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = "SELECT TransactionID, BorrowerID, BorrowerName, BookISBN, BookTitle, IssueDate, DueDate, ReturnDate, Status, FineAmount, DaysLate FROM dbo.Transactions";
            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                var tx = new BorrowTransaction
                {
                    TransactionID = reader.GetString(0),
                    BorrowerID = reader.GetString(1),
                    BorrowerName = reader.GetString(2),
                    BookISBN = reader.GetString(3),
                    BookTitle = reader.GetString(4),
                    IssueDate = DateTime.Parse(reader.GetString(5)),
                    DueDate = DateTime.Parse(reader.GetString(6)),
                    ReturnDate = reader.IsDBNull(7) ? null : DateTime.Parse(reader.GetString(7)),
                    Status = reader.GetString(8),
                    FineAmount = (decimal)reader.GetFloat(9),
                    DaysLate = reader.GetInt32(10)
                };
                Transactions.Add(tx);
            }
        }

        private void LoadReservations()
        {
            using var conn = Database.Open();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = "SELECT ReservationID, MemberID, MemberName, BookISBN, BookTitle, ReservationDate, QueueNumber, Status FROM dbo.Reservations";
            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                Reservations.Add(new Reservation
                {
                    ReservationID = reader.GetInt32(0),
                    MemberID = reader.GetString(1),
                    MemberName = reader.GetString(2),
                    BookISBN = reader.GetString(3),
                    BookTitle = reader.GetString(4),
                    ReservationDate = DateTime.Parse(reader.GetString(5)),
                    QueueNumber = reader.GetInt32(6),
                    Status = reader.GetString(7)
                });
            }
        }

        private void LoadActivities()
        {
            using var conn = Database.Open();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = "SELECT TimeText, Description, ActionType, TagText FROM dbo.Activities ORDER BY Id DESC";
            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                Activities.Add(new ActivityLog
                {
                    TimeText = reader.GetString(0),
                    Description = reader.GetString(1),
                    ActionType = reader.GetString(2),
                    TagText = reader.GetString(3)
                });
            }
        }

        private void LoadSettings()
        {
            using var conn = Database.Open();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = "SELECT [Key], Value FROM dbo.Settings";
            using var reader = cmd.ExecuteReader();
            var categories = new List<string>();
            while (reader.Read())
            {
                var key = reader.GetString(0);
                var value = reader.GetString(1);

                if (key == "LoanPeriodDays" && int.TryParse(value, out var lpd)) Settings.LoanPeriodDays = lpd;
                else if (key == "FinePerDayLate" && decimal.TryParse(value, out var fpdl)) Settings.FinePerDayLate = fpdl;
                else if (key == "MaxBooksPerBorrower" && int.TryParse(value, out var mbpb)) Settings.MaxBooksPerBorrower = mbpb;
                else if (key == "CalculateFinesAutomatically") Settings.CalculateFinesAutomatically = bool.Parse(value);
                else if (key == "BlockNewLoansWhileFineUnpaid") Settings.BlockNewLoansWhileFineUnpaid = bool.Parse(value);
                else if (key.StartsWith("Category:")) categories.Add(key["Category:".Length..]);
            }

            if (categories.Count > 0) Settings.Categories = categories;
        }

        private void LoadAdmin()
        {
            using var conn = Database.Open();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = "SELECT LibrarianID, Username, Password, DisplayName, Role FROM dbo.Admin";
            using var reader = cmd.ExecuteReader();
            if (reader.Read())
            {
                CurrentAdmin.LibrarianID = reader.GetInt32(0);
                CurrentAdmin.Username = reader.GetString(1);
                CurrentAdmin.Password = reader.GetString(2);
                CurrentAdmin.DisplayName = reader.GetString(3);
                CurrentAdmin.Role = reader.GetString(4);
            }
        }

        private void AddBook(Book book)
        {
            if (book.ItemID == 0)
                book.ItemID = GetNextBookId();
            Books.Add(book);
            using var conn = Database.Open();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = @"
                INSERT INTO dbo.Books (ItemID, Title, Author, ISBN, Category, TotalCopies, AvailableCopies, Publisher, PublicationYear, ShelfLocation, CoverColorHex, Description)
                VALUES (@ItemID, @Title, @Author, @ISBN, @Category, @TotalCopies, @AvailableCopies, @Publisher, @PublicationYear, @ShelfLocation, @CoverColorHex, @Description)
            ";
            cmd.Parameters.AddWithValue("@ItemID", book.ItemID);
            cmd.Parameters.AddWithValue("@Title", book.Title);
            cmd.Parameters.AddWithValue("@Author", book.Author);
            cmd.Parameters.AddWithValue("@ISBN", book.ISBN);
            cmd.Parameters.AddWithValue("@Category", book.Category);
            cmd.Parameters.AddWithValue("@TotalCopies", book.TotalCopies);
            cmd.Parameters.AddWithValue("@AvailableCopies", book.AvailableCopies);
            cmd.Parameters.AddWithValue("@Publisher", book.Publisher);
            cmd.Parameters.AddWithValue("@PublicationYear", book.PublicationYear);
            cmd.Parameters.AddWithValue("@ShelfLocation", book.ShelfLocation);
            cmd.Parameters.AddWithValue("@CoverColorHex", book.CoverColorHex);
            cmd.Parameters.AddWithValue("@Description", book.Description);
            cmd.ExecuteNonQuery();
        }

        private void AddMember(Member member)
        {
            Members.Add(member);
            using var conn = Database.Open();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = @"
                INSERT INTO dbo.Members (MemberID, StudentID, FullName, Email, CourseAndYear, RegisteredDate, MaxLoans, CurrentLoansCount, OutstandingFines)
                VALUES (@MemberID, @StudentID, @FullName, @Email, @CourseAndYear, @RegisteredDate, @MaxLoans, @CurrentLoansCount, @OutstandingFines)
            ";
            cmd.Parameters.AddWithValue("@MemberID", member.MemberID);
            cmd.Parameters.AddWithValue("@StudentID", member.StudentID);
            cmd.Parameters.AddWithValue("@FullName", member.FullName);
            cmd.Parameters.AddWithValue("@Email", member.Email);
            cmd.Parameters.AddWithValue("@CourseAndYear", member.CourseAndYear);
            cmd.Parameters.AddWithValue("@RegisteredDate", member.RegisteredDate.ToString("o"));
            cmd.Parameters.AddWithValue("@MaxLoans", member.MaxLoans);
            cmd.Parameters.AddWithValue("@CurrentLoansCount", member.CurrentLoansCount);
            cmd.Parameters.AddWithValue("@OutstandingFines", (double)member.OutstandingFines);
            cmd.ExecuteNonQuery();
        }

        private void AddTransaction(BorrowTransaction tx)
        {
            Transactions.Add(tx);
            using var conn = Database.Open();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = @"
                INSERT INTO dbo.Transactions (TransactionID, BorrowerID, BorrowerName, BookISBN, BookTitle, IssueDate, DueDate, ReturnDate, Status, FineAmount, DaysLate)
                VALUES (@TransactionID, @BorrowerID, @BorrowerName, @BookISBN, @BookTitle, @IssueDate, @DueDate, @ReturnDate, @Status, @FineAmount, @DaysLate)
            ";
            cmd.Parameters.AddWithValue("@TransactionID", tx.TransactionID);
            cmd.Parameters.AddWithValue("@BorrowerID", tx.BorrowerID);
            cmd.Parameters.AddWithValue("@BorrowerName", tx.BorrowerName);
            cmd.Parameters.AddWithValue("@BookISBN", tx.BookISBN);
            cmd.Parameters.AddWithValue("@BookTitle", tx.BookTitle);
            cmd.Parameters.AddWithValue("@IssueDate", tx.IssueDate.ToString("o"));
            cmd.Parameters.AddWithValue("@DueDate", tx.DueDate.ToString("o"));
            cmd.Parameters.AddWithValue("@ReturnDate", tx.ReturnDate.HasValue ? (object?)tx.ReturnDate.Value.ToString("o") : DBNull.Value);
            cmd.Parameters.AddWithValue("@Status", tx.Status);
            cmd.Parameters.AddWithValue("@FineAmount", (double)tx.FineAmount);
            cmd.Parameters.AddWithValue("@DaysLate", tx.DaysLate);
            cmd.ExecuteNonQuery();
        }

        private void AddReservation(Reservation r)
        {
            Reservations.Add(r);
            using var conn = Database.Open();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = @"
                DECLARE @NewId INT;
                INSERT INTO dbo.Reservations (MemberID, MemberName, BookISBN, BookTitle, ReservationDate, QueueNumber, Status)
                VALUES (@MemberID, @MemberName, @BookISBN, @BookTitle, @ReservationDate, @QueueNumber, @Status);
                SET @NewId = SCOPE_IDENTITY();
                SELECT @NewId;
            ";
            cmd.Parameters.AddWithValue("@MemberID", r.MemberID);
            cmd.Parameters.AddWithValue("@MemberName", r.MemberName);
            cmd.Parameters.AddWithValue("@BookISBN", r.BookISBN);
            cmd.Parameters.AddWithValue("@BookTitle", r.BookTitle);
            cmd.Parameters.AddWithValue("@ReservationDate", r.ReservationDate.ToString("o"));
            cmd.Parameters.AddWithValue("@QueueNumber", r.QueueNumber);
            cmd.Parameters.AddWithValue("@Status", r.Status);
            r.ReservationID = (int)cmd.ExecuteScalar();
        }

        private void AddActivity(ActivityLog activity)
        {
            Activities.Add(activity);
            using var conn = Database.Open();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = @"
                INSERT INTO dbo.Activities (TimeText, Description, ActionType, TagText)
                VALUES (@TimeText, @Description, @ActionType, @TagText)
            ";
            cmd.Parameters.AddWithValue("@TimeText", activity.TimeText);
            cmd.Parameters.AddWithValue("@Description", activity.Description);
            cmd.Parameters.AddWithValue("@ActionType", activity.ActionType);
            cmd.Parameters.AddWithValue("@TagText", activity.TagText);
            cmd.ExecuteNonQuery();
        }

        private void SaveSetting(string key, string value)
        {
            using var conn = Database.Open();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = @"
                IF EXISTS (SELECT 1 FROM dbo.Settings WHERE [Key] = @Key)
                    UPDATE dbo.Settings SET Value = @Value WHERE [Key] = @Key
                ELSE
                    INSERT INTO dbo.Settings ([Key], Value) VALUES (@Key, @Value)
            ";
            cmd.Parameters.AddWithValue("@Key", key);
            cmd.Parameters.AddWithValue("@Value", value);
            cmd.ExecuteNonQuery();
        }

        private void SaveAdmin(Librarian admin)
        {
            using var conn = Database.Open();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = @"
                IF EXISTS (SELECT 1 FROM dbo.Admin WHERE LibrarianID = @LibrarianID)
                    UPDATE dbo.Admin SET Username = @Username, Password = @Password, DisplayName = @DisplayName, Role = @Role WHERE LibrarianID = @LibrarianID
                ELSE
                    INSERT INTO dbo.Admin (LibrarianID, Username, Password, DisplayName, Role) VALUES (@LibrarianID, @Username, @Password, @DisplayName, @Role)
            ";
            cmd.Parameters.AddWithValue("@LibrarianID", admin.LibrarianID);
            cmd.Parameters.AddWithValue("@Username", admin.Username);
            cmd.Parameters.AddWithValue("@Password", admin.Password);
            cmd.Parameters.AddWithValue("@DisplayName", admin.DisplayName);
            cmd.Parameters.AddWithValue("@Role", admin.Role);
            cmd.ExecuteNonQuery();
        }

        private int GetNextTransactionNumber()
        {
            using var conn = Database.Open();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = "SELECT ISNULL(MAX(CAST(SUBSTRING(TransactionID, 7, LEN(TransactionID)) AS INT)), 0) + 1 FROM dbo.Transactions WHERE TransactionID LIKE 'TX-%' AND TransactionID LIKE 'TX-' + @year + '-%'";
            cmd.Parameters.AddWithValue("@year", DateTime.Now.Year.ToString());
            return Convert.ToInt32(cmd.ExecuteScalar());
        }

        private int GetNextReservationId()
        {
            using var conn = Database.Open();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = "SELECT ISNULL(MAX(ReservationID), 0) + 1 FROM dbo.Reservations";
            return Convert.ToInt32(cmd.ExecuteScalar());
        }

        public int GetNextBookId()
        {
            using var conn = Database.Open();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = "SELECT ISNULL(MAX(ItemID), 0) + 1 FROM dbo.Books";
            return Convert.ToInt32(cmd.ExecuteScalar());
        }

        public (int totalTitles, int booksOnLoan, int overdueReturns, int registeredBorrowers) GetStatistics()
        {
            int totalTitles = Books.Count;
            int activeLoans = Transactions.Count(t => t.Status == "Borrowed" || t.Status == "Overdue");
            int booksOnLoan = activeLoans;
            int overdue = Transactions.Count(t => t.Status == "Overdue");
            int borrowers = Members.Count;

            return (totalTitles, booksOnLoan, overdue, borrowers);
        }

        public string IssueBook(string memberIdOrStudentId, string isbn)
        {
            var member = Members.FirstOrDefault(m =>
                m.MemberID.Equals(memberIdOrStudentId.Trim(), StringComparison.OrdinalIgnoreCase) ||
                m.StudentID.Equals(memberIdOrStudentId.Trim(), StringComparison.OrdinalIgnoreCase));

            if (member == null)
                return "Error: Member ID or Student ID not found.";

            if (Settings.BlockNewLoansWhileFineUnpaid && member.OutstandingFines > 0)
                return $"Error: Member has unpaid fines of ₱{member.OutstandingFines:N2}. Clear fine before borrowing.";

            if (member.CurrentLoansCount >= Settings.MaxBooksPerBorrower)
                return $"Error: Member reached max borrowing limit of {Settings.MaxBooksPerBorrower} books.";

            var book = Books.FirstOrDefault(b => b.ISBN.Equals(isbn.Trim(), StringComparison.OrdinalIgnoreCase));
            if (book == null)
                return "Error: Book with this ISBN not found.";

            if (!book.BorrowCopy())
                return "Error: No copies of this book are currently available.";

            member.CurrentLoansCount++;

            string txId = $"TX-{DateTime.Now.Year}-{GetNextTransactionNumber():D4}";
            var issueDate = DateTime.Now;
            var dueDate = issueDate.AddDays(Settings.LoanPeriodDays);

            var tx = new BorrowTransaction(txId, member.MemberID, member.FullName, book.ISBN, book.Title, issueDate, dueDate);
            AddTransaction(tx);
            Transactions.Insert(0, tx);

            var activity = new ActivityLog
            {
                TimeText = DateTime.Now.ToString("h:mm tt"),
                Description = $"{member.FullName} borrowed {book.Title}",
                ActionType = "Issued",
                TagText = "Issued"
            };
            AddActivity(activity);
            Activities.Insert(0, activity);

            return $"Success: Book '{book.Title}' issued to {member.FullName}! Due on {dueDate:MMM dd, yyyy}.";
        }

        public (bool success, string message, decimal fineAmount) ReturnBook(string txIdOrIsbn)
        {
            var tx = Transactions.FirstOrDefault(t =>
                t.Status != "Returned" &&
                (t.TransactionID.Equals(txIdOrIsbn.Trim(), StringComparison.OrdinalIgnoreCase) ||
                 t.BookISBN.Equals(txIdOrIsbn.Trim(), StringComparison.OrdinalIgnoreCase)));

            if (tx == null)
                return (false, "Error: No active loan found for this Transaction ID or ISBN.", 0m);

            var book = Books.FirstOrDefault(b => b.ISBN.Equals(tx.BookISBN, StringComparison.OrdinalIgnoreCase));
            if (book != null)
            {
                book.ReturnCopy();
            }

            var member = Members.FirstOrDefault(m => m.MemberID.Equals(tx.BorrowerID, StringComparison.OrdinalIgnoreCase));
            if (member != null && member.CurrentLoansCount > 0)
            {
                member.CurrentLoansCount--;
            }

            var returnDate = DateTime.Now;
            tx.ReturnDate = returnDate;

            decimal calculatedFine = 0m;
            int daysLate = 0;
            if (returnDate.Date > tx.DueDate.Date)
            {
                daysLate = (int)(returnDate.Date - tx.DueDate.Date).TotalDays;
                calculatedFine = daysLate * Settings.FinePerDayLate;
            }

            tx.DaysLate = daysLate;
            tx.FineAmount = calculatedFine;
            tx.Status = "Returned";

            if (member != null && calculatedFine > 0)
            {
                member.OutstandingFines += calculatedFine;
            }

            var activity = new ActivityLog
            {
                TimeText = DateTime.Now.ToString("h:mm tt"),
                Description = $"{tx.BorrowerName} returned {tx.BookTitle}",
                ActionType = "Returned",
                TagText = calculatedFine > 0 ? $"Fine ₱{calculatedFine:N2}" : "Returned"
            };
            AddActivity(activity);
            Activities.Insert(0, activity);

            string msg = calculatedFine > 0
                ? $"Book '{tx.BookTitle}' returned. Overdue by {daysLate} days. Fine calculated: ₱{calculatedFine:N2}."
                : $"Book '{tx.BookTitle}' returned successfully with no fines.";

            return (true, msg, calculatedFine);
        }

        public string ReserveBook(string memberId, string isbn)
        {
            var member = Members.FirstOrDefault(m => m.MemberID.Equals(memberId.Trim(), StringComparison.OrdinalIgnoreCase));
            if (member == null) return "Member not found.";

            var book = Books.FirstOrDefault(b => b.ISBN.Equals(isbn.Trim(), StringComparison.OrdinalIgnoreCase));
            if (book == null) return "Book not found.";

            int existingQueue = Reservations.Count(r => r.BookISBN == book.ISBN && r.Status == "Waiting");

            var reservation = new Reservation
            {
                MemberID = member.MemberID,
                MemberName = member.FullName,
                BookISBN = book.ISBN,
                BookTitle = book.Title,
                ReservationDate = DateTime.Now,
                QueueNumber = existingQueue + 1,
                Status = "Waiting"
            };

            AddReservation(reservation);
            Reservations.Add(reservation);
            var activity = new ActivityLog
            {
                TimeText = DateTime.Now.ToString("h:mm tt"),
                Description = $"{member.FullName} reserved {book.Title}",
                ActionType = "Reserved",
                TagText = "Reserved"
            };
            AddActivity(activity);
            Activities.Insert(0, activity);

            return $"Reservation confirmed! Queue position #{reservation.QueueNumber}.";
        }
    }
}
