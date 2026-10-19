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

    public Task GuardarCambiosAsync() => db.SaveChangesAsync();
}
