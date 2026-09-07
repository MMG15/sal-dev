using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Ssal.Api.Data;
using Ssal.Api.Models;

namespace Ssal.Api.Controllers;

[ApiController]
[Route("api/presupuestos")]
[Authorize]
public class PresupuestosController(SsalDbContext db) : ControllerBase
{
    // ─── POST /api/presupuestos ───────────────────────────────────────────
    // Crea o reemplaza el presupuesto de una consulta
    [HttpPost]
    public async Task<IActionResult> Crear([FromBody] GuardarPresupuestoRequest req)
    {
        // Verificar que la consulta existe
        var consulta = await db.Consultas.FindAsync(req.IdConsulta);
        if (consulta is null) return BadRequest(new { error = "Consulta no encontrada" });

        // Si ya existe uno para esta consulta, eliminarlo
        var existente = await db.Presupuestos
            .Include(p => p.Items)
            .FirstOrDefaultAsync(p => p.IdConsulta == req.IdConsulta);

        if (existente is not null)
        {
            db.RemoveRange(existente.Items);
            db.Remove(existente);
            await db.SaveChangesAsync();
        }

        // Obtener precios actuales de los análisis seleccionados
        var ids = req.Items.Select(i => i.IdAnalisis).ToList();
        var analisis = await db.Analisis
            .Where(a => ids.Contains(a.IdAnalisis))
            .ToDictionaryAsync(a => a.IdAnalisis);

        var items = req.Items.Select(i => new PresupuestoAnalisis
        {
            IdAnalisis = i.IdAnalisis,
            PrecioUsdSnapshot = analisis.TryGetValue(i.IdAnalisis, out var a) ? a.PrecioUsd : 0
        }).ToList();

        var totalUsd = items.Sum(i => i.PrecioUsdSnapshot) + req.AdicionalAsesoramiento;
        var cotizacion = req.Cotizacion > 0 ? req.Cotizacion : 1;

        // Generar código RPO
        var hoy = DateTime.UtcNow;
        var prefijo = $"RPO-{hoy:yyyyMMdd}-";
        var ultimoCodigo = await db.Presupuestos
            .Where(p => p.CodigoRpo != null && p.CodigoRpo.StartsWith(prefijo))
            .OrderByDescending(p => p.CodigoRpo)
            .Select(p => p.CodigoRpo)
            .FirstOrDefaultAsync();

        int siguiente = 1;
        if (ultimoCodigo is not null)
        {
            var partes = ultimoCodigo.Split('-');
            if (partes.Length == 3 && int.TryParse(partes[2], out var n))
                siguiente = n + 1;
        }

        var subClaim = User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value
            ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value
            ?? "0";
        var idUsuario = int.Parse(subClaim);

        var presupuesto = new Presupuesto
        {
            IdConsulta = req.IdConsulta,
            CreadoPor = idUsuario,
            CodigoRpo = $"{prefijo}{siguiente:D3}",
            ImporteDolares = totalUsd,
            ImportePesos = totalUsd * cotizacion,
            Cotizacion = cotizacion,
            AdicionalAsesoramiento = req.AdicionalAsesoramiento,
            Estado = "borrador",
            Fecha = hoy,
            Items = items
        };

        db.Presupuestos.Add(presupuesto);
        await db.SaveChangesAsync();

        return Ok(new { presupuesto.IdPresupuesto, presupuesto.CodigoRpo, presupuesto.ImporteDolares, presupuesto.ImportePesos });
    }

    // ─── GET /api/presupuestos ────────────────────────────────────────────
    [HttpGet]
    public async Task<IActionResult> Listar(
        [FromQuery] string? busqueda,
        [FromQuery] string? estado,
        [FromQuery] int pagina = 1,
        [FromQuery] int porPagina = 20)
    {
        var q = db.Presupuestos
            .Include(p => p.Consulta).ThenInclude(c => c.Cliente)
            .Include(p => p.Consulta).ThenInclude(c => c.Empresa)
            .Include(p => p.CreadoPorUsuario)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(estado))
            q = q.Where(p => p.Estado == estado);

        if (!string.IsNullOrWhiteSpace(busqueda))
        {
            var b = busqueda.ToLower();
            q = q.Where(p =>
                (p.CodigoRpo != null && p.CodigoRpo.ToLower().Contains(b)) ||
                (p.Consulta.Codigo != null && p.Consulta.Codigo.ToLower().Contains(b)) ||
                (p.Consulta.Cliente != null && (p.Consulta.Cliente.Nombre + " " + p.Consulta.Cliente.Apellido).ToLower().Contains(b)) ||
                (p.Consulta.Empresa != null && p.Consulta.Empresa.RazonSocial.ToLower().Contains(b)));
        }

