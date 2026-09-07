using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Ssal.Api.Migrations
{
    /// <inheritdoc />
    public partial class SseCodigoYCamposOpcionales : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_sses_clientes_id_cliente",
                table: "sses");

            migrationBuilder.DropForeignKey(
                name: "FK_sses_presupuestos_id_presupuesto",
                table: "sses");

            migrationBuilder.AlterColumn<int>(
                name: "id_presupuesto",
                table: "sses",
                type: "integer",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AlterColumn<int>(
                name: "id_cliente",
                table: "sses",
                type: "integer",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AddColumn<string>(
                name: "codigo",
                table: "sses",
                type: "character varying(30)",
                maxLength: 30,
                nullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_sses_clientes_id_cliente",
                table: "sses",
                column: "id_cliente",
                principalTable: "clientes",
                principalColumn: "id_cliente");

            migrationBuilder.AddForeignKey(
                name: "FK_sses_presupuestos_id_presupuesto",
                table: "sses",
                column: "id_presupuesto",
                principalTable: "presupuestos",
                principalColumn: "id_presupuesto");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_sses_clientes_id_cliente",
                table: "sses");

            migrationBuilder.DropForeignKey(
                name: "FK_sses_presupuestos_id_presupuesto",
                table: "sses");

            migrationBuilder.DropColumn(
                name: "codigo",
                table: "sses");

            migrationBuilder.AlterColumn<int>(
                name: "id_presupuesto",
                table: "sses",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "id_cliente",
                table: "sses",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_sses_clientes_id_cliente",
                table: "sses",
                column: "id_cliente",
                principalTable: "clientes",
                principalColumn: "id_cliente",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_sses_presupuestos_id_presupuesto",
                table: "sses",
                column: "id_presupuesto",
                principalTable: "presupuestos",
                principalColumn: "id_presupuesto",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
