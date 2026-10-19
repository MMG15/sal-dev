using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Ssal.Domain;

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

    // Preparado para la futura vinculación automática de consultas por mail o WhatsApp (RF-06).
    // Hoy nadie los completa: toda consulta cargada desde el sistema queda en "manual" / sin externo_id.
    [Column("origen")]
    [MaxLength(20)]
    public string Origen { get; set; } = "manual"; // manual | automatico

    // Identificador del mensaje de origen (Message-ID del mail, id del mensaje de WhatsApp, etc.)
    // Sirve para no crear la misma consulta dos veces si el canal reintenta o se vuelve a leer.
    [Column("externo_id")]
    [MaxLength(150)]
    public string? ExternoId { get; set; }

    // Navegación
    [ForeignKey("IdCliente")]
    public Cliente? Cliente { get; set; }

    [ForeignKey("IdEmpresa")]
    public Empresa? Empresa { get; set; }

    [ForeignKey("IdResponsable")]
    public Usuario? Responsable { get; set; }

    public Presupuesto? Presupuesto { get; set; }
}
