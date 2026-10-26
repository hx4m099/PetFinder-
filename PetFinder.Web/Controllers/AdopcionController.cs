using BLL.Dtos;
using BLL.Services;
using ENTITY.Constants;
using ENTITY.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PetFinder.Web.ViewModels;

namespace PetFinder.Web.Controllers;

/// <summary>
/// Catálogo de mascotas en adopción y agendamiento de citas.
/// </summary>
public class AdopcionController : Controller
{
    private readonly IAdopcionService _adopcionService;
    private readonly ICitaService     _citaService;
    private readonly IRefugioService  _refugioService;
    private readonly ILogger<AdopcionController> _logger;

    public AdopcionController(
        IAdopcionService adopcionService,
        ICitaService citaService,
        IRefugioService refugioService,
        ILogger<AdopcionController> logger)
    {
        _adopcionService = adopcionService;
        _citaService     = citaService;
        _refugioService  = refugioService;
        _logger          = logger;
    }

    // GET /Adopcion o /Adopcion/Catalogo
    [HttpGet]
    public async Task<IActionResult> Catalogo(
        int? refugioId, Especie? especie, int? edadMax)
    {
        var mascotas = await _adopcionService.ObtenerCatalogoAsync(refugioId, especie, edadMax);
        var refugios = await _refugioService.ObtenerTodosAsync();

        return View(new CatalogoViewModel
        {
            Mascotas      = mascotas,
            Refugios      = refugios,
            RefugioFiltro = refugioId,
            EspecieFiltro = especie,
            EdadMaxFiltro = edadMax
        });
    }

    // GET /Adopcion/Detalle/5
    [HttpGet]
    public async Task<IActionResult> Detalle(int id)
    {
        var mascota = await _adopcionService.ObtenerDetalleAsync(id);
        return View(new MascotaDetalleViewModel
        {
            Mascota  = mascota,
            FormCita = new CitaFormViewModel
            {
                MascotaId = mascota.Id,
                RefugioId = mascota.RefugioId
            }
        });
    }

    // POST /Adopcion/AgendarCita
    [HttpPost, ValidateAntiForgeryToken, Authorize]
    public async Task<IActionResult> AgendarCita(CitaFormViewModel vm)
    {
        if (!ModelState.IsValid)
        {
            var mascota = await _adopcionService.ObtenerDetalleAsync(vm.MascotaId);
            return View("Detalle", new MascotaDetalleViewModel { Mascota = mascota, FormCita = vm });
        }

        try
        {
            var dto  = new CitaFormDto(vm.MascotaId, vm.RefugioId, vm.FechaHora, vm.NotasAdoptante);
            var cita = await _citaService.AgendarCitaAsync(dto, GetUsuarioId());
            TempData["Success"] = $"¡Cita agendada para el {cita.FechaHora:dd/MM/yyyy HH:mm}!";
            return RedirectToAction("MisCitas", "Cita");
        }
        catch (Exception ex)
        {
            TempData["Error"] = ex.Message;
            return RedirectToAction(nameof(Detalle), new { id = vm.MascotaId });
        }
    }

    private int GetUsuarioId()
        => int.Parse(User.FindFirst(PetFinderConstants.ClaimUserId)!.Value);
}
