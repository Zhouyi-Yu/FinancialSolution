using System.ComponentModel.DataAnnotations;

namespace FinanceApi.Models.Entities
{
    public enum CategoryType
    {
        Expense,
        Income,
        Both
    }

    public class Category
    {
        public Guid Id { get; set; }

        public Guid BudgetSpaceId { get; set; }
        public BudgetSpace? BudgetSpace { get; set; }

        [Required]
        public string Name { get; set; } = string.Empty;

        public CategoryType AppliesTo { get; set; } = CategoryType.Both;

        // Navigation
        public ICollection<Transaction> Transactions { get; set; } = new List<Transaction>();
    }
}
