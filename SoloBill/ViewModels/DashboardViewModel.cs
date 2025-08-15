using SoloBill.Models;
using System.Collections.Generic;

namespace SoloBill.ViewModels
{
    public class DashboardViewModel
    {
        public int TotalClients { get; set; }
        public int InvoicesSent { get; set; }
        public int UnpaidInvoices { get; set; }
        public List<Invoice> RecentInvoices { get; set; } = new();
    }
}