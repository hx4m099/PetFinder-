// ═══════════════════════════════════════════════════════════════
// PetFinder — Tests unitarios: ReporteService
// xUnit + Moq + FluentAssertions
// Verifica: Crear reporte, obtener mis reportes, desactivar, acceso denegado
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

public class ReporteServiceTests
{
    private readonly Mock<IReporteRepository> _reporteRepoMock;
    private readonly Mock<IUsuarioRepository> _usuarioRepoMock;
    private readonly Mock<ILogger<ReporteService>> _loggerMock;
    private readonly ReporteService _sut;

    public ReporteServiceTests()
    {
        _reporteRepoMock = new Mock<IReporteRepository>();
        _usuarioRepoMock = new Mock<IUsuarioRepository>();
        _loggerMock      = new Mock<ILogger<ReporteService>>();
        _sut = new ReporteService(
            _reporteRepoMock.Object,
            _usuarioRepoMock.Object,
            _loggerMock.Object);
    }

    // ── CrearReporte ─────────────────────────────────────────

    [Fact]
    public async Task CrearReporteAsync_ConDatosValidos_DebeRetornarResumen()
    {
        // Arrange
        var usuario = new Usuario { Id = 1, Nombre = "Ana", Email = "ana@test.com", Activo = true };
        _usuarioRepoMock.Setup(r => r.ObtenerPorIdAsync(1)).ReturnsAsync(usuario);

        _reporteRepoMock
            .Setup(r => r.CrearAsync(It.IsAny<ReporteMascota>()))
            .ReturnsAsync((ReporteMascota rp) => { rp.Id = 50; return rp; });

        var dto = new ReporteFormDto(
            TipoReporte.Perdida, Especie.Perro, "Max", "Labrador",
            "Dorado", "CDMX", "Se perdió en el parque");

        // Act
        var resultado = await _sut.CrearReporteAsync(dto, 1, "/uploads/foto.jpg");

        // Assert
        resultado.Should().NotBeNull();
        resultado.Id.Should().Be(50);
        resultado.TipoReporte.Should().Be(TipoReporte.Perdida);
        resultado.Especie.Should().Be(Especie.Perro);
        resultado.NombreMascota.Should().Be("Max");
        resultado.Ciudad.Should().Be("CDMX");
        resultado.FotoUrl.Should().Be("/uploads/foto.jpg");
        resultado.NombreUsuario.Should().Be("Ana");
    }

    [Fact]
    public async Task CrearReporteAsync_ConUsuarioInexistente_DebeLanzarEntidadNoEncontrada()
    {
        // Arrange
        _usuarioRepoMock.Setup(r => r.ObtenerPorIdAsync(999)).ReturnsAsync((Usuario?)null);
        var dto = new ReporteFormDto(TipoReporte.Perdida, Especie.Gato, null, null, null, "CDMX", null);

        // Act & Assert
        await _sut.Invoking(s => s.CrearReporteAsync(dto, 999, null))
            .Should().ThrowAsync<EntidadNoEncontradaException>();
    }

    // ── ObtenerMisReportes ───────────────────────────────────

    [Fact]
    public async Task ObtenerMisReportesAsync_ConReportesExistentes_DebeRetornarLista()
    {
        // Arrange
        var usuario = new Usuario { Id = 1, Nombre = "Ana", Email = "ana@test.com", Activo = true };
        _usuarioRepoMock.Setup(r => r.ObtenerPorIdAsync(1)).ReturnsAsync(usuario);

        var reportes = new List<ReporteMascota>
        {
            new() { Id = 1, TipoReporte = TipoReporte.Perdida, Especie = Especie.Perro,
                    Ciudad = "CDMX", UsuarioId = 1, FechaReporte = DateTime.UtcNow, Activo = true },
            new() { Id = 2, TipoReporte = TipoReporte.Encontrada, Especie = Especie.Gato,
                    Ciudad = "GDL", UsuarioId = 1, FechaReporte = DateTime.UtcNow.AddDays(-1), Activo = true }
        };
        _reporteRepoMock.Setup(r => r.ObtenerPorUsuarioAsync(1)).ReturnsAsync(reportes);

        // Act
        var resultado = await _sut.ObtenerMisReportesAsync(1);

        // Assert
        resultado.Should().HaveCount(2);
        resultado.First().FechaReporte.Should().BeAfter(resultado.Last().FechaReporte);
    }

    // ── DesactivarReporte ────────────────────────────────────

    [Fact]
    public async Task DesactivarAsync_ComoDueno_DebeDesactivar()
    {
        // Arrange
        var reporte = new ReporteMascota { Id = 10, UsuarioId = 1, Activo = true, Ciudad = "CDMX" };
        var usuario = new Usuario { Id = 1, Nombre = "Ana", Rol = Rol.Adoptante, Activo = true };

        _reporteRepoMock.Setup(r => r.ObtenerPorIdAsync(10)).ReturnsAsync(reporte);
        _usuarioRepoMock.Setup(r => r.ObtenerPorIdAsync(1)).ReturnsAsync(usuario);

        // Act
        await _sut.DesactivarAsync(10, 1);

        // Assert
        _reporteRepoMock.Verify(r => r.DesactivarAsync(10), Times.Once);
    }

    [Fact]
    public async Task DesactivarAsync_ComoOtroUsuario_DebeLanzarAccesoDenegado()
    {
        // Arrange
        var reporte = new ReporteMascota { Id = 10, UsuarioId = 1, Activo = true, Ciudad = "CDMX" };
        var otroUsuario = new Usuario { Id = 2, Nombre = "Otro", Rol = Rol.Adoptante, Activo = true };

        _reporteRepoMock.Setup(r => r.ObtenerPorIdAsync(10)).ReturnsAsync(reporte);
        _usuarioRepoMock.Setup(r => r.ObtenerPorIdAsync(2)).ReturnsAsync(otroUsuario);

        // Act & Assert
        await _sut.Invoking(s => s.DesactivarAsync(10, 2))
            .Should().ThrowAsync<AccesoDenegadoException>();
    }

    [Fact]
    public async Task DesactivarAsync_ComoAdmin_DebeDesactivar()
    {
        // Arrange
        var reporte = new ReporteMascota { Id = 10, UsuarioId = 1, Activo = true, Ciudad = "CDMX" };
        var admin = new Usuario { Id = 3, Nombre = "Admin", Rol = Rol.Administrador, Activo = true };

        _reporteRepoMock.Setup(r => r.ObtenerPorIdAsync(10)).ReturnsAsync(reporte);
        _usuarioRepoMock.Setup(r => r.ObtenerPorIdAsync(3)).ReturnsAsync(admin);

        // Act
        await _sut.DesactivarAsync(10, 3);

        // Assert
        _reporteRepoMock.Verify(r => r.DesactivarAsync(10), Times.Once);
    }

    // ── ObtenerPorId ─────────────────────────────────────────

    [Fact]
    public async Task ObtenerPorIdAsync_ConIdInexistente_DebeLanzarEntidadNoEncontrada()
    {
        // Arrange
        _reporteRepoMock.Setup(r => r.ObtenerPorIdAsync(999)).ReturnsAsync((ReporteMascota?)null);

        // Act & Assert
        await _sut.Invoking(s => s.ObtenerPorIdAsync(999))
            .Should().ThrowAsync<EntidadNoEncontradaException>();
    }
}
