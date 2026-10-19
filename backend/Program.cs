using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using QuestPDF.Infrastructure;
using Ssal.Infrastructure.Data;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

QuestPDF.Settings.License = LicenseType.Community;

var builder = WebApplication.CreateBuilder(args);

// Base de datos PostgreSQL
builder.Services.AddDbContext<SsalDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// Autenticación JWT
var jwtKey = builder.Configuration["Jwt:Key"]
    ?? throw new InvalidOperationException("Jwt:Key no configurada en appsettings.");

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey))
        };

        // Una cuenta desactivada pierde el acceso de inmediato, aunque el token todavía no haya vencido
        options.Events = new JwtBearerEvents
        {
            OnTokenValidated = async ctx =>
            {
                var db = ctx.HttpContext.RequestServices.GetRequiredService<SsalDbContext>();
                var sub = ctx.Principal?.FindFirst(JwtRegisteredClaimNames.Sub)?.Value
                    ?? ctx.Principal?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                var rol = ctx.Principal?.FindFirst(ClaimTypes.Role)?.Value;
                if (!int.TryParse(sub, out var id))
                {
                    ctx.Fail("Token inválido.");
                    return;
                }

                var activo = rol == "PORTAL"
                    ? await db.PortalUsuarios.AnyAsync(p => p.IdPortalUsuario == id && p.Activo)
                    : await db.Usuarios.AnyAsync(u => u.IdUsuario == id && u.Activo);

                if (!activo) ctx.Fail("Cuenta desactivada.");
            }
        };
    });

builder.Services.AddAuthorization();

// CORS: permite que el frontend React en localhost:5173 consuma la API
builder.Services.AddCors(options =>
{
    options.AddPolicy("FrontendDev", policy =>
        policy.WithOrigins("http://localhost:5173")
              .AllowAnyHeader()
              .AllowAnyMethod());
});

builder.Services.AddControllers();

// Bandeja de correo del laboratorio: lee el buzón en segundo plano (apagado por defecto
// hasta tener credenciales reales, ver MailImport:Habilitado en appsettings).
builder.Services.AddScoped<Ssal.Infrastructure.ImportadorCorreos>();
builder.Services.AddHostedService<Ssal.Infrastructure.ImportadorCorreosBackgroundService>();

// Análisis: primer módulo migrado a Arquitectura Limpia (Fase 2). El controller ya no
// conoce SsalDbContext — depende de los casos de uso, que a su vez dependen de la interfaz
// del repositorio; la implementación real (EF Core) se resuelve acá, en el punto de entrada.
builder.Services.AddScoped<Ssal.Application.Analisis.IAnalisisRepository, Ssal.Infrastructure.Repositorios.AnalisisRepository>();
builder.Services.AddScoped<Ssal.Application.Analisis.ListarAnalisisActivos>();
builder.Services.AddScoped<Ssal.Application.Analisis.ListarTodosLosAnalisis>();
builder.Services.AddScoped<Ssal.Application.Analisis.ListarGruposDeAnalisis>();
builder.Services.AddScoped<Ssal.Application.Analisis.CrearAnalisis>();
builder.Services.AddScoped<Ssal.Application.Analisis.ActualizarAnalisis>();
builder.Services.AddScoped<Ssal.Application.Analisis.ToggleActivoAnalisis>();
builder.Services.AddScoped<Ssal.Application.Analisis.EliminarAnalisis>();

// Auditoría: segundo módulo migrado (lectura pura, sin reglas de negocio que mutan nada).
builder.Services.AddScoped<Ssal.Application.Auditoria.IAuditoriaRepository, Ssal.Infrastructure.Repositorios.AuditoriaRepository>();
builder.Services.AddScoped<Ssal.Application.Auditoria.ListarAuditoria>();

