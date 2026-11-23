namespace BookHaven.Models
{
    /// <summary>
    /// Represents a physical or digital Book in the library.
    /// Demonstrates INHERITANCE (inherits from LibraryItem) and POLYMORPHISM.
    /// </summary>
    public class Book : LibraryItem
    {
        public string ISBN { get; set; } = string.Empty;
        public string Publisher { get; set; } = string.Empty;
        public int PublicationYear { get; set; }
        public string Description { get; set; } = string.Empty;
        public string ShelfLocation { get; set; } = "Main Shelf";
        public string CoverColorHex { get; set; } = "#3B82F6"; // Default blue
        public string? CoverImagePath { get; set; }
        public bool Archived { get; set; } = false;

        public Book(int itemId, string title, string author, string isbn, string category, int totalCopies, int availableCopies = -1)
            : base(itemId, title, author, category, totalCopies)
        {
            ISBN = isbn;
            if (availableCopies >= 0 && availableCopies <= totalCopies)
            {
                this.availableCopies = availableCopies;
            }
            else
            {
                this.availableCopies = totalCopies;
            }
        }

        public string GetStockStatus()
        {
            if (AvailableCopies == 0)
                return "All borrowed";
            if (AvailableCopies <= 1 && TotalCopies > 2)
                return "Low stock";
            return "Available";
        }

        public override string DisplayDetails()
        {
            return $"Book: {Title} by {Author}\nISBN: {ISBN}\nCategory: {Category}\nCopies: {AvailableCopies}/{TotalCopies} Available\nLocation: {ShelfLocation}";
        }
    }
}
