# Distributed Cache con Redis y .NET

> Proyecto de demostración para evidenciar conocimientos en **cache distribuido con Redis**, **Minimal APIs** y buenas prácticas en **.NET 10**.

## Objetivo

Este proyecto fue creado con fines educativos y de portafolio. Implementa una API REST de productos que utiliza **Redis como cache distribuido**, demostrando:

- Integración de Redis con `IDistributedCache` en .NET
- Estrategias de invalidación y actualización de cache
- Arquitectura limpia con Minimal APIs organizadas por features
- Soft delete con query filters globales de EF Core
- Configuración externalizada mediante Options Pattern
- Containerización de Redis con Docker Compose

## Stack Tecnologico

| Tecnologia | Uso |
|---|---|
| .NET 10 / C# | Framework principal |
| Minimal APIs | Endpoints HTTP sin controllers |
| Entity Framework Core | ORM con InMemory Database |
| Redis | Cache distribuido |
| StackExchange.Redis | Cliente Redis para .NET |
| Docker Compose | Infraestructura de Redis |
| Xunit | Test Unitario e Integracion |
| TestContainers | Servicios Reales |

## Arquitectura

```
DistributedCache/
├── Data/
│   └── AppDbContext.cs          # DbContext con seed data y query filters
├── Endpoints/
│   ├── IEndpoint.cs             # Interfaz para auto-registro de endpoints
│   └── EndpointExtensions.cs    # Descubrimiento automatico via reflection
├── Features/
│   └── Products/
│       ├── GetAllProducts.cs    # GET /products
│       ├── GetProductById.cs    # GET /products/{id} (con cache)
│       ├── CreateProduct.cs     # POST /products
│       ├── UpdateProduct.cs     # PUT /products/{id} (invalida cache)
│       └── DeleteProduct.cs     # DELETE /products/{id} (soft delete)
├── Models/
│   └── Product.cs               # Entidad con soporte para soft delete
├── Settings/
│   ├── CacheSettings.cs         # Configuracion de expiracion del cache
│   └── DatabaseSettings.cs      # Configuracion de base de datos
├── redis/
│   ├── Dockerfile               # Redis 7 Alpine
│   └── redis.conf               # Persistencia AOF, 256MB, LRU eviction
├── Program.cs                   # Composicion de la aplicacion
└── docker-compose.yml           # Orquestacion de Redis
```

## Endpoints

| Metodo | Ruta | Descripcion | Cache |
|---|---|---|---|
| `GET` | `/products` | Lista todos los productos | - |
| `GET` | `/products/{id}` | Obtiene un producto por ID | Lee/escribe cache |
| `POST` | `/products` | Crea un producto | - |
| `PUT` | `/products/{id}` | Actualiza un producto | Invalida cache |
| `DELETE` | `/products/{id}` | Elimina un producto (soft delete) | Actualiza cache |

## Estrategia de Cache

- **Cache-aside pattern**: se consulta primero Redis; si hay miss, se lee de la base de datos y se almacena en cache.
- **Expiracion dual**:
  - Absoluta: 5 minutos (el entry se elimina sin importar el uso)
  - Sliding: 2 minutos (se renueva con cada lectura)
- **Invalidacion en escritura**: el `PUT` elimina la entrada del cache para forzar un refresh.
- **Actualizacion en delete**: el `DELETE` actualiza el cache con el producto marcado como borrado, evitando lecturas inconsistentes.
- **Respuesta transparente**: el `GET /products/{id}` incluye el campo `source` (`"cache"` o `"database"`) para visualizar de donde provienen los datos.

## Conceptos Demostrados

- **Distributed Caching**: uso de `IDistributedCache` como abstraccion que permite intercambiar proveedores (Redis, SQL Server, NCache) sin cambiar la logica.
- **Soft Delete**: borrado logico con `IsDeleted` + `DeletedAt` y query filter global en EF Core que excluye automaticamente los registros eliminados.
- **Feature-based organization**: cada endpoint es una clase independiente que implementa `IEndpoint`, descubierta automaticamente por reflection.
- **Options Pattern**: configuracion tipada (`CacheSettings`, `DatabaseSettings`) inyectada via `IOptions<T>`.
- **Redis tuning**: configuracion personalizada con persistencia AOF, limite de memoria de 256MB y politica de eviccion `allkeys-lru`.

## Requisitos

- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- [Docker](https://www.docker.com/) (para Redis)

## Como Ejecutar

1. **Levantar Redis**:
   ```bash
   docker compose up -d
   ```

2. **Ejecutar la API**:
   ```bash
   dotnet run
   ```

3. **Probar los endpoints** (la API expone OpenAPI en desarrollo):
   ```bash
   # Obtener todos los productos
   curl http://localhost:5000/products

   # Obtener un producto (primera vez: database, segunda vez: cache)
   curl http://localhost:5000/products/1
   curl http://localhost:5000/products/1
   ```

## Licencia

Proyecto con fines educativos y de demostracion de conocimientos.
