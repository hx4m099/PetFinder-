// ═══════════════════════════════════════════════════════════════
// PetFinder — Tests unitarios: CitaService
// xUnit + Moq + FluentAssertions
// Verifica: Agendar, confirmar, cancelar, completar, máquina de estados
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

public class CitaServiceTests
{
    private readonly Mock<ICitaRepository>    _citaRepoMock;
    private readonly Mock<IMascotaRepository> _mascotaRepoMock;
    private readonly Mock<IRefugioRepository> _refugioRepoMock;
    private readonly Mock<IUsuarioRepository> _usuarioRepoMock;
    private readonly Mock<ILogger<CitaService>> _loggerMock;
    private readonly CitaService _sut;

    private readonly Refugio _refugio = new()
    {
        Id = 1, Nombre = "Patitas Felices", Ciudad = "CDMX",
        UsuarioAdminId = 10, Activo = true
    };

    private readonly Mascota _mascota = new()
    {
        Id = 5, Nombre = "Max", Especie = Especie.Perro,
        Disponible = true, RefugioId = 1
    };

    public CitaServiceTests()
    {
        _citaRepoMock    = new Mock<ICitaRepository>();
        _mascotaRepoMock = new Mock<IMascotaRepository>();
        _refugioRepoMock = new Mock<IRefugioRepository>();
        _usuarioRepoMock = new Mock<IUsuarioRepository>();
        _loggerMock      = new Mock<ILogger<CitaService>>();
        _sut = new CitaService(
            _citaRepoMock.Object,
            _mascotaRepoMock.Object,
            _refugioRepoMock.Object,
            _usuarioRepoMock.Object,
            _loggerMock.Object);
    }

    // ── Agendar cita ─────────────────────────────────────────

    [Fact]
    public async Task AgendarCitaAsync_ConDatosValidos_DebeCrearCitaPendiente()
    {
        // Arrange
        _mascotaRepoMock.Setup(r => r.ObtenerPorIdAsync(5)).ReturnsAsync(_mascota);
        _refugioRepoMock.Setup(r => r.ObtenerPorIdAsync(1)).ReturnsAsync(_refugio);
        _citaRepoMock.Setup(r => r.ExisteSolapamientoAsync(1, It.IsAny<DateTime>(), null)).ReturnsAsync(false);
        _citaRepoMock
            .Setup(r => r.CrearAsync(It.IsAny<Cita>()))
            .ReturnsAsync((Cita c) => { c.Id = 100; return c; });

        var fechaCita = DateTime.UtcNow.AddDays(3);
        var dto = new CitaFormDto(5, 1, fechaCita, "Quiero conocer a Max");

        // Act
        var resultado = await _sut.AgendarCitaAsync(dto, adoptanteId: 20);

        // Assert
        resultado.Should().NotBeNull();
        resultado.Id.Should().Be(100);
        resultado.Estado.Should().Be(EstadoCita.Pendiente);
        resultado.NombreMascota.Should().Be("Max");
        resultado.NombreRefugio.Should().Be("Patitas Felices");
        resultado.NotasAdoptante.Should().Be("Quiero conocer a Max");
    }

    [Fact]
    public async Task AgendarCitaAsync_ConMascotaNoDisponible_DebeLanzarExcepcion()
    {
        // Arrange
        var mascotaNoDisponible = new Mascota
        {
            Id = 5, Nombre = "Max", Disponible = false, RefugioId = 1
        };
        _mascotaRepoMock.Setup(r => r.ObtenerPorIdAsync(5)).ReturnsAsync(mascotaNoDisponible);

        var dto = new CitaFormDto(5, 1, DateTime.UtcNow.AddDays(1), null);

        // Act & Assert
        await _sut.Invoking(s => s.AgendarCitaAsync(dto, 20))
            .Should().ThrowAsync<PetFinderException>()
            .WithMessage("*no está disponible*");
    }

    [Fact]
    public async Task AgendarCitaAsync_ConSolapamientoHorario_DebeLanzarCitaDuplicada()
    {
        // Arrange
        _mascotaRepoMock.Setup(r => r.ObtenerPorIdAsync(5)).ReturnsAsync(_mascota);
        _refugioRepoMock.Setup(r => r.ObtenerPorIdAsync(1)).ReturnsAsync(_refugio);
        _citaRepoMock.Setup(r => r.ExisteSolapamientoAsync(1, It.IsAny<DateTime>(), null)).ReturnsAsync(true);

        var dto = new CitaFormDto(5, 1, DateTime.UtcNow.AddDays(1), null);

        // Act & Assert
        await _sut.Invoking(s => s.AgendarCitaAsync(dto, 20))
            .Should().ThrowAsync<CitaDuplicadaException>();
    }

