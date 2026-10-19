using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Ssal.Domain;

[Table("facturas")]
public class Factura
{
    [Key]
    [Column("id_factura")]
    public int IdFactura { get; set; }

    [Required]
    [Column("id_sse")]
    public int IdSse { get; set; }

    [Required]
    [Column("creado_por")]
    public int CreadoPor { get; set; }

    [Column("numero")]
    [MaxLength(30)]
    public string? Numero { get; set; }

    [Column("importe_pesos")]
    public decimal ImportePesos { get; set; }

    [Column("importe_dolares")]
    public decimal ImporteDolares { get; set; }

    [Column("estado")]
    [MaxLength(20)]
    public string Estado { get; set; } = "emitida"; // emitida | pagada | anulada (anulada: pendiente, ver RF-25)

    [Column("fecha")]
    public DateTime Fecha { get; set; } = DateTime.UtcNow; // fecha de emisión (la informa el usuario según TANGO)

    [Column("fecha_pago")]
    public DateTime? FechaPago { get; set; }

    // Reservado para la anulación (RF-25). Todavía no se usa.
    [Column("motivo_anulacion")]
    [MaxLength(500)]
    public string? MotivoAnulacion { get; set; }

    // Navegación
    [ForeignKey("IdSse")]
    public Sse Sse { get; set; } = null!;

    [ForeignKey("CreadoPor")]
    public Usuario CreadoPorUsuario { get; set; } = null!;
}
