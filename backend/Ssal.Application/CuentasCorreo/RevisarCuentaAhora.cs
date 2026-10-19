namespace Ssal.Application.CuentasCorreo;

public class RevisarCuentaAhora(ICuentaCorreoRepository repo, IRevisorDeCuentaCorreo revisor)
{
    public async Task<Resultado<RevisionResultadoDto>> EjecutarAsync(int id, CancellationToken ct)
    {
        if (!await repo.ExisteAsync(id)) return Resultado<RevisionResultadoDto>.Fallido("No encontrado");

        var (importados, error) = await revisor.RevisarAsync(id, ct);
        if (error is not null) return Resultado<RevisionResultadoDto>.Fallido(error);

        return Resultado<RevisionResultadoDto>.Exitoso(new RevisionResultadoDto(importados));
    }
}
