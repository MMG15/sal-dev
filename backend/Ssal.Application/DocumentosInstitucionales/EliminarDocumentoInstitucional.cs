namespace Ssal.Application.DocumentosInstitucionales;

public class EliminarDocumentoInstitucional(IDocumentoInstitucionalRepository repo)
{
    public async Task<bool> EjecutarAsync(int id)
    {
        var documento = await repo.ObtenerAsync(id);
        if (documento is null) return false;

        repo.EliminarDelDisco(documento.NombreAlmacenado);
        repo.Eliminar(documento);
        await repo.GuardarCambiosAsync();
        return true;
    }
}
