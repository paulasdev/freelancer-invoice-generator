using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace SoloBill.Models
{
    public class ApplicationUser : IdentityUser
    {
        [Required]
        public string CompanyName { get; set; } = string.Empty;
        public string? Address { get; set; }
        public string? CompanyPhone { get; set; }
        public string? LogoPath { get; set; }
        public string? BankDetails { get; set; }
        public string? PaymentMethod { get; set; }


        [MaxLength(100)]
        public string? BankName { get; set; }

        [MaxLength(34)]
        public string? IBAN { get; set; }

        [MaxLength(11)]
        public string? BIC { get; set; }

        public ICollection<Client> Clients { get; set; } = new List<Client>();
        public ICollection<Invoice> Invoices { get; set; } = new List<Invoice>();

    }
}