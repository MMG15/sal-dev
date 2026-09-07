using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Ssal.Api.Models;

[Table("auditorias_rotulo")]
public class AuditoriaRotulo
{
    [Key]
    [Column("id_auditoria")]
    public int IdAuditoria { get; set; }

    [Required]
    [Column("id_rotulo")]
    public int IdRotulo { get; set; }

    [Required]
    [Column("id_usuario")]
    public int IdUsuario { get; set; }

    [Required]
    [Column("campo")]
    [MaxLength(20)]
    public string Campo { get; set; } = "estado"; // numero_unico | descripcion | estado

    [Column("valor_anterior")]
    [MaxLength(500)]
    public string? ValorAnterior { get; set; }

    [Column("valor_nuevo")]
    [MaxLength(500)]
    public string? ValorNuevo { get; set; }

    [Column("motivo")]
    [MaxLength(500)]
    public string? Motivo { get; set; }

    [Column("fecha_cambio")]
    public DateTime FechaCambio { get; set; } = DateTime.UtcNow;

    // Navegación
    [ForeignKey("IdRotulo")]
    public RotuloInterno RotuloInterno { get; set; } = null!;

    [ForeignKey("IdUsuario")]
    public Usuario Usuario { get; set; } = null!;
}
