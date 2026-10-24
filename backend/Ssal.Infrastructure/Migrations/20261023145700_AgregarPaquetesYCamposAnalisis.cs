using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Ssal.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AgregarPaquetesYCamposAnalisis : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "cantidad_muestra",
                table: "grupos_analisis",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "norma",
                table: "grupos_analisis",
                type: "character varying(300)",
                maxLength: 300,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "tipo_muestra",
                table: "grupos_analisis",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "metodo",
                table: "analisis",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "tercerizado",
                table: "analisis",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateTable(
                name: "paquetes",
                columns: table => new
                {
                    id_paquete = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    nombre = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    area = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    tipo_muestra = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    norma = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true),
                    cantidad_muestra = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    precio_usd = table.Column<decimal>(type: "numeric", nullable: false),
                    activo = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_paquetes", x => x.id_paquete);
                });

            migrationBuilder.CreateTable(
                name: "paquete_analisis",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    id_paquete = table.Column<int>(type: "integer", nullable: false),
                    id_analisis = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_paquete_analisis", x => x.id);
                    table.ForeignKey(
                        name: "FK_paquete_analisis_analisis_id_analisis",
                        column: x => x.id_analisis,
                        principalTable: "analisis",
                        principalColumn: "id_analisis",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_paquete_analisis_paquetes_id_paquete",
                        column: x => x.id_paquete,
                        principalTable: "paquetes",
                        principalColumn: "id_paquete",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "presupuesto_paquetes",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    id_presupuesto = table.Column<int>(type: "integer", nullable: false),
                    id_paquete = table.Column<int>(type: "integer", nullable: false),
                    precio_usd_snapshot = table.Column<decimal>(type: "numeric", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_presupuesto_paquetes", x => x.id);
                    table.ForeignKey(
                        name: "FK_presupuesto_paquetes_paquetes_id_paquete",
                        column: x => x.id_paquete,
                        principalTable: "paquetes",
                        principalColumn: "id_paquete",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_presupuesto_paquetes_presupuestos_id_presupuesto",
                        column: x => x.id_presupuesto,
                        principalTable: "presupuestos",
                        principalColumn: "id_presupuesto",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_paquete_analisis_id_analisis",
                table: "paquete_analisis",
                column: "id_analisis");

            migrationBuilder.CreateIndex(
                name: "IX_paquete_analisis_id_paquete",
                table: "paquete_analisis",
                column: "id_paquete");

            migrationBuilder.CreateIndex(
                name: "IX_presupuesto_paquetes_id_paquete",
                table: "presupuesto_paquetes",
                column: "id_paquete");

            migrationBuilder.CreateIndex(
                name: "IX_presupuesto_paquetes_id_presupuesto",
                table: "presupuesto_paquetes",
                column: "id_presupuesto");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "paquete_analisis");

            migrationBuilder.DropTable(
                name: "presupuesto_paquetes");

            migrationBuilder.DropTable(
                name: "paquetes");

            migrationBuilder.DropColumn(
                name: "cantidad_muestra",
                table: "grupos_analisis");

            migrationBuilder.DropColumn(
                name: "norma",
                table: "grupos_analisis");

            migrationBuilder.DropColumn(
                name: "tipo_muestra",
                table: "grupos_analisis");

            migrationBuilder.DropColumn(
                name: "metodo",
                table: "analisis");

            migrationBuilder.DropColumn(
                name: "tercerizado",
                table: "analisis");
        }
    }
}
