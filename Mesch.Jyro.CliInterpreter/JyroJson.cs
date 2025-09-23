using System.Text.Json;

namespace Mesch.Jyro.CliInterpreter;

/// <summary>
/// Provides convenience methods for loading and saving Jyro values from JSON sources.
/// </summary>
public static class JyroJson
{
    /// <summary>
    /// Loads a <see cref="JyroValue"/> from a JSON file.
    /// </summary>
    /// <param name="path">The path to the JSON file.</param>
    /// <param name="options">Optional JSON serializer options.</param>
    /// <returns>A <see cref="JyroValue"/> representing the parsed JSON data.</returns>
    /// <exception cref="ArgumentException">Thrown when the path is null or empty.</exception>
    /// <exception cref="FileNotFoundException">Thrown when the file does not exist.</exception>
    /// <exception cref="JsonException">Thrown when the JSON is malformed.</exception>
    public static JyroValue LoadFile(string path, JsonSerializerOptions? options = null)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            throw new ArgumentException("Path cannot be null or empty.", nameof(path));
        }

        if (!File.Exists(path))
        {
            throw new FileNotFoundException($"JSON file not found: {path}", path);
        }

        var json = File.ReadAllText(path);
        return JyroValue.FromJson(json, options);
    }

    /// <summary>
    /// Asynchronously loads a <see cref="JyroValue"/> from a JSON file.
    /// </summary>
    /// <param name="path">The path to the JSON file.</param>
    /// <param name="options">Optional JSON serializer options.</param>
    /// <param name="cancellationToken">Cancellation token for the operation.</param>
    /// <returns>A task that represents the asynchronous operation, containing a <see cref="JyroValue"/>.</returns>
    /// <exception cref="ArgumentException">Thrown when the path is null or empty.</exception>
    /// <exception cref="FileNotFoundException">Thrown when the file does not exist.</exception>
    /// <exception cref="JsonException">Thrown when the JSON is malformed.</exception>
    public static async Task<JyroValue> LoadFileAsync(string path, JsonSerializerOptions? options = null, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            throw new ArgumentException("Path cannot be null or empty.", nameof(path));
        }

        if (!File.Exists(path))
        {
            throw new FileNotFoundException($"JSON file not found: {path}", path);
        }

        var json = await File.ReadAllTextAsync(path, cancellationToken);
        return JyroValue.FromJson(json, options);
    }

    /// <summary>
    /// Saves a <see cref="JyroValue"/> to a JSON file.
    /// </summary>
    /// <param name="value">The value to serialize.</param>
    /// <param name="path">The output file path.</param>
    /// <param name="options">Optional JSON serializer options.</param>
    /// <exception cref="ArgumentNullException">Thrown when value or path is null.</exception>
    /// <exception cref="ArgumentException">Thrown when the path is empty.</exception>
    public static void SaveFile(JyroValue value, string path, JsonSerializerOptions? options = null)
    {
        if (value == null)
        {
            throw new ArgumentNullException(nameof(value));
        }

        if (string.IsNullOrWhiteSpace(path))
        {
            throw new ArgumentException("Path cannot be null or empty.", nameof(path));
        }

        var directory = Path.GetDirectoryName(path);
        if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }

        var json = value.ToJson(options);
        File.WriteAllText(path, json);
    }

    /// <summary>
    /// Asynchronously saves a <see cref="JyroValue"/> to a JSON file.
    /// </summary>
    /// <param name="value">The value to serialize.</param>
    /// <param name="path">The output file path.</param>
    /// <param name="options">Optional JSON serializer options.</param>
    /// <param name="cancellationToken">Cancellation token for the operation.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    /// <exception cref="ArgumentNullException">Thrown when value or path is null.</exception>
    /// <exception cref="ArgumentException">Thrown when the path is empty.</exception>
    public static async Task SaveFileAsync(JyroValue value, string path, JsonSerializerOptions? options = null, CancellationToken cancellationToken = default)
    {
        if (value == null)
        {
            throw new ArgumentNullException(nameof(value));
        }

        if (string.IsNullOrWhiteSpace(path))
        {
            throw new ArgumentException("Path cannot be null or empty.", nameof(path));
        }

        var directory = Path.GetDirectoryName(path);
        if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }

        var json = value.ToJson(options);
        await File.WriteAllTextAsync(path, json, cancellationToken);
    }
}