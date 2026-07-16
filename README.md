# Lofasi Banking API

API backend para una plataforma bancaria digital. Permite registrar usuarios con perfil de cliente, crear cuentas bancarias, consultar balances, realizar depósitos/retiros y revisar el historial cronológico de transacciones.

El proyecto está construido con .NET 10, ASP.NET Core Web API, Entity Framework Core, SQLite, ASP.NET Core Identity y JWT.

## Decisiones Principales

### Arquitectura

Se implementó una arquitectura limpia por capas, separando responsabilidades y evitando que la capa de presentación conozca detalles de persistencia.

```text
src/
  Lofasi.API
  Lofasi.Application
  Lofasi.Domain
  Lofasi.Infrastructure

tests/
  Lofasi.UnitTests
```

La dirección de dependencias es:

```text
Lofasi.API -> Lofasi.Application
Lofasi.API -> Lofasi.Infrastructure
Lofasi.Infrastructure -> Lofasi.Application
Lofasi.Infrastructure -> Lofasi.Domain
Lofasi.Application -> Lofasi.Domain
Lofasi.Domain -> sin dependencias externas
```

La API no accede directamente a `DbContext`. Los controladores solo reciben requests, invocan servicios de aplicación y devuelven DTOs.

### Autenticación

Se usa ASP.NET Core Identity, pero no se colocó directamente en Application.

La decisión fue:

- `Lofasi.Application` define contratos como `IAuthService`, `ICurrentUserService` e `IJwtTokenService`.
- `Lofasi.Infrastructure` implementa esos contratos usando ASP.NET Core Identity, EF Core y JWT.
- `Lofasi.API` implementa `CurrentUserService` porque lee el usuario autenticado desde `HttpContext`.

Esto mantiene Application independiente de frameworks externos.

### Registro De Usuario Y Cliente

Se eligió la opción B: registrar usuario y perfil de cliente en una sola operación.

`POST /api/auth/register` crea:

- Un usuario Identity.
- Un perfil `Customer` asociado al `UserId` del usuario Identity.

Esto simplifica el flujo del ejercicio y evita que el cliente de la API pueda enviar un `customerId` arbitrario al crear cuentas.

### Manejo De Dinero

Se decidió no almacenar dinero como `decimal` en las entidades ni en la base de datos.

Motivo:

- Los movimientos monetarios deben evitar problemas de precisión, redondeos implícitos o configuración específica del proveedor de base de datos.
- SQLite no tiene un tipo decimal nativo equivalente a bases como SQL Server.

La decisión fue almacenar dinero como enteros en centavos:

```text
10.99 -> 1099
```

Ejemplos internos:

- `MonthlyIncomeInCents`
- `BalanceInCents`
- `AmountInCents`
- `BalanceAfterTransactionInCents`

La clase `Money` centraliza la conversión:

```csharp
Money.ToCents(10.99m);   // 1099
Money.FromCents(1099);   // 10.99m
```

Además, se rechazan valores con más de dos decimales en vez de redondearlos silenciosamente.

### Generación De Número De Cuenta

El número de cuenta sigue el formato requerido:

```text
ACC-YYYYMMDD-XXXX
```

Ejemplo:

```text
ACC-20260715-0427
```

Reglas:

- Prefijo fijo `ACC-`.
- Fecha de creación en formato `yyyyMMdd`.
- Sufijo de cuatro dígitos.
- Longitud total: 17 caracteres.
- Restricción única en base de datos sobre `AccountNumber`.
- El servicio de aplicación reintenta la generación si detecta una colisión antes de persistir.

### Manejo Global De Excepciones

Los controladores no usan bloques repetitivos `try-catch`.

Los errores de negocio se expresan con excepciones custom en Application:

- `ValidationException`
- `NotFoundException`
- `InsufficientFundsException`
- `InvalidCredentialsException`
- `UnauthenticatedException`
- `ConflictException`
- `BusinessException`

La API usa `IExceptionHandler` mediante `GlobalExceptionHandler` para traducir esas excepciones a respuestas HTTP limpias.

Ejemplo de respuesta:

```json
{
  "statusCode": 400,
  "error": "Insufficient funds.",
  "traceId": "..."
}
```

## Estructura Del Proyecto

### Lofasi.Domain

Contiene el modelo de dominio puro.

Archivos principales:

- `Entities/Customer.cs`
- `Entities/BankAccount.cs`
- `Entities/AccountTransaction.cs`
- `Enums/Gender.cs`
- `Enums/TransactionType.cs`
- `ValueObjects/Money.cs`

Responsabilidades:

- Representar clientes, cuentas y transacciones.
- Proteger invariantes básicas.
- Manejar depósitos y retiros desde `BankAccount`.
- Registrar el balance histórico después de cada transacción.

No contiene:

- EF Core.
- Identity.
- ASP.NET Core.
- DTOs HTTP.

### Lofasi.Application

Contiene contratos, DTOs, servicios de caso de uso y excepciones de negocio.

Responsabilidades:

- Orquestar casos de uso.
- Validar reglas de aplicación.
- Convertir dinero decimal de entrada a centavos.
- Enforzar que las cuentas pertenezcan al usuario autenticado.
- Definir interfaces para persistencia y servicios externos.

