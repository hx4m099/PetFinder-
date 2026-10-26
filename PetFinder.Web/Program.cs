using BLL.Extensions;
using BLL.Exceptions;
using Microsoft.AspNetCore.Authentication.Cookies;
using PetFinder.Web.Options;

var builder = WebApplication.CreateBuilder(args);

// ── Servicios ─────────────────────────────────────────────────────────────────
builder.Services.AddControllersWithViews();

// Configuración de opciones
builder.Services.Configure<MapaOptions>(builder.Configuration.GetSection(MapaOptions.SectionName));

// Registra BLL completo (que internamente registra DAL + repos en memoria)
builder.Services.AddBll();

// Autenticación con cookies
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath        = "/Auth/Login";
        options.LogoutPath       = "/Auth/Logout";
        options.AccessDeniedPath = "/Auth/AccesoDenegado";
        options.ExpireTimeSpan   = TimeSpan.FromHours(8);
        options.SlidingExpiration = true;
        options.Cookie.HttpOnly  = true;
        options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
    });

builder.Services.AddHttpContextAccessor();

var app = builder.Build();

// ── Middleware pipeline ───────────────────────────────────────────────────────
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

// Middleware centralizado para excepciones de negocio
app.Use(async (context, next) =>
{
    try { await next(); }
    catch (AccesoDenegadoException ex)
    {
        context.Response.Redirect($"/Auth/AccesoDenegado?msg={Uri.EscapeDataString(ex.Message)}");
    }
    catch (EntidadNoEncontradaException)
    {
        context.Response.StatusCode = 404;
        context.Response.Redirect("/Home/NotFound");
    }
});

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
