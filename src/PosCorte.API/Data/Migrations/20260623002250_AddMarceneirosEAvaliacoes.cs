using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace PosCorte.API.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddMarceneirosEAvaliacoes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "marceneiros",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityAlwaysColumn),
                    Nome = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Email = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Telefone = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    FotoUrl = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    Cidade = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    Estado = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: false),
                    Bairro = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    Cep = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    Especialidades = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    Bio = table.Column<string>(type: "character varying(600)", maxLength: 600, nullable: false),
                    NotaMedia = table.Column<decimal>(type: "numeric(3,2)", nullable: false),
                    TotalAvaliacoes = table.Column<int>(type: "integer", nullable: false),
                    TotalServicos = table.Column<int>(type: "integer", nullable: false),
                    Disponivel = table.Column<bool>(type: "boolean", nullable: false),
                    Verificado = table.Column<bool>(type: "boolean", nullable: false),
                    OrigemExterna = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    DataCadastro = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_marceneiros", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "avaliacoes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityAlwaysColumn),
                    MarceneiroId = table.Column<int>(type: "integer", nullable: false),
                    ProjetoId = table.Column<int>(type: "integer", nullable: true),
                    AutorNome = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Nota = table.Column<int>(type: "integer", nullable: false),
                    Comentario = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    DataCriacao = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_avaliacoes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_avaliacoes_marceneiros_MarceneiroId",
                        column: x => x.MarceneiroId,
                        principalTable: "marceneiros",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_avaliacoes_MarceneiroId",
                table: "avaliacoes",
                column: "MarceneiroId");

            migrationBuilder.CreateIndex(
                name: "IX_marceneiros_OrigemExterna",
                table: "marceneiros",
                column: "OrigemExterna");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "avaliacoes");

            migrationBuilder.DropTable(
                name: "marceneiros");
        }
    }
}
