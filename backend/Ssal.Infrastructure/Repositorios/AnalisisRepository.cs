using Microsoft.EntityFrameworkCore;
using Ssal.Application.Analisis;
using Ssal.Domain;
using Ssal.Infrastructure.Data;

namespace Ssal.Infrastructure.Repositorios;

public class AnalisisRepository(SsalDbContext db) : IAnalisisRepository
{
    public async Task<List<GrupoAnalisis>> ListarGruposConAnalisisAsync() =>
        await db.GruposAnalisis.Include(g => g.Analisis).ToListAsync();

    public async Task<List<Ssal.Domain.Analisis>> ListarTodosConGrupoAsync() =>
        await db.Analisis.Include(a => a.Grupo).ToListAsync();

    public async Task<List<GrupoAnalisis>> ListarGruposAsync() =>
        await db.GruposAnalisis.ToListAsync();

    public async Task<bool> ExisteCodigoAsync(string codigo, int? excluirId) =>
        await db.Analisis.AnyAsync(a => a.Codigo == codigo && a.IdAnalisis != (excluirId ?? 0));

    public async Task<Ssal.Domain.Analisis?> ObtenerPorIdAsync(int id) =>
        await db.Analisis.FindAsync(id);

    public async Task<GrupoAnalisis?> ObtenerGrupoPorIdAsync(int id) =>
        await db.GruposAnalisis.FindAsync(id);

    public GrupoAnalisis CrearGrupo(string codigoAgrupador, string area)
    {
        var grupo = new GrupoAnalisis
        {
            CodigoAgrupador = codigoAgrupador,
            Area = area,
            Descripcion = codigoAgrupador
        };
        db.GruposAnalisis.Add(grupo);
        return grupo;
    }

    public void Agregar(Ssal.Domain.Analisis analisis) => db.Analisis.Add(analisis);

    public async Task<bool> TieneUsoEnPresupuestosAsync(int idAnalisis) =>
        await db.PresupuestosAnalisis.AnyAsync(p => p.IdAnalisis == idAnalisis);

    public void Eliminar(Ssal.Domain.Analisis analisis) => db.Analisis.Remove(analisis);

    public async Task<List<Paquete>> ListarPaquetesConAnalisisAsync() =>
        await db.Paquetes.Include(p => p.Items).ThenInclude(i => i.Analisis).Where(p => p.Activo).ToListAsync();

    public async Task<List<Paquete>> ListarTodosLosPaquetesConAnalisisAsync() =>
        await db.Paquetes.Include(p => p.Items).ThenInclude(i => i.Analisis).ToListAsync();

    public async Task<Paquete?> ObtenerPaquetePorIdAsync(int id) =>
        await db.Paquetes.Include(p => p.Items).FirstOrDefaultAsync(p => p.IdPaquete == id);

    public async Task<List<Ssal.Domain.Analisis>> ObtenerAnalisisPorIdsAsync(List<int> ids) =>
        await db.Analisis.Where(a => ids.Contains(a.IdAnalisis)).ToListAsync();

    public void AgregarPaquete(Paquete paquete) => db.Paquetes.Add(paquete);

    public void ReemplazarItemsDePaquete(Paquete paquete, List<Ssal.Domain.Analisis> nuevosAnalisis)
    {
        db.PaquetesAnalisis.RemoveRange(paquete.Items);
        paquete.Items = nuevosAnalisis
            .Select(a => new PaqueteAnalisis { IdPaquete = paquete.IdPaquete, IdAnalisis = a.IdAnalisis })
            .ToList();
    }

    public async Task<bool> TienePaqueteUsoEnPresupuestosAsync(int idPaquete) =>
        await db.PresupuestosPaquetes.AnyAsync(p => p.IdPaquete == idPaquete);

    public void EliminarPaquete(Paquete paquete) => db.Paquetes.Remove(paquete);

    public Task GuardarCambiosAsync() => db.SaveChangesAsync();
}
