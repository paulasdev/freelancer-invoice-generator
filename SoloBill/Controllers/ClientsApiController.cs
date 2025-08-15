using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SoloBill.Data;
using SoloBill.Models;


namespace SoloBill.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ClientsApiController : ControllerBase
    {
        private readonly SoloBillDbContext _context;

        public ClientsApiController(SoloBillDbContext context)
        {
            _context = context;
        }

        // GET: api/Clients
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Client>>> GetClients()
        {
            return await _context.Clients.ToListAsync();
        }

        // GET: api/Clients/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Client>> GetClientByid(int id)
        {
            var client = await _context.Clients.FindAsync(id);

            if (client == null)
            {
                return NotFound();
            }

            return client;
        }
    }
}