using BLL.Dtos;
using BLL.Exceptions;
using DAL.Interfaces;
using ENTITY.Entities;
using Microsoft.Extensions.Logging;

namespace BLL.Services;

/// <summary>Interfaz del servicio de refugios.</summary>
public interface IRefugioService
{
    Task<IEnumerable<RefugioDto>> ObtenerTodosAsync();
    Task<RefugioDto> ObtenerPorIdAsync(int id);
    Task<RefugioDto?> ObtenerPorUsuarioAsync(int usuarioId);
    Task<RefugioDto> CrearAsync(RefugioFormDto dto, int usuarioAdminId);
    Task ActualizarAsync(int id, RefugioFormDto dto, int usuarioAdminId);

    /// <summary>
    /// Obtiene los refugios activos con coordenadas válidas para alimentar el mapa interactivo.
    /// Permite ordenar por cercanía al usuario y filtrar por especie disponible.
    /// </summary>
    Task<IEnumerable<RefugioMapaDto>> ObtenerParaMapaAsync(
        double? latUsuario = null,
        double? lonUsuario = null,
        string? especie = null);
}

/// <summary>Implementación del servicio de refugios.</summary>
public class RefugioService : IRefugioService
{
    private readonly IRefugioRepository _repo;
    private readonly ILogger<RefugioService> _logger;

    public RefugioService(IRefugioRepository repo, ILogger<RefugioService> logger)
    {
        _repo   = repo;
        _logger = logger;
    }

    public async Task<IEnumerable<RefugioDto>> ObtenerTodosAsync()
    {
        var refugios = await _repo.ObtenerTodosAsync();
        return refugios.Select(MapToDto);
    }

    public async Task<RefugioDto> ObtenerPorIdAsync(int id)
    {
        var r = await _repo.ObtenerPorIdAsync(id)
            ?? throw new EntidadNoEncontradaException("Refugio", id);
        return MapToDto(r);
    }

    public async Task<RefugioDto?> ObtenerPorUsuarioAsync(int usuarioId)
    {
        var r = await _repo.ObtenerPorUsuarioAdminAsync(usuarioId);
        return r is null ? null : MapToDto(r);
    }

    public async Task<RefugioDto> CrearAsync(RefugioFormDto dto, int usuarioAdminId)
    {
        var refugio = new Refugio
        {
            Nombre          = dto.Nombre.Trim(),
            Descripcion     = dto.Descripcion?.Trim(),
            Direccion       = dto.Direccion?.Trim(),
            Ciudad          = dto.Ciudad.Trim(),
            Email           = dto.Email?.Trim(),
            Telefono        = dto.Telefono?.Trim(),
            HorarioAtencion = dto.HorarioAtencion?.Trim(),
            UsuarioAdminId  = usuarioAdminId,
            Activo          = true,
            FechaRegistro   = DateTime.UtcNow
        };
        var creado = await _repo.CrearAsync(refugio);
        _logger.LogInformation("Refugio creado: {Nombre}", creado.Nombre);
        return MapToDto(creado);
    }

    public async Task ActualizarAsync(int id, RefugioFormDto dto, int usuarioAdminId)
    {
        var refugio = await _repo.ObtenerPorIdAsync(id)
            ?? throw new EntidadNoEncontradaException("Refugio", id);

        if (refugio.UsuarioAdminId != usuarioAdminId)
            throw new AccesoDenegadoException("Solo el administrador del refugio puede editarlo.");

        refugio.Nombre          = dto.Nombre.Trim();
        refugio.Descripcion     = dto.Descripcion?.Trim();
        refugio.Direccion       = dto.Direccion?.Trim();
        refugio.Ciudad          = dto.Ciudad.Trim();
        refugio.Email           = dto.Email?.Trim();
        refugio.Telefono        = dto.Telefono?.Trim();
        refugio.HorarioAtencion = dto.HorarioAtencion?.Trim();

        await _repo.ActualizarAsync(refugio);
    }

