namespace ENTITY.Constants;

/// <summary>
/// Constantes globales de la aplicación PetFinder.
/// Centraliza "magic strings" y valores fijos para evitar duplicación.
/// </summary>
public static class PetFinderConstants
{
    // ── Roles (deben coincidir con el enum Rol) ──────────────────────────────
    public const string RolAdoptante    = "Adoptante";
    public const string RolRefugio      = "Refugio";
    public const string RolAdministrador = "Administrador";

    // ── Cookies de autenticación ─────────────────────────────────────────────
    public const string AuthCookieName  = "PetFinder.Auth";
    public const string ClaimUserId     = "UserId";
    public const string ClaimRol        = "Rol";
    public const string ClaimNombre     = "Nombre";
    public const string ClaimRefugioId  = "RefugioId";

    // ── Reglas de negocio ────────────────────────────────────────────────────
    /// <summary>Número de días recientes para filtrar reportes de mascotas.</summary>
    public const int DiasFiltroReporteReciente = 30;

    /// <summary>Porcentaje mínimo de similitud para mostrar una coincidencia.</summary>
    public const double UmbralSimilitud = 0.35;

    /// <summary>Número máximo de coincidencias a devolver.</summary>
    public const int MaxCoincidencias = 10;

    /// <summary>Duración estándar de una cita en horas.</summary>
    public const int DuracionCitaHoras = 1;

    // ── Imágenes ─────────────────────────────────────────────────────────────
    public const string RutaFotosReportes   = "uploads/reportes";
    public const string RutaFotosMascotas   = "uploads/mascotas";
    public const long   TamanioMaxImagenBytes = 5 * 1024 * 1024; // 5 MB

    // ── Paginación ───────────────────────────────────────────────────────────
    public const int TamanioPageDefault = 12;

    // ── Mensajes de error ────────────────────────────────────────────────────
    public const string ErrorCitaDuplicada       = "Ya existe una cita en ese horario para el refugio.";
    public const string ErrorEstadoInvalido      = "La transición de estado no está permitida.";
    public const string ErrorMascotaNoEncontrada = "No se encontró la mascota solicitada.";
    public const string ErrorUsuarioNoEncontrado = "No se encontró el usuario solicitado.";
    public const string ErrorRefugioNoEncontrado = "No se encontró el refugio solicitado.";
    public const string ErrorAccesoDenegado      = "No tienes permisos para realizar esta acción.";
    public const string ErrorReporteNoEncontrado = "No se encontró el reporte solicitado.";
}
