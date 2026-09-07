using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Ssal.Api.Migrations
{
    /// <inheritdoc />
    public partial class PresupuestoArchivos : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "presupuesto_archivos",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    id_presupuesto = table.Column<int>(type: "integer", nullable: false),
                    nombre_original = table.Column<string>(type: "character varying(260)", maxLength: 260, nullable: false),
                    nombre_almacenado = table.Column<string>(type: "character varying(260)", maxLength: 260, nullable: false),
                    tipo_mime = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    tamaño_bytes = table.Column<long>(type: "bigint", nullable: false),
                    subido_en = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    subido_por = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_presupuesto_archivos", x => x.id);
                    table.ForeignKey(
                        name: "FK_presupuesto_archivos_presupuestos_id_presupuesto",
                        column: x => x.id_presupuesto,
                        principalTable: "presupuestos",
                        principalColumn: "id_presupuesto",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_presupuesto_archivos_usuarios_subido_por",
                        column: x => x.subido_por,
                        principalTable: "usuarios",
                        principalColumn: "id_usuario",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_presupuesto_archivos_id_presupuesto",
                table: "presupuesto_archivos",
                column: "id_presupuesto");

            migrationBuilder.CreateIndex(
                name: "IX_presupuesto_archivos_subido_por",
                table: "presupuesto_archivos",
                column: "subido_por");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "presupuesto_archivos");
        }
    }
}
