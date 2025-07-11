using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SoloBill.Migrations
{
    /// <inheritdoc />
    public partial class AddClientLogoField : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "LogoFileName",
                table: "Clients",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.UpdateData(
                table: "Clients",
                keyColumn: "ClientId",
                keyValue: 1,
                column: "LogoFileName",
                value: null);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LogoFileName",
                table: "Clients");
        }
    }
}
