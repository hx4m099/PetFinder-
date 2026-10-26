// ═══════════════════════════════════════════════════════════════
// PetFinder — Tests unitarios: CoincidenciaService
// xUnit + Moq + FluentAssertions
// Verifica: Búsqueda de coincidencias con mock de similitud
// ═══════════════════════════════════════════════════════════════

using BLL.Exceptions;
using BLL.Services;
using DAL.Interfaces;
using ENTITY.Entities;
using ENTITY.Enums;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;

namespace PetFinder.Tests.BllTests;

public class CoincidenciaServiceTests
{
    private readonly Mock<IReporteRepository> _reporteRepoMock;
    private readonly Mock<IUsuarioRepository> _usuarioRepoMock;
    private readonly Mock<IImageSimilarityService> _similarityMock;
    private readonly Mock<ILogger<CoincidenciaService>> _loggerMock;
    private readonly CoincidenciaService _sut;

    public CoincidenciaServiceTests()
    {
        _reporteRepoMock = new Mock<IReporteRepository>();
        _usuarioRepoMock = new Mock<IUsuarioRepository>();
        _similarityMock  = new Mock<IImageSimilarityService>();
        _loggerMock      = new Mock<ILogger<CoincidenciaService>>();
        _sut = new CoincidenciaService(
            _reporteRepoMock.Object,
            _usuarioRepoMock.Object,
            _similarityMock.Object,
            _loggerMock.Object);
    }

    [Fact]
    public async Task BuscarCoincidenciasAsync_ConCandidatos_DebeRetornarOrdenadoPorSimilitud()
    {
        // Arrange
        var reporteOrigen = new ReporteMascota
        {
            Id = 1, TipoReporte = TipoReporte.Perdida, Especie = Especie.Perro,
            Ciudad = "CDMX", FotoUrl = "/foto1.jpg", UsuarioId = 1,
            FechaReporte = DateTime.UtcNow, Activo = true
        };

        var candidato1 = new ReporteMascota
        {
            Id = 2, TipoReporte = TipoReporte.Encontrada, Especie = Especie.Perro,
            Ciudad = "CDMX", FotoUrl = "/foto2.jpg", UsuarioId = 2,
            FechaReporte = DateTime.UtcNow.AddDays(-1), Activo = true
        };

        var candidato2 = new ReporteMascota
        {
            Id = 3, TipoReporte = TipoReporte.Encontrada, Especie = Especie.Perro,
            Ciudad = "CDMX", FotoUrl = "/foto3.jpg", UsuarioId = 3,
            FechaReporte = DateTime.UtcNow.AddDays(-2), Activo = true
        };

        _reporteRepoMock.Setup(r => r.ObtenerPorIdAsync(1)).ReturnsAsync(reporteOrigen);
        _reporteRepoMock
            .Setup(r => r.FiltrarParaComparacionAsync(
                Especie.Perro, "CDMX", It.IsAny<DateTime>(), 1))
            .ReturnsAsync(new[] { candidato1, candidato2 });

        // Candidato1 tiene más similitud
        _similarityMock
            .Setup(s => s.CalcularSimilitudAsync("/foto1.jpg", "/foto2.jpg"))
            .ReturnsAsync(0.85);
        _similarityMock
            .Setup(s => s.CalcularSimilitudAsync("/foto1.jpg", "/foto3.jpg"))
            .ReturnsAsync(0.60);

        var usuario2 = new Usuario { Id = 2, Nombre = "Juan", Telefono = "555" };
        var usuario3 = new Usuario { Id = 3, Nombre = "María", Telefono = "666" };
        _usuarioRepoMock.Setup(r => r.ObtenerPorIdAsync(2)).ReturnsAsync(usuario2);
        _usuarioRepoMock.Setup(r => r.ObtenerPorIdAsync(3)).ReturnsAsync(usuario3);

        // Act
        var resultado = (await _sut.BuscarCoincidenciasAsync(1)).ToList();

        // Assert
        resultado.Should().HaveCount(2);
        resultado[0].PorcentajeSimilitud.Should().Be(0.85);
        resultado[0].NombreUsuario.Should().Be("Juan");
        resultado[1].PorcentajeSimilitud.Should().Be(0.60);
        resultado[1].NombreUsuario.Should().Be("María");
    }

    [Fact]
    public async Task BuscarCoincidenciasAsync_SinCandidatos_DebeRetornarListaVacia()
    {
        // Arrange
        var reporteOrigen = new ReporteMascota
        {
            Id = 1, TipoReporte = TipoReporte.Perdida, Especie = Especie.Gato,
            Ciudad = "MTY", UsuarioId = 1, FechaReporte = DateTime.UtcNow, Activo = true
        };

        _reporteRepoMock.Setup(r => r.ObtenerPorIdAsync(1)).ReturnsAsync(reporteOrigen);
        _reporteRepoMock
            .Setup(r => r.FiltrarParaComparacionAsync(
                Especie.Gato, "MTY", It.IsAny<DateTime>(), 1))
            .ReturnsAsync(Enumerable.Empty<ReporteMascota>());

        // Act
        var resultado = await _sut.BuscarCoincidenciasAsync(1);

        // Assert
        resultado.Should().BeEmpty();
    }

