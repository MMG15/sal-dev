using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Ssal.Api.Data;
using Ssal.Api.Models;

namespace Ssal.Api.Controllers;

[ApiController]
[Route("api/sses")]
[Authorize(Roles = "ROL-01,ROL-02,ROL-07")]
public class SseController(SsalDbContext db) : ControllerBase
{
    int UsuarioActual => int.Parse(
        User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value
        ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");

    // ─── GET /api/sses ────────────────────────────────────────────────────
    [HttpGet]
    public async Task<IActionResult> Listar(
        [FromQuery] string? busqueda,
        [FromQuery] string? estado,
        [FromQuery] string? area,
        [FromQuery] int pagina = 1,
        [FromQuery] int porPagina = 20)
    {
        var q = db.Sses
            .Include(s => s.Cliente)
            .Include(s => s.Presupuesto).ThenInclude(p => p!.Consulta).ThenInclude(c => c.Empresa)
            .Include(s => s.RotuloInterno)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(estado))
            q = q.Where(s => s.Estado == estado);

        if (!string.IsNullOrWhiteSpace(area))
            q = q.Where(s => s.Area == area);

        if (!string.IsNullOrWhiteSpace(busqueda))
        {
            var b = busqueda.ToLower();
            q = q.Where(s =>
                (s.Codigo != null && s.Codigo.ToLower().Contains(b)) ||
                (s.CodigoRpo != null && s.CodigoRpo.ToLower().Contains(b)) ||
                (s.Cliente != null && (s.Cliente.Nombre + " " + s.Cliente.Apellido).ToLower().Contains(b)) ||
                (s.RotuloInterno != null && s.RotuloInterno.NumeroUnico.ToLower().Contains(b)));
        }

        var total = await q.CountAsync();
        var items = await q
            .OrderByDescending(s => s.Fecha)
            .Skip((pagina - 1) * porPagina)
            .Take(porPagina)
            .Select(s => new
            {
                s.IdSse,
                s.Codigo,
                s.CodigoRpo,
                s.Area,
                s.Estado,
                s.Fecha,
                s.FormaPago,
                s.ImporteDolares,
                s.ImportePesos,
                Cliente = s.Cliente == null ? null : new { s.Cliente.Nombre, s.Cliente.Apellido, s.Cliente.Email },
                Empresa = s.Presupuesto == null ? null
                    : s.Presupuesto.Consulta.Empresa == null ? null
                    : new { s.Presupuesto.Consulta.Empresa.RazonSocial },
                Rotulo = s.RotuloInterno == null ? null : new { s.RotuloInterno.IdRotulo, s.RotuloInterno.NumeroUnico, s.RotuloInterno.Estado }
            })
            .ToListAsync();

        return Ok(new { total, pagina, porPagina, items });
    }

