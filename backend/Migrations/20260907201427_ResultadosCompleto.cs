using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Ssal.Api.Migrations
{
    /// <inheritdoc />
    public partial class ResultadosCompleto : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_resultados_usuarios_cargado_por",
                table: "resultados");

            migrationBuilder.AlterColumn<DateTime>(
                name: "fecha_carga",
                table: "resultados",
                type: "timestamp with time zone",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone");

            migrationBuilder.AlterColumn<int>(
                name: "cargado_por",
                table: "resultados",
                type: "integer",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AddColumn<DateTime>(
                name: "fecha_validacion",
                table: "resultados",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "motivo_rechazo",
                table: "resultados",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "validado_por",
                table: "resultados",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "rango_max",
                table: "analisis",
                type: "numeric",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "rango_min",
                table: "analisis",
                type: "numeric",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "unidad",
                table: "analisis",
                type: "character varying(30)",
                maxLength: 30,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "valor_esperado",
                table: "analisis",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_resultados_validado_por",
                table: "resultados",
                column: "validado_por");

            migrationBuilder.AddForeignKey(
                name: "FK_resultados_usuarios_cargado_por",
                table: "resultados",
                column: "cargado_por",
                principalTable: "usuarios",
                principalColumn: "id_usuario",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_resultados_usuarios_validado_por",
                table: "resultados",
                column: "validado_por",
                principalTable: "usuarios",
                principalColumn: "id_usuario",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_resultados_usuarios_cargado_por",
                table: "resultados");

            migrationBuilder.DropForeignKey(
                name: "FK_resultados_usuarios_validado_por",
                table: "resultados");

            migrationBuilder.DropIndex(
                name: "IX_resultados_validado_por",
                table: "resultados");

            migrationBuilder.DropColumn(
                name: "fecha_validacion",
                table: "resultados");

            migrationBuilder.DropColumn(
                name: "motivo_rechazo",
                table: "resultados");

            migrationBuilder.DropColumn(
                name: "validado_por",
                table: "resultados");

            migrationBuilder.DropColumn(
                name: "rango_max",
                table: "analisis");

            migrationBuilder.DropColumn(
                name: "rango_min",
                table: "analisis");

            migrationBuilder.DropColumn(
                name: "unidad",
                table: "analisis");

            migrationBuilder.DropColumn(
                name: "valor_esperado",
                table: "analisis");

            migrationBuilder.AlterColumn<DateTime>(
                name: "fecha_carga",
                table: "resultados",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified),
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "cargado_por",
                table: "resultados",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_resultados_usuarios_cargado_por",
                table: "resultados",
                column: "cargado_por",
                principalTable: "usuarios",
                principalColumn: "id_usuario",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
