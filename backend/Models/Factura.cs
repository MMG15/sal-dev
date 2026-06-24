using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Ssal.Api.Models;

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
    public string Estado { get; set; } = "emitida"; // emitida | anulada | pagada

    [Column("fecha")]
    public DateTime Fecha { get; set; } = DateTime.UtcNow;

    // Navegación
    [ForeignKey("IdSse")]
    public Sse Sse { get; set; } = null!;

    [ForeignKey("CreadoPor")]
    public Usuario CreadoPorUsuario { get; set; } = null!;
}
