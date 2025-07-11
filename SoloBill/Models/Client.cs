using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

public class Client
{
    public int ClientId { get; set; }
    public string Name { get; set; } = "";
    public string Email { get; set; } = "";
    public string Company { get; set; } = "";
    public string Address { get; set; } = "";
    public ICollection<Invoice> Invoices { get; set; } = new List<Invoice>();
    [NotMapped]
    public IFormFile? LogoFile { get; set; }
    public string? LogoFileName { get; set; }
}