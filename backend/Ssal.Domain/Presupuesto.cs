using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Ssal.Domain;

[Table("presupuestos")]
public class Presupuesto
{
    [Key]
    [Column("id_presupuesto")]
    public int IdPresupuesto { get; set; }

    [Required]
    [Column("id_consulta")]
    public int IdConsulta { get; set; }

    [Required]
    [Column("creado_por")]
    public int CreadoPor { get; set; }

    [Column("codigo_rpo")]
    [MaxLength(20)]
    public string? CodigoRpo { get; set; } // RPO 01-05

    [Column("importe_pesos")]
    public decimal ImportePesos { get; set; }

    [Column("importe_dolares")]
    public decimal ImporteDolares { get; set; }

    [Column("adicional_asesoramiento")]
    public decimal AdicionalAsesoramiento { get; set; } = 0;

    [Column("tipo_asesoramiento")]
    [MaxLength(20)]
    public string? TipoAsesoramiento { get; set; } // consulta | terreno | proceso_completo

    [Column("cotizacion")]
    public decimal Cotizacion { get; set; } = 1;

    // Qué valor mostrar en los PDF (presupuesto y SSE): usd | ars | ambos
    [Column("mostrar_moneda")]
    [MaxLength(10)]
    public string MostrarMoneda { get; set; } = "ambos";

    [Column("estado")]
    [MaxLength(30)]
    public string Estado { get; set; } = "borrador"; // borrador | enviado | aceptado | rechazado

    [Column("fecha")]
    public DateTime Fecha { get; set; } = DateTime.UtcNow;

    // Message-Id del último mail enviado con este presupuesto, para reconocer la respuesta
    // del cliente (aceptación/rechazo por mail) y vincularla a esta consulta automáticamente.
    [Column("message_id_enviado")]
    [MaxLength(250)]
    public string? MessageIdEnviado { get; set; }

    // Navegación
    [ForeignKey("IdConsulta")]
    public Consulta Consulta { get; set; } = null!;

    [ForeignKey("CreadoPor")]
    public Usuario CreadoPorUsuario { get; set; } = null!;

    public Sse? Sse { get; set; }

    public ICollection<PresupuestoAnalisis> Items { get; set; } = [];
    public ICollection<PresupuestoPaquete> Paquetes { get; set; } = [];
    public ICollection<PresupuestoMetodo> Metodos { get; set; } = [];
    public ICollection<PresupuestoArchivo> Archivos { get; set; } = [];
}
