using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PosCorte.API.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddVistoriaProjeto : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "DataLimiteVistoria",
                table: "projetos",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "MotivoDisputa",
                table: "projetos",
                type: "character varying(1000)",
                maxLength: 1000,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DataLimiteVistoria",
                table: "projetos");

            migrationBuilder.DropColumn(
                name: "MotivoDisputa",
                table: "projetos");
        }
    }
}
