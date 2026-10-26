using BLL.Dtos;
using PetFinder.Web.Options;

namespace PetFinder.Web.Models;

/// <summary>
/// Modelo de vista para la página principal (Landing Page).
/// Provee los datos necesarios para las secciones de impacto, mapa, catálogo destacado y avisos comunitarios.
/// </summary>
public class HomeViewModel
{
    /// <summary>Cifras de impacto comunitario para los contadores animados.</summary>
    public EstadisticasImpactoDto Estadisticas { get; set; } = null!;

    /// <summary>Lista de refugios activos con geolocalización para alimentar la lista y mapa.</summary>
    public IEnumerable<RefugioMapaDto> Refugios { get; set; } = new List<RefugioMapaDto>();

    /// <summary>Mascotas destacadas para adopción (4 seleccionadas).</summary>
    public IEnumerable<MascotaResumenDto> MascotasDestacadas { get; set; } = new List<MascotaResumenDto>();

    /// <summary>Avisos recientes del tablero comunitario de mascotas perdidas/encontradas.</summary>
    public IEnumerable<ReporteResumenDto> AvisosRecientes { get; set; } = new List<ReporteResumenDto>();

    /// <summary>Configuración inicial del mapa desde appsettings.json.</summary>
    public MapaOptions MapaConfig { get; set; } = new();
}
