using DAL.Interfaces;
using DAL.Seed;
using ENTITY.Entities;
using ENTITY.Enums;

namespace DAL.Repositories;

/// <summary>
/// Implementación en memoria del repositorio de mascotas.
/// </summary>
public class MascotaRepository : IMascotaRepository
{
    private readonly InMemoryDataStore _store;

    public MascotaRepository(InMemoryDataStore store) => _store = store;

    /// <inheritdoc/>
    public Task<IEnumerable<Mascota>> ObtenerDisponiblesAsync()
        => Task.FromResult(_store.Mascotas.Where(m => m.Disponible).AsEnumerable());

    /// <inheritdoc/>
    public Task<IEnumerable<Mascota>> BuscarAsync(
        int? refugioId = null,
        Especie? especie = null,
        int? edadMaxMeses = null,
        bool soloDisponibles = true)
    {
        var query = _store.Mascotas.AsEnumerable();

        if (soloDisponibles)
            query = query.Where(m => m.Disponible);
        if (refugioId.HasValue)
            query = query.Where(m => m.RefugioId == refugioId.Value);
        if (especie.HasValue)
            query = query.Where(m => m.Especie == especie.Value);
        if (edadMaxMeses.HasValue)
            query = query.Where(m => m.EdadMeses <= edadMaxMeses.Value);

        return Task.FromResult(query);
    }

    /// <inheritdoc/>
    public Task<Mascota?> ObtenerPorIdAsync(int id)
        => Task.FromResult(_store.Mascotas.FirstOrDefault(m => m.Id == id));

    /// <inheritdoc/>
    public Task<IEnumerable<Mascota>> ObtenerPorRefugioAsync(int refugioId)
        => Task.FromResult(_store.Mascotas.Where(m => m.RefugioId == refugioId).AsEnumerable());

    /// <inheritdoc/>
    public Task<Mascota> CrearAsync(Mascota mascota)
    {
        mascota.Id = _store.Mascotas.Any()
            ? _store.Mascotas.Max(m => m.Id) + 1
            : 1;
        _store.Mascotas.Add(mascota);
        return Task.FromResult(mascota);
    }

    /// <inheritdoc/>
    public Task ActualizarAsync(Mascota mascota)
    {
        var index = _store.Mascotas.FindIndex(m => m.Id == mascota.Id);
        if (index >= 0)
            _store.Mascotas[index] = mascota;
        return Task.CompletedTask;
    }

    /// <inheritdoc/>
    public Task EliminarAsync(int id)
    {
        var mascota = _store.Mascotas.FirstOrDefault(m => m.Id == id);
        if (mascota is not null)
            mascota.Disponible = false;
        return Task.CompletedTask;
    }
}