// Empresas
builder.Services.AddScoped<Ssal.Application.Empresas.IEmpresaRepository, Ssal.Infrastructure.Repositorios.EmpresaRepository>();
builder.Services.AddScoped<Ssal.Application.Empresas.ListarEmpresas>();
builder.Services.AddScoped<Ssal.Application.Empresas.ObtenerEmpresa>();
builder.Services.AddScoped<Ssal.Application.Empresas.CrearEmpresa>();
builder.Services.AddScoped<Ssal.Application.Empresas.ActualizarEmpresa>();

// Clientes
builder.Services.AddScoped<Ssal.Application.Clientes.IClienteRepository, Ssal.Infrastructure.Repositorios.ClienteRepository>();
builder.Services.AddScoped<Ssal.Application.Clientes.ListarClientes>();
builder.Services.AddScoped<Ssal.Application.Clientes.ObtenerCliente>();
builder.Services.AddScoped<Ssal.Application.Clientes.CrearCliente>();
builder.Services.AddScoped<Ssal.Application.Clientes.ActualizarCliente>();

// Consultas
builder.Services.AddScoped<Ssal.Application.Consultas.IConsultaRepository, Ssal.Infrastructure.Repositorios.ConsultaRepository>();
builder.Services.AddScoped<Ssal.Application.Consultas.ListarConsultas>();
builder.Services.AddScoped<Ssal.Application.Consultas.ObtenerConsulta>();
builder.Services.AddScoped<Ssal.Application.Consultas.CrearConsulta>();
builder.Services.AddScoped<Ssal.Application.Consultas.ActualizarConsulta>();

// Facturas
builder.Services.AddScoped<Ssal.Application.Facturas.IFacturaRepository, Ssal.Infrastructure.Repositorios.FacturaRepository>();
builder.Services.AddScoped<Ssal.Application.Facturas.ListarFacturas>();
builder.Services.AddScoped<Ssal.Application.Facturas.ListarSsesDisponibles>();
builder.Services.AddScoped<Ssal.Application.Facturas.RegistrarFactura>();
builder.Services.AddScoped<Ssal.Application.Facturas.MarcarFacturaPagada>();

// Usuarios
builder.Services.AddScoped<Ssal.Application.Comun.IHashDeContrasenas, Ssal.Infrastructure.HashDeContrasenasBCrypt>();
builder.Services.AddScoped<Ssal.Application.Usuarios.IUsuarioRepository, Ssal.Infrastructure.Repositorios.UsuarioRepository>();
builder.Services.AddScoped<Ssal.Application.Usuarios.ListarRoles>();
builder.Services.AddScoped<Ssal.Application.Usuarios.ListarUsuarios>();
builder.Services.AddScoped<Ssal.Application.Usuarios.CrearUsuario>();
builder.Services.AddScoped<Ssal.Application.Usuarios.ActualizarUsuario>();
builder.Services.AddScoped<Ssal.Application.Usuarios.DesactivarUsuario>();
builder.Services.AddScoped<Ssal.Application.Usuarios.ActivarUsuario>();
builder.Services.AddScoped<Ssal.Application.Usuarios.RestablecerPassword>();

// Auth
builder.Services.AddScoped<Ssal.Application.Auth.IAuthRepository, Ssal.Infrastructure.Repositorios.AuthRepository>();
builder.Services.AddScoped<Ssal.Application.Auth.IGeneradorToken, Ssal.Infrastructure.GeneradorTokenJwt>();
builder.Services.AddScoped<Ssal.Application.Auth.IniciarSesion>();
builder.Services.AddScoped<Ssal.Application.Auth.CambiarPassword>();

// Acceso al portal del cliente (lo administra el personal desde la ficha del cliente)
builder.Services.AddScoped<Ssal.Application.PortalAccesos.IPortalAccesoRepository, Ssal.Infrastructure.Repositorios.PortalAccesoRepository>();
builder.Services.AddScoped<Ssal.Application.PortalAccesos.ObtenerEstadoAcceso>();
builder.Services.AddScoped<Ssal.Application.PortalAccesos.GenerarOrestablecerAcceso>();

