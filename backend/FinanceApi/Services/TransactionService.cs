using FinanceApi.Data;
using FinanceApi.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace FinanceApi.Services
{
    public class TransactionService
    {
        private readonly AppDbContext _context;

        public TransactionService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<(IEnumerable<Transaction>, int TotalCount)> GetTransactionsAsync(
            Guid budgetSpaceId, 
            int page, 
            int pageSize, 
            DateTime? fromDate, 
            DateTime? toDate, 
            TransactionType? type, 
            Guid? categoryId, 
            string? descriptionQuery)
        {
            var query = _context.Transactions
                .Include(t => t.Category)
                .Include(t => t.TransactionTags).ThenInclude(tt => tt.Tag)
                .Where(t => t.BudgetSpaceId == budgetSpaceId && !t.IsDeleted);

            if (fromDate.HasValue) query = query.Where(t => t.Date >= fromDate.Value);
            if (toDate.HasValue) query = query.Where(t => t.Date <= toDate.Value);
            if (type.HasValue) query = query.Where(t => t.Type == type.Value);
            if (categoryId.HasValue) query = query.Where(t => t.CategoryId == categoryId.Value);
            if (!string.IsNullOrEmpty(descriptionQuery))
            {
                var lowerQ = descriptionQuery.ToLower();
                query = query.Where(t => (t.Note != null && t.Note.ToLower().Contains(lowerQ)) || (t.Merchant != null && t.Merchant.ToLower().Contains(lowerQ)));
            }
            
            var totalCount = await query.CountAsync();
            var items = await query
                .OrderByDescending(t => t.Date)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (items, totalCount);
        }

        public async Task<Transaction?> GetByIdAsync(Guid id, Guid budgetSpaceId)
        {
            return await _context.Transactions
                .Include(t => t.Category)
                .Include(t => t.TransactionTags).ThenInclude(tt => tt.Tag)
                .FirstOrDefaultAsync(t => t.Id == id && t.BudgetSpaceId == budgetSpaceId && !t.IsDeleted);
        }

        public async Task<Transaction> CreateAsync(Transaction transaction, List<Guid> tagIds)
        {
            _context.Transactions.Add(transaction);
            
            if (tagIds.Any())
            {
                foreach (var tagId in tagIds)
                {
                    _context.TransactionTags.Add(new TransactionTag { TransactionId = transaction.Id, TagId = tagId });
                }
            }

            await _context.SaveChangesAsync();
            return transaction;
        }

        public async Task<Transaction?> UpdateAsync(Guid id, Guid budgetSpaceId, Transaction updates, List<Guid>? tagIds)
        {
            var existing = await _context.Transactions
                .Include(t => t.TransactionTags)
                .FirstOrDefaultAsync(t => t.Id == id && t.BudgetSpaceId == budgetSpaceId && !t.IsDeleted);

            if (existing == null) return null;

            existing.Type = updates.Type;
            existing.Amount = updates.Amount;
            existing.Currency = updates.Currency;
            existing.ExchangeRateToBase = updates.ExchangeRateToBase;
            existing.AmountInBase = updates.Amount * updates.ExchangeRateToBase; // Simple computation
            existing.Date = updates.Date;
            existing.CategoryId = updates.CategoryId;
            existing.Merchant = updates.Merchant;
            existing.Note = updates.Note;
            existing.UpdatedAt = DateTime.UtcNow;

            if (tagIds != null)
            {
                _context.TransactionTags.RemoveRange(existing.TransactionTags);
                foreach (var tagId in tagIds)
                {
                    _context.TransactionTags.Add(new TransactionTag { TransactionId = existing.Id, TagId = tagId });
                }
            }

            await _context.SaveChangesAsync();
            return existing;
        }

        public async Task<bool> DeleteAsync(Guid id, Guid budgetSpaceId)
        {
            var transaction = await _context.Transactions.FirstOrDefaultAsync(t => t.Id == id && t.BudgetSpaceId == budgetSpaceId);
            if (transaction == null) return false;

            transaction.IsDeleted = true;
            transaction.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
