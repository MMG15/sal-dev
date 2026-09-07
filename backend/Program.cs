using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using QuestPDF.Infrastructure;
using Ssal.Api.Data;
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
            db.Usuarios.Add(new Ssal.Api.Models.Usuario
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
            new Ssal.Api.Models.GrupoAnalisis
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
            new Ssal.Api.Models.GrupoAnalisis
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
            new Ssal.Api.Models.GrupoAnalisis
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
            new Ssal.Api.Models.GrupoAnalisis
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
            new Ssal.Api.Models.GrupoAnalisis
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
            new Ssal.Api.Models.GrupoAnalisis
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
