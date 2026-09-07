using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Ssal.Api.Migrations
{
    /// <inheritdoc />
    public partial class ConsultasPresupuestosItems : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "cotizacion",
                table: "presupuestos",
                type: "numeric",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.CreateTable(
                name: "presupuesto_analisis",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    id_presupuesto = table.Column<int>(type: "integer", nullable: false),
                    id_analisis = table.Column<int>(type: "integer", nullable: false),
                    precio_usd_snapshot = table.Column<decimal>(type: "numeric", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_presupuesto_analisis", x => x.id);
                    table.ForeignKey(
                        name: "FK_presupuesto_analisis_analisis_id_analisis",
                        column: x => x.id_analisis,
                        principalTable: "analisis",
                        principalColumn: "id_analisis",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_presupuesto_analisis_presupuestos_id_presupuesto",
                        column: x => x.id_presupuesto,
                        principalTable: "presupuestos",
                        principalColumn: "id_presupuesto",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_presupuesto_analisis_id_analisis",
                table: "presupuesto_analisis",
                column: "id_analisis");

            migrationBuilder.CreateIndex(
                name: "IX_presupuesto_analisis_id_presupuesto",
                table: "presupuesto_analisis",
                column: "id_presupuesto");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "presupuesto_analisis");

            migrationBuilder.DropColumn(
                name: "cotizacion",
                table: "presupuestos");
        }
    }
}
