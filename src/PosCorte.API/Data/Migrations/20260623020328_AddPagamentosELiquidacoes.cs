using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace PosCorte.API.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddPagamentosELiquidacoes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "pagamentos",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityAlwaysColumn),
                    ProjetoId = table.Column<int>(type: "integer", nullable: false),
                    Modo = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    AsaasPaymentId = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    AsaasCustomerId = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    ValorTotal = table.Column<decimal>(type: "numeric(12,2)", nullable: false),
                    ValorMarceneiro = table.Column<decimal>(type: "numeric(12,2)", nullable: false),
                    ValorPlataforma = table.Column<decimal>(type: "numeric(12,2)", nullable: false),
                    PixCopiaECola = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    QrCodeBase64 = table.Column<string>(type: "text", nullable: true),
                    InvoiceUrl = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    ExpiraEm = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DataCriacao = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()"),
                    DataConfirmacao = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_pagamentos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_pagamentos_projetos_ProjetoId",
                        column: x => x.ProjetoId,
                        principalTable: "projetos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "liquidacoes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityAlwaysColumn),
                    PagamentoId = table.Column<int>(type: "integer", nullable: false),
                    ProjetoId = table.Column<int>(type: "integer", nullable: false),
                    ValorMarceneiro = table.Column<decimal>(type: "numeric(12,2)", nullable: false),
                    ValorPlataforma = table.Column<decimal>(type: "numeric(12,2)", nullable: false),
                    Status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    AsaasSplitId = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    DataCriacao = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()"),
                    DataConclusao = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_liquidacoes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_liquidacoes_pagamentos_PagamentoId",
                        column: x => x.PagamentoId,
                        principalTable: "pagamentos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_liquidacoes_PagamentoId",
                table: "liquidacoes",
                column: "PagamentoId");

            migrationBuilder.CreateIndex(
                name: "IX_pagamentos_AsaasPaymentId",
                table: "pagamentos",
                column: "AsaasPaymentId");

            migrationBuilder.CreateIndex(
                name: "IX_pagamentos_ProjetoId",
                table: "pagamentos",
                column: "ProjetoId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "liquidacoes");

            migrationBuilder.DropTable(
                name: "pagamentos");
        }
    }
}
