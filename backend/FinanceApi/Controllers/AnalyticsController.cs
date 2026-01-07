using FinanceApi.Data;
using FinanceApi.Models.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FinanceApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class AnalyticsController : ControllerBase
    {
        private readonly AppDbContext _context;

        public AnalyticsController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet("monthlySummary")]
        public async Task<ActionResult<MonthlySummaryDto>> GetMonthlySummary([FromQuery] Guid budgetSpaceId, [FromQuery] string month)
        {
            if (!DateTime.TryParse(month + "-01", out var date)) return BadRequest("Invalid month format (YYYY-MM)");
            
            var start = new DateTime(date.Year, date.Month, 1, 0, 0, 0, DateTimeKind.Utc);
            var end = start.AddMonths(1);

            var transactions = await _context.Transactions
                .Where(t => t.BudgetSpaceId == budgetSpaceId && 
                            !t.IsDeleted &&
                            t.Date >= start && t.Date < end)
                .ToListAsync();

            var income = transactions.Where(t => t.Type == TransactionType.Income).Sum(t => t.AmountInBase);
            var expense = transactions.Where(t => t.Type == TransactionType.Expense).Sum(t => t.AmountInBase);

            return Ok(new MonthlySummaryDto
            {
                Month = month,
                IncomeTotal = income,
                ExpenseTotal = expense,
                Net = income - expense
            });
        }

        [HttpGet("categoryBreakdown")]
        public async Task<ActionResult<CategoryBreakdownDto>> GetCategoryBreakdown([FromQuery] Guid budgetSpaceId, [FromQuery] string month, [FromQuery] TransactionType type = TransactionType.Expense)
        {
            if (!DateTime.TryParse(month + "-01", out var date)) return BadRequest("Invalid month format (YYYY-MM)");

            var start = new DateTime(date.Year, date.Month, 1, 0, 0, 0, DateTimeKind.Utc);
            var end = start.AddMonths(1);

            var data = await _context.Transactions
                .Where(t => t.BudgetSpaceId == budgetSpaceId && 
                            !t.IsDeleted &&
                            t.Type == type &&
                            t.Date >= start && t.Date < end)
                .GroupBy(t => t.Category != null ? t.Category.Name : "Uncategorized")
                .Select(g => new { Category = g.Key, Total = g.Sum(x => x.AmountInBase) })
                .OrderByDescending(x => x.Total)
                .ToListAsync();

            var items = data.Select(d => new CategoryRef { CategoryName = d.Category, Total = d.Total }).ToList();
            
            return Ok(new CategoryBreakdownDto { Month = month, Items = items });
        }

        [HttpGet("trends")]
        public async Task<ActionResult<TrendsDto>> GetTrends([FromQuery] Guid budgetSpaceId, [FromQuery] int months = 6)
        {
            var end = DateTime.UtcNow;
            var start = new DateTime(end.Year, end.Month, 1).AddMonths(-(months - 1));

            var data = await _context.Transactions
                .Where(t => t.BudgetSpaceId == budgetSpaceId && 
                            !t.IsDeleted &&
                            t.Date >= start)
                .GroupBy(t => new { t.Date.Year, t.Date.Month })
                .Select(g => new 
                { 
                    Year = g.Key.Year, 
                    Month = g.Key.Month,
                    Income = g.Where(x => x.Type == TransactionType.Income).Sum(x => x.AmountInBase),
                    Expense = g.Where(x => x.Type == TransactionType.Expense).Sum(x => x.AmountInBase)
                })
                .OrderBy(x => x.Year).ThenBy(x => x.Month)
                .ToListAsync();

            var points = data.Select(d => new TrendPoint 
            { 
                Month = $"{d.Year}-{d.Month:D2}",
                Income = d.Income,
                Expense = d.Expense,
                Net = d.Income - d.Expense
            }).ToList();

            return Ok(new TrendsDto { Points = points });
        }
    }

    public class MonthlySummaryDto
    {
        public string Month { get; set; } = string.Empty;
        public decimal IncomeTotal { get; set; }
        public decimal ExpenseTotal { get; set; }
        public decimal Net { get; set; }
    }

    public class CategoryBreakdownDto
    {
        public string Month { get; set; } = string.Empty;
        public List<CategoryRef> Items { get; set; } = new();
    }

    public class CategoryRef
    {
        public string CategoryName { get; set; } = string.Empty;
        public decimal Total { get; set; }
    }

    public class TrendsDto
    {
        public List<TrendPoint> Points { get; set; } = new();
    }

    public class TrendPoint
    {
        public string Month { get; set; } = string.Empty;
        public decimal Income { get; set; }
        public decimal Expense { get; set; }
        public decimal Net { get; set; }
    }
}
