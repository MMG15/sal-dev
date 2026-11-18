using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Ssal.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class EliminarMetodosDePresupuesto : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "presupuesto_metodos");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "presupuesto_metodos",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    id_metodo = table.Column<int>(type: "integer", nullable: false),
                    id_presupuesto = table.Column<int>(type: "integer", nullable: false)
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
                name: "IX_presupuesto_metodos_id_metodo",
                table: "presupuesto_metodos",
                column: "id_metodo");

            migrationBuilder.CreateIndex(
                name: "IX_presupuesto_metodos_id_presupuesto",
                table: "presupuesto_metodos",
                column: "id_presupuesto");
        }
    }
}
