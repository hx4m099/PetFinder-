using BLL.Dtos;
using BLL.Exceptions;
using DAL.Interfaces;
using ENTITY.Constants;
using ENTITY.Enums;
using Microsoft.Extensions.Logging;

namespace BLL.Services;

// ── Interfaz del servicio de similitud ──────────────────────────────────────

/// <summary>
/// Contrato para el cálculo de similitud entre imágenes de mascotas.
/// Principio OCP: se puede cambiar la implementación (mock → real) sin modificar CoincidenciaService.
/// </summary>
public interface IImageSimilarityService
{
    /// <summary>
    /// Calcula la similitud entre dos imágenes representadas por sus rutas.
    /// </summary>
    /// <returns>Valor entre 0.0 (ninguna similitud) y 1.0 (idénticas).</returns>
    Task<double> CalcularSimilitudAsync(string? fotoUrlA, string? fotoUrlB);

    /// <summary>
    /// Extrae el vector de características de una imagen y lo serializa como JSON.
    /// En la implementación real usará embeddings de un modelo ML.
    /// </summary>
    Task<string?> ExtraerVectorAsync(string? fotoUrl);
}

// ── Implementación Mock ──────────────────────────────────────────────────────

/// <summary>
/// Implementación simulada del servicio de similitud de imágenes.
/// Devuelve porcentajes aleatorios pero reproducibles (basados en los nombres).
/// Se reemplazará por una implementación real con ML.NET o una API de visión.
/// </summary>
public class MockImageSimilarityService : IImageSimilarityService
{
    private readonly ILogger<MockImageSimilarityService> _logger;

    public MockImageSimilarityService(ILogger<MockImageSimilarityService> logger)
        => _logger = logger;

    /// <inheritdoc/>
    public Task<double> CalcularSimilitudAsync(string? fotoUrlA, string? fotoUrlB)
    {
        if (string.IsNullOrWhiteSpace(fotoUrlA) || string.IsNullOrWhiteSpace(fotoUrlB))
        {
            // Sin foto → similitud base baja (solo atributos textuales)
            return Task.FromResult(0.20);
        }

        // Similitud determinista basada en hash para reproducibilidad en pruebas
        var seed = Math.Abs((fotoUrlA + fotoUrlB).GetHashCode());
        var rng  = new Random(seed);
        // Rango realista: 30% - 95%
        var similitud = 0.30 + rng.NextDouble() * 0.65;

        _logger.LogDebug("Similitud mock entre '{A}' y '{B}': {S:P1}", fotoUrlA, fotoUrlB, similitud);
        return Task.FromResult(Math.Round(similitud, 4));
    }

    /// <inheritdoc/>
    public Task<string?> ExtraerVectorAsync(string? fotoUrl)
    {
        if (string.IsNullOrWhiteSpace(fotoUrl))
            return Task.FromResult<string?>(null);

        // Vector simulado: 128 valores aleatorios en [-1, 1]
        var seed   = Math.Abs(fotoUrl.GetHashCode());
        var rng    = new Random(seed);
        var vector = Enumerable.Range(0, 128).Select(_ => rng.NextDouble() * 2 - 1).ToArray();
        var json   = System.Text.Json.JsonSerializer.Serialize(vector);

        return Task.FromResult<string?>(json);
    }
}

// ── Servicio de coincidencias ────────────────────────────────────────────────

/// <summary>
/// Interfaz del servicio que busca coincidencias entre reportes de mascotas.
/// </summary>
public interface ICoincidenciaService
{
    /// <summary>
    /// Dado un reporte, filtra candidatos por especie/ciudad/fecha y luego
    /// los ordena por similitud de imagen de mayor a menor.
    /// </summary>
    Task<IEnumerable<CoincidenciaDto>> BuscarCoincidenciasAsync(int reporteId);
}

/// <summary>
/// Implementación del servicio de coincidencias.
/// Sigue el flujo: filtrado previo → cálculo de similitud → ordenamiento.
/// </summary>
public class CoincidenciaService : ICoincidenciaService
{
    private readonly IReporteRepository      _reporteRepo;
    private readonly IUsuarioRepository      _usuarioRepo;
    private readonly IImageSimilarityService _similarityService;
    private readonly ILogger<CoincidenciaService> _logger;

    public CoincidenciaService(
        IReporteRepository reporteRepo,
        IUsuarioRepository usuarioRepo,
        IImageSimilarityService similarityService,
        ILogger<CoincidenciaService> logger)
    {
        _reporteRepo       = reporteRepo;
        _usuarioRepo       = usuarioRepo;
        _similarityService = similarityService;
        _logger            = logger;
    }

    /// <inheritdoc/>
    public async Task<IEnumerable<CoincidenciaDto>> BuscarCoincidenciasAsync(int reporteId)
    {
        // 1. Cargar reporte origen
        var origen = await _reporteRepo.ObtenerPorIdAsync(reporteId)
            ?? throw new EntidadNoEncontradaException("ReporteMascota", reporteId);

        // 2. Filtro previo: especie + ciudad + fecha reciente
        //    El tipo opuesto al del reporte: si busco mi perro perdido, comparo con "encontradas"
        var fechaMinima = DateTime.UtcNow.AddDays(-PetFinderConstants.DiasFiltroReporteReciente);

        var candidatos = await _reporteRepo.FiltrarParaComparacionAsync(
            origen.Especie,
            origen.Ciudad,
            fechaMinima,
            excluirReporteId: reporteId);

        // Solo comparar con el tipo de reporte opuesto (Perdida ↔ Encontrada)
        var tipoOpuesto = origen.TipoReporte == TipoReporte.Perdida
            ? TipoReporte.Encontrada
            : TipoReporte.Perdida;

        candidatos = candidatos.Where(c => c.TipoReporte == tipoOpuesto).ToList();

        _logger.LogInformation(
            "Reporte {Id}: {N} candidatos tras filtrado previo (especie={E}, ciudad={C})",
            reporteId, candidatos.Count(), origen.Especie, origen.Ciudad);

        // 3. Calcular similitud para cada candidato
        var tareas = candidatos.Select(async c =>
        {
            var similitud = await _similarityService.CalcularSimilitudAsync(
                origen.FotoUrl, c.FotoUrl);
            var usuario = await _usuarioRepo.ObtenerPorIdAsync(c.UsuarioId);

            return new CoincidenciaDto(
                c.Id,
                c.TipoReporte,
                c.Especie,
                c.NombreMascota,
                c.Raza,
                c.ColorPelaje,
                c.Ciudad,
                c.FotoUrl,
                c.Descripcion,
                c.FechaReporte,
                similitud,
                usuario?.Nombre ?? "Usuario desconocido",
                usuario?.Telefono);
        });

        var resultados = await Task.WhenAll(tareas);

        // 4. Filtrar por umbral y ordenar de mayor a menor similitud
        return resultados
            .Where(r => r.PorcentajeSimilitud >= PetFinderConstants.UmbralSimilitud)
            .OrderByDescending(r => r.PorcentajeSimilitud)
            .Take(PetFinderConstants.MaxCoincidencias);
    }
}
