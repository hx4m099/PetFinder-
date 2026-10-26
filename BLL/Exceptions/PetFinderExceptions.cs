namespace BLL.Exceptions;

/// <summary>
/// Excepción base para reglas de negocio de PetFinder.
/// Permite distinguir errores de dominio de errores técnicos.
/// </summary>
public class PetFinderException : Exception
{
    public PetFinderException(string message) : base(message) { }
    public PetFinderException(string message, Exception inner) : base(message, inner) { }
}

/// <summary>
/// Se lanza cuando una entidad solicitada no existe en el sistema.
/// </summary>
public class EntidadNoEncontradaException : PetFinderException
{
    public string NombreEntidad { get; }
    public object Id { get; }

    public EntidadNoEncontradaException(string nombreEntidad, object id)
        : base($"{nombreEntidad} con id '{id}' no fue encontrado.")
    {
        NombreEntidad = nombreEntidad;
        Id = id;
    }
}

/// <summary>
/// Se lanza cuando se intenta registrar un correo electrónico ya existente.
/// </summary>
public class EmailDuplicadoException : PetFinderException
{
    public EmailDuplicadoException(string email)
        : base($"El correo electrónico '{email}' ya está registrado.") { }
}

/// <summary>
/// Se lanza cuando las credenciales de inicio de sesión son incorrectas.
/// </summary>
public class CredencialesInvalidasException : PetFinderException
{
    public CredencialesInvalidasException()
        : base("Correo electrónico o contraseña incorrectos.") { }
}

/// <summary>
/// Se lanza cuando una cita se solapa con otra existente en el mismo refugio y horario.
/// </summary>
public class CitaDuplicadaException : PetFinderException
{
    public CitaDuplicadaException(DateTime fechaHora)
        : base($"Ya existe una cita agendada para el {fechaHora:dd/MM/yyyy HH:mm} en ese refugio.") { }
}

/// <summary>
/// Se lanza cuando una transición de estado de cita no está permitida.
/// </summary>
public class EstadoCitaInvalidoException : PetFinderException
{
    public EstadoCitaInvalidoException(string estadoActual, string estadoNuevo)
        : base($"No se puede cambiar el estado de '{estadoActual}' a '{estadoNuevo}'.") { }
}

/// <summary>
/// Se lanza cuando el usuario intenta realizar una acción fuera de su rol.
/// </summary>
public class AccesoDenegadoException : PetFinderException
{
    public AccesoDenegadoException(string detalle = "No tienes permisos para esta acción.")
        : base(detalle) { }
}

/// <summary>
/// Se lanza cuando la imagen supera el tamaño permitido o tiene un formato inválido.
/// </summary>
public class ImagenInvalidaException : PetFinderException
{
    public ImagenInvalidaException(string motivo)
        : base($"La imagen no es válida: {motivo}") { }
}