// Infraestructura compartida de correo (ahora en Ssal.Infrastructure)
builder.Services.AddScoped<Ssal.Application.Comun.ICifradoContrasenas, Ssal.Infrastructure.CifradoContrasenasAes>();
builder.Services.AddScoped<Ssal.Application.Comun.IEnvioCorreos, Ssal.Infrastructure.EnvioCorreos>();

// Presupuestos
builder.Services.AddScoped<Ssal.Application.Presupuestos.IPresupuestoRepository, Ssal.Infrastructure.Repositorios.PresupuestoRepository>();
builder.Services.AddScoped<Ssal.Application.Presupuestos.IPresupuestoPdfGenerator, Ssal.Infrastructure.PresupuestoPdfGenerator>();
builder.Services.AddScoped<Ssal.Application.Presupuestos.ListarPresupuestos>();
builder.Services.AddScoped<Ssal.Application.Presupuestos.ObtenerPresupuesto>();
builder.Services.AddScoped<Ssal.Application.Presupuestos.CrearPresupuesto>();
builder.Services.AddScoped<Ssal.Application.Presupuestos.DescargarPresupuestoPdf>();
builder.Services.AddScoped<Ssal.Application.Presupuestos.EnviarPresupuesto>();
builder.Services.AddScoped<Ssal.Application.Presupuestos.CambiarEstadoPresupuesto>();
builder.Services.AddScoped<Ssal.Application.Presupuestos.ListarArchivosPresupuesto>();
builder.Services.AddScoped<Ssal.Application.Presupuestos.SubirArchivoPresupuesto>();
builder.Services.AddScoped<Ssal.Application.Presupuestos.DescargarArchivoPresupuesto>();
builder.Services.AddScoped<Ssal.Application.Presupuestos.EliminarArchivoPresupuesto>();

// SSE / Rótulos
builder.Services.AddScoped<Ssal.Application.Sse.ISseRepository, Ssal.Infrastructure.Repositorios.SseRepository>();
builder.Services.AddScoped<Ssal.Application.Sse.ISsePdfGenerator, Ssal.Infrastructure.SsePdfGenerator>();
builder.Services.AddScoped<Ssal.Application.Sse.ListarSse>();
builder.Services.AddScoped<Ssal.Application.Sse.ObtenerSse>();
builder.Services.AddScoped<Ssal.Application.Sse.CrearSse>();
builder.Services.AddScoped<Ssal.Application.Sse.AsignarClienteSse>();
builder.Services.AddScoped<Ssal.Application.Sse.DescargarSsePdf>();
builder.Services.AddScoped<Ssal.Application.Sse.CambiarEstadoSse>();
builder.Services.AddScoped<Ssal.Application.Sse.AsignarRotulo>();
builder.Services.AddScoped<Ssal.Application.Sse.RegistrarMuestra>();
builder.Services.AddScoped<Ssal.Application.Sse.ActualizarRotulo>();

// Resultados
builder.Services.AddScoped<Ssal.Application.Resultados.IResultadoRepository, Ssal.Infrastructure.Repositorios.ResultadoRepository>();
builder.Services.AddScoped<Ssal.Application.Resultados.IInformePdfGenerator, Ssal.Infrastructure.InformePdfGenerator>();
builder.Services.AddScoped<Ssal.Application.Resultados.ListarMuestras>();
builder.Services.AddScoped<Ssal.Application.Resultados.ObtenerMuestra>();
builder.Services.AddScoped<Ssal.Application.Resultados.GuardarInforme>();
builder.Services.AddScoped<Ssal.Application.Resultados.DescargarInformePdf>();
builder.Services.AddScoped<Ssal.Application.Resultados.CargarValorResultado>();
builder.Services.AddScoped<Ssal.Application.Resultados.ValidarResultado>();

