using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Ssal.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AgregarDetalleNormativoAnalisis : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "fecha_resultado",
                table: "resultados",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "criterio_aceptacion",
                table: "analisis",
                type: "character varying(1500)",
                maxLength: 1500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "limite_deteccion",
                table: "analisis",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "referencia_normativa",
                table: "analisis",
                type: "character varying(1500)",
                maxLength: 1500,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "fecha_resultado",
                table: "resultados");

            migrationBuilder.DropColumn(
                name: "criterio_aceptacion",
                table: "analisis");

            migrationBuilder.DropColumn(
                name: "limite_deteccion",
                table: "analisis");

            migrationBuilder.DropColumn(
                name: "referencia_normativa",
                table: "analisis");
        }
    }
}
