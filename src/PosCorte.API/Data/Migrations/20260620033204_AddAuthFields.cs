using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PosCorte.API.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddAuthFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "Ativo",
                table: "usuarios",
                type: "boolean",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AddColumn<string>(
                name: "Role",
                table: "usuarios",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "Arquiteto");

            migrationBuilder.AddColumn<string>(
                name: "SenhaHash",
                table: "usuarios",
                type: "character varying(255)",
                maxLength: 255,
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Ativo",
                table: "usuarios");

            migrationBuilder.DropColumn(
                name: "Role",
                table: "usuarios");

            migrationBuilder.DropColumn(
                name: "SenhaHash",
                table: "usuarios");
        }
    }
}
