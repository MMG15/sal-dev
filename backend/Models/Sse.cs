using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Ssal.Api.Models;

[Table("sses")]
public class Sse
{
    [Key]
    [Column("id_sse")]
    public int IdSse { get; set; }

    [Required]
    [Column("id_presupuesto")]
    public int IdPresupuesto { get; set; }

    [Required]
    [Column("id_cliente")]
    public int IdCliente { get; set; }

    [Required]
    [Column("creado_por")]
    public int CreadoPor { get; set; }

    [Column("codigo_rpo")]
    [MaxLength(20)]
    public string? CodigoRpo { get; set; } // RPO 01-09

    [Column("area")]
    [MaxLength(10)]
    public string Area { get; set; } = "MIC"; // MIC | FQ | AMBAS

    [Column("importe_pesos")]
    public decimal ImportePesos { get; set; }

    [Column("importe_dolares")]
    public decimal ImporteDolares { get; set; }

    [Column("forma_pago")]
    [MaxLength(30)]
    public string? FormaPago { get; set; } // efectivo | cheque | transferencia

    [Column("estado")]
    [MaxLength(30)]
    public string Estado { get; set; } = "activa";

    [Column("fecha")]
    public DateTime Fecha { get; set; } = DateTime.UtcNow;

    // Navegación
    [ForeignKey("IdPresupuesto")]
    public Presupuesto Presupuesto { get; set; } = null!;

    [ForeignKey("IdCliente")]
    public Cliente Cliente { get; set; } = null!;

    [ForeignKey("CreadoPor")]
    public Usuario CreadoPorUsuario { get; set; } = null!;

    public RotuloInterno? RotuloInterno { get; set; }
    public Factura? Factura { get; set; }
}
