namespace BookHaven.Models
{
    public class Librarian
    {
        public int LibrarianID { get; set; } = 1;
        public string Username { get; set; } = "admin";
        public string Password { get; set; } = "admin";
        public string DisplayName { get; set; } = "Admin Librarian";
        public string Role { get; set; } = "Head Librarian";
    }
}
