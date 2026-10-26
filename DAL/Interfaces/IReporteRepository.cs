using ENTITY.Entities;
using ENTITY.Enums;

namespace DAL.Interfaces;

/// <summary>
/// Contrato de acceso a datos para la entidad <see cref="ReporteMascota"/>.
/// </summary>
public interface IReporteRepository
{
    /// <summary>Obtiene todos los reportes activos.</summary>
    Task<IEnumerable<ReporteMascota>> ObtenerActivosAsync();

    /// <summary>Obtiene un reporte por su identificador.</summary>
    Task<ReporteMascota?> ObtenerPorIdAsync(int id);

    /// <summary>Obtiene los reportes de un usuario específico.</summary>
    Task<IEnumerable<ReporteMascota>> ObtenerPorUsuarioAsync(int usuarioId);

    /// <summary>
    /// Filtra reportes por especie, ciudad y fecha mínima (para comparación de similitud).
    /// </summary>
    Task<IEnumerable<ReporteMascota>> FiltrarParaComparacionAsync(
        Especie especie,
        string ciudad,
        DateTime fechaMinima,
        int? excluirReporteId = null);

    /// <summary>Crea un nuevo reporte de mascota.</summary>
    Task<ReporteMascota> CrearAsync(ReporteMascota reporte);

    /// <summary>Actualiza un reporte existente.</summary>
    Task ActualizarAsync(ReporteMascota reporte);

    /// <summary>Desactiva un reporte (mascota encontrada/resuelta).</summary>
    Task DesactivarAsync(int id);
}
