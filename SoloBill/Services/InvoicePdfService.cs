using System.IO;
using System.Text;
using SoloBill.Data;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace SoloBill.Services
{
    public class InvoicePdfService
    {
    
        public byte[] GeneratePdf(Invoice invoice)
        {
            // QuestPDF license 
            QuestPDF.Settings.License = QuestPDF.Infrastructure.LicenseType.Community;

            var pdf = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Margin(50);
                    page.Content()
                        .Column(col =>
                        {
                            col.Item().Text($"Invoice #{invoice.InvoiceId}").FontSize(20).Bold();
                            col.Item().Text($"Client: {invoice.Client?.Name}");
                            col.Item().Text($"Amount: {invoice.Amount:C}");
                            col.Item().Text($"Issue Date: {invoice.IssueDate:dd/MM/yyyy}");
                            col.Item().Text($"Due Date: {invoice.DueDate:dd/MM/yyyy}");
                            col.Item().Text($"Paid: {(invoice.IsPaid ? "Yes" : "No")}");
                        });
                });
            });

            return pdf.GeneratePdf();
        }
    }
}