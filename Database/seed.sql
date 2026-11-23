USE BookHaven;
GO

-- Clear existing data first
DELETE FROM dbo.Activities;
DELETE FROM dbo.Reservations;
DELETE FROM dbo.Transactions;
DELETE FROM dbo.Members;
DELETE FROM dbo.Books;
DELETE FROM dbo.Settings;
DELETE FROM dbo.Admins;
GO

-- --- Books ---
INSERT INTO dbo.Books (ItemID, Title, Author, ISBN, Category, TotalCopies, AvailableCopies, Publisher, PublicationYear, ShelfLocation, CoverColorHex, Description, Archived)
VALUES
    (1, N'Clean Code', N'Robert C. Martin', N'978-0-13-235088-4', N'Programming', 5, 3, N'Prentice Hall', 2008, N'Shelf A-1', N'#2563EB', N'Even bad code can function. But if code isn''t clean, it can bring a development organization to its knees. Every year, countless hours and significant resources are lost because of poorly written code.', 0),
    (2, N'The Pragmatic Programmer', N'Hunt & Thomas', N'978-0-13-595705-9', N'Programming', 2, 0, N'Addison-Wesley', 2019, N'Shelf A-2', N'#7C3AED', N'The Pragmatic Programmer cuts through the increasing specialization and technicalities of modern software development to examine the core process--taking a requirement and producing working, maintainable code.', 0),
    (3, N'Introduction to Algorithms', N'Cormen et al.', N'978-0-262-04630-5', N'Computer Science', 4, 2, N'MIT Press', 2022, N'Shelf B-3', N'#0EA5E9', N'A comprehensive update of the leading algorithms text, with new material on matchings in bipartite graphs, online algorithms, machine learning, and other topics.', 0),
    (4, N'Database System Concepts', N'Silberschatz et al.', N'978-0-07-802215-9', N'Databases', 3, 1, N'McGraw-Hill', 2019, N'Shelf D-1', N'#059669', N'Presents the fundamental concepts of database management in an intuitive manner geared toward allowing students to begin working with databases as quickly as possible.', 0),
    (5, N'Noli Me Tangere', N'José Rizal', N'978-0-14-303965-5', N'Filipiana', 6, 4, N'Penguin Classics', 1887, N'Shelf F-1', N'#D97706', N'In more than a century since its appearance, José Rizal''s Noli Me Tangere has become widely known as the great novel of the Philippines, depicting Spanish colonial rule.', 0),
    (6, N'Calculus: Early Transcendentals', N'James Stewart', N'978-1-285-74155-0', N'Mathematics', 3, 2, N'Cengage Learning', 2015, N'Shelf M-2', N'#DC2626', N'Success in your calculus course starts here! James Stewart''s Calculus: Early Transcendentals texts are world-wide best-sellers for a reason: they are clear, accurate, and filled with relevant, real-world examples.', 0);
GO

-- --- Members ---
INSERT INTO dbo.Members (MemberID, StudentID, FullName, Email, CourseAndYear, RegisteredDate, MaxLoans, CurrentLoansCount, OutstandingFines)
VALUES
    (N'BH-M-0142', N'2023-0142', N'Noella Mariz Beltran', N'noellamarizbeltran@umak.edu.ph', N'BS Computer Science 3rd Year', N'2023-09-01', 2, 0, 0.0),
    (N'BH-M-0087', N'2022-0087', N'Carl Mendiola', N'carlmendiola@umak.edu.ph', N'BS Information Tech 4th Year', N'2022-09-01', 1, 0, 0.0),
    (N'BH-M-0215', N'2024-0215', N'Dan Gevera', N'dangevera@umak.edu.ph', N'BS Data Science 2nd Year', N'2024-09-01', 1, 0, 35.0),
    (N'BH-M-0031', N'2021-0031', N'Hans Pasion', N'hanspasion@umak.edu.ph', N'BS Applied Mathematics 4th Year', N'2021-09-01', 1, 0, 50.0),
    (N'BH-M-0178', N'2023-0178', N'Zither Paller', N'zitherpaller@umak.edu.ph', N'BS Computer Engineering 3rd Year', N'2023-09-01', 1, 0, 0.0),
    (N'BH-M-0260', N'2024-0260', N'Sherwin Cari', N'sherwincari@umak.edu.ph', N'AB Literature 1st Year', N'2024-09-01', 1, 0, 0.0);
