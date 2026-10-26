using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Ssal.Domain;

[Table("analisis")]
public class Analisis
{
    [Key]
    [Column("id_analisis")]
    public int IdAnalisis { get; set; }

    [Required]
    [Column("id_grupo")]
    public int IdGrupo { get; set; }

    [Required]
    [Column("codigo")]
    [MaxLength(30)]
    public string Codigo { get; set; } = string.Empty;

    [Required]
    [Column("nombre")]
    [MaxLength(200)]
    public string Nombre { get; set; } = string.Empty;

    [Column("area")]
    [MaxLength(10)]
    public string Area { get; set; } = "MIC"; // MIC | FQ

    [Column("precio_usd")]
    public decimal PrecioUsd { get; set; }

    [Column("unidad")]
    [MaxLength(30)]
    public string? Unidad { get; set; } // ej: mg/kg, UFC/g, %

    [Column("rango_min")]
    public decimal? RangoMin { get; set; }

    [Column("rango_max")]
    public decimal? RangoMax { get; set; }

    [Column("valor_esperado")]
    [MaxLength(100)]
    public string? ValorEsperado { get; set; } // para resultados cualitativos, ej: "Ausencia", "Negativo"

    // Técnica/norma/equipo con la que se hace este análisis (catálogo reutilizable, ver Metodo.cs
    // — antes era texto libre repetido en cada análisis).
    [Column("id_metodo")]
    public int? IdMetodo { get; set; }

    // Se manda a hacer a un laboratorio externo en vez de hacerse in-house. Afecta vigencia de
    // la oferta y condiciones de pago (suele pedirse anticipo antes de poder iniciar el análisis).
    [Column("tercerizado")]
    public bool Tercerizado { get; set; } = false;

    [Column("activo")]
    public bool Activo { get; set; } = true;

    // Navegación
    [ForeignKey("IdGrupo")]
    public GrupoAnalisis Grupo { get; set; } = null!;

    [ForeignKey("IdMetodo")]
    public Metodo? Metodo { get; set; }

    public ICollection<Resultado> Resultados { get; set; } = [];
}
