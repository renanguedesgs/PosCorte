using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace PosCorte.API.Data.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "usuarios",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityAlwaysColumn),
                    Nome = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Email = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    CpfCnpj = table.Column<string>(type: "character varying(18)", maxLength: 18, nullable: false),
                    Telefone = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    DataCadastro = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_usuarios", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "projetos",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityAlwaysColumn),
                    UsuarioId = table.Column<int>(type: "integer", nullable: false),
                    NomeProjeto = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    UrlArquivoCorteCloud = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    QtdPecas = table.Column<int>(type: "integer", nullable: false),
                    QtdGavetas = table.Column<int>(type: "integer", nullable: false),
                    CepObra = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    EnderecoCompleto = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    StatusProjeto = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    DataCriacao = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_projetos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_projetos_usuarios_UsuarioId",
                        column: x => x.UsuarioId,
                        principalTable: "usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ordens_servico",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityAlwaysColumn),
                    ProjetoId = table.Column<int>(type: "integer", nullable: false),
                    ExternalProviderId = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    StatusProvedor = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    MontadorNome = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    MontadorTelefone = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    MontadorFotoUrl = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    DataAgendamento = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    DataAtualizacao = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ordens_servico", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ordens_servico_projetos_ProjetoId",
                        column: x => x.ProjetoId,
                        principalTable: "projetos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ordens_servico_ProjetoId",
                table: "ordens_servico",
                column: "ProjetoId");

            migrationBuilder.CreateIndex(
                name: "IX_projetos_UsuarioId",
                table: "projetos",
                column: "UsuarioId");

            migrationBuilder.CreateIndex(
                name: "IX_usuarios_CpfCnpj",
                table: "usuarios",
                column: "CpfCnpj",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_usuarios_Email",
                table: "usuarios",
                column: "Email",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ordens_servico");

            migrationBuilder.DropTable(
                name: "projetos");

            migrationBuilder.DropTable(
                name: "usuarios");
        }
    }
}
