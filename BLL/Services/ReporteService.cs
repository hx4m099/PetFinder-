using BLL.Dtos;
using BLL.Exceptions;
using DAL.Interfaces;
using ENTITY.Constants;
using ENTITY.Entities;
using ENTITY.Enums;
using Microsoft.Extensions.Logging;

namespace BLL.Services;

/// <summary>
/// Interfaz del servicio de gestión de reportes de mascotas perdidas/encontradas.
/// </summary>
public interface IReporteService
{
    /// <summary>Crea un nuevo reporte y asocia la URL de la foto guardada.</summary>
    Task<ReporteResumenDto> CrearReporteAsync(ReporteFormDto dto, int usuarioId, string? fotoUrl);

    /// <summary>Obtiene los reportes del usuario autenticado.</summary>
    Task<IEnumerable<ReporteResumenDto>> ObtenerMisReportesAsync(int usuarioId);

    /// <summary>Obtiene todos los reportes activos (para el Admin).</summary>
    Task<IEnumerable<ReporteResumenDto>> ObtenerTodosActivosAsync();

    /// <summary>Obtiene un reporte por su Id.</summary>
    Task<ReporteResumenDto> ObtenerPorIdAsync(int reporteId);

    /// <summary>Desactiva un reporte (mascota resuelta).</summary>
    Task DesactivarAsync(int reporteId, int usuarioId);
}

/// <summary>
/// Implementación del servicio de reportes de mascotas.
/// </summary>
public class ReporteService : IReporteService
{
    private readonly IReporteRepository _reporteRepo;
    private readonly IUsuarioRepository _usuarioRepo;
    private readonly ILogger<ReporteService> _logger;

    public ReporteService(
        IReporteRepository reporteRepo,
        IUsuarioRepository usuarioRepo,
        ILogger<ReporteService> logger)
    {
        _reporteRepo = reporteRepo;
        _usuarioRepo = usuarioRepo;
        _logger      = logger;
    }

    /// <inheritdoc/>
    public async Task<ReporteResumenDto> CrearReporteAsync(
        ReporteFormDto dto, int usuarioId, string? fotoUrl)
    {
        var usuario = await _usuarioRepo.ObtenerPorIdAsync(usuarioId)
            ?? throw new EntidadNoEncontradaException("Usuario", usuarioId);

        var reporte = new ReporteMascota
        {
            TipoReporte   = dto.TipoReporte,
            Especie       = dto.Especie,
            NombreMascota = dto.NombreMascota?.Trim(),
            Raza          = dto.Raza?.Trim(),
            ColorPelaje   = dto.ColorPelaje?.Trim(),
            Ciudad        = dto.Ciudad.Trim(),
            Descripcion   = dto.Descripcion?.Trim(),
            FotoUrl       = fotoUrl,
            UsuarioId     = usuarioId,
            FechaReporte  = DateTime.UtcNow,
            Activo        = true
        };

        var creado = await _reporteRepo.CrearAsync(reporte);
        _logger.LogInformation("Reporte creado: Id={Id}, Tipo={Tipo}, Especie={Especie}, Ciudad={Ciudad}",
            creado.Id, creado.TipoReporte, creado.Especie, creado.Ciudad);

        return MapToResumen(creado, usuario.Nombre);
    }

    /// <inheritdoc/>
    public async Task<IEnumerable<ReporteResumenDto>> ObtenerMisReportesAsync(int usuarioId)
    {
        var usuario = await _usuarioRepo.ObtenerPorIdAsync(usuarioId)
            ?? throw new EntidadNoEncontradaException("Usuario", usuarioId);

        var reportes = await _reporteRepo.ObtenerPorUsuarioAsync(usuarioId);
        return reportes.Select(r => MapToResumen(r, usuario.Nombre)).OrderByDescending(r => r.FechaReporte);
    }

    /// <inheritdoc/>
    public async Task<IEnumerable<ReporteResumenDto>> ObtenerTodosActivosAsync()
    {
        var reportes = await _reporteRepo.ObtenerActivosAsync();
        var usuarios = new Dictionary<int, string>();

        var tareas = reportes.Select(async r =>
        {
            if (!usuarios.ContainsKey(r.UsuarioId))
            {
                var u = await _usuarioRepo.ObtenerPorIdAsync(r.UsuarioId);
                usuarios[r.UsuarioId] = u?.Nombre ?? "Usuario desconocido";
            }
            return MapToResumen(r, usuarios[r.UsuarioId]);
        });

        return (await Task.WhenAll(tareas)).OrderByDescending(r => r.FechaReporte);
    }

    /// <inheritdoc/>
    public async Task<ReporteResumenDto> ObtenerPorIdAsync(int reporteId)
    {
        var reporte = await _reporteRepo.ObtenerPorIdAsync(reporteId)
            ?? throw new EntidadNoEncontradaException("ReporteMascota", reporteId);

        var usuario = await _usuarioRepo.ObtenerPorIdAsync(reporte.UsuarioId);
        return MapToResumen(reporte, usuario?.Nombre ?? "Usuario desconocido");
    }

    /// <inheritdoc/>
    public async Task DesactivarAsync(int reporteId, int usuarioId)
    {
        var reporte = await _reporteRepo.ObtenerPorIdAsync(reporteId)
            ?? throw new EntidadNoEncontradaException("ReporteMascota", reporteId);

        var usuario = await _usuarioRepo.ObtenerPorIdAsync(usuarioId)
            ?? throw new EntidadNoEncontradaException("Usuario", usuarioId);

        // Solo el dueño o el admin pueden desactivar
        if (reporte.UsuarioId != usuarioId && usuario.Rol != ENTITY.Enums.Rol.Administrador)
            throw new AccesoDenegadoException("Solo el autor del reporte o un administrador puede desactivarlo.");

        await _reporteRepo.DesactivarAsync(reporteId);
        _logger.LogInformation("Reporte {Id} desactivado por usuario {UsuarioId}", reporteId, usuarioId);
    }

    // ── Mapper privado ────────────────────────────────────────────────────────
    private static ReporteResumenDto MapToResumen(ReporteMascota r, string nombreUsuario) =>
        new(r.Id, r.TipoReporte, r.Especie, r.NombreMascota,
            r.Ciudad, r.FotoUrl, r.FechaReporte, r.Activo, nombreUsuario);
}
