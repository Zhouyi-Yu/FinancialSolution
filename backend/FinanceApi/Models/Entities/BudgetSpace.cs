using System.ComponentModel.DataAnnotations;

namespace FinanceApi.Models.Entities
{
    public class BudgetSpace
    {
        public Guid Id { get; set; }

        public Guid OwnerUserId { get; set; }
        public User? Owner { get; set; }

        [Required]
        public string Name { get; set; } = string.Empty;

        [Required]
        public string BaseCurrency { get; set; } = "CAD";

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        public ICollection<BudgetSpaceMember> Members { get; set; } = new List<BudgetSpaceMember>();
        public ICollection<Transaction> Transactions { get; set; } = new List<Transaction>();
        public ICollection<Category> Categories { get; set; } = new List<Category>();
        public ICollection<Tag> Tags { get; set; } = new List<Tag>();
    }
}
