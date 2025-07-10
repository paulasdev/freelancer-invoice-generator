using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SoloBill.Migrations
{
    /// <inheritdoc />
    public partial class SeedClientAndInvoice : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Clients",
                columns: new[] { "ClientId", "Address", "Company", "Email", "Name" },
                values: new object[] { 1, "123 Main Street", "ACME Inc.", "info@acme.com", "ACME Inc." });

            migrationBuilder.InsertData(
                table: "Invoices",
                columns: new[] { "InvoiceId", "Amount", "ClientId", "DueDate", "IsPaid", "IssueDate" },
                values: new object[] { 1, 500.00m, 1, new DateTime(2025, 7, 31, 0, 0, 0, 0, DateTimeKind.Unspecified), false, new DateTime(2025, 7, 1, 0, 0, 0, 0, DateTimeKind.Unspecified) });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Invoices",
                keyColumn: "InvoiceId",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Clients",
                keyColumn: "ClientId",
                keyValue: 1);
        }
    }
}
