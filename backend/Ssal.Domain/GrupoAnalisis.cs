using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Ssal.Domain;

[Table("grupos_analisis")]
public class GrupoAnalisis
{
    [Key]
    [Column("id_grupo")]
    public int IdGrupo { get; set; }

    [Required]
    [Column("codigo_agrupador")]
    [MaxLength(30)]
    public string CodigoAgrupador { get; set; } = string.Empty;

    [Column("descripcion")]
    [MaxLength(300)]
    public string? Descripcion { get; set; }

    [Column("area")]
    [MaxLength(10)]
    public string Area { get; set; } = "MIC"; // MIC | FQ

    // Agua, Alimentos, Ambiente, etc. Texto libre: el laboratorio todavía no tiene cerrada la
    // lista completa de categorías (hoy solo se modeló agua).
    [Column("tipo_muestra")]
    [MaxLength(50)]
    public string? TipoMuestra { get; set; }

    [Column("norma")]
    [MaxLength(300)]
    public string? Norma { get; set; } // ej: "C.A.A. art. N° 982"

    [Column("cantidad_muestra")]
    [MaxLength(200)]
    public string? CantidadMuestra { get; set; } // ej: "400mL en envase estéril"

    // Navegación
    public ICollection<Analisis> Analisis { get; set; } = [];
}
