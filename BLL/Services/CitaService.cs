using BLL.Dtos;
using BLL.Exceptions;
using DAL.Interfaces;
using ENTITY.Constants;
using ENTITY.Entities;
using ENTITY.Enums;
using Microsoft.Extensions.Logging;

namespace BLL.Services;

/// <summary>
/// Interfaz del servicio de gestión de citas de adopción.
/// </summary>
public interface ICitaService
{
    /// <summary>Agenda una nueva cita, verificando disponibilidad de horario.</summary>
    Task<CitaDto> AgendarCitaAsync(CitaFormDto dto, int adoptanteId);

    /// <summary>Obtiene las citas del adoptante autenticado.</summary>
    Task<IEnumerable<CitaDto>> ObtenerMisCitasAsync(int adoptanteId);

    /// <summary>Obtiene las citas de un refugio (para el panel del refugio).</summary>
    Task<IEnumerable<CitaDto>> ObtenerCitasRefugioAsync(int refugioId);

    /// <summary>Cancela una cita (la puede hacer el adoptante o el refugio).</summary>
    Task CancelarCitaAsync(int citaId, int usuarioId);

    /// <summary>Confirma una cita (solo el refugio).</summary>
    Task ConfirmarCitaAsync(int citaId, int refugioId, string? notasRefugio);

    /// <summary>Marca una cita como completada (solo el refugio).</summary>
    Task CompletarCitaAsync(int citaId, int refugioId);

    /// <summary>Obtiene el detalle de una cita por Id.</summary>
    Task<CitaDto> ObtenerPorIdAsync(int citaId, int usuarioId);
}

/// <summary>
/// Implementación del servicio de citas con validación de estados y solapamiento de horarios.
/// </summary>
public class CitaService : ICitaService
{
    private readonly ICitaRepository    _citaRepo;
    private readonly IMascotaRepository _mascotaRepo;
    private readonly IRefugioRepository _refugioRepo;
    private readonly IUsuarioRepository _usuarioRepo;
    private readonly ILogger<CitaService> _logger;

    /// <summary>
    /// Mapa de transiciones de estado permitidas.
    /// Clave = estado actual; Valor = estados a los que se puede transitar.
    /// </summary>
    private static readonly Dictionary<EstadoCita, HashSet<EstadoCita>> TransicionesPermitidas = new()
    {
        [EstadoCita.Pendiente]  = [EstadoCita.Confirmada, EstadoCita.Cancelada],
        [EstadoCita.Confirmada] = [EstadoCita.Completada, EstadoCita.Cancelada],
        [EstadoCita.Cancelada]  = [],   // estado terminal
        [EstadoCita.Completada] = []    // estado terminal
    };

    public CitaService(
        ICitaRepository    citaRepo,
        IMascotaRepository mascotaRepo,
        IRefugioRepository refugioRepo,
        IUsuarioRepository usuarioRepo,
        ILogger<CitaService> logger)
    {
        _citaRepo    = citaRepo;
        _mascotaRepo = mascotaRepo;
        _refugioRepo = refugioRepo;
        _usuarioRepo = usuarioRepo;
        _logger      = logger;
    }

    /// <inheritdoc/>
    public async Task<CitaDto> AgendarCitaAsync(CitaFormDto dto, int adoptanteId)
    {
        // Validar existencia
        var mascota = await _mascotaRepo.ObtenerPorIdAsync(dto.MascotaId)
            ?? throw new EntidadNoEncontradaException("Mascota", dto.MascotaId);

        if (!mascota.Disponible)
            throw new PetFinderException("La mascota ya no está disponible para adopción.");

        var refugio = await _refugioRepo.ObtenerPorIdAsync(dto.RefugioId)
            ?? throw new EntidadNoEncontradaException("Refugio", dto.RefugioId);

        // Verificar solapamiento de horario
        var hayConflicto = await _citaRepo.ExisteSolapamientoAsync(dto.RefugioId, dto.FechaHora);
        if (hayConflicto)
            throw new CitaDuplicadaException(dto.FechaHora);

        var cita = new Cita
        {
            FechaHora      = dto.FechaHora,
            Estado         = EstadoCita.Pendiente,
            NotasAdoptante = dto.NotasAdoptante?.Trim(),
            AdoptanteId    = adoptanteId,
            MascotaId      = dto.MascotaId,
            RefugioId      = dto.RefugioId,
            FechaCreacion  = DateTime.UtcNow
        };

        var creada = await _citaRepo.CrearAsync(cita);
        _logger.LogInformation(
            "Cita agendada: Id={Id}, Mascota={MId}, Refugio={RId}, Fecha={F}",
            creada.Id, dto.MascotaId, dto.RefugioId, dto.FechaHora);

        return MapToDto(creada, mascota, refugio);
    }

    /// <inheritdoc/>
    public async Task<IEnumerable<CitaDto>> ObtenerMisCitasAsync(int adoptanteId)
    {
        var citas = await _citaRepo.ObtenerPorAdoptanteAsync(adoptanteId);
        return await MapCitasAsync(citas);
    }

    /// <inheritdoc/>
    public async Task<IEnumerable<CitaDto>> ObtenerCitasRefugioAsync(int refugioId)
    {
        var citas = await _citaRepo.ObtenerPorRefugioAsync(refugioId);
        return await MapCitasAsync(citas);
    }

