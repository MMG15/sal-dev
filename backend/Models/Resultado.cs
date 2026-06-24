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

    [Required]
    [Column("cargado_por")]
    public int CargadoPor { get; set; }

    [Column("valor")]
    [MaxLength(500)]
    public string? Valor { get; set; }

    [Column("estado")]
    [MaxLength(20)]
    public string Estado { get; set; } = "pendiente"; // pendiente | cargado | validado

    [Column("fecha_carga")]
    public DateTime FechaCarga { get; set; } = DateTime.UtcNow;

    // Navegación
    [ForeignKey("IdMuestra")]
    public Muestra Muestra { get; set; } = null!;

    [ForeignKey("IdAnalisis")]
    public Analisis Analisis { get; set; } = null!;

    [ForeignKey("CargadoPor")]
    public Usuario CargadoPorUsuario { get; set; } = null!;
}
