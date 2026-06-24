using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Ssal.Api.Models;

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

    // Navegación
    public ICollection<Analisis> Analisis { get; set; } = [];
}
