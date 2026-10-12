
using BankingAccountApi.Models;
using Microsoft.EntityFrameworkCore;

namespace WebApplication2.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(
        DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<BankAccount> BankAccounts => Set<BankAccount>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<BankAccount>()
            .HasIndex(account => account.AccountNumber)
            .IsUnique();
    }
}
