namespace Ssal.Application.Sse;

public class RegistrarMuestra(ISseRepository repo)
{
    // Condición de la muestra al recibirla (PO 04):
    // apta               -> se recibe normalmente
    // no_apta_procesar   -> no está en condiciones pero el cliente pide procesarla igual
    // rechazada          -> no está en condiciones y no se procesa (fin del servicio)
    public async Task<Resultado<int>> EjecutarAsync(int idSse, RegistrarMuestraCommand cmd, int idUsuario)
    {
        var rotulo = await repo.ObtenerRotuloConMuestraAsync(idSse);
        if (rotulo is null) return Resultado<int>.Fallido("Primero asigná un rótulo a esta SSE.");
        if (rotulo.Muestra is not null) return Resultado<int>.Fallido("Esta SSE ya tiene una muestra registrada.");

        var rechazada = cmd.Condicion == "rechazada";
        if (rechazada && string.IsNullOrWhiteSpace(cmd.Observacion))
            return Resultado<int>.Fallido("Indicá el motivo del rechazo en la observación.");

        var muestra = new Ssal.Domain.Muestra
        {
            IdRotulo = rotulo.IdRotulo,
            Tipo = cmd.Tipo,
            Observacion = cmd.Observacion,
            Estado = rechazada ? "rechazada" : "en_proceso",
            FechaRecepcion = DateTime.UtcNow,
            RecibidoPor = idUsuario
        };
        repo.AgregarMuestra(muestra);

        var sse = await repo.ObtenerConPresupuestoItemsAsync(idSse);
        if (sse is not null) sse.Estado = rechazada ? "rechazada" : "analizando";

        // Al recibir la muestra (si no fue rechazada), se abre un resultado pendiente
        // por cada análisis solicitado en el presupuesto vinculado.
        if (!rechazada && sse?.Presupuesto is not null)
        {
            foreach (var item in sse.Presupuesto.Items)
            {
                repo.AgregarResultado(new Ssal.Domain.Resultado
                {
                    Muestra = muestra,
                    IdAnalisis = item.IdAnalisis,
                    Estado = "pendiente"
                });
            }
        }

        await repo.GuardarCambiosAsync();
        return Resultado<int>.Exitoso(muestra.IdMuestra);
    }
}
