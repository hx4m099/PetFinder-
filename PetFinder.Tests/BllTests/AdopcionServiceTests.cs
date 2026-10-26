// ═══════════════════════════════════════════════════════════════
// PetFinder — Tests unitarios: AdopcionService
// xUnit + Moq + FluentAssertions
// Verifica: Catálogo, crear mascota, actualizar, eliminar, acceso denegado
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

public class AdopcionServiceTests
{
    private readonly Mock<IMascotaRepository> _mascotaRepoMock;
    private readonly Mock<IRefugioRepository> _refugioRepoMock;
    private readonly Mock<ILogger<AdopcionService>> _loggerMock;
    private readonly AdopcionService _sut;

    private readonly Refugio _refugio = new()
    {
        Id = 1, Nombre = "Patitas Felices", Ciudad = "CDMX",
        UsuarioAdminId = 10, Activo = true
    };

    public AdopcionServiceTests()
    {
        _mascotaRepoMock = new Mock<IMascotaRepository>();
        _refugioRepoMock = new Mock<IRefugioRepository>();
        _loggerMock      = new Mock<ILogger<AdopcionService>>();
        _sut = new AdopcionService(
            _mascotaRepoMock.Object,
            _refugioRepoMock.Object,
            _loggerMock.Object);
    }

    // ── Catálogo ─────────────────────────────────────────────

    [Fact]
    public async Task ObtenerCatalogoAsync_SinFiltros_DebeRetornarMascotas()
    {
        // Arrange
        var mascotas = new List<Mascota>
        {
            new() { Id = 1, Nombre = "Max", Especie = Especie.Perro, EdadMeses = 24,
                    Vacunada = true, Castrada = false, Disponible = true, RefugioId = 1 },
            new() { Id = 2, Nombre = "Luna", Especie = Especie.Gato, EdadMeses = 12,
                    Vacunada = true, Castrada = true, Disponible = true, RefugioId = 1 }
        };

        _mascotaRepoMock.Setup(r => r.BuscarAsync(null, null, null, true)).ReturnsAsync(mascotas);
        _refugioRepoMock.Setup(r => r.ObtenerPorIdAsync(1)).ReturnsAsync(_refugio);

        // Act
        var resultado = (await _sut.ObtenerCatalogoAsync()).ToList();

        // Assert
        resultado.Should().HaveCount(2);
        resultado.Should().Contain(m => m.Nombre == "Max");
        resultado.Should().Contain(m => m.Nombre == "Luna");
        resultado.All(m => m.NombreRefugio == "Patitas Felices").Should().BeTrue();
    }

    // ── Crear mascota ────────────────────────────────────────

    [Fact]
    public async Task CrearMascotaAsync_ConRefugioValido_DebeCrearYRetornarResumen()
    {
        // Arrange
        _refugioRepoMock.Setup(r => r.ObtenerPorIdAsync(1)).ReturnsAsync(_refugio);
        _mascotaRepoMock
            .Setup(r => r.CrearAsync(It.IsAny<Mascota>()))
            .ReturnsAsync((Mascota m) => { m.Id = 100; return m; });

        var dto = new MascotaFormDto("Rocky", Especie.Perro, "Pastor Alemán",
            36, "Muy juguetón", true, false);

        // Act
        var resultado = await _sut.CrearMascotaAsync(dto, 1, "/foto/rocky.jpg");

        // Assert
        resultado.Should().NotBeNull();
        resultado.Id.Should().Be(100);
        resultado.Nombre.Should().Be("Rocky");
        resultado.Especie.Should().Be(Especie.Perro);
        resultado.Raza.Should().Be("Pastor Alemán");
        resultado.NombreRefugio.Should().Be("Patitas Felices");
    }

    [Fact]
    public async Task CrearMascotaAsync_ConRefugioInexistente_DebeLanzarEntidadNoEncontrada()
    {
        // Arrange
        _refugioRepoMock.Setup(r => r.ObtenerPorIdAsync(999)).ReturnsAsync((Refugio?)null);

        var dto = new MascotaFormDto("Test", Especie.Gato, null, 12, null, false, false);

        // Act & Assert
        await _sut.Invoking(s => s.CrearMascotaAsync(dto, 999, null))
            .Should().ThrowAsync<EntidadNoEncontradaException>();
    }

    // ── Actualizar mascota ───────────────────────────────────

