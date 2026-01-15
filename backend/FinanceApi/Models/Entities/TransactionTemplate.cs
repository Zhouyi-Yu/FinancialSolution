using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace FinanceApi.Models.Entities
{
    public class TransactionTemplate
    {
        public Guid Id { get; set; }

        public Guid BudgetSpaceId { get; set; }
        [JsonIgnore]
        public BudgetSpace? BudgetSpace { get; set; }

        [Required]
        [MaxLength(100)]
        public string TemplateName { get; set; } = string.Empty; // e.g. "Rent", "Netflix"

        public TransactionType Type { get; set; } // Income vs Expense

        [Column(TypeName = "decimal(18,2)")]
        public decimal? Amount { get; set; } // Optional: some templates might just be for the Name/Category

        public string Currency { get; set; } = "CAD";

        public Guid? CategoryId { get; set; }
        [JsonIgnore]
        public Category? Category { get; set; }

        public string? Merchant { get; set; }
        public string? Note { get; set; } // Default note
    }
}
