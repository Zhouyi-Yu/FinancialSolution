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
        public async Task<ActionResult<TrendsDto>> GetTrends([FromQuery] Guid budgetSpaceId, [FromQuery] string range = "6M")
        {
            var end = DateTime.UtcNow;
            DateTime start;
            string granularity = "Month"; // Hour, Day, Week, Month

            switch (range.ToUpper())
            {
                case "1D":
                    start = end.AddHours(-24);
                    granularity = "Hour";
                    break;
                case "1W":
                    start = end.AddDays(-7);
                    granularity = "Day";
                    break;
                case "1M":
                    start = end.AddDays(-30);
                    granularity = "Day";
                    break;
                case "3M":
                    start = end.AddMonths(-3);
                    granularity = "Week";
                    break;
                case "6M":
                    start = end.AddMonths(-6);
                    granularity = "Month";
                    break;
                case "MAX":
                    start = DateTime.MinValue; // Will be clamped by query
                    granularity = "Month";
                    break;
                default: // Fallback to 6M logic
                    start = end.AddMonths(-6);
                    granularity = "Month";
                    break;
            }

            // Ensure UTC kind for Npgsql
            if (start.Kind == DateTimeKind.Unspecified && start != DateTime.MinValue)
                start = DateTime.SpecifyKind(start, DateTimeKind.Utc);
            
            // 1. Fetch Raw Data (Client-side evaluation for grouping flexibility)
            var query = _context.Transactions
                .Where(t => t.BudgetSpaceId == budgetSpaceId && 
                            !t.IsDeleted);
            
            if (range.ToUpper() != "MAX")
            {
                query = query.Where(t => t.Date >= start);
            }

            var rawData = await query
                .Select(t => new { t.Date, t.Type, t.AmountInBase })
                .ToListAsync();

            // 2. Group Data in Memory
            IEnumerable<dynamic> groupedData;
            
            if (granularity == "Hour")
            {
                groupedData = rawData
                    .GroupBy(t => new { t.Date.Date, t.Date.Hour })
                    .Select(g => new 
                    { 
                        Label = $"{g.Key.Hour}:00",
                        SortKey = g.Key.Date.AddHours(g.Key.Hour),
                        Income = g.Where(x => x.Type == TransactionType.Income).Sum(x => x.AmountInBase),
                        Expense = g.Where(x => x.Type == TransactionType.Expense).Sum(x => x.AmountInBase)
                    });
            }
            else if (granularity == "Day")
            {
                groupedData = rawData
                    .GroupBy(t => t.Date.Date)
                    .Select(g => new 
                    { 
                        Label = g.Key.ToString("MM-dd"),
                        SortKey = g.Key,
                        Income = g.Where(x => x.Type == TransactionType.Income).Sum(x => x.AmountInBase),
                        Expense = g.Where(x => x.Type == TransactionType.Expense).Sum(x => x.AmountInBase)
                    });
            }
            else if (granularity == "Week")
            {
                // Group by start of week (Sunday)
                groupedData = rawData
                    .GroupBy(t => t.Date.Date.AddDays(-(int)t.Date.DayOfWeek))
                    .Select(g => new 
                    { 
                        Label = g.Key.ToString("MM-dd"),
                        SortKey = g.Key,
                        Income = g.Where(x => x.Type == TransactionType.Income).Sum(x => x.AmountInBase),
                        Expense = g.Where(x => x.Type == TransactionType.Expense).Sum(x => x.AmountInBase)
                    });
            }
            else // Month
            {
                groupedData = rawData
                    .GroupBy(t => new { t.Date.Year, t.Date.Month })
                    .Select(g => new 
                    { 
                        Label = $"{g.Key.Year}-{g.Key.Month:D2}",
                        SortKey = new DateTime(g.Key.Year, g.Key.Month, 1),
                        Income = g.Where(x => x.Type == TransactionType.Income).Sum(x => x.AmountInBase),
                        Expense = g.Where(x => x.Type == TransactionType.Expense).Sum(x => x.AmountInBase)
                    });
            }

            var points = groupedData
                .OrderBy(x => x.SortKey)
                .Select(d => new TrendPoint 
                { 
                    Month = d.Label, // Reusing 'Month' property for Label to avoid breaking DTO
                    Income = d.Income,
                    Expense = d.Expense,
                    Net = d.Income - d.Expense
                })
                .ToList();

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
