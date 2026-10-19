using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Ssal.Api.Migrations
{
    /// <inheritdoc />
    public partial class AgregarCorreosEntrantes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "correos_entrantes",
                columns: table => new
                {
                    id_correo = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    externo_id = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: false),
                    remitente = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    remitente_nombre = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: true),
                    asunto = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true),
                    cuerpo = table.Column<string>(type: "text", nullable: true),
                    fecha_recibido = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    estado = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    id_consulta_generada = table.Column<int>(type: "integer", nullable: true),
                    id_usuario_accion = table.Column<int>(type: "integer", nullable: true),
                    fecha_accion = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_correos_entrantes", x => x.id_correo);
                    table.ForeignKey(
                        name: "FK_correos_entrantes_consultas_id_consulta_generada",
                        column: x => x.id_consulta_generada,
                        principalTable: "consultas",
                        principalColumn: "id_consulta",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_correos_entrantes_usuarios_id_usuario_accion",
                        column: x => x.id_usuario_accion,
                        principalTable: "usuarios",
                        principalColumn: "id_usuario",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateIndex(
                name: "IX_correos_entrantes_externo_id",
                table: "correos_entrantes",
                column: "externo_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_correos_entrantes_id_consulta_generada",
                table: "correos_entrantes",
                column: "id_consulta_generada");

            migrationBuilder.CreateIndex(
                name: "IX_correos_entrantes_id_usuario_accion",
                table: "correos_entrantes",
                column: "id_usuario_accion");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "correos_entrantes");
        }
    }
}
