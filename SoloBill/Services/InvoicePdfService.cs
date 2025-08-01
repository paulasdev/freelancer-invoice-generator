using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using SoloBill.ViewModels;
using Microsoft.AspNetCore.Hosting;
using System.IO;

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

            var document = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Margin(30);
                    page.Size(PageSizes.A4);
                    page.DefaultTextStyle(x => x.FontSize(12).FontFamily("Inter"));

                    // Header
                    page.Header().Row(row =>
                    {
                        row.RelativeItem().Column(col =>
                        {
                            col.Item().Text("INVOICE").FontSize(20).Bold();
                            col.Item().Text($"#{model.Invoice.InvoiceNumber}").FontSize(12).FontColor(Colors.Grey.Darken1);
                        });

                        if (!string.IsNullOrEmpty(model.LogoPath) && File.Exists(model.LogoPath))
                        {
                            row.ConstantItem(100).Height(50).Image(model.LogoPath);
                        }
                    });

                    // Content
                    page.Content().PaddingVertical(20).Column(col =>
                    {
                        col.Spacing(20);

                        // Bill From & To
                        col.Item().Row(row =>
                        {
                            row.RelativeItem().Column(inner =>
                            {
                                inner.Spacing(3);
                                inner.Item().Text("Bill From").Bold();
                                inner.Item().Text(model.CompanyName ?? "");
                                inner.Item().Text(model.Address ?? "");
                                inner.Item().Text(model.CompanyPhone ?? "");
                                inner.Item().Text(model.Email ?? "");
                            });

                            row.RelativeItem().Column(inner =>
                            {
                                inner.Spacing(3);
                                inner.Item().Text("Bill To").Bold();
                                inner.Item().Text(model.Client?.Name ?? "");
                                inner.Item().Text(model.Client?.Company ?? "");
                                inner.Item().Text(model.Client?.Address ?? "");
                                inner.Item().Text(model.Client?.Email ?? "");
                            });
                        });

                        col.Item().LineHorizontal(1);

                        // Items Table
                        col.Item().Table(table =>
                        {
                            table.ColumnsDefinition(cols =>
                            {
                                cols.RelativeColumn(3);
                                cols.RelativeColumn(1);
                                cols.RelativeColumn(1);
                                cols.RelativeColumn(1);
                            });

                            table.Header(header =>
                            {
                                header.Cell().Element(container => container.Background(Colors.Grey.Lighten3).PaddingVertical(5).PaddingLeft(4))
                                    .Text("Description").Bold();

                                header.Cell().Element(container => container.Background(Colors.Grey.Lighten3).PaddingVertical(5).PaddingLeft(4))
                                    .AlignRight().Text("Qty").Bold();

                                header.Cell().Element(container => container.Background(Colors.Grey.Lighten3).PaddingVertical(5).PaddingLeft(4))
                                    .AlignRight().Text("Rate").Bold();

                                header.Cell().Element(container => container.Background(Colors.Grey.Lighten3).PaddingVertical(5).PaddingLeft(4))
                                    .AlignRight().Text("Total").Bold();
                            });

                            foreach (var item in model.Items)
                            {
                                table.Cell().Element(CellStyle).Text(item.Description ?? "");
                                table.Cell().Element(CellStyle).AlignRight().Text(item.Quantity.ToString());
                                table.Cell().Element(CellStyle).AlignRight().Text($"€{item.UnitPrice:N2}");
                                table.Cell().Element(CellStyle).AlignRight().Text($"€{(item.Quantity * item.UnitPrice):N2}");
                            }
                        });

                        // Total
                        col.Item().AlignRight().Text($"Total: €{model.Items.Sum(i => i.Quantity * i.UnitPrice):N2}").FontSize(14).Bold();

                        col.Item().LineHorizontal(1);
                    });

                    // Footer (Bank info + Notes)
                    page.Footer().Column(footer =>
                    {
                        footer.Spacing(5);

                        footer.Item().Row(row =>
                        {
                            row.RelativeItem().Column(colLeft =>
                            {
                                colLeft.Spacing(2);
                                colLeft.Item().Text("Payment Method").Bold();
                                colLeft.Item().Text($"Bank: {model.BankName}");
                                colLeft.Item().Text($"IBAN: {model.IBAN}");
                                colLeft.Item().Text($"BIC: {model.BIC}");
                            });

                            if (!string.IsNullOrEmpty(model.Notes))
                            {
                                row.RelativeItem().Column(colRight =>
                                {
                                    colRight.Spacing(2);
                                    colRight.Item().Text("Note").Bold();
                                    colRight.Item().Text(model.Notes);
                                });
                            }
                        });

                        footer.Item().PaddingTop(10)
                            .Text("Invoice was created on a computer and is valid without signature and seal.")
                            .FontSize(10).Italic().FontColor(Colors.Grey.Darken2);
                    });
                });
            });

            return document.GeneratePdf();
        }

        private static IContainer CellStyle(IContainer container)
        {
            return container.PaddingVertical(4).PaddingHorizontal(2);
        }
    }
}