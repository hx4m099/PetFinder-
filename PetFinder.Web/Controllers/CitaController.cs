using BLL.Dtos;
using BLL.Services;
using ENTITY.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PetFinder.Web.ViewModels;

namespace PetFinder.Web.Controllers;

/// <summary>
/// Gestión de citas del adoptante: ver, cancelar.
/// </summary>
[Authorize]
public class CitaController : Controller
{
    private readonly ICitaService _citaService;
    private readonly ILogger<CitaController> _logger;

    public CitaController(ICitaService citaService, ILogger<CitaController> logger)
    {
        _citaService = citaService;
        _logger      = logger;
    }

    // GET /Cita/MisCitas
    [HttpGet]
    public async Task<IActionResult> MisCitas()
    {
        var citas = (await _citaService.ObtenerMisCitasAsync(GetUsuarioId())).ToList();
        return View(new MisCitasViewModel
        {
            Pendientes  = citas.Where(c => c.Estado == ENTITY.Enums.EstadoCita.Pendiente),
            Confirmadas = citas.Where(c => c.Estado == ENTITY.Enums.EstadoCita.Confirmada),
            Historial   = citas.Where(c => c.Estado is ENTITY.Enums.EstadoCita.Cancelada
                                                     or ENTITY.Enums.EstadoCita.Completada)
        });
    }

    // POST /Cita/Cancelar/5
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Cancelar(int id)
    {
        try
        {
            await _citaService.CancelarCitaAsync(id, GetUsuarioId());
            TempData["Success"] = "Cita cancelada correctamente.";
        }
        catch (Exception ex) { TempData["Error"] = ex.Message; }
        return RedirectToAction(nameof(MisCitas));
    }

    private int GetUsuarioId()
        => int.Parse(User.FindFirst(PetFinderConstants.ClaimUserId)!.Value);
}
