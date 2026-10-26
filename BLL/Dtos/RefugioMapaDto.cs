namespace BLL.Dtos;

/// <summary>
/// DTO con datos geográficos y de disponibilidad para el mapa interactivo de refugios.
/// </summary>
/// <param name="Id">Identificador del refugio.</param>
/// <param name="Nombre">Nombre del refugio.</param>
/// <param name="Direccion">Dirección física.</param>
/// <param name="Horario">Horario de atención.</param>
/// <param name="Latitud">Coordenada de latitud.</param>
/// <param name="Longitud">Coordenada de longitud.</param>
/// <param name="Telefono">Teléfono de contacto.</param>
/// <param name="FotoUrl">URL de la fotografía del refugio.</param>
/// <param name="MascotasDisponibles">Cantidad de mascotas disponibles para adopción.</param>
/// <param name="EspeciesDisponibles">Lista de especies disponibles en el refugio.</param>
/// <param name="DistanciaKm">Distancia en kilómetros respecto a la ubicación del usuario (si está disponible).</param>
public record RefugioMapaDto(
    int Id,
    string Nombre,
    string? Direccion,
    string? Horario,
    double Latitud,
    double Longitud,
    string? Telefono,
    string? FotoUrl,
    int MascotasDisponibles,
    List<string> EspeciesDisponibles,
    double? DistanciaKm = null
);
