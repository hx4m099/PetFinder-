using BLL.Dtos;
using BLL.Services;
using ENTITY.Constants;
using ENTITY.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PetFinder.Web.Helpers;
using PetFinder.Web.ViewModels;

namespace PetFinder.Web.Controllers;

/// <summary>
/// Controlador para reportes de mascotas perdidas/encontradas y búsqueda de coincidencias.
/// </summary>
[Authorize]
public class ReporteController : Controller
{
    private readonly IReporteService     _reporteService;
    private readonly ICoincidenciaService _coincidenciaService;
    private readonly IWebHostEnvironment _env;
    private readonly ILogger<ReporteController> _logger;

    public ReporteController(
        IReporteService reporteService,
        ICoincidenciaService coincidenciaService,
        IWebHostEnvironment env,
        ILogger<ReporteController> logger)
    {
        _reporteService     = reporteService;
        _coincidenciaService = coincidenciaService;
        _env    = env;
        _logger = logger;
    }

    // GET /Reporte/Crear
    [HttpGet]
    public IActionResult Crear() => View(new ReporteFormViewModel());

    // POST /Reporte/Crear
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Crear(ReporteFormViewModel vm)
    {
        if (!ModelState.IsValid) return View(vm);

        try
        {
            var fotoUrl = await ImagenHelper.GuardarImagenAsync(
                vm.Foto, _env, PetFinderConstants.RutaFotosReportes);

            var dto     = new ReporteFormDto(vm.TipoReporte, vm.Especie, vm.NombreMascota,
                                              vm.Raza, vm.ColorPelaje, vm.Ciudad, vm.Descripcion);
            var reporte = await _reporteService.CrearReporteAsync(dto, GetUsuarioId(), fotoUrl);

            TempData["Success"] = "Reporte creado. Buscando coincidencias...";
            return RedirectToAction(nameof(BuscarCoincidencias), new { id = reporte.Id });
        }
        catch (Exception ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            return View(vm);
        }
    }

    // GET /Reporte/BuscarCoincidencias/5
    [HttpGet]
    public async Task<IActionResult> BuscarCoincidencias(int id)
    {
        var reporte = await _reporteService.ObtenerPorIdAsync(id);
        return View(new CoincidenciasViewModel { Reporte = reporte, Buscando = true });
    }

    // GET /Reporte/Coincidencias/5  (AJAX polling o retorno directo)
    [HttpGet]
    public async Task<IActionResult> Coincidencias(int id)
    {
        var reporte      = await _reporteService.ObtenerPorIdAsync(id);
        var coincidencias = await _coincidenciaService.BuscarCoincidenciasAsync(id);
        return View(new CoincidenciasViewModel
        {
            Reporte       = reporte,
            Coincidencias = coincidencias,
            Buscando      = false
        });
    }

    // GET /Reporte/MisReportes
    [HttpGet]
    public async Task<IActionResult> MisReportes()
    {
        var reportes = await _reporteService.ObtenerMisReportesAsync(GetUsuarioId());
        return View(reportes);
    }

    // POST /Reporte/Desactivar/5
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Desactivar(int id)
    {
        await _reporteService.DesactivarAsync(id, GetUsuarioId());
        TempData["Success"] = "Reporte marcado como resuelto.";
        return RedirectToAction(nameof(MisReportes));
    }

    // ── Helpers ──────────────────────────────────────────────────────────────
    private int GetUsuarioId()
        => int.Parse(User.FindFirst(PetFinderConstants.ClaimUserId)!.Value);
}
