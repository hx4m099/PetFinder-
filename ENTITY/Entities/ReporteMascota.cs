using System.ComponentModel.DataAnnotations;
using ENTITY.Enums;

namespace ENTITY.Entities;

/// <summary>
/// Representa un reporte de mascota perdida o encontrada,
/// creado por un usuario para iniciar la búsqueda de coincidencias.
/// </summary>
public class ReporteMascota
{
    /// <summary>Identificador único del reporte.</summary>
    public int Id { get; set; }

    /// <summary>Tipo de reporte: mascota perdida o encontrada.</summary>
    public TipoReporte TipoReporte { get; set; }

    /// <summary>Especie de la mascota reportada.</summary>
    public Especie Especie { get; set; }

    /// <summary>Nombre de la mascota (puede estar vacío si es encontrada).</summary>
    [MaxLength(100)]
    public string? NombreMascota { get; set; }

    /// <summary>Raza estimada de la mascota.</summary>
    [MaxLength(100)]
    public string? Raza { get; set; }

    /// <summary>Color predominante del pelaje.</summary>
    [MaxLength(100)]
    public string? ColorPelaje { get; set; }

    /// <summary>Ciudad donde se perdió o encontró la mascota.</summary>
    [Required(ErrorMessage = "La ciudad es obligatoria para el filtrado.")]
    [MaxLength(100)]
    public string Ciudad { get; set; } = string.Empty;

    /// <summary>Descripción adicional de características o circunstancias.</summary>
    [MaxLength(1000)]
    public string? Descripcion { get; set; }

    /// <summary>URL o ruta local de la foto de la mascota reportada.</summary>
    [MaxLength(500)]
    public string? FotoUrl { get; set; }

    /// <summary>
    /// Vector de características de imagen serializado como JSON.
    /// Usado por el servicio de similitud para comparar fotos.
    /// </summary>
    [MaxLength(4000)]
    public string? VectorImagenJson { get; set; }

    /// <summary>Fecha y hora en que se creó el reporte.</summary>
    public DateTime FechaReporte { get; set; } = DateTime.UtcNow;

    /// <summary>Indica si el reporte sigue activo (no resuelto).</summary>
    public bool Activo { get; set; } = true;

    // Clave foránea
    /// <summary>ID del usuario que generó el reporte.</summary>
    public int UsuarioId { get; set; }

    /// <summary>Usuario que generó el reporte.</summary>
    public Usuario? Usuario { get; set; }
}
