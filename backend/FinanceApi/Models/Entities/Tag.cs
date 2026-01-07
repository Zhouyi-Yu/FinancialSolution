using System.ComponentModel.DataAnnotations;

namespace FinanceApi.Models.Entities
{
    public class Tag
    {
        public Guid Id { get; set; }

        public Guid BudgetSpaceId { get; set; }
        public BudgetSpace? BudgetSpace { get; set; }

        [Required]
        public string Name { get; set; } = string.Empty;

        // Navigation
        public ICollection<TransactionTag> TransactionTags { get; set; } = new List<TransactionTag>();
    }
}
