using DAL.Interfaces;
using DAL.Seed;
using ENTITY.Constants;
using ENTITY.Entities;
using ENTITY.Enums;

namespace DAL.Repositories;

/// <summary>
/// Implementación en memoria del repositorio de citas.
/// Incluye validación de solapamiento de horarios en memoria.
/// </summary>
public class CitaRepository : ICitaRepository
{
    private readonly InMemoryDataStore _store;

    public CitaRepository(InMemoryDataStore store) => _store = store;

    /// <inheritdoc/>
    public Task<IEnumerable<Cita>> ObtenerPorAdoptanteAsync(int adoptanteId)
        => Task.FromResult(_store.Citas.Where(c => c.AdoptanteId == adoptanteId).AsEnumerable());

    /// <inheritdoc/>
    public Task<IEnumerable<Cita>> ObtenerPorRefugioAsync(int refugioId)
        => Task.FromResult(_store.Citas.Where(c => c.RefugioId == refugioId).AsEnumerable());

    /// <inheritdoc/>
    public Task<Cita?> ObtenerPorIdAsync(int id)
        => Task.FromResult(_store.Citas.FirstOrDefault(c => c.Id == id));

    /// <inheritdoc/>
    public Task<bool> ExisteSolapamientoAsync(int refugioId, DateTime fechaHora, int? excluirCitaId = null)
    {
        // Una cita se considera solapada si está en el mismo refugio,
        // en el mismo bloque de 1 hora y no está cancelada.
        var ventanaInicio = fechaHora;
        var ventanaFin    = fechaHora.AddHours(PetFinderConstants.DuracionCitaHoras);

        var existe = _store.Citas.Any(c =>
            c.RefugioId == refugioId &&
            c.Estado != EstadoCita.Cancelada &&
            (excluirCitaId == null || c.Id != excluirCitaId.Value) &&
            c.FechaHora < ventanaFin &&
            c.FechaHora.AddHours(PetFinderConstants.DuracionCitaHoras) > ventanaInicio);

        return Task.FromResult(existe);
    }

    /// <inheritdoc/>
    public Task<Cita> CrearAsync(Cita cita)
    {
        cita.Id = _store.Citas.Any()
            ? _store.Citas.Max(c => c.Id) + 1
            : 1;
        _store.Citas.Add(cita);
        return Task.FromResult(cita);
    }

    /// <inheritdoc/>
    public Task ActualizarAsync(Cita cita)
    {
        var index = _store.Citas.FindIndex(c => c.Id == cita.Id);
        if (index >= 0)
            _store.Citas[index] = cita;
        return Task.CompletedTask;
    }

    /// <inheritdoc/>
    public Task<IEnumerable<Cita>> ObtenerPorEstadoAsync(EstadoCita estado)
        => Task.FromResult(_store.Citas.Where(c => c.Estado == estado).AsEnumerable());
}
