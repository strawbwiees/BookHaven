namespace BookHaven.Models
{
    /// <summary>
    /// Abstract parent class representing any item that can be cataloged in the library.
    /// Demonstrates ABSTRACTION and POLYMORPHISM.
    /// </summary>
    public abstract class LibraryItem
    {
        public int ItemID { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Author { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;

        // Encapsulation of copy counts
        protected int totalCopies;
        protected int availableCopies;

        public int TotalCopies
        {
            get => totalCopies;
            set
            {
                if (value < 0) throw new ArgumentException("Total copies cannot be negative.");
                totalCopies = value;
                if (availableCopies > totalCopies)
                    availableCopies = totalCopies;
            }
        }

        public int AvailableCopies
        {
            get => availableCopies;
            protected set => availableCopies = Math.Max(0, value);
        }

        /// <summary>
        /// Changes the total number of copies owned while keeping the number of
        /// copies currently on loan intact. Returns false when the new total is
        /// smaller than the number of copies that are already out.
        /// </summary>
        public bool AdjustTotalCopies(int newTotalCopies)
        {
            if (newTotalCopies < 0) return false;

            int copiesOnLoan = totalCopies - availableCopies;
            if (newTotalCopies < copiesOnLoan) return false;

            totalCopies = newTotalCopies;
            availableCopies = newTotalCopies - copiesOnLoan;
            return true;
        }

        public bool IsAvailable => availableCopies > 0;

        protected LibraryItem(int itemId, string title, string author, string category, int totalCopies)
        {
            ItemID = itemId;
            Title = title;
            Author = author;
            Category = category;
            TotalCopies = totalCopies;
            availableCopies = totalCopies;
        }

        /// <summary>
        /// Borrows one copy of the item. Returns true if successful.
        /// Demonstrates ENCAPSULATION.
        /// </summary>
        public virtual bool BorrowCopy()
        {
            if (availableCopies > 0)
            {
                availableCopies--;
                return true;
            }
            return false;
        }

        /// <summary>
        /// Returns one copy of the item.
        /// </summary>
        public virtual void ReturnCopy()
        {
            if (availableCopies < totalCopies)
            {
                availableCopies++;
            }
        }

        /// <summary>
        /// Abstract or virtual method for polymorphic string representations or details.
        /// Demonstrates POLYMORPHISM.
        /// </summary>
        public abstract string DisplayDetails();
    }
}
