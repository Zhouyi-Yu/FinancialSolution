using System.ComponentModel.DataAnnotations;

namespace FinanceApi.Models.Entities
{
    public enum BudgetRole
    {
        Owner,
        Editor,
        Viewer
    }

    public class BudgetSpaceMember
    {
        public Guid BudgetSpaceId { get; set; }
        public BudgetSpace? BudgetSpace { get; set; }

        public Guid UserId { get; set; }
        public User? User { get; set; }

        public BudgetRole Role { get; set; }
    }
}
