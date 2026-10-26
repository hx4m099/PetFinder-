using ENTITY.Entities;
using ENTITY.Enums;

namespace DAL.Seed;

/// <summary>
/// Almacén de datos en memoria compartido como Singleton durante el desarrollo.
/// Contiene datos semilla (seed) para todas las entidades principales.
/// Se reemplazará completamente por el DbContext de EF Core al conectar Oracle.
/// </summary>
public class InMemoryDataStore
{
    public List<Usuario>       Usuarios { get; }
    public List<Refugio>       Refugios { get; }
    public List<Mascota>       Mascotas { get; }
    public List<ReporteMascota> Reportes { get; }
    public List<Cita>          Citas    { get; }

    public InMemoryDataStore()
    {
        Usuarios = SeedUsuarios();
        Refugios = SeedRefugios();
        Mascotas = SeedMascotas();
        Reportes = SeedReportes();
        Citas    = SeedCitas();
    }

    // ── Seed: Usuarios ───────────────────────────────────────────────────────
    private static List<Usuario> SeedUsuarios() =>
    [
        new()
        {
            Id           = 1,
            Nombre       = "Administrador PetFinder",
            Email        = "admin@petfinder.mx",
            PasswordHash = "$2a$11$admin_hash_placeholder",   // bcrypt simulado
            Rol          = Rol.Administrador,
            Ciudad       = "Ciudad de México",
            Activo       = true,
            FechaRegistro = new DateTime(2024, 1, 1)
        },
        new()
        {
            Id           = 2,
            Nombre       = "Refugio Patitas Felices",
            Email        = "refugio@patitasfelices.mx",
            PasswordHash = "$2a$11$refugio_hash_placeholder",
            Rol          = Rol.Refugio,
            Ciudad       = "Guadalajara",
            Telefono     = "3312345678",
            Activo       = true,
            FechaRegistro = new DateTime(2024, 2, 15)
        },
        new()
        {
            Id           = 3,
            Nombre       = "Ana García",
            Email        = "ana@gmail.com",
            PasswordHash = "$2a$11$adoptante_hash_placeholder",
            Rol          = Rol.Adoptante,
            Ciudad       = "Guadalajara",
            Telefono     = "3398765432",
            Activo       = true,
            FechaRegistro = new DateTime(2024, 3, 10)
        },
        new()
        {
            Id           = 4,
            Nombre       = "Carlos López",
            Email        = "carlos@gmail.com",
            PasswordHash = "$2a$11$adoptante2_hash_placeholder",
            Rol          = Rol.Adoptante,
            Ciudad       = "Monterrey",
            Activo       = true,
            FechaRegistro = new DateTime(2024, 4, 5)
        },
        new()
        {
            Id           = 5,
            Nombre       = "Refugio Huellitas CDMX",
            Email        = "refugio@huellitas.mx",
            PasswordHash = "$2a$11$refugio2_hash_placeholder",
            Rol          = Rol.Refugio,
            Ciudad       = "Ciudad de México",
            Telefono     = "5512345678",
            Activo       = true,
            FechaRegistro = new DateTime(2024, 3, 20)
        }
    ];

    // ── Seed: Refugios ───────────────────────────────────────────────────────
    private static List<Refugio> SeedRefugios() =>
    [
        new()
        {
            Id             = 1,
            Nombre         = "Patitas Felices",
            Descripcion    = "Refugio dedicado al rescate y adopción responsable de perros y gatos desde 2010.",
            Direccion      = "Av. Chapultepec 1234, Col. Americana",
            Ciudad         = "Guadalajara",
            Email          = "contacto@patitasfelices.mx",
            Telefono       = "3312345678",
            HorarioAtencion = "Lunes a Sábado 9:00 - 18:00",
            Activo         = true,
            UsuarioAdminId = 2,
            FechaRegistro  = new DateTime(2024, 2, 15)
        },
        new()
        {
            Id             = 2,
            Nombre         = "Huellitas CDMX",
            Descripcion    = "Refugio urbano en Ciudad de México con más de 200 animales rescatados.",
            Direccion      = "Calle Sonora 456, Col. Hipódromo",
            Ciudad         = "Ciudad de México",
            Email          = "contacto@huellitas.mx",
            Telefono       = "5512345678",
            HorarioAtencion = "Martes a Domingo 10:00 - 17:00",
            Activo         = true,
            UsuarioAdminId = 5,
            FechaRegistro  = new DateTime(2024, 3, 20)
        }
    ];