    // ── Confirmar cita ───────────────────────────────────────

    [Fact]
    public async Task ConfirmarCitaAsync_ComoPendiente_DebeConfirmar()
    {
        // Arrange
        var cita = new Cita
        {
            Id = 10, Estado = EstadoCita.Pendiente,
            AdoptanteId = 20, MascotaId = 5, RefugioId = 1,
            FechaHora = DateTime.UtcNow.AddDays(2)
        };
        _citaRepoMock.Setup(r => r.ObtenerPorIdAsync(10)).ReturnsAsync(cita);
        _refugioRepoMock.Setup(r => r.ObtenerPorIdAsync(1)).ReturnsAsync(_refugio);

        // Act — refugioAdminId = 10 (UsuarioAdminId del refugio)
        await _sut.ConfirmarCitaAsync(10, refugioAdminId: 10, "Bienvenido");

        // Assert
        _citaRepoMock.Verify(r => r.ActualizarAsync(It.Is<Cita>(c =>
            c.Estado == EstadoCita.Confirmada &&
            c.NotasRefugio == "Bienvenido")), Times.Once);
    }

    [Fact]
    public async Task ConfirmarCitaAsync_ComoNoAdminRefugio_DebeLanzarAccesoDenegado()
    {
        // Arrange
        var cita = new Cita
        {
            Id = 10, Estado = EstadoCita.Pendiente,
            AdoptanteId = 20, MascotaId = 5, RefugioId = 1,
            FechaHora = DateTime.UtcNow.AddDays(2)
        };
        _citaRepoMock.Setup(r => r.ObtenerPorIdAsync(10)).ReturnsAsync(cita);
        _refugioRepoMock.Setup(r => r.ObtenerPorIdAsync(1)).ReturnsAsync(_refugio);

        // Act & Assert — usuario 99 no es el admin del refugio
        await _sut.Invoking(s => s.ConfirmarCitaAsync(10, refugioAdminId: 99, null))
            .Should().ThrowAsync<AccesoDenegadoException>();
    }

    // ── Cancelar cita ────────────────────────────────────────

    [Fact]
    public async Task CancelarCitaAsync_ComoAdoptante_DebeCancelar()
    {
        // Arrange
        var cita = new Cita
        {
            Id = 10, Estado = EstadoCita.Pendiente,
            AdoptanteId = 20, MascotaId = 5, RefugioId = 1,
            FechaHora = DateTime.UtcNow.AddDays(2)
        };
        var adoptante = new Usuario { Id = 20, Nombre = "Ana", Rol = Rol.Adoptante, Activo = true };

        _citaRepoMock.Setup(r => r.ObtenerPorIdAsync(10)).ReturnsAsync(cita);
        _usuarioRepoMock.Setup(r => r.ObtenerPorIdAsync(20)).ReturnsAsync(adoptante);
        _refugioRepoMock.Setup(r => r.ObtenerPorIdAsync(1)).ReturnsAsync(_refugio);

        // Act
        await _sut.CancelarCitaAsync(10, usuarioId: 20);

        // Assert
        _citaRepoMock.Verify(r => r.ActualizarAsync(It.Is<Cita>(c =>
            c.Estado == EstadoCita.Cancelada)), Times.Once);
    }

    [Fact]
    public async Task CancelarCitaAsync_ComoOtroUsuario_DebeLanzarAccesoDenegado()
    {
        // Arrange
        var cita = new Cita
        {
            Id = 10, Estado = EstadoCita.Pendiente,
            AdoptanteId = 20, MascotaId = 5, RefugioId = 1,
            FechaHora = DateTime.UtcNow.AddDays(2)
        };
        var otroUsuario = new Usuario { Id = 99, Nombre = "Intruso", Rol = Rol.Adoptante, Activo = true };

        _citaRepoMock.Setup(r => r.ObtenerPorIdAsync(10)).ReturnsAsync(cita);
        _usuarioRepoMock.Setup(r => r.ObtenerPorIdAsync(99)).ReturnsAsync(otroUsuario);
        _refugioRepoMock.Setup(r => r.ObtenerPorIdAsync(1)).ReturnsAsync(_refugio);

        // Act & Assert
        await _sut.Invoking(s => s.CancelarCitaAsync(10, usuarioId: 99))
            .Should().ThrowAsync<AccesoDenegadoException>();
    }

    // ── Completar cita ───────────────────────────────────────

