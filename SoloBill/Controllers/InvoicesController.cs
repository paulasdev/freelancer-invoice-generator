using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SoloBill.Data;
using SoloBill.Services;
using SoloBill.ViewModels;
using Microsoft.AspNetCore.Identity;
using SoloBill.Models;

namespace SoloBill.Controllers
{
    public class InvoicesController : Controller
    {
        private readonly SoloBillDbContext _context;
        private readonly InvoicePdfService _pdfService;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IWebHostEnvironment _env;

        public InvoicesController(SoloBillDbContext context, InvoicePdfService pdfService, UserManager<ApplicationUser> userManager, IWebHostEnvironment env)
        {
            _context = context;
            _pdfService = pdfService;
            _userManager = userManager;
            _env = env;
        }

        // GET: Invoices
        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Unauthorized();

            var invoices = await _context.Invoices
                .Include(i => i.Client)
                .Where(i => i.UserId == user.Id)
                .ToListAsync();

            return View(invoices);
        }

        // GET: Invoices/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
                return NotFound();

            var user = await _userManager.GetUserAsync(User);
            var invoice = await _context.Invoices
                .Include(i => i.Client)
                .FirstOrDefaultAsync(i => i.InvoiceId == id && i.UserId == user.Id);

            if (invoice == null)
                return NotFound();

