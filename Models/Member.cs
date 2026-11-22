namespace BookHaven.Models
{
    /// <summary>
    /// Represents a registered library borrower/student.
    /// Demonstrates ENCAPSULATION and business rule validation.
    /// </summary>
    public class Member
    {
        public string MemberID { get; set; } = string.Empty; // e.g. "BH-M-0142"
        public string StudentID { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string CourseAndYear { get; set; } = string.Empty;
        public DateTime RegisteredDate { get; set; } = DateTime.Now;
        public int MaxLoans { get; set; } = 3;
        public int CurrentLoansCount { get; set; } = 0;
        public decimal OutstandingFines { get; set; } = 0.00m;

        public string Status
        {
            get
            {
                if (OutstandingFines > 0) return "Fine due";
                if (CurrentLoansCount >= MaxLoans) return "Max loans";
                return "Active";
            }
        }

        public string Initials
        {
            get
            {
                if (string.IsNullOrWhiteSpace(FullName)) return "M";
                var parts = FullName.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length == 1) return parts[0][0].ToString().ToUpper();
                return $"{parts[0][0]}{parts[^1][0]}".ToUpper();
            }
        }

        public Member() { }

        public Member(string memberId, string studentId, string fullName, string email, string courseAndYear, int currentLoans = 0, decimal fines = 0m)
        {
            MemberID = memberId;
            StudentID = studentId;
            FullName = fullName;
            Email = email;
            CourseAndYear = courseAndYear;
            CurrentLoansCount = currentLoans;
            OutstandingFines = fines;
        }

        public bool CanBorrow(out string reason)
        {
            if (OutstandingFines > 0)
            {
                reason = $"Member has unpaid fines of ₱{OutstandingFines:N2}. Clear fines before borrowing.";
                return false;
            }

            if (CurrentLoansCount >= MaxLoans)
            {
                reason = $"Member has reached maximum borrowing limit ({MaxLoans} books).";
                return false;
            }

            reason = "Eligible to borrow.";
            return true;
        }
    }
}
