using DAL.Interfaces;
using DAL.Seed;
using ENTITY.Entities;

namespace DAL.Repositories;

/// <summary>
/// Implementación en memoria del repositorio de usuarios.
/// Utiliza <see cref="InMemoryDataStore"/> como fuente de datos compartida.
/// Se reemplazará por EF Core sin modificar BLL ni la GUI.
/// </summary>
public class UsuarioRepository : IUsuarioRepository
{
    private readonly InMemoryDataStore _store;

    public UsuarioRepository(InMemoryDataStore store) => _store = store;

    /// <inheritdoc/>
    public Task<IEnumerable<Usuario>> ObtenerTodosAsync()
        => Task.FromResult(_store.Usuarios.Where(u => u.Activo).AsEnumerable());

    /// <inheritdoc/>
    public Task<Usuario?> ObtenerPorIdAsync(int id)
        => Task.FromResult(_store.Usuarios.FirstOrDefault(u => u.Id == id));

    /// <inheritdoc/>
    public Task<Usuario?> ObtenerPorEmailAsync(string email)
        => Task.FromResult(
            _store.Usuarios.FirstOrDefault(u =>
                u.Email.Equals(email, StringComparison.OrdinalIgnoreCase)));

    /// <inheritdoc/>
    public Task<Usuario> CrearAsync(Usuario usuario)
    {
        usuario.Id = _store.Usuarios.Any()
            ? _store.Usuarios.Max(u => u.Id) + 1
            : 1;
        _store.Usuarios.Add(usuario);
        return Task.FromResult(usuario);
    }

    /// <inheritdoc/>
    public Task ActualizarAsync(Usuario usuario)
    {
        var index = _store.Usuarios.FindIndex(u => u.Id == usuario.Id);
        if (index >= 0)
            _store.Usuarios[index] = usuario;
        return Task.CompletedTask;
    }

    /// <inheritdoc/>
    public Task EliminarAsync(int id)
    {
        var usuario = _store.Usuarios.FirstOrDefault(u => u.Id == id);
        if (usuario is not null)
            usuario.Activo = false;
        return Task.CompletedTask;
    }

    /// <inheritdoc/>
    public Task<bool> EmailExisteAsync(string email)
        => Task.FromResult(
            _store.Usuarios.Any(u =>
                u.Email.Equals(email, StringComparison.OrdinalIgnoreCase)));
}
