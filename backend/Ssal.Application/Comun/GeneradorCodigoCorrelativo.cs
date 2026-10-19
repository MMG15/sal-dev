namespace Ssal.Application.Comun;

// Mismo formato de código en Consultas (CF-), Presupuestos (RPO-) y SSE (SSE-, ROT-): prefijo +
// fecha + correlativo reiniciado por día (o por mes, en el caso del rótulo). Antes esta lógica
// estaba copiada y pegada en varios controllers; a medida que se migra cada módulo, pasa a usar esto.
public static class GeneradorCodigoCorrelativo
{
    // digitos: 3 para CF-/RPO-/SSE- (el default, válido para todos salvo el rótulo, que usa 4).
    public static string Siguiente(string prefijo, string? ultimoCodigoConElMismoPrefijo, int digitos = 3)
    {
        var siguiente = 1;
        if (ultimoCodigoConElMismoPrefijo is not null)
        {
            var partes = ultimoCodigoConElMismoPrefijo.Split('-');
            if (partes.Length == 3 && int.TryParse(partes[2], out var n))
                siguiente = n + 1;
        }
        return $"{prefijo}{siguiente.ToString($"D{digitos}")}";
    }
}
