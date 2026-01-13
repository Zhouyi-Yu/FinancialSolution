using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FinanceApi.Models.Entities
{
    public enum TransactionType
    {
        Income,
        Expense
    }

    public class Transaction
    {
        public Guid Id { get; set; }

        public Guid BudgetSpaceId { get; set; }
        public BudgetSpace? BudgetSpace { get; set; }

        public TransactionType Type { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal Amount { get; set; }

        [Required]
        public string Currency { get; set; } = "CAD";

        [Column(TypeName = "decimal(18,6)")]
        public decimal ExchangeRateToBase { get; set; } = 1.0m;

        [Column(TypeName = "decimal(18,2)")]
        public decimal AmountInBase { get; set; }

        public DateTime Date { get; set; }

        public Guid? CategoryId { get; set; }
        public Category? Category { get; set; }

        public string? Merchant { get; set; }
        public string? Note { get; set; }

        public bool IsDeleted { get; set; } = false;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // Navigation
        public ICollection<TransactionTag> TransactionTags { get; set; } = new List<TransactionTag>();

        // For PDF import duplicate detection
        public string? DeduplicationHash { get; set; }
    }
}
