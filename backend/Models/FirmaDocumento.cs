using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Ssal.Api.Models;

[Table("firmas_documento")]
public class FirmaDocumento
{
    [Key]
    [Column("id_firma")]
    public int IdFirma { get; set; }

    [Required]
    [Column("id_firmante")]
    public int IdFirmante { get; set; }

    [Required]
    [Column("tipo_documento")]
    [MaxLength(30)]
    public string TipoDocumento { get; set; } = string.Empty; // informe | presupuesto | sse | factura

    [Required]
    [Column("id_documento")]
    public int IdDocumento { get; set; }

    [Column("tipo_firma")]
    [MaxLength(20)]
    public string TipoFirma { get; set; } = "responsable"; // responsable | aprobacion

    [Column("estado")]
    [MaxLength(20)]
    public string Estado { get; set; } = "firmado"; // firmado | rechazado

    [Column("motivo_rechazo")]
    public string? MotivoRechazo { get; set; }

    [Column("fecha_firma")]
    public DateTime FechaFirma { get; set; } = DateTime.UtcNow;

    // Navegación
    [ForeignKey("IdFirmante")]
    public Usuario Firmante { get; set; } = null!;
}
