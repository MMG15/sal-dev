using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Ssal.Api.Models;

[Table("consultas")]
public class Consulta
{
    [Key]
    [Column("id_consulta")]
    public int IdConsulta { get; set; }

    [Column("id_cliente")]
    public int? IdCliente { get; set; }

    [Column("id_empresa")]
    public int? IdEmpresa { get; set; }

    [Column("id_responsable")]
    public int? IdResponsable { get; set; }

    [Column("codigo")]
    [MaxLength(30)]
    public string? Codigo { get; set; }

    [Column("canal")]
    [MaxLength(30)]
    public string Canal { get; set; } = "mail"; // mail | whatsapp

    [Column("estado")]
    [MaxLength(30)]
    public string Estado { get; set; } = "recibida"; // recibida | respondida | aceptada | rechazada

    [Column("descripcion")]
    public string? Descripcion { get; set; }

    [Column("fecha")]
    public DateTime Fecha { get; set; } = DateTime.UtcNow;

    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navegación
    [ForeignKey("IdCliente")]
    public Cliente? Cliente { get; set; }

    [ForeignKey("IdEmpresa")]
    public Empresa? Empresa { get; set; }

    [ForeignKey("IdResponsable")]
    public Usuario? Responsable { get; set; }

    public Presupuesto? Presupuesto { get; set; }
}