    // ─── GET /api/sses/{id} ───────────────────────────────────────────────
    [HttpGet("{id}")]
    public async Task<IActionResult> Detalle(int id)
    {
        var s = await db.Sses
            .Include(x => x.Cliente).ThenInclude(c => c!.Empresa)
            .Include(x => x.Presupuesto).ThenInclude(p => p!.Consulta).ThenInclude(c => c.Cliente)
            .Include(x => x.Presupuesto).ThenInclude(p => p!.Consulta).ThenInclude(c => c.Empresa)
            .Include(x => x.Presupuesto).ThenInclude(p => p!.Items).ThenInclude(i => i.Analisis)
            .Include(x => x.CreadoPorUsuario)
            .Include(x => x.RotuloInterno).ThenInclude(r => r!.AsignadoPorUsuario)
            .Include(x => x.RotuloInterno).ThenInclude(r => r!.Muestra).ThenInclude(m => m!.RecibidoPorUsuario)
            .Include(x => x.RotuloInterno).ThenInclude(r => r!.Auditorias).ThenInclude(a => a.Usuario)
            .FirstOrDefaultAsync(x => x.IdSse == id);

        if (s is null) return NotFound();

        return Ok(new
        {
            s.IdSse,
            s.Codigo,
            s.CodigoRpo,
            s.Area,
            s.Estado,
            s.Fecha,
            s.FormaPago,
            s.ImporteDolares,
            s.ImportePesos,
            CreadoPor = new { s.CreadoPorUsuario.Nombre, s.CreadoPorUsuario.Apellido },
            Cliente = s.Cliente == null ? null : new
            {
                s.Cliente.IdCliente,
                s.Cliente.Nombre,
                s.Cliente.Apellido,
                s.Cliente.Email,
                s.Cliente.Telefono,
                Empresa = s.Cliente.Empresa == null ? null : new { s.Cliente.Empresa.RazonSocial }
            },
            Presupuesto = s.Presupuesto == null ? null : new
            {
                s.Presupuesto.IdPresupuesto,
                s.Presupuesto.CodigoRpo,
                s.Presupuesto.ImporteDolares,
                s.Presupuesto.ImportePesos,
                Analisis = s.Presupuesto.Items.Select(i => new { i.Analisis.Nombre, i.Analisis.Area })
            },
            Rotulo = s.RotuloInterno == null ? null : new
            {
                s.RotuloInterno.IdRotulo,
                s.RotuloInterno.NumeroUnico,
                s.RotuloInterno.Descripcion,
                s.RotuloInterno.Estado,
                s.RotuloInterno.FechaAsignacion,
                AsignadoPor = new { s.RotuloInterno.AsignadoPorUsuario.Nombre, s.RotuloInterno.AsignadoPorUsuario.Apellido },
                Muestra = s.RotuloInterno.Muestra == null ? null : new
                {
                    s.RotuloInterno.Muestra.IdMuestra,
                    s.RotuloInterno.Muestra.Tipo,
                    s.RotuloInterno.Muestra.Estado,
                    s.RotuloInterno.Muestra.Observacion,
                    s.RotuloInterno.Muestra.FechaRecepcion,
                    RecibidoPor = new { s.RotuloInterno.Muestra.RecibidoPorUsuario.Nombre, s.RotuloInterno.Muestra.RecibidoPorUsuario.Apellido }
                },
                Auditorias = s.RotuloInterno.Auditorias
                    .OrderByDescending(a => a.FechaCambio)
                    .Select(a => new
                    {
                        a.IdAuditoria,
                        a.Campo,
                        a.ValorAnterior,
                        a.ValorNuevo,
                        a.Motivo,
                        a.FechaCambio,
                        Usuario = new { a.Usuario.Nombre, a.Usuario.Apellido }
                    })
            }
        });
    }

    // ─── POST /api/sses ───────────────────────────────────────────────────
    [HttpPost]
    public async Task<IActionResult> Crear([FromBody] CrearSseRequest req)
    {
        var hoy = DateTime.UtcNow;
        var prefijo = $"SSE-{hoy:yyyyMMdd}-";
        var ultimo = await db.Sses
            .Where(s => s.Codigo != null && s.Codigo.StartsWith(prefijo))
            .OrderByDescending(s => s.Codigo)
            .Select(s => s.Codigo)
            .FirstOrDefaultAsync();

        int siguiente = 1;
        if (ultimo is not null)
        {
            var partes = ultimo.Split('-');
            if (partes.Length == 3 && int.TryParse(partes[2], out var n))
                siguiente = n + 1;
        }

        // Obtener CodigoRpo del presupuesto vinculado
        string? codigoRpo = null;
        if (req.IdPresupuesto.HasValue)
        {
            codigoRpo = await db.Presupuestos
                .Where(p => p.IdPresupuesto == req.IdPresupuesto)
                .Select(p => p.CodigoRpo)
                .FirstOrDefaultAsync();
        }

        var sse = new Sse
        {
            Codigo = $"{prefijo}{siguiente:D3}",
            CodigoRpo = codigoRpo,
            IdPresupuesto = req.IdPresupuesto,
            IdCliente = req.IdCliente,
            CreadoPor = UsuarioActual,
            Area = req.Area,
            FormaPago = req.FormaPago,
            ImporteDolares = req.ImporteDolares,
            ImportePesos = req.ImportePesos,
            Estado = "activa",
            Fecha = hoy
        };

        db.Sses.Add(sse);
        await db.SaveChangesAsync();
        return CreatedAtAction(nameof(Detalle), new { id = sse.IdSse }, new { sse.IdSse, sse.Codigo });
    }

