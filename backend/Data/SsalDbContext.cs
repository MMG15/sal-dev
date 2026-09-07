using Microsoft.EntityFrameworkCore;
using Ssal.Api.Models;

namespace Ssal.Api.Data;

public class SsalDbContext(DbContextOptions<SsalDbContext> options) : DbContext(options)
{
    public DbSet<Empresa> Empresas => Set<Empresa>();
    public DbSet<Cliente> Clientes => Set<Cliente>();
    public DbSet<Rol> Roles => Set<Rol>();
    public DbSet<Usuario> Usuarios => Set<Usuario>();
    public DbSet<Consulta> Consultas => Set<Consulta>();
    public DbSet<Presupuesto> Presupuestos => Set<Presupuesto>();
    public DbSet<Sse> Sses => Set<Sse>();
    public DbSet<RotuloInterno> RotulosInternos => Set<RotuloInterno>();
    public DbSet<AuditoriaRotulo> AuditoriasRotulo => Set<AuditoriaRotulo>();
    public DbSet<Muestra> Muestras => Set<Muestra>();
    public DbSet<GrupoAnalisis> GruposAnalisis => Set<GrupoAnalisis>();
    public DbSet<Analisis> Analisis => Set<Analisis>();
    public DbSet<Resultado> Resultados => Set<Resultado>();
    public DbSet<Factura> Facturas => Set<Factura>();
    public DbSet<FirmaDocumento> FirmasDocumento => Set<FirmaDocumento>();
    public DbSet<PresupuestoAnalisis> PresupuestosAnalisis => Set<PresupuestoAnalisis>();
    public DbSet<PresupuestoArchivo> PresupuestosArchivos => Set<PresupuestoArchivo>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Índices únicos
        modelBuilder.Entity<Usuario>()
            .HasIndex(u => u.Email)
            .IsUnique();

        modelBuilder.Entity<RotuloInterno>()
            .HasIndex(r => r.NumeroUnico)
            .IsUnique();

        modelBuilder.Entity<Rol>()
            .HasIndex(r => r.Codigo)
            .IsUnique();

        // Evitar cascade delete en ciclos de FK
        modelBuilder.Entity<Consulta>()
            .HasOne(c => c.Responsable)
            .WithMany()
            .HasForeignKey(c => c.IdResponsable)
            .OnDelete(DeleteBehavior.SetNull);

        modelBuilder.Entity<Presupuesto>()
            .HasOne(p => p.CreadoPorUsuario)
            .WithMany()
            .HasForeignKey(p => p.CreadoPor)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<PresupuestoArchivo>()
            .HasOne(a => a.SubidoPorUsuario)
            .WithMany()
            .HasForeignKey(a => a.SubidoPor)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Sse>()
            .HasOne(s => s.CreadoPorUsuario)
            .WithMany()
            .HasForeignKey(s => s.CreadoPor)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<RotuloInterno>()
            .HasOne(r => r.AsignadoPorUsuario)
            .WithMany()
            .HasForeignKey(r => r.AsignadoPor)
            .OnDelete(DeleteBehavior.Restrict);

        // Seed: roles del sistema según el documento
        modelBuilder.Entity<Rol>().HasData(
            new Rol { IdRol = 1, Codigo = "ROL-01", Nombre = "Administrador general / Director Técnico", Descripcion = "Acceso total. Aprobación final con firma digital." },
            new Rol { IdRol = 2, Codigo = "ROL-02", Nombre = "Administrativo", Descripcion = "Registro de consultas, clientes, SSE y rótulos." },
            new Rol { IdRol = 3, Codigo = "ROL-03", Nombre = "Presupuestos", Descripcion = "Generación y envío de presupuestos." },
            new Rol { IdRol = 4, Codigo = "ROL-04", Nombre = "Cobranzas", Descripcion = "Facturación y seguimiento de deudas." },
            new Rol { IdRol = 5, Codigo = "ROL-05", Nombre = "Cliente externo", Descripcion = "Solo lectura en portal web." },
            new Rol { IdRol = 6, Codigo = "ROL-06", Nombre = "Administrador de sistema", Descripcion = "Backoffice técnico. Exclusivo del equipo de desarrollo." },
            new Rol { IdRol = 7, Codigo = "ROL-07", Nombre = "Analista", Descripcion = "Realiza análisis y carga resultados." },
            new Rol { IdRol = 8, Codigo = "ROL-08", Nombre = "Responsable de área MIC", Descripcion = "Supervisa Microbiología. Firma digital de documentos." },
            new Rol { IdRol = 9, Codigo = "ROL-09", Nombre = "Responsable de área FQ", Descripcion = "Supervisa Físico Química. Firma digital de documentos." }
        );
    }
}