        var total = await q.CountAsync();
        var items = await q
            .OrderByDescending(p => p.Fecha)
            .Skip((pagina - 1) * porPagina)
            .Take(porPagina)
            .Select(p => new
            {
                p.IdPresupuesto,
                p.CodigoRpo,
                p.Estado,
                p.ImporteDolares,
                p.ImportePesos,
                p.Cotizacion,
                p.Fecha,
                ConsultaCodigo = p.Consulta.Codigo,
                IdConsulta = p.Consulta.IdConsulta,
                Cliente = p.Consulta.Cliente == null ? null
                    : new { p.Consulta.Cliente.Nombre, p.Consulta.Cliente.Apellido, p.Consulta.Cliente.Email },
                Empresa = p.Consulta.Empresa == null ? null
                    : new { p.Consulta.Empresa.RazonSocial },
                CreadoPor = new { p.CreadoPorUsuario.Nombre, p.CreadoPorUsuario.Apellido }
            })
            .ToListAsync();

        return Ok(new { total, pagina, porPagina, items });
    }

    // ─── GET /api/presupuestos/{id} ───────────────────────────────────────
    [HttpGet("{id}")]
    public async Task<IActionResult> Detalle(int id)
    {
        var p = await db.Presupuestos
            .Include(x => x.Consulta).ThenInclude(c => c.Cliente).ThenInclude(cl => cl!.Empresa)
            .Include(x => x.Consulta).ThenInclude(c => c.Empresa)
            .Include(x => x.CreadoPorUsuario)
            .Include(x => x.Items).ThenInclude(i => i.Analisis).ThenInclude(a => a.Grupo)
            .FirstOrDefaultAsync(x => x.IdPresupuesto == id);

        if (p is null) return NotFound();

        return Ok(new
        {
            p.IdPresupuesto,
            p.CodigoRpo,
            p.Estado,
            p.ImporteDolares,
            p.ImportePesos,
            p.Cotizacion,
            p.AdicionalAsesoramiento,
            p.Fecha,
            Consulta = new
            {
                p.Consulta.IdConsulta,
                p.Consulta.Codigo,
                p.Consulta.Canal,
                p.Consulta.Descripcion,
                Cliente = p.Consulta.Cliente == null ? null : new
                {
                    p.Consulta.Cliente.IdCliente,
                    p.Consulta.Cliente.Nombre,
                    p.Consulta.Cliente.Apellido,
                    p.Consulta.Cliente.Email,
                    p.Consulta.Cliente.Telefono,
                    Empresa = p.Consulta.Cliente.Empresa == null ? null
                        : new { p.Consulta.Cliente.Empresa.IdEmpresa, p.Consulta.Cliente.Empresa.RazonSocial }
                },
                Empresa = p.Consulta.Empresa == null ? null
                    : new { p.Consulta.Empresa.IdEmpresa, p.Consulta.Empresa.RazonSocial }
            },
            CreadoPor = new { p.CreadoPorUsuario.Nombre, p.CreadoPorUsuario.Apellido },
            Items = p.Items
                .OrderBy(i => i.Analisis.Area)
                .ThenBy(i => i.Analisis.Grupo.CodigoAgrupador)
                .Select(i => new
                {
                    i.Id,
                    i.IdAnalisis,
                    i.PrecioUsdSnapshot,
                    i.Analisis.Codigo,
                    i.Analisis.Nombre,
                    i.Analisis.Area,
                    GrupoNombre = i.Analisis.Grupo.CodigoAgrupador
                })
        });
    }

    // ─── PUT /api/presupuestos/{id}/estado ───────────────────────────────
    [HttpPut("{id}/estado")]
    public async Task<IActionResult> CambiarEstado(int id, [FromBody] CambiarEstadoRequest req)
    {
        var p = await db.Presupuestos.FindAsync(id);
        if (p is null) return NotFound();
        p.Estado = req.Estado;
        await db.SaveChangesAsync();
        return NoContent();
    }

    // ─── GET /api/presupuestos/{id}/archivos ─────────────────────────────
    [HttpGet("{id}/archivos")]
    public async Task<IActionResult> ListarArchivos(int id)
    {
        var archivos = await db.PresupuestosArchivos
            .Where(a => a.IdPresupuesto == id)
            .OrderByDescending(a => a.SubidoEn)
            .Select(a => new
            {
                a.Id,
                a.NombreOriginal,
                a.TipoMime,
                a.TamañoBytes,
                a.SubidoEn,
                SubidoPor = a.SubidoPorUsuario.Nombre + " " + a.SubidoPorUsuario.Apellido
            })
            .ToListAsync();

        return Ok(archivos);
    }

    // ─── POST /api/presupuestos/{id}/archivos ────────────────────────────
    [HttpPost("{id}/archivos")]
    [RequestSizeLimit(20 * 1024 * 1024)] // 20 MB
    public async Task<IActionResult> SubirArchivo(int id, IFormFile archivo, IConfiguration config)
    {
        if (archivo is null || archivo.Length == 0)
            return BadRequest(new { error = "No se recibió ningún archivo." });

        var extensionesPermitidas = new[] { ".pdf", ".doc", ".docx", ".xls", ".xlsx", ".png", ".jpg", ".jpeg", ".zip" };
        var ext = Path.GetExtension(archivo.FileName).ToLowerInvariant();
        if (!extensionesPermitidas.Contains(ext))
            return BadRequest(new { error = $"Tipo de archivo no permitido ({ext})." });

        var presupuesto = await db.Presupuestos.FindAsync(id);
        if (presupuesto is null) return NotFound();

        var basePath = config["Storage:BasePath"] ?? "storage";
        var carpeta = Path.Combine(Directory.GetCurrentDirectory(), basePath, "presupuestos", id.ToString());
        Directory.CreateDirectory(carpeta);

        var nombreAlmacenado = $"{Guid.NewGuid()}{ext}";
        var rutaCompleta = Path.Combine(carpeta, nombreAlmacenado);

        using (var stream = System.IO.File.Create(rutaCompleta))
            await archivo.CopyToAsync(stream);

        var subClaim = User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value
            ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0";

        var registro = new Ssal.Api.Models.PresupuestoArchivo
        {
            IdPresupuesto = id,
            NombreOriginal = archivo.FileName,
            NombreAlmacenado = nombreAlmacenado,
            TipoMime = archivo.ContentType,
            TamañoBytes = archivo.Length,
            SubidoPor = int.Parse(subClaim),
            SubidoEn = DateTime.UtcNow
        };

        db.PresupuestosArchivos.Add(registro);
        await db.SaveChangesAsync();

        return Ok(new { registro.Id, registro.NombreOriginal, registro.TamañoBytes });
    }

    // ─── GET /api/presupuestos/{id}/archivos/{archivoId} ─────────────────
    [HttpGet("{id}/archivos/{archivoId}")]
    public async Task<IActionResult> DescargarArchivo(int id, int archivoId, IConfiguration config)
    {
        var archivo = await db.PresupuestosArchivos
            .FirstOrDefaultAsync(a => a.Id == archivoId && a.IdPresupuesto == id);
        if (archivo is null) return NotFound();

        var basePath = config["Storage:BasePath"] ?? "storage";
        var ruta = Path.Combine(Directory.GetCurrentDirectory(), basePath, "presupuestos", id.ToString(), archivo.NombreAlmacenado);

        if (!System.IO.File.Exists(ruta)) return NotFound(new { error = "Archivo no encontrado en disco." });

        var bytes = await System.IO.File.ReadAllBytesAsync(ruta);
        return File(bytes, archivo.TipoMime, archivo.NombreOriginal);
    }

    // ─── DELETE /api/presupuestos/{id}/archivos/{archivoId} ──────────────
    [HttpDelete("{id}/archivos/{archivoId}")]
    public async Task<IActionResult> EliminarArchivo(int id, int archivoId, IConfiguration config)
    {
        var archivo = await db.PresupuestosArchivos
            .FirstOrDefaultAsync(a => a.Id == archivoId && a.IdPresupuesto == id);
        if (archivo is null) return NotFound();

        var basePath = config["Storage:BasePath"] ?? "storage";
        var ruta = Path.Combine(Directory.GetCurrentDirectory(), basePath, "presupuestos", id.ToString(), archivo.NombreAlmacenado);

        if (System.IO.File.Exists(ruta))
            System.IO.File.Delete(ruta);

        db.PresupuestosArchivos.Remove(archivo);
        await db.SaveChangesAsync();
        return NoContent();
    }
}

public record ItemPresupuestoRequest(int IdAnalisis);

public record GuardarPresupuestoRequest(
    int IdConsulta,
    List<ItemPresupuestoRequest> Items,
    decimal Cotizacion,
    decimal AdicionalAsesoramiento);

public record CambiarEstadoRequest(string Estado);
