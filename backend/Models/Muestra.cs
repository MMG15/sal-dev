using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Ssal.Api.Models;

[Table("muestras")]
public class Muestra
{
    [Key]
    [Column("id_muestra")]
    public int IdMuestra { get; set; }

    [Required]
    [Column("id_rotulo")]
    public int IdRotulo { get; set; }

    [Required]
    [Column("recibido_por")]
    public int RecibidoPor { get; set; }

    [Column("tipo")]
    [MaxLength(100)]
    public string? Tipo { get; set; }

    [Column("estado")]
    [MaxLength(30)]
    public string Estado { get; set; } = "en_proceso"; // en_proceso | completada | rechazada

    [Column("observacion")]
    public string? Observacion { get; set; }

    [Column("fecha_recepcion")]
    public DateTime FechaRecepcion { get; set; } = DateTime.UtcNow;

    // Navegación
    [ForeignKey("IdRotulo")]
    public RotuloInterno RotuloInterno { get; set; } = null!;

    [ForeignKey("RecibidoPor")]
    public Usuario RecibidoPorUsuario { get; set; } = null!;

    public ICollection<Resultado> Resultados { get; set; } = [];
}
