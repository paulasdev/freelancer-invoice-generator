﻿using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SoloBill.Migrations
{
    /// <inheritdoc />
    public partial class AddPaymentMethodToApplicationUser : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "PaymentMethod",
                table: "AspNetUsers",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PaymentMethod",
                table: "AspNetUsers");
        }
    }
}
