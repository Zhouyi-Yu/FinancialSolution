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
    public class CategoriesController : ControllerBase
    {
        private readonly AppDbContext _context;

        public CategoriesController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Category>>> GetAll([FromQuery] Guid budgetSpaceId)
        {
            return await _context.Categories
                .Where(c => c.BudgetSpaceId == budgetSpaceId)
                .OrderBy(c => c.Name)
                .ToListAsync();
        }

        [HttpPost]
        public async Task<ActionResult<Category>> Create(CreateCategoryRequest request)
        {
            var exists = await _context.Categories
                .AnyAsync(c => c.BudgetSpaceId == request.BudgetSpaceId && c.Name == request.Name);

            if (exists) return Conflict("Category name already exists in this budget space.");

            var category = new Category
            {
                BudgetSpaceId = request.BudgetSpaceId,
                Name = request.Name,
                AppliesTo = request.AppliesTo
            };

            _context.Categories.Add(category);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetAll), new { budgetSpaceId = category.BudgetSpaceId }, category);
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<Category>> Update(Guid id, CreateCategoryRequest request)
        {
            var category = await _context.Categories.FindAsync(id);
            if (category == null) return NotFound();

            category.Name = request.Name;
            category.AppliesTo = request.AppliesTo;
            await _context.SaveChangesAsync();
            return Ok(category);
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> Delete(Guid id)
        {
            var category = await _context.Categories.FindAsync(id);
            if (category == null) return NotFound();

            // Check usage
            var used = await _context.Transactions.AnyAsync(t => t.CategoryId == id && !t.IsDeleted);
            if (used) 
            {
                // For demo, prevent deletion
                return Conflict("Category is in use. Cannot delete.");
                // Or set to null:
                // var txs = _context.Transactions.Where(t => t.CategoryId == id);
                // foreach(var t in txs) t.CategoryId = null;
            }

            _context.Categories.Remove(category);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }

    public class CreateCategoryRequest
    {
        public Guid BudgetSpaceId { get; set; }
        public string Name { get; set; } = string.Empty;
        public CategoryType AppliesTo { get; set; } = CategoryType.Both;
    }
}
