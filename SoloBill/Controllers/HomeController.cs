using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SoloBill.Data;
using SoloBill.Models;

namespace SoloBill.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SoloBillDbContext _db;

    public HomeController(
        ILogger<HomeController> logger,
        SignInManager<ApplicationUser> signInManager,
        UserManager<ApplicationUser> userManager,
        SoloBillDbContext db)
    {
        _logger = logger;
        _signInManager = signInManager;
        _userManager = userManager;
        _db = db;
    }

    public IActionResult Index() => View();

    [AllowAnonymous]
    [HttpGet("/demo")]
    [HttpGet("Home/DemoLogin")]
    public async Task<IActionResult> DemoLogin()
    {
#if !DEBUG
   
#endif
        const string email = "demo@solobill.test";
        const string password = "Demo!12345";

        var user = await _userManager.FindByEmailAsync(email);
        if (user is null)
        {
            user = new ApplicationUser
            {
                UserName = email,
                Email = email,
                EmailConfirmed = true,
                CompanyName = "SoloBill Demo Ltd",
                Address = "123 Invoice Street, Billingtown, INV 123, Ireland",
                CompanyPhone = "+353 1 234 5678",
                BankName = "Demo Bank",
                IBAN = "IE00 DEMO 1234 5678 90",
                BIC = "DEMOBICX",
                LogoPath = "/images/demo-logo.png"
            };

            var create = await _userManager.CreateAsync(user, password);
            if (!create.Succeeded)
                return BadRequest(string.Join("; ", create.Errors.Select(e => e.Description)));
        }

        // Seed only if this user has no data yet
        bool hasAnyData = await _db.Clients.AnyAsync(c => c.UserId == user.Id);
        if (!hasAnyData)
        {
            await SeedDemoDataAsync(user);
        }

        await _signInManager.SignInAsync(user, isPersistent: false);
        return RedirectToAction("Index", "Dashboard");
    }

    private async Task SeedDemoDataAsync(ApplicationUser user)
    {
        // 1) Clients
        var clients = new List<Client>
        {
            new Client { Name = "Sarah Thompson", Company = "Thompson Digital", Email = "sarah@thompsondigital.ie", Address = "25 Tech Park, Dublin 2, Ireland", UserId = user.Id },
            new Client { Name = "João Pereira", Company = "Luz Marketing", Email = "joao@luzmarketing.pt", Address = "Rua da Luz, 18, Lisbon, Portugal", UserId = user.Id },
            new Client { Name = "Anita Kapoor", Company = "Kapoor & Co", Email = "anita@kapoorco.in", Address = "Bandra West, Mumbai, India", UserId = user.Id }
        };
        _db.Clients.AddRange(clients);
        await _db.SaveChangesAsync();

        async Task AddInvoiceAsync(Client client, DateTime issue, int dueInDays, bool isPaid, (string desc, int qty, decimal rate)[] items, string? notes = null)
        {
            var invoice = new Invoice
            {
                ClientId = client.ClientId,
                IssueDate = issue,
                DueDate = issue.AddDays(dueInDays),
                IsPaid = isPaid,
                Notes = notes?.Trim(),
                UserId = user.Id
            };

            _db.Invoices.Add(invoice);
            await _db.SaveChangesAsync(); // get InvoiceId

            // Items
            var toAdd = items.Select(i => new InvoiceItem
            {
                InvoiceId = invoice.InvoiceId,
                Description = i.desc,
                Quantity = i.qty,
                UnitPrice = i.rate
            }).ToList();

            _db.InvoiceItems.AddRange(toAdd);
            await _db.SaveChangesAsync();

            // Amount & Number
            invoice.Amount = toAdd.Sum(x => x.Quantity * x.UnitPrice);
            invoice.InvoiceNumber = $"INV{invoice.InvoiceId:D3}";
            _db.Invoices.Update(invoice);
            await _db.SaveChangesAsync();
        }

        // 2) A few nice-looking demo invoices
        await AddInvoiceAsync(
            client: clients[0],
            issue: DateTime.Today.AddDays(-14),
            dueInDays: 14,
            isPaid: false,
            items: new[] {
                ("Website redesign sprint", 1, 1200.00m),
                ("Landing page copywriting", 1, 250.00m),
                ("Performance tuning (hours)", 6, 60.00m)
            },
            notes: "Payment due within 14 days. Thank you!"
        );

        await AddInvoiceAsync(
            client: clients[1],
            issue: DateTime.Today.AddDays(-35),
            dueInDays: 30,
            isPaid: true,
            items: new[] {
                ("Brand kit & logo", 1, 700.00m),
                ("Social media templates (pack)", 1, 180.00m)
            },
            notes: "Paid via bank transfer."
        );

        await AddInvoiceAsync(
            client: clients[2],
            issue: DateTime.Today.AddDays(-5),
            dueInDays: 21,
            isPaid: false,
            items: new[] {
                ("Monthly retainer", 1, 500.00m),
                ("Feature dev (hours)", 8, 55.00m)
            }
        );
    }

    public IActionResult Privacy() => View();

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error() =>
        View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
}