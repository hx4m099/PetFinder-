using ENTITY.Entities;

namespace DAL.Interfaces;

/// <summary>
/// Contrato de acceso a datos para la entidad <see cref="Refugio"/>.
/// </summary>
public interface IRefugioRepository
{
    /// <summary>Obtiene todos los refugios activos.</summary>
    Task<IEnumerable<Refugio>> ObtenerTodosAsync();

    /// <summary>Obtiene un refugio por su identificador.</summary>
    Task<Refugio?> ObtenerPorIdAsync(int id);

    /// <summary>Obtiene el refugio gestionado por un usuario administrador.</summary>
    Task<Refugio?> ObtenerPorUsuarioAdminAsync(int usuarioAdminId);

    /// <summary>Busca refugios por ciudad.</summary>
    Task<IEnumerable<Refugio>> BuscarPorCiudadAsync(string ciudad);

    /// <summary>Crea un nuevo refugio.</summary>
    Task<Refugio> CrearAsync(Refugio refugio);

    /// <summary>Actualiza los datos de un refugio.</summary>
    Task ActualizarAsync(Refugio refugio);

    /// <summary>Desactiva un refugio.</summary>
    Task EliminarAsync(int id);
}
