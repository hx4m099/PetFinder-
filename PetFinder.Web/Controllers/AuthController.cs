using BLL.Dtos;
using BLL.Services;
using ENTITY.Constants;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using PetFinder.Web.ViewModels;
using System.Security.Claims;

namespace PetFinder.Web.Controllers;

/// <summary>
/// Gestiona el registro, inicio y cierre de sesión.
/// La autenticación es simulada con cookies; sin base de datos real.
/// </summary>
public class AuthController : Controller
{
    private readonly IAuthService _authService;
    private readonly ILogger<AuthController> _logger;

    public AuthController(IAuthService authService, ILogger<AuthController> logger)
    {
        _authService = authService;
        _logger      = logger;
    }

    // GET /Auth/Login
    [HttpGet]
    public IActionResult Login(string? returnUrl)
    {
        if (User.Identity?.IsAuthenticated == true)
            return RedirectToAction("Index", "Home");

        return View(new LoginViewModel { ReturnUrl = returnUrl });
    }

    // POST /Auth/Login
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel vm)
    {
        if (!ModelState.IsValid) return View(vm);

        try
        {
            var sesion = await _authService.LoginAsync(new LoginDto(vm.Email, vm.Password));
            await SignInAsync(sesion);
            _logger.LogInformation("Login exitoso: {Email}", vm.Email);
            return RedirectToLocal(vm.ReturnUrl);
        }
        catch (BLL.Exceptions.CredencialesInvalidasException ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            return View(vm);
        }
    }

    // GET /Auth/Registro
    [HttpGet]
    public IActionResult Registro() => View(new RegistroViewModel());

    // POST /Auth/Registro
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Registro(RegistroViewModel vm)
    {
        if (!ModelState.IsValid) return View(vm);

        try
        {
            var dto    = new RegistroDto(vm.Nombre, vm.Email, vm.Password, vm.ConfirmPassword,
                                         vm.Rol, vm.Telefono, vm.Ciudad);
            var sesion = await _authService.RegistrarAsync(dto);
            await SignInAsync(sesion);
            TempData["Success"] = "¡Bienvenido a PetFinder! Tu cuenta ha sido creada.";
            return RedirectToAction("Index", "Home");
        }
        catch (BLL.Exceptions.EmailDuplicadoException ex)
        {
            ModelState.AddModelError("Email", ex.Message);
            return View(vm);
        }
    }

    // POST /Auth/Logout
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return RedirectToAction("Index", "Home");
    }

    // GET /Auth/AccesoDenegado
    public IActionResult AccesoDenegado(string? msg)
    {
        ViewBag.Mensaje = msg ?? PetFinderConstants.ErrorAccesoDenegado;
        return View();
    }

    // ── Helpers privados ──────────────────────────────────────────────────────

    private async Task SignInAsync(UsuarioSesionDto sesion)
    {
        var claims = new List<Claim>
        {
            new(PetFinderConstants.ClaimUserId,  sesion.Id.ToString()),
            new(PetFinderConstants.ClaimNombre,  sesion.Nombre),
            new(PetFinderConstants.ClaimRol,     sesion.Rol.ToString()),
            new(ClaimTypes.Name,                 sesion.Nombre),
            new(ClaimTypes.Email,                sesion.Email),
            new(ClaimTypes.Role,                 sesion.Rol.ToString())
        };

        if (sesion.RefugioId.HasValue)
            claims.Add(new Claim(PetFinderConstants.ClaimRefugioId, sesion.RefugioId.Value.ToString()));

        var identity  = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        var principal = new ClaimsPrincipal(identity);

        await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);
    }

    private IActionResult RedirectToLocal(string? returnUrl)
        => Url.IsLocalUrl(returnUrl) ? Redirect(returnUrl) : RedirectToAction("Index", "Home");
}
