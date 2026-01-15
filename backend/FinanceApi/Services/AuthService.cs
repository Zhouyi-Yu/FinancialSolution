using FinanceApi.Data;
using FinanceApi.Models.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using BCrypt.Net;

namespace FinanceApi.Services
{
    public class AuthService
    {
        private readonly AppDbContext _context;
        private readonly IConfiguration _configuration;

        public AuthService(AppDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        public async Task<(bool Success, string Message, string? Token, User? User)> RegisterAsync(string email, string password, string? displayName)
        {
            if (await _context.Users.AnyAsync(u => u.Email == email))
            {
                return (false, "Email already in use.", null, null);
            }

            var passwordHash = BCrypt.Net.BCrypt.HashPassword(password);
            var user = new User
            {
                Email = email,
                PasswordHash = passwordHash,
                DisplayName = displayName,
                DefaultCurrency = "CAD" // Default
            };

            _context.Users.Add(user);
            
            // Create a default budget space for the user
            var budgetSpace = new BudgetSpace
            {
                Name = "My finances",
                BaseCurrency = "CAD",
                Owner = user
            };
            _context.BudgetSpaces.Add(budgetSpace);
            
            // Add membership as Owner
            var membership = new BudgetSpaceMember
            {
                BudgetSpace = budgetSpace,
                User = user,
                Role = BudgetRole.Owner
            };
            _context.BudgetSpaceMembers.Add(membership);
            
            // Seed default categories (Procore "Cost Catalog" inspired baseline)
            var defaultCategories = new List<Category>
            {
                new Category { Name = "Housing", AppliesTo = CategoryType.Expense, BudgetSpace = budgetSpace },
                new Category { Name = "Transportation", AppliesTo = CategoryType.Expense, BudgetSpace = budgetSpace },
                new Category { Name = "Food & Dining", AppliesTo = CategoryType.Expense, BudgetSpace = budgetSpace },
                new Category { Name = "Utilities", AppliesTo = CategoryType.Expense, BudgetSpace = budgetSpace },
                new Category { Name = "Salary", AppliesTo = CategoryType.Income, BudgetSpace = budgetSpace },
                new Category { Name = "Freelance", AppliesTo = CategoryType.Income, BudgetSpace = budgetSpace }
            };
            _context.Categories.AddRange(defaultCategories);

            await _context.SaveChangesAsync();

            var token = GenerateJwtToken(user);
            return (true, "Registered successfully.", token, user);
        }

        public async Task<(bool Success, string Message, string? Token, User? User)> LoginAsync(string email, string password)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
            if (user == null || !BCrypt.Net.BCrypt.Verify(password, user.PasswordHash))
            {
                return (false, "Invalid email or password.", null, null);
            }

            var token = GenerateJwtToken(user);
            return (true, "Login successful.", token, user);
        }

        private string GenerateJwtToken(User user)
        {
            var jwtKey = _configuration["Jwt:Key"];
            var jwtIssuer = _configuration["Jwt:Issuer"];
            var jwtAudience = _configuration["Jwt:Audience"];
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey!));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
                new Claim(JwtRegisteredClaimNames.Email, user.Email),
                new Claim("displayName", user.DisplayName ?? "")
            };

            var token = new JwtSecurityToken(
                issuer: jwtIssuer,
                audience: jwtAudience,
                claims: claims,
                expires: DateTime.Now.AddDays(7),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
