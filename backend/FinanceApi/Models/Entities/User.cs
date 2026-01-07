using System.ComponentModel.DataAnnotations;

namespace FinanceApi.Models.Entities
{
    public class User
    {
        public Guid Id { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        public string PasswordHash { get; set; } = string.Empty;

        public string? DisplayName { get; set; }

        public string DefaultCurrency { get; set; } = "CAD";

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        public ICollection<BudgetSpaceMember> Memberships { get; set; } = new List<BudgetSpaceMember>();
        public ICollection<BudgetSpace> OwnedSpaces { get; set; } = new List<BudgetSpace>();
    }
}
