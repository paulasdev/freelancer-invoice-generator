using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using SoloBill.ViewModels;
using Microsoft.AspNetCore.Hosting;
using System.IO;
using System.Linq;

namespace SoloBill.Services
{
    public class InvoicePdfService
    {
        private readonly IWebHostEnvironment _env;

        public InvoicePdfService(IWebHostEnvironment env)
        {
            _env = env;
        }

        public byte[] GeneratePdf(InvoiceViewModel model)
        {
            QuestPDF.Settings.License = LicenseType.Community;

            var brand = "#5D5FEF";
            var panel = "#F3F4F8";
            var textDark = "#2D2D2D";
            var totalAmount = model.Items.Sum(i => i.Quantity * i.UnitPrice);

            var document = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Margin(28);
                    page.Size(PageSizes.A4);
                    page.DefaultTextStyle(x => x.FontFamily("Inter").FontSize(11).FontColor(textDark));

                    page.Content().Column(col =>
                    {
                        col.Spacing(14);

                        // =========================
                        // Header: left banner + right logo/invoice info
                        // =========================
                        col.Item().Row(row =>
                        {
                            // Left rounded banner with INVOICE
                            row.RelativeItem().Element(e =>
              e.Background(panel)
               .Padding(24)
               .Height(90)
               .AlignMiddle()
               .AlignLeft()
               .Text("INVOICE").FontSize(24).SemiBold());

                            // Right: logo + Invoice No + Date
                            row.RelativeItem().Column(right =>
                            {
                                right.Spacing(8);

                                // Logo line (aligned right)
                                right.Item().AlignRight().Row(r =>
                                {
                                    r.Spacing(8);
                                    r.AutoItem().Element(c => BuildLogo(c, model, brand));
                                });

                                // Invoice No and Date
                                right.Item().AlignRight().Column(info =>
                                {
                                    info.Spacing(2);
                                    info.Item().Text($"Invoice No: #{model.Invoice.InvoiceNumber}").Bold();
                                    info.Item().Text($"Date : {model.Invoice.IssueDate:dd MMMM, yyyy}");
                                    info.Item().Text($"Due Date : {model.Invoice.DueDate:dd MMMM, yyyy}");
                                });
                            });
                        });

                        // =========================
                        // Bill From / Bill To
                        // =========================
                        col.Item().Row(row =>
                        {
                            row.RelativeItem().Column(c =>
                            {
                                c.Spacing(2);
                                c.Item().Text("Bill From :").Bold().FontColor(brand);
                                c.Item().Text(model.CompanyName ?? "");
                                c.Item().Text(model.Address ?? "");
                                if (!string.IsNullOrWhiteSpace(model.CompanyPhone))
                                    c.Item().Text(model.CompanyPhone);
                                if (!string.IsNullOrWhiteSpace(model.Email))
                                    c.Item().Text(model.Email);
                            });

                            row.RelativeItem().Column(c =>
                            {
                                c.Spacing(2);
                                c.Item().Text("Bill To :").Bold().FontColor(brand);
                                c.Item().Text(model.Client?.Name ?? "");
                                if (!string.IsNullOrWhiteSpace(model.Client?.Company))
                                    c.Item().Text(model.Client.Company);
                                c.Item().Text(model.Client?.Address ?? "");
                                if (!string.IsNullOrWhiteSpace(model.Client?.Email))
                                    c.Item().Text(model.Client.Email);
                            });
                        });

                        // =========================
                        // Items table
                        // =========================
                        col.Item().Element(e => BuildItemsTable(e, model));

                        // =========================
                        // Payment + Total band
                        // =========================
                        col.Item().Element(e => e.Background(panel).Padding(16))
                                   .Row(r =>
                                   {
                                       r.RelativeItem().Column(left =>
                                       {
                                           left.Spacing(3);
                                           left.Item().Text("Payment Method").Bold();
                                           if (!string.IsNullOrWhiteSpace(model.BankName))
                                               left.Item().Text($"Bank: {model.BankName}");
                                           if (!string.IsNullOrWhiteSpace(model.IBAN))
                                               left.Item().Text($"IBAN: {model.IBAN}");
                                           if (!string.IsNullOrWhiteSpace(model.BIC))
                                               left.Item().Text($"BIC: {model.BIC}");
                                       });

                                       r.ConstantItem(220).AlignRight().Column(right =>
                                       {
                                           right.Spacing(4);
                                           right.Item().Row(rr =>
                                           {
                                               rr.RelativeItem().AlignRight().Text("Total");
                                               rr.ConstantItem(110).AlignRight().Text($"€{totalAmount:N2}").Bold();
                                           });
                                       });
                                   });

                        // =========================
                        // Notes
                        // =========================
                        col.Item().Column(n =>
                        {
                            n.Spacing(6);
                            n.Item().Text("Notes").Bold();
                            n.Item().Text(model.Notes ?? "");
                            n.Item().PaddingTop(12).LineHorizontal(0.5f);
                        });
                    });

                    // =========================
                    // Footer sentence
                    // =========================
                    page.Footer().AlignCenter().Text("Invoice was created on a computer and is valid without signature and seal.")
                                   .FontSize(9).FontColor(Colors.Grey.Darken2);
                });
            });

            return document.GeneratePdf();
        }


        private void BuildLogo(IContainer container, InvoiceViewModel model, string brandColor)
        {

            string? logoPath = model.LogoPath;
            if (!string.IsNullOrWhiteSpace(logoPath))
            {
                if (!Path.IsPathRooted(logoPath))
                    logoPath = Path.Combine(_env.WebRootPath, "uploads", Path.GetFileName(logoPath));
            }

            if (!string.IsNullOrWhiteSpace(logoPath) && File.Exists(logoPath))
            {
                container.Width(36).Height(36).Image(logoPath).FitWidth();
                return;
            }


            var initials = GetInitials(model.CompanyName);
            container.Width(36).Height(36)
           .Background(brandColor)
           .AlignCenter().AlignMiddle()
           .Text(initials).FontSize(14).Bold().FontColor(Colors.White);
        }

        private static string GetInitials(string? name)
        {
            if (string.IsNullOrWhiteSpace(name)) return "•";
            var parts = name.Split(' ', StringSplitOptions.RemoveEmptyEntries)
                            .Take(2)
                            .Select(p => char.ToUpperInvariant(p[0]));
            return string.Concat(parts);
        }

        private static void BuildItemsTable(IContainer container, InvoiceViewModel model)
        {
            container.PaddingTop(8).Table(table =>
            {
                table.ColumnsDefinition(cols =>
                {
                    cols.RelativeColumn(3);  // description
                    cols.RelativeColumn(1);  // qty
                    cols.RelativeColumn(1);  // rate
                    cols.RelativeColumn(1);  // subtotal
                });

                // header row with bottom rule
                table.Header(header =>
                {
                    header.Cell().Text("Product Description").SemiBold();
                    header.Cell().AlignRight().Text("Qty").SemiBold();
                    header.Cell().AlignRight().Text("Rate").SemiBold();
                    header.Cell().AlignRight().Text("Subtotal").SemiBold();

                    header.Cell().ColumnSpan(4)
       .PaddingTop(4)
       .Element(c => c.Height(1).Background(Colors.Grey.Lighten1));
                });

                foreach (var item in model.Items)
                {
                    table.Cell().Element(CellBase).Text(item.Description ?? "");
                    table.Cell().Element(CellBase).AlignRight().Text(item.Quantity.ToString());
                    table.Cell().Element(CellBase).AlignRight().Text($"€{item.UnitPrice:N2}");
                    table.Cell().Element(CellBase).AlignRight().Text($"€{(item.Quantity * item.UnitPrice):N2}");
                }

                static IContainer CellBase(IContainer c) =>
                    c.PaddingVertical(6).PaddingHorizontal(2);
            });
        }
    }
}