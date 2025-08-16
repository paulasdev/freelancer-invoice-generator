using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Identity;
using SoloBill.Models;

namespace SoloBill.Models
{
public class Invoice
{
    public string? InvoiceNumber { get; set; } = "";
    public int InvoiceId { get; set; }
    public int ClientId { get; set; }
    public Client? Client { get; set; }
    public DateTime IssueDate { get; set; }
    public DateTime DueDate { get; set; }
    public decimal Amount { get; set; }
    public bool IsPaid { get; set; }
    public string? Notes { get; set; }

    // Reference to the logged-in user
    public string? UserId { get; set; }
    public ApplicationUser? User { get; set; }

    public ICollection<InvoiceItem> Items { get; set; } = new List<InvoiceItem>();

    [NotMapped]
    public string Status => IsPaid ? "Paid" : "Unpaid";

    [NotMapped]
    public DateTime CreatedAt => IssueDate;

    [NotMapped]
    public string ClientName => Client?.Name ?? "Unknown";

    [NotMapped]
    public string CompanyName => Client?.Company ?? "N/A";
}
}