Interfaces relevantes:

- `IAccountService`
- `ICustomerService`
- `IAuthService`
- `ICurrentUserService`
- `IAccountRepository`
- `ICustomerRepository`
- `IUnitOfWork`
- `IAccountNumberGenerator`
- `IDateTimeProvider`

### Lofasi.Infrastructure

Contiene implementaciones técnicas.

Responsabilidades:

- EF Core con SQLite.
- ASP.NET Core Identity.
- Generación JWT.
- Repositorios.
- Unit of Work.
- Generador de números de cuenta.

`BankingDbContext` hereda de:

```csharp
IdentityDbContext<ApplicationUser, IdentityRole<Guid>, Guid>
```

Esto permite almacenar en una sola base SQLite:

- Usuarios Identity.
- Roles Identity.
- Clientes.
- Cuentas.
- Transacciones.

### Lofasi.API

Contiene la capa de presentación HTTP.

Responsabilidades:

- Controladores.
- Configuración de JWT.
- Swagger/OpenAPI.
- Global exception handling.
- Implementación de `ICurrentUserService` usando `HttpContext`.

Los controladores no usan EF Core ni `DbContext`.

## Requisitos

- .NET SDK 10.
- SQLite no requiere instalación manual para este proyecto.

Verificar versión:

```bash
dotnet --version
```

## Configuración

La configuración principal está en:

```text
src/Lofasi.API/appsettings.json
```

Valores relevantes:

```json
{
  "ConnectionStrings": {
    "Database": "Data Source=lofasi.db"
  },
  "Jwt": {
    "Issuer": "Lofasi",
    "Audience": "Lofasi.Api",
    "Secret": "development-only-secret-change-me-development-only-secret-change-me",
    "ExpirationMinutes": 60
  }
}
```

Nota: el secreto JWT incluido es solo para desarrollo local. En un ambiente real debe venir desde variables de entorno, user secrets o un proveedor seguro.

## Ejecutar El Proyecto

### Inicializacion Automatizada

Se agregaron scripts para inicializar el proyecto de forma repetible en Linux/macOS y Windows.

Los scripts hacen lo siguiente:

- Restauran paquetes NuGet.
- Restauran la herramienta local `dotnet-ef` desde `.config/dotnet-tools.json`.
- Ejecutan las migraciones de EF Core.
- Crean o actualizan la base SQLite `lofasi.db`.
- Compilan la solucion.
- Opcionalmente ejecutan la API.

Linux/macOS:

```bash
./scripts/init.sh
```

Linux/macOS inicializando y ejecutando la API:

```bash
./scripts/init.sh --run
```

Windows PowerShell:

```powershell
.\scripts\init.ps1
```

Windows PowerShell inicializando y ejecutando la API:

```powershell
.\scripts\init.ps1 -Run
```

Estos scripts son la forma recomendada de levantar el proyecto por primera vez porque automatizan la creacion de la base de datos.

### Comandos Manuales

Restaurar paquetes:

```bash
dotnet restore
```

Compilar:

```bash
dotnet build
```

Ejecutar API:

```bash
dotnet run --project src/Lofasi.API
```

Swagger UI estará disponible en desarrollo en:

```text
https://localhost:{puerto}/swagger
```

También puede aparecer un puerto HTTP según `launchSettings.json`.

## Base De Datos

La base usa SQLite.

Connection string por defecto:

```text
Data Source=lofasi.db
```

Cuando se agreguen migraciones, los comandos esperados son:

```bash
dotnet ef migrations add InitialCreate --project src/Lofasi.Infrastructure --startup-project src/Lofasi.API
dotnet ef database update --project src/Lofasi.Infrastructure --startup-project src/Lofasi.API
```

En este repositorio ya existe una migracion inicial en:

```text
src/Lofasi.Infrastructure/Persistence/Migrations
```

Por eso, para crear la base local solo se necesita ejecutar:

```bash
dotnet tool restore
dotnet tool run dotnet-ef database update --project src/Lofasi.Infrastructure --startup-project src/Lofasi.API --context BankingDbContext
```

Los scripts `scripts/init.sh` y `scripts/init.ps1` ejecutan esos pasos automaticamente.

Importante: si `dotnet ef` no está instalado:

```bash
dotnet tool install --global dotnet-ef
```

Nota: este proyecto usa una herramienta local de .NET, por lo que no es obligatorio instalar `dotnet-ef` globalmente si se usa `dotnet tool restore`.

## Docker

Se agrego un `Dockerfile` multi-stage.

El contenedor publica la API en el puerto interno `8080` y usa SQLite en `/data/lofasi.db` por defecto.

Construir la imagen:

```bash
docker build -t lofasi-api .
```

Ejecutar el contenedor:

```bash
docker run --rm -p 8080:8080 -v lofasi-data:/data lofasi-api
```

La API quedara disponible en:

```text
http://localhost:8080
```

Swagger en Docker:

```text
http://localhost:8080/swagger
```

