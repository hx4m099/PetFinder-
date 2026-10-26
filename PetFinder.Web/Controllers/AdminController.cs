using BLL.Services;
using ENTITY.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PetFinder.Web.ViewModels;

namespace PetFinder.Web.Controllers;

/// <summary>
/// Panel del administrador: estadísticas y gestión básica.
/// Solo accesible para el rol Administrador.
/// </summary>
[Authorize(Roles = PetFinderConstants.RolAdministrador)]
public class AdminController : Controller
{
    private readonly IAdopcionService  _adopcionService;
    private readonly IReporteService   _reporteService;
    private readonly IRefugioService   _refugioService;
    private readonly ILogger<AdminController> _logger;

    public AdminController(
        IAdopcionService adopcionService,
        IReporteService reporteService,
        IRefugioService refugioService,
        ILogger<AdminController> logger)
    {
        _adopcionService = adopcionService;
        _reporteService  = reporteService;
        _refugioService  = refugioService;
        _logger          = logger;
    }

    // GET /Admin/Panel
    [HttpGet]
    public async Task<IActionResult> Panel()
    {
        var refugios = (await _refugioService.ObtenerTodosAsync()).ToList();
        var mascotas = (await _adopcionService.ObtenerCatalogoAsync()).ToList();
        var reportes = (await _reporteService.ObtenerTodosActivosAsync()).ToList();

        return View(new PanelAdminViewModel
        {
            TotalRefugios  = refugios.Count,
            TotalMascotas  = mascotas.Count,
            TotalReportes  = reportes.Count,
            UltimosReportes = reportes.Take(5)
        });
    }

    // GET /Admin/Refugios
    [HttpGet]
    public async Task<IActionResult> Refugios()
    {
        var refugios = await _refugioService.ObtenerTodosAsync();
        return View(refugios);
    }

    // GET /Admin/Reportes
    [HttpGet]
    public async Task<IActionResult> Reportes()
    {
        var reportes = await _reporteService.ObtenerTodosActivosAsync();
        return View(reportes);
    }
}
