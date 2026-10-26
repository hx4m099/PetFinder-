using ENTITY.Entities;
using ENTITY.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DATOS.Configurations;

/// <summary>
/// Configuración Fluent API para la entidad <see cref="Usuario"/>.
/// </summary>
public class UsuarioConfiguration : IEntityTypeConfiguration<Usuario>
{
    public void Configure(EntityTypeBuilder<Usuario> builder)
    {
        builder.ToTable("USUARIOS");

        builder.HasKey(u => u.Id);
        builder.Property(u => u.Id).HasColumnName("ID").ValueGeneratedOnAdd();

        builder.Property(u => u.Nombre)
               .HasColumnName("NOMBRE").HasMaxLength(150).IsRequired();

        builder.Property(u => u.Email)
               .HasColumnName("EMAIL").HasMaxLength(200).IsRequired();
        builder.HasIndex(u => u.Email).IsUnique();

        builder.Property(u => u.PasswordHash)
               .HasColumnName("PASSWORD_HASH").HasMaxLength(500).IsRequired();

        builder.Property(u => u.Telefono)
               .HasColumnName("TELEFONO").HasMaxLength(20);

        builder.Property(u => u.Ciudad)
               .HasColumnName("CIUDAD").HasMaxLength(100);

        builder.Property(u => u.Rol)
               .HasColumnName("ROL").HasConversion<int>();

        builder.Property(u => u.FechaRegistro)
               .HasColumnName("FECHA_REGISTRO");

        builder.Property(u => u.Activo)
               .HasColumnName("ACTIVO").HasConversion<int>();
    }
}
