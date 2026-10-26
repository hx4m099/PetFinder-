using ENTITY.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DATOS.Configurations;

/// <summary>
/// Configuración Fluent API para la entidad <see cref="ReporteMascota"/>.
/// </summary>
public class ReporteMascotaConfiguration : IEntityTypeConfiguration<ReporteMascota>
{
    public void Configure(EntityTypeBuilder<ReporteMascota> builder)
    {
        builder.ToTable("REPORTES_MASCOTA");

        builder.HasKey(r => r.Id);
        builder.Property(r => r.Id).HasColumnName("ID").ValueGeneratedOnAdd();

        builder.Property(r => r.TipoReporte)
               .HasColumnName("TIPO_REPORTE").HasConversion<int>();

        builder.Property(r => r.Especie)
               .HasColumnName("ESPECIE").HasConversion<int>();

        builder.Property(r => r.NombreMascota)
               .HasColumnName("NOMBRE_MASCOTA").HasMaxLength(100);

        builder.Property(r => r.Raza)
               .HasColumnName("RAZA").HasMaxLength(100);

        builder.Property(r => r.ColorPelaje)
               .HasColumnName("COLOR_PELAJE").HasMaxLength(100);

        builder.Property(r => r.Ciudad)
               .HasColumnName("CIUDAD").HasMaxLength(100).IsRequired();

        builder.Property(r => r.Descripcion)
               .HasColumnName("DESCRIPCION").HasMaxLength(1000);

        builder.Property(r => r.FotoUrl)
               .HasColumnName("FOTO_URL").HasMaxLength(500);

        builder.Property(r => r.VectorImagenJson)
               .HasColumnName("VECTOR_IMAGEN_JSON").HasMaxLength(4000);

        builder.Property(r => r.FechaReporte)
               .HasColumnName("FECHA_REPORTE");

        builder.Property(r => r.Activo)
               .HasColumnName("ACTIVO").HasConversion<int>();

        builder.Property(r => r.UsuarioId)
               .HasColumnName("USUARIO_ID");

        builder.HasOne(r => r.Usuario)
               .WithMany(u => u.Reportes)
               .HasForeignKey(r => r.UsuarioId)
               .OnDelete(DeleteBehavior.Cascade);
    }
}
