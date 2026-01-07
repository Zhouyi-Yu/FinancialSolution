using FinanceApi.Services;
using FinanceApi.Models.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace FinanceApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class TransactionsController : ControllerBase
    {
        private readonly TransactionService _service;

        public TransactionsController(TransactionService service)
        {
            _service = service;
        }

        [HttpGet]
        public async Task<ActionResult<PagedResult<TransactionDto>>> GetAll(
            [FromQuery] Guid budgetSpaceId,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20,
            [FromQuery] DateTime? from = null,
            [FromQuery] DateTime? to = null,
            [FromQuery] TransactionType? type = null,
            [FromQuery] Guid? categoryId = null,
            [FromQuery] string? q = null)
        {
            // TODO: Validate user access to budgetSpaceId here or in middleware
            var (items, count) = await _service.GetTransactionsAsync(budgetSpaceId, page, pageSize, from, to, type, categoryId, q);
            
            var dtos = items.Select(MapToDto).ToList();
            return Ok(new PagedResult<TransactionDto> { Items = dtos, TotalCount = count, Page = page, PageSize = pageSize });
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<TransactionDto>> Get(Guid id, [FromQuery] Guid budgetSpaceId)
        {
            var item = await _service.GetByIdAsync(id, budgetSpaceId);
            if (item == null) return NotFound();
            return Ok(MapToDto(item));
        }

        [HttpPost]
        public async Task<ActionResult<TransactionDto>> Create(CreateTransactionRequest request)
        {
            var tx = new Transaction
            {
                BudgetSpaceId = request.BudgetSpaceId,
                Type = request.Type,
                Amount = request.Amount,
                Currency = request.Currency,
                ExchangeRateToBase = request.ExchangeRateToBase ?? 1.0m,
                AmountInBase = request.Amount * (request.ExchangeRateToBase ?? 1.0m),
                Date = request.Date,
                CategoryId = request.CategoryId,
                Merchant = request.Merchant,
                Note = request.Note
            };

            await _service.CreateAsync(tx, request.TagIds ?? new List<Guid>());
            return CreatedAtAction(nameof(Get), new { id = tx.Id, budgetSpaceId = tx.BudgetSpaceId }, MapToDto(tx));
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<TransactionDto>> Update(Guid id, UpdateTransactionRequest request)
        {
            var tx = new Transaction
            {
                Type = request.Type,
                Amount = request.Amount,
                Currency = request.Currency,
                ExchangeRateToBase = request.ExchangeRateToBase ?? 1.0m,
                Date = request.Date,
                CategoryId = request.CategoryId,
                Merchant = request.Merchant,
                Note = request.Note
            };

            var updated = await _service.UpdateAsync(id, request.BudgetSpaceId, tx, request.TagIds);
            if (updated == null) return NotFound();

            return Ok(MapToDto(updated));
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> Delete(Guid id, [FromQuery] Guid budgetSpaceId)
        {
            var success = await _service.DeleteAsync(id, budgetSpaceId);
            if (!success) return NotFound();
            return NoContent();
        }

        private static TransactionDto MapToDto(Transaction t)
        {
            return new TransactionDto
            {
                Id = t.Id,
                Type = t.Type.ToString(),
                Amount = t.Amount,
                Currency = t.Currency,
                Date = t.Date,
                CategoryName = t.Category?.Name,
                Merchant = t.Merchant,
                Tags = t.TransactionTags.Select(tt => tt.Tag!.Name).ToList()
            };
        }
    }

    public class TransactionDto
    {
        public Guid Id { get; set; }
        public string Type { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public string Currency { get; set; } = string.Empty;
        public DateTime Date { get; set; }
        public string? CategoryName { get; set; }
        public string? Merchant { get; set; }
        public List<string> Tags { get; set; } = new();
    }

    public class CreateTransactionRequest
    {
        public Guid BudgetSpaceId { get; set; }
        public TransactionType Type { get; set; }
        [Range(0.01, double.MaxValue)] public decimal Amount { get; set; }
        public string Currency { get; set; } = "CAD";
        public decimal? ExchangeRateToBase { get; set; }
        public DateTime Date { get; set; }
        public Guid? CategoryId { get; set; }
        public List<Guid>? TagIds { get; set; }
        public string? Merchant { get; set; }
        public string? Note { get; set; }
    }

    public class UpdateTransactionRequest : CreateTransactionRequest { }

    public class PagedResult<T>
    {
        public List<T> Items { get; set; } = new();
        public int TotalCount { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
    }
}
