using System.ComponentModel.DataAnnotations;
using ENTITY.Enums;

namespace ENTITY.Entities;

/// <summary>
/// Representa un usuario registrado en la plataforma PetFinder.
/// Puede tener uno de tres roles: Adoptante, Refugio o Administrador.
/// </summary>
public class Usuario
{
    /// <summary>Identificador único del usuario.</summary>
    public int Id { get; set; }

    /// <summary>Nombre completo del usuario.</summary>
    [Required(ErrorMessage = "El nombre es obligatorio.")]
    [MaxLength(150)]
    public string Nombre { get; set; } = string.Empty;

    /// <summary>Correo electrónico único usado para iniciar sesión.</summary>
    [Required(ErrorMessage = "El correo es obligatorio.")]
    [EmailAddress]
    [MaxLength(200)]
    public string Email { get; set; } = string.Empty;

    /// <summary>Contraseña almacenada con hash (bcrypt o PBKDF2).</summary>
    [Required]
    [MaxLength(500)]
    public string PasswordHash { get; set; } = string.Empty;

    /// <summary>Número de teléfono de contacto.</summary>
    [Phone]
    [MaxLength(20)]
    public string? Telefono { get; set; }

    /// <summary>Ciudad de residencia del usuario.</summary>
    [MaxLength(100)]
    public string? Ciudad { get; set; }

    /// <summary>Rol del usuario dentro del sistema.</summary>
    public Rol Rol { get; set; } = Rol.Adoptante;

    /// <summary>Fecha en que el usuario se registró.</summary>
    public DateTime FechaRegistro { get; set; } = DateTime.UtcNow;

    /// <summary>Indica si la cuenta está activa.</summary>
    public bool Activo { get; set; } = true;

    // Navegación
    /// <summary>Reportes de mascotas creados por este usuario.</summary>
    public ICollection<ReporteMascota> Reportes { get; set; } = new List<ReporteMascota>();

    /// <summary>Citas agendadas por este usuario como adoptante.</summary>
    public ICollection<Cita> Citas { get; set; } = new List<Cita>();
}
