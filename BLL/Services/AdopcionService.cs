using BLL.Dtos;
using BLL.Exceptions;
using DAL.Interfaces;
using ENTITY.Entities;
using ENTITY.Enums;
using Microsoft.Extensions.Logging;

namespace BLL.Services;

/// <summary>
/// Interfaz del servicio de adopción: gestión del catálogo de mascotas.
/// </summary>
public interface IAdopcionService
{
    /// <summary>Obtiene el catálogo de mascotas con filtros opcionales.</summary>
    Task<IEnumerable<MascotaResumenDto>> ObtenerCatalogoAsync(
        int? refugioId = null,
        Especie? especie = null,
        int? edadMaxMeses = null);

    /// <summary>Obtiene el detalle completo de una mascota.</summary>
    Task<MascotaDetalleDto> ObtenerDetalleAsync(int mascotaId);

    /// <summary>Crea una nueva mascota en el refugio del usuario.</summary>
    Task<MascotaResumenDto> CrearMascotaAsync(MascotaFormDto dto, int refugioId, string? fotoUrl);

    /// <summary>Actualiza los datos de una mascota existente.</summary>
    Task ActualizarMascotaAsync(int mascotaId, MascotaFormDto dto, int refugioId);

    /// <summary>Desactiva (soft-delete) una mascota del catálogo.</summary>
    Task EliminarMascotaAsync(int mascotaId, int refugioId);

    /// <summary>Obtiene las mascotas publicadas por un refugio específico.</summary>
    Task<IEnumerable<MascotaResumenDto>> ObtenerPorRefugioAsync(int refugioId);
}

/// <summary>
/// Implementación del servicio de adopción.
/// </summary>
public class AdopcionService : IAdopcionService
{
    private readonly IMascotaRepository  _mascotaRepo;
    private readonly IRefugioRepository  _refugioRepo;
    private readonly ILogger<AdopcionService> _logger;

    public AdopcionService(
        IMascotaRepository mascotaRepo,
        IRefugioRepository refugioRepo,
        ILogger<AdopcionService> logger)
    {
        _mascotaRepo = mascotaRepo;
        _refugioRepo = refugioRepo;
        _logger      = logger;
    }

    /// <inheritdoc/>
    public async Task<IEnumerable<MascotaResumenDto>> ObtenerCatalogoAsync(
        int? refugioId = null, Especie? especie = null, int? edadMaxMeses = null)
    {
        var mascotas = await _mascotaRepo.BuscarAsync(refugioId, especie, edadMaxMeses);
        var resultado = new List<MascotaResumenDto>();

        foreach (var m in mascotas)
        {
            var refugio = await _refugioRepo.ObtenerPorIdAsync(m.RefugioId);
            resultado.Add(MapToResumen(m, refugio));
        }

        return resultado.OrderByDescending(m => m.Id);
    }

    /// <inheritdoc/>
    public async Task<MascotaDetalleDto> ObtenerDetalleAsync(int mascotaId)
    {
        var mascota = await _mascotaRepo.ObtenerPorIdAsync(mascotaId)
            ?? throw new EntidadNoEncontradaException("Mascota", mascotaId);

        var refugio = await _refugioRepo.ObtenerPorIdAsync(mascota.RefugioId)
            ?? throw new EntidadNoEncontradaException("Refugio", mascota.RefugioId);

        return new MascotaDetalleDto(
            mascota.Id,
            mascota.Nombre,
            mascota.Especie,
            mascota.Raza,
            mascota.EdadMeses,
            mascota.Descripcion,
            mascota.FotoUrl,
            mascota.Vacunada,
            mascota.Castrada,
            mascota.Disponible,
            refugio.Id,
            refugio.Nombre,
            refugio.Ciudad,
            refugio.HorarioAtencion);
    }

    /// <inheritdoc/>
    public async Task<MascotaResumenDto> CrearMascotaAsync(
        MascotaFormDto dto, int refugioId, string? fotoUrl)
    {
        var refugio = await _refugioRepo.ObtenerPorIdAsync(refugioId)
            ?? throw new EntidadNoEncontradaException("Refugio", refugioId);

        var mascota = new Mascota
        {
            Nombre        = dto.Nombre.Trim(),
            Especie       = dto.Especie,
            Raza          = dto.Raza?.Trim(),
            EdadMeses     = dto.EdadMeses,
            Descripcion   = dto.Descripcion?.Trim(),
            FotoUrl       = fotoUrl,
            Vacunada      = dto.Vacunada,
            Castrada      = dto.Castrada,
            Disponible    = true,
            RefugioId     = refugioId,
            FechaRegistro = DateTime.UtcNow
        };

        var creada = await _mascotaRepo.CrearAsync(mascota);
        _logger.LogInformation("Mascota creada: {Nombre} en refugio {RefugioId}", creada.Nombre, refugioId);
        return MapToResumen(creada, refugio);
    }

    /// <inheritdoc/>
    public async Task ActualizarMascotaAsync(int mascotaId, MascotaFormDto dto, int refugioId)
    {
        var mascota = await _mascotaRepo.ObtenerPorIdAsync(mascotaId)
            ?? throw new EntidadNoEncontradaException("Mascota", mascotaId);

        if (mascota.RefugioId != refugioId)
            throw new AccesoDenegadoException("Esta mascota no pertenece a tu refugio.");

        mascota.Nombre      = dto.Nombre.Trim();
        mascota.Especie     = dto.Especie;
        mascota.Raza        = dto.Raza?.Trim();
        mascota.EdadMeses   = dto.EdadMeses;
        mascota.Descripcion = dto.Descripcion?.Trim();
        mascota.Vacunada    = dto.Vacunada;
        mascota.Castrada    = dto.Castrada;

        await _mascotaRepo.ActualizarAsync(mascota);
        _logger.LogInformation("Mascota {Id} actualizada por refugio {RefugioId}", mascotaId, refugioId);
    }

    /// <inheritdoc/>
    public async Task EliminarMascotaAsync(int mascotaId, int refugioId)
    {
        var mascota = await _mascotaRepo.ObtenerPorIdAsync(mascotaId)
            ?? throw new EntidadNoEncontradaException("Mascota", mascotaId);

        if (mascota.RefugioId != refugioId)
            throw new AccesoDenegadoException("Esta mascota no pertenece a tu refugio.");

        await _mascotaRepo.EliminarAsync(mascotaId);
        _logger.LogInformation("Mascota {Id} desactivada por refugio {RefugioId}", mascotaId, refugioId);
    }

    /// <inheritdoc/>
    public async Task<IEnumerable<MascotaResumenDto>> ObtenerPorRefugioAsync(int refugioId)
    {
        var mascotas = await _mascotaRepo.ObtenerPorRefugioAsync(refugioId);
        var refugio  = await _refugioRepo.ObtenerPorIdAsync(refugioId);
        return mascotas.Select(m => MapToResumen(m, refugio)).OrderByDescending(m => m.Id);
    }

    // ── Mapper privado ────────────────────────────────────────────────────────
    private static MascotaResumenDto MapToResumen(Mascota m, ENTITY.Entities.Refugio? r) =>
        new(m.Id, m.Nombre, m.Especie, m.Raza, m.EdadMeses,
            m.FotoUrl, m.Vacunada, m.Castrada,
            r?.Nombre ?? "Refugio desconocido",
            r?.Ciudad ?? string.Empty);
}
