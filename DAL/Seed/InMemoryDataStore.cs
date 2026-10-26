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

        // Vincular relaciones de navegación
        foreach (var refugio in Refugios)
        {
            refugio.Mascotas = Mascotas.Where(m => m.RefugioId == refugio.Id).ToList();
        }
    }

    // ── Seed: Usuarios ───────────────────────────────────────────────────────
    private static List<Usuario> SeedUsuarios() =>
    [
        new()
        {
            Id           = 1,
            Nombre       = "Administrador PetFinder",
            Email        = "admin@petfinder.mx",
            PasswordHash = "il2p8ZME8omrK6OnOkLEeK9ynd7L3UYXZizOaRLieFs=",   // Admin12345
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
            PasswordHash = "r0NqYXaNPxx70Hly06kZvskJVl3eQp/JWcRm7aTzD9Q=",  // Refugio12345
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
            PasswordHash = "RNJAz4LA4oAFTzVYE7BvixF06opjjd8JzXuMxmBSlAQ=",  // Adoptante12345
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
            PasswordHash = "RNJAz4LA4oAFTzVYE7BvixF06opjjd8JzXuMxmBSlAQ=",  // Adoptante12345
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
            PasswordHash = "r0NqYXaNPxx70Hly06kZvskJVl3eQp/JWcRm7aTzD9Q=",  // Refugio12345
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
            Id              = 1,
            Nombre          = "Patitas Felices (Ejemplo Ficticio)",
            Descripcion     = "Refugio dedicado al rescate, rehabilitación y adopción responsable de perros y gatos desde 2018.",
            Direccion       = "Av. Álvaro Obregón 168, Col. Roma Norte",
            Ciudad          = "Ciudad de México",
            Email           = "contacto@patitasfelices.ficticio.mx",
            Telefono        = "55-1234-5678",
            FotoUrl         = "https://images.unsplash.com/photo-1548767797-d8c844163c4c?w=500&auto=format&fit=crop&q=80",
            HorarioAtencion = "Lunes a Sábado 9:00 - 18:00",
            Latitud         = 19.4172,
            Longitud        = -99.1643,
            Activo          = true,
            UsuarioAdminId  = 2,
            FechaRegistro   = new DateTime(2024, 2, 15)
        },
        new()
        {
            Id              = 2,
            Nombre          = "Huellitas del Parque (Ejemplo Ficticio)",
            Descripcion     = "Albergue urbano con atención médica veterinaria y programas continuos de adopción amorosa.",
            Direccion       = "Calle Sonora 120, Col. Condesa",
            Ciudad          = "Ciudad de México",
            Email           = "contacto@huellitasparque.ficticio.mx",
            Telefono        = "55-2345-6789",
            FotoUrl         = "https://images.unsplash.com/photo-1583511655857-d19b40a7a54e?w=500&auto=format&fit=crop&q=80",
            HorarioAtencion = "Martes a Domingo 10:00 - 17:00",
            Latitud         = 19.4106,
            Longitud        = -99.1705,
            Activo          = true,
            UsuarioAdminId  = 5,
            FechaRegistro   = new DateTime(2024, 3, 20)
        },
        new()
        {
            Id              = 3,
            Nombre          = "Santuario San Roque (Ejemplo Ficticio)",
            Descripcion     = "Hogar temporal para rescate de animales en situación vulnerable y programas de socialización.",
            Direccion       = "Calle Londres 84, Col. Juárez",
            Ciudad          = "Ciudad de México",
            Email           = "info@sanroque.ficticio.mx",
            Telefono        = "55-3456-7890",
            FotoUrl         = "https://images.unsplash.com/photo-1601758228041-f3b2795255f1?w=500&auto=format&fit=crop&q=80",
            HorarioAtencion = "Lunes a Viernes 10:00 - 18:00",
            Latitud         = 19.4265,
            Longitud        = -99.1610,
            Activo          = true,
            UsuarioAdminId  = 2,
            FechaRegistro   = new DateTime(2024, 4, 10)
        },
        new()
        {
            Id              = 4,
            Nombre          = "Hogar Peludo Del Valle (Ejemplo Ficticio)",
            Descripcion     = "Centro de adopciones enfocado en cachorros y gatitos rescatados que buscan familias para siempre.",
            Direccion       = "Av. Coyoacán 820, Col. Del Valle",
            Ciudad          = "Ciudad de México",
            Email           = "adopciones@hogarpeludo.ficticio.mx",
            Telefono        = "55-4567-8901",
            FotoUrl         = "https://images.unsplash.com/photo-1543466835-00a7907e9de1?w=500&auto=format&fit=crop&q=80",
            HorarioAtencion = "Miércoles a Domingo 11:00 - 18:00",
            Latitud         = 19.3985,
            Longitud        = -99.1601,
            Activo          = true,
            UsuarioAdminId  = 5,
            FechaRegistro   = new DateTime(2024, 5, 5)
        },
        new()
        {
            Id              = 5,
            Nombre          = "Refugio Coyoacán Animal (Ejemplo Ficticio)",
            Descripcion     = "Espacio comunitario que promueve el rescate, la vacunación y la adopción responsable en el sur.",
            Direccion       = "Calle Francisco Sosa 210, Santa Catarina",
            Ciudad          = "Ciudad de México",
            Email           = "contacto@coyoacananimal.ficticio.mx",
            Telefono        = "55-5678-9012",
            FotoUrl         = "https://images.unsplash.com/photo-1587300003388-59208cc962cb?w=500&auto=format&fit=crop&q=80",
            HorarioAtencion = "Sábados y Domingos 10:00 - 16:00",
            Latitud         = 19.3502,
            Longitud        = -99.1624,
            Activo          = true,
            UsuarioAdminId  = 2,
            FechaRegistro   = new DateTime(2024, 6, 12)
        },
        new()
        {
            Id              = 6,
            Nombre          = "Albergue Esperanza (Ejemplo Ficticio)",
            Descripcion     = "Segundas oportunidades con amor para mascotas senior o con necesidades especiales.",
            Direccion       = "Av. Homero 430, Col. Polanco",
            Ciudad          = "Ciudad de México",
            Email           = "esperanza@refugios.ficticio.mx",
            Telefono        = "55-6789-0123",
            FotoUrl         = "https://images.unsplash.com/photo-1561037404-61cd46aa615b?w=500&auto=format&fit=crop&q=80",
            HorarioAtencion = "Lunes a Sábado 9:30 - 17:30",
            Latitud         = 19.4395,
            Longitud        = -99.1914,
            Activo          = true,
            UsuarioAdminId  = 5,
            FechaRegistro   = new DateTime(2024, 7, 1)
        }
    ];

    // ── Seed: Mascotas ───────────────────────────────────────────────────────
    private static List<Mascota> SeedMascotas() =>
    [
        new()
        {
            Id            = 1,
            Nombre        = "Fideo",
            Especie       = Especie.Perro,
            Raza          = "Labrador Mix",
            EdadMeses     = 18,
            Descripcion   = "Fideo es un perro juguetón y cariñoso. Le encanta correr y es muy sociable con niños.",
            FotoUrl       = "/uploads/mascotas/fideo.jpg",
            Disponible    = true,
            Vacunada      = true,
            Castrada      = true,
            RefugioId     = 1,
            FechaRegistro = new DateTime(2024, 6, 1)
        },
        new()
        {
            Id            = 2,
            Nombre        = "Luna",
            Especie       = Especie.Gato,
            Raza          = "Siamés Mix",
            EdadMeses     = 12,
            Descripcion   = "Luna es tranquila y afectuosa. Ideal para departamentos. Se lleva bien con otros gatos.",
            FotoUrl       = "/uploads/mascotas/luna.jpg",
            Disponible    = true,
            Vacunada      = true,
            Castrada      = true,
            RefugioId     = 1,
            FechaRegistro = new DateTime(2024, 7, 15)
        },
        new()
        {
            Id            = 3,
            Nombre        = "Thor",
            Especie       = Especie.Perro,
            Raza          = "Pastor Alemán Mix",
            EdadMeses     = 36,
            Descripcion   = "Thor es leal y protector. Necesita espacio y ejercicio diario. Muy obediente.",
            FotoUrl       = "/uploads/mascotas/thor.jpg",
            Disponible    = true,
            Vacunada      = true,
            Castrada      = false,
            RefugioId     = 1,
            FechaRegistro = new DateTime(2024, 5, 20)
        },
        new()
        {
            Id            = 4,
            Nombre        = "Mia",
            Especie       = Especie.Gato,
            Raza          = "Angora Mix",
            EdadMeses     = 6,
            Descripcion   = "Mia es una gatita bebé muy curiosa y juguetona. Busca su hogar para siempre.",
            FotoUrl       = "/uploads/mascotas/mia.jpg",
            Disponible    = true,
            Vacunada      = true,
            Castrada      = false,
            RefugioId     = 2,
            FechaRegistro = new DateTime(2024, 9, 1)
        },
        new()
        {
            Id            = 5,
            Nombre        = "Rocky",
            Especie       = Especie.Perro,
            Raza          = "Bulldog Francés Mix",
            EdadMeses     = 24,
            Descripcion   = "Rocky es tranquilo y le encanta dormir. Perfecto para espacios pequeños.",
            FotoUrl       = "/uploads/mascotas/rocky.jpg",
            Disponible    = true,
            Vacunada      = true,
            Castrada      = true,
            RefugioId     = 2,
            FechaRegistro = new DateTime(2024, 8, 10)
        },
        new()
        {
            Id            = 6,
            Nombre        = "Pipa",
            Especie       = Especie.Ave,
            Raza          = "Periquito",
            EdadMeses     = 4,
            Descripcion   = "Pipa es un periquito verde que imita sonidos y es muy sociable.",
            FotoUrl       = "/uploads/mascotas/pipa.jpg",
            Disponible    = true,
            Vacunada      = false,
            Castrada      = false,
            RefugioId     = 2,
            FechaRegistro = new DateTime(2024, 10, 1)
        },
        new()
        {
            Id            = 7,
            Nombre        = "Max",
            Especie       = Especie.Perro,
            Raza          = "Golden Retriever Mix",
            EdadMeses     = 14,
            Descripcion   = "Max es noble, juguetón y súper amigable con toda la familia.",
            FotoUrl       = "https://images.unsplash.com/photo-1552053831-71594a27632d?w=500&auto=format&fit=crop&q=80",
            Disponible    = true,
            Vacunada      = true,
            Castrada      = true,
            RefugioId     = 3,
            FechaRegistro = new DateTime(2024, 9, 15)
        },
        new()
        {
            Id            = 8,
            Nombre        = "Pelusa",
            Especie       = Especie.Gato,
            Raza          = "Persa Mix",
            EdadMeses     = 16,
            Descripcion   = "Pelusa es un gato ronroneador que adora los mimos en la tarde.",
            FotoUrl       = "https://images.unsplash.com/photo-1514888286974-6c03e2ca1dba?w=500&auto=format&fit=crop&q=80",
            Disponible    = true,
            Vacunada      = true,
            Castrada      = true,
            RefugioId     = 3,
            FechaRegistro = new DateTime(2024, 9, 20)
        },
        new()
        {
            Id            = 9,
            Nombre        = "Toby",
            Especie       = Especie.Perro,
            Raza          = "Beagle Mix",
            EdadMeses     = 10,
            Descripcion   = "Toby tiene un olfato agudo y orejotas adorables. Muy curioso y cariñoso.",
            FotoUrl       = "https://images.unsplash.com/photo-1537151625747-768eb6cf92b2?w=500&auto=format&fit=crop&q=80",
            Disponible    = true,
            Vacunada      = true,
            Castrada      = true,
            RefugioId     = 4,
            FechaRegistro = new DateTime(2024, 9, 28)
        },
        new()
        {
            Id            = 10,
            Nombre        = "Nala",
            Especie       = Especie.Gato,
            Raza          = "Europeo Común",
            EdadMeses     = 8,
            Descripcion   = "Nala tiene unos ojos verdes cautivadores. Muy ágil y sociable.",
            FotoUrl       = "https://images.unsplash.com/photo-1573865526739-10659fec78a5?w=500&auto=format&fit=crop&q=80",
            Disponible    = true,
            Vacunada      = true,
            Castrada      = true,
            RefugioId     = 4,
            FechaRegistro = new DateTime(2024, 10, 5)
        },
        new()
        {
            Id            = 11,
            Nombre        = "Canelo",
            Especie       = Especie.Perro,
            Raza          = "Mestizo",
            EdadMeses     = 20,
            Descripcion   = "Canelo es el compañero ideal para caminatas al aire libre. Muy obediente.",
            FotoUrl       = "https://images.unsplash.com/photo-1583511655857-d19b40a7a54e?w=500&auto=format&fit=crop&q=80",
            Disponible    = true,
            Vacunada      = true,
            Castrada      = true,
            RefugioId     = 5,
            FechaRegistro = new DateTime(2024, 10, 10)
        },
        new()
        {
            Id            = 12,
            Nombre        = "Mochi",
            Especie       = Especie.Gato,
            Raza          = "Blanco Doméstico",
            EdadMeses     = 11,
            Descripcion   = "Mochi es blanco como la nieve, muy dulce y pacífico.",
            FotoUrl       = "https://images.unsplash.com/photo-1533738363-b7f9aef128ce?w=500&auto=format&fit=crop&q=80",
            Disponible    = true,
            Vacunada      = true,
            Castrada      = true,
            RefugioId     = 6,
            FechaRegistro = new DateTime(2024, 10, 12)
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
