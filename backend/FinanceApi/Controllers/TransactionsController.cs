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
        private readonly PdfParserService _pdfParser;

        public TransactionsController(TransactionService service, PdfParserService pdfParser)
        {
            _service = service;
            _pdfParser = pdfParser;
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

        // AC 1 & AC 5: Upload PDF and return preview for user confirmation
        [HttpPost("import/preview")]
        public async Task<ActionResult<PdfImportPreviewResponse>> PreviewImport([FromForm] PdfImportRequest request)
        {
            if (request.File == null || request.File.Length == 0)
            {
                return BadRequest(new { message = "No file uploaded" });
            }

            if (!request.File.FileName.EndsWith(".pdf", StringComparison.OrdinalIgnoreCase))
            {
                return BadRequest(new { message = "Only PDF files are supported" });
            }

            using var stream = request.File.OpenReadStream();
            var parseResult = await _pdfParser.ParseBankStatementAsync(stream);

            if (!parseResult.Success)
            {
                // AC 6: Error handling for encryption or format issues
                return BadRequest(new { message = parseResult.ErrorMessage });
            }

            // AC 4: Check for duplicates
            var existingHashes = await _service.GetExistingHashesAsync(request.BudgetSpaceId);
            var preview = parseResult.Transactions.Select(t => new ParsedTransactionPreview
            {
                Date = t.Date,
                Description = t.Description,
                Amount = t.Amount,
                Type = t.Type.ToString(),
                IsDuplicate = existingHashes.Contains(t.DeduplicationHash),
                Hash = t.DeduplicationHash
            }).ToList();

            return Ok(new PdfImportPreviewResponse
            {
                BankDetected = parseResult.BankDetected,
                TotalTransactions = preview.Count,
                DuplicatesDetected = preview.Count(p => p.IsDuplicate),
                Transactions = preview
            });
        }

        // AC 3 & AC 5: Confirm and save previewed transactions
        [HttpPost("import/confirm")]
        public async Task<ActionResult<ImportConfirmResponse>> ConfirmImport(ConfirmImportRequest request)
        {
            var imported = new List<Guid>();
            var skipped = 0;

            foreach (var txPreview in request.Transactions)
            {
                // Skip duplicates (AC 4)
                if (txPreview.IsDuplicate && !request.ImportDuplicates)
                {
                    skipped++;
                    continue;
                }

                var transaction = new Transaction
                {
                    BudgetSpaceId = request.BudgetSpaceId,
                    Type = Enum.Parse<TransactionType>(txPreview.Type),
                    Amount = txPreview.Amount,
                    Currency = "CAD", // Default, could be enhanced
                    ExchangeRateToBase = 1.0m,
                    AmountInBase = txPreview.Amount,
                    Date = txPreview.Date,
                    Merchant = txPreview.Description,
                    Note = "Imported from PDF",
                    CategoryId = request.DefaultCategoryId
                };

                await _service.CreateAsync(transaction, new List<Guid>());
                imported.Add(transaction.Id);
            }

            return Ok(new ImportConfirmResponse
            {
                ImportedCount = imported.Count,
                SkippedCount = skipped,
                TransactionIds = imported
            });
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

    // PDF Import DTOs
    public class PdfImportRequest
    {
        public Guid BudgetSpaceId { get; set; }
        public IFormFile? File { get; set; }
    }

    public class ParsedTransactionPreview
    {
        public DateTime Date { get; set; }
        public string Description { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public string Type { get; set; } = string.Empty;
        public bool IsDuplicate { get; set; }
        public string Hash { get; set; } = string.Empty;
    }

    public class PdfImportPreviewResponse
    {
        public string BankDetected { get; set; } = string.Empty;
        public int TotalTransactions { get; set; }
        public int DuplicatesDetected { get; set; }
        public List<ParsedTransactionPreview> Transactions { get; set; } = new();
    }

    public class ConfirmImportRequest
    {
        public Guid BudgetSpaceId { get; set; }
        public Guid? DefaultCategoryId { get; set; }
        public bool ImportDuplicates { get; set; } = false;
        public List<ParsedTransactionPreview> Transactions { get; set; } = new();
    }

    public class ImportConfirmResponse
    {
        public int ImportedCount { get; set; }
        public int SkippedCount { get; set; }
        public List<Guid> TransactionIds { get; set; } = new();
    }
}
