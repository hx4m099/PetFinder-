using BLL.Dtos;
using BLL.Services;
using ENTITY.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PetFinder.Web.Helpers;
using PetFinder.Web.ViewModels;

namespace PetFinder.Web.Controllers;

/// <summary>
/// Panel del refugio: gestión de mascotas y confirmación de citas.
/// Solo accesible para usuarios con rol Refugio.
/// </summary>
[Authorize(Roles = PetFinderConstants.RolRefugio)]
public class RefugioController : Controller
{
    private readonly IAdopcionService _adopcionService;
    private readonly ICitaService     _citaService;
    private readonly IRefugioService  _refugioService;
    private readonly IWebHostEnvironment _env;
    private readonly ILogger<RefugioController> _logger;

    public RefugioController(
        IAdopcionService adopcionService,
        ICitaService citaService,
        IRefugioService refugioService,
        IWebHostEnvironment env,
        ILogger<RefugioController> logger)
    {
        _adopcionService = adopcionService;
        _citaService     = citaService;
        _refugioService  = refugioService;
        _env             = env;
        _logger          = logger;
    }

    // GET /Refugio/Panel
    [HttpGet]
    public async Task<IActionResult> Panel()
    {
        var refugioId = GetRefugioId();
        if (refugioId == 0) return RedirectToAction("Index", "Home");

        var refugio  = await _refugioService.ObtenerPorIdAsync(refugioId);
        var mascotas = await _adopcionService.ObtenerPorRefugioAsync(refugioId);
        var citas    = (await _citaService.ObtenerCitasRefugioAsync(refugioId)).ToList();

        return View(new PanelRefugioViewModel
        {
            Refugio          = refugio,
            Mascotas         = mascotas,
            CitasPendientes  = citas.Where(c => c.Estado == ENTITY.Enums.EstadoCita.Pendiente),
            CitasConfirmadas = citas.Where(c => c.Estado == ENTITY.Enums.EstadoCita.Confirmada)
        });
    }

    // GET /Refugio/CrearMascota
    [HttpGet]
    public IActionResult CrearMascota() => View(new MascotaFormViewModel());

    // POST /Refugio/CrearMascota
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> CrearMascota(MascotaFormViewModel vm)
    {
        if (!ModelState.IsValid) return View(vm);
        try
        {
            var fotoUrl = await ImagenHelper.GuardarImagenAsync(
                vm.Foto, _env, PetFinderConstants.RutaFotosMascotas);

            var dto = new MascotaFormDto(vm.Nombre, vm.Especie, vm.Raza,
                                         vm.EdadMeses, vm.Descripcion, vm.Vacunada, vm.Castrada);
            await _adopcionService.CrearMascotaAsync(dto, GetRefugioId(), fotoUrl);
            TempData["Success"] = "Mascota publicada correctamente.";
            return RedirectToAction(nameof(Panel));
        }
        catch (Exception ex) { ModelState.AddModelError(string.Empty, ex.Message); return View(vm); }
    }

    // GET /Refugio/EditarMascota/5
    [HttpGet]
    public async Task<IActionResult> EditarMascota(int id)
    {
        var m = await _adopcionService.ObtenerDetalleAsync(id);
        return View(new MascotaFormViewModel
        {
            Id = m.Id, Nombre = m.Nombre, Especie = m.Especie, Raza = m.Raza,
            EdadMeses = m.EdadMeses, Descripcion = m.Descripcion,
            Vacunada = m.Vacunada, Castrada = m.Castrada, FotoActualUrl = m.FotoUrl
        });
    }

    // POST /Refugio/EditarMascota/5
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> EditarMascota(int id, MascotaFormViewModel vm)
    {
        if (!ModelState.IsValid) return View(vm);
        try
        {
            var dto = new MascotaFormDto(vm.Nombre, vm.Especie, vm.Raza,
                                         vm.EdadMeses, vm.Descripcion, vm.Vacunada, vm.Castrada);
            await _adopcionService.ActualizarMascotaAsync(id, dto, GetRefugioId());
            TempData["Success"] = "Mascota actualizada.";
            return RedirectToAction(nameof(Panel));
        }
        catch (Exception ex) { ModelState.AddModelError(string.Empty, ex.Message); return View(vm); }
    }

    // POST /Refugio/EliminarMascota/5
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> EliminarMascota(int id)
    {
        await _adopcionService.EliminarMascotaAsync(id, GetRefugioId());
        TempData["Success"] = "Mascota eliminada del catálogo.";
        return RedirectToAction(nameof(Panel));
    }

    // POST /Refugio/ConfirmarCita/5
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> ConfirmarCita(int id, string? notas)
    {
        try
        {
            await _citaService.ConfirmarCitaAsync(id, GetUsuarioId(), notas);
            TempData["Success"] = "Cita confirmada.";
        }
        catch (Exception ex) { TempData["Error"] = ex.Message; }
        return RedirectToAction(nameof(Panel));
    }

    // POST /Refugio/CancelarCita/5
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> CancelarCita(int id)
    {
        try
        {
            await _citaService.CancelarCitaAsync(id, GetUsuarioId());
            TempData["Success"] = "Cita cancelada.";
        }
        catch (Exception ex) { TempData["Error"] = ex.Message; }
        return RedirectToAction(nameof(Panel));
    }

    private int GetUsuarioId()
        => int.Parse(User.FindFirst(PetFinderConstants.ClaimUserId)!.Value);

    private int GetRefugioId()
    {
        var claim = User.FindFirst(PetFinderConstants.ClaimRefugioId);
        return claim is null ? 0 : int.Parse(claim.Value);
    }
}