    /// <inheritdoc/>
    public async Task<IEnumerable<RefugioMapaDto>> ObtenerParaMapaAsync(
        double? latUsuario = null,
        double? lonUsuario = null,
        string? especie = null)
    {
        var todos = await _repo.ObtenerTodosAsync();

        // 1. Filtrar únicamente refugios activos con coordenadas válidas
        var validos = todos
            .Where(r => r.Activo && TieneCoordenadasValidas(r.Latitud, r.Longitud))
            .ToList();

        var dtos = new List<RefugioMapaDto>();

        foreach (var r in validos)
        {
            var disponibles = r.Mascotas?.Where(m => m.Disponible).ToList() ?? new List<Mascota>();
            int conteoDisponibles = disponibles.Count;
            var especies = disponibles
                .Select(m => m.Especie.ToString())
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();

            // Filtrar por especie si se especificó (e.g. Perro, Gato)
            if (!string.IsNullOrWhiteSpace(especie) &&
                !especie.Equals("Todos", StringComparison.OrdinalIgnoreCase))
            {
                if (!especies.Any(e => e.Equals(especie.Trim(), StringComparison.OrdinalIgnoreCase)))
                    continue;
            }

            double? distanciaKm = null;
            if (latUsuario.HasValue && lonUsuario.HasValue)
            {
                distanciaKm = CalcularDistanciaHaversine(
                    latUsuario.Value,
                    lonUsuario.Value,
                    r.Latitud!.Value,
                    r.Longitud!.Value);
            }

            dtos.Add(new RefugioMapaDto(
                r.Id,
                r.Nombre,
                r.Direccion,
                r.HorarioAtencion ?? r.Horario,
                r.Latitud!.Value,
                r.Longitud!.Value,
                r.Telefono,
                r.FotoUrl,
                conteoDisponibles,
                especies,
                distanciaKm
            ));
        }

        // Si se proporcionó ubicación, ordenar por cercanía; de lo contrario, alfabéticamente
        return (latUsuario.HasValue && lonUsuario.HasValue)
            ? dtos.OrderBy(d => d.DistanciaKm ?? double.MaxValue)
            : dtos.OrderBy(d => d.Nombre);
    }

    /// <summary>
    /// Determina si una coordenada geográfica es válida.
    /// </summary>
    public static bool TieneCoordenadasValidas(double? lat, double? lon)
    {
        if (!lat.HasValue || !lon.HasValue) return false;
        if (double.IsNaN(lat.Value) || double.IsInfinity(lat.Value)) return false;
        if (double.IsNaN(lon.Value) || double.IsInfinity(lon.Value)) return false;
        if (lat.Value < -90.0 || lat.Value > 90.0) return false;
        if (lon.Value < -180.0 || lon.Value > 180.0) return false;
        // Excluir 0,0 por ser valor nulo por defecto en algunos sistemas
        if (Math.Abs(lat.Value) < 0.0001 && Math.Abs(lon.Value) < 0.0001) return false;

        return true;
    }

    /// <summary>
    /// Calcula la distancia geodésica en kilómetros entre dos coordenadas usando la fórmula de Haversine.
    /// </summary>
    public static double CalcularDistanciaHaversine(double lat1, double lon1, double lat2, double lon2)
    {
        const double RadioTierraKm = 6371.0;
        var dLat = (lat2 - lat1) * Math.PI / 180.0;
        var dLon = (lon2 - lon1) * Math.PI / 180.0;
        var rLat1 = lat1 * Math.PI / 180.0;
        var rLat2 = lat2 * Math.PI / 180.0;

        var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                Math.Cos(rLat1) * Math.Cos(rLat2) *
                Math.Sin(dLon / 2) * Math.Sin(dLon / 2);

        var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
        return Math.Round(RadioTierraKm * c, 2);
    }

    private static RefugioDto MapToDto(Refugio r) =>
        new(r.Id, r.Nombre, r.Descripcion, r.Direccion,
            r.Ciudad, r.Email, r.Telefono, r.FotoUrl, r.HorarioAtencion);
}
