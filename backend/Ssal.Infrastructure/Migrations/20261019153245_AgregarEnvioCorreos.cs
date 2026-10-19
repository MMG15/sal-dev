using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Ssal.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AgregarEnvioCorreos : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "message_id_enviado",
                table: "presupuestos",
                type: "character varying(250)",
                maxLength: 250,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "puerto_smtp",
                table: "cuentas_correo",
                type: "integer",
                nullable: false,
                defaultValue: 587);

            migrationBuilder.AddColumn<string>(
                name: "servidor_smtp",
                table: "cuentas_correo",
                type: "character varying(150)",
                maxLength: 150,
                nullable: false,
                defaultValue: "smtp.gmail.com");

            migrationBuilder.AddColumn<int>(
                name: "id_consulta_relacionada",
                table: "correos_entrantes",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "message_id_enviado",
                table: "correos_entrantes",
                type: "character varying(250)",
                maxLength: 250,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_correos_entrantes_id_consulta_relacionada",
                table: "correos_entrantes",
                column: "id_consulta_relacionada");

            migrationBuilder.AddForeignKey(
                name: "FK_correos_entrantes_consultas_id_consulta_relacionada",
                table: "correos_entrantes",
                column: "id_consulta_relacionada",
                principalTable: "consultas",
                principalColumn: "id_consulta",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_correos_entrantes_consultas_id_consulta_relacionada",
                table: "correos_entrantes");

            migrationBuilder.DropIndex(
                name: "IX_correos_entrantes_id_consulta_relacionada",
                table: "correos_entrantes");

            migrationBuilder.DropColumn(
                name: "message_id_enviado",
                table: "presupuestos");

            migrationBuilder.DropColumn(
                name: "puerto_smtp",
                table: "cuentas_correo");

            migrationBuilder.DropColumn(
                name: "servidor_smtp",
                table: "cuentas_correo");

            migrationBuilder.DropColumn(
                name: "id_consulta_relacionada",
                table: "correos_entrantes");

            migrationBuilder.DropColumn(
                name: "message_id_enviado",
                table: "correos_entrantes");
        }
    }
}