Para habilitar Swagger dentro del contenedor, ejecutar con ambiente `Development`:

```bash
docker run --rm -p 8080:8080 -v lofasi-data:/data -e ASPNETCORE_ENVIRONMENT=Development lofasi-api
```

El Dockerfile define:

```text
ConnectionStrings__Database=Data Source=/data/lofasi.db
Database__ApplyMigrationsOnStartup=true
```

Esto significa que, al iniciar el contenedor, la API aplica automaticamente las migraciones pendientes y crea la base SQLite si no existe.

Para usar otro archivo de base de datos:

```bash
docker run --rm -p 8080:8080 \
  -v lofasi-data:/data \
  -e ConnectionStrings__Database="Data Source=/data/custom-lofasi.db" \
  lofasi-api
```

Para definir un secreto JWT diferente en Docker:

```bash
docker run --rm -p 8080:8080 \
  -v lofasi-data:/data \
  -e Jwt__Secret="replace-this-with-a-long-secure-secret-for-local-docker" \
  lofasi-api
```

Importante: en produccion no se debe usar el secreto JWT incluido en `appsettings.json`.

## Autenticación En Swagger

Flujo recomendado:

1. Ejecutar `POST /api/auth/register`.
2. Copiar `accessToken` de la respuesta.
3. Abrir Swagger UI.
4. Usar el botón `Authorize`.
5. Ingresar el token JWT.
6. Ejecutar endpoints protegidos.

## Endpoints

### Registrar Usuario Y Cliente

```http
POST /api/auth/register
```

Request:

```json
{
  "email": "jane@example.com",
  "password": "SecurePassword123!",
  "fullName": "Jane Doe",
  "dateOfBirth": "1990-05-10",
  "gender": "Female",
  "monthlyIncome": 4500.75
}
```

Response:

```json
{
  "accessToken": "jwt-token",
  "expiresAtUtc": "2026-07-15T13:00:00Z",
  "userId": "guid",
  "customerId": "guid",
  "email": "jane@example.com"
}
```

### Login

```http
POST /api/auth/login
```

Request:

```json
{
  "email": "jane@example.com",
  "password": "SecurePassword123!"
}
```

### Perfil Del Cliente Autenticado

```http
GET /api/customers/me
```

Requiere JWT.

### Crear Cuenta Bancaria

```http
POST /api/accounts
```

Requiere JWT.

Request:

```json
{
  "openingBalance": 1000.50
}
```

El `customerId` no se envía en el request. Se toma desde el usuario autenticado.

### Consultar Balance

```http
GET /api/accounts/{accountNumber}/balance
```

Requiere JWT.

Ejemplo:

```http
GET /api/accounts/ACC-20260715-0427/balance
```

### Depositar

```http
POST /api/accounts/{accountNumber}/deposits
```

Requiere JWT.

Request:

```json
{
  "amount": 250.25
}
```

### Retirar

```http
POST /api/accounts/{accountNumber}/withdrawals
```

Requiere JWT.

Request:

```json
{
  "amount": 100.00
}
```

Si no hay fondos suficientes, se retorna un error de negocio con `400 Bad Request`.

### Historial De Transacciones

```http
GET /api/accounts/{accountNumber}/transactions
```

Requiere JWT.

Devuelve las transacciones en orden cronológico.

Cada entrada incluye:

- Identificador único de transacción.
- Tipo: `Deposit` o `Withdrawal`.
- Monto decimal.
- Monto en centavos.
- Timestamp.
- Balance histórico posterior a la transacción.
- Balance histórico en centavos.

## Pruebas

Ejecutar pruebas unitarias:

```bash
dotnet test
```

O ejecutar solo el proyecto de pruebas:

```bash
dotnet test tests/Lofasi.UnitTests/Lofasi.UnitTests.csproj
```

Pruebas cubiertas:

- Depósitos.
- Retiros con fondos suficientes.
- Retiros con fondos insuficientes.
- Formato del generador de número de cuenta.
- Unicidad básica del generador de número de cuenta.

## Seguridad Y Propiedad De Cuentas

Los endpoints bancarios requieren JWT.

La búsqueda de cuentas se hace usando:

```text
accountNumber + userId autenticado
```

Esto evita que un usuario acceda a cuentas de otro cliente.

Si una cuenta existe pero no pertenece al usuario autenticado, la API responde como si no existiera usando `404 Not Found`. Esta decisión evita filtrar la existencia de cuentas ajenas.

## Advertencias Conocidas

Actualmente NuGet reporta advertencias de vulnerabilidad en paquetes transitivos:

- `Microsoft.OpenApi 2.3.0`
- `SQLitePCLRaw.lib.e_sqlite3 2.1.11`

Estas advertencias no impiden compilar ni ejecutar las pruebas, pero deben revisarse antes de un despliegue real.

## Comandos Útiles

Restaurar:

```bash
dotnet restore
```

Compilar:

```bash
dotnet build
```

Ejecutar pruebas:

```bash
dotnet test
```

Ejecutar API:

```bash
dotnet run --project src/Lofasi.API
```

Validar formato:

```bash
dotnet format Lofasi.slnx --verify-no-changes
```