    [Fact]
    public async Task CompletarCitaAsync_DesdeConfirmada_DebeCompletar()
    {
        // Arrange
        var cita = new Cita
        {
            Id = 10, Estado = EstadoCita.Confirmada,
            AdoptanteId = 20, MascotaId = 5, RefugioId = 1,
            FechaHora = DateTime.UtcNow.AddDays(-1)
        };
        _citaRepoMock.Setup(r => r.ObtenerPorIdAsync(10)).ReturnsAsync(cita);
        _refugioRepoMock.Setup(r => r.ObtenerPorIdAsync(1)).ReturnsAsync(_refugio);

        // Act
        await _sut.CompletarCitaAsync(10, refugioAdminId: 10);

        // Assert
        _citaRepoMock.Verify(r => r.ActualizarAsync(It.Is<Cita>(c =>
            c.Estado == EstadoCita.Completada)), Times.Once);
    }

    // ── Máquina de estados: transiciones inválidas ────────────

    [Theory]
    [InlineData(EstadoCita.Cancelada)]
    [InlineData(EstadoCita.Completada)]
    public async Task ConfirmarCitaAsync_DesdeEstadoTerminal_DebeLanzarEstadoInvalido(EstadoCita estadoTerminal)
    {
        // Arrange
        var cita = new Cita
        {
            Id = 10, Estado = estadoTerminal,
            AdoptanteId = 20, MascotaId = 5, RefugioId = 1,
            FechaHora = DateTime.UtcNow
        };
        _citaRepoMock.Setup(r => r.ObtenerPorIdAsync(10)).ReturnsAsync(cita);
        _refugioRepoMock.Setup(r => r.ObtenerPorIdAsync(1)).ReturnsAsync(_refugio);

        // Act & Assert
        await _sut.Invoking(s => s.ConfirmarCitaAsync(10, 10, null))
            .Should().ThrowAsync<EstadoCitaInvalidoException>();
    }

    [Fact]
    public async Task CompletarCitaAsync_DesdePendiente_DebeLanzarEstadoInvalido()
    {
        // Arrange — solo se puede completar desde Confirmada
        var cita = new Cita
        {
            Id = 10, Estado = EstadoCita.Pendiente,
            AdoptanteId = 20, MascotaId = 5, RefugioId = 1,
            FechaHora = DateTime.UtcNow
        };
        _citaRepoMock.Setup(r => r.ObtenerPorIdAsync(10)).ReturnsAsync(cita);
        _refugioRepoMock.Setup(r => r.ObtenerPorIdAsync(1)).ReturnsAsync(_refugio);

        // Act & Assert
        await _sut.Invoking(s => s.CompletarCitaAsync(10, 10))
            .Should().ThrowAsync<EstadoCitaInvalidoException>();
    }

    // ── Obtener mis citas ────────────────────────────────────

    [Fact]
    public async Task ObtenerMisCitasAsync_DebeRetornarCitasDelAdoptante()
    {
        // Arrange
        var citas = new List<Cita>
        {
            new() { Id = 1, Estado = EstadoCita.Pendiente, AdoptanteId = 20,
                    MascotaId = 5, RefugioId = 1, FechaHora = DateTime.UtcNow.AddDays(1),
                    FechaCreacion = DateTime.UtcNow },
            new() { Id = 2, Estado = EstadoCita.Confirmada, AdoptanteId = 20,
                    MascotaId = 5, RefugioId = 1, FechaHora = DateTime.UtcNow.AddDays(3),
                    FechaCreacion = DateTime.UtcNow }
        };
        _citaRepoMock.Setup(r => r.ObtenerPorAdoptanteAsync(20)).ReturnsAsync(citas);
        _mascotaRepoMock.Setup(r => r.ObtenerPorIdAsync(5)).ReturnsAsync(_mascota);
        _refugioRepoMock.Setup(r => r.ObtenerPorIdAsync(1)).ReturnsAsync(_refugio);

        // Act
        var resultado = (await _sut.ObtenerMisCitasAsync(20)).ToList();

        // Assert
        resultado.Should().HaveCount(2);
        resultado.Should().Contain(c => c.Estado == EstadoCita.Pendiente);
        resultado.Should().Contain(c => c.Estado == EstadoCita.Confirmada);
    }

    // ── Cita inexistente ─────────────────────────────────────

    [Fact]
    public async Task ConfirmarCitaAsync_ConCitaInexistente_DebeLanzarEntidadNoEncontrada()
    {
        // Arrange
        _citaRepoMock.Setup(r => r.ObtenerPorIdAsync(999)).ReturnsAsync((Cita?)null);

        // Act & Assert
        await _sut.Invoking(s => s.ConfirmarCitaAsync(999, 10, null))
            .Should().ThrowAsync<EntidadNoEncontradaException>();
    }
}
