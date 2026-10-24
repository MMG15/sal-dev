namespace Ssal.Application.Sse;

public class RegistrarMuestra(ISseRepository repo)
{
    // Condición de la muestra al recibirla (PO 04):
    // apta               -> se recibe normalmente
    // no_apta_procesar   -> no está en condiciones pero el cliente pide procesarla igual
    // rechazada          -> no está en condiciones y no se procesa (fin del servicio)
    public async Task<Resultado<int>> EjecutarAsync(int idSse, int idRotulo, RegistrarMuestraCommand cmd, int idUsuario)
    {
        var rotulo = await repo.ObtenerRotuloConMuestraAsync(idRotulo);
        if (rotulo is null || rotulo.IdSse != idSse) return Resultado<int>.Fallido("Rótulo no encontrado para esta SSE.");
        if (rotulo.Muestra is not null) return Resultado<int>.Fallido("Este rótulo ya tiene una muestra registrada.");

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
        if (sse is not null)
        {
            // Si el pedido tiene varias muestras repetidas, el rechazo de una sola no cierra
            // todo el pedido como rechazado (las demás pueden seguir su curso normalmente).
            sse.Estado = rechazada && sse.RotulosInternos.Count <= 1 ? "rechazada" : "analizando";
        }

        // Al recibir la muestra (si no fue rechazada), se abre un resultado pendiente
        // por cada análisis solicitado en el presupuesto vinculado, ya sea suelto o
        // dentro de un paquete (sin duplicar si el mismo análisis aparece en ambos).
        if (!rechazada && sse?.Presupuesto is not null)
        {
            var idsAnalisis = sse.Presupuesto.Items.Select(i => i.IdAnalisis)
                .Concat(sse.Presupuesto.Paquetes.SelectMany(pp => pp.Paquete.Items.Select(pa => pa.IdAnalisis)))
                .Distinct();

            foreach (var idAnalisis in idsAnalisis)
            {
                repo.AgregarResultado(new Ssal.Domain.Resultado
                {
                    Muestra = muestra,
                    IdAnalisis = idAnalisis,
                    Estado = "pendiente"
                });
            }
        }

        await repo.GuardarCambiosAsync();
        return Resultado<int>.Exitoso(muestra.IdMuestra);
    }
}
