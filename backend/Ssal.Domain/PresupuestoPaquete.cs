using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Ssal.Domain;

[Table("presupuesto_paquetes")]
public class PresupuestoPaquete
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Required]
    [Column("id_presupuesto")]
    public int IdPresupuesto { get; set; }

    [Required]
    [Column("id_paquete")]
    public int IdPaquete { get; set; }

    [Column("precio_usd_snapshot")]
    public decimal PrecioUsdSnapshot { get; set; }

    // Navegación
    [ForeignKey("IdPresupuesto")]
    public Presupuesto Presupuesto { get; set; } = null!;

    [ForeignKey("IdPaquete")]
    public Paquete Paquete { get; set; } = null!;
}
