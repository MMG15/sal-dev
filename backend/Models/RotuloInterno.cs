using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Ssal.Api.Models;

[Table("rotulos_internos")]
public class RotuloInterno
{
    [Key]
    [Column("id_rotulo")]
    public int IdRotulo { get; set; }

    [Required]
    [Column("id_sse")]
    public int IdSse { get; set; }

    [Required]
    [Column("asignado_por")]
    public int AsignadoPor { get; set; }

    [Required]
    [Column("numero_unico")]
    [MaxLength(50)]
    public string NumeroUnico { get; set; } = string.Empty;

    [Column("descripcion")]
    [MaxLength(500)]
    public string? Descripcion { get; set; }

    [Column("estado")]
    [MaxLength(20)]
    public string Estado { get; set; } = "activo";

    [Column("fecha_asignacion")]
    public DateTime FechaAsignacion { get; set; } = DateTime.UtcNow;

    // Navegación
    [ForeignKey("IdSse")]
    public Sse Sse { get; set; } = null!;

    [ForeignKey("AsignadoPor")]
    public Usuario AsignadoPorUsuario { get; set; } = null!;

    public ICollection<AuditoriaRotulo> Auditorias { get; set; } = [];
    public Muestra? Muestra { get; set; }
}
