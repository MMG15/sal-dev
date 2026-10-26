using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Ssal.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AgregarMetodosCatalogo : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "metodo",
                table: "analisis");

            migrationBuilder.AddColumn<int>(
                name: "id_metodo",
                table: "analisis",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "metodos",
                columns: table => new
                {
                    id_metodo = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    nombre = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    descripcion = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    area = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    activo = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_metodos", x => x.id_metodo);
                });

            migrationBuilder.CreateTable(
                name: "presupuesto_metodos",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    id_presupuesto = table.Column<int>(type: "integer", nullable: false),
                    id_metodo = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_presupuesto_metodos", x => x.id);
                    table.ForeignKey(
                        name: "FK_presupuesto_metodos_metodos_id_metodo",
                        column: x => x.id_metodo,
                        principalTable: "metodos",
                        principalColumn: "id_metodo",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_presupuesto_metodos_presupuestos_id_presupuesto",
                        column: x => x.id_presupuesto,
                        principalTable: "presupuestos",
                        principalColumn: "id_presupuesto",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_analisis_id_metodo",
                table: "analisis",
                column: "id_metodo");

            migrationBuilder.CreateIndex(
                name: "IX_presupuesto_metodos_id_metodo",
                table: "presupuesto_metodos",
                column: "id_metodo");

            migrationBuilder.CreateIndex(
                name: "IX_presupuesto_metodos_id_presupuesto",
                table: "presupuesto_metodos",
                column: "id_presupuesto");

            migrationBuilder.AddForeignKey(
                name: "FK_analisis_metodos_id_metodo",
                table: "analisis",
                column: "id_metodo",
                principalTable: "metodos",
                principalColumn: "id_metodo");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_analisis_metodos_id_metodo",
                table: "analisis");

            migrationBuilder.DropTable(
                name: "presupuesto_metodos");

            migrationBuilder.DropTable(
                name: "metodos");

            migrationBuilder.DropIndex(
                name: "IX_analisis_id_metodo",
                table: "analisis");

            migrationBuilder.DropColumn(
                name: "id_metodo",
                table: "analisis");

            migrationBuilder.AddColumn<string>(
                name: "metodo",
                table: "analisis",
                type: "text",
                nullable: true);
        }
    }
}
