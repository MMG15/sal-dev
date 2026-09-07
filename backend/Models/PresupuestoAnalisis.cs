using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Ssal.Api.Models;

[Table("presupuesto_analisis")]
public class PresupuestoAnalisis
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Required]
    [Column("id_presupuesto")]
    public int IdPresupuesto { get; set; }

    [Required]
    [Column("id_analisis")]
    public int IdAnalisis { get; set; }

    [Column("precio_usd_snapshot")]
    public decimal PrecioUsdSnapshot { get; set; }

    // Navegación
    [ForeignKey("IdPresupuesto")]
    public Presupuesto Presupuesto { get; set; } = null!;

    [ForeignKey("IdAnalisis")]
    public Analisis Analisis { get; set; } = null!;
}
