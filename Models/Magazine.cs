namespace BookHaven.Models
{
    /// <summary>
    /// Demonstrates INHERITANCE and POLYMORPHISM with specialized periodical attributes.
    /// </summary>
    public class Magazine : LibraryItem
    {
        public int IssueNumber { get; set; }
        public string MonthYear { get; set; } = string.Empty;

        public Magazine(int itemId, string title, string publisher, string category, int issueNumber, string monthYear, int totalCopies)
            : base(itemId, title, publisher, category, totalCopies)
        {
            IssueNumber = issueNumber;
            MonthYear = monthYear;
        }

        public override string DisplayDetails()
        {
            return $"Periodical: {Title} (Issue #{IssueNumber}, {MonthYear})\nCategory: {Category}\nCopies: {AvailableCopies}/{TotalCopies}";
        }
    }
}
