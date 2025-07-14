using System.ComponentModel.DataAnnotations.Schema;

public class Invoice
{
    public int InvoiceId { get; set; }
    public int ClientId { get; set; }
    public Client Client { get; set; }  = null!;
    public DateTime IssueDate { get; set; }
    public DateTime DueDate { get; set; }
    public decimal Amount { get; set; }
    public bool IsPaid { get; set; }

    [NotMapped]
    public string Status => IsPaid ? "Paid" : "Unpaid";

    [NotMapped]
    public DateTime CreatedAt => IssueDate;

    [NotMapped]
    public string InvoiceNumber => $"INV-{InvoiceId:D5}";

    [NotMapped]
    public string ClientName => Client?.Name ?? "Unknown";
}