    [Fact]
    public async Task BuscarCoincidenciasAsync_ConReporteInexistente_DebeLanzarEntidadNoEncontrada()
    {
        // Arrange
        _reporteRepoMock.Setup(r => r.ObtenerPorIdAsync(999)).ReturnsAsync((ReporteMascota?)null);

        // Act & Assert
        await _sut.Invoking(s => s.BuscarCoincidenciasAsync(999))
            .Should().ThrowAsync<EntidadNoEncontradaException>();
    }

    [Fact]
    public async Task BuscarCoincidenciasAsync_FiltraPorTipoOpuesto_SoloComparaEncuentraConPerdida()
    {
        // Arrange: reporte origen es Perdida, así que solo debe comparar con Encontrada
        var reporteOrigen = new ReporteMascota
        {
            Id = 1, TipoReporte = TipoReporte.Perdida, Especie = Especie.Perro,
            Ciudad = "CDMX", FotoUrl = "/f1.jpg", UsuarioId = 1,
            FechaReporte = DateTime.UtcNow, Activo = true
        };

        // Candidato del mismo tipo (Perdida) — no debe incluirse
        var candidatoMismoTipo = new ReporteMascota
        {
            Id = 2, TipoReporte = TipoReporte.Perdida, Especie = Especie.Perro,
            Ciudad = "CDMX", FotoUrl = "/f2.jpg", UsuarioId = 2,
            FechaReporte = DateTime.UtcNow, Activo = true
        };

        // Candidato opuesto (Encontrada) — sí se incluye
        var candidatoOpuesto = new ReporteMascota
        {
            Id = 3, TipoReporte = TipoReporte.Encontrada, Especie = Especie.Perro,
            Ciudad = "CDMX", FotoUrl = "/f3.jpg", UsuarioId = 3,
            FechaReporte = DateTime.UtcNow, Activo = true
        };

        _reporteRepoMock.Setup(r => r.ObtenerPorIdAsync(1)).ReturnsAsync(reporteOrigen);
        _reporteRepoMock
            .Setup(r => r.FiltrarParaComparacionAsync(
                Especie.Perro, "CDMX", It.IsAny<DateTime>(), 1))
            .ReturnsAsync(new[] { candidatoMismoTipo, candidatoOpuesto });

        _similarityMock
            .Setup(s => s.CalcularSimilitudAsync(It.IsAny<string?>(), It.IsAny<string?>()))
            .ReturnsAsync(0.70);

        _usuarioRepoMock.Setup(r => r.ObtenerPorIdAsync(It.IsAny<int>()))
            .ReturnsAsync(new Usuario { Id = 3, Nombre = "Test" });

        // Act
        var resultado = (await _sut.BuscarCoincidenciasAsync(1)).ToList();

        // Assert: solo el tipo opuesto
        resultado.Should().HaveCount(1);
        resultado[0].TipoReporte.Should().Be(TipoReporte.Encontrada);
    }

    [Fact]
    public async Task BuscarCoincidenciasAsync_FiltraPorUmbralMinimo()
    {
        // Arrange
        var reporteOrigen = new ReporteMascota
        {
            Id = 1, TipoReporte = TipoReporte.Perdida, Especie = Especie.Perro,
            Ciudad = "CDMX", FotoUrl = "/f1.jpg", UsuarioId = 1,
            FechaReporte = DateTime.UtcNow, Activo = true
        };

        var candidato = new ReporteMascota
        {
            Id = 2, TipoReporte = TipoReporte.Encontrada, Especie = Especie.Perro,
            Ciudad = "CDMX", FotoUrl = "/f2.jpg", UsuarioId = 2,
            FechaReporte = DateTime.UtcNow, Activo = true
        };

        _reporteRepoMock.Setup(r => r.ObtenerPorIdAsync(1)).ReturnsAsync(reporteOrigen);
        _reporteRepoMock
            .Setup(r => r.FiltrarParaComparacionAsync(
                Especie.Perro, "CDMX", It.IsAny<DateTime>(), 1))
            .ReturnsAsync(new[] { candidato });

        // Similitud muy baja, debería ser filtrado por UmbralSimilitud (0.35)
        _similarityMock
            .Setup(s => s.CalcularSimilitudAsync("/f1.jpg", "/f2.jpg"))
            .ReturnsAsync(0.10);

        _usuarioRepoMock.Setup(r => r.ObtenerPorIdAsync(2))
            .ReturnsAsync(new Usuario { Id = 2, Nombre = "Test" });

        // Act
        var resultado = await _sut.BuscarCoincidenciasAsync(1);

        // Assert: filtrado por umbral
        resultado.Should().BeEmpty();
    }
}
