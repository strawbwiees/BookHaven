namespace BookHaven.Models
{
    public class Reservation
    {
        public int ReservationID { get; set; }
        public string MemberID { get; set; } = string.Empty;
        public string MemberName { get; set; } = string.Empty;
        public string BookISBN { get; set; } = string.Empty;
        public string BookTitle { get; set; } = string.Empty;
        public DateTime ReservationDate { get; set; } = DateTime.Now;
        public int QueueNumber { get; set; } = 1;
        public string Status { get; set; } = "Waiting"; // Waiting, Ready, Fulfilled, Cancelled
    }

    public class ActivityLog
    {
        public string TimeText { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string ActionType { get; set; } = "Issued"; // Issued, Returned, Reserved, Fine
        public string TagText { get; set; } = string.Empty;
    }

    public class LibrarySettings
    {
        public int LoanPeriodDays { get; set; } = 7;
        public decimal FinePerDayLate { get; set; } = 5.00m;
        public int MaxBooksPerBorrower { get; set; } = 3;
        public bool CalculateFinesAutomatically { get; set; } = true;
        public bool BlockNewLoansWhileFineUnpaid { get; set; } = true;
        public List<string> Categories { get; set; } = new()
        {
            "Programming",
            "Computer Science",
            "Databases",
            "Filipiana",
            "Mathematics"
        };
    }
}
