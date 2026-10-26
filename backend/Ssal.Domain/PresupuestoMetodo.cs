using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Ssal.Domain;

[Table("presupuesto_metodos")]
public class PresupuestoMetodo
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Required]
    [Column("id_presupuesto")]
    public int IdPresupuesto { get; set; }

    [Required]
    [Column("id_metodo")]
    public int IdMetodo { get; set; }

    // Navegación
    [ForeignKey("IdPresupuesto")]
    public Presupuesto Presupuesto { get; set; } = null!;

    [ForeignKey("IdMetodo")]
    public Metodo Metodo { get; set; } = null!;
}