            return View(invoice);
        }

        // GET: Invoices/Create
        [HttpGet]
        public async Task<IActionResult> Create(int? id)
        {
            var user = await _userManager.GetUserAsync(User);
            Invoice invoice = null;
            List<InvoiceItem> items = new() { new InvoiceItem() };

            if (id.HasValue)
            {
                invoice = await _context.Invoices.Include(i => i.Client).FirstOrDefaultAsync(i => i.InvoiceId == id);
                items = await _context.InvoiceItems.Where(ii => ii.InvoiceId == id).ToListAsync();
            }
            else
            {
                invoice = new Invoice
                {
                    IssueDate = DateTime.Today,
                    DueDate = DateTime.Today.AddDays(7)
                };
            }

            var viewModel = new InvoiceViewModel
            {
                Invoice = invoice,
                Client = invoice?.Client,
                Items = items,
                CompanyName = user.CompanyName,
                Address = user.Address,
                CompanyPhone = user.CompanyPhone,
                Email = user.Email,
                LogoPath = user.LogoPath,
                BankName = user.BankName,
                IBAN = user.IBAN,
                BIC = user.BIC,
                Notes = invoice.Notes
            };

            ViewData["ClientId"] = new SelectList(_context.Clients.Where(c => c.UserId == user.Id), "ClientId", "Name");
            return View(viewModel);
        }

        // GET: Invoices/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
                return NotFound();

            var user = await _userManager.GetUserAsync(User);
            var invoice = await _context.Invoices
                .Include(i => i.Client)
                .FirstOrDefaultAsync(i => i.InvoiceId == id && i.UserId == user.Id);

            if (invoice == null)
                return NotFound();

            ViewData["ClientId"] = new SelectList(
                _context.Clients.Where(c => c.UserId == user.Id), "ClientId", "Name", invoice.ClientId);

            return View(invoice);
        }


        // GET: Invoices/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
                return NotFound();

            var user = await _userManager.GetUserAsync(User);
            var invoice = await _context.Invoices
                .Include(i => i.Client)
                .FirstOrDefaultAsync(i => i.InvoiceId == id && i.UserId == user.Id);

            if (invoice == null)
                return NotFound();

            return View(invoice);
        }


        // POST: Invoices/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        // POST: Invoices/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(InvoiceViewModel viewModel)
        {
            var user = await _userManager.GetUserAsync(User);
            viewModel.Invoice.UserId = user.Id;

            if (!ModelState.IsValid)
            {
                foreach (var kvp in ModelState)
                {
                    foreach (var error in kvp.Value.Errors)
                    {
                        Console.WriteLine($"Model error in '{kvp.Key}': {error.ErrorMessage}");
                    }
                }

                // Reload form data on error
                ViewData["ClientId"] = new SelectList(_context.Clients.Where(c => c.UserId == user.Id), "ClientId", "Name", viewModel.Invoice.ClientId);
                viewModel.Client = await _context.Clients.FindAsync(viewModel.Invoice.ClientId);
                viewModel.CompanyName = user.CompanyName;
                viewModel.Address = user.Address;
                viewModel.CompanyPhone = user.CompanyPhone;
                viewModel.Email = user.Email;
                viewModel.LogoPath = user.LogoPath;
                viewModel.BankName = user.BankName;
                viewModel.IBAN = user.IBAN;
                viewModel.BIC = user.BIC;
                viewModel.Notes = user.BankDetails;

                return View(viewModel);
            }

            // 1. Calculate invoice total
            viewModel.Invoice.Amount = viewModel.Items.Sum(i => i.Quantity * i.UnitPrice);

            // 2. Add invoice (without InvoiceNumber yet)
            _context.Invoices.Add(viewModel.Invoice);
            await _context.SaveChangesAsync(); // now InvoiceId is set

            // 3. Generate invoice number
            viewModel.Invoice.InvoiceNumber = $"INV{DateTime.Today:yyyyMMdd}{viewModel.Invoice.InvoiceId:D2}";

            // 4. Update invoice
            _context.Invoices.Update(viewModel.Invoice);

            // 5. Set InvoiceId for items and add
            foreach (var item in viewModel.Items)
            {
                item.InvoiceId = viewModel.Invoice.InvoiceId;
            }
            viewModel.Invoice.UserId = user.Id;
            _context.InvoiceItems.AddRange(viewModel.Items);


            // 6. Save all
            await _context.SaveChangesAsync();

            return RedirectToAction("Create", new { id = viewModel.Invoice.InvoiceId });
        }


        // POST: Invoices/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("InvoiceId,ClientId,IssueDate,DueDate,Amount,IsPaid")] Invoice updatedInvoice)
        {
            var user = await _userManager.GetUserAsync(User);

            if (id != updatedInvoice.InvoiceId)
                return NotFound();

            // 🔐 Fetch invoice and validate ownership
            var invoice = await _context.Invoices
                .FirstOrDefaultAsync(i => i.InvoiceId == id && i.UserId == user.Id);

            if (invoice == null)
                return Unauthorized(); // or return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    // Update fields securely
                    invoice.ClientId = updatedInvoice.ClientId;
                    invoice.IssueDate = updatedInvoice.IssueDate;
                    invoice.DueDate = updatedInvoice.DueDate;
                    invoice.Amount = updatedInvoice.Amount;
                    invoice.IsPaid = updatedInvoice.IsPaid;

                    _context.Update(invoice);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!InvoiceExists(updatedInvoice.InvoiceId))
                        return NotFound();
                    else
                        throw;
                }

                return RedirectToAction(nameof(Index));
            }

            ViewData["ClientId"] = new SelectList(
                _context.Clients.Where(c => c.UserId == user.Id),
                "ClientId", "Name", updatedInvoice.ClientId);

            return View(updatedInvoice);
        }



        // POST: Invoices/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            var invoice = await _context.Invoices
                .FirstOrDefaultAsync(i => i.InvoiceId == id && i.UserId == user.Id);

            if (invoice != null)
            {
                _context.Invoices.Remove(invoice);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }

        //PDF service
        [HttpGet]
        public async Task<IActionResult> ExportToPdf(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            var invoice = await _context.Invoices
                .Include(i => i.Client)
                .Include(i => i.Items)
                .FirstOrDefaultAsync(i => i.InvoiceId == id && i.UserId == user.Id);

            if (invoice == null)
                return NotFound();

            var viewModel = new InvoiceViewModel
            {
                Invoice = invoice,
                Client = invoice.Client,
                Items = invoice.Items.ToList(),
                CompanyName = user.CompanyName,
                Address = user.Address,
                CompanyPhone = user.CompanyPhone,
                Email = user.Email,
                BankName = user.BankName,
                IBAN = user.IBAN,
                BIC = user.BIC,
                Notes = invoice.Notes,
                LogoPath = Path.Combine(_env.WebRootPath, "uploads", user.LogoPath ?? "")
            };

            var pdfBytes = _pdfService.GeneratePdf(viewModel);
            return File(pdfBytes, "application/pdf", $"Invoice_{invoice.InvoiceNumber}.pdf");
        }


        private bool InvoiceExists(int id)
        {
            return _context.Invoices.Any(e => e.InvoiceId == id);
        }

    }
}