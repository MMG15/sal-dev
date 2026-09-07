using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Ssal.Api.Models;

[Table("resultados")]
public class Resultado
{
    [Key]
    [Column("id_resultado")]
    public int IdResultado { get; set; }

    [Required]
    [Column("id_muestra")]
    public int IdMuestra { get; set; }

    [Required]
    [Column("id_analisis")]
    public int IdAnalisis { get; set; }

    [Column("cargado_por")]
    public int? CargadoPor { get; set; }

    [Column("valor")]
    [MaxLength(500)]
    public string? Valor { get; set; }

    [Column("estado")]
    [MaxLength(20)]
    public string Estado { get; set; } = "pendiente"; // pendiente | cargado | validado | rechazado

    [Column("fecha_carga")]
    public DateTime? FechaCarga { get; set; }

    [Column("validado_por")]
    public int? ValidadoPor { get; set; }

    [Column("fecha_validacion")]
    public DateTime? FechaValidacion { get; set; }

    [Column("motivo_rechazo")]
    [MaxLength(500)]
    public string? MotivoRechazo { get; set; }

    // Navegación
    [ForeignKey("IdMuestra")]
    public Muestra Muestra { get; set; } = null!;

    [ForeignKey("IdAnalisis")]
    public Analisis Analisis { get; set; } = null!;

    [ForeignKey("CargadoPor")]
    public Usuario? CargadoPorUsuario { get; set; }

    [ForeignKey("ValidadoPor")]
    public Usuario? ValidadoPorUsuario { get; set; }
}
