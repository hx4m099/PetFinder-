namespace ENTITY.Enums;

/// <summary>
/// Estado actual de una cita de adopción.
/// </summary>
public enum EstadoCita
{
    /// <summary>La cita fue agendada y está pendiente de confirmación.</summary>
    Pendiente = 1,

    /// <summary>La cita fue confirmada por el refugio.</summary>
    Confirmada = 2,

    /// <summary>La cita fue cancelada (por el adoptante o el refugio).</summary>
    Cancelada = 3,

    /// <summary>La cita ya fue realizada exitosamente.</summary>
    Completada = 4
}