    /// <inheritdoc/>
    public async Task CancelarCitaAsync(int citaId, int usuarioId)
    {
        var cita    = await ObtenerCitaValidaAsync(citaId);
        var usuario = await _usuarioRepo.ObtenerPorIdAsync(usuarioId)
            ?? throw new EntidadNoEncontradaException("Usuario", usuarioId);

        // Verificar que sea el adoptante, el refugio o un admin
        var refugio = await _refugioRepo.ObtenerPorIdAsync(cita.RefugioId);
        bool esAdoptante = cita.AdoptanteId == usuarioId;
        bool esRefugio   = refugio?.UsuarioAdminId == usuarioId;
        bool esAdmin     = usuario.Rol == Rol.Administrador;

        if (!esAdoptante && !esRefugio && !esAdmin)
            throw new AccesoDenegadoException("No tienes permisos para cancelar esta cita.");

        ValidarTransicion(cita.Estado, EstadoCita.Cancelada);

        cita.Estado              = EstadoCita.Cancelada;
        cita.FechaActualizacion  = DateTime.UtcNow;
        await _citaRepo.ActualizarAsync(cita);

        _logger.LogInformation("Cita {Id} cancelada por usuario {UsuarioId}", citaId, usuarioId);
    }

    /// <inheritdoc/>
    public async Task ConfirmarCitaAsync(int citaId, int refugioAdminId, string? notasRefugio)
    {
        var cita   = await ObtenerCitaValidaAsync(citaId);
        var refugio = await _refugioRepo.ObtenerPorIdAsync(cita.RefugioId)
            ?? throw new EntidadNoEncontradaException("Refugio", cita.RefugioId);

        if (refugio.UsuarioAdminId != refugioAdminId)
            throw new AccesoDenegadoException("Solo el administrador del refugio puede confirmar citas.");

        ValidarTransicion(cita.Estado, EstadoCita.Confirmada);

        cita.Estado             = EstadoCita.Confirmada;
        cita.NotasRefugio       = notasRefugio?.Trim();
        cita.FechaActualizacion = DateTime.UtcNow;
        await _citaRepo.ActualizarAsync(cita);

        _logger.LogInformation("Cita {Id} confirmada por refugio {RefugioId}", citaId, cita.RefugioId);
    }

    /// <inheritdoc/>
    public async Task CompletarCitaAsync(int citaId, int refugioAdminId)
    {
        var cita    = await ObtenerCitaValidaAsync(citaId);
        var refugio = await _refugioRepo.ObtenerPorIdAsync(cita.RefugioId)
            ?? throw new EntidadNoEncontradaException("Refugio", cita.RefugioId);

        if (refugio.UsuarioAdminId != refugioAdminId)
            throw new AccesoDenegadoException("Solo el administrador del refugio puede completar citas.");

        ValidarTransicion(cita.Estado, EstadoCita.Completada);

        cita.Estado             = EstadoCita.Completada;
        cita.FechaActualizacion = DateTime.UtcNow;
        await _citaRepo.ActualizarAsync(cita);

        _logger.LogInformation("Cita {Id} completada", citaId);
    }

    /// <inheritdoc/>
    public async Task<CitaDto> ObtenerPorIdAsync(int citaId, int usuarioId)
    {
        var cita    = await ObtenerCitaValidaAsync(citaId);
        var usuario = await _usuarioRepo.ObtenerPorIdAsync(usuarioId)
            ?? throw new EntidadNoEncontradaException("Usuario", usuarioId);

        var refugio = await _refugioRepo.ObtenerPorIdAsync(cita.RefugioId);
        bool esAdoptante = cita.AdoptanteId == usuarioId;
        bool esRefugio   = refugio?.UsuarioAdminId == usuarioId;
        bool esAdmin     = usuario.Rol == Rol.Administrador;

        if (!esAdoptante && !esRefugio && !esAdmin)
            throw new AccesoDenegadoException();

        var mascota = await _mascotaRepo.ObtenerPorIdAsync(cita.MascotaId)!;
        return MapToDto(cita, mascota!, refugio!);
    }

    // ── Helpers privados ─────────────────────────────────────────────────────

    private async Task<Cita> ObtenerCitaValidaAsync(int citaId)
        => await _citaRepo.ObtenerPorIdAsync(citaId)
           ?? throw new EntidadNoEncontradaException("Cita", citaId);

    private static void ValidarTransicion(EstadoCita actual, EstadoCita nuevo)
    {
        if (!TransicionesPermitidas[actual].Contains(nuevo))
            throw new EstadoCitaInvalidoException(actual.ToString(), nuevo.ToString());
    }

    private async Task<IEnumerable<CitaDto>> MapCitasAsync(IEnumerable<Cita> citas)
    {
        var resultado = new List<CitaDto>();
        foreach (var c in citas.OrderByDescending(x => x.FechaHora))
        {
            var mascota = await _mascotaRepo.ObtenerPorIdAsync(c.MascotaId);
            var refugio = await _refugioRepo.ObtenerPorIdAsync(c.RefugioId);
            resultado.Add(MapToDto(c, mascota, refugio));
        }
        return resultado;
    }

    private static CitaDto MapToDto(Cita c, Mascota? m, ENTITY.Entities.Refugio? r) =>
        new(c.Id, c.FechaHora, c.Estado,
            m?.Nombre  ?? "Mascota desconocida",
            m?.FotoUrl,
            r?.Nombre  ?? "Refugio desconocido",
            r?.Ciudad  ?? string.Empty,
            c.NotasAdoptante,
            c.NotasRefugio,
            c.FechaCreacion);
}
