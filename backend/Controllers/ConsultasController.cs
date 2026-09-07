using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Ssal.Api.Data;
using Ssal.Api.Models;

namespace Ssal.Api.Controllers;

[ApiController]
[Route("api/consultas")]
[Authorize]
public class ConsultasController(SsalDbContext db) : ControllerBase
{
    /// <summary>Días sin respuesta a partir de los cuales una consulta "recibida" se considera pendiente/demorada.</summary>
    private const int DiasParaPendiente = 2;

    // ─── GET /api/consultas ───────────────────────────────────────────────
    [HttpGet]
    public async Task<IActionResult> Listar(
        [FromQuery] string? busqueda,
        [FromQuery] string? estado,
        [FromQuery] int pagina = 1,
        [FromQuery] int porPagina = 20)
    {
        var q = db.Consultas
            .Include(c => c.Cliente)
            .Include(c => c.Empresa)
            .Include(c => c.Responsable)
            .Include(c => c.Presupuesto)
            .AsQueryable();

        // "pendiente" es un estado virtual: consultas recibidas que llevan varios días sin respuesta.
        if (estado == "pendiente")
        {
            var limite = DateTime.UtcNow.AddDays(-DiasParaPendiente);
            q = q.Where(c => c.Estado == "recibida" && c.CreatedAt <= limite);
        }
        else if (!string.IsNullOrWhiteSpace(estado))
        {
            q = q.Where(c => c.Estado == estado);
        }

        if (!string.IsNullOrWhiteSpace(busqueda))
        {
            var b = busqueda.ToLower();
            q = q.Where(c =>
                (c.Codigo != null && c.Codigo.ToLower().Contains(b)) ||
                (c.Cliente != null && (c.Cliente.Nombre + " " + c.Cliente.Apellido).ToLower().Contains(b)) ||
                (c.Empresa != null && c.Empresa.RazonSocial.ToLower().Contains(b)) ||
                (c.Descripcion != null && c.Descripcion.ToLower().Contains(b)));
        }

        var total = await q.CountAsync();
        var crudo = await q
            .OrderByDescending(c => c.CreatedAt)
            .Skip((pagina - 1) * porPagina)
            .Take(porPagina)
            .Select(c => new
            {
                c.IdConsulta,
                c.Codigo,
                c.Canal,
                c.Estado,
                c.Descripcion,
                c.Fecha,
                c.CreatedAt,
                Cliente = c.Cliente == null ? null : new { c.Cliente.IdCliente, c.Cliente.Nombre, c.Cliente.Apellido, c.Cliente.Email },
                Empresa = c.Empresa == null ? null : new { c.Empresa.IdEmpresa, c.Empresa.RazonSocial },
                Responsable = c.Responsable == null ? null : new { c.Responsable.IdUsuario, c.Responsable.Nombre, c.Responsable.Apellido },
                TienePresupuesto = c.Presupuesto != null,
                EstadoPresupuesto = c.Presupuesto == null ? null : c.Presupuesto.Estado
            })
            .ToListAsync();

        var ahora = DateTime.UtcNow;
        var items = crudo.Select(c => new
        {
            c.IdConsulta,
            c.Codigo,
            c.Canal,
            c.Estado,
            c.Descripcion,
            c.Fecha,
            c.CreatedAt,
            c.Cliente,
            c.Empresa,
            c.Responsable,
            c.TienePresupuesto,
            c.EstadoPresupuesto,
            DiasSinResponder = (int)(ahora - c.CreatedAt).TotalDays,
            Pendiente = c.Estado == "recibida" && (ahora - c.CreatedAt).TotalDays >= DiasParaPendiente
        });

        return Ok(new { total, pagina, porPagina, items });
    }

