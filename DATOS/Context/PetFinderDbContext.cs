using ENTITY.Entities;
using ENTITY.Enums;
using Microsoft.EntityFrameworkCore;

namespace DATOS.Context;

/// <summary>
/// DbContext principal de PetFinder.
/// Configura las entidades mediante Fluent API y está preparado para Oracle.
/// La cadena de conexión se suministra por inyección de dependencias desde appsettings.
/// </summary>
public class PetFinderDbContext : DbContext
{
    /// <summary>Constructor para inyección de dependencias.</summary>
    public PetFinderDbContext(DbContextOptions<PetFinderDbContext> options)
        : base(options) { }

    // ── DbSets ───────────────────────────────────────────────────────────────
    public DbSet<Usuario>       Usuarios       { get; set; }
    public DbSet<Refugio>       Refugios       { get; set; }
    public DbSet<Mascota>       Mascotas       { get; set; }
    public DbSet<ReporteMascota> ReportesMascota { get; set; }
    public DbSet<Cita>          Citas          { get; set; }

    /// <inheritdoc/>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Aplicar todas las configuraciones por convención de carpeta
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(PetFinderDbContext).Assembly);
    }
}
