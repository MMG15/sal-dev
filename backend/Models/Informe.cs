using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Ssal.Api.Models;

[Table("informes")]
public class Informe
{
    [Key]
    [Column("id_informe")]
    public int IdInforme { get; set; }

    [Required]
    [Column("id_muestra")]
    public int IdMuestra { get; set; }

    [Required]
    [Column("generado_por")]
    public int GeneradoPor { get; set; }

    [Column("codigo")]
    [MaxLength(30)]
    public string Codigo { get; set; } = string.Empty; // INF-YYYYMMDD-NNN (RPO 01-04)

    [Column("fecha_generacion")]
    public DateTime FechaGeneracion { get; set; } = DateTime.UtcNow;

    // Navegación
    [ForeignKey("IdMuestra")]
    public Muestra Muestra { get; set; } = null!;

    [ForeignKey("GeneradoPor")]
    public Usuario GeneradoPorUsuario { get; set; } = null!;
}
