using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Ssal.Api.Migrations
{
    /// <inheritdoc />
    public partial class AgregarAuditoriaSistema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "auditoria_sistema",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    fecha = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    id_usuario = table.Column<int>(type: "integer", nullable: true),
                    accion = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    objetivo = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: true),
                    detalle = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_auditoria_sistema", x => x.id);
                    table.ForeignKey(
                        name: "FK_auditoria_sistema_usuarios_id_usuario",
                        column: x => x.id_usuario,
                        principalTable: "usuarios",
                        principalColumn: "id_usuario");
                });

            migrationBuilder.CreateIndex(
                name: "IX_auditoria_sistema_fecha",
                table: "auditoria_sistema",
                column: "fecha");

            migrationBuilder.CreateIndex(
                name: "IX_auditoria_sistema_id_usuario",
                table: "auditoria_sistema",
                column: "id_usuario");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "auditoria_sistema");
        }
    }
}
