using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Ssal.Api.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "empresas",
                columns: table => new
                {
                    id_empresa = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    razon_social = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    cuit = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    email = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: true),
                    telefono = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    direccion = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true),
                    estado = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_empresas", x => x.id_empresa);
                });

            migrationBuilder.CreateTable(
                name: "grupos_analisis",
                columns: table => new
                {
                    id_grupo = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    codigo_agrupador = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    descripcion = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true),
                    area = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_grupos_analisis", x => x.id_grupo);
                });

            migrationBuilder.CreateTable(
                name: "roles",
                columns: table => new
                {
                    id_rol = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    codigo = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    nombre = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    descripcion = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_roles", x => x.id_rol);
                });

            migrationBuilder.CreateTable(
                name: "clientes",
                columns: table => new
                {
                    id_cliente = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    id_empresa = table.Column<int>(type: "integer", nullable: true),
                    nombre = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    apellido = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    email = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: true),
                    telefono = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    usuario_web = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    estado = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_clientes", x => x.id_cliente);
                    table.ForeignKey(
                        name: "FK_clientes_empresas_id_empresa",
                        column: x => x.id_empresa,
                        principalTable: "empresas",
                        principalColumn: "id_empresa");
                });

            migrationBuilder.CreateTable(
                name: "analisis",
                columns: table => new
                {
                    id_analisis = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    id_grupo = table.Column<int>(type: "integer", nullable: false),
                    codigo = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    nombre = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    area = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    precio_usd = table.Column<decimal>(type: "numeric", nullable: false),
                    activo = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_analisis", x => x.id_analisis);
                    table.ForeignKey(
                        name: "FK_analisis_grupos_analisis_id_grupo",
                        column: x => x.id_grupo,
                        principalTable: "grupos_analisis",
                        principalColumn: "id_grupo",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "usuarios",
                columns: table => new
                {
                    id_usuario = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    id_rol = table.Column<int>(type: "integer", nullable: false),
                    nombre = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    apellido = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    email = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    password_hash = table.Column<string>(type: "text", nullable: false),
                    activo = table.Column<bool>(type: "boolean", nullable: false),
                    forzar_cambio_pwd = table.Column<bool>(type: "boolean", nullable: false),
                    ultimo_acceso = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_usuarios", x => x.id_usuario);
                    table.ForeignKey(
                        name: "FK_usuarios_roles_id_rol",
                        column: x => x.id_rol,
                        principalTable: "roles",
                        principalColumn: "id_rol",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "consultas",
                columns: table => new
                {
                    id_consulta = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    id_cliente = table.Column<int>(type: "integer", nullable: true),
                    id_empresa = table.Column<int>(type: "integer", nullable: true),
                    id_responsable = table.Column<int>(type: "integer", nullable: true),
                    codigo = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    canal = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    estado = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    descripcion = table.Column<string>(type: "text", nullable: true),
                    fecha = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_consultas", x => x.id_consulta);
                    table.ForeignKey(
                        name: "FK_consultas_clientes_id_cliente",
                        column: x => x.id_cliente,
                        principalTable: "clientes",
                        principalColumn: "id_cliente");
                    table.ForeignKey(
                        name: "FK_consultas_empresas_id_empresa",
                        column: x => x.id_empresa,
                        principalTable: "empresas",
                        principalColumn: "id_empresa");
                    table.ForeignKey(
                        name: "FK_consultas_usuarios_id_responsable",
                        column: x => x.id_responsable,
                        principalTable: "usuarios",
                        principalColumn: "id_usuario",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "firmas_documento",
                columns: table => new
                {
                    id_firma = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    id_firmante = table.Column<int>(type: "integer", nullable: false),
                    tipo_documento = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    id_documento = table.Column<int>(type: "integer", nullable: false),
                    tipo_firma = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    estado = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    motivo_rechazo = table.Column<string>(type: "text", nullable: true),
                    fecha_firma = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_firmas_documento", x => x.id_firma);
                    table.ForeignKey(
                        name: "FK_firmas_documento_usuarios_id_firmante",
                        column: x => x.id_firmante,
                        principalTable: "usuarios",
                        principalColumn: "id_usuario",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "presupuestos",
                columns: table => new
                {
                    id_presupuesto = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    id_consulta = table.Column<int>(type: "integer", nullable: false),
                    creado_por = table.Column<int>(type: "integer", nullable: false),
                    codigo_rpo = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    importe_pesos = table.Column<decimal>(type: "numeric", nullable: false),
                    importe_dolares = table.Column<decimal>(type: "numeric", nullable: false),
                    adicional_asesoramiento = table.Column<decimal>(type: "numeric", nullable: false),
                    estado = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    fecha = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_presupuestos", x => x.id_presupuesto);
                    table.ForeignKey(
                        name: "FK_presupuestos_consultas_id_consulta",
                        column: x => x.id_consulta,
                        principalTable: "consultas",
                        principalColumn: "id_consulta",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_presupuestos_usuarios_creado_por",
                        column: x => x.creado_por,
                        principalTable: "usuarios",
                        principalColumn: "id_usuario",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "sses",
                columns: table => new
                {
                    id_sse = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    id_presupuesto = table.Column<int>(type: "integer", nullable: false),
                    id_cliente = table.Column<int>(type: "integer", nullable: false),
                    creado_por = table.Column<int>(type: "integer", nullable: false),
                    codigo_rpo = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    area = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    importe_pesos = table.Column<decimal>(type: "numeric", nullable: false),
                    importe_dolares = table.Column<decimal>(type: "numeric", nullable: false),
                    forma_pago = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    estado = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    fecha = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_sses", x => x.id_sse);
                    table.ForeignKey(
                        name: "FK_sses_clientes_id_cliente",
                        column: x => x.id_cliente,
                        principalTable: "clientes",
                        principalColumn: "id_cliente",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_sses_presupuestos_id_presupuesto",
                        column: x => x.id_presupuesto,
                        principalTable: "presupuestos",
                        principalColumn: "id_presupuesto",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_sses_usuarios_creado_por",
                        column: x => x.creado_por,
                        principalTable: "usuarios",
                        principalColumn: "id_usuario",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "facturas",
                columns: table => new
                {
                    id_factura = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    id_sse = table.Column<int>(type: "integer", nullable: false),
                    creado_por = table.Column<int>(type: "integer", nullable: false),
                    numero = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    importe_pesos = table.Column<decimal>(type: "numeric", nullable: false),
                    importe_dolares = table.Column<decimal>(type: "numeric", nullable: false),
                    estado = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    fecha = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_facturas", x => x.id_factura);
                    table.ForeignKey(
                        name: "FK_facturas_sses_id_sse",
                        column: x => x.id_sse,
                        principalTable: "sses",
                        principalColumn: "id_sse",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_facturas_usuarios_creado_por",
                        column: x => x.creado_por,
                        principalTable: "usuarios",
                        principalColumn: "id_usuario",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "rotulos_internos",
                columns: table => new
                {
                    id_rotulo = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    id_sse = table.Column<int>(type: "integer", nullable: false),
                    asignado_por = table.Column<int>(type: "integer", nullable: false),
                    numero_unico = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    descripcion = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    estado = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    fecha_asignacion = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_rotulos_internos", x => x.id_rotulo);
                    table.ForeignKey(
                        name: "FK_rotulos_internos_sses_id_sse",
                        column: x => x.id_sse,
                        principalTable: "sses",
                        principalColumn: "id_sse",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_rotulos_internos_usuarios_asignado_por",
                        column: x => x.asignado_por,
                        principalTable: "usuarios",
                        principalColumn: "id_usuario",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "auditorias_rotulo",
                columns: table => new
                {
                    id_auditoria = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    id_rotulo = table.Column<int>(type: "integer", nullable: false),
                    id_usuario = table.Column<int>(type: "integer", nullable: false),
                    valor_anterior = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    valor_nuevo = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    motivo = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    fecha_cambio = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_auditorias_rotulo", x => x.id_auditoria);
                    table.ForeignKey(
                        name: "FK_auditorias_rotulo_rotulos_internos_id_rotulo",
                        column: x => x.id_rotulo,
                        principalTable: "rotulos_internos",
                        principalColumn: "id_rotulo",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_auditorias_rotulo_usuarios_id_usuario",
                        column: x => x.id_usuario,
                        principalTable: "usuarios",
                        principalColumn: "id_usuario",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "muestras",
                columns: table => new
                {
                    id_muestra = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    id_rotulo = table.Column<int>(type: "integer", nullable: false),
                    recibido_por = table.Column<int>(type: "integer", nullable: false),
                    tipo = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    estado = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    observacion = table.Column<string>(type: "text", nullable: true),
                    fecha_recepcion = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_muestras", x => x.id_muestra);
                    table.ForeignKey(
                        name: "FK_muestras_rotulos_internos_id_rotulo",
                        column: x => x.id_rotulo,
                        principalTable: "rotulos_internos",
                        principalColumn: "id_rotulo",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_muestras_usuarios_recibido_por",
                        column: x => x.recibido_por,
                        principalTable: "usuarios",
                        principalColumn: "id_usuario",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "resultados",
                columns: table => new
                {
                    id_resultado = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    id_muestra = table.Column<int>(type: "integer", nullable: false),
                    id_analisis = table.Column<int>(type: "integer", nullable: false),
                    cargado_por = table.Column<int>(type: "integer", nullable: false),
                    valor = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    estado = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    fecha_carga = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_resultados", x => x.id_resultado);
                    table.ForeignKey(
                        name: "FK_resultados_analisis_id_analisis",
                        column: x => x.id_analisis,
                        principalTable: "analisis",
                        principalColumn: "id_analisis",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_resultados_muestras_id_muestra",
                        column: x => x.id_muestra,
                        principalTable: "muestras",
                        principalColumn: "id_muestra",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_resultados_usuarios_cargado_por",
                        column: x => x.cargado_por,
                        principalTable: "usuarios",
                        principalColumn: "id_usuario",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "roles",
                columns: new[] { "id_rol", "codigo", "descripcion", "nombre" },
                values: new object[,]
                {
                    { 1, "ROL-01", "Acceso total. Aprobación final con firma digital.", "Administrador general / Director Técnico" },
                    { 2, "ROL-02", "Registro de consultas, clientes, SSE y rótulos.", "Administrativo" },
                    { 3, "ROL-03", "Generación y envío de presupuestos.", "Presupuestos" },
                    { 4, "ROL-04", "Facturación y seguimiento de deudas.", "Cobranzas" },
                    { 5, "ROL-05", "Solo lectura en portal web.", "Cliente externo" },
                    { 6, "ROL-06", "Backoffice técnico. Exclusivo del equipo de desarrollo.", "Administrador de sistema" },
                    { 7, "ROL-07", "Realiza análisis y carga resultados.", "Analista" },
                    { 8, "ROL-08", "Supervisa Microbiología. Firma digital de documentos.", "Responsable de área MIC" },
                    { 9, "ROL-09", "Supervisa Físico Química. Firma digital de documentos.", "Responsable de área FQ" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_analisis_id_grupo",
                table: "analisis",
                column: "id_grupo");

            migrationBuilder.CreateIndex(
                name: "IX_auditorias_rotulo_id_rotulo",
                table: "auditorias_rotulo",
                column: "id_rotulo");

            migrationBuilder.CreateIndex(
                name: "IX_auditorias_rotulo_id_usuario",
                table: "auditorias_rotulo",
                column: "id_usuario");

            migrationBuilder.CreateIndex(
                name: "IX_clientes_id_empresa",
                table: "clientes",
                column: "id_empresa");

            migrationBuilder.CreateIndex(
                name: "IX_consultas_id_cliente",
                table: "consultas",
                column: "id_cliente");

            migrationBuilder.CreateIndex(
                name: "IX_consultas_id_empresa",
                table: "consultas",
                column: "id_empresa");

            migrationBuilder.CreateIndex(
                name: "IX_consultas_id_responsable",
                table: "consultas",
                column: "id_responsable");

            migrationBuilder.CreateIndex(
                name: "IX_facturas_creado_por",
                table: "facturas",
                column: "creado_por");

            migrationBuilder.CreateIndex(
                name: "IX_facturas_id_sse",
                table: "facturas",
                column: "id_sse",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_firmas_documento_id_firmante",
                table: "firmas_documento",
                column: "id_firmante");

            migrationBuilder.CreateIndex(
                name: "IX_muestras_id_rotulo",
                table: "muestras",
                column: "id_rotulo",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_muestras_recibido_por",
                table: "muestras",
                column: "recibido_por");

            migrationBuilder.CreateIndex(
                name: "IX_presupuestos_creado_por",
                table: "presupuestos",
                column: "creado_por");

            migrationBuilder.CreateIndex(
                name: "IX_presupuestos_id_consulta",
                table: "presupuestos",
                column: "id_consulta",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_resultados_cargado_por",
                table: "resultados",
                column: "cargado_por");

            migrationBuilder.CreateIndex(
                name: "IX_resultados_id_analisis",
                table: "resultados",
                column: "id_analisis");

            migrationBuilder.CreateIndex(
                name: "IX_resultados_id_muestra",
                table: "resultados",
                column: "id_muestra");

            migrationBuilder.CreateIndex(
                name: "IX_roles_codigo",
                table: "roles",
                column: "codigo",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_rotulos_internos_asignado_por",
                table: "rotulos_internos",
                column: "asignado_por");

            migrationBuilder.CreateIndex(
                name: "IX_rotulos_internos_id_sse",
                table: "rotulos_internos",
                column: "id_sse",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_rotulos_internos_numero_unico",
                table: "rotulos_internos",
                column: "numero_unico",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_sses_creado_por",
                table: "sses",
                column: "creado_por");

            migrationBuilder.CreateIndex(
                name: "IX_sses_id_cliente",
                table: "sses",
                column: "id_cliente");

            migrationBuilder.CreateIndex(
                name: "IX_sses_id_presupuesto",
                table: "sses",
                column: "id_presupuesto",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_usuarios_email",
                table: "usuarios",
                column: "email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_usuarios_id_rol",
                table: "usuarios",
                column: "id_rol");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "auditorias_rotulo");

            migrationBuilder.DropTable(
                name: "facturas");

            migrationBuilder.DropTable(
                name: "firmas_documento");

            migrationBuilder.DropTable(
                name: "resultados");

            migrationBuilder.DropTable(
                name: "analisis");

            migrationBuilder.DropTable(
                name: "muestras");

            migrationBuilder.DropTable(
                name: "grupos_analisis");

            migrationBuilder.DropTable(
                name: "rotulos_internos");

            migrationBuilder.DropTable(
                name: "sses");

            migrationBuilder.DropTable(
                name: "presupuestos");

            migrationBuilder.DropTable(
                name: "consultas");

            migrationBuilder.DropTable(
                name: "clientes");

            migrationBuilder.DropTable(
                name: "usuarios");

            migrationBuilder.DropTable(
                name: "empresas");

            migrationBuilder.DropTable(
                name: "roles");
        }
    }
}
