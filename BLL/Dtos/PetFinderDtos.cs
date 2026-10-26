using ENTITY.Enums;

namespace BLL.Dtos;

// ── Auth ─────────────────────────────────────────────────────────────────────

/// <summary>Datos necesarios para registrar un nuevo usuario.</summary>
public record RegistroDto(
    string Nombre,
    string Email,
    string Password,
    string ConfirmPassword,
    Rol    Rol,
    string? Telefono,
    string? Ciudad
);

/// <summary>Datos de inicio de sesión.</summary>
public record LoginDto(
    string Email,
    string Password
);

/// <summary>Resultado de una autenticación exitosa.</summary>
public record UsuarioSesionDto(
    int    Id,
    string Nombre,
    string Email,
    Rol    Rol,
    int?   RefugioId   // null si el usuario no es Refugio
);

// ── Usuario ──────────────────────────────────────────────────────────────────

/// <summary>Vista resumida de un usuario para listados administrativos.</summary>
public record UsuarioResumenDto(
    int    Id,
    string Nombre,
    string Email,
    Rol    Rol,
    string? Ciudad,
    bool   Activo,
    DateTime FechaRegistro
);

// ── Refugio ──────────────────────────────────────────────────────────────────

/// <summary>Vista completa de un refugio para la pantalla de detalle.</summary>
public record RefugioDto(
    int     Id,
    string  Nombre,
    string? Descripcion,
    string? Direccion,
    string  Ciudad,
    string? Email,
    string? Telefono,
    string? FotoUrl,
    string? HorarioAtencion
);

/// <summary>Datos para crear o editar un refugio.</summary>
public record RefugioFormDto(
    string  Nombre,
    string? Descripcion,
    string? Direccion,
    string  Ciudad,
    string? Email,
    string? Telefono,
    string? HorarioAtencion
);

// ── Mascota ──────────────────────────────────────────────────────────────────

/// <summary>Vista resumida de mascota para tarjetas del catálogo.</summary>
public record MascotaResumenDto(
    int     Id,
    string  Nombre,
    Especie Especie,
    string? Raza,
    int     EdadMeses,
    string? FotoUrl,
    bool    Vacunada,
    bool    Castrada,
    string  NombreRefugio,
    string  CiudadRefugio
);

/// <summary>Vista completa de mascota para la página de detalle + agendar cita.</summary>
public record MascotaDetalleDto(
    int     Id,
    string  Nombre,
    Especie Especie,
    string? Raza,
    int     EdadMeses,
    string? Descripcion,
    string? FotoUrl,
    bool    Vacunada,
    bool    Castrada,
    bool    Disponible,
    int     RefugioId,
    string  NombreRefugio,
    string  CiudadRefugio,
    string? HorarioRefugio
);

/// <summary>Datos para crear o editar una mascota en el panel del refugio.</summary>
public record MascotaFormDto(
    string  Nombre,
    Especie Especie,
    string? Raza,
    int     EdadMeses,
    string? Descripcion,
    bool    Vacunada,
    bool    Castrada
);

// ── Reporte de mascota ────────────────────────────────────────────────────────

/// <summary>Datos para crear un reporte de mascota perdida o encontrada.</summary>
public record ReporteFormDto(
    TipoReporte TipoReporte,
    Especie     Especie,
    string?     NombreMascota,
    string?     Raza,
    string?     ColorPelaje,
    string      Ciudad,
    string?     Descripcion
);

/// <summary>Vista resumida de un reporte para listados.</summary>
public record ReporteResumenDto(
    int         Id,
    TipoReporte TipoReporte,
    Especie     Especie,
    string?     NombreMascota,
    string      Ciudad,
    string?     FotoUrl,
    DateTime    FechaReporte,
    bool        Activo,
    string      NombreUsuario
);

// ── Coincidencias ─────────────────────────────────────────────────────────────

/// <summary>
/// Resultado de comparar un reporte con otro: incluye el porcentaje de similitud.
/// </summary>
public record CoincidenciaDto(
    int         ReporteId,
    TipoReporte TipoReporte,
    Especie     Especie,
    string?     NombreMascota,
    string?     Raza,
    string?     ColorPelaje,
    string      Ciudad,
    string?     FotoUrl,
    string?     Descripcion,
    DateTime    FechaReporte,
    double      PorcentajeSimilitud,   // 0.0 – 1.0
    string      NombreUsuario,
    string?     TelefonoUsuario
);

// ── Cita ──────────────────────────────────────────────────────────────────────

/// <summary>Datos para agendar una nueva cita.</summary>
public record CitaFormDto(
    int      MascotaId,
    int      RefugioId,
    DateTime FechaHora,
    string?  NotasAdoptante
);

/// <summary>Vista de una cita para "Mis citas".</summary>
public record CitaDto(
    int         Id,
    DateTime    FechaHora,
    EstadoCita  Estado,
    string      NombreMascota,
    string?     FotoMascota,
    string      NombreRefugio,
    string      CiudadRefugio,
    string?     NotasAdoptante,
    string?     NotasRefugio,
    DateTime    FechaCreacion
);

/// <summary>Datos para que el refugio actualice el estado de una cita.</summary>
public record ActualizarCitaDto(
    int        CitaId,
    EstadoCita NuevoEstado,
    string?    NotasRefugio
);
