using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Ssal.Api.Models;

[Table("clientes")]
public class Cliente
{
    [Key]
    [Column("id_cliente")]
    public int IdCliente { get; set; }

    [Column("id_empresa")]
    public int? IdEmpresa { get; set; }

    [Required]
    [Column("nombre")]
    [MaxLength(100)]
    public string Nombre { get; set; } = string.Empty;

    [Required]
    [Column("apellido")]
    [MaxLength(100)]
    public string Apellido { get; set; } = string.Empty;

    [Column("email")]
    [MaxLength(150)]
    public string? Email { get; set; }

    [Column("telefono")]
    [MaxLength(50)]
    public string? Telefono { get; set; }

    [Column("cuit")]
    [MaxLength(20)]
    public string? Cuit { get; set; }

    [Column("condicion_iva")]
    [MaxLength(30)]
    public string? CondicionIva { get; set; }

    [Column("usuario_web")]
    [MaxLength(100)]
    public string? UsuarioWeb { get; set; }

    [Column("estado")]
    [MaxLength(20)]
    public string Estado { get; set; } = "activo";

    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navegación
    [ForeignKey("IdEmpresa")]
    public Empresa? Empresa { get; set; }

    public ICollection<Consulta> Consultas { get; set; } = [];
}
