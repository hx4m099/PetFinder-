// PetFinder.Tests/BllTests/RefugioServiceTests.cs
// ═══════════════════════════════════════════════════════════════
// PetFinder — Tests unitarios: RefugioService
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

public class RefugioServiceTests
{
    private readonly Mock<IRefugioRepository> _repoMock;
    private readonly Mock<ILogger<RefugioService>> _loggerMock;
    private readonly RefugioService _sut;

    public RefugioServiceTests()
    {
        _repoMock = new Mock<IRefugioRepository>();
        _loggerMock = new Mock<ILogger<RefugioService>>();
        _sut = new RefugioService(_repoMock.Object, _loggerMock.Object);
    }

    [Fact]
    public async Task ObtenerParaMapaAsync_SoloRefugiosConCoordenadasValidas_DebeExcluirInactivosYSinCoordenadas()
    {
        // Arrange
        var refugios = new List<Refugio>
        {
            new() { Id = 1, Nombre = "Refugio Válido", Latitud = 19.4172, Longitud = -99.1643, Activo = true },
            new() { Id = 2, Nombre = "Sin Coordenadas", Latitud = null, Longitud = null, Activo = true },
            new() { Id = 3, Nombre = "Null Island", Latitud = 0.0, Longitud = 0.0, Activo = true },
            new() { Id = 4, Nombre = "Inactivo", Latitud = 19.4200, Longitud = -99.1700, Activo = false }
        };

        _repoMock.Setup(r => r.ObtenerTodosAsync()).ReturnsAsync(refugios);

        // Act
        var resultado = (await _sut.ObtenerParaMapaAsync()).ToList();

        // Assert
        resultado.Should().HaveCount(1);
        resultado.First().Id.Should().Be(1);
        resultado.First().Nombre.Should().Be("Refugio Válido");
    }

    [Fact]
    public async Task ObtenerParaMapaAsync_CalculaCantidadMascotasDisponibles_Correctamente()
    {
        // Arrange
        var refugio = new Refugio
        {
            Id = 1,
            Nombre = "Refugio Mascotas",
            Latitud = 19.4172,
            Longitud = -99.1643,
            Activo = true,
            Mascotas = new List<Mascota>
            {
                new() { Id = 10, Nombre = "Max", Especie = Especie.Perro, Disponible = true },
                new() { Id = 11, Nombre = "Mimi", Especie = Especie.Gato, Disponible = true },
                new() { Id = 12, Nombre = "Rocky", Especie = Especie.Perro, Disponible = false } // No disponible
            }
        };

        _repoMock.Setup(r => r.ObtenerTodosAsync()).ReturnsAsync(new List<Refugio> { refugio });

        // Act
        var resultado = (await _sut.ObtenerParaMapaAsync()).ToList();

        // Assert
        resultado.Should().HaveCount(1);
        resultado.First().MascotasDisponibles.Should().Be(2);
        resultado.First().EspeciesDisponibles.Should().Contain(new[] { "Perro", "Gato" });
    }

    [Fact]
    public async Task ObtenerParaMapaAsync_ConUbicacionUsuario_OrdenaPorDistanciaAscendente()
    {
        // Arrange
        // Usuario en Roma Norte: 19.4172, -99.1643
        double latUsuario = 19.4172;
        double lonUsuario = -99.1643;

        var cercano = new Refugio { Id = 1, Nombre = "Cercano Roma", Latitud = 19.4180, Longitud = -99.1650, Activo = true };
        var lejano = new Refugio { Id = 2, Nombre = "Lejano Coyoacán", Latitud = 19.3490, Longitud = -99.1620, Activo = true };
        var intermedio = new Refugio { Id = 3, Nombre = "Medio Polanco", Latitud = 19.4326, Longitud = -99.1912, Activo = true };

        _repoMock.Setup(r => r.ObtenerTodosAsync()).ReturnsAsync(new List<Refugio> { lejano, cercano, intermedio });

        // Act
        var resultado = (await _sut.ObtenerParaMapaAsync(latUsuario, lonUsuario)).ToList();

        // Assert
        resultado.Should().HaveCount(3);
        resultado[0].Id.Should().Be(1); // Cercano primero
        resultado[1].Id.Should().Be(3); // Intermedio segundo
        resultado[2].Id.Should().Be(2); // Lejano al final
        resultado[0].DistanciaKm.Should().BeLessThan(resultado[1].DistanciaKm!.Value);
        resultado[1].DistanciaKm.Should().BeLessThan(resultado[2].DistanciaKm!.Value);
    }

    [Fact]
    public async Task ObtenerParaMapaAsync_ConFiltroEspecie_FiltraSoloRefugiosConEsaEspecie()
    {
        // Arrange
        var refugioSoloPerros = new Refugio
        {
            Id = 1,
            Nombre = "Solo Canes",
            Latitud = 19.4172,
            Longitud = -99.1643,
            Activo = true,
            Mascotas = new List<Mascota> { new() { Id = 1, Nombre = "Fido", Especie = Especie.Perro, Disponible = true } }
        };

        var refugioConGatos = new Refugio
        {
            Id = 2,
            Nombre = "Mundo Felino",
            Latitud = 19.4190,
            Longitud = -99.1670,
            Activo = true,
            Mascotas = new List<Mascota> { new() { Id = 2, Nombre = "Michi", Especie = Especie.Gato, Disponible = true } }
        };

        _repoMock.Setup(r => r.ObtenerTodosAsync()).ReturnsAsync(new List<Refugio> { refugioSoloPerros, refugioConGatos });

        // Act
        var resultadoGatos = (await _sut.ObtenerParaMapaAsync(especie: "Gato")).ToList();

        // Assert
        resultadoGatos.Should().HaveCount(1);
        resultadoGatos.First().Id.Should().Be(2);
        resultadoGatos.First().Nombre.Should().Be("Mundo Felino");
    }
}
