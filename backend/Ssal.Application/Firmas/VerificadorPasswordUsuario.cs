using Ssal.Application.Comun;

namespace Ssal.Application.Firmas;

// Compartido por FirmarDocumento y AprobarDocumento: ambos piden confirmar la contraseña
// del usuario actual antes de registrar la firma.
public class VerificadorPasswordUsuario(IFirmaRepository repo, IHashDeContrasenas hash)
{
    public async Task<bool> EsValidaAsync(string? password, int idUsuario)
    {
        if (string.IsNullOrEmpty(password)) return false;
        var usuario = await repo.ObtenerUsuarioAsync(idUsuario);
        return usuario is not null && hash.Verificar(password, usuario.PasswordHash);
    }
}
