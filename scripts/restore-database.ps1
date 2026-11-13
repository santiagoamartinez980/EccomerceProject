param(
    [Parameter(Mandatory = $true)] [string] $BackupPath,
    [string] $Server = 'localhost',
    [string] $Database = 'EccomerceDB'
)

$ErrorActionPreference = 'Stop'
if ($Database -notmatch '^[A-Za-z][A-Za-z0-9_]*$') { throw 'Nombre de base no válido.' }
$resolved = (Resolve-Path -LiteralPath $BackupPath).Path
if ([IO.Path]::GetExtension($resolved) -ne '.bak') { throw 'Se requiere un archivo .bak.' }
if ([IO.Path]::GetFileName($resolved) -notmatch ('^' + [regex]::Escape($Database) + '_\d{8}_\d{6}_[a-fA-F0-9]{8}\.bak$')) {
    throw 'El nombre del archivo no corresponde a una copia generada por esta aplicación.'
}

# BACKUP/RESTORE leen la ruta desde el proceso de SQL Server, no desde esta consola.
$sqlPath = $resolved.Replace("'", "''")
$sqlDatabase = '[' + $Database + ']'
$verify = "RESTORE VERIFYONLY FROM DISK = N'$sqlPath' WITH CHECKSUM"
& sqlcmd -S $Server -E -d master -b -Q $verify
if ($LASTEXITCODE -ne 0) { throw 'La verificación de la copia falló.' }


$answer = Read-Host "Se reemplazará $Database en $Server y se cerrarán sus conexiones. Escriba RESTAURAR para continuar"
if ($answer -cne 'RESTAURAR') { Write-Host 'Cancelado.'; exit 0 }

$restore = "IF DB_ID(N'$Database') IS NOT NULL ALTER DATABASE $sqlDatabase SET SINGLE_USER WITH ROLLBACK IMMEDIATE; RESTORE DATABASE $sqlDatabase FROM DISK = N'$sqlPath' WITH REPLACE, RECOVERY, CHECKSUM; ALTER DATABASE $sqlDatabase SET MULTI_USER;"
& sqlcmd -S $Server -E -d master -b -Q $restore
if ($LASTEXITCODE -ne 0) { throw 'La restauración falló. Revise SQL Server antes de volver a iniciar la aplicación.' }
Write-Host 'Base restaurada. Inicie sesión con los usuarios contenidos en la copia.'
