# Manual técnico — EccomerceProject

## 1. Alcance y estructura

La aplicación es una tienda de productos para PC. El repositorio contiene:

| Ruta | Responsabilidad |
| --- | --- |
| `backend/APIEccomerce` | API ASP.NET Core 8, reglas de negocio, acceso a SQL Server e integraciones. |
| `backend/APIEccomerce.Tests` | Pruebas .NET con xUnit, Moq y FluentAssertions; también contiene recursos de pruebas de carga. |
| `frontend/AngularPcMatchUp` | Cliente Angular 20, rutas, pantallas y servicios HTTP. |
| `scripts/restore-database.ps1` | Recuperación de SQL Server cuando no es posible entrar al panel. |
| `docs/copias-de-seguridad.md` | Procedimiento específico de copias y recuperación. |
| `docker-compose.yml` | Definición de SQL Server, API y cliente web para contenedores. |

El flujo principal es **navegador → Angular → API → SQL Server**. Angular consume rutas `/api` y el backend usa Entity Framework Core con el proveedor de SQL Server. En desarrollo, el proxy de Angular envía `/api` a `http://localhost:5000`. En Docker, Nginx sirve el cliente y envía `/api/` al servicio `backend:8080`.

## 2. Componentes del backend

`Program.cs` registra repositorios, servicios, `AppDbContext`, autenticación JWT, CORS, Swagger y `/health`. La API sigue en general la cadena **Controller → Service → Repository → AppDbContext**. `AppDbContext` define las entidades `Users`, `Products`, `Categories`, `Carts`, `CartItems`, `Addresses`, `Orders`, `OrderDetails` y `Payments`, junto con sus relaciones. Las migraciones están en `backend/APIEccomerce/Migrations`.

Áreas de la API:

| Ruta base | Función principal |
| --- | --- |
| `/api/Access` | Registro, inicio de sesión y validación de token. |
| `/api/Product` | Consulta de catálogo y gestión de productos; subida de imágenes para administradores. |
| `/api/Category` | Consulta y administración de categorías. |
| `/api/Cart` | Carrito del usuario autenticado. |
| `/api/Address` | Direcciones del usuario autenticado. |
| `/api/Order` | Pedidos, cancelación y cambio administrativo de estado. |
| `/api/Payment` | Creación de intención de pago y recepción del webhook de Wompi. |
| `/api/admin/backups` | Lista, creación, descarga y restauración de copias; requiere rol `Admin`. |

Swagger está habilitado por `Program.cs` y puede consultarse en `/swagger` cuando la API está en ejecución. `/health` confirma que responde el proceso; su configuración actual no comprueba explícitamente la conexión con SQL Server.

## 3. Frontend

`src/app/app.routes.ts` compone las rutas públicas y las áreas de autenticación, productos, carrito, direcciones, pedidos, checkout y administración. `features/admin/admin.routes.ts` incluye el panel, productos, categorías y copias de seguridad. Los servicios HTTP de cada función están bajo `features/*/services`.

`authGuard` y `adminGuard` controlan navegación en el cliente. El interceptor agrega el token guardado en `localStorage` a las peticiones protegidas. **La autorización efectiva corresponde al backend**: las acciones administrativas usan `[Authorize(Roles = "Admin")]`.

## 4. Configuración y secretos

La configuración .NET se obtiene de `appsettings.json`, el archivo por entorno y las variables de entorno. Para variables anidadas se usa `__` como separador, por ejemplo `ConnectionStrings__DefaultConnection`.

| Clave | Uso |
| --- | --- |
| `ConnectionStrings:DefaultConnection` | Conexión a la base SQL Server `EccomerceDB`. |
| `Jwt:Key` | Firma y validación de tokens JWT. |
| `Cloudinary:CloudName`, `Cloudinary:ApiKey`, `Cloudinary:ApiSecret` | Subida de imágenes de productos. |
| `GoogleMaps:ApiKey` | Geocodificación de direcciones. |
| `Wompi:PublicKey`, `Wompi:IntegritySecret`, `Wompi:EventSecret` | Intención de pago y validación del webhook. |
| `Frontend:Url` | Dirección de retorno tras el pago. |
| `Backup:Directory` | Directorio absoluto de archivos `.bak`. |

