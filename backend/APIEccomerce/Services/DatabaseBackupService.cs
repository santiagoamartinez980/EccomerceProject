using Microsoft.Data.SqlClient;
using System.Data;
using System.Text.RegularExpressions;

namespace APIEccomerce.Services;

public sealed class DatabaseBackupService(IConfiguration configuration)
{
    private static readonly Regex BackupName = new(@"^EccomerceDB_\d{8}_\d{6}_[a-f0-9]{8}\.bak$", RegexOptions.Compiled | RegexOptions.IgnoreCase);
    private readonly string? _directory = configuration["Backup:Directory"];
    private readonly string _connectionString = configuration.GetConnectionString("DefaultConnection")
        ?? throw new InvalidOperationException("Falta ConnectionStrings:DefaultConnection.");

    public string DirectoryPath => !string.IsNullOrWhiteSpace(_directory)
        ? Path.GetFullPath(_directory)
        : throw new InvalidOperationException("Configure Backup:Directory fuera del proyecto y conceda acceso de lectura y escritura a SQL Server y a la API.");

    public IReadOnlyList<BackupFile> List()
    {
        if (!Directory.Exists(DirectoryPath)) return [];
        return Directory.EnumerateFiles(DirectoryPath, "*.bak")
            .Where(path => BackupName.IsMatch(Path.GetFileName(path)))
            .Select(path => new FileInfo(path))
            .OrderByDescending(file => file.CreationTimeUtc)
            .Select(file => new BackupFile(file.Name, file.Length, file.CreationTimeUtc))
            .ToList();
    }

    public string GetPath(string name)
    {
        if (!BackupName.IsMatch(name)) throw new ArgumentException("Nombre de copia inválido.");
        var path = Path.Combine(DirectoryPath, name);
        if (!File.Exists(path)) throw new FileNotFoundException("Copia no encontrada.");
        return path;
    }

    public async Task<BackupFile> CreateAsync(CancellationToken cancellationToken)
    {
        Directory.CreateDirectory(DirectoryPath);
        var name = $"EccomerceDB_{DateTime.UtcNow:yyyyMMdd_HHmmss}_{Guid.NewGuid().ToString("N")[..8]}.bak";
        var path = Path.Combine(DirectoryPath, name);
        var database = new SqlConnectionStringBuilder(_connectionString).InitialCatalog;
        if (string.IsNullOrWhiteSpace(database)) throw new InvalidOperationException("La conexión debe indicar Database.");

        await using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);
        await using var command = connection.CreateCommand();
        command.CommandText = $"BACKUP DATABASE {Quote(database)} TO DISK = @path WITH COPY_ONLY, CHECKSUM, INIT";
        command.Parameters.Add(new SqlParameter("@path", SqlDbType.NVarChar, 4000) { Value = path });
        command.CommandTimeout = 0;
        try
        {
            await command.ExecuteNonQueryAsync(cancellationToken);
            var file = new FileInfo(path);
            return new BackupFile(file.Name, file.Length, file.CreationTimeUtc);
        }
        catch
        {
            if (File.Exists(path)) File.Delete(path);
            throw;
        }
    }

    public async Task RestoreAsync(string name, CancellationToken cancellationToken)
    {
        var path = GetPath(name);
        var builder = new SqlConnectionStringBuilder(_connectionString);
        var database = builder.InitialCatalog;
        if (string.IsNullOrWhiteSpace(database)) throw new InvalidOperationException("La conexión debe indicar Database.");
        builder.InitialCatalog = "master";
        builder.Pooling = false;

        await using var connection = new SqlConnection(builder.ConnectionString);
        await connection.OpenAsync(cancellationToken);
        await using (var header = connection.CreateCommand())
        {
            header.CommandText = "RESTORE HEADERONLY FROM DISK = @path";
            header.Parameters.Add(new SqlParameter("@path", SqlDbType.NVarChar, 4000) { Value = path });
            header.CommandTimeout = 0;
            await using var reader = await header.ExecuteReaderAsync(cancellationToken);
            if (!await reader.ReadAsync(cancellationToken) ||
                !string.Equals(reader.GetString(reader.GetOrdinal("DatabaseName")), database, StringComparison.OrdinalIgnoreCase))
                throw new InvalidOperationException("La copia no pertenece a la base configurada.");
        }
        await using (var verify = connection.CreateCommand())
        {
            verify.CommandText = "RESTORE VERIFYONLY FROM DISK = @path WITH CHECKSUM";
            verify.Parameters.Add(new SqlParameter("@path", SqlDbType.NVarChar, 4000) { Value = path });
            verify.CommandTimeout = 0;
            await verify.ExecuteNonQueryAsync(cancellationToken);
        }

        SqlConnection.ClearAllPools();
        await using var command = connection.CreateCommand();
        command.CommandText = $"IF DB_ID(@database) IS NOT NULL ALTER DATABASE {Quote(database)} SET SINGLE_USER WITH ROLLBACK IMMEDIATE; " +
                              $"RESTORE DATABASE {Quote(database)} FROM DISK = @path WITH REPLACE, RECOVERY, CHECKSUM; " +
                              $"ALTER DATABASE {Quote(database)} SET MULTI_USER;";
        command.Parameters.Add(new SqlParameter("@database", SqlDbType.NVarChar, 128) { Value = database });
        command.Parameters.Add(new SqlParameter("@path", SqlDbType.NVarChar, 4000) { Value = path });
        command.CommandTimeout = 0;
        try { await command.ExecuteNonQueryAsync(cancellationToken); }
        finally
        {
            await using var reset = connection.CreateCommand();
            reset.CommandText = $"IF DB_ID(@database) IS NOT NULL ALTER DATABASE {Quote(database)} SET MULTI_USER";
            reset.Parameters.Add(new SqlParameter("@database", SqlDbType.NVarChar, 128) { Value = database });
            await reset.ExecuteNonQueryAsync(CancellationToken.None);
            SqlConnection.ClearAllPools();
        }
    }

    private static string Quote(string identifier) => "[" + identifier.Replace("]", "]]", StringComparison.Ordinal) + "]";
}

public sealed record BackupFile(string Name, long Bytes, DateTime CreatedUtc);
