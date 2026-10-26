using BLL.Dtos;
using BLL.Exceptions;
using DAL.Interfaces;
using ENTITY.Constants;
using ENTITY.Entities;
using ENTITY.Enums;
using Microsoft.Extensions.Logging;

namespace BLL.Services;

/// <summary>
/// Interfaz pública del servicio de autenticación.
/// Permite registrar usuarios e iniciar/cerrar sesión con cookies simuladas.
/// </summary>
public interface IAuthService
{
    /// <summary>Registra un nuevo usuario y devuelve sus datos de sesión.</summary>
    Task<UsuarioSesionDto> RegistrarAsync(RegistroDto dto);

    /// <summary>Valida credenciales y devuelve datos de sesión si son correctas.</summary>
    Task<UsuarioSesionDto> LoginAsync(LoginDto dto);

    /// <summary>Obtiene los datos de sesión de un usuario por su Id.</summary>
    Task<UsuarioSesionDto?> ObtenerSesionAsync(int usuarioId);
}

/// <summary>
/// Implementación del servicio de autenticación.
/// Usa hashing simulado; se reemplazará por BCrypt.Net al conectar la BD real.
/// </summary>
public class AuthService : IAuthService
{
    private readonly IUsuarioRepository _usuarioRepo;
    private readonly IRefugioRepository _refugioRepo;
    private readonly ILogger<AuthService> _logger;

    public AuthService(
        IUsuarioRepository usuarioRepo,
        IRefugioRepository refugioRepo,
        ILogger<AuthService> logger)
    {
        _usuarioRepo = usuarioRepo;
        _refugioRepo = refugioRepo;
        _logger      = logger;
    }

    /// <inheritdoc/>
    public async Task<UsuarioSesionDto> RegistrarAsync(RegistroDto dto)
    {
        if (await _usuarioRepo.EmailExisteAsync(dto.Email))
            throw new EmailDuplicadoException(dto.Email);

        var usuario = new Usuario
        {
            Nombre       = dto.Nombre.Trim(),
            Email        = dto.Email.Trim().ToLowerInvariant(),
            PasswordHash = HashPassword(dto.Password),
            Rol          = dto.Rol,
            Telefono     = dto.Telefono?.Trim(),
            Ciudad       = dto.Ciudad?.Trim(),
            Activo       = true,
            FechaRegistro = DateTime.UtcNow
        };

        var creado = await _usuarioRepo.CrearAsync(usuario);
        _logger.LogInformation("Nuevo usuario registrado: {Email}, Rol: {Rol}", creado.Email, creado.Rol);

        return await BuildSesionDto(creado);
    }

    /// <inheritdoc/>
    public async Task<UsuarioSesionDto> LoginAsync(LoginDto dto)
    {
        var usuario = await _usuarioRepo.ObtenerPorEmailAsync(
            dto.Email.Trim().ToLowerInvariant());

        if (usuario is null || !usuario.Activo || !VerifyPassword(dto.Password, usuario.PasswordHash))
        {
            _logger.LogWarning("Intento de login fallido para: {Email}", dto.Email);
            throw new CredencialesInvalidasException();
        }

        _logger.LogInformation("Login exitoso: {Email}", usuario.Email);
        return await BuildSesionDto(usuario);
    }

    /// <inheritdoc/>
    public async Task<UsuarioSesionDto?> ObtenerSesionAsync(int usuarioId)
    {
        var usuario = await _usuarioRepo.ObtenerPorIdAsync(usuarioId);
        if (usuario is null || !usuario.Activo)
            return null;

        return await BuildSesionDto(usuario);
    }

    // ── Helpers privados ─────────────────────────────────────────────────────

    /// <summary>
    /// Hash simulado para desarrollo. Se sustituirá por BCrypt.Net.Next.
    /// NUNCA usar en producción.
    /// </summary>
    private static string HashPassword(string password)
        => Convert.ToBase64String(
            System.Security.Cryptography.SHA256.HashData(
                System.Text.Encoding.UTF8.GetBytes(password + "_petfinder_salt")));

    private static bool VerifyPassword(string password, string hash)
        => HashPassword(password) == hash;

    private async Task<UsuarioSesionDto> BuildSesionDto(Usuario usuario)
    {
        int? refugioId = null;
        if (usuario.Rol == Rol.Refugio)
        {
            var refugio = await _refugioRepo.ObtenerPorUsuarioAdminAsync(usuario.Id);
            refugioId = refugio?.Id;
        }

        return new UsuarioSesionDto(
            usuario.Id,
            usuario.Nombre,
            usuario.Email,
            usuario.Rol,
            refugioId);
    }
}