    [Fact]
    public async Task ActualizarMascotaAsync_ConRefugioCorrecto_DebeActualizar()
    {
        // Arrange
        var mascota = new Mascota
        {
            Id = 5, Nombre = "Original", Especie = Especie.Perro,
            RefugioId = 1, Disponible = true
        };
        _mascotaRepoMock.Setup(r => r.ObtenerPorIdAsync(5)).ReturnsAsync(mascota);

        var dto = new MascotaFormDto("Actualizado", Especie.Perro, "Labrador",
            24, "Actualizado", true, true);

        // Act
        await _sut.ActualizarMascotaAsync(5, dto, 1);

        // Assert
        _mascotaRepoMock.Verify(r => r.ActualizarAsync(It.Is<Mascota>(m =>
            m.Nombre == "Actualizado" && m.Raza == "Labrador")), Times.Once);
    }

    [Fact]
    public async Task ActualizarMascotaAsync_ConRefugioIncorrecto_DebeLanzarAccesoDenegado()
    {
        // Arrange
        var mascota = new Mascota { Id = 5, Nombre = "Test", RefugioId = 1 };
        _mascotaRepoMock.Setup(r => r.ObtenerPorIdAsync(5)).ReturnsAsync(mascota);

        var dto = new MascotaFormDto("Hack", Especie.Gato, null, 12, null, false, false);

        // Act & Assert — refugioId 99 no es el dueño
        await _sut.Invoking(s => s.ActualizarMascotaAsync(5, dto, 99))
            .Should().ThrowAsync<AccesoDenegadoException>();
    }

    // ── Eliminar mascota ─────────────────────────────────────

    [Fact]
    public async Task EliminarMascotaAsync_ConRefugioCorrecto_DebeEliminar()
    {
        // Arrange
        var mascota = new Mascota { Id = 5, Nombre = "Test", RefugioId = 1 };
        _mascotaRepoMock.Setup(r => r.ObtenerPorIdAsync(5)).ReturnsAsync(mascota);

        // Act
        await _sut.EliminarMascotaAsync(5, 1);

        // Assert
        _mascotaRepoMock.Verify(r => r.EliminarAsync(5), Times.Once);
    }

    [Fact]
    public async Task EliminarMascotaAsync_ConRefugioIncorrecto_DebeLanzarAccesoDenegado()
    {
        // Arrange
        var mascota = new Mascota { Id = 5, Nombre = "Test", RefugioId = 1 };
        _mascotaRepoMock.Setup(r => r.ObtenerPorIdAsync(5)).ReturnsAsync(mascota);

        // Act & Assert
        await _sut.Invoking(s => s.EliminarMascotaAsync(5, 99))
            .Should().ThrowAsync<AccesoDenegadoException>();
    }

    // ── Detalle ──────────────────────────────────────────────

    [Fact]
    public async Task ObtenerDetalleAsync_ConMascotaExistente_DebeRetornarDetalle()
    {
        // Arrange
        var mascota = new Mascota
        {
            Id = 5, Nombre = "Max", Especie = Especie.Perro, Raza = "Labrador",
            EdadMeses = 24, Descripcion = "Amigable", FotoUrl = "/foto.jpg",
            Vacunada = true, Castrada = false, Disponible = true, RefugioId = 1
        };
        _mascotaRepoMock.Setup(r => r.ObtenerPorIdAsync(5)).ReturnsAsync(mascota);
        _refugioRepoMock.Setup(r => r.ObtenerPorIdAsync(1)).ReturnsAsync(_refugio);

        // Act
        var resultado = await _sut.ObtenerDetalleAsync(5);

        // Assert
        resultado.Id.Should().Be(5);
        resultado.Nombre.Should().Be("Max");
        resultado.NombreRefugio.Should().Be("Patitas Felices");
        resultado.Disponible.Should().BeTrue();
    }

    [Fact]
    public async Task ObtenerDetalleAsync_ConMascotaInexistente_DebeLanzarEntidadNoEncontrada()
    {
        // Arrange
        _mascotaRepoMock.Setup(r => r.ObtenerPorIdAsync(999)).ReturnsAsync((Mascota?)null);

        // Act & Assert
        await _sut.Invoking(s => s.ObtenerDetalleAsync(999))
            .Should().ThrowAsync<EntidadNoEncontradaException>();
    }
}
