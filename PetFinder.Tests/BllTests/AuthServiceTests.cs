// ═══════════════════════════════════════════════════════════════
// PetFinder — Tests unitarios: AuthService
// xUnit + Moq + FluentAssertions
// Verifica: Registro, Login, email duplicado, credenciales inválidas
// ═══════════════════════════════════════════════════════════════

using BLL.Dtos;
using BLL.Exceptions;
using BLL.Services;
using DAL.Interfaces;
using ENTITY.Entities;
using ENTITY.Enums;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;

namespace PetFinder.Tests.BllTests;

public class AuthServiceTests
{
    private readonly Mock<IUsuarioRepository> _usuarioRepoMock;
    private readonly Mock<IRefugioRepository> _refugioRepoMock;
    private readonly Mock<ILogger<AuthService>> _loggerMock;
    private readonly AuthService _sut;

    public AuthServiceTests()
    {
        _usuarioRepoMock = new Mock<IUsuarioRepository>();
        _refugioRepoMock = new Mock<IRefugioRepository>();
        _loggerMock      = new Mock<ILogger<AuthService>>();
        _sut = new AuthService(
            _usuarioRepoMock.Object,
            _refugioRepoMock.Object,
            _loggerMock.Object);
    }

    // ── Registro ─────────────────────────────────────────────

    [Fact]
    public async Task RegistrarAsync_ConDatosValidos_DebeCrearUsuarioYDevolverSesion()
    {
        // Arrange
        var dto = new RegistroDto("Ana García", "ana@test.com", "Pass12345",
            "Pass12345", Rol.Adoptante, "5512345678", "CDMX");

        _usuarioRepoMock
            .Setup(r => r.EmailExisteAsync("ana@test.com"))
            .ReturnsAsync(false);

        _usuarioRepoMock
            .Setup(r => r.CrearAsync(It.IsAny<Usuario>()))
            .ReturnsAsync((Usuario u) =>
            {
                u.Id = 99;
                return u;
            });

        // Act
        var resultado = await _sut.RegistrarAsync(dto);

        // Assert
        resultado.Should().NotBeNull();
        resultado.Id.Should().Be(99);
        resultado.Nombre.Should().Be("Ana García");
        resultado.Email.Should().Be("ana@test.com");
        resultado.Rol.Should().Be(Rol.Adoptante);
        resultado.RefugioId.Should().BeNull();

        _usuarioRepoMock.Verify(r => r.CrearAsync(It.IsAny<Usuario>()), Times.Once);
    }

    [Fact]
    public async Task RegistrarAsync_ConEmailDuplicado_DebeLanzarEmailDuplicadoException()
    {
        // Arrange
        var dto = new RegistroDto("Test", "duplicado@test.com", "Pass12345",
            "Pass12345", Rol.Adoptante, null, null);

        _usuarioRepoMock
            .Setup(r => r.EmailExisteAsync("duplicado@test.com"))
            .ReturnsAsync(true);

        // Act & Assert
        await _sut.Invoking(s => s.RegistrarAsync(dto))
            .Should().ThrowAsync<EmailDuplicadoException>();

        _usuarioRepoMock.Verify(r => r.CrearAsync(It.IsAny<Usuario>()), Times.Never);
    }

    [Fact]
    public async Task RegistrarAsync_ConRolRefugio_DebeAsignarRefugioId()
    {
        // Arrange
        var dto = new RegistroDto("Refugio Test", "refugio@test.com", "Pass12345",
            "Pass12345", Rol.Refugio, null, "CDMX");

        _usuarioRepoMock.Setup(r => r.EmailExisteAsync(It.IsAny<string>())).ReturnsAsync(false);
        _usuarioRepoMock.Setup(r => r.CrearAsync(It.IsAny<Usuario>()))
            .ReturnsAsync((Usuario u) => { u.Id = 10; return u; });

        _refugioRepoMock.Setup(r => r.ObtenerPorUsuarioAdminAsync(10))
            .ReturnsAsync(new Refugio { Id = 5, Nombre = "Refugio Test", Ciudad = "CDMX", UsuarioAdminId = 10 });

        // Act
        var resultado = await _sut.RegistrarAsync(dto);

        // Assert
        resultado.Rol.Should().Be(Rol.Refugio);
        resultado.RefugioId.Should().Be(5);
    }

    // ── Login ────────────────────────────────────────────────

