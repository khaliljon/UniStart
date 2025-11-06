using Microsoft.AspNetCore.Identity;

namespace UniStart.Models
{
    public class ApplicationUser : IdentityUser
    {
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? LastLoginAt { get; set; }
        
        // Статистика
        public int TotalCardsStudied { get; set; } = 0;
        public int TotalQuizzesTaken { get; set; } = 0;
    }
}
