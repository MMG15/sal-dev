using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Ssal.Api.Models;

[Table("empresas")]
public class Empresa
{
    [Key]
    [Column("id_empresa")]
    public int IdEmpresa { get; set; }

    [Required]
    [Column("razon_social")]
    [MaxLength(200)]
    public string RazonSocial { get; set; } = string.Empty;

    [Column("cuit")]
    [MaxLength(20)]
    public string? Cuit { get; set; }

    [Column("email")]
    [MaxLength(150)]
    public string? Email { get; set; }

    [Column("telefono")]
    [MaxLength(50)]
    public string? Telefono { get; set; }

    [Column("direccion")]
    [MaxLength(300)]
    public string? Direccion { get; set; }

    [Column("estado")]
    [MaxLength(20)]
    public string Estado { get; set; } = "activa";

    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [Column("updated_at")]
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Navegación
    public ICollection<Cliente> Clientes { get; set; } = [];
    public ICollection<Consulta> Consultas { get; set; } = [];
}
