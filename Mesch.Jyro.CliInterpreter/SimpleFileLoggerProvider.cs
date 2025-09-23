using System.Text;
using Microsoft.Extensions.Logging;

namespace Mesch.Jyro.CliInterpreter;

/// <summary>
/// Provides a simple file-based logger implementation for the Jyro console application.
/// </summary>
internal sealed class SimpleFileLoggerProvider : ILoggerProvider
{
    private readonly StreamWriter _writer;
    private bool _disposed;

    /// <summary>
    /// Initializes a new instance of the <see cref="SimpleFileLoggerProvider"/> class.
    /// </summary>
    /// <param name="path">The file path for log output.</param>
    /// <exception cref="ArgumentException">Thrown when the path is null or empty.</exception>
    public SimpleFileLoggerProvider(string path)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            throw new ArgumentException("Log file path cannot be null or empty.", nameof(path));
        }

        var directory = Path.GetDirectoryName(Path.GetFullPath(path));
        if (!string.IsNullOrEmpty(directory))
        {
            Directory.CreateDirectory(directory);
        }

        var stream = new FileStream(path, FileMode.Append, FileAccess.Write, FileShare.Read);
        _writer = new StreamWriter(stream, new UTF8Encoding(encoderShouldEmitUTF8Identifier: false))
        {
            AutoFlush = true
        };
    }

    /// <summary>
    /// Creates a logger instance for the specified category.
    /// </summary>
    /// <param name="categoryName">The category name for the logger.</param>
    /// <returns>A logger instance.</returns>
    public ILogger CreateLogger(string categoryName) => new SimpleFileLogger(categoryName, _writer);

    /// <summary>
    /// Releases all resources used by the logger provider.
    /// </summary>
    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _disposed = true;
        _writer.Dispose();
    }
}

/// <summary>
/// A simple file-based logger implementation.
/// </summary>
internal sealed class SimpleFileLogger : ILogger
{
    private readonly string _category;
    private readonly StreamWriter _writer;

    /// <summary>
    /// Initializes a new instance of the <see cref="SimpleFileLogger"/> class.
    /// </summary>
    /// <param name="category">The logger category.</param>
    /// <param name="writer">The stream writer for output.</param>
    public SimpleFileLogger(string category, StreamWriter writer)
    {
        _category = category ?? throw new ArgumentNullException(nameof(category));
        _writer = writer ?? throw new ArgumentNullException(nameof(writer));
    }

    /// <summary>
    /// Begins a logical operation scope. Not implemented for file logging.
    /// </summary>
    /// <typeparam name="TState">The type of the state object.</typeparam>
    /// <param name="state">The state object.</param>
    /// <returns>A disposable scope object.</returns>
    IDisposable ILogger.BeginScope<TState>(TState state) => NullScope.Instance;

    /// <summary>
    /// Checks if the given log level is enabled.
    /// </summary>
    /// <param name="logLevel">The log level to check.</param>
    /// <returns>Always returns true as filtering is handled by the logging framework.</returns>
    public bool IsEnabled(LogLevel logLevel) => true;

    /// <summary>
    /// Writes a log entry to the file.
    /// </summary>
    /// <typeparam name="TState">The type of the state object.</typeparam>
    /// <param name="logLevel">The log level.</param>
    /// <param name="eventId">The event identifier.</param>
    /// <param name="state">The state object.</param>
    /// <param name="exception">The exception to log.</param>
    /// <param name="formatter">The formatter function.</param>
    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
    {
        if (formatter == null)
        {
            return;
        }

        var line = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} [{logLevel}] {_category}: {formatter(state, exception)}";

        lock (_writer)
        {
            _writer.WriteLine(line);
            if (exception != null)
            {
                _writer.WriteLine(exception.ToString());
            }
        }
    }

    /// <summary>
    /// Null scope implementation for the simple file logger.
    /// </summary>
    private sealed class NullScope : IDisposable
    {
        /// <summary>
        /// Gets the singleton instance of the null scope.
        /// </summary>
        public static readonly NullScope Instance = new();

        /// <summary>
        /// Does nothing when disposed.
        /// </summary>
        public void Dispose()
        {
            // No operation required
        }
    }
}