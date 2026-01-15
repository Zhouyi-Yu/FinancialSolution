using FinanceApi.Data;
using FinanceApi.Models.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace FinanceApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class TransactionTemplatesController : ControllerBase
    {
        private readonly AppDbContext _context;

        public TransactionTemplatesController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetList()
        {
            var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userIdStr == null) return Unauthorized();

            var userId = Guid.Parse(userIdStr);
            
            // Use membership table - no need to Include BudgetSpace since we only use ID
            var membership = await _context.BudgetSpaceMembers
                .FirstOrDefaultAsync(m => m.UserId == userId);
                
            if (membership == null) return BadRequest("No budget space found");

            var templates = await _context.TransactionTemplates
                .Where(t => t.BudgetSpaceId == membership.BudgetSpaceId)
                .OrderBy(t => t.TemplateName)
                .ToListAsync();

            return Ok(templates);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] TransactionTemplate template)
        {
            var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userIdStr == null) return Unauthorized();

            var userId = Guid.Parse(userIdStr);
            
            // Use membership table - no need to Include BudgetSpace since we only use ID
            var membership = await _context.BudgetSpaceMembers
                .FirstOrDefaultAsync(m => m.UserId == userId);
                
            if (membership == null) return BadRequest("No budget space found");

            template.Id = Guid.NewGuid();
            template.BudgetSpaceId = membership.BudgetSpaceId;

            // Security: Ensure category belongs to space if set
            if (template.CategoryId.HasValue)
            {
                var catExists = await _context.Categories.AnyAsync(c => c.Id == template.CategoryId.Value && c.BudgetSpaceId == membership.BudgetSpaceId);
                if (!catExists) template.CategoryId = null; 
            }

            _context.TransactionTemplates.Add(template);
            await _context.SaveChangesAsync();
            return Ok(template);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userIdStr == null) return Unauthorized();

            var userId = Guid.Parse(userIdStr);
            
            // Use membership table - no need to Include BudgetSpace since we only use ID
            var membership = await _context.BudgetSpaceMembers
                .FirstOrDefaultAsync(m => m.UserId == userId);
                
            if (membership == null) return BadRequest("No budget space found");

            var template = await _context.TransactionTemplates
                .FirstOrDefaultAsync(t => t.Id == id && t.BudgetSpaceId == membership.BudgetSpaceId);

            if (template == null) return NotFound();

            _context.TransactionTemplates.Remove(template);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}
