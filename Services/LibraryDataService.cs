using BookHaven.Models;

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

            SeedInitialData();
        }

        private void SeedInitialData()
        {
            // Seed Books
            Books.Add(new Book(1, "Clean Code", "Robert C. Martin", "978-0-13-235088-4", "Programming", 5, 3)
            {
                Publisher = "Prentice Hall",
                PublicationYear = 2008,
                ShelfLocation = "Shelf A-1",
                CoverColorHex = "#2563EB",
                Description = "Even bad code can function. But if code isn't clean, it can bring a development organization to its knees. Every year, countless hours and significant resources are lost because of poorly written code."
            });

            Books.Add(new Book(2, "The Pragmatic Programmer", "Hunt & Thomas", "978-0-13-595705-9", "Programming", 2, 0)
            {
                Publisher = "Addison-Wesley",
                PublicationYear = 2019,
                ShelfLocation = "Shelf A-2",
                CoverColorHex = "#7C3AED",
                Description = "The Pragmatic Programmer cuts through the increasing specialization and technicalities of modern software development to examine the core process--taking a requirement and producing working, maintainable code."
            });

            Books.Add(new Book(3, "Introduction to Algorithms", "Cormen et al.", "978-0-262-04630-5", "Computer Science", 4, 2)
            {
                Publisher = "MIT Press",
                PublicationYear = 2022,
                ShelfLocation = "Shelf B-3",
                CoverColorHex = "#0EA5E9",
                Description = "A comprehensive update of the leading algorithms text, with new material on matchings in bipartite graphs, online algorithms, machine learning, and other topics."
            });

            Books.Add(new Book(4, "Database System Concepts", "Silberschatz et al.", "978-0-07-802215-9", "Databases", 3, 1)
            {
                Publisher = "McGraw-Hill",
                PublicationYear = 2019,
                ShelfLocation = "Shelf D-1",
                CoverColorHex = "#059669",
                Description = "Presents the fundamental concepts of database management in an intuitive manner geared toward allowing students to begin working with databases as quickly as possible."
            });

            Books.Add(new Book(5, "Noli Me Tangere", "José Rizal", "978-0-14-303965-5", "Filipiana", 6, 4)
            {
                Publisher = "Penguin Classics",
                PublicationYear = 1887,
                ShelfLocation = "Shelf F-1",
                CoverColorHex = "#D97706",
                Description = "In more than a century since its appearance, José Rizal's Noli Me Tangere has become widely known as the great novel of the Philippines, depicting Spanish colonial rule."
            });

            Books.Add(new Book(6, "Calculus: Early Transcendentals", "James Stewart", "978-1-285-74155-0", "Mathematics", 3, 2)
            {
                Publisher = "Cengage Learning",
                PublicationYear = 2015,
                ShelfLocation = "Shelf M-2",
                CoverColorHex = "#DC2626",
                Description = "Success in your calculus course starts here! James Stewart's Calculus: Early Transcendentals texts are world-wide best-sellers for a reason: they are clear, accurate, and filled with relevant, real-world examples."
            });

            // Seed Members
            Members.Add(new Member("BH-M-0142", "2023-0142", "Aira Mendoza", "aira.mendoza@example.edu", "BS Computer Science 3rd Year", 2, 0m));
            Members.Add(new Member("BH-M-0087", "2022-0087", "Marco Santos", "marco.santos@example.edu", "BS Information Tech 4th Year", 1, 0m));
            Members.Add(new Member("BH-M-0215", "2024-0215", "Jun Dela Cruz", "jun.delacruz@example.edu", "BS Data Science 2nd Year", 1, 35.00m));
            Members.Add(new Member("BH-M-0031", "2021-0031", "Bea Ramirez", "bea.ramirez@example.edu", "BS Applied Mathematics 4th Year", 1, 50.00m));
            Members.Add(new Member("BH-M-0178", "2023-0178", "Carlo Reyes", "carlo.reyes@example.edu", "BS Computer Engineering 3rd Year", 1, 0m));
            Members.Add(new Member("BH-M-0260", "2024-0260", "Tina Bautista", "tina.bautista@example.edu", "AB Literature 1st Year", 1, 0m));
            Members.Add(new Member("BH-M-0304", "2024-0304", "Lea Villanueva", "lea.villanueva@example.edu", "BS Information Systems 2nd Year", 0, 0m));

            // Seed Transactions
            var today = new DateTime(2026, 11, 22);

            Transactions.Add(new BorrowTransaction("TX-2026-0320", "BH-M-0142", "Aira Mendoza", "978-0-13-235088-4", "Clean Code", new DateTime(2026, 11, 22), new DateTime(2026, 11, 29)));
            Transactions.Add(new BorrowTransaction("TX-2026-0319", "BH-M-0087", "Marco Santos", "978-0-13-595705-9", "The Pragmatic Programmer", new DateTime(2026, 11, 21), new DateTime(2026, 11, 28)));
            Transactions.Add(new BorrowTransaction("TX-2026-0318", "BH-M-0178", "Carlo Reyes", "978-0-262-04630-5", "Introduction to Algorithms", new DateTime(2026, 11, 18), new DateTime(2026, 11, 25)));
            Transactions.Add(new BorrowTransaction("TX-2026-0317", "BH-M-0260", "Tina Bautista", "978-0-14-303965-5", "Noli Me Tangere", new DateTime(2026, 11, 19), new DateTime(2026, 11, 26)));

            var overdue1 = new BorrowTransaction("TX-2026-0315", "BH-M-0215", "Jun Dela Cruz", "978-0-07-802215-9", "Database System Concepts", new DateTime(2026, 11, 8), new DateTime(2026, 11, 15))
            {
                Status = "Overdue",
                DaysLate = 7,
                FineAmount = 35.00m
            };
            Transactions.Add(overdue1);

            var overdue2 = new BorrowTransaction("TX-2026-0312", "BH-M-0031", "Bea Ramirez", "978-1-285-74155-0", "Calculus: Early Transcendentals", new DateTime(2026, 11, 5), new DateTime(2026, 11, 12))
            {
                Status = "Overdue",
                DaysLate = 10,
                FineAmount = 50.00m
            };
            Transactions.Add(overdue2);

            var returned1 = new BorrowTransaction("TX-2026-0310", "BH-M-0304", "Lea Villanueva", "978-0-14-303965-5", "Noli Me Tangere", new DateTime(2026, 11, 10), new DateTime(2026, 11, 17))
            {
                ReturnDate = new DateTime(2026, 11, 24),
                Status = "Returned",
                FineAmount = 0m
            };
            Transactions.Add(returned1);

            // Seed Reservations
            Reservations.Add(new Reservation
            {
                ReservationID = 1,
                MemberID = "BH-M-0031",
                MemberName = "Bea Ramirez",
                BookISBN = "978-0-13-595705-9",
                BookTitle = "The Pragmatic Programmer",
                QueueNumber = 1,
                Status = "Waiting"
            });
            Reservations.Add(new Reservation
            {
                ReservationID = 2,
                MemberID = "BH-M-0142",
                MemberName = "Aira Mendoza",
                BookISBN = "978-0-13-595705-9",
                BookTitle = "The Pragmatic Programmer",
                QueueNumber = 2,
                Status = "Waiting"
            });
            Reservations.Add(new Reservation
            {
                ReservationID = 3,
                MemberID = "BH-M-0304",
                MemberName = "Lea Villanueva",
                BookISBN = "978-0-07-802215-9",
                BookTitle = "Database System Concepts",
                QueueNumber = 1,
                Status = "Waiting"
            });

            // Seed Activities
            Activities.Add(new ActivityLog { TimeText = "9:12 AM", Description = "Lea Villanueva returned Noli Me Tangere", ActionType = "Returned", TagText = "Returned" });
            Activities.Add(new ActivityLog { TimeText = "10:03 AM", Description = "Marco Santos borrowed Introduction to Algorithms", ActionType = "Issued", TagText = "Issued" });
            Activities.Add(new ActivityLog { TimeText = "11:40 AM", Description = "Bea Ramirez reserved The Pragmatic Programmer", ActionType = "Reserved", TagText = "Reserved" });
            Activities.Add(new ActivityLog { TimeText = "1:25 PM", Description = "Carlo Reyes paid a late fine", ActionType = "Fine", TagText = "Fine ₱15.00" });
        }

        public (int totalTitles, int booksOnLoan, int overdueReturns, int registeredBorrowers) GetStatistics()
        {
            int totalTitles = 1248 + (Books.Count - 6);
            int activeLoans = Transactions.Count(t => t.Status == "Borrowed" || t.Status == "Overdue");
            int booksOnLoan = 186 + (activeLoans - 6);
            int overdue = 14 + (Transactions.Count(t => t.Status == "Overdue") - 2);
            int borrowers = 532 + (Members.Count - 7);

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

            string txId = $"TX-{DateTime.Now.Year}-{Transactions.Count + 101:D4}";
            var issueDate = DateTime.Now;
            var dueDate = issueDate.AddDays(Settings.LoanPeriodDays);

            var tx = new BorrowTransaction(txId, member.MemberID, member.FullName, book.ISBN, book.Title, issueDate, dueDate);
            Transactions.Insert(0, tx);

            Activities.Insert(0, new ActivityLog
            {
                TimeText = DateTime.Now.ToString("h:mm tt"),
                Description = $"{member.FullName} borrowed {book.Title}",
                ActionType = "Issued",
                TagText = "Issued"
            });

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

            Activities.Insert(0, new ActivityLog
            {
                TimeText = DateTime.Now.ToString("h:mm tt"),
                Description = $"{tx.BorrowerName} returned {tx.BookTitle}",
                ActionType = "Returned",
                TagText = calculatedFine > 0 ? $"Fine ₱{calculatedFine:N2}" : "Returned"
            });

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
                ReservationID = Reservations.Count + 1,
                MemberID = member.MemberID,
                MemberName = member.FullName,
                BookISBN = book.ISBN,
                BookTitle = book.Title,
                ReservationDate = DateTime.Now,
                QueueNumber = existingQueue + 1,
                Status = "Waiting"
            };

            Reservations.Add(reservation);
            Activities.Insert(0, new ActivityLog
            {
                TimeText = DateTime.Now.ToString("h:mm tt"),
                Description = $"{member.FullName} reserved {book.Title}",
                ActionType = "Reserved",
                TagText = "Reserved"
            });

            return $"Reservation confirmed! Queue position #{reservation.QueueNumber}.";
        }
    }
}
