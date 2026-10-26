using BLL.Services;
using BLL.Validators;
using DAL.Extensions;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

namespace BLL.Extensions;

/// <summary>
/// Métodos de extensión para registrar todos los servicios del proyecto BLL
/// en el contenedor de inyección de dependencias.
/// La GUI solo necesita llamar a <see cref="AddBll"/> desde Program.cs.
/// </summary>
public static class BllServiceExtensions
{
    /// <summary>
    /// Registra BLL (servicios + validadores) y DAL (repositorios).
    /// Internamente llama a <see cref="DalServiceExtensions.AddDal"/> para
    /// mantener el principio de encapsulamiento entre capas.
    /// </summary>
    /// <param name="services">La colección de servicios de la aplicación.</param>
    /// <returns>La colección de servicios para encadenamiento.</returns>
    public static IServiceCollection AddBll(this IServiceCollection services)
    {
        // 1. Registrar capa DAL (repositorios en memoria)
        services.AddDal();

        // 2. Servicio de similitud de imágenes (mock, intercambiable por OCP)
        services.AddScoped<IImageSimilarityService, MockImageSimilarityService>();

        // 3. Servicios de negocio
        services.AddScoped<IAuthService,        AuthService>();
        services.AddScoped<IReporteService,     ReporteService>();
        services.AddScoped<ICoincidenciaService, CoincidenciaService>();
        services.AddScoped<IAdopcionService,    AdopcionService>();
        services.AddScoped<ICitaService,        CitaService>();
        services.AddScoped<IRefugioService,     RefugioService>();
        services.AddScoped<IEstadisticasService, EstadisticasService>();

        // 4. Validadores FluentValidation (auto-discovery por ensamblado)
        services.AddValidatorsFromAssemblyContaining<RegistroDtoValidator>();

        return services;
    }
}
