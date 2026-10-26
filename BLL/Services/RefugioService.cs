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

    private static RefugioDto MapToDto(Refugio r) =>
        new(r.Id, r.Nombre, r.Descripcion, r.Direccion,
            r.Ciudad, r.Email, r.Telefono, r.FotoUrl, r.HorarioAtencion);
}