    // ─── PUT /api/sses/{id}/estado ────────────────────────────────────────
    [HttpPut("{id}/estado")]
    public async Task<IActionResult> CambiarEstado(int id, [FromBody] CambiarEstadoSseRequest req)
    {
        var s = await db.Sses.FindAsync(id);
        if (s is null) return NotFound();
        s.Estado = req.Estado;
        await db.SaveChangesAsync();
        return NoContent();
    }

    // ─── POST /api/sses/{id}/rotulo ───────────────────────────────────────
    [HttpPost("{id}/rotulo")]
    public async Task<IActionResult> AsignarRotulo(int id, [FromBody] AsignarRotuloRequest req)
    {
        var sse = await db.Sses.Include(s => s.RotuloInterno).FirstOrDefaultAsync(s => s.IdSse == id);
        if (sse is null) return NotFound();
        if (sse.RotuloInterno is not null)
            return BadRequest(new { error = "Esta SSE ya tiene un rótulo asignado." });

        // Generar NumeroUnico si no se envió
        string numeroUnico = req.NumeroUnico?.Trim() ?? await GenerarNumeroUnico();

        if (await db.RotulosInternos.AnyAsync(r => r.NumeroUnico == numeroUnico))
            return BadRequest(new { error = $"El número de rótulo '{numeroUnico}' ya existe." });

        var rotulo = new RotuloInterno
        {
            IdSse = id,
            NumeroUnico = numeroUnico,
            Descripcion = req.Descripcion,
            Estado = "activo",
            FechaAsignacion = DateTime.UtcNow,
            AsignadoPor = UsuarioActual
        };

        db.RotulosInternos.Add(rotulo);
        await db.SaveChangesAsync();
        return Ok(new { rotulo.IdRotulo, rotulo.NumeroUnico });
    }

    // ─── POST /api/sses/{id}/muestra ─────────────────────────────────────
    [HttpPost("{id}/muestra")]
    public async Task<IActionResult> RegistrarMuestra(int id, [FromBody] RegistrarMuestraRequest req)
    {
        var rotulo = await db.RotulosInternos
            .Include(r => r.Muestra)
            .FirstOrDefaultAsync(r => r.IdSse == id);

        if (rotulo is null)
            return BadRequest(new { error = "Primero asigná un rótulo a esta SSE." });
        if (rotulo.Muestra is not null)
            return BadRequest(new { error = "Esta SSE ya tiene una muestra registrada." });

        // Condición de la muestra al recibirla (PO 04):
        // apta               -> se recibe normalmente
        // no_apta_procesar   -> no está en condiciones pero el cliente pide procesarla igual
        // rechazada          -> no está en condiciones y no se procesa (fin del servicio)
        var rechazada = req.Condicion == "rechazada";
        if (rechazada && string.IsNullOrWhiteSpace(req.Observacion))
            return BadRequest(new { error = "Indicá el motivo del rechazo en la observación." });

        var muestra = new Muestra
        {
            IdRotulo = rotulo.IdRotulo,
            Tipo = req.Tipo,
            Observacion = req.Observacion,
            Estado = rechazada ? "rechazada" : "en_proceso",
            FechaRecepcion = DateTime.UtcNow,
            RecibidoPor = UsuarioActual
        };

        db.Muestras.Add(muestra);

        var sse = await db.Sses
            .Include(s => s.Presupuesto).ThenInclude(p => p!.Items)
            .FirstOrDefaultAsync(s => s.IdSse == id);
        if (sse is not null) sse.Estado = rechazada ? "rechazada" : "analizando";

        // Al recibir la muestra (si no fue rechazada), se abre un resultado pendiente
        // por cada análisis solicitado en el presupuesto vinculado.
        if (!rechazada && sse?.Presupuesto is not null)
        {
            foreach (var item in sse.Presupuesto.Items)
            {
                db.Resultados.Add(new Resultado
                {
                    Muestra = muestra,
                    IdAnalisis = item.IdAnalisis,
                    Estado = "pendiente"
                });
            }
        }

        await db.SaveChangesAsync();
        return Ok(new { muestra.IdMuestra });
    }

