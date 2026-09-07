using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Ssal.Api.Migrations
{
    /// <inheritdoc />
    public partial class ClienteEmpresaDatosFiscales : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "condicion_iva",
                table: "empresas",
                type: "character varying(30)",
                maxLength: 30,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "condicion_iva",
                table: "clientes",
                type: "character varying(30)",
                maxLength: 30,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "cuit",
                table: "clientes",
                type: "character varying(20)",
                maxLength: 20,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "condicion_iva",
                table: "empresas");

            migrationBuilder.DropColumn(
                name: "condicion_iva",
                table: "clientes");

            migrationBuilder.DropColumn(
                name: "cuit",
                table: "clientes");
        }
    }
}
