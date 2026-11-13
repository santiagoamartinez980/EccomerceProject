using APIEccomerce.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace APIEccomerce.Controllers;

[ApiController]
[Route("api/admin/backups")]
[Authorize(Roles = "Admin")]
public sealed class DatabaseBackupController(DatabaseBackupService backups, ILogger<DatabaseBackupController> logger) : ControllerBase
{
    [HttpGet]
    public IActionResult List()
    {
        try { return Ok(backups.List()); }
        catch (InvalidOperationException ex) { return StatusCode(503, new { message = ex.Message }); }
    }

    [HttpPost]
    public async Task<IActionResult> Create(CancellationToken cancellationToken)
    {
        try { return Ok(await backups.CreateAsync(cancellationToken)); }
        catch (Exception ex)
        {
            logger.LogError(ex, "No se pudo crear la copia de seguridad.");
            return StatusCode(500, new { message = "No se pudo crear la copia. Revise permisos y espacio del servidor." });
        }
    }

    [HttpGet("{name}")]
    public IActionResult Download(string name)
    {
        try
        {
            var path = backups.GetPath(name);
            return PhysicalFile(path, "application/octet-stream", name);
        }
        catch (ArgumentException) { return BadRequest(new { message = "Nombre de copia inválido." }); }
        catch (FileNotFoundException) { return NotFound(); }
    }

    [HttpPost("{name}/restore")]
    public async Task<IActionResult> Restore(string name, CancellationToken cancellationToken)
    {
        try
        {
            await backups.RestoreAsync(name, cancellationToken);
            logger.LogWarning("El administrador {User} restauró la base desde {Backup}.", User.Identity?.Name, name);
            return Ok(new { message = "Base de datos restaurada. Inicie sesión de nuevo." });
        }
        catch (ArgumentException) { return BadRequest(new { message = "Nombre de copia inválido." }); }
        catch (FileNotFoundException) { return NotFound(); }
        catch (Exception ex)
        {
            logger.LogError(ex, "Falló la restauración de {Backup}.", name);
            return StatusCode(500, new { message = "Falló la restauración. Use el procedimiento de recuperación del servidor y revise los registros." });
        }
    }
}
