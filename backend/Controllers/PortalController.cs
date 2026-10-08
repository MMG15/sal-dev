using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Ssal.Api.Data;
using Ssal.Api.Models;
using Ssal.Api.Services;

namespace Ssal.Api.Controllers;

// Portal web del cliente externo (RF-22, CU-07). Solo lectura sobre sus propios pedidos.
// Un informe se muestra y descarga únicamente cuando está aprobado (RF-36).
[ApiController]
[Route("api/portal")]
public class PortalController(SsalDbContext db, IConfiguration config) : ControllerBase
{
    private const string RolPortal = "PORTAL";

    int CuentaActual => int.Parse(
        User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value
        ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");

    // ─── POST /api/portal/login ───────────────────────────────────────────
    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<IActionResult> Login([FromBody] LoginRequest req)
    {
        var email = req.Email.Trim().ToLowerInvariant();
        var cuenta = await db.PortalUsuarios
            .Include(p => p.Cliente)
            .FirstOrDefaultAsync(p => p.Email == email && p.Activo);

        if (cuenta is null || !BCrypt.Net.BCrypt.Verify(req.Password, cuenta.PasswordHash))
            return Unauthorized(new { mensaje = "Credenciales incorrectas." });

        cuenta.UltimoAcceso = DateTime.UtcNow;
        await db.SaveChangesAsync();

        return Ok(new
        {
            token = GenerarToken(cuenta),
            idUsuario = cuenta.IdPortalUsuario,
            nombre = cuenta.Cliente.Nombre,
            apellido = cuenta.Cliente.Apellido,
            email = cuenta.Email,
            rolCodigo = RolPortal,
            rolNombre = "Cliente externo",
            forzarCambioPwd = cuenta.ForzarCambioPwd
        });
    }

    // ─── POST /api/portal/cambiar-password ────────────────────────────────
    [HttpPost("cambiar-password")]
    [Authorize(Roles = RolPortal)]
    public async Task<IActionResult> CambiarPassword([FromBody] CambiarPasswordPortalRequest req)
    {
        var cuenta = await db.PortalUsuarios.FindAsync(CuentaActual);
        if (cuenta is null) return NotFound();

        if (!BCrypt.Net.BCrypt.Verify(req.PasswordActual, cuenta.PasswordHash))
            return BadRequest(new { error = "La contraseña actual no es correcta." });

        if (string.IsNullOrEmpty(req.PasswordNueva) || req.PasswordNueva.Length < 8)
            return BadRequest(new { error = "La nueva contraseña debe tener al menos 8 caracteres." });

        cuenta.PasswordHash = BCrypt.Net.BCrypt.HashPassword(req.PasswordNueva);
        cuenta.ForzarCambioPwd = false;
        await db.SaveChangesAsync();
        return NoContent();
    }

    // ─── GET /api/portal/pedidos ──────────────────────────────────────────
    // Rótulos del cliente con su estado: en análisis, disponible o rechazada.
    [HttpGet("pedidos")]
    [Authorize(Roles = RolPortal)]
    public async Task<IActionResult> Pedidos()
    {
        var idCliente = await ClienteActual();
        if (idCliente is null) return NotFound();

        var sseIds = SsesDelCliente(idCliente.Value).Select(s => s.IdSse);

        var muestras = await db.Muestras
            .Include(m => m.RotuloInterno).ThenInclude(r => r.Sse)
            .Include(m => m.Informe)
            .Include(m => m.Resultados).ThenInclude(r => r.Analisis)
            .Where(m => sseIds.Contains(m.RotuloInterno.IdSse))
            .OrderByDescending(m => m.FechaRecepcion)
            .ToListAsync();

        var firmas = await FirmasDeInformes(muestras.Where(m => m.Informe is not null).Select(m => m.Informe!.IdInforme));

        var items = muestras.Select(m =>
        {
            var areas = m.Resultados.Select(r => r.Analisis.Area).Distinct().ToList();
            var aprobado = m.Informe is not null && EstaAprobado(m.Informe.IdInforme, areas, firmas);

            var estado = m.Estado == "rechazada" ? "rechazada"
                : aprobado ? "disponible"
                : "en_analisis";

            return new
            {
                idMuestra = m.IdMuestra,
                numeroRotulo = m.RotuloInterno.NumeroUnico,
                codigoSse = m.RotuloInterno.Sse.Codigo,
                fechaRecepcion = m.FechaRecepcion,
                estado,
                informe = aprobado ? new { m.Informe!.IdInforme, m.Informe.Codigo, m.Informe.FechaGeneracion } : null
            };
        });

        return Ok(items);
    }

    // ─── GET /api/portal/informes/{id} ────────────────────────────────────
    [HttpGet("informes/{id:int}")]
    [Authorize(Roles = RolPortal)]
    public async Task<IActionResult> Detalle(int id)
    {
        var informe = await CargarInformeDelCliente(id);
        if (informe is null) return NotFound();

        var firmas = await FirmasDeInformes([id]);
        var areas = informe.Muestra.Resultados.Select(r => r.Analisis.Area).Distinct().ToList();
        if (!EstaAprobado(id, areas, firmas))
            return StatusCode(StatusCodes.Status403Forbidden, new { error = "Este informe todavía no está disponible." });

        return Ok(new
        {
            informe.IdInforme,
            informe.Codigo,
            fecha = informe.FechaGeneracion,
            rotulo = informe.Muestra.RotuloInterno.NumeroUnico,
            codigoSse = informe.Muestra.RotuloInterno.Sse.Codigo,
            resultados = informe.Muestra.Resultados
                .OrderBy(r => r.Analisis.Area)
                .ThenBy(r => r.Analisis.Nombre)
                .Select(r => new
                {
                    r.Analisis.Area,
                    analisis = r.Analisis.Nombre,
                    valor = r.Valor,
                    unidad = r.Analisis.Unidad,
                    referencia = ReferenciaTexto(r.Analisis)
                })
        });
    }

    // ─── GET /api/portal/informes/{id}/pdf ────────────────────────────────
    [HttpGet("informes/{id:int}/pdf")]
    [Authorize(Roles = RolPortal)]
    public async Task<IActionResult> Pdf(int id)
    {
        var informe = await db.Informes
            .Include(i => i.Muestra).ThenInclude(m => m.RotuloInterno).ThenInclude(r => r.Sse).ThenInclude(s => s.Cliente!.Empresa)
            .Include(i => i.Muestra).ThenInclude(m => m.RotuloInterno).ThenInclude(r => r.Sse).ThenInclude(s => s.Presupuesto).ThenInclude(p => p!.Consulta).ThenInclude(c => c.Empresa)
            .Include(i => i.Muestra).ThenInclude(m => m.Resultados).ThenInclude(r => r.Analisis).ThenInclude(a => a.Grupo)
            .Include(i => i.GeneradoPorUsuario)
            .FirstOrDefaultAsync(i => i.IdInforme == id);
        if (informe is null) return NotFound();

        var idCliente = await ClienteActual();
        if (idCliente is null || !await PerteneceAlCliente(informe.Muestra.RotuloInterno.IdSse, idCliente.Value))
            return NotFound();

        var eventos = await db.FirmasDocumento
            .Include(f => f.Firmante)
            .Where(f => f.TipoDocumento == "informe" && f.IdDocumento == id)
            .OrderBy(f => f.IdFirma)
            .ToListAsync();
        var areas = informe.Muestra.Resultados.Select(r => r.Analisis.Area).Distinct().ToList();
        var ciclo = CicloFirma.Calcular(eventos, areas);
        if (ciclo.Estado != CicloFirma.Aprobado)
            return StatusCode(StatusCodes.Status403Forbidden, new { error = "Este informe todavía no está disponible." });

        var pdf = InformePdfBuilder.Generar(informe, ciclo.Estado, CicloFirma.EventosCicloActual(eventos));
        return File(pdf, "application/pdf", $"{informe.Codigo}.pdf");
    }

    // ─── Helpers ──────────────────────────────────────────────────────────

    private async Task<int?> ClienteActual()
    {
        var cuenta = await db.PortalUsuarios.FindAsync(CuentaActual);
        return cuenta?.IdCliente;
    }

    // SSE del cliente: por cliente asignado a la SSE o por la consulta del presupuesto
    private IQueryable<Sse> SsesDelCliente(int idCliente) =>
        db.Sses.Where(s => s.IdCliente == idCliente
            || (s.Presupuesto != null && s.Presupuesto.Consulta.IdCliente == idCliente));

    private async Task<bool> PerteneceAlCliente(int idSse, int idCliente) =>
        await SsesDelCliente(idCliente).AnyAsync(s => s.IdSse == idSse);

    private async Task<Informe?> CargarInformeDelCliente(int id)
    {
        var informe = await db.Informes
            .Include(i => i.Muestra).ThenInclude(m => m.RotuloInterno).ThenInclude(r => r.Sse)
            .Include(i => i.Muestra).ThenInclude(m => m.Resultados).ThenInclude(r => r.Analisis)
            .FirstOrDefaultAsync(i => i.IdInforme == id);
        if (informe is null) return null;

        var idCliente = await ClienteActual();
        if (idCliente is null || !await PerteneceAlCliente(informe.Muestra.RotuloInterno.IdSse, idCliente.Value))
            return null;

        return informe;
    }

    private async Task<List<FirmaDocumento>> FirmasDeInformes(IEnumerable<int> idsInformes)
    {
        var ids = idsInformes.ToList();
        return await db.FirmasDocumento
            .Where(f => f.TipoDocumento == "informe" && ids.Contains(f.IdDocumento))
            .OrderBy(f => f.IdFirma)
            .ToListAsync();
    }

    private static bool EstaAprobado(int idInforme, List<string> areas, List<FirmaDocumento> firmas) =>
        CicloFirma.Calcular(firmas.Where(f => f.IdDocumento == idInforme), areas).Estado == CicloFirma.Aprobado;

    private static string? ReferenciaTexto(Analisis a)
    {
        if (a.ValorEsperado is not null) return $"Esperado: {a.ValorEsperado}";
        if (a.RangoMin is not null || a.RangoMax is not null)
            return $"{a.RangoMin?.ToString() ?? "—"} a {a.RangoMax?.ToString() ?? "—"}";
        return null;
    }

    private string GenerarToken(PortalUsuario cuenta)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(config["Jwt:Key"]!));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, cuenta.IdPortalUsuario.ToString()),
            new Claim(JwtRegisteredClaimNames.Email, cuenta.Email),
            new Claim(ClaimTypes.Role, RolPortal),
            new Claim("nombre", cuenta.Cliente.Nombre),
            new Claim("apellido", cuenta.Cliente.Apellido),
        };

        var token = new JwtSecurityToken(
            issuer: config["Jwt:Issuer"],
            audience: config["Jwt:Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(double.Parse(config["Jwt:ExpiresInMinutes"]!)),
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}

public record CambiarPasswordPortalRequest(string PasswordActual, string? PasswordNueva);
