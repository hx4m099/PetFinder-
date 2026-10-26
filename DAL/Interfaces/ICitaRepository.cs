using ENTITY.Entities;
using ENTITY.Enums;

namespace DAL.Interfaces;

/// <summary>
/// Contrato de acceso a datos para la entidad <see cref="Cita"/>.
/// </summary>
public interface ICitaRepository
{
    /// <summary>Obtiene todas las citas de un adoptante.</summary>
    Task<IEnumerable<Cita>> ObtenerPorAdoptanteAsync(int adoptanteId);

    /// <summary>Obtiene todas las citas de un refugio.</summary>
    Task<IEnumerable<Cita>> ObtenerPorRefugioAsync(int refugioId);

    /// <summary>Obtiene una cita por su identificador.</summary>
    Task<Cita?> ObtenerPorIdAsync(int id);

    /// <summary>
    /// Verifica si existe una cita para el mismo refugio y horario (para evitar duplicados).
    /// </summary>
    Task<bool> ExisteSolapamientoAsync(int refugioId, DateTime fechaHora, int? excluirCitaId = null);

    /// <summary>Crea una nueva cita y devuelve la entidad con Id asignado.</summary>
    Task<Cita> CrearAsync(Cita cita);

    /// <summary>Actualiza el estado u otros campos de una cita.</summary>
    Task ActualizarAsync(Cita cita);

    /// <summary>Obtiene citas filtradas por estado.</summary>
    Task<IEnumerable<Cita>> ObtenerPorEstadoAsync(EstadoCita estado);
}
