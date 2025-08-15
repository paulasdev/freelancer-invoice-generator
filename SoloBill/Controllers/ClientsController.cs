using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SoloBill.Data;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using SoloBill.Models;

namespace SoloBill.Controllers
{
    public class ClientsController : Controller
    {
        private readonly SoloBillDbContext _context;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly UserManager<ApplicationUser> _userManager;

        public ClientsController(
            SoloBillDbContext context,
            IWebHostEnvironment webHostEnvironment,
            UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _webHostEnvironment = webHostEnvironment;
            _userManager = userManager;
        }

        // GET: Clients
        public async Task<IActionResult> Index()
        {
            var userId = _userManager.GetUserId(User);

            var userClients = await _context.Clients
                .Where(c => c.UserId == userId)
                .Include(c => c.Invoices.Where(i => i.UserId == userId))
                .AsNoTracking()
                .ToListAsync();

            return View(userClients);
        }

        // GET: Clients/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var userId = _userManager.GetUserId(User);

            var client = await _context.Clients
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.ClientId == id && c.UserId == userId);

            if (client == null) return NotFound();

            return View(client);
        }

        // GET: Clients/Create
        public IActionResult Create() => View();

        // POST: Clients/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("ClientId,Name,Email,Company,Address")] Client client)
        {
            var userId = _userManager.GetUserId(User);
            client.UserId = userId;

            if (!ModelState.IsValid) return View(client);

            _context.Add(client);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // GET: Clients/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var userId = _userManager.GetUserId(User);

            var client = await _context.Clients
                .FirstOrDefaultAsync(c => c.ClientId == id && c.UserId == userId);

            if (client == null) return NotFound();

            return View(client);
        }

        // POST: Clients/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("ClientId,Name,Email,Company,Address")] Client client)
        {
            if (id != client.ClientId) return NotFound();

            client.UserId = _userManager.GetUserId(User);

            if (!ModelState.IsValid) return View(client);

            try
            {
                _context.Update(client);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ClientExists(client.ClientId)) return NotFound();
                throw;
            }

            return RedirectToAction(nameof(Index));
        }

        // GET: Clients/Delete/5  (keep this one; remove the duplicate)
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var userId = _userManager.GetUserId(User);

            var client = await _context.Clients
                .AsNoTracking()
                .FirstOrDefaultAsync(m => m.ClientId == id && m.UserId == userId);

            if (client == null) return NotFound();

            return View(client);
        }

        // POST: Clients/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var userId = _userManager.GetUserId(User);

            var client = await _context.Clients
                .FirstOrDefaultAsync(c => c.ClientId == id && c.UserId == userId);

            if (client != null)
            {
                _context.Clients.Remove(client);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }

        private bool ClientExists(int id) =>
            _context.Clients.Any(e => e.ClientId == id);
    }
}