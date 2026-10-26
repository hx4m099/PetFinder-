using ENTITY.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DATOS.Configurations;

/// <summary>
/// Configuración Fluent API para la entidad <see cref="Refugio"/>.
/// </summary>
public class RefugioConfiguration : IEntityTypeConfiguration<Refugio>
{
    public void Configure(EntityTypeBuilder<Refugio> builder)
    {
        builder.ToTable("REFUGIOS");

        builder.HasKey(r => r.Id);
        builder.Property(r => r.Id).HasColumnName("ID").ValueGeneratedOnAdd();

        builder.Property(r => r.Nombre)
               .HasColumnName("NOMBRE").HasMaxLength(200).IsRequired();

        builder.Property(r => r.Descripcion)
               .HasColumnName("DESCRIPCION").HasMaxLength(1000);

        builder.Property(r => r.Direccion)
               .HasColumnName("DIRECCION").HasMaxLength(300);

        builder.Property(r => r.Ciudad)
               .HasColumnName("CIUDAD").HasMaxLength(100).IsRequired();

        builder.Property(r => r.Email)
               .HasColumnName("EMAIL").HasMaxLength(200);

        builder.Property(r => r.Telefono)
               .HasColumnName("TELEFONO").HasMaxLength(20);

        builder.Property(r => r.FotoUrl)
               .HasColumnName("FOTO_URL").HasMaxLength(500);

        builder.Property(r => r.HorarioAtencion)
               .HasColumnName("HORARIO_ATENCION").HasMaxLength(200);

        builder.Property(r => r.Latitud)
               .HasColumnName("LATITUD")
               .HasPrecision(10, 7);

        builder.Property(r => r.Longitud)
               .HasColumnName("LONGITUD")
               .HasPrecision(10, 7);

        builder.Ignore(r => r.Horario);

        builder.Property(r => r.Activo)
               .HasColumnName("ACTIVO").HasConversion<int>();

        builder.Property(r => r.FechaRegistro)
               .HasColumnName("FECHA_REGISTRO");

        builder.Property(r => r.UsuarioAdminId)
               .HasColumnName("USUARIO_ADMIN_ID");

        builder.HasOne(r => r.UsuarioAdmin)
               .WithMany()
               .HasForeignKey(r => r.UsuarioAdminId)
               .OnDelete(DeleteBehavior.Restrict);
    }
}
