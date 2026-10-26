// PetFinder.Web/Controllers/HomeController.cs
using System.Diagnostics;
using BLL.Dtos;
using BLL.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using PetFinder.Web.Models;
using PetFinder.Web.Options;

namespace PetFinder.Web.Controllers;

public class HomeController : Controller
{
    private readonly IEstadisticasService _estadisticasService;
    private readonly IRefugioService _refugioService;
    private readonly IAdopcionService _adopcionService;
    private readonly IReporteService _reporteService;
    private readonly IOptions<MapaOptions> _mapaOptions;
    private readonly ILogger<HomeController> _logger;

    public HomeController(
        IEstadisticasService estadisticasService,
        IRefugioService refugioService,
        IAdopcionService adopcionService,
        IReporteService reporteService,
        IOptions<MapaOptions> mapaOptions,
        ILogger<HomeController> logger)
    {
        _estadisticasService = estadisticasService;
        _refugioService = refugioService;
        _adopcionService = adopcionService;
        _reporteService = reporteService;
        _mapaOptions = mapaOptions;
        _logger = logger;
    }

    public async Task<IActionResult> Index()
    {
        try
        {
            var statsTask = _estadisticasService.ObtenerEstadisticasImpactoAsync();
            var refugiosTask = _refugioService.ObtenerParaMapaAsync();
            var catalogoTask = _adopcionService.ObtenerCatalogoAsync();
            var reportesTask = _reporteService.ObtenerTodosActivosAsync();

            await Task.WhenAll(statsTask, refugiosTask, catalogoTask, reportesTask);

            var viewModel = new HomeViewModel
            {
                Estadisticas = await statsTask,
                Refugios = await refugiosTask,
                MascotasDestacadas = (await catalogoTask).Take(4),
                AvisosRecientes = (await reportesTask).Take(6),
                MapaConfig = _mapaOptions.Value
            };

            return View(viewModel);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al cargar la información para la página principal.");
            var fallbackModel = new HomeViewModel
            {
                Estadisticas = new EstadisticasImpactoDto(0, 0, 0, 0),
                MapaConfig = _mapaOptions.Value
            };
            return View(fallbackModel);
        }
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
