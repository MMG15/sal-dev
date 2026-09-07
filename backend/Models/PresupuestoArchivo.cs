using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Ssal.Api.Models;

[Table("presupuesto_archivos")]
public class PresupuestoArchivo
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Required]
    [Column("id_presupuesto")]
    public int IdPresupuesto { get; set; }

    [Required]
    [Column("nombre_original")]
    [MaxLength(260)]
    public string NombreOriginal { get; set; } = string.Empty;

    [Required]
    [Column("nombre_almacenado")]
    [MaxLength(260)]
    public string NombreAlmacenado { get; set; } = string.Empty;

    [Column("tipo_mime")]
    [MaxLength(100)]
    public string TipoMime { get; set; } = string.Empty;

    [Column("tamaño_bytes")]
    public long TamañoBytes { get; set; }

    [Column("subido_en")]
    public DateTime SubidoEn { get; set; } = DateTime.UtcNow;

    [Column("subido_por")]
    public int SubidoPor { get; set; }

    // Navegación
    [ForeignKey("IdPresupuesto")]
    public Presupuesto Presupuesto { get; set; } = null!;

    [ForeignKey("SubidoPor")]
    public Usuario SubidoPorUsuario { get; set; } = null!;
}
