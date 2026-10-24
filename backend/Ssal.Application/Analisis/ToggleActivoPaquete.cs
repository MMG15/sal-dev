namespace Ssal.Application.Analisis;

public class ToggleActivoPaquete(IAnalisisRepository repo)
{
    public async Task<Resultado<(int IdPaquete, bool Activo)>> EjecutarAsync(int id)
    {
        var paquete = await repo.ObtenerPaquetePorIdAsync(id);
        if (paquete is null) return Resultado<(int, bool)>.Fallido("No encontrado");

        paquete.Activo = !paquete.Activo;
        await repo.GuardarCambiosAsync();

        return Resultado<(int, bool)>.Exitoso((paquete.IdPaquete, paquete.Activo));
    }
}
