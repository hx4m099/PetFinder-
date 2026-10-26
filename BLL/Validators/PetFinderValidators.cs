using BLL.Dtos;
using ENTITY.Constants;
using FluentValidation;

namespace BLL.Validators;

/// <summary>
/// Validador de FluentValidation para el registro de nuevos usuarios.
/// </summary>
public class RegistroDtoValidator : AbstractValidator<RegistroDto>
{
    public RegistroDtoValidator()
    {
        RuleFor(x => x.Nombre)
            .NotEmpty().WithMessage("El nombre es obligatorio.")
            .MaximumLength(150).WithMessage("El nombre no puede exceder 150 caracteres.");

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("El correo electrónico es obligatorio.")
            .EmailAddress().WithMessage("El formato del correo electrónico no es válido.")
            .MaximumLength(200);

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("La contraseña es obligatoria.")
            .MinimumLength(8).WithMessage("La contraseña debe tener al menos 8 caracteres.")
            .Matches(@"[A-Z]").WithMessage("Debe contener al menos una letra mayúscula.")
            .Matches(@"[0-9]").WithMessage("Debe contener al menos un número.");

        RuleFor(x => x.ConfirmPassword)
            .Equal(x => x.Password).WithMessage("Las contraseñas no coinciden.");

        RuleFor(x => x.Telefono)
            .Matches(@"^\+?[\d\s\-]{7,20}$").WithMessage("El formato del teléfono no es válido.")
            .When(x => !string.IsNullOrWhiteSpace(x.Telefono));

        RuleFor(x => x.Ciudad)
            .MaximumLength(100)
            .When(x => !string.IsNullOrWhiteSpace(x.Ciudad));
    }
}

/// <summary>
/// Validador para el inicio de sesión.
/// </summary>
public class LoginDtoValidator : AbstractValidator<LoginDto>
{
    public LoginDtoValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("El correo electrónico es obligatorio.")
            .EmailAddress().WithMessage("Formato de correo no válido.");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("La contraseña es obligatoria.");
    }
}

/// <summary>
/// Validador para la creación / edición de una mascota.
/// </summary>
public class MascotaFormDtoValidator : AbstractValidator<MascotaFormDto>
{
    public MascotaFormDtoValidator()
    {
        RuleFor(x => x.Nombre)
            .NotEmpty().WithMessage("El nombre de la mascota es obligatorio.")
            .MaximumLength(100);

        RuleFor(x => x.EdadMeses)
            .InclusiveBetween(0, 360)
            .WithMessage("La edad debe estar entre 0 y 360 meses (30 años).");

        RuleFor(x => x.Descripcion)
            .MaximumLength(1000)
            .When(x => !string.IsNullOrWhiteSpace(x.Descripcion));
    }
}

/// <summary>
/// Validador para la creación de un reporte de mascota.
/// </summary>
public class ReporteFormDtoValidator : AbstractValidator<ReporteFormDto>
{
    public ReporteFormDtoValidator()
    {
        RuleFor(x => x.Ciudad)
            .NotEmpty().WithMessage("La ciudad es obligatoria para el filtrado de coincidencias.")
            .MaximumLength(100);

        RuleFor(x => x.NombreMascota)
            .MaximumLength(100)
            .When(x => !string.IsNullOrWhiteSpace(x.NombreMascota));

        RuleFor(x => x.Descripcion)
            .MaximumLength(1000)
            .When(x => !string.IsNullOrWhiteSpace(x.Descripcion));

        RuleFor(x => x.ColorPelaje)
            .MaximumLength(100)
            .When(x => !string.IsNullOrWhiteSpace(x.ColorPelaje));
    }
}

/// <summary>
/// Validador para agendar una cita.
/// </summary>
public class CitaFormDtoValidator : AbstractValidator<CitaFormDto>
{
    public CitaFormDtoValidator()
    {
        RuleFor(x => x.MascotaId)
            .GreaterThan(0).WithMessage("Debe seleccionar una mascota válida.");

        RuleFor(x => x.RefugioId)
            .GreaterThan(0).WithMessage("Debe seleccionar un refugio válido.");

        RuleFor(x => x.FechaHora)
            .GreaterThan(DateTime.UtcNow)
            .WithMessage("La fecha de la cita debe ser futura.")
            .LessThan(DateTime.UtcNow.AddMonths(3))
            .WithMessage("No se pueden agendar citas con más de 3 meses de anticipación.");

        RuleFor(x => x.NotasAdoptante)
            .MaximumLength(500)
            .When(x => !string.IsNullOrWhiteSpace(x.NotasAdoptante));
    }
}

/// <summary>
/// Validador para crear o editar un refugio.
/// </summary>
public class RefugioFormDtoValidator : AbstractValidator<RefugioFormDto>
{
    public RefugioFormDtoValidator()
    {
        RuleFor(x => x.Nombre)
            .NotEmpty().WithMessage("El nombre del refugio es obligatorio.")
            .MaximumLength(200);

        RuleFor(x => x.Ciudad)
            .NotEmpty().WithMessage("La ciudad del refugio es obligatoria.")
            .MaximumLength(100);

        RuleFor(x => x.Email)
            .EmailAddress().WithMessage("El formato del correo no es válido.")
            .When(x => !string.IsNullOrWhiteSpace(x.Email));

        RuleFor(x => x.Descripcion)
            .MaximumLength(1000)
            .When(x => !string.IsNullOrWhiteSpace(x.Descripcion));
    }
}
