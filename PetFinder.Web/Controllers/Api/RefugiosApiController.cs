// PetFinder.Web/Controllers/Api/RefugiosApiController.cs
using BLL.Dtos;
using BLL.Services;
using ENTITY.Enums;
using Microsoft.AspNetCore.Mvc;

namespace PetFinder.Web.Controllers.Api;

/// <summary>
/// Endpoint API REST para consultar la lista de refugios asociados con geolocalización,
/// disponibilidad de mascotas y cálculo de distancia relativa al usuario.
/// </summary>
[ApiController]
[Route("api/refugios")]
[Produces("application/json")]
public class RefugiosApiController : ControllerBase
{
    private readonly IRefugioService _refugioService;
    private readonly ILogger<RefugiosApiController> _logger;

    public RefugiosApiController(IRefugioService refugioService, ILogger<RefugiosApiController> logger)
    {
        _refugioService = refugioService;
        _logger = logger;
    }

    /// <summary>
    /// Retorna los refugios aliados activos con coordenadas válidas, mascotas disponibles
    /// y distancia opcional si se proporcionan las coordenadas del usuario.
    /// </summary>
    /// <param name="lat">Latitud del usuario (opcional)</param>
    /// <param name="lon">Longitud del usuario (opcional)</param>
    /// <param name="especie">Filtro de especie disponible en el refugio (opcional)</param>
    /// <returns>Lista de refugios geolocalizados</returns>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<RefugioMapaDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> ObtenerRefugios(
        [FromQuery] double? lat = null,
        [FromQuery] double? lon = null,
        [FromQuery] string? especie = null)
    {
        _logger.LogInformation("Consulta API refugios para mapa: lat={Lat}, lon={Lon}, especie={Especie}", lat, lon, especie);
        var refugios = await _refugioService.ObtenerParaMapaAsync(lat, lon, especie);
        return Ok(refugios);
    }
}
