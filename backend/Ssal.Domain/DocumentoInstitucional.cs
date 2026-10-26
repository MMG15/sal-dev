using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Ssal.Domain;

// Biblioteca de documentos reutilizables del laboratorio (certificación ISO 9001, procedimientos,
// disposiciones, habilitantes) — se cargan una vez y se adjuntan desde ahí a cualquier presupuesto,
// en vez de subir el archivo de nuevo cada vez.
[Table("documentos_institucionales")]
public class DocumentoInstitucional
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    // Nombre para mostrar en la biblioteca, ej. "Certificación ISO 9001".
    [Required]
    [Column("nombre")]
    [MaxLength(200)]
    public string Nombre { get; set; } = string.Empty;

    [Required]
    [Column("nombre_original")]
    [MaxLength(300)]
    public string NombreOriginal { get; set; } = string.Empty;

    [Required]
    [Column("nombre_almacenado")]
    [MaxLength(300)]
    public string NombreAlmacenado { get; set; } = string.Empty;

    [Required]
    [Column("tipo_mime")]
    [MaxLength(150)]
    public string TipoMime { get; set; } = string.Empty;

    [Column("tamanio_bytes")]
    public long TamañoBytes { get; set; }

    [Required]
    [Column("subido_por")]
    public int SubidoPor { get; set; }

    [Column("subido_en")]
    public DateTime SubidoEn { get; set; } = DateTime.UtcNow;

    // Navegación
    [ForeignKey("SubidoPor")]
    public Usuario SubidoPorUsuario { get; set; } = null!;
}
