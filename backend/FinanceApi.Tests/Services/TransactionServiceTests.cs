using FinanceApi.Data;
using FinanceApi.Models.Entities;
using FinanceApi.Services;
using Microsoft.EntityFrameworkCore;

namespace FinanceApi.Tests.Services
{
    public class TransactionServiceTests
    {
        private readonly AppDbContext _context;
        private readonly TransactionService _service;

        public TransactionServiceTests()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString()) // Unique DB per test
                .Options;

            _context = new AppDbContext(options);
            _service = new TransactionService(_context);
        }

        [Fact]
        public async Task CreateAsync_ShouldAddTransaction()
        {
            // Arrange
            var tx = new Transaction
            {
                Amount = 100,
                Type = TransactionType.Expense,
                Date = DateTime.Now,
                BudgetSpaceId = Guid.NewGuid()
            };

            // Act
            var created = await _service.CreateAsync(tx, new List<Guid>());

            // Assert
            Assert.NotEqual(Guid.Empty, created.Id);
            Assert.Equal(1, await _context.Transactions.CountAsync());
        }

        [Fact]
        public async Task GetTransactionsAsync_ShouldFilterByType()
        {
            // Arrange
            var spaceId = Guid.NewGuid();
            _context.Transactions.AddRange(
                new Transaction { BudgetSpaceId = spaceId, Type = TransactionType.Income, Amount = 1000, Date = DateTime.Now },
                new Transaction { BudgetSpaceId = spaceId, Type = TransactionType.Expense, Amount = 50, Date = DateTime.Now },
                new Transaction { BudgetSpaceId = spaceId, Type = TransactionType.Expense, Amount = 20, Date = DateTime.Now }
            );
            await _context.SaveChangesAsync();

            // Act
            var (results, count) = await _service.GetTransactionsAsync(spaceId, 1, 10, null, null, TransactionType.Expense, null, null);

            // Assert
            Assert.Equal(2, count);
            Assert.All(results, t => Assert.Equal(TransactionType.Expense, t.Type));
        }

        [Fact]
        public async Task GetTransactionsAsync_ShouldRespectPagination()
        {
             // Arrange
            var spaceId = Guid.NewGuid();
            var txs = Enumerable.Range(1, 15).Select(i => new Transaction 
            { 
                BudgetSpaceId = spaceId, 
                Amount = i, 
                Date = DateTime.Now.AddDays(-i) 
            });
            _context.Transactions.AddRange(txs);
            await _context.SaveChangesAsync();

            // Act
            var (results, count) = await _service.GetTransactionsAsync(spaceId, page: 2, pageSize: 5, null, null, null, null, null);

            // Assert
            Assert.Equal(15, count);
            Assert.Equal(5, results.Count());
        }

        [Fact]
        public async Task GetTransactionsAsync_ShouldNeverReturnSoftDeletedRecords()
        {
            // Arrange
            var spaceId = Guid.NewGuid();
            var tx = new Transaction 
            { 
                BudgetSpaceId = spaceId, 
                Amount = 100, 
                IsDeleted = true, // Soft deleted
                Date = DateTime.Now 
            };
            _context.Transactions.Add(tx);
            await _context.SaveChangesAsync();

            // Act
            var (results, count) = await _service.GetTransactionsAsync(spaceId, 1, 10, null, null, null, null, null);

            // Assert
            Assert.Equal(0, count);
            Assert.Empty(results);
        }

        [Fact]
        public async Task UpdateAsync_ShouldCorrectlyCalculateBaseAmount()
        {
            // Arrange
            var spaceId = Guid.NewGuid();
            var tx = new Transaction { Id = Guid.NewGuid(), BudgetSpaceId = spaceId, Amount = 100, ExchangeRateToBase = 1.0m, Date = DateTime.Now };
            _context.Transactions.Add(tx);
            await _context.SaveChangesAsync();

            var updates = new Transaction { Amount = 100, ExchangeRateToBase = 1.35m }; // USD to CAD for example

            // Act
            var updated = await _service.UpdateAsync(tx.Id, spaceId, updates, null);

            // Assert
            Assert.NotNull(updated);
            Assert.Equal(135.0m, updated!.AmountInBase); // Verification of line 87 logic
        }

        [Fact]
        public async Task GetTransactionsAsync_ShouldHandleNullMerchantAndNoteInSearch()
        {
            // Arrange
            var spaceId = Guid.NewGuid();
            _context.Transactions.Add(new Transaction 
            { 
                BudgetSpaceId = spaceId, 
                Amount = 50, 
                Merchant = null, // Edge case: null merchant
                Note = "Finding this", 
                Date = DateTime.Now 
            });
            await _context.SaveChangesAsync();

            // Act
            var (results, count) = await _service.GetTransactionsAsync(spaceId, 1, 10, null, null, null, null, "finding");

            // Assert
            Assert.Equal(1, count);
            Assert.Single(results);
        }
    }
}

