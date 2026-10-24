using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Ssal.Domain;

[Table("paquete_analisis")]
public class PaqueteAnalisis
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Required]
    [Column("id_paquete")]
    public int IdPaquete { get; set; }

    [Required]
    [Column("id_analisis")]
    public int IdAnalisis { get; set; }

    // Navegación
    [ForeignKey("IdPaquete")]
    public Paquete Paquete { get; set; } = null!;

    [ForeignKey("IdAnalisis")]
    public Analisis Analisis { get; set; } = null!;
}
