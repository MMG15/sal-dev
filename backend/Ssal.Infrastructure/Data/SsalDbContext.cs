using Microsoft.EntityFrameworkCore;
using Ssal.Domain;

namespace Ssal.Infrastructure.Data;

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
    public DbSet<Informe> Informes => Set<Informe>();
    public DbSet<Factura> Facturas => Set<Factura>();
    public DbSet<FirmaDocumento> FirmasDocumento => Set<FirmaDocumento>();
    public DbSet<PresupuestoAnalisis> PresupuestosAnalisis => Set<PresupuestoAnalisis>();
    public DbSet<Paquete> Paquetes => Set<Paquete>();
    public DbSet<PaqueteAnalisis> PaquetesAnalisis => Set<PaqueteAnalisis>();
    public DbSet<PresupuestoPaquete> PresupuestosPaquetes => Set<PresupuestoPaquete>();
    public DbSet<PresupuestoArchivo> PresupuestosArchivos => Set<PresupuestoArchivo>();
    public DbSet<PortalUsuario> PortalUsuarios => Set<PortalUsuario>();
    public DbSet<AuditoriaSistema> Auditoria => Set<AuditoriaSistema>();
    public DbSet<CorreoEntrante> CorreosEntrantes => Set<CorreoEntrante>();
    public DbSet<CuentaCorreo> CuentasCorreo => Set<CuentaCorreo>();

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

        modelBuilder.Entity<AuditoriaSistema>()
            .HasIndex(a => a.Fecha);

        // Evita duplicar una consulta si el futuro canal automático (mail/WhatsApp) reintenta
        // o vuelve a leer el mismo mensaje. Nulo en todas las consultas cargadas a mano.
        modelBuilder.Entity<Consulta>()
            .HasIndex(c => c.ExternoId)
            .IsUnique();

        // Bandeja de correo: el Message-Id no se puede repetir (evita reimportar el mismo mail)
        modelBuilder.Entity<CorreoEntrante>()
            .HasIndex(c => c.ExternoId)
            .IsUnique();

        modelBuilder.Entity<CorreoEntrante>()
            .HasOne(c => c.ConsultaGenerada)
            .WithMany()
            .HasForeignKey(c => c.IdConsultaGenerada)
            .OnDelete(DeleteBehavior.SetNull);

        modelBuilder.Entity<CorreoEntrante>()
            .HasOne(c => c.UsuarioAccion)
            .WithMany()
            .HasForeignKey(c => c.IdUsuarioAccion)
            .OnDelete(DeleteBehavior.SetNull);

        modelBuilder.Entity<CorreoEntrante>()
            .HasOne(c => c.CuentaCorreo)
            .WithMany()
            .HasForeignKey(c => c.IdCuentaCorreo)
            .OnDelete(DeleteBehavior.SetNull);

        modelBuilder.Entity<CorreoEntrante>()
            .HasOne(c => c.ConsultaRelacionada)
            .WithMany()
            .HasForeignKey(c => c.IdConsultaRelacionada)
            .OnDelete(DeleteBehavior.SetNull);

        // Portal del cliente: un acceso por cliente y email único
        modelBuilder.Entity<PortalUsuario>()
            .HasIndex(p => p.Email)
            .IsUnique();

        modelBuilder.Entity<PortalUsuario>()
            .HasIndex(p => p.IdCliente)
            .IsUnique();

        modelBuilder.Entity<PortalUsuario>()
            .HasOne(p => p.Cliente)
            .WithMany()
            .HasForeignKey(p => p.IdCliente)
            .OnDelete(DeleteBehavior.Restrict);

        // Historial de firmas: cada evento queda ligado a su firmante (RF-35)
        modelBuilder.Entity<FirmaDocumento>()
            .HasOne(f => f.Firmante)
            .WithMany()
            .HasForeignKey(f => f.IdFirmante)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<FirmaDocumento>()
            .HasIndex(f => new { f.TipoDocumento, f.IdDocumento });

        // El número de factura (TANGO) no puede repetirse
        modelBuilder.Entity<Factura>()
            .HasIndex(f => f.Numero)
            .IsUnique();

        // Una SSE tiene una sola factura activa. Las anuladas no cuentan (cuando exista la anulación)
        modelBuilder.Entity<Factura>()
            .HasIndex(f => f.IdSse)
            .IsUnique()
            .HasFilter("estado <> 'anulada'");

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

        modelBuilder.Entity<Resultado>()
            .HasOne(r => r.CargadoPorUsuario)
            .WithMany()
            .HasForeignKey(r => r.CargadoPor)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Resultado>()
            .HasOne(r => r.ValidadoPorUsuario)
            .WithMany()
            .HasForeignKey(r => r.ValidadoPor)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Informe>()
            .HasOne(i => i.GeneradoPorUsuario)
            .WithMany()
            .HasForeignKey(i => i.GeneradoPor)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Informe>()
            .HasOne(i => i.Muestra)
            .WithOne(m => m.Informe)
            .HasForeignKey<Informe>(i => i.IdMuestra);

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
