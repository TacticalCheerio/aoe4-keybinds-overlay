namespace AoE4KeybindsOverlay.Services.Persistence;

/// <summary>
/// Provides save/load operations for application settings and statistics.
/// Data is persisted as JSON files in the application's data directory.
/// </summary>
public interface IPersistenceService
{
    /// <summary>
    /// Gets the root directory where application data is stored.
    /// </summary>
    string DataDirectory { get; }

    /// <summary>
    /// Saves an object as JSON to the specified file name within the data directory.
    /// </summary>
    /// <typeparam name="T">The type of object to save.</typeparam>
    /// <param name="fileName">The file name (e.g., "settings.json"). Relative to the data directory.</param>
    /// <param name="data">The object to serialize and save.</param>
    void Save<T>(string fileName, T data);

    /// <summary>
    /// Saves an object as JSON asynchronously to the specified file name within the data directory.
    /// </summary>
    /// <typeparam name="T">The type of object to save.</typeparam>
    /// <param name="fileName">The file name (e.g., "settings.json"). Relative to the data directory.</param>
    /// <param name="data">The object to serialize and save.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task SaveAsync<T>(string fileName, T data, CancellationToken cancellationToken = default);

    /// <summary>
    /// Loads an object from a JSON file within the data directory.
    /// </summary>
    /// <typeparam name="T">The type of object to deserialize.</typeparam>
    /// <param name="fileName">The file name (e.g., "settings.json"). Relative to the data directory.</param>
    /// <returns>The deserialized object, or the default value if the file does not exist.</returns>
    T? Load<T>(string fileName);

    /// <summary>
    /// Loads an object from a JSON file asynchronously within the data directory.
    /// </summary>
    /// <typeparam name="T">The type of object to deserialize.</typeparam>
    /// <param name="fileName">The file name (e.g., "settings.json"). Relative to the data directory.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The deserialized object, or the default value if the file does not exist.</returns>
    Task<T?> LoadAsync<T>(string fileName, CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks whether a file exists in the data directory.
    /// </summary>
    /// <param name="fileName">The file name to check.</param>
    /// <returns>True if the file exists.</returns>
    bool Exists(string fileName);

    /// <summary>
    /// Deletes a file from the data directory if it exists.
    /// </summary>
    /// <param name="fileName">The file name to delete.</param>
    /// <returns>True if the file was deleted, false if it did not exist.</returns>
    bool Delete(string fileName);
}