// Firmas
builder.Services.AddScoped<Ssal.Application.Firmas.IFirmaRepository, Ssal.Infrastructure.Repositorios.FirmaRepository>();
builder.Services.AddScoped<Ssal.Application.Firmas.CargadorEstadoDocumento>();
builder.Services.AddScoped<Ssal.Application.Firmas.VerificadorPasswordUsuario>();
builder.Services.AddScoped<Ssal.Application.Firmas.ListarFirmas>();
builder.Services.AddScoped<Ssal.Application.Firmas.ObtenerFirma>();
builder.Services.AddScoped<Ssal.Application.Firmas.FirmarDocumento>();
builder.Services.AddScoped<Ssal.Application.Firmas.AprobarDocumento>();
builder.Services.AddScoped<Ssal.Application.Firmas.RechazarDocumento>();
builder.Services.AddScoped<Ssal.Application.Firmas.ReenviarDocumento>();

// Correo entrante
builder.Services.AddScoped<Ssal.Application.Correos.ICorreoEntranteRepository, Ssal.Infrastructure.Repositorios.CorreoEntranteRepository>();
builder.Services.AddScoped<Ssal.Application.Correos.ListarCorreos>();
builder.Services.AddScoped<Ssal.Application.Correos.ObtenerCorreo>();
builder.Services.AddScoped<Ssal.Application.Correos.ResponderCorreo>();
builder.Services.AddScoped<Ssal.Application.Correos.DescartarCorreo>();
builder.Services.AddScoped<Ssal.Application.Correos.ConvertirCorreo>();

// Cuentas de correo
builder.Services.AddScoped<Ssal.Application.CuentasCorreo.ICuentaCorreoRepository, Ssal.Infrastructure.Repositorios.CuentaCorreoRepository>();
builder.Services.AddScoped<Ssal.Application.CuentasCorreo.IRevisorDeCuentaCorreo, Ssal.Infrastructure.RevisorDeCuentaCorreo>();
builder.Services.AddScoped<Ssal.Application.CuentasCorreo.ListarCuentasCorreo>();
builder.Services.AddScoped<Ssal.Application.CuentasCorreo.CrearCuentaCorreo>();
builder.Services.AddScoped<Ssal.Application.CuentasCorreo.ActualizarCuentaCorreo>();
builder.Services.AddScoped<Ssal.Application.CuentasCorreo.RevisarCuentaAhora>();
builder.Services.AddScoped<Ssal.Application.CuentasCorreo.EliminarCuentaCorreo>();

// Portal del cliente externo
builder.Services.AddScoped<Ssal.Application.Portal.IPortalRepository, Ssal.Infrastructure.Repositorios.PortalRepository>();
builder.Services.AddScoped<Ssal.Application.Portal.IGeneradorTokenPortal, Ssal.Infrastructure.GeneradorTokenJwtPortal>();
builder.Services.AddScoped<Ssal.Application.Portal.IniciarSesionPortal>();
builder.Services.AddScoped<Ssal.Application.Portal.CambiarPasswordPortal>();
builder.Services.AddScoped<Ssal.Application.Portal.ListarPedidos>();
builder.Services.AddScoped<Ssal.Application.Portal.ObtenerInformeDetalle>();
builder.Services.AddScoped<Ssal.Application.Portal.DescargarInformePortalPdf>();

var app = builder.Build();