    [Fact]
    public async Task LoginAsync_ConCredencialesCorrectas_DebeDevolverSesion()
    {
        // Arrange
        var password = "MiPassword123";
        var hash = Convert.ToBase64String(
            System.Security.Cryptography.SHA256.HashData(
                System.Text.Encoding.UTF8.GetBytes(password + "_petfinder_salt")));

        var usuario = new Usuario
        {
            Id = 1, Nombre = "Test User", Email = "test@test.com",
            PasswordHash = hash, Rol = Rol.Adoptante, Activo = true
        };

        _usuarioRepoMock
            .Setup(r => r.ObtenerPorEmailAsync("test@test.com"))
            .ReturnsAsync(usuario);

        var dto = new LoginDto("test@test.com", password);

        // Act
        var resultado = await _sut.LoginAsync(dto);

        // Assert
        resultado.Should().NotBeNull();
        resultado.Id.Should().Be(1);
        resultado.Email.Should().Be("test@test.com");
    }

    [Fact]
    public async Task LoginAsync_ConPasswordIncorrecto_DebeLanzarCredencialesInvalidasException()
    {
        // Arrange
        var usuario = new Usuario
        {
            Id = 1, Nombre = "Test", Email = "test@test.com",
            PasswordHash = "hash_incorrecto", Rol = Rol.Adoptante, Activo = true
        };

        _usuarioRepoMock
            .Setup(r => r.ObtenerPorEmailAsync("test@test.com"))
            .ReturnsAsync(usuario);

        var dto = new LoginDto("test@test.com", "PasswordIncorrecto");

        // Act & Assert
        await _sut.Invoking(s => s.LoginAsync(dto))
            .Should().ThrowAsync<CredencialesInvalidasException>();
    }

    [Fact]
    public async Task LoginAsync_ConEmailInexistente_DebeLanzarCredencialesInvalidasException()
    {
        // Arrange
        _usuarioRepoMock
            .Setup(r => r.ObtenerPorEmailAsync("noexiste@test.com"))
            .ReturnsAsync((Usuario?)null);

        var dto = new LoginDto("noexiste@test.com", "CualquierPassword");

        // Act & Assert
        await _sut.Invoking(s => s.LoginAsync(dto))
            .Should().ThrowAsync<CredencialesInvalidasException>();
    }

    [Fact]
    public async Task LoginAsync_ConUsuarioInactivo_DebeLanzarCredencialesInvalidasException()
    {
        // Arrange
        var password = "Pass12345";
        var hash = Convert.ToBase64String(
            System.Security.Cryptography.SHA256.HashData(
                System.Text.Encoding.UTF8.GetBytes(password + "_petfinder_salt")));

        var usuario = new Usuario
        {
            Id = 1, Nombre = "Inactivo", Email = "inactivo@test.com",
            PasswordHash = hash, Rol = Rol.Adoptante, Activo = false
        };

        _usuarioRepoMock
            .Setup(r => r.ObtenerPorEmailAsync("inactivo@test.com"))
            .ReturnsAsync(usuario);

        // Act & Assert
        await _sut.Invoking(s => s.LoginAsync(new LoginDto("inactivo@test.com", password)))
            .Should().ThrowAsync<CredencialesInvalidasException>();
    }

    // ── ObtenerSesion ────────────────────────────────────────

    [Fact]
    public async Task ObtenerSesionAsync_ConUsuarioActivo_DebeDevolverSesion()
    {
        // Arrange
        var usuario = new Usuario { Id = 5, Nombre = "Test", Email = "t@t.com", Rol = Rol.Adoptante, Activo = true };
        _usuarioRepoMock.Setup(r => r.ObtenerPorIdAsync(5)).ReturnsAsync(usuario);

        // Act
        var resultado = await _sut.ObtenerSesionAsync(5);

        // Assert
        resultado.Should().NotBeNull();
        resultado!.Id.Should().Be(5);
    }

    [Fact]
    public async Task ObtenerSesionAsync_ConUsuarioInexistente_DebeRetornarNull()
    {
        // Arrange
        _usuarioRepoMock.Setup(r => r.ObtenerPorIdAsync(999)).ReturnsAsync((Usuario?)null);

        // Act
        var resultado = await _sut.ObtenerSesionAsync(999);

        // Assert
        resultado.Should().BeNull();
    }
}
