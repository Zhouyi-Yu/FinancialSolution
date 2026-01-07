namespace FinanceApi.Models.Entities
{
    public class TransactionTag
    {
        public Guid TransactionId { get; set; }
        public Transaction? Transaction { get; set; }

        public Guid TagId { get; set; }
        public Tag? Tag { get; set; }
    }
}
