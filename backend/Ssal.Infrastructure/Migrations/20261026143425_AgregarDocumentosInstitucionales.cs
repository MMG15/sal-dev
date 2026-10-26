using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Ssal.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AgregarDocumentosInstitucionales : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "documentos_institucionales",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    nombre = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    nombre_original = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    nombre_almacenado = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    tipo_mime = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    tamanio_bytes = table.Column<long>(type: "bigint", nullable: false),
                    subido_por = table.Column<int>(type: "integer", nullable: false),
                    subido_en = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_documentos_institucionales", x => x.id);
                    table.ForeignKey(
                        name: "FK_documentos_institucionales_usuarios_subido_por",
                        column: x => x.subido_por,
                        principalTable: "usuarios",
                        principalColumn: "id_usuario",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_documentos_institucionales_subido_por",
                table: "documentos_institucionales",
                column: "subido_por");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "documentos_institucionales");
        }
    }
}
