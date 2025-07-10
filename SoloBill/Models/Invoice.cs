public class Invoice
{
    public int InvoiceId { get; set; }
    public int ClientId { get; set; }
    public Client Client { get; set; }  = null!;
    public DateTime IssueDate { get; set; }
    public DateTime DueDate { get; set; }
    public decimal Amount { get; set; }
    public bool IsPaid { get; set; }
}