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

 modelBuilder.Entity<Invoice>()
    .Property(i => i.Amount)
    .HasPrecision(18, 2);

modelBuilder.Entity<InvoiceItem>()
    .Property(ii => ii.UnitPrice)
    .HasPrecision(18, 2);
}
}
