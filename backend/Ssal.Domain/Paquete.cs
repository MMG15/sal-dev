using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Ssal.Domain;

// Combo de análisis vendido como unidad, con precio propio bonificado (no es la suma de sus
// partes). Catálogo reutilizable: una vez cargado se puede usar en cualquier presupuesto futuro.
// Los paquetes escalonados (ej. Básico / Básico+Metales / Básico+Metales+Arsénico) se guardan
// como paquetes independientes, cada uno con su propia lista de análisis y precio.
[Table("paquetes")]
public class Paquete
{
    [Key]
    [Column("id_paquete")]
    public int IdPaquete { get; set; }

    [Required]
    [Column("nombre")]
    [MaxLength(200)]
    public string Nombre { get; set; } = string.Empty;

    [Column("area")]
    [MaxLength(10)]
    public string Area { get; set; } = "MIC"; // MIC | FQ

    [Column("tipo_muestra")]
    [MaxLength(50)]
    public string? TipoMuestra { get; set; }

    [Column("norma")]
    [MaxLength(300)]
    public string? Norma { get; set; }

    [Column("cantidad_muestra")]
    [MaxLength(200)]
    public string? CantidadMuestra { get; set; }

    [Column("precio_usd")]
    public decimal PrecioUsd { get; set; }

    [Column("activo")]
    public bool Activo { get; set; } = true;

    // Navegación
    public ICollection<PaqueteAnalisis> Items { get; set; } = [];
}
