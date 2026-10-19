using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Ssal.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AgregarEmpresaSse : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "id_empresa",
                table: "sses",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_sses_id_empresa",
                table: "sses",
                column: "id_empresa");

            migrationBuilder.AddForeignKey(
                name: "FK_sses_empresas_id_empresa",
                table: "sses",
                column: "id_empresa",
                principalTable: "empresas",
                principalColumn: "id_empresa");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_sses_empresas_id_empresa",
                table: "sses");

            migrationBuilder.DropIndex(
                name: "IX_sses_id_empresa",
                table: "sses");

            migrationBuilder.DropColumn(
                name: "id_empresa",
                table: "sses");
        }
    }
}