// Aplica migraciones y seed inicial
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<SsalDbContext>();
    db.Database.Migrate();

    var seedUsuarios = new (string Email, string Nombre, string Apellido, int IdRol, string Password)[]
    {
        ("admin@ssal.uccuyo.edu.ar",            "Administrador", "Sistema",   1, "Admin1234!"),
        ("administrativo@ssal.uccuyo.edu.ar",   "María",         "González",  2, "Ssal1234!"),
        ("presupuestos@ssal.uccuyo.edu.ar",     "Carlos",        "López",     3, "Ssal1234!"),
        ("cobranzas@ssal.uccuyo.edu.ar",        "Laura",         "Martínez",  4, "Ssal1234!"),
        ("cliente@ssal.uccuyo.edu.ar",          "Juan",          "Pérez",     5, "Ssal1234!"),
        ("sysadmin@ssal.uccuyo.edu.ar",         "Dev",           "Admin",     6, "Ssal1234!"),
        ("analista@ssal.uccuyo.edu.ar",         "Sofía",         "Torres",    7, "Ssal1234!"),
        ("resp.mic@ssal.uccuyo.edu.ar",         "Roberto",       "Díaz",      8, "Ssal1234!"),
        ("resp.fq@ssal.uccuyo.edu.ar",          "Ana",           "Vargas",    9, "Ssal1234!"),
    };

    foreach (var u in seedUsuarios)
    {
        if (!db.Usuarios.Any(x => x.Email == u.Email))
        {
            db.Usuarios.Add(new Ssal.Domain.Usuario
            {
                IdRol = u.IdRol,
                Nombre = u.Nombre,
                Apellido = u.Apellido,
                Email = u.Email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(u.Password),
                Activo = true
            });
        }
    }
    db.SaveChanges();

    // Seed: grupos y análisis de referencia
    if (!db.GruposAnalisis.Any())
    {
        var gruposMic = new[]
        {
            new Ssal.Domain.GrupoAnalisis
            {
                CodigoAgrupador = "Recuentos generales",
                Descripcion = "Recuentos microbiológicos de rutina",
                Area = "MIC",
                Analisis =
                [
                    new() { Codigo = "MIC-001", Nombre = "Aerobios mesófilos en placa (UFC/g)", Area = "MIC", PrecioUsd = 8.00m },
                    new() { Codigo = "MIC-002", Nombre = "Coliformes totales (NMP/g o UFC/g)", Area = "MIC", PrecioUsd = 8.00m },
                    new() { Codigo = "MIC-003", Nombre = "Coliformes fecales / E. coli (NMP/g)", Area = "MIC", PrecioUsd = 9.00m },
                    new() { Codigo = "MIC-004", Nombre = "Hongos y levaduras (UFC/g)", Area = "MIC", PrecioUsd = 9.00m },
                    new() { Codigo = "MIC-005", Nombre = "Staphylococcus aureus coagulasa (+) (UFC/g)", Area = "MIC", PrecioUsd = 10.00m },
                    new() { Codigo = "MIC-006", Nombre = "Psicrófilos (UFC/g)", Area = "MIC", PrecioUsd = 9.00m },
                    new() { Codigo = "MIC-007", Nombre = "Anaerobios esporulados sulfito-reductores", Area = "MIC", PrecioUsd = 11.00m },
                ]
            },
            new Ssal.Domain.GrupoAnalisis
            {
                CodigoAgrupador = "Patógenos específicos",
                Descripcion = "Detección e identificación de patógenos de interés sanitario",
                Area = "MIC",
                Analisis =
                [
                    new() { Codigo = "MIC-101", Nombre = "Salmonella spp. (ausencia/25 g)", Area = "MIC", PrecioUsd = 22.00m },
                    new() { Codigo = "MIC-102", Nombre = "Listeria monocytogenes (ausencia/25 g)", Area = "MIC", PrecioUsd = 25.00m },
                    new() { Codigo = "MIC-103", Nombre = "E. coli O157:H7 (ausencia/25 g)", Area = "MIC", PrecioUsd = 22.00m },
                    new() { Codigo = "MIC-104", Nombre = "Campylobacter spp. (ausencia/25 g)", Area = "MIC", PrecioUsd = 25.00m },
                ]
            },
        };

        var gruposFq = new[]
        {
            new Ssal.Domain.GrupoAnalisis
            {
                CodigoAgrupador = "Composición proximal",
                Descripcion = "Determinaciones de composición nutricional",
                Area = "FQ",
                Analisis =
                [
                    new() { Codigo = "FQ-001", Nombre = "Humedad (%)", Area = "FQ", PrecioUsd = 7.00m },
                    new() { Codigo = "FQ-002", Nombre = "Cenizas totales (%)", Area = "FQ", PrecioUsd = 7.00m },
                    new() { Codigo = "FQ-003", Nombre = "Proteínas totales (N×6.25) (%)", Area = "FQ", PrecioUsd = 9.00m },
                    new() { Codigo = "FQ-004", Nombre = "Extracto etéreo — Grasas totales (%)", Area = "FQ", PrecioUsd = 9.00m },
                    new() { Codigo = "FQ-005", Nombre = "Fibra dietaria total (%)", Area = "FQ", PrecioUsd = 15.00m },
                    new() { Codigo = "FQ-006", Nombre = "Hidratos de carbono — por diferencia (%)", Area = "FQ", PrecioUsd = 5.00m },
                ]
            },
            new Ssal.Domain.GrupoAnalisis
            {
                CodigoAgrupador = "Parámetros fisicoquímicos",
                Descripcion = "Determinaciones fisicoquímicas de proceso y calidad",
                Area = "FQ",
                Analisis =
                [
                    new() { Codigo = "FQ-101", Nombre = "pH", Area = "FQ", PrecioUsd = 6.00m },
                    new() { Codigo = "FQ-102", Nombre = "Acidez titulable (g ácido láctico/100 g)", Area = "FQ", PrecioUsd = 7.00m },
                    new() { Codigo = "FQ-103", Nombre = "Actividad de agua (Aw)", Area = "FQ", PrecioUsd = 10.00m },
                    new() { Codigo = "FQ-104", Nombre = "Sólidos solubles (°Brix)", Area = "FQ", PrecioUsd = 6.00m },
                    new() { Codigo = "FQ-105", Nombre = "Índice de refracción", Area = "FQ", PrecioUsd = 6.00m },
                    new() { Codigo = "FQ-106", Nombre = "Densidad relativa", Area = "FQ", PrecioUsd = 6.00m },
                    new() { Codigo = "FQ-107", Nombre = "Sólidos totales (%)", Area = "FQ", PrecioUsd = 7.00m },
                ]
            },
            new Ssal.Domain.GrupoAnalisis
            {
                CodigoAgrupador = "Minerales",
                Descripcion = "Determinación de minerales por espectroscopía",
                Area = "FQ",
                Analisis =
                [
                    new() { Codigo = "FQ-201", Nombre = "Sodio (mg/100 g)", Area = "FQ", PrecioUsd = 12.00m },
                    new() { Codigo = "FQ-202", Nombre = "Potasio (mg/100 g)", Area = "FQ", PrecioUsd = 12.00m },
                    new() { Codigo = "FQ-203", Nombre = "Calcio (mg/100 g)", Area = "FQ", PrecioUsd = 12.00m },
                    new() { Codigo = "FQ-204", Nombre = "Hierro (mg/100 g)", Area = "FQ", PrecioUsd = 13.00m },
                ]
            },
            new Ssal.Domain.GrupoAnalisis
            {
                CodigoAgrupador = "Contaminantes inorgánicos",
                Descripcion = "Metales pesados y contaminantes por ICP-OES",
                Area = "FQ",
                Analisis =
                [
                    new() { Codigo = "FQ-301", Nombre = "Plomo — Pb (mg/kg)", Area = "FQ", PrecioUsd = 30.00m },
                    new() { Codigo = "FQ-302", Nombre = "Cadmio — Cd (mg/kg)", Area = "FQ", PrecioUsd = 30.00m },
                    new() { Codigo = "FQ-303", Nombre = "Arsénico — As (mg/kg)", Area = "FQ", PrecioUsd = 30.00m },
                    new() { Codigo = "FQ-304", Nombre = "Mercurio — Hg (mg/kg)", Area = "FQ", PrecioUsd = 35.00m },
                ]
            },
        };

        db.GruposAnalisis.AddRange(gruposMic);
        db.GruposAnalisis.AddRange(gruposFq);
        db.SaveChanges();
    }
}

app.UseCors("FrontendDev");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();
