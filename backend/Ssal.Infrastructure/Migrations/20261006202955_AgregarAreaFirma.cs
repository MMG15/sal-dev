using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Ssal.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AgregarAreaFirma : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_firmas_documento_usuarios_id_firmante",
                table: "firmas_documento");

            migrationBuilder.AddColumn<string>(
                name: "area",
                table: "firmas_documento",
                type: "character varying(10)",
                maxLength: 10,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_firmas_documento_tipo_documento_id_documento",
                table: "firmas_documento",
                columns: new[] { "tipo_documento", "id_documento" });

            migrationBuilder.AddForeignKey(
                name: "FK_firmas_documento_usuarios_id_firmante",
                table: "firmas_documento",
                column: "id_firmante",
                principalTable: "usuarios",
                principalColumn: "id_usuario",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_firmas_documento_usuarios_id_firmante",
                table: "firmas_documento");

            migrationBuilder.DropIndex(
                name: "IX_firmas_documento_tipo_documento_id_documento",
                table: "firmas_documento");

            migrationBuilder.DropColumn(
                name: "area",
                table: "firmas_documento");

            migrationBuilder.AddForeignKey(
                name: "FK_firmas_documento_usuarios_id_firmante",
                table: "firmas_documento",
                column: "id_firmante",
                principalTable: "usuarios",
                principalColumn: "id_usuario",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
