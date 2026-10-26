using ENTITY.Constants;

namespace PetFinder.Web.Helpers;

/// <summary>
/// Utilidad para guardar imágenes subidas por el usuario en wwwroot/uploads.
/// </summary>
public static class ImagenHelper
{
    private static readonly string[] _extensionesPermitidas = [".jpg", ".jpeg", ".png", ".webp", ".gif"];

    /// <summary>
    /// Guarda un archivo de imagen en la carpeta indicada dentro de wwwroot.
    /// Devuelve la URL relativa (ej. /uploads/mascotas/abc123.jpg) o null si no hay archivo.
    /// </summary>
    public static async Task<string?> GuardarImagenAsync(
        IFormFile? archivo,
        IWebHostEnvironment env,
        string subcarpeta)
    {
        if (archivo is null || archivo.Length == 0)
            return null;

        if (archivo.Length > PetFinderConstants.TamanioMaxImagenBytes)
            throw new InvalidOperationException($"La imagen supera el tamaño máximo de 5 MB.");

        var extension = Path.GetExtension(archivo.FileName).ToLowerInvariant();
        if (!_extensionesPermitidas.Contains(extension))
            throw new InvalidOperationException($"Solo se permiten imágenes: {string.Join(", ", _extensionesPermitidas)}");

        var carpeta = Path.Combine(env.WebRootPath, subcarpeta);
        Directory.CreateDirectory(carpeta);

        var nombreArchivo = $"{Guid.NewGuid()}{extension}";
        var rutaCompleta  = Path.Combine(carpeta, nombreArchivo);

        await using var stream = new FileStream(rutaCompleta, FileMode.Create);
        await archivo.CopyToAsync(stream);

        // Retorna la URL relativa para usar en src de img
        return $"/{subcarpeta.Replace("\\", "/")}/{nombreArchivo}";
    }
}
