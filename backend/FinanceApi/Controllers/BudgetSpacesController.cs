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
    public class BudgetSpacesController : ControllerBase
    {
        private readonly AppDbContext _context;

        public BudgetSpacesController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<BudgetSpaceDto>>> GetMySpaces()
        {
            var userId = GetUserId();
            var spaces = await _context.BudgetSpaceMembers
                .Where(m => m.UserId == userId)
                .Include(m => m.BudgetSpace)
                .Select(m => new BudgetSpaceDto
                {
                    Id = m.BudgetSpaceId,
                    Name = m.BudgetSpace!.Name,
                    BaseCurrency = m.BudgetSpace.BaseCurrency,
                    Role = m.Role.ToString()
                })
                .ToListAsync();

            return Ok(spaces);
        }

        [HttpPost]
        public async Task<ActionResult<BudgetSpaceDto>> Create(CreateBudgetSpaceRequest request)
        {
            var userId = GetUserId();
            var user = await _context.Users.FindAsync(userId);
            if (user == null) return Unauthorized();

            var space = new BudgetSpace
            {
                Name = request.Name,
                BaseCurrency = request.BaseCurrency,
                OwnerUserId = userId
            };
            _context.BudgetSpaces.Add(space);

            var member = new BudgetSpaceMember
            {
                BudgetSpace = space,
                User = user,
                Role = BudgetRole.Owner
            };
            _context.BudgetSpaceMembers.Add(member);

            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetMySpaces), new BudgetSpaceDto
            {
                Id = space.Id,
                Name = space.Name,
                BaseCurrency = space.BaseCurrency,
                Role = BudgetRole.Owner.ToString()
            });
        }

        private Guid GetUserId()
        {
            var claim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (claim == null) throw new UnauthorizedAccessException();
            return Guid.Parse(claim.Value);
        }
    }

    public class BudgetSpaceDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string BaseCurrency { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
    }

    public class CreateBudgetSpaceRequest
    {
        public string Name { get; set; } = string.Empty;
        public string BaseCurrency { get; set; } = "CAD";
    }
}
