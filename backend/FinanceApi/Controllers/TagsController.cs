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
    public class TagsController : ControllerBase
    {
        private readonly AppDbContext _context;

        public TagsController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<TagDto>>> GetAll([FromQuery] Guid budgetSpaceId)
        {
            var tags = await _context.Tags
                .Where(t => t.BudgetSpaceId == budgetSpaceId)
                .Select(t => new TagDto
                {
                    Id = t.Id,
                    Name = t.Name,
                    UsageCount = t.TransactionTags.Count()
                })
                .OrderByDescending(t => t.UsageCount)
                .ToListAsync();

            return Ok(tags);
        }

        [HttpPost]
        public async Task<ActionResult<Tag>> Create(CreateTagRequest request)
        {
            var exists = await _context.Tags
                .AnyAsync(t => t.BudgetSpaceId == request.BudgetSpaceId && t.Name == request.Name);

            if (exists) return Conflict("Tag name already exists.");

            var tag = new Tag
            {
                BudgetSpaceId = request.BudgetSpaceId,
                Name = request.Name
            };

            _context.Tags.Add(tag);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetAll), new { budgetSpaceId = tag.BudgetSpaceId }, tag);
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<Tag>> Update(Guid id, CreateTagRequest request)
        {
            var tag = await _context.Tags.FindAsync(id);
            if (tag == null) return NotFound();

            tag.Name = request.Name;
            await _context.SaveChangesAsync();
            return Ok(tag);
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> Delete(Guid id)
        {
            var tag = await _context.Tags.FindAsync(id);
            if (tag == null) return NotFound();

            _context.Tags.Remove(tag);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }

    public class CreateTagRequest
    {
        public Guid BudgetSpaceId { get; set; }
        public string Name { get; set; } = string.Empty;
    }

    public class TagDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public int UsageCount { get; set; }
    }
}
