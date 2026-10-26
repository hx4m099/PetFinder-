using DAL.Interfaces;
using DAL.Seed;
using ENTITY.Constants;
using ENTITY.Entities;
using ENTITY.Enums;

namespace DAL.Repositories;

/// <summary>
/// Implementación en memoria del repositorio de reportes de mascotas.
/// </summary>
public class ReporteRepository : IReporteRepository
{
    private readonly InMemoryDataStore _store;

    public ReporteRepository(InMemoryDataStore store) => _store = store;

    /// <inheritdoc/>
    public Task<IEnumerable<ReporteMascota>> ObtenerActivosAsync()
        => Task.FromResult(_store.Reportes.Where(r => r.Activo).AsEnumerable());

    /// <inheritdoc/>
    public Task<ReporteMascota?> ObtenerPorIdAsync(int id)
        => Task.FromResult(_store.Reportes.FirstOrDefault(r => r.Id == id));

    /// <inheritdoc/>
    public Task<IEnumerable<ReporteMascota>> ObtenerPorUsuarioAsync(int usuarioId)
        => Task.FromResult(_store.Reportes.Where(r => r.UsuarioId == usuarioId).AsEnumerable());

    /// <inheritdoc/>
    public Task<IEnumerable<ReporteMascota>> FiltrarParaComparacionAsync(
        Especie especie,
        string ciudad,
        DateTime fechaMinima,
        int? excluirReporteId = null)
    {
        var resultado = _store.Reportes
            .Where(r =>
                r.Activo &&
                r.Especie == especie &&
                r.Ciudad.Equals(ciudad, StringComparison.OrdinalIgnoreCase) &&
                r.FechaReporte >= fechaMinima &&
                (excluirReporteId == null || r.Id != excluirReporteId.Value))
            .AsEnumerable();

        return Task.FromResult(resultado);
    }

    /// <inheritdoc/>
    public Task<ReporteMascota> CrearAsync(ReporteMascota reporte)
    {
        reporte.Id = _store.Reportes.Any()
            ? _store.Reportes.Max(r => r.Id) + 1
            : 1;
        _store.Reportes.Add(reporte);
        return Task.FromResult(reporte);
    }

    /// <inheritdoc/>
    public Task ActualizarAsync(ReporteMascota reporte)
    {
        var index = _store.Reportes.FindIndex(r => r.Id == reporte.Id);
        if (index >= 0)
            _store.Reportes[index] = reporte;
        return Task.CompletedTask;
    }

    /// <inheritdoc/>
    public Task DesactivarAsync(int id)
    {
        var reporte = _store.Reportes.FirstOrDefault(r => r.Id == id);
        if (reporte is not null)
            reporte.Activo = false;
        return Task.CompletedTask;
    }
}
