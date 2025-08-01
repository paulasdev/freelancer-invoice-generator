using SoloBill.Models;
using System.Collections.Generic;

namespace SoloBill.ViewModels
{
    public class InvoiceViewModel
    {
        public Invoice Invoice { get; set; } = new Invoice();
        public List<InvoiceItem> Items { get; set; } = new List<InvoiceItem>();

        public Client? Client { get; set; }

        public string CompanyName { get; set; } = "";
        public string? Address { get; set; }
        public string? CompanyPhone { get; set; }
        public string? Email { get; set; }
        public string? LogoPath { get; set; }

        public string? BankName { get; set; }
        public string? IBAN { get; set; }
        public string? BIC { get; set; }
        public string? Notes { get; set; }
    }
}