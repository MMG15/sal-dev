using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Ssal.Domain;

// Técnica/norma/equipo con la que se hace un análisis (norma + equipo + sensibilidad + rango +
// condiciones especiales). Catálogo reutilizable: antes era texto libre repetido en cada
// análisis; ahora se carga una vez y se referencia desde Analisis, y también se puede asociar
// directamente a un Presupuesto (para mostrar en el PDF qué método se va a aplicar).
[Table("metodos")]
public class Metodo
{
    [Key]
    [Column("id_metodo")]
    public int IdMetodo { get; set; }

    [Required]
    [Column("nombre")]
    [MaxLength(200)]
    public string Nombre { get; set; } = string.Empty;

    [Column("descripcion")]
    [MaxLength(1000)]
    public string? Descripcion { get; set; }

    [Column("area")]
    [MaxLength(10)]
    public string Area { get; set; } = "MIC"; // MIC | FQ | AMBAS

    [Column("activo")]
    public bool Activo { get; set; } = true;

    // Navegación
    public ICollection<Analisis> Analisis { get; set; } = [];
    public ICollection<PresupuestoMetodo> Presupuestos { get; set; } = [];
}
