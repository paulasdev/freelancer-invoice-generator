using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Identity;
using System.Collections.Generic;
using SoloBill.Models;

namespace SoloBill.Models
{
public class Client
{
    public int ClientId { get; set; }
    public string Name { get; set; } = "";
    public string Email { get; set; } = "";
    public string Company { get; set; } = "";
    public string Address { get; set; } = "";

    // Reference to the logged-in user
    public string? UserId { get; set; } = "";
    public ApplicationUser? User { get; set; }

    public ICollection<Invoice> Invoices { get; set; } = new List<Invoice>();
}
}