// PetFinder.Tests/BllTests/EstadisticasServiceTests.cs
// ═══════════════════════════════════════════════════════════════
// PetFinder — Tests unitarios: EstadisticasService
// xUnit + Moq + FluentAssertions
// ═══════════════════════════════════════════════════════════════

using BLL.Services;
using DAL.Interfaces;
using ENTITY.Entities;
using ENTITY.Enums;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;

namespace PetFinder.Tests.BllTests;

public class EstadisticasServiceTests
{
    private readonly Mock<IRefugioRepository> _refugioRepoMock;
    private readonly Mock<IReporteRepository> _reporteRepoMock;
    private readonly Mock<IMascotaRepository> _mascotaRepoMock;
    private readonly Mock<ILogger<EstadisticasService>> _loggerMock;
    private readonly EstadisticasService _sut;

    public EstadisticasServiceTests()
    {
        _refugioRepoMock = new Mock<IRefugioRepository>();
        _reporteRepoMock = new Mock<IReporteRepository>();
        _mascotaRepoMock = new Mock<IMascotaRepository>();
        _loggerMock = new Mock<ILogger<EstadisticasService>>();

        _sut = new EstadisticasService(
            _refugioRepoMock.Object,
            _reporteRepoMock.Object,
            _mascotaRepoMock.Object,
            _loggerMock.Object);
    }

    [Fact]
    public async Task ObtenerEstadisticasImpactoAsync_CalculaMetricasCorrectas()
    {
        // Arrange
        var refugios = new List<Refugio>
        {
            new() { Id = 1, Nombre = "Refugio 1", Activo = true },
            new() { Id = 2, Nombre = "Refugio 2", Activo = true },
            new() { Id = 3, Nombre = "Refugio 3", Activo = true }
        };

        var reportes = new List<ReporteMascota>
        {
            new() { Id = 1, Activo = true, TipoReporte = TipoReporte.Perdida, Especie = Especie.Perro, Ciudad = "CDMX" },
            new() { Id = 2, Activo = true, TipoReporte = TipoReporte.Encontrada, Especie = Especie.Gato, Ciudad = "CDMX" }
        };

        _refugioRepoMock.Setup(r => r.ObtenerTodosAsync()).ReturnsAsync(refugios);
        _reporteRepoMock.Setup(r => r.ObtenerActivosAsync()).ReturnsAsync(reportes);

        // Act
        var stats = await _sut.ObtenerEstadisticasImpactoAsync();

        // Assert
        stats.Should().NotBeNull();
        stats.RefugiosAliados.Should().Be(3);
        stats.ReportesActivos.Should().Be(2);
        stats.MascotasReunidas.Should().BeGreaterThan(0);
        stats.AdopcionesConcretadas.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task ObtenerEstadisticasImpactoAsync_EnCasoDeError_RetornaFallbackValido()
    {
        // Arrange
        _refugioRepoMock.Setup(r => r.ObtenerTodosAsync()).ThrowsAsync(new Exception("DB Connection Error"));

        // Act
        var stats = await _sut.ObtenerEstadisticasImpactoAsync();

        // Assert
        stats.Should().NotBeNull();
        stats.RefugiosAliados.Should().Be(6);
        stats.MascotasReunidas.Should().Be(248);
    }
}