    // ─── GET /api/consultas/{id} ──────────────────────────────────────────
    [HttpGet("{id}")]
    public async Task<IActionResult> Detalle(int id)
    {
        var c = await db.Consultas
            .Include(x => x.Cliente).ThenInclude(cl => cl!.Empresa)
            .Include(x => x.Empresa)
            .Include(x => x.Responsable)
            .Include(x => x.Presupuesto).ThenInclude(p => p!.Items).ThenInclude(i => i.Analisis).ThenInclude(a => a.Grupo)
            .FirstOrDefaultAsync(x => x.IdConsulta == id);

        if (c is null) return NotFound();

        return Ok(new
        {
            c.IdConsulta,
            c.Codigo,
            c.Canal,
            c.Estado,
            c.Descripcion,
            c.Fecha,
            c.CreatedAt,
            Cliente = c.Cliente == null ? null : new
            {
                c.Cliente.IdCliente,
                c.Cliente.Nombre,
                c.Cliente.Apellido,
                c.Cliente.Email,
                c.Cliente.Telefono,
                Empresa = c.Cliente.Empresa == null ? null : new { c.Cliente.Empresa.IdEmpresa, c.Cliente.Empresa.RazonSocial }
            },
            Empresa = c.Empresa == null ? null : new { c.Empresa.IdEmpresa, c.Empresa.RazonSocial },
            Responsable = c.Responsable == null ? null : new { c.Responsable.IdUsuario, c.Responsable.Nombre, c.Responsable.Apellido },
            Presupuesto = c.Presupuesto == null ? null : new
            {
                c.Presupuesto.IdPresupuesto,
                c.Presupuesto.CodigoRpo,
                c.Presupuesto.Estado,
                c.Presupuesto.ImporteDolares,
                c.Presupuesto.ImportePesos,
                c.Presupuesto.Cotizacion,
                c.Presupuesto.AdicionalAsesoramiento,
                c.Presupuesto.TipoAsesoramiento,
                c.Presupuesto.Fecha,
                Items = c.Presupuesto.Items.Select(i => new
                {
                    i.Id,
                    i.IdAnalisis,
                    i.PrecioUsdSnapshot,
                    i.Analisis.Codigo,
                    i.Analisis.Nombre,
                    i.Analisis.Area,
                    GrupoNombre = i.Analisis.Grupo.CodigoAgrupador
                })
            }
        });
    }

    // ─── POST /api/consultas ──────────────────────────────────────────────
    [HttpPost]
    public async Task<IActionResult> Crear([FromBody] CrearConsultaRequest req)
    {
        // Generar código: CF-YYYYMMDD-NNN
        var hoy = DateTime.UtcNow;
        var prefijo = $"CF-{hoy:yyyyMMdd}-";
        var ultimoCodigo = await db.Consultas
            .Where(c => c.Codigo != null && c.Codigo.StartsWith(prefijo))
            .OrderByDescending(c => c.Codigo)
            .Select(c => c.Codigo)
            .FirstOrDefaultAsync();

        int siguiente = 1;
        if (ultimoCodigo is not null)
        {
            var partes = ultimoCodigo.Split('-');
            if (partes.Length == 3 && int.TryParse(partes[2], out var n))
                siguiente = n + 1;
        }
        var codigo = $"{prefijo}{siguiente:D3}";

        var consulta = new Consulta
        {
            Codigo = codigo,
            Canal = req.Canal ?? "mail",
            Estado = "recibida",
            Descripcion = req.Descripcion,
            Fecha = hoy,
            CreatedAt = hoy,
            IdCliente = req.IdCliente,
            IdEmpresa = req.IdEmpresa,
            IdResponsable = req.IdResponsable
        };

        db.Consultas.Add(consulta);
        await db.SaveChangesAsync();
        return CreatedAtAction(nameof(Detalle), new { id = consulta.IdConsulta }, new { consulta.IdConsulta, consulta.Codigo });
    }

    // ─── PUT /api/consultas/{id} ──────────────────────────────────────────
    [HttpPut("{id}")]
    public async Task<IActionResult> Actualizar(int id, [FromBody] ActualizarConsultaRequest req)
    {
        var c = await db.Consultas.FindAsync(id);
        if (c is null) return NotFound();

        if (req.Canal is not null) c.Canal = req.Canal;
        if (req.Estado is not null) c.Estado = req.Estado;
        if (req.Descripcion is not null) c.Descripcion = req.Descripcion;
        if (req.IdCliente.HasValue) c.IdCliente = req.IdCliente;
        if (req.IdEmpresa.HasValue) c.IdEmpresa = req.IdEmpresa;
        if (req.IdResponsable.HasValue) c.IdResponsable = req.IdResponsable;

        await db.SaveChangesAsync();
        return NoContent();
    }
}

public record CrearConsultaRequest(
    string? Canal,
    string? Descripcion,
    int? IdCliente,
    int? IdEmpresa,
    int? IdResponsable);

public record ActualizarConsultaRequest(
    string? Canal,
    string? Estado,
    string? Descripcion,
    int? IdCliente,
    int? IdEmpresa,
    int? IdResponsable);
