-- ============================================================
-- BookHaven Schema
-- Run in Visual Studio: right-click BookHaven (localdb)\MSSQLLocalDB
--   > New Query > paste this > Execute (F5)
-- Or let the app run it automatically on first launch.
-- ============================================================

IF OBJECT_ID(N'dbo.Books', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Books (
        ItemID            INT             NOT NULL PRIMARY KEY,
        Title             NVARCHAR(255)   NOT NULL,
        Author            NVARCHAR(255)   NOT NULL,
        ISBN              NVARCHAR(50)    NOT NULL UNIQUE,
        Category          NVARCHAR(100)   NOT NULL,
        TotalCopies       INT             NOT NULL,
        AvailableCopies   INT             NOT NULL,
        Publisher         NVARCHAR(255)   NOT NULL,
        PublicationYear   INT             NOT NULL,
        ShelfLocation     NVARCHAR(100)   NOT NULL,
        CoverColorHex     NVARCHAR(20)    NOT NULL,
        Description       NVARCHAR(MAX)   NOT NULL,
        Archived          BIT             NOT NULL DEFAULT 0
    );
END
GO

IF COL_LENGTH(N'dbo.Books', N'Archived') IS NULL
BEGIN
    ALTER TABLE dbo.Books
        ADD Archived BIT NOT NULL
            CONSTRAINT DF_Books_Archived DEFAULT 0;
END
GO

IF OBJECT_ID(N'dbo.Members', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Members (
        MemberID              NVARCHAR(50)  NOT NULL PRIMARY KEY,
        StudentID             NVARCHAR(50)  NOT NULL,
        FullName              NVARCHAR(255) NOT NULL,
        Email                 NVARCHAR(255) NOT NULL,
        CourseAndYear         NVARCHAR(255) NOT NULL,
        RegisteredDate        NVARCHAR(50)  NOT NULL,
        MaxLoans              INT           NOT NULL,
        CurrentLoansCount     INT           NOT NULL,
        OutstandingFines      REAL          NOT NULL
    );
END
GO

IF OBJECT_ID(N'dbo.Transactions', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Transactions (
        TransactionID  NVARCHAR(50)  NOT NULL PRIMARY KEY,
        BorrowerID     NVARCHAR(50)  NOT NULL,
        BorrowerName   NVARCHAR(255) NOT NULL,
        BookISBN       NVARCHAR(50)  NOT NULL,
        BookTitle      NVARCHAR(255) NOT NULL,
        IssueDate      NVARCHAR(50)  NOT NULL,
        DueDate        NVARCHAR(50)  NOT NULL,
        ReturnDate     NVARCHAR(50)  NULL,
        Status         NVARCHAR(50)  NOT NULL,
        FineAmount     REAL          NOT NULL,
        DaysLate       INT           NOT NULL
    );
END
GO

IF OBJECT_ID(N'dbo.Reservations', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Reservations (
        ReservationID    INT           NOT NULL IDENTITY(1,1) PRIMARY KEY,
        MemberID         NVARCHAR(50) NOT NULL,
        MemberName       NVARCHAR(255) NOT NULL,
        BookISBN         NVARCHAR(50) NOT NULL,
        BookTitle        NVARCHAR(255) NOT NULL,
        ReservationDate  NVARCHAR(50) NOT NULL,
        QueueNumber      INT          NOT NULL,
        Status           NVARCHAR(50) NOT NULL
    );
END
GO

IF OBJECT_ID(N'dbo.Activities', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Activities (
        Id          INT           NOT NULL IDENTITY(1,1) PRIMARY KEY,
        TimeText    NVARCHAR(50)  NOT NULL,
        Description NVARCHAR(500) NOT NULL,
        ActionType  NVARCHAR(50)  NOT NULL,
        TagText     NVARCHAR(50)  NOT NULL
    );
END
GO

IF OBJECT_ID(N'dbo.Settings', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Settings (
        [Key]   NVARCHAR(100) NOT NULL PRIMARY KEY,
        Value   NVARCHAR(500) NOT NULL
    );
END
GO

IF OBJECT_ID(N'dbo.Admin', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Admin (
        LibrarianID  INT           NOT NULL PRIMARY KEY,
        Username     NVARCHAR(100) NOT NULL,
        Password     NVARCHAR(100) NOT NULL,
        DisplayName  NVARCHAR(255) NOT NULL,
        Role         NVARCHAR(100) NOT NULL
    );
END
GO
