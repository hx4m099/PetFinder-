using ENTITY.Entities;

namespace DAL.Interfaces;

/// <summary>
/// Contrato de acceso a datos para la entidad <see cref="Usuario"/>.
/// La implementación en memoria (seed) se sustituirá por EF Core sin modificar esta interfaz.
/// </summary>
public interface IUsuarioRepository
{
    /// <summary>Obtiene todos los usuarios activos.</summary>
    Task<IEnumerable<Usuario>> ObtenerTodosAsync();

    /// <summary>Obtiene un usuario por su identificador.</summary>
    Task<Usuario?> ObtenerPorIdAsync(int id);

    /// <summary>Obtiene un usuario por su correo electrónico.</summary>
    Task<Usuario?> ObtenerPorEmailAsync(string email);

    /// <summary>Crea un nuevo usuario y devuelve la entidad con Id asignado.</summary>
    Task<Usuario> CrearAsync(Usuario usuario);

    /// <summary>Actualiza los datos de un usuario existente.</summary>
    Task ActualizarAsync(Usuario usuario);

    /// <summary>Desactiva (soft-delete) un usuario por su Id.</summary>
    Task EliminarAsync(int id);

    /// <summary>Verifica si un correo electrónico ya está registrado.</summary>
    Task<bool> EmailExisteAsync(string email);
}
