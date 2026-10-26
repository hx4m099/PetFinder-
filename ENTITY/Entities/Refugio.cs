using System.ComponentModel.DataAnnotations;

namespace ENTITY.Entities;

/// <summary>
/// Representa un refugio de animales aliado de PetFinder
/// que publica mascotas disponibles para adopción.
/// </summary>
public class Refugio
{
    /// <summary>Identificador único del refugio.</summary>
    public int Id { get; set; }

    /// <summary>Nombre oficial del refugio.</summary>
    [Required(ErrorMessage = "El nombre del refugio es obligatorio.")]
    [MaxLength(200)]
    public string Nombre { get; set; } = string.Empty;

    /// <summary>Descripción del refugio y su misión.</summary>
    [MaxLength(1000)]
    public string? Descripcion { get; set; }

    /// <summary>Dirección física del refugio.</summary>
    [MaxLength(300)]
    public string? Direccion { get; set; }

    /// <summary>Ciudad donde se ubica el refugio.</summary>
    [Required]
    [MaxLength(100)]
    public string Ciudad { get; set; } = string.Empty;

    /// <summary>Correo electrónico de contacto del refugio.</summary>
    [EmailAddress]
    [MaxLength(200)]
    public string? Email { get; set; }

    /// <summary>Teléfono de contacto del refugio.</summary>
    [Phone]
    [MaxLength(20)]
    public string? Telefono { get; set; }

    /// <summary>URL o ruta de la foto/logo del refugio.</summary>
    [MaxLength(500)]
    public string? FotoUrl { get; set; }

    /// <summary>Horario de atención en formato texto legible.</summary>
    [MaxLength(200)]
    public string? HorarioAtencion { get; set; }

    /// <summary>Propiedad de acceso directo / alias para horario.</summary>
    public string? Horario
    {
        get => HorarioAtencion;
        set => HorarioAtencion = value;
    }

    /// <summary>Coordenada geográfica de latitud para el mapa.</summary>
    public double? Latitud { get; set; }

    /// <summary>Coordenada geográfica de longitud para el mapa.</summary>
    public double? Longitud { get; set; }

    /// <summary>Indica si el refugio está activo en la plataforma.</summary>
    public bool Activo { get; set; } = true;

    /// <summary>Fecha en que el refugio se registró.</summary>
    public DateTime FechaRegistro { get; set; } = DateTime.UtcNow;

    // Clave foránea al usuario administrador del refugio
    /// <summary>ID del usuario que gestiona este refugio.</summary>
    public int UsuarioAdminId { get; set; }

    /// <summary>Usuario administrador del refugio.</summary>
    public Usuario? UsuarioAdmin { get; set; }

    // Navegación
    /// <summary>Mascotas publicadas por este refugio.</summary>
    public ICollection<Mascota> Mascotas { get; set; } = new List<Mascota>();

    /// <summary>Citas agendadas en este refugio.</summary>
    public ICollection<Cita> Citas { get; set; } = new List<Cita>();
}
