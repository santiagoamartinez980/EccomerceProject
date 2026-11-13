# Configuración local sin credenciales en Git

El backend de desarrollo obtiene las credenciales de .NET User Secrets. El archivo `backend/APIEccomerce/appsettings.Development.json` es local e ignorado por Git. Se puede crear a partir de `appsettings.Development.example.json` para ajustar la conexión local a SQL Server y las rutas de copias de seguridad.

Desde la raíz del repositorio, configure los valores privados con `dotnet user-secrets set <Nombre> <Valor> --project backend/APIEccomerce/APIEccomerce.csproj`. Los nombres utilizados son `Jwt:Key`, `Cloudinary:ApiKey`, `Cloudinary:ApiSecret`, `GoogleMaps:ApiKey`, `Wompi:IntegritySecret` y `Wompi:EventSecret`. `Cloudinary:CloudName` y `Wompi:PublicKey` se pueden configurar del mismo modo. No pegue los valores en archivos versionados.

Para Docker Compose, copie `.env.example` a `.env` y configure las variables indicadas allí. `.env` está ignorado por Git. En despliegues, configure estas variables en el gestor de secretos del entorno.

Las credenciales que aparecieron en commits anteriores deben rotarse en sus respectivos proveedores; eliminarlas de un commit nuevo no invalida las copias presentes en el historial de Git.
