# Ejecución con Docker Compose

## Preparación

Se necesita Docker Engine o Docker Desktop con el daemon activo. Copie `.env.example` a `.env` en la raíz y complete los valores privados: contraseña de SQL Server, clave JWT, Cloudinary, Google Maps y las tres claves de Wompi. El archivo `.env` está ignorado por Git. La clave JWT debe ser privada y suficientemente larga; no use la clave de ejemplo de `appsettings.json`.

`FRONTEND_URL` debe apuntar a la dirección que abre el comprador para volver desde Wompi, por ejemplo `http://localhost:4200` en pruebas locales o una URL HTTPS pública. El webhook de Wompi debe apuntar a `https://<dominio-publico>/api/Payment/webhook`. Compose no crea el túnel ni registra esta URL en el panel de Wompi; la URL de un túnel temporal cambia al reiniciarlo.

Los puertos predeterminados son frontend `4200`, API `5000` y SQL Server `1434`. Si la ejecución local ya usa un puerto, cambie `FRONTEND_PORT`, `API_PORT` o `DB_PORT` en `.env`. Si cambia el puerto o dominio del frontend, cambie también `FRONTEND_URL`.

## Arranque y verificación

Desde la raíz del proyecto:

```powershell
docker compose config --quiet
docker compose up --build -d
docker compose ps
```

Abra `http://localhost:4200` (o el `FRONTEND_PORT` configurado). Compruebe `http://localhost:5000/health` y `http://localhost:4200/api/Category` con los puertos elegidos. `/health` indica que la API responde; no prueba por sí solo la conexión a SQL Server. El backend aplica las migraciones al iniciar. Compruebe `docker compose logs backend` para confirmar la conexión y las migraciones.

Compose crea dos volúmenes persistentes: `sqldata` para la base y `sqlbackups` para archivos `.bak`. Un contenedor inicial ajusta permisos de `sqlbackups` para SQL Server. SQL Server y API ven las copias en `/var/opt/mssql/backups`, y la API configura `Backup__Directory` con esa ruta. El panel **Admin → Copias de seguridad** puede crear, listar, descargar y restaurar copias si las cuentas SQL tienen los permisos necesarios. **Descargue cada copia y guárdela fuera del host Docker**; los dos volúmenes no sustituyen una copia externa.

## Recuperación sin acceso al panel

Conserve el `.bak` fuera del host. Si se pierde la base, un operador debe restaurarlo desde SQL Server; el script PowerShell del repositorio está pensado para SQL Server en Windows con autenticación integrada, no para el contenedor Linux. Para Compose, vuelva a colocar el archivo en el volumen `sqlbackups` o en una ruta legible por SQL Server y use una herramienta SQL conectada a `master` para ejecutar `RESTORE VERIFYONLY` y `RESTORE DATABASE`. Reubique archivos físicos con `MOVE` si cambia la instancia o el sistema de archivos. El proceso de recuperación debe probarse en una base aislada antes de necesitarlo en producción.

## Diagnóstico

- `docker compose config --quiet` señala variables faltantes. Complete `.env` sin publicar su contenido.
- Si `db` no queda healthy, revise `docker compose logs db`, la contraseña y memoria disponible. SQL Server necesita recursos suficientes para iniciar.
- Si el backend no conecta, revise `docker compose logs backend` y `docker compose ps`; el backend espera a que `db` esté healthy.
- Si fallan las copias, compruebe los permisos del volumen `sqlbackups` y permisos SQL de `BACKUP DATABASE` y `RESTORE DATABASE`.
- Si Wompi no devuelve al usuario o no envía eventos, compruebe `FRONTEND_URL`, el túnel o dominio público y la URL registrada en el panel de Wompi.

Evite `docker compose down -v` si quiere conservar datos y copias: esa opción elimina ambos volúmenes.
