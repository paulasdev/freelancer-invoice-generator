using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SoloBill.Models;

namespace SoloBill.Data;

public class SoloBillDbContext : IdentityDbContext<ApplicationUser>
{
    public SoloBillDbContext(DbContextOptions<SoloBillDbContext> options)
        : base(options)
    {
    }
    public DbSet<Client> Clients { get; set; }
    public DbSet<Invoice> Invoices { get; set; }
    public DbSet<InvoiceItem> InvoiceItems { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        //Prevents truncation of decimal values in SQL Server
        modelBuilder.Entity<Invoice>()
            .Property(i => i.Amount)
            .HasPrecision(18, 2);

//Test data      
modelBuilder.Entity<Client>().HasData(new Client
{
    ClientId = 1,
    Name = "ACME Inc.",
    Email = "info@acme.com",
    Company = "ACME Inc.",
    Address = "123 Main Street"
});

modelBuilder.Entity<Invoice>().HasData(new Invoice
{
    InvoiceId = 1,
    ClientId = 1,
    IssueDate = new DateTime(2025, 7, 1),
    DueDate = new DateTime(2025, 7, 31),
    Amount = 500.00m,
    IsPaid = false
});
    }
}
