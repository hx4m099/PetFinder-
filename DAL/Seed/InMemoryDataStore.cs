// DAL/Seed/InMemoryDataStore.cs
using ENTITY.Entities;
using ENTITY.Enums;

namespace DAL.Seed;

/// <summary>
/// Almacén de datos en memoria compartido como Singleton durante el desarrollo.
/// Contiene datos semilla (seed) para Valledupar, Cesar, Colombia.
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
            Email        = "admin@petfinder.co",
            PasswordHash = "il2p8ZME8omrK6OnOkLEeK9ynd7L3UYXZizOaRLieFs=",   // Admin12345
            Rol          = Rol.Administrador,
            Ciudad       = "Valledupar, Cesar",
            Telefono     = "315-700-0001",
            Activo       = true,
            FechaRegistro = new DateTime(2024, 1, 1)
        },
        new()
        {
            Id           = 2,
            Nombre       = "Fundación Huellitas del Cesar",
            Email        = "refugio@huellitasdelcesar.org",
            PasswordHash = "r0NqYXaNPxx70Hly06kZvskJVl3eQp/JWcRm7aTzD9Q=",  // Refugio12345
            Rol          = Rol.Refugio,
            Ciudad       = "Valledupar, Cesar",
            Telefono     = "315-782-9012",
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
            Ciudad       = "Valledupar, Cesar",
            Telefono     = "320-456-7890",
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
            Ciudad       = "Valledupar, Cesar",
            Telefono     = "311-987-6543",
            Activo       = true,
            FechaRegistro = new DateTime(2024, 4, 5)
        },
        new()
        {
            Id           = 5,
            Nombre       = "Refugio Guatapurí Animal",
            Email        = "refugio@guatapuri.org",
            PasswordHash = "r0NqYXaNPxx70Hly06kZvskJVl3eQp/JWcRm7aTzD9Q=",  // Refugio12345
            Rol          = Rol.Refugio,
            Ciudad       = "Valledupar, Cesar",
            Telefono     = "318-654-3210",
            Activo       = true,
            FechaRegistro = new DateTime(2024, 3, 20)
        }
    ];

    // ── Seed: Refugios en Valledupar, Cesar ──────────────────────────────────
    private static List<Refugio> SeedRefugios() =>
    [
        new()
        {
            Id              = 1,
            Nombre          = "Fundación Huellitas del Cesar",
            Descripcion     = "Refugio dedicado al rescate, esterilización y adopción responsable de animales en situación de calle en Valledupar.",
            Direccion       = "Cra 9 # 13B-45, Barrio Novalito",
            Ciudad          = "Valledupar, Cesar",
            Email           = "contacto@huellitasdelcesar.org",
            Telefono        = "315-782-9012",
            FotoUrl         = "https://images.unsplash.com/photo-1548767797-d8c844163c4c?w=600&auto=format&fit=crop&q=80",
            HorarioAtencion = "Lunes a Sábado 8:00 - 17:00",
            Latitud         = 10.4712,
            Longitud        = -73.2495,
            Activo          = true,
            UsuarioAdminId  = 2,
            FechaRegistro   = new DateTime(2024, 2, 15)
        },
        new()
        {
            Id              = 2,
            Nombre          = "Refugio Guatapurí Animal",
            Descripcion     = "Albergue con atención médico-veterinaria y campañas educativas para la tenencia responsable en el norte de Valledupar.",
            Direccion       = "Av. Simón Bolívar con Calle 12, Barrio Los Cortijos",
            Ciudad          = "Valledupar, Cesar",
            Email           = "contacto@guatapurianimal.org",
            Telefono        = "318-654-3210",
            FotoUrl         = "https://images.unsplash.com/photo-1601758228041-f3b2795255f1?w=600&auto=format&fit=crop&q=80",
            HorarioAtencion = "Martes a Domingo 9:00 - 16:30",
            Latitud         = 10.4665,
            Longitud        = -73.2615,
            Activo          = true,
            UsuarioAdminId  = 5,
            FechaRegistro   = new DateTime(2024, 3, 20)
        },
        new()
        {
            Id              = 3,
            Nombre          = "Albergue San Roque Valledupar",
            Descripcion     = "Hogar temporal para recuperación física y emocional de mascotas rescatadas con amor.",
            Direccion       = "Calle 9B # 14-30, Barrio San Joaquín",
            Ciudad          = "Valledupar, Cesar",
            Email           = "adopciones@sanroquevalledupar.org",
            Telefono        = "300-891-2345",
            FotoUrl         = "https://images.unsplash.com/photo-1583337130417-3346a1be7dee?w=600&auto=format&fit=crop&q=80",
            HorarioAtencion = "Lunes a Viernes 8:30 - 17:30",
            Latitud         = 10.4682,
            Longitud        = -73.2552,
            Activo          = true,
            UsuarioAdminId  = 2,
            FechaRegistro   = new DateTime(2024, 4, 10)
        },
        new()
        {
            Id              = 4,
            Nombre          = "Hogar de Paso Santo Ecce Homo",
            Descripcion     = "Red de hogares de paso que acoge cachorros, gatitos y animales de edad avanzada en Valledupar.",
            Direccion       = "Transversal 23 # 16A-10, Barrio Garupal",
            Ciudad          = "Valledupar, Cesar",
            Email           = "eccehomo.mascotas@gmail.com",
            Telefono        = "316-543-9870",
            FotoUrl         = "https://images.unsplash.com/photo-1587300003388-59208cc962cb?w=600&auto=format&fit=crop&q=80",
            HorarioAtencion = "Miércoles a Domingo 10:00 - 17:00",
            Latitud         = 10.4740,
            Longitud        = -73.2670,
            Activo          = true,
            UsuarioAdminId  = 5,
            FechaRegistro   = new DateTime(2024, 5, 5)
        },
        new()
        {
            Id              = 5,
            Nombre          = "Amor Callejero Vallenato",
            Descripcion     = "Iniciativa ciudadana de rescate, esterilización masiva y adopción comunitaria en el Cesar.",
            Direccion       = "Calle 6 # 19B-12, Barrio Ciudad Jardín",
            Ciudad          = "Valledupar, Cesar",
            Email           = "contacto@amorcallejerovallenato.org",
            Telefono        = "311-234-5678",
            FotoUrl         = "https://images.unsplash.com/photo-1561037404-61cd46aa615b?w=600&auto=format&fit=crop&q=80",
            HorarioAtencion = "Sábados y Domingos 9:00 - 15:00",
            Latitud         = 10.4735,
            Longitud        = -73.2595,
            Activo          = true,
            UsuarioAdminId  = 2,
            FechaRegistro   = new DateTime(2024, 6, 12)
        },
        new()
        {
            Id              = 6,
            Nombre          = "Rescate Animal La Nevada",
            Descripcion     = "Atención a animales vulnerables en la comuna 5 y alrededores, brindando vacunación y hogares seguros.",
            Direccion       = "Manzana 12 Casa 4, Barrio La Nevada",
            Ciudad          = "Valledupar, Cesar",
            Email           = "nevada.animales@gmail.com",
            Telefono        = "316-908-1234",
            FotoUrl         = "https://images.unsplash.com/photo-1598133894008-61f7fdb8cc3a?w=600&auto=format&fit=crop&q=80",
            HorarioAtencion = "Lunes a Sábado 9:00 - 16:00",
            Latitud         = 10.4845,
            Longitud        = -73.2790,
            Activo          = true,
            UsuarioAdminId  = 5,
            FechaRegistro   = new DateTime(2024, 7, 1)
        }
    ];

    // ── Seed: Mascotas en Valledupar, Cesar ───────────────────────────────────
    private static List<Mascota> SeedMascotas() =>
    [
        new()
        {
            Id            = 1,
            Nombre        = "Fideo",
            Especie       = Especie.Perro,
            Raza          = "Labrador Criollo",
            EdadMeses     = 18,
            Descripcion   = "Fideo es un perro juguetón y cariñoso. Le encanta nadar en el río Guatapurí y es muy sociable con niños.",
            FotoUrl       = "https://images.unsplash.com/photo-1543466835-00a7907e9de1?w=600&auto=format&fit=crop&q=80",
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
            Raza          = "Siamés Criollo",
            EdadMeses     = 12,
            Descripcion   = "Luna es tranquila y afectuosa. Muy acostumbrada a casas frescas y se lleva excelente con otros felinos.",
            FotoUrl       = "https://images.unsplash.com/photo-1514888286974-6c03e2ca1dba?w=600&auto=format&fit=crop&q=80",
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
            Raza          = "Pastor Mestizo",
            EdadMeses     = 36,
            Descripcion   = "Thor es leal y protector. Ideal para fincas o casas con patio amplio. Muy obediente y noble.",
            FotoUrl       = "https://images.unsplash.com/photo-1583511655857-d19b40a7a54e?w=600&auto=format&fit=crop&q=80",
            Disponible    = true,
            Vacunada      = true,
            Castrada      = true,
            RefugioId     = 1,
            FechaRegistro = new DateTime(2024, 5, 20)
        },
        new()
        {
            Id            = 4,
            Nombre        = "Mía",
            Especie       = Especie.Gato,
            Raza          = "Criollo Angora",
            EdadMeses     = 6,
            Descripcion   = "Mía es una gatita bebé de ojos curiosos y pelo sedoso. Rescatada en el barrio San Joaquín.",
            FotoUrl       = "https://images.unsplash.com/photo-1573865526739-10659fec78a5?w=600&auto=format&fit=crop&q=80",
            Disponible    = true,
            Vacunada      = true,
            Castrada      = true,
            RefugioId     = 2,
            FechaRegistro = new DateTime(2024, 8, 1)
        },
        new()
        {
            Id            = 5,
            Nombre        = "Simón",
            Especie       = Especie.Perro,
            Raza          = "Beagle Criollo",
            EdadMeses     = 24,
            Descripcion   = "Simón es sociable, curioso y enérgico. Ama los paseos por el parque La Vallenata.",
            FotoUrl       = "https://images.unsplash.com/photo-1537151625747-768eb6cf92b2?w=600&auto=format&fit=crop&q=80",
            Disponible    = true,
            Vacunada      = true,
            Castrada      = true,
            RefugioId     = 2,
            FechaRegistro = new DateTime(2024, 8, 10)
        },
        new()
        {
            Id            = 6,
            Nombre        = "Kiwi",
            Especie       = Especie.Ave,
            Raza          = "Periquito Rescatado",
            EdadMeses     = 8,
            Descripcion   = "Ave recuperada de tenencia irregular, lista para vivir en un aviario espacioso y protegido.",
            FotoUrl       = "https://images.unsplash.com/photo-1552728089-57bdde30beb3?w=600&auto=format&fit=crop&q=80",
            Disponible    = true,
            Vacunada      = false,
            Castrada      = false,
            RefugioId     = 2,
            FechaRegistro = new DateTime(2024, 9, 1)
        },
        new()
        {
            Id            = 7,
            Nombre        = "Max",
            Especie       = Especie.Perro,
            Raza          = "Golden Mix Vallenato",
            EdadMeses     = 14,
            Descripcion   = "Max es noble, juguetón y súper amigable con toda la familia.",
            FotoUrl       = "https://images.unsplash.com/photo-1552053831-71594a27632d?w=600&auto=format&fit=crop&q=80",
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
            Raza          = "Criollo Persa",
            EdadMeses     = 16,
            Descripcion   = "Pelusa es un gato ronroneador que adora dormir a la sombra en las tardes calurosas.",
            FotoUrl       = "https://images.unsplash.com/photo-1533738363-b7f9aef128ce?w=600&auto=format&fit=crop&q=80",
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
            Raza          = "Criollo de Orejas Largas",
            EdadMeses     = 10,
            Descripcion   = "Toby tiene un corazón enorme y es muy fiel. Espera un hogar lleno de cariño.",
            FotoUrl       = "https://images.unsplash.com/photo-1587300003388-59208cc962cb?w=600&auto=format&fit=crop&q=80",
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
            Raza          = "Criolla Tricolor",
            EdadMeses     = 8,
            Descripcion   = "Nala tiene unos ojos verdes cautivadores. Muy ágil y sociable con las personas.",
            FotoUrl       = "https://images.unsplash.com/photo-1518791841217-8f162f1e1131?w=600&auto=format&fit=crop&q=80",
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
            Raza          = "Criollo Caramelo",
            EdadMeses     = 20,
            Descripcion   = "Canelo es el compañero ideal para caminatas mañaneras por Los Cortijos. Muy obediente.",
            FotoUrl       = "https://images.unsplash.com/photo-1535930891776-0c2dfb7fda1a?w=600&auto=format&fit=crop&q=80",
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
            Raza          = "Blanco Vallenato",
            EdadMeses     = 11,
            Descripcion   = "Mochi es blanco con manchitas cafés, muy dulce y pacífico.",
            FotoUrl       = "https://images.unsplash.com/photo-1543852786-1cf6624b9987?w=600&auto=format&fit=crop&q=80",
            Disponible    = true,
            Vacunada      = true,
            Castrada      = true,
            RefugioId     = 6,
            FechaRegistro = new DateTime(2024, 10, 12)
        }
    ];

    // ── Seed: Reportes de mascotas en Valledupar, Cesar ──────────────────────
    private static List<ReporteMascota> SeedReportes() =>
    [
        new()
        {
            Id           = 1,
            TipoReporte  = TipoReporte.Perdida,
            Especie      = Especie.Perro,
            NombreMascota = "Firulais",
            Raza         = "Labrador dorado criollo",
            ColorPelaje  = "Dorado",
            Ciudad       = "Valledupar, Cesar",
            Descripcion  = "Se extravió cerca de la Glorieta Mi Pedazo de Acordeón. Lleva collar azul y placa con su nombre.",
            FotoUrl      = "https://images.unsplash.com/photo-1552053831-71594a27632d?w=600&auto=format&fit=crop&q=80",
            FechaReporte = DateTime.UtcNow.AddDays(-2),
            Activo       = true,
            UsuarioId    = 3
        },
        new()
        {
            Id           = 2,
            TipoReporte  = TipoReporte.Encontrada,
            Especie      = Especie.Perro,
            NombreMascota = null,
            Raza         = "Mestizo amarillo",
            ColorPelaje  = "Amarillo claro",
            Ciudad       = "Valledupar, Cesar",
            Descripcion  = "Encontré un perrito dócil resguardado de la lluvia en el Barrio Los Cortijos. Sin collar.",
            FotoUrl      = "https://images.unsplash.com/photo-1543466835-00a7907e9de1?w=600&auto=format&fit=crop&q=80",
            FechaReporte = DateTime.UtcNow.AddDays(-1),
            Activo       = true,
            UsuarioId    = 4
        },
        new()
        {
            Id           = 3,
            TipoReporte  = TipoReporte.Perdida,
            Especie      = Especie.Gato,
            NombreMascota = "Nube",
            Raza         = "Angora criollo",
            ColorPelaje  = "Blanco",
            Ciudad       = "Valledupar, Cesar",
            Descripcion  = "Gatita blanca de ojos verdes, muy consentida. Se salió de casa en el Barrio Novalito.",
            FotoUrl      = "https://images.unsplash.com/photo-1514888286974-6c03e2ca1dba?w=600&auto=format&fit=crop&q=80",
            FechaReporte = DateTime.UtcNow.AddDays(-3),
            Activo       = true,
            UsuarioId    = 3
        },
        new()
        {
            Id           = 4,
            TipoReporte  = TipoReporte.Perdida,
            Especie      = Especie.Perro,
            NombreMascota = "Rocky",
            Raza         = "Criollo café",
            ColorPelaje  = "Caramelo con pecho blanco",
            Ciudad       = "Valledupar, Cesar",
            Descripcion  = "Visto por última vez en inmediaciones de la Plaza Alfonso López. Es muy amigable.",
            FotoUrl      = "https://images.unsplash.com/photo-1583511655857-d19b40a7a54e?w=600&auto=format&fit=crop&q=80",
            FechaReporte = DateTime.UtcNow.AddDays(-4),
            Activo       = true,
            UsuarioId    = 4
        },
        new()
        {
            Id           = 5,
            TipoReporte  = TipoReporte.Encontrada,
            Especie      = Especie.Gato,
            NombreMascota = null,
            Raza         = "Gato común",
            ColorPelaje  = "Atigrado gris",
            Ciudad       = "Valledupar, Cesar",
            Descripcion  = "Encontrado gatito joven cerca del Balneario Hurtado. Muy tranquilo y ronroneador.",
            FotoUrl      = "https://images.unsplash.com/photo-1573865526739-10659fec78a5?w=600&auto=format&fit=crop&q=80",
            FechaReporte = DateTime.UtcNow.AddDays(-5),
            Activo       = true,
            UsuarioId    = 3
        },
        new()
        {
            Id           = 6,
            TipoReporte  = TipoReporte.Perdida,
            Especie      = Especie.Perro,
            NombreMascota = "Bruno",
            Raza         = "Beagle mix",
            ColorPelaje  = "Tricolor",
            Ciudad       = "Valledupar, Cesar",
            Descripcion  = "Extraviado en el Barrio Garupal. Responde a silbidos y lleva collar rojo.",
            FotoUrl      = "https://images.unsplash.com/photo-1537151625747-768eb6cf92b2?w=600&auto=format&fit=crop&q=80",
            FechaReporte = DateTime.UtcNow.AddDays(-6),
            Activo       = true,
            UsuarioId    = 4
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
            NotasAdoptante = "Voy con mi familia a conocer a Fideo en la sede de Novalito.",
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
            NotasAdoptante = "Tengo experiencia previa con gatitos.",
            NotasRefugio   = "Confirmado. Pregunte por el equipo de adopciones en recepción.",
            AdoptanteId    = 4,
            MascotaId      = 4,
            RefugioId      = 2,
            FechaCreacion  = DateTime.UtcNow.AddDays(-2)
        }
    ];
}
