# Copias de seguridad de EccomerceDB

## Preparación

Configure `Backup__Directory` como variable de entorno de la API con una ruta absoluta **fuera del repositorio**. La cuenta que ejecuta SQL Server necesita lectura y escritura allí; la cuenta de la API necesita lectura y permiso para crear la carpeta. La cuenta SQL usada por la API requiere permisos de `BACKUP DATABASE` y `RESTORE DATABASE` sobre la base (para restaurar, normalmente privilegios elevados). Si no quiere dar permisos de restauración a la API, use el script desde el servidor y no use el botón del panel.

En este equipo, la configuración de desarrollo apunta a la carpeta de copias predeterminada de la instancia local de SQL Server. En otro equipo debe cambiarse esa ruta o definirse `Backup__Directory`.

Ejemplo local: `Backup__Directory=C:\SqlBackups\EccomerceDB`. No guarde copias en `wwwroot`, Git ni en la misma unidad como única copia. Proteja el directorio: los archivos contienen usuarios, direcciones, pedidos y demás datos de la tienda.

## Uso normal

En **Admin → Copias de seguridad**, cree una copia, descárguela y guárdela en un almacenamiento distinto al servidor. El panel muestra las copias que permanecen en la carpeta configurada y permite restaurar una de ellas si la aplicación sigue disponible. Restaurar reemplaza el estado actual de la base: cierre la tienda o anuncie mantenimiento antes de hacerlo. Al terminar, vuelva a iniciar sesión.

Esta función crea copias manuales. Para producción, programe además copias automáticas de SQL Server y compruebe periódicamente que se pueden restaurar. El botón por sí solo no asegura que exista una copia reciente.

## Si la base se perdió y no puede iniciar sesión

El acceso de administrador vive en la base. Use una cuenta con acceso administrativo a SQL Server, en el propio servidor, para restaurar el archivo `.bak` guardado fuera de la aplicación:

```powershell
powershell -ExecutionPolicy Bypass -File .\scripts\restore-database.ps1 -BackupPath 'C:\SqlBackups\EccomerceDB\EccomerceDB_20261113_120000_1234abcd.bak' -Server localhost
```

El script verifica la copia, pide escribir `RESTAURAR` y reemplaza `EccomerceDB`. Requiere `sqlcmd` y autenticación integrada de Windows. SQL Server debe poder leer el archivo. Si se restaura en otro servidor o cambian las rutas de datos/log, use SQL Server Management Studio para reubicar los archivos durante la restauración. Después de restaurar, los usuarios y roles vuelven exactamente al estado guardado en la copia.