No publique credenciales en el repositorio ni en el cliente Angular. Configure secretos en variables de entorno o un almacén de secretos. Antes de desplegar, sustituya la clave JWT de ejemplo por una clave privada propia. El archivo local `appsettings.Development.json` puede contener datos sensibles y está ignorado por Git. La configuración de desarrollo de este equipo apunta a la carpeta de copias predeterminada de SQL Server en Windows; en otra instalación debe configurarse una ruta apropiada.

El registro público crea usuarios con rol `User`; el código no expone una operación pública para promover a `Admin`. La provisión inicial de un administrador debe realizarla un operador con acceso a la base, mediante un procedimiento controlado.

## 5. Ejecución local

Requisitos: SDK .NET 8, Node.js compatible con Angular 20, npm y SQL Server accesible. Para la recuperación mediante script se necesita además `sqlcmd` y una cuenta de Windows con permisos sobre SQL Server.

1. Configure la conexión SQL Server y las claves necesarias en el entorno de la API. En desarrollo local, el perfil `http` escucha en `http://localhost:5000` y establece `ASPNETCORE_ENVIRONMENT=Development`.
2. Desde la raíz del repositorio, inicie el backend:

   ```powershell
   dotnet run --project backend/APIEccomerce/APIEccomerce.csproj --launch-profile http
   ```

3. En otra terminal, inicie el frontend:

   ```powershell
   cd frontend/AngularPcMatchUp
   npm ci
   npm start
   ```

El servidor de Angular usa `proxy.conf.json` para enviar las llamadas `/api` a la API local. `Program.cs` ejecuta `Database.Migrate()` al arrancar, con hasta cinco intentos; si todos fallan, registra el error y continúa iniciando la API. Compruebe los registros y la disponibilidad real de la base antes de considerar el sistema listo.

Para trabajar con migraciones desde `backend/APIEccomerce`, `AppDbContextFactory` lee la configuración del entorno y crea el contexto de diseño. Ejemplo de actualización manual:

```powershell
cd backend/APIEccomerce
dotnet ef database update
```

Este comando requiere las herramientas de EF Core y una cadena de conexión válida. Antes de aplicar migraciones a una base con datos, cree una copia recuperable.

## 6. Autenticación e integraciones

Las contraseñas se almacenan con BCrypt. El inicio de sesión emite un JWT firmado con HMAC SHA-256, con identificador, correo y rol, y vencimiento de dos horas. El backend valida firma y expiración. Los permisos de administrador dependen del rol incluido en el token emitido; tras restaurar la base, es aconsejable cerrar sesión y volver a autenticarse.

- **Cloudinary:** `CloudinaryService` carga imágenes en la carpeta remota `productos` y devuelve la URL segura. La imagen no se almacena como archivo binario en SQL Server.
- **Google Maps:** `GoogleGeocodingService` consulta la API de geocodificación para obtener coordenadas y datos de la dirección. Requiere `GoogleMaps:ApiKey`.
- **Wompi:** `PaymentService` genera la intención de pago en COP, firma su integridad y valida la firma del webhook `transaction.updated` antes de actualizar el pago o confirmar el pedido. Requiere las tres claves de Wompi y una URL pública de retorno. El webhook está en `/api/Payment/webhook`.

## 7. Copias y recuperación

El módulo `DatabaseBackupService` ejecuta `BACKUP DATABASE` con `COPY_ONLY` y `CHECKSUM`. Conserva archivos `.bak` en `Backup:Directory` con un nombre generado por fecha y un identificador aleatorio. La lista y descarga leen ese mismo directorio. El controlador completo exige rol `Admin`.

Para usarlo, el proceso SQL Server necesita poder **leer y escribir** en el directorio, y la API necesita **leer** los archivos y acceder al directorio. El usuario SQL de la API necesita permiso de copia; la restauración exige permisos más altos. La API verifica la cabecera y la integridad de la copia antes de ejecutar `RESTORE DATABASE` desde una conexión a `master`. La restauración reemplaza el contenido actual y cierra conexiones activas. Hágala en una ventana de mantenimiento.