GO

-- --- Transactions ---
INSERT INTO dbo.Transactions (TransactionID, BorrowerID, BorrowerName, BookISBN, BookTitle, IssueDate, DueDate, ReturnDate, Status, FineAmount, DaysLate)
VALUES
    (N'TX-2026-0320', N'BH-M-0142', N'Noella Mariz Beltran', N'978-0-13-235088-4', N'Clean Code', N'2026-11-22T00:00:00', N'2026-11-29T00:00:00', NULL, N'Borrowed', 0.0, 0),
    (N'TX-2026-0319', N'BH-M-0087', N'Carl Mendiola', N'978-0-13-595705-9', N'The Pragmatic Programmer', N'2026-11-21T00:00:00', N'2026-11-28T00:00:00', NULL, N'Borrowed', 0.0, 0),
    (N'TX-2026-0318', N'BH-M-0178', N'Zither Paller', N'978-0-262-04630-5', N'Introduction to Algorithms', N'2026-11-18T00:00:00', N'2026-11-25T00:00:00', NULL, N'Borrowed', 0.0, 0),
    (N'TX-2026-0317', N'BH-M-0260', N'Sherwin Cari', N'978-0-14-303965-5', N'Noli Me Tangere', N'2026-11-19T00:00:00', N'2026-11-26T00:00:00', NULL, N'Borrowed', 0.0, 0),
    (N'TX-2026-0315', N'BH-M-0215', N'Dan Gevera', N'978-0-07-802215-9', N'Database System Concepts', N'2026-11-08T00:00:00', N'2026-11-15T00:00:00', NULL, N'Overdue', 35.0, 7),
    (N'TX-2026-0312', N'BH-M-0031', N'Hans Pasion', N'978-1-285-74155-0', N'Calculus: Early Transcendentals', N'2026-11-05T00:00:00', N'2026-11-12T00:00:00', NULL, N'Overdue', 50.0, 10);
GO

-- --- Reservations ---
SET IDENTITY_INSERT dbo.Reservations ON;

INSERT INTO dbo.Reservations (ReservationID, MemberID, MemberName, BookISBN, BookTitle, ReservationDate, QueueNumber, Status)
VALUES
    (1, N'BH-M-0031', N'Hans Pasion', N'978-0-13-595705-9', N'The Pragmatic Programmer', N'2026-11-22T11:40:00', 1, N'Waiting'),
    (2, N'BH-M-0142', N'Noella Mariz Beltran', N'978-0-13-595705-9', N'The Pragmatic Programmer', N'2026-11-22T11:41:00', 2, N'Waiting');

SET IDENTITY_INSERT dbo.Reservations OFF;
GO

-- --- Activities ---
INSERT INTO dbo.Activities (TimeText, Description, ActionType, TagText)
VALUES
    (N'10:03 AM', N'Carl Mendiola borrowed Introduction to Algorithms', N'Issued', N'Issued'),
    (N'11:40 AM', N'Hans Pasion reserved The Pragmatic Programmer', N'Reserved', N'Reserved'),
    (N'1:25 PM', N'Zither Paller paid a late fine', N'Fine', N'Fine ₱15.00');
GO

-- --- Settings ---
INSERT INTO dbo.Settings ([Key], Value)
VALUES
    (N'LoanPeriodDays', N'7'),
    (N'FinePerDayLate', N'5.00'),
    (N'MaxBooksPerBorrower', N'3'),
    (N'CalculateFinesAutomatically', N'True'),
    (N'BlockNewLoansWhileFineUnpaid', N'True'),
    (N'Category:Programming', N'1'),
    (N'Category:Computer Science', N'1'),
    (N'Category:Databases', N'1'),
    (N'Category:Filipiana', N'1'),
    (N'Category:Mathematics', N'1');
GO

-- --- Admins ---
INSERT INTO dbo.Admins (LibrarianID, Username, Password, DisplayName, Role)
VALUES (1, N'admin', N'admin', N'Admin Librarian', N'Head Librarian');
GO  