using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Ssal.Api.Models;

// Registro de accesos y acciones críticas sobre usuarios (RF-30)
[Table("auditoria_sistema")]
public class AuditoriaSistema
{
    [Key]
    [Column("id")]
    public long Id { get; set; }

    [Column("fecha")]
    public DateTime Fecha { get; set; } = DateTime.UtcNow;

    // Quién hizo la acción. Null si fue un intento de ingreso sin cuenta válida.
    [Column("id_usuario")]
    public int? IdUsuario { get; set; }

    [Required]
    [Column("accion")]
    [MaxLength(40)]
    public string Accion { get; set; } = string.Empty;

    // Email de la cuenta afectada o del intento de ingreso
    [Column("objetivo")]
    [MaxLength(150)]
    public string? Objetivo { get; set; }

    [Column("detalle")]
    [MaxLength(500)]
    public string? Detalle { get; set; }

    [ForeignKey("IdUsuario")]
    public Usuario? Usuario { get; set; }
}
