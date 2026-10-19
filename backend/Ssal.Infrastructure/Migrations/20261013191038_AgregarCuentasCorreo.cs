using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Ssal.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AgregarCuentasCorreo : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "id_cuenta_correo",
                table: "correos_entrantes",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "cuentas_correo",
                columns: table => new
                {
                    id_cuenta = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    nombre = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    servidor = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    puerto = table.Column<int>(type: "integer", nullable: false),
                    usuario = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    password_cifrada = table.Column<string>(type: "text", nullable: false),
                    habilitada = table.Column<bool>(type: "boolean", nullable: false),
                    intervalo_minutos = table.Column<int>(type: "integer", nullable: false),
                    ultima_revision = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ultimo_error = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_cuentas_correo", x => x.id_cuenta);
                });

            migrationBuilder.CreateIndex(
                name: "IX_correos_entrantes_id_cuenta_correo",
                table: "correos_entrantes",
                column: "id_cuenta_correo");

            migrationBuilder.AddForeignKey(
                name: "FK_correos_entrantes_cuentas_correo_id_cuenta_correo",
                table: "correos_entrantes",
                column: "id_cuenta_correo",
                principalTable: "cuentas_correo",
                principalColumn: "id_cuenta",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_correos_entrantes_cuentas_correo_id_cuenta_correo",
                table: "correos_entrantes");

            migrationBuilder.DropTable(
                name: "cuentas_correo");

            migrationBuilder.DropIndex(
                name: "IX_correos_entrantes_id_cuenta_correo",
                table: "correos_entrantes");

            migrationBuilder.DropColumn(
                name: "id_cuenta_correo",
                table: "correos_entrantes");
        }
    }
}
