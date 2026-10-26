using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Ssal.Application.DocumentosInstitucionales;
using Ssal.Domain;
using Ssal.Infrastructure.Data;

namespace Ssal.Infrastructure.Repositorios;

public class DocumentoInstitucionalRepository(SsalDbContext db, IConfiguration config) : IDocumentoInstitucionalRepository
{
    public async Task<List<DocumentoInstitucional>> ListarAsync() =>
        await db.DocumentosInstitucionales.Include(d => d.SubidoPorUsuario).ToListAsync();

    public async Task<DocumentoInstitucional?> ObtenerAsync(int id) =>
        await db.DocumentosInstitucionales.FindAsync(id);

    public void Agregar(DocumentoInstitucional documento) => db.DocumentosInstitucionales.Add(documento);

    public void Eliminar(DocumentoInstitucional documento) => db.DocumentosInstitucionales.Remove(documento);

    public async Task<string> GuardarEnDiscoAsync(string extension, Stream contenido, CancellationToken ct)
    {
        var carpeta = Carpeta();
        Directory.CreateDirectory(carpeta);

        var nombreAlmacenado = $"{Guid.NewGuid()}{extension}";
        var rutaCompleta = Path.Combine(carpeta, nombreAlmacenado);

        using (var archivo = System.IO.File.Create(rutaCompleta))
            await contenido.CopyToAsync(archivo, ct);

        return nombreAlmacenado;
    }

    public async Task<byte[]?> LeerDeDiscoAsync(string nombreAlmacenado)
    {
        var ruta = Path.Combine(Carpeta(), nombreAlmacenado);
        if (!System.IO.File.Exists(ruta)) return null;
        return await System.IO.File.ReadAllBytesAsync(ruta);
    }

    public void EliminarDelDisco(string nombreAlmacenado)
    {
        var ruta = Path.Combine(Carpeta(), nombreAlmacenado);
        if (System.IO.File.Exists(ruta))
            System.IO.File.Delete(ruta);
    }

    private string Carpeta()
    {
        var basePath = config["Storage:BasePath"] ?? "storage";
        return Path.Combine(Directory.GetCurrentDirectory(), basePath, "documentos-institucionales");
    }

    public Task GuardarCambiosAsync() => db.SaveChangesAsync();
}