    // ─── PUT /api/sses/{id}/rotulo ────────────────────────────────────────
    // Corrige número/descripción/estado del rótulo. Operación crítica (RF-13/RF-14):
    // exige motivo y audita cada campo modificado por separado.
    [HttpPut("{id}/rotulo")]
    public async Task<IActionResult> ActualizarRotulo(int id, [FromBody] ActualizarRotuloRequest req)
    {
        var rotulo = await db.RotulosInternos.FirstOrDefaultAsync(r => r.IdSse == id);
        if (rotulo is null) return NotFound();

        var cambios = new List<AuditoriaRotulo>();

        if (req.NumeroUnico is not null && req.NumeroUnico != rotulo.NumeroUnico)
        {
            var duplicado = await db.RotulosInternos.AnyAsync(r => r.IdRotulo != rotulo.IdRotulo && r.NumeroUnico == req.NumeroUnico);
            if (duplicado) return BadRequest(new { error = $"El número de rótulo '{req.NumeroUnico}' ya existe." });

            cambios.Add(new AuditoriaRotulo { Campo = "numero_unico", ValorAnterior = rotulo.NumeroUnico, ValorNuevo = req.NumeroUnico });
            rotulo.NumeroUnico = req.NumeroUnico;
        }

        if (req.Descripcion is not null && req.Descripcion != rotulo.Descripcion)
        {
            cambios.Add(new AuditoriaRotulo { Campo = "descripcion", ValorAnterior = rotulo.Descripcion, ValorNuevo = req.Descripcion });
            rotulo.Descripcion = req.Descripcion;
        }

        if (req.Estado is not null && req.Estado != rotulo.Estado)
        {
            cambios.Add(new AuditoriaRotulo { Campo = "estado", ValorAnterior = rotulo.Estado, ValorNuevo = req.Estado });
            rotulo.Estado = req.Estado;
        }

        if (cambios.Count == 0) return NoContent();

        if (string.IsNullOrWhiteSpace(req.Motivo))
            return BadRequest(new { error = "Indicá el motivo de la corrección." });

        foreach (var cambio in cambios)
        {
            cambio.IdRotulo = rotulo.IdRotulo;
            cambio.IdUsuario = UsuarioActual;
            cambio.Motivo = req.Motivo;
            cambio.FechaCambio = DateTime.UtcNow;
        }
        db.AuditoriasRotulo.AddRange(cambios);

        await db.SaveChangesAsync();
        return NoContent();
    }

    private async Task<string> GenerarNumeroUnico()
    {
        var prefijo = $"ROT-{DateTime.UtcNow:yyyyMM}-";
        var ultimo = await db.RotulosInternos
            .Where(r => r.NumeroUnico.StartsWith(prefijo))
            .OrderByDescending(r => r.NumeroUnico)
            .Select(r => r.NumeroUnico)
            .FirstOrDefaultAsync();

        int siguiente = 1;
        if (ultimo is not null)
        {
            var partes = ultimo.Split('-');
            if (partes.Length == 3 && int.TryParse(partes[2], out var n))
                siguiente = n + 1;
        }
        return $"{prefijo}{siguiente:D4}";
    }
}

public record CrearSseRequest(
    int? IdPresupuesto,
    int? IdCliente,
    string Area,
    string? FormaPago,
    decimal ImporteDolares,
    decimal ImportePesos);

public record CambiarEstadoSseRequest(string Estado);

public record AsignarRotuloRequest(string? NumeroUnico, string? Descripcion);

public record RegistrarMuestraRequest(string? Tipo, string? Observacion, string Condicion);

public record ActualizarRotuloRequest(string? NumeroUnico, string? Estado, string? Descripcion, string? Motivo);
