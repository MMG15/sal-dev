using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Ssal.Domain;

// Bandeja intermedia: todo lo que llega al mail de consultas del laboratorio entra acá primero.
// No todo correo es un pedido real, así que recién pasa a ser una Consulta formal (con su
// código CF-...) cuando alguien con permiso lo revisa y decide convertirlo.
[Table("correos_entrantes")]
public class CorreoEntrante
{
    [Key]
    [Column("id_correo")]
    public int IdCorreo { get; set; }

    // Message-Id del mail: evita importar el mismo correo dos veces si el buzón se vuelve a leer.
    [Required]
    [Column("externo_id")]
    [MaxLength(250)]
    public string ExternoId { get; set; } = string.Empty;

    [Required]
    [Column("remitente")]
    [MaxLength(200)]
    public string Remitente { get; set; } = string.Empty;

    [Column("remitente_nombre")]
    [MaxLength(150)]
    public string? RemitenteNombre { get; set; }

    [Column("asunto")]
    [MaxLength(300)]
    public string? Asunto { get; set; }

    [Column("cuerpo")]
    public string? Cuerpo { get; set; }

    [Column("fecha_recibido")]
    public DateTime FechaRecibido { get; set; } = DateTime.UtcNow;

    [Column("estado")]
    [MaxLength(20)]
    public string Estado { get; set; } = "pendiente"; // pendiente | respondido | convertido | descartado

    // Si se convirtió en una Consulta formal, queda el vínculo
    [Column("id_consulta_generada")]
    public int? IdConsultaGenerada { get; set; }

    [Column("id_usuario_accion")]
    public int? IdUsuarioAccion { get; set; }

    [Column("fecha_accion")]
    public DateTime? FechaAccion { get; set; }

    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // De qué casilla vino (útil si en algún momento hay más de una configurada)
    [Column("id_cuenta_correo")]
    public int? IdCuentaCorreo { get; set; }

    // Message-Id de la respuesta que mandó el sistema (si se respondió desde acá), para
    // poder reconocer la respuesta del cliente cuando llegue y vincularla automáticamente.
    [Column("message_id_enviado")]
    [MaxLength(250)]
    public string? MessageIdEnviado { get; set; }

    // Se completa solo, al importar, cuando este correo es la respuesta a un presupuesto o a
    // otro correo ya respondido por el sistema (matching por In-Reply-To/References). No implica
    // que se haya convertido en Consulta: solo que está relacionado con una que ya existe.
    [Column("id_consulta_relacionada")]
    public int? IdConsultaRelacionada { get; set; }

    // Navegación
    [ForeignKey("IdConsultaGenerada")]
    public Consulta? ConsultaGenerada { get; set; }

    [ForeignKey("IdUsuarioAccion")]
    public Usuario? UsuarioAccion { get; set; }

    [ForeignKey("IdCuentaCorreo")]
    public CuentaCorreo? CuentaCorreo { get; set; }

    [ForeignKey("IdConsultaRelacionada")]
    public Consulta? ConsultaRelacionada { get; set; }
}
