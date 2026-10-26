namespace ENTITY.Enums;

/// <summary>
/// Define los roles de usuario disponibles en el sistema PetFinder.
/// </summary>
public enum Rol
{
    /// <summary>Usuario que busca adoptar mascotas o reporta mascotas perdidas.</summary>
    Adoptante = 1,

    /// <summary>Organización que publica mascotas disponibles para adopción.</summary>
    Refugio = 2,

    /// <summary>Administrador con acceso completo al sistema.</summary>
    Administrador = 3
}