    // ── Seed: Mascotas ───────────────────────────────────────────────────────
    private static List<Mascota> SeedMascotas() =>
    [
        new()
        {
            Id          = 1,
            Nombre      = "Fideo",
            Especie     = Especie.Perro,
            Raza        = "Labrador Mix",
            EdadMeses   = 18,
            Descripcion = "Fideo es un perro juguetón y cariñoso. Le encanta correr y es muy sociable con niños.",
            FotoUrl     = "/uploads/mascotas/fideo.jpg",
            Disponible  = true,
            Vacunada    = true,
            Castrada    = true,
            RefugioId   = 1,
            FechaRegistro = new DateTime(2024, 6, 1)
        },
        new()
        {
            Id          = 2,
            Nombre      = "Luna",
            Especie     = Especie.Gato,
            Raza        = "Siamés Mix",
            EdadMeses   = 12,
            Descripcion = "Luna es tranquila y afectuosa. Ideal para departamentos. Lleva bien con otros gatos.",
            FotoUrl     = "/uploads/mascotas/luna.jpg",
            Disponible  = true,
            Vacunada    = true,
            Castrada    = true,
            RefugioId   = 1,
            FechaRegistro = new DateTime(2024, 7, 15)
        },
        new()
        {
            Id          = 3,
            Nombre      = "Thor",
            Especie     = Especie.Perro,
            Raza        = "Pastor Alemán Mix",
            EdadMeses   = 36,
            Descripcion = "Thor es leal y protector. Necesita espacio y ejercicio diario. Sabe sentarse y dar la pata.",
            FotoUrl     = "/uploads/mascotas/thor.jpg",
            Disponible  = true,
            Vacunada    = true,
            Castrada    = false,
            RefugioId   = 1,
            FechaRegistro = new DateTime(2024, 5, 20)
        },
        new()
        {
            Id          = 4,
            Nombre      = "Mia",
            Especie     = Especie.Gato,
            Raza        = "Angora Mix",
            EdadMeses   = 6,
            Descripcion = "Mia es una gatita bebé muy curiosa y juguetona. Busca su hogar para siempre.",
            FotoUrl     = "/uploads/mascotas/mia.jpg",
            Disponible  = true,
            Vacunada    = true,
            Castrada    = false,
            RefugioId   = 2,
            FechaRegistro = new DateTime(2024, 9, 1)
        },
        new()
        {
            Id          = 5,
            Nombre      = "Rocky",
            Especie     = Especie.Perro,
            Raza        = "Bulldog Francés Mix",
            EdadMeses   = 24,
            Descripcion = "Rocky es tranquilo y le encanta dormir. Perfecto para espacios pequeños.",
            FotoUrl     = "/uploads/mascotas/rocky.jpg",
            Disponible  = true,
            Vacunada    = true,
            Castrada    = true,
            RefugioId   = 2,
            FechaRegistro = new DateTime(2024, 8, 10)
        },
        new()
        {
            Id          = 6,
            Nombre      = "Pipa",
            Especie     = Especie.Ave,
            Raza        = "Periquito",
            EdadMeses   = 4,
            Descripcion = "Pipa es un periquito verde que imita sonidos y es muy sociable.",
            FotoUrl     = "/uploads/mascotas/pipa.jpg",
            Disponible  = true,
            Vacunada    = false,
            Castrada    = false,
            RefugioId   = 2,
            FechaRegistro = new DateTime(2024, 10, 1)
        }
    ];

    // ── Seed: Reportes de mascotas ───────────────────────────────────────────
    private static List<ReporteMascota> SeedReportes() =>
    [
        new()
        {
            Id           = 1,
            TipoReporte  = TipoReporte.Perdida,
            Especie      = Especie.Perro,
            NombreMascota = "Firulais",
            Raza         = "Labrador dorado",
            ColorPelaje  = "Dorado",
            Ciudad       = "Guadalajara",
            Descripcion  = "Se perdió el sábado por la tarde cerca del parque Ávila Camacho. Lleva collar azul.",
            FotoUrl      = "/uploads/reportes/firulais.jpg",
            FechaReporte = DateTime.UtcNow.AddDays(-5),
            Activo       = true,
            UsuarioId    = 3
        },
        new()
        {
            Id           = 2,
            TipoReporte  = TipoReporte.Encontrada,
            Especie      = Especie.Perro,
            NombreMascota = null,
            Raza         = "Labrador mix",
            ColorPelaje  = "Amarillo claro",
            Ciudad       = "Guadalajara",
            Descripcion  = "Encontré un perro grande de color amarillo en la colonia Chapalita. Sin collar.",
            FotoUrl      = "/uploads/reportes/perro_encontrado.jpg",
            FechaReporte = DateTime.UtcNow.AddDays(-3),
            Activo       = true,
            UsuarioId    = 4
        },
        new()
        {
            Id           = 3,
            TipoReporte  = TipoReporte.Perdida,
            Especie      = Especie.Gato,
            NombreMascota = "Nube",
            Raza         = "Angora blanco",
            ColorPelaje  = "Blanco",
            Ciudad       = "Ciudad de México",
            Descripcion  = "Gata blanca de pelo largo, muy cariñosa. Desapareció del departamento.",
            FotoUrl      = "/uploads/reportes/nube.jpg",
            FechaReporte = DateTime.UtcNow.AddDays(-10),
            Activo       = true,
            UsuarioId    = 3
        }
    ];

    // ── Seed: Citas ───────────────────────────────────────────────────────────
    private static List<Cita> SeedCitas() =>
    [
        new()
        {
            Id             = 1,
            FechaHora      = DateTime.UtcNow.AddDays(3).Date.AddHours(10),
            Estado         = EstadoCita.Pendiente,
            NotasAdoptante = "Voy con mi pareja para conocer a Fideo.",
            AdoptanteId    = 3,
            MascotaId      = 1,
            RefugioId      = 1,
            FechaCreacion  = DateTime.UtcNow.AddDays(-1)
        },
        new()
        {
            Id             = 2,
            FechaHora      = DateTime.UtcNow.AddDays(5).Date.AddHours(11),
            Estado         = EstadoCita.Confirmada,
            NotasAdoptante = "Tengo experiencia con gatos.",
            NotasRefugio   = "Confirmado. Pregunte por Ana en recepción.",
            AdoptanteId    = 4,
            MascotaId      = 4,
            RefugioId      = 2,
            FechaCreacion  = DateTime.UtcNow.AddDays(-2)
        }
    ];
}
