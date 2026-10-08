using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Ssal.Api.Models;

// Acceso del cliente externo al portal web (RF-22, ROL-05). Tabla separada de los usuarios internos.
[Table("portal_usuarios")]
public class PortalUsuario
{
    [Key]
    [Column("id_portal_usuario")]
    public int IdPortalUsuario { get; set; }

    // Un solo acceso por cliente
    [Required]
    [Column("id_cliente")]
    public int IdCliente { get; set; }

    [Required]
    [Column("email")]
    [MaxLength(150)]
    public string Email { get; set; } = string.Empty;

    [Required]
    [Column("password_hash")]
    public string PasswordHash { get; set; } = string.Empty;

    // Al generar o restablecer, la contraseña es temporal y hay que cambiarla en el primer ingreso
    [Column("forzar_cambio_pwd")]
    public bool ForzarCambioPwd { get; set; } = true;

    [Column("activo")]
    public bool Activo { get; set; } = true;

    [Required]
    [Column("creado_por")]
    public int CreadoPor { get; set; }

    [Column("ultimo_acceso")]
    public DateTime? UltimoAcceso { get; set; }

    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [ForeignKey("IdCliente")]
    public Cliente Cliente { get; set; } = null!;
}
