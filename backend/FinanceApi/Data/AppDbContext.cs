using FinanceApi.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace FinanceApi.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<User> Users { get; set; }
        public DbSet<BudgetSpace> BudgetSpaces { get; set; }
        public DbSet<BudgetSpaceMember> BudgetSpaceMembers { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Tag> Tags { get; set; }
        public DbSet<Transaction> Transactions { get; set; }
        public DbSet<TransactionTag> TransactionTags { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // User
            modelBuilder.Entity<User>()
                .HasIndex(u => u.Email)
                .IsUnique();

            // BudgetSpaceMember (Composite Key)
            modelBuilder.Entity<BudgetSpaceMember>()
                .HasKey(m => new { m.BudgetSpaceId, m.UserId });

            modelBuilder.Entity<BudgetSpaceMember>()
                .HasOne(m => m.BudgetSpace)
                .WithMany(b => b.Members)
                .HasForeignKey(m => m.BudgetSpaceId);

            modelBuilder.Entity<BudgetSpaceMember>()
                .HasOne(m => m.User)
                .WithMany(u => u.Memberships)
                .HasForeignKey(m => m.UserId);

            // Category
            modelBuilder.Entity<Category>()
                .HasIndex(c => new { c.BudgetSpaceId, c.Name })
                .IsUnique();
            
            // Tag
            modelBuilder.Entity<Tag>()
                .HasIndex(t => new { t.BudgetSpaceId, t.Name })
                .IsUnique();

            // TransactionTag (Composite Key)
            modelBuilder.Entity<TransactionTag>()
                .HasKey(tt => new { tt.TransactionId, tt.TagId });
            
            modelBuilder.Entity<TransactionTag>()
                .HasOne(tt => tt.Transaction)
                .WithMany(t => t.TransactionTags)
                .HasForeignKey(tt => tt.TransactionId);

            modelBuilder.Entity<TransactionTag>()
                .HasOne(tt => tt.Tag)
                .WithMany(t => t.TransactionTags)
                .HasForeignKey(tt => tt.TagId);

            // Indexes for Performance
            modelBuilder.Entity<Transaction>()
                .HasIndex(t => new { t.BudgetSpaceId, t.Date }); // For sorting/filtering by date
            
            modelBuilder.Entity<Transaction>()
                .HasIndex(t => new { t.BudgetSpaceId, t.IsDeleted }); // For filtering logic

            modelBuilder.Entity<Transaction>()
                .HasIndex(t => t.CategoryId);
        }
    }
}
