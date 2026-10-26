using DAL.Interfaces;
using DAL.Repositories;
using DAL.Seed;
using Microsoft.Extensions.DependencyInjection;

namespace DAL.Extensions;

/// <summary>
/// Métodos de extensión para registrar todos los servicios del proyecto DAL
/// en el contenedor de inyección de dependencias.
/// </summary>
public static class DalServiceExtensions
{
    /// <summary>
    /// Registra el almacén de datos en memoria y todos los repositorios en el DI container.
    /// Al conectar Oracle, solo se reemplaza este método: se quita el Singleton del store
    /// y se agrega el DbContext + implementaciones EF Core.
    /// </summary>
    /// <param name="services">La colección de servicios de la aplicación.</param>
    /// <returns>La colección de servicios para encadenamiento.</returns>
    public static IServiceCollection AddDal(this IServiceCollection services)
    {
        // Data store singleton (solo en modo in-memory)
        services.AddSingleton<InMemoryDataStore>();

        // Repositorios
        services.AddScoped<IUsuarioRepository, UsuarioRepository>();
        services.AddScoped<IRefugioRepository, RefugioRepository>();
        services.AddScoped<IMascotaRepository, MascotaRepository>();
        services.AddScoped<IReporteRepository, ReporteRepository>();
        services.AddScoped<ICitaRepository,    CitaRepository>();

        return services;
    }
}
