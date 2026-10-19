using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Ssal.Api.Migrations
{
    /// <inheritdoc />
    public partial class AgregarOrigenConsulta : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "externo_id",
                table: "consultas",
                type: "character varying(150)",
                maxLength: 150,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "origen",
                table: "consultas",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "manual");

            migrationBuilder.CreateIndex(
                name: "IX_consultas_externo_id",
                table: "consultas",
                column: "externo_id",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_consultas_externo_id",
                table: "consultas");

            migrationBuilder.DropColumn(
                name: "externo_id",
                table: "consultas");

            migrationBuilder.DropColumn(
                name: "origen",
                table: "consultas");
        }
    }
}
