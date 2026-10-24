using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Ssal.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AgregarMostrarMonedaPresupuesto : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "mostrar_moneda",
                table: "presupuestos",
                type: "character varying(10)",
                maxLength: 10,
                nullable: false,
                defaultValue: "ambos");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "mostrar_moneda",
                table: "presupuestos");
        }
    }
}
