using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using SoloBill.Data;
using SoloBill.Models;
using Microsoft.AspNetCore.Identity;
using SoloBill.ViewModels;

[Authorize]
public class DashboardController : Controller
{
    private readonly SoloBillDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;

    public DashboardController(SoloBillDbContext context, UserManager<ApplicationUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    public async Task<IActionResult> Index()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return Unauthorized();

        var invoices = await _context.Invoices
            .Include(i => i.Client)
            .Where(i => i.UserId == user.Id)
            .OrderByDescending(i => i.IssueDate)
            .ToListAsync();

        var clients = await _context.Clients
            .Where(c => c.UserId == user.Id)
            .ToListAsync();

        var unpaidCount = invoices.Count(i => !i.IsPaid);

        var model = new DashboardViewModel
        {
            TotalClients = clients.Count,
            InvoicesSent = invoices.Count,
            UnpaidInvoices = unpaidCount,
            RecentInvoices = invoices.Take(5).ToList()
        };

        return View(model);
    }
}