namespace PetFinder.Web.Options;

/// <summary>
/// Opciones de configuración para el mapa interactivo de refugios.
/// Permite configurar el centro y zoom inicial desde appsettings.json sin quemar código.
/// </summary>
public class MapaOptions
{
    public const string SectionName = "MapaOptions";

    /// <summary>Latitud inicial para el centro del mapa.</summary>
    public double CentroLatitud { get; set; } = 19.4172;

    /// <summary>Longitud inicial para el centro del mapa.</summary>
    public double CentroLongitud { get; set; } = -99.1643;

    /// <summary>Nivel de zoom inicial del mapa.</summary>
    public int ZoomInicial { get; set; } = 13;
}
