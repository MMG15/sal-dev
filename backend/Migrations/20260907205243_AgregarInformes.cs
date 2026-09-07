using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Ssal.Api.Migrations
{
    /// <inheritdoc />
    public partial class AgregarInformes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "informes",
                columns: table => new
                {
                    id_informe = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    id_muestra = table.Column<int>(type: "integer", nullable: false),
                    generado_por = table.Column<int>(type: "integer", nullable: false),
                    codigo = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    fecha_generacion = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_informes", x => x.id_informe);
                    table.ForeignKey(
                        name: "FK_informes_muestras_id_muestra",
                        column: x => x.id_muestra,
                        principalTable: "muestras",
                        principalColumn: "id_muestra",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_informes_usuarios_generado_por",
                        column: x => x.generado_por,
                        principalTable: "usuarios",
                        principalColumn: "id_usuario",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_informes_generado_por",
                table: "informes",
                column: "generado_por");

            migrationBuilder.CreateIndex(
                name: "IX_informes_id_muestra",
                table: "informes",
                column: "id_muestra",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "informes");
        }
    }
}
