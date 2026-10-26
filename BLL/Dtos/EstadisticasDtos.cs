namespace BLL.Dtos;

/// <summary>
/// Cifras de impacto comunitario para la landing page.
/// </summary>
/// <param name="MascotasReunidas">Total de mascotas rescatadas / reunidas con sus familias.</param>
/// <param name="RefugiosAliados">Cantidad de refugios aliados registrados y activos.</param>
/// <param name="AdopcionesConcretadas">Total de adopciones completadas exitosamente.</param>
/// <param name="ReportesActivos">Reportes de mascotas perdidas/encontradas actualmente activos.</param>
public record EstadisticasImpactoDto(
    int MascotasReunidas,
    int RefugiosAliados,
    int AdopcionesConcretadas,
    int ReportesActivos
);
