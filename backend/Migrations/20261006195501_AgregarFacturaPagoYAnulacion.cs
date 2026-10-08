using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Ssal.Api.Migrations
{
    /// <inheritdoc />
    public partial class AgregarFacturaPagoYAnulacion : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_facturas_id_sse",
                table: "facturas");

            migrationBuilder.AddColumn<DateTime>(
                name: "fecha_pago",
                table: "facturas",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "motivo_anulacion",
                table: "facturas",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_facturas_id_sse",
                table: "facturas",
                column: "id_sse",
                unique: true,
                filter: "estado <> 'anulada'");

            migrationBuilder.CreateIndex(
                name: "IX_facturas_numero",
                table: "facturas",
                column: "numero",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_facturas_id_sse",
                table: "facturas");

            migrationBuilder.DropIndex(
                name: "IX_facturas_numero",
                table: "facturas");

            migrationBuilder.DropColumn(
                name: "fecha_pago",
                table: "facturas");

            migrationBuilder.DropColumn(
                name: "motivo_anulacion",
                table: "facturas");

            migrationBuilder.CreateIndex(
                name: "IX_facturas_id_sse",
                table: "facturas",
                column: "id_sse",
                unique: true);
        }
    }
}
