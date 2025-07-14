using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using SoloBill.Data;
using SoloBill.Models;
using SoloBill.ViewModels;

[Authorize]
public class DashboardController : Controller
{
    private readonly SoloBillDbContext _context;

    public DashboardController(SoloBillDbContext context)
    {
        _context = context;
    }

    public IActionResult Index()
{
    var invoices = _context.Invoices
        .Include(i => i.Client)
        .ToList(); 

    var model = new DashboardViewModel
    {
        TotalClients = _context.Clients.Count(),
        InvoicesSent = invoices.Count(),
        UnpaidInvoices = invoices.Count(i => i.Status != "Paid"),
        RecentInvoices = invoices
            .OrderByDescending(i => i.CreatedAt)
            .Take(7)
            .ToList()
    };

    return View(model);
}
}