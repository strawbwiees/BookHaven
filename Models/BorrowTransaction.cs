namespace BookHaven.Models
{
    public class BorrowTransaction
    {
        public string TransactionID { get; set; } = string.Empty;
        public string BorrowerID { get; set; } = string.Empty;
        public string BorrowerName { get; set; } = string.Empty;
        public string BookISBN { get; set; } = string.Empty;
        public string BookTitle { get; set; } = string.Empty;
        public DateTime IssueDate { get; set; }
        public DateTime DueDate { get; set; }
        public DateTime? ReturnDate { get; set; }
        public string Status { get; set; } = "Borrowed"; // Borrowed, Returned, Overdue
        public decimal FineAmount { get; set; } = 0.00m;
        public int DaysLate { get; set; } = 0;

        public BorrowTransaction() { }

        public BorrowTransaction(string transactionId, string borrowerId, string borrowerName, string isbn, string bookTitle, DateTime issueDate, DateTime dueDate)
        {
            TransactionID = transactionId;
            BorrowerID = borrowerId;
            BorrowerName = borrowerName;
            BookISBN = isbn;
            BookTitle = bookTitle;
            IssueDate = issueDate;
            DueDate = dueDate;
            Status = "Borrowed";
        }

        public void UpdateOverdueStatus(DateTime currentDate, decimal fineRatePerDay)
        {
            if (Status == "Returned") return;

            DateTime checkDate = ReturnDate ?? currentDate;
            if (checkDate.Date > DueDate.Date)
            {
                DaysLate = (int)(checkDate.Date - DueDate.Date).TotalDays;
                FineAmount = DaysLate * fineRatePerDay;
                Status = "Overdue";
            }
            else
            {
                DaysLate = 0;
                FineAmount = 0m;
                Status = "Borrowed";
            }
        }
    }
}
