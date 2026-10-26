using BLL.Dtos;
using ENTITY.Enums;
using System.ComponentModel.DataAnnotations;

namespace PetFinder.Web.ViewModels;

// ── Auth ViewModels ───────────────────────────────────────────────────────────

public class LoginViewModel
{
    [Required(ErrorMessage = "El correo es obligatorio")]
    [EmailAddress(ErrorMessage = "Formato de correo inválido")]
    [Display(Name = "Correo electrónico")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "La contraseña es obligatoria")]
    [DataType(DataType.Password)]
    [Display(Name = "Contraseña")]
    public string Password { get; set; } = string.Empty;

    public string? ReturnUrl { get; set; }
}

public class RegistroViewModel
{
    [Required(ErrorMessage = "El nombre es obligatorio")]
    [MaxLength(150)]
    [Display(Name = "Nombre completo")]
    public string Nombre { get; set; } = string.Empty;

    [Required(ErrorMessage = "El correo es obligatorio")]
    [EmailAddress(ErrorMessage = "Formato inválido")]
    [Display(Name = "Correo electrónico")]
    public string Email { get; set; } = string.Empty;

    [Required, MinLength(8, ErrorMessage = "Mínimo 8 caracteres")]
    [DataType(DataType.Password)]
    [Display(Name = "Contraseña")]
    public string Password { get; set; } = string.Empty;

    [Required]
    [DataType(DataType.Password)]
    [Compare("Password", ErrorMessage = "Las contraseñas no coinciden")]
    [Display(Name = "Confirmar contraseña")]
    public string ConfirmPassword { get; set; } = string.Empty;

    [Display(Name = "Tipo de cuenta")]
    public Rol Rol { get; set; } = Rol.Adoptante;

    [Phone, Display(Name = "Teléfono")]
    public string? Telefono { get; set; }

    [MaxLength(100), Display(Name = "Ciudad")]
    public string? Ciudad { get; set; }
}

// ── Reporte ViewModels ────────────────────────────────────────────────────────

public class ReporteFormViewModel
{
    [Required(ErrorMessage = "Selecciona el tipo de reporte")]
    [Display(Name = "Tipo de reporte")]
    public TipoReporte TipoReporte { get; set; }

    [Required(ErrorMessage = "Selecciona la especie")]
    [Display(Name = "Especie")]
    public Especie Especie { get; set; }

    [MaxLength(100), Display(Name = "Nombre de la mascota")]
    public string? NombreMascota { get; set; }

    [MaxLength(100), Display(Name = "Raza")]
    public string? Raza { get; set; }

    [MaxLength(100), Display(Name = "Color del pelaje")]
    public string? ColorPelaje { get; set; }

    [Required(ErrorMessage = "La ciudad es obligatoria")]
    [MaxLength(100), Display(Name = "Ciudad")]
    public string Ciudad { get; set; } = string.Empty;

    [MaxLength(1000), Display(Name = "Descripción adicional")]
    public string? Descripcion { get; set; }

    [Display(Name = "Foto de la mascota")]
    public IFormFile? Foto { get; set; }
}

public class CoincidenciasViewModel
{
    public ReporteResumenDto Reporte { get; set; } = null!;
    public IEnumerable<CoincidenciaDto> Coincidencias { get; set; } = [];
    public bool Buscando { get; set; }
}

// ── Adopción ViewModels ───────────────────────────────────────────────────────

public class CatalogoViewModel
{
    public IEnumerable<MascotaResumenDto> Mascotas { get; set; } = [];
    public IEnumerable<RefugioDto> Refugios { get; set; } = [];
    public int? RefugioFiltro { get; set; }
    public Especie? EspecieFiltro { get; set; }
    public int? EdadMaxFiltro { get; set; }
}

public class MascotaDetalleViewModel
{
    public MascotaDetalleDto Mascota { get; set; } = null!;
    public CitaFormViewModel FormCita { get; set; } = new();
}

// ── Cita ViewModels ────────────────────────────────────────────────────────────

public class CitaFormViewModel
{
    [Required]
    public int MascotaId { get; set; }

    [Required]
    public int RefugioId { get; set; }

    [Required(ErrorMessage = "Selecciona fecha y hora")]
    [Display(Name = "Fecha y hora")]
    [DataType(DataType.DateTime)]
    public DateTime FechaHora { get; set; } = DateTime.Now.AddDays(1).Date.AddHours(10);

    [MaxLength(500), Display(Name = "Notas adicionales")]
    public string? NotasAdoptante { get; set; }
}

public class MisCitasViewModel
{
    public IEnumerable<CitaDto> Pendientes  { get; set; } = [];
    public IEnumerable<CitaDto> Confirmadas { get; set; } = [];
    public IEnumerable<CitaDto> Historial   { get; set; } = [];
}

// ── Panel Refugio ViewModels ──────────────────────────────────────────────────

public class PanelRefugioViewModel
{
    public RefugioDto Refugio { get; set; } = null!;
    public IEnumerable<MascotaResumenDto> Mascotas { get; set; } = [];
    public IEnumerable<CitaDto> CitasPendientes { get; set; } = [];
    public IEnumerable<CitaDto> CitasConfirmadas { get; set; } = [];
}

public class MascotaFormViewModel
{
    public int? Id { get; set; }

    [Required(ErrorMessage = "El nombre es obligatorio")]
    [MaxLength(100), Display(Name = "Nombre")]
    public string Nombre { get; set; } = string.Empty;

    [Required, Display(Name = "Especie")]
    public Especie Especie { get; set; }

    [MaxLength(100), Display(Name = "Raza")]
    public string? Raza { get; set; }

    [Range(0, 360), Display(Name = "Edad (meses)")]
    public int EdadMeses { get; set; }

    [MaxLength(1000), Display(Name = "Descripción")]
    public string? Descripcion { get; set; }

    [Display(Name = "Vacunada")]
    public bool Vacunada { get; set; }

    [Display(Name = "Castrada/Esterilizada")]
    public bool Castrada { get; set; }

    [Display(Name = "Foto de la mascota")]
    public IFormFile? Foto { get; set; }

    public string? FotoActualUrl { get; set; }
}

// ── Admin ViewModels ───────────────────────────────────────────────────────────

public class PanelAdminViewModel
{
    public int TotalUsuarios  { get; set; }
    public int TotalRefugios  { get; set; }
    public int TotalMascotas  { get; set; }
    public int TotalReportes  { get; set; }
    public IEnumerable<UsuarioResumenDto> UltimosUsuarios { get; set; } = [];
    public IEnumerable<ReporteResumenDto> UltimosReportes { get; set; } = [];
}
