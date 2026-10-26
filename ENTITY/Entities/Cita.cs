using System.ComponentModel.DataAnnotations;
using ENTITY.Enums;

namespace ENTITY.Entities;

/// <summary>
/// Representa una cita agendada por un adoptante para conocer
/// a una mascota en el refugio y potencialmente adoptarla.
/// </summary>
public class Cita
{
    /// <summary>Identificador único de la cita.</summary>
    public int Id { get; set; }

    /// <summary>Fecha y hora programada para la cita.</summary>
    [Required(ErrorMessage = "La fecha y hora de la cita son obligatorias.")]
    public DateTime FechaHora { get; set; }

    /// <summary>Estado actual de la cita.</summary>
    public EstadoCita Estado { get; set; } = EstadoCita.Pendiente;

    /// <summary>Notas adicionales del adoptante para la cita.</summary>
    [MaxLength(500)]
    public string? NotasAdoptante { get; set; }

    /// <summary>Notas del refugio (confirmación, instrucciones, etc.).</summary>
    [MaxLength(500)]
    public string? NotasRefugio { get; set; }

    /// <summary>Fecha en que se creó la cita.</summary>
    public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;

    /// <summary>Fecha de la última actualización de estado.</summary>
    public DateTime? FechaActualizacion { get; set; }

    // Claves foráneas
    /// <summary>ID del adoptante que agendó la cita.</summary>
    public int AdoptanteId { get; set; }

    /// <summary>Adoptante que agendó la cita.</summary>
    public Usuario? Adoptante { get; set; }

    /// <summary>ID de la mascota para la que se agenda la cita.</summary>
    public int MascotaId { get; set; }

    /// <summary>Mascota para la cual se agenda la cita.</summary>
    public Mascota? Mascota { get; set; }

    /// <summary>ID del refugio donde se realizará la cita.</summary>
    public int RefugioId { get; set; }

    /// <summary>Refugio donde se realizará la cita.</summary>
    public Refugio? Refugio { get; set; }
}
