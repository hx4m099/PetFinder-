# 🐾 PetFinder

Aplicación web en **C# / ASP.NET Core MVC (.NET 8)** con dos funciones principales:

1. **Búsqueda de mascotas perdidas** — el usuario reporta una mascota perdida o encontrada con foto, y el sistema la compara con reportes recientes sugiriendo las coincidencias más parecidas (filtradas por especie, ciudad y fecha reciente).
2. **Adopción con refugios aliados** — los refugios publican animales disponibles y el usuario aparta una cita desde la app (agendar, confirmar, cancelar). El resto del proceso es presencial en el refugio.

---

## 🏗️ Arquitectura de Capas

```
PetFinder.sln
├── ENTITY          ← Entidades, enums y constantes. Sin dependencias externas.
├── DATOS           ← DbContext (PetFinderDbContext) + Fluent API. Preparado para Oracle.
├── DAL             ← Interfaces de repositorio + implementaciones en memoria (seed).
├── BLL             ← Servicios de negocio, DTOs, validadores y excepciones.
├── PetFinder.Web   ← ASP.NET Core MVC: controladores, ViewModels, vistas Razor y Three.js.
└── PetFinder.Tests ← Pruebas unitarias xUnit + Moq sobre BLL.
```

### Regla de dependencia estricta
```
Web → BLL → DAL → DATOS → (Oracle EF Core en el futuro)
            ↑               ↑
          ENTITY ──────────────────── (compartido por todos)
```

> La GUI **nunca** accede a DAL ni a DATOS directamente. Todo pasa por BLL.

### Inyección de dependencias
```csharp
// Program.cs (PetFinder.Web)
builder.Services.AddBll(); // Internamente llama a AddDal() → registra todos los repos
```

---

## 🗂️ Estructura de carpetas

```
ENTITY/
├── Entities/       Usuario, Refugio, Mascota, ReporteMascota, Cita
├── Enums/          Rol, Especie, EstadoCita, TipoReporte
└── Constants/      PetFinderConstants

DATOS/
├── Context/        PetFinderDbContext
└── Configurations/ Fluent API por entidad (UsuarioConfiguration, etc.)

DAL/
├── Interfaces/     IUsuarioRepository, IRefugioRepository, IMascotaRepository,
│                   IReporteRepository, ICitaRepository
├── Repositories/   Implementaciones en memoria
├── Seed/           InMemoryDataStore (datos semilla)
└── Extensions/     DalServiceExtensions (AddDal)

BLL/
├── Services/       AuthService, ReporteService, CoincidenciaService,
│                   AdopcionService, CitaService
├── Dtos/           DTOs de entrada y salida
├── Validators/     FluentValidation
├── Exceptions/     Excepciones de negocio
└── Extensions/     BllServiceExtensions (AddBll)

PetFinder.Web/
├── Controllers/    HomeController, AuthController, ReporteController,
│                   AdopcionController, CitaController, RefugioController,
│                   AdminController
├── ViewModels/     ViewModels por pantalla
├── Views/          Vistas Razor + _Layout compartido
└── wwwroot/        Bootstrap 5, CSS propio, JS / Three.js

PetFinder.Tests/
└── BllTests/       Pruebas unitarias con xUnit + Moq
```

---

## 🚀 Cómo ejecutar el proyecto

### Prerrequisitos
- .NET 8 SDK
- Visual Studio 2022 o VS Code con extensión C#

### Pasos
```bash
# 1. Clonar el repositorio
git clone https://github.com/hx4m099/PetFinder-.git
cd PetFinder-

# 2. Restaurar dependencias y compilar
dotnet build PetFinder.sln

# 3. Ejecutar la aplicación web
cd PetFinder.Web
dotnet run

# 4. Abrir en el navegador
# http://localhost:5000
```

> **Nota de seguridad:** La cadena de conexión Oracle nunca debe subirse al repositorio.  
> Usa `dotnet user-secrets` o `appsettings.Development.json` (en `.gitignore`).  
> Consulta `appsettings.example.json` para ver el formato esperado.

### Ejecutar pruebas
```bash
dotnet test PetFinder.sln
```

---

## 🛠️ Tecnologías

| Capa | Tecnología |
|------|-----------|
| Backend | C# 12, ASP.NET Core MVC 8 |
| ORM (futuro) | Entity Framework Core 8 + Oracle.EntityFrameworkCore |
| Autenticación | Cookie Authentication (simulada) |
| Validación | DataAnnotations + FluentValidation |
| Frontend | Razor Views, Bootstrap 5 |
| Animaciones 3D | Three.js |
| Pruebas | xUnit, Moq |
| CI/CD | GitHub (ramas por fase) |

---

## 📋 Estado de las Fases

| Fase | Descripción | Estado |
|------|-------------|--------|
| ✅ Fase 1 | ENTITY + DATOS + DAL (repos en memoria) | **Completa** |
| ✅ Fase 2 | BLL (servicios, DTOs, validadores) | **Completa** |
| ✅ Fase 3 | PetFinder.Web (controladores + vistas Razor) | **Completa** |
| ✅ Fase 4 | Three.js (animaciones 3D interactivas) | **Completa** |
| ⏳ Fase 5 | PetFinder.Tests (xUnit + Moq) | Pendiente |

---

## 🔌 Cómo conectar Oracle (cuando llegue el momento)

1. Agregar el paquete NuGet:
   ```bash
   dotnet add DATOS/DATOS.csproj package Oracle.EntityFrameworkCore --version 8.x.x
   ```
2. Configurar la cadena de conexión en `user-secrets` o `appsettings.Development.json`:
   ```json
   {
     "ConnectionStrings": {
       "PetFinderOracle": "Data Source=//host:1521/PETFINDER;User Id=...;Password=..."
     }
   }
   ```
3. En `DalServiceExtensions.AddDal()`:
   - Quitar `services.AddSingleton<InMemoryDataStore>()`
   - Agregar `services.AddDbContext<PetFinderDbContext>(opt => opt.UseOracle(...))`
   - Reemplazar `XxxRepository` por versiones EF Core (sin modificar BLL ni Web)
4. Ejecutar la primera migración:
   ```bash
   dotnet ef migrations add InitialCreate --project DATOS --startup-project PetFinder.Web
   dotnet ef database update --project DATOS --startup-project PetFinder.Web
   ```

---

## 👥 Roles del sistema

| Rol | Permisos |
|-----|---------|
| **Adoptante** | Reportar mascotas, buscar coincidencias, ver catálogo, agendar citas |
| **Refugio** | Publicar/editar mascotas, confirmar/cancelar citas |
| **Administrador** | Gestión completa: usuarios, refugios, reportes, estadísticas |
