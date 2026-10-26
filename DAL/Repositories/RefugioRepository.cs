using DAL.Interfaces;
using DAL.Seed;
using ENTITY.Entities;

namespace DAL.Repositories;

/// <summary>
/// Implementación en memoria del repositorio de refugios.
/// </summary>
public class RefugioRepository : IRefugioRepository
{
    private readonly InMemoryDataStore _store;

    public RefugioRepository(InMemoryDataStore store) => _store = store;

    /// <inheritdoc/>
    public Task<IEnumerable<Refugio>> ObtenerTodosAsync()
        => Task.FromResult(_store.Refugios.Where(r => r.Activo).AsEnumerable());

    /// <inheritdoc/>
    public Task<Refugio?> ObtenerPorIdAsync(int id)
        => Task.FromResult(_store.Refugios.FirstOrDefault(r => r.Id == id));

    /// <inheritdoc/>
    public Task<Refugio?> ObtenerPorUsuarioAdminAsync(int usuarioAdminId)
        => Task.FromResult(
            _store.Refugios.FirstOrDefault(r => r.UsuarioAdminId == usuarioAdminId));

    /// <inheritdoc/>
    public Task<IEnumerable<Refugio>> BuscarPorCiudadAsync(string ciudad)
        => Task.FromResult(
            _store.Refugios.Where(r =>
                r.Activo &&
                r.Ciudad.Contains(ciudad, StringComparison.OrdinalIgnoreCase))
            .AsEnumerable());

    /// <inheritdoc/>
    public Task<Refugio> CrearAsync(Refugio refugio)
    {
        refugio.Id = _store.Refugios.Any()
            ? _store.Refugios.Max(r => r.Id) + 1
            : 1;
        _store.Refugios.Add(refugio);
        return Task.FromResult(refugio);
    }

    /// <inheritdoc/>
    public Task ActualizarAsync(Refugio refugio)
    {
        var index = _store.Refugios.FindIndex(r => r.Id == refugio.Id);
        if (index >= 0)
            _store.Refugios[index] = refugio;
        return Task.CompletedTask;
    }

    /// <inheritdoc/>
    public Task EliminarAsync(int id)
    {
        var refugio = _store.Refugios.FirstOrDefault(r => r.Id == id);
        if (refugio is not null)
            refugio.Activo = false;
        return Task.CompletedTask;
    }
}
