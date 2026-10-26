using System.ComponentModel.DataAnnotations;
using ENTITY.Enums;

namespace ENTITY.Entities;

/// <summary>
/// Representa una mascota publicada por un refugio para adopción.
/// </summary>
public class Mascota
{
    /// <summary>Identificador único de la mascota.</summary>
    public int Id { get; set; }

    /// <summary>Nombre de la mascota.</summary>
    [Required(ErrorMessage = "El nombre de la mascota es obligatorio.")]
    [MaxLength(100)]
    public string Nombre { get; set; } = string.Empty;

    /// <summary>Especie de la mascota (perro, gato, etc.).</summary>
    public Especie Especie { get; set; }

    /// <summary>Raza o tipo de la mascota.</summary>
    [MaxLength(100)]
    public string? Raza { get; set; }

    /// <summary>Edad estimada en meses.</summary>
    [Range(0, 360, ErrorMessage = "La edad debe estar entre 0 y 360 meses.")]
    public int EdadMeses { get; set; }

    /// <summary>Descripción de la personalidad y características de la mascota.</summary>
    [MaxLength(1000)]
    public string? Descripcion { get; set; }

    /// <summary>URL o ruta local de la foto principal de la mascota.</summary>
    [MaxLength(500)]
    public string? FotoUrl { get; set; }

    /// <summary>Indica si la mascota está disponible para adopción.</summary>
    public bool Disponible { get; set; } = true;

    /// <summary>Indica si la mascota está vacunada.</summary>
    public bool Vacunada { get; set; }

    /// <summary>Indica si la mascota está castrada/esterilizada.</summary>
    public bool Castrada { get; set; }

    /// <summary>Fecha en que la mascota fue registrada en el sistema.</summary>
    public DateTime FechaRegistro { get; set; } = DateTime.UtcNow;

    // Clave foránea
    /// <summary>ID del refugio que publicó esta mascota.</summary>
    public int RefugioId { get; set; }

    /// <summary>Refugio al que pertenece la mascota.</summary>
    public Refugio? Refugio { get; set; }

    // Navegación
    /// <summary>Citas agendadas para conocer/adoptar esta mascota.</summary>
    public ICollection<Cita> Citas { get; set; } = new List<Cita>();
}
