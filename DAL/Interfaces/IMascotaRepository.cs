using ENTITY.Entities;
using ENTITY.Enums;

namespace DAL.Interfaces;

/// <summary>
/// Contrato de acceso a datos para la entidad <see cref="Mascota"/>.
/// </summary>
public interface IMascotaRepository
{
    /// <summary>Obtiene todas las mascotas disponibles para adopción.</summary>
    Task<IEnumerable<Mascota>> ObtenerDisponiblesAsync();

    /// <summary>Obtiene mascotas con filtros opcionales.</summary>
    Task<IEnumerable<Mascota>> BuscarAsync(
        int? refugioId = null,
        Especie? especie = null,
        int? edadMaxMeses = null,
        bool soloDisponibles = true);

    /// <summary>Obtiene una mascota por su identificador.</summary>
    Task<Mascota?> ObtenerPorIdAsync(int id);

    /// <summary>Obtiene todas las mascotas de un refugio.</summary>
    Task<IEnumerable<Mascota>> ObtenerPorRefugioAsync(int refugioId);

    /// <summary>Crea una nueva mascota.</summary>
    Task<Mascota> CrearAsync(Mascota mascota);

    /// <summary>Actualiza los datos de una mascota.</summary>
    Task ActualizarAsync(Mascota mascota);

    /// <summary>Elimina o desactiva una mascota.</summary>
    Task EliminarAsync(int id);
}
