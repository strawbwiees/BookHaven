# 📖 BookHaven — Library Management System

> **A modern, desktop-based Library Management System built with C#, Windows Forms, and Object-Oriented Programming (OOP).**

---

## 🌟 Overview

**BookHaven** is designed to help librarians efficiently manage book inventories, registered borrowers, loans, book returns, and late fee calculations. The user interface features a clean, responsive layout inspired by modern design standards, complete with custom-rendered vector graphics, book covers, and pill navigation.

---

## 🚀 Key Features

- **📚 Catalog & Book Shelf**:
  - Grid and table inventory view with real book cover thumbnails.
  - Interactive **Book Details Modal** showing complete metadata, copies meter, and loan status.
  - Dedicated **Add Book** and **Edit Book** tabs with field validation and category assignment.
  - Real-time search by title, author, or ISBN, along with quick category filter pills.

- **🔄 Issue & Return Desk**:
  - Borrower & Book real-time lookups with live status previews.
  - Automatic due date calculation based on library rules.
  - One-click book return with automatic overdue fine calculator (₱5.00/day).
  - Live "Today's Activity" event timeline.

- **📊 Dashboard & Metrics**:
  - Metric counters: Catalog Titles, Books on Loan, Overdue Returns, Registered Borrowers.
  - "Most Overdue" loan leaderboard with borrower avatars and calculated penalties.
  - 7-day borrowing trend bar chart.
  - Quick action shortcuts for common librarian workflows.

- **📋 Borrowed Books & Reservation Queue**:
  - Filter transactions by *On Loan*, *Overdue*, *Reservations*, and *Returned*.
  - Export transaction logs to CSV.
  - Reservation queue management for books currently on loan.

- **👥 Borrower Management**:
  - Card-based member directory with initials avatars, loan counts, and active/fine badges.
  - Quick registration for new student members.

- **⚙️ Library Rules & Settings**:
  - Customizable loan duration, overdue fine rates, and borrowing limits.
  - Category manager for organizing books.
  - Admin credentials manager.

- **🔐 Authentication**:
  - Secure login portal pre-seeded with admin credentials.

---

## 🏛️ OOP Architecture

BookHaven illustrates the four core pillars of Object-Oriented Programming:

| Pillar | Implementation |
|---|---|
| **Abstraction** | `LibraryItem` base class exposing abstract item metadata, copy tracking, and virtual methods (`DisplayDetails`, `BorrowCopy`, `ReturnCopy`). |
| **Inheritance** | Specialized items (`Book`, `Magazine`) inherit from `LibraryItem` and extend it with specific properties (ISBN, Shelf Location, Publisher, Year, Periodical Issue Number). |
| **Encapsulation** | Private backing fields (`availableCopies`, `totalCopies`) protected by validated getters and methods to ensure data integrity. |
| **Polymorphism** | Override of item display methods and polymorphic collection handling across library items and transactions. |

---

## 🛠️ Tech Stack & Requirements

- **Language**: C# 13+
- **Framework**: .NET 10.0 (Windows Forms)
- **UI & Graphics**: System.Drawing (GDI+ custom-rendered rounded panels, pill buttons, badges, and avatars)
- **Data Persistence**: In-memory singleton service (`LibraryDataService`) pre-seeded with mockup data.

---

## 🏃 Getting Started

### Prerequisites
- [.NET 10.0 SDK](https://dotnet.microsoft.com/download) or higher
- Windows 10/11
- Visual Studio 2022 / VS Code / JetBrains Rider

### Build & Run via Command Line

```powershell
# Clone the repository
git clone https://github.com/<your-username>/BookHaven.git
cd BookHaven

# Restore and run
dotnet build
dotnet run
```

### Default Credentials
- **Username**: `admin`
- **Password**: `admin`

---

## 📁 Project Structure

```text
BookHaven/
├── Forms/
│   ├── BookDetailsModal.cs   # Book details pop-up modal
│   ├── LoginForm.cs          # Login window
│   └── MainForm.cs           # Main application shell & sidebar
├── Models/
│   ├── Book.cs               # Book model (inherits LibraryItem)
│   ├── BorrowTransaction.cs  # Loan & return transaction model
│   ├── Librarian.cs          # Admin account model
│   ├── LibraryItem.cs        # Abstract base class
│   ├── Magazine.cs           # Periodical item model
│   ├── Member.cs             # Borrower model
│   └── Reservation.cs        # Reservation, activity, and settings models
├── Resources/
│   ├── logo_shield.png       # Shield logo
│   └── BookCovers/           # Real cover art for catalog titles
├── Services/
│   ├── CoverImageGenerator.cs
│   └── LibraryDataService.cs # Core in-memory data service
├── Tests/
│   └── SmokeTester.cs        # Automated visual & smoke test suite
├── UI/
│   ├── CustomControls.cs     # CardPanel, PillButton, NavPillButton, RoundedTextBox
│   └── UITheme.cs            # Color palette, font styles, and avatar drawing
├── Views/
│   ├── BooksView.cs          # The Book Shelf (Catalog, Add, Edit)
│   ├── BorrowedBooksView.cs  # Loan records & reservations
│   ├── BorrowersView.cs      # Borrower directory & registration
│   ├── DashboardView.cs      # Home dashboard & statistics
│   ├── IssueReturnView.cs    # Issue & return transactions
│   └── SettingsView.cs       # Library rules & configuration
├── BookHaven.csproj
├── BookHaven.slnx
├── Program.cs
└── README.md
```
