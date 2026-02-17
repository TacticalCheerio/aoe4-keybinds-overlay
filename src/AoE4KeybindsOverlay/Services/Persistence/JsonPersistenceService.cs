using System.IO;
using System.Text.Json;

namespace AoE4KeybindsOverlay.Services.Persistence;

/// <summary>
/// Implements <see cref="IPersistenceService"/> using <see cref="System.Text.Json"/>
/// for serialization. Files are stored under <c>%APPDATA%\AoE4KeybindsOverlay\</c>.
/// </summary>
public sealed class JsonPersistenceService : IPersistenceService
{
    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true
    };

    /// <inheritdoc />
    public string DataDirectory { get; }

    /// <summary>
    /// Initializes a new instance of <see cref="JsonPersistenceService"/> using
    /// the default <c>%APPDATA%\AoE4KeybindsOverlay</c> directory.
    /// </summary>
    public JsonPersistenceService()
        : this(Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "AoE4KeybindsOverlay"))
    {
    }

    /// <summary>
    /// Initializes a new instance of <see cref="JsonPersistenceService"/> with
    /// a custom data directory. Primarily useful for testing.
    /// </summary>
    /// <param name="dataDirectory">The directory in which to store data files.</param>
    public JsonPersistenceService(string dataDirectory)
    {
        DataDirectory = dataDirectory ?? throw new ArgumentNullException(nameof(dataDirectory));
        EnsureDirectoryExists();
    }

    /// <inheritdoc />
    public void Save<T>(string fileName, T data)
    {
        var filePath = GetFilePath(fileName);
        EnsureDirectoryExists();

        var json = JsonSerializer.Serialize(data, SerializerOptions);
        File.WriteAllText(filePath, json);
    }

    /// <inheritdoc />
    public async Task SaveAsync<T>(string fileName, T data, CancellationToken cancellationToken = default)
    {
        var filePath = GetFilePath(fileName);
        EnsureDirectoryExists();

        await using var stream = File.Create(filePath);
        await JsonSerializer.SerializeAsync(stream, data, SerializerOptions, cancellationToken)
            .ConfigureAwait(false);
    }

    /// <inheritdoc />
    public T? Load<T>(string fileName)
    {
        var filePath = GetFilePath(fileName);

        if (!File.Exists(filePath))
            return default;

        var json = File.ReadAllText(filePath);

        if (string.IsNullOrWhiteSpace(json))
            return default;

        return JsonSerializer.Deserialize<T>(json, SerializerOptions);
    }

    /// <inheritdoc />
    public async Task<T?> LoadAsync<T>(string fileName, CancellationToken cancellationToken = default)
    {
        var filePath = GetFilePath(fileName);

        if (!File.Exists(filePath))
            return default;

        await using var stream = File.OpenRead(filePath);

        if (stream.Length == 0)
            return default;

        return await JsonSerializer.DeserializeAsync<T>(stream, SerializerOptions, cancellationToken)
            .ConfigureAwait(false);
    }

    /// <inheritdoc />
    public bool Exists(string fileName)
    {
        var filePath = GetFilePath(fileName);
        return File.Exists(filePath);
    }

    /// <inheritdoc />
    public bool Delete(string fileName)
    {
        var filePath = GetFilePath(fileName);

        if (!File.Exists(filePath))
            return false;

        File.Delete(filePath);
        return true;
    }

    /// <summary>
    /// Resolves a relative file name to an absolute path within the data directory.
    /// </summary>
    /// <param name="fileName">The file name.</param>
    /// <returns>The full file path.</returns>
    private string GetFilePath(string fileName)
    {
        if (string.IsNullOrWhiteSpace(fileName))
            throw new ArgumentException("File name must not be null or whitespace.", nameof(fileName));

        // Guard against path traversal
        if (fileName.Contains("..") || Path.IsPathRooted(fileName))
            throw new ArgumentException("File name must be a simple relative name without path traversal.", nameof(fileName));

        return Path.Combine(DataDirectory, fileName);
    }

    /// <summary>
    /// Ensures the data directory exists, creating it if necessary.
    /// </summary>
    private void EnsureDirectoryExists()
    {
        if (!Directory.Exists(DataDirectory))
        {
            Directory.CreateDirectory(DataDirectory);
        }
    }
}
