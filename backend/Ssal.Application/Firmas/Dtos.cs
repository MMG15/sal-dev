namespace Ssal.Application.Firmas;

public record DocInfoDto(
    string Tipo, int Id, string Codigo, string Destinatario, string? DestinatarioEmail, string? Sse, string? Rotulo,
    DateTime Fecha, List<string> Areas);

public record PermisosDto(bool Firmar, bool Aprobar, bool Rechazar, bool Reenviar);

public record ListarFirmasItemDto(
    string Tipo, int Id, string Codigo, string Destinatario, string? Sse, string? Rotulo,
    DateTime Fecha, List<string> Areas, string Estado, List<string> AreasFirmadas);

public record EventoFirmaDto(int IdFirma, string TipoFirma, string? Area, string Estado, string? MotivoRechazo, DateTime FechaFirma, string Firmante);

public record DetalleFirmaDto(DocInfoDto Documento, string Estado, List<string> AreasFirmadas, PermisosDto Permisos, List<EventoFirmaDto> Historial);

public record ConfirmarPasswordCommand(string? Password);

public record RechazarCommand(string? Motivo);

public record EnviarInformeFirmadoCommand(string Destinatario, string Asunto, string Cuerpo);
