using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Ssal.Api.Models;

// Casilla de correo desde la que se importan consultas. El administrador general y el
// administrador de sistema pueden dar de alta más de una y editar las existentes.
[Table("cuentas_correo")]
public class CuentaCorreo
{
    [Key]
    [Column("id_cuenta")]
    public int IdCuenta { get; set; }

    // Etiqueta para identificarla en la lista, ej. "Casilla principal de consultas"
    [Required]
    [Column("nombre")]
    [MaxLength(100)]
    public string Nombre { get; set; } = string.Empty;

    [Required]
    [Column("servidor")]
    [MaxLength(150)]
    public string Servidor { get; set; } = "imap.gmail.com";

    [Column("puerto")]
    public int Puerto { get; set; } = 993;

    // Mismas credenciales que el IMAP: en Gmail, la contraseña de aplicación sirve para ambos.
    [Required]
    [Column("servidor_smtp")]
    [MaxLength(150)]
    public string ServidorSmtp { get; set; } = "smtp.gmail.com";

    [Column("puerto_smtp")]
    public int PuertoSmtp { get; set; } = 587;

    [Required]
    [Column("usuario")]
    [MaxLength(200)]
    public string Usuario { get; set; } = string.Empty;

    // Cifrada con Services/Cifrado.cs. Nunca se devuelve al frontend.
    [Required]
    [Column("password_cifrada")]
    public string PasswordCifrada { get; set; } = string.Empty;

    [Column("habilitada")]
    public bool Habilitada { get; set; } = true;

    [Column("intervalo_minutos")]
    public int IntervaloMinutos { get; set; } = 5;

    [Column("ultima_revision")]
    public DateTime? UltimaRevision { get; set; }

    // Último error de conexión/autenticación, para avisar en la pantalla si algo falla
    [Column("ultimo_error")]
    [MaxLength(500)]
    public string? UltimoError { get; set; }

    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [Column("updated_at")]
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
