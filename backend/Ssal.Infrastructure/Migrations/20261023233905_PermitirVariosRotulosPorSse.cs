using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Ssal.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class PermitirVariosRotulosPorSse : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_rotulos_internos_id_sse",
                table: "rotulos_internos");

            migrationBuilder.CreateIndex(
                name: "IX_rotulos_internos_id_sse",
                table: "rotulos_internos",
                column: "id_sse");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_rotulos_internos_id_sse",
                table: "rotulos_internos");

            migrationBuilder.CreateIndex(
                name: "IX_rotulos_internos_id_sse",
                table: "rotulos_internos",
                column: "id_sse",
                unique: true);
        }
    }
}
