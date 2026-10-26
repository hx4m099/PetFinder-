using ENTITY.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DATOS.Configurations;

/// <summary>
/// Configuración Fluent API para la entidad <see cref="Cita"/>.
/// </summary>
public class CitaConfiguration : IEntityTypeConfiguration<Cita>
{
    public void Configure(EntityTypeBuilder<Cita> builder)
    {
        builder.ToTable("CITAS");

        builder.HasKey(c => c.Id);
        builder.Property(c => c.Id).HasColumnName("ID").ValueGeneratedOnAdd();

        builder.Property(c => c.FechaHora)
               .HasColumnName("FECHA_HORA").IsRequired();

        builder.Property(c => c.Estado)
               .HasColumnName("ESTADO").HasConversion<int>();

        builder.Property(c => c.NotasAdoptante)
               .HasColumnName("NOTAS_ADOPTANTE").HasMaxLength(500);

        builder.Property(c => c.NotasRefugio)
               .HasColumnName("NOTAS_REFUGIO").HasMaxLength(500);

        builder.Property(c => c.FechaCreacion)
               .HasColumnName("FECHA_CREACION");

        builder.Property(c => c.FechaActualizacion)
               .HasColumnName("FECHA_ACTUALIZACION");

        builder.Property(c => c.AdoptanteId)
               .HasColumnName("ADOPTANTE_ID");

        builder.Property(c => c.MascotaId)
               .HasColumnName("MASCOTA_ID");

        builder.Property(c => c.RefugioId)
               .HasColumnName("REFUGIO_ID");

        // Índice compuesto para validar duplicados
        builder.HasIndex(c => new { c.RefugioId, c.FechaHora })
               .HasDatabaseName("IX_CITAS_REFUGIO_FECHA");

        builder.HasOne(c => c.Adoptante)
               .WithMany(u => u.Citas)
               .HasForeignKey(c => c.AdoptanteId)
               .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(c => c.Mascota)
               .WithMany(m => m.Citas)
               .HasForeignKey(c => c.MascotaId)
               .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(c => c.Refugio)
               .WithMany(r => r.Citas)
               .HasForeignKey(c => c.RefugioId)
               .OnDelete(DeleteBehavior.Restrict);
    }
}