Si la base se perdió, el administrador almacenado en ella ya no podrá iniciar sesión. Un operador del servidor debe usar `scripts/restore-database.ps1` con un `.bak` conservado fuera de la aplicación. El script verifica el archivo, pide confirmar escribiendo `RESTAURAR` y usa autenticación integrada de Windows. Ejemplo:

```powershell
powershell -ExecutionPolicy Bypass -File .\scripts\restore-database.ps1 -BackupPath 'C:\SqlBackups\EccomerceDB\EccomerceDB_20261113_120000_1234abcd.bak' -Server localhost
```

El archivo debe ser visible para el proceso de SQL Server. Si se recupera en otra instancia con otras rutas físicas para datos y log, reubique los archivos desde SQL Server Management Studio. Consulte `docs/copias-de-seguridad.md` para el procedimiento operativo. El módulo crea copias **manuales**; la programación periódica y la conservación externa deben configurarse por separado.

## 8. Contenedores y despliegue

`docker-compose.yml` define SQL Server 2022, API y frontend Nginx. SQL Server publica el puerto local `1434`; la API, `5000`; el frontend, `4200`. SQL Server usa el volumen `sqldata` para sus datos. El frontend hace proxy de `/api` al backend. Para iniciar la pila, proporcione mediante un archivo `.env` local o el entorno las variables referenciadas en Compose, especialmente `SA_PASSWORD` y las de Cloudinary, y ejecute:

```powershell
docker compose up --build
```

Compose monta el volumen `sqlbackups` en una ruta compartida por SQL Server y la API, y configura `Backup__Directory`. Un contenedor inicial prepara sus permisos. Copie `.env.example` a `.env` y complete las claves exigidas para Cloudinary, Google Maps, Wompi y JWT, además de la contraseña de SQL Server. Una copia en el mismo host o volumen no sustituye una copia externa. Consulte `docs/docker.md` para los pasos de arranque, puertos, verificación y recuperación.

Para producción, configure HTTPS, secretos privados, una ruta pública de webhook y copias automáticas verificadas. El Nginx incluido establece `proxy_read_timeout 3600s`; una copia o restauración que exceda ese tiempo requerirá ajuste operativo.

## 9. Pruebas y verificación

Desde la raíz del repositorio:

```powershell
dotnet test backend/APIEccomerce.sln
dotnet build backend/APIEccomerce/APIEccomerce.csproj
```

Desde `frontend/AngularPcMatchUp`:

```powershell
npm run build
npm run test:ci
```

El frontend usa Jest para pruebas unitarias y tiene configuración y un script `npm run e2e` de Playwright; no se verificaron casos E2E en el repositorio. El proyecto .NET usa xUnit. Estas órdenes indican cómo ejecutar las suites existentes; su resultado depende del entorno y no sustituye una prueba de restauración en una base aislada.

## 10. Diagnóstico rápido

| Síntoma | Comprobación |
| --- | --- |
| La API inicia pero las operaciones fallan | Revise conexión SQL y registros de `Database.Migrate()`. `/health` por sí solo no valida la base. |
| `401` o `403` | Compruebe token vigente, encabezado `Authorization` y rol del usuario. |
| El panel de copias devuelve `503` | Configure `Backup:Directory` con ruta absoluta accesible. |
| La copia devuelve `500` | Revise espacio y permisos del servicio SQL Server sobre la carpeta; revise también permisos SQL para `BACKUP DATABASE`. |
| No aparece una copia descargada en el panel | Copie el `.bak` al directorio configurado; el panel lista archivos locales con el patrón de nombre generado por la aplicación. |
| La restauración falla | Verifique integridad del `.bak`, permisos `RESTORE`, conexiones activas y registros; use el procedimiento externo si la aplicación no puede operar. |
| No cargan imágenes, direcciones o pagos | Compruebe por separado las credenciales y la disponibilidad de Cloudinary, Google Maps o Wompi. |

Este manual describe el código presente en el repositorio. Los procedimientos de producción deben adaptarse a la instancia SQL Server, las cuentas de servicio y la infraestructura donde se despliegue.
