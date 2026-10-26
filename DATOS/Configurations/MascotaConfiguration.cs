using ENTITY.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DATOS.Configurations;

/// <summary>
/// Configuración Fluent API para la entidad <see cref="Mascota"/>.
/// </summary>
public class MascotaConfiguration : IEntityTypeConfiguration<Mascota>
{
    public void Configure(EntityTypeBuilder<Mascota> builder)
    {
        builder.ToTable("MASCOTAS");

        builder.HasKey(m => m.Id);
        builder.Property(m => m.Id).HasColumnName("ID").ValueGeneratedOnAdd();

        builder.Property(m => m.Nombre)
               .HasColumnName("NOMBRE").HasMaxLength(100).IsRequired();

        builder.Property(m => m.Especie)
               .HasColumnName("ESPECIE").HasConversion<int>();

        builder.Property(m => m.Raza)
               .HasColumnName("RAZA").HasMaxLength(100);

        builder.Property(m => m.EdadMeses)
               .HasColumnName("EDAD_MESES");

        builder.Property(m => m.Descripcion)
               .HasColumnName("DESCRIPCION").HasMaxLength(1000);

        builder.Property(m => m.FotoUrl)
               .HasColumnName("FOTO_URL").HasMaxLength(500);

        builder.Property(m => m.Disponible)
               .HasColumnName("DISPONIBLE").HasConversion<int>();

        builder.Property(m => m.Vacunada)
               .HasColumnName("VACUNADA").HasConversion<int>();

        builder.Property(m => m.Castrada)
               .HasColumnName("CASTRADA").HasConversion<int>();

        builder.Property(m => m.FechaRegistro)
               .HasColumnName("FECHA_REGISTRO");

        builder.Property(m => m.RefugioId)
               .HasColumnName("REFUGIO_ID");

        builder.HasOne(m => m.Refugio)
               .WithMany(r => r.Mascotas)
               .HasForeignKey(m => m.RefugioId)
               .OnDelete(DeleteBehavior.Cascade);
    }
}
