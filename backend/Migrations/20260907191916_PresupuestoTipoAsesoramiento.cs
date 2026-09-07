using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Ssal.Api.Migrations
{
    /// <inheritdoc />
    public partial class PresupuestoTipoAsesoramiento : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "tipo_asesoramiento",
                table: "presupuestos",
                type: "character varying(20)",
                maxLength: 20,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "tipo_asesoramiento",
                table: "presupuestos");
        }
    }
}
