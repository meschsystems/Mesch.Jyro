using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace Mesch.Jyro;

/// <summary>
/// Provides a fluent builder interface for configuring and executing Jyro scripts.
/// This class offers a simplified, one-stop entry point for host applications that
/// want to execute Jyro scripts without manually orchestrating the compilation pipeline.
/// </summary>
/// <remarks>
/// The JyroBuilder follows the Builder pattern, allowing for method chaining to
/// configure script source, data context, execution options, and host functions.
/// Once configured, the entire compilation and execution pipeline is run automatically.
/// </remarks>
public sealed partial class JyroBuilder
{
    private string? _scriptSource;
    private JyroValue? _data;
    private JyroExecutionOptions _options = new();
    private JyroScriptResolver? _resolver;
    private readonly List<IJyroFunction> _hostFunctions = [];
    private readonly ILoggerFactory _loggerFactory;

    // Parsing result from the last Parse() operation
    private JyroParsingResult? _lastParsingResult;

    /// <summary>
    /// Gets the parsing result from the last parsing operation, if available.
    /// </summary>
    public JyroParsingResult? LastParsingResult => _lastParsingResult;

    /// <summary>
    /// Initializes a new instance of the <see cref="JyroBuilder"/> class with the specified logger factory.
    /// </summary>
    /// <param name="loggerFactory">The logger factory to use for creating component loggers.</param>
    /// <exception cref="ArgumentNullException">Thrown when loggerFactory is null.</exception>
    public JyroBuilder(ILoggerFactory loggerFactory)
    {
        _loggerFactory = loggerFactory ?? throw new ArgumentNullException(nameof(loggerFactory));
    }

    /// <summary>
    /// Creates a new JyroBuilder instance with the specified logger factory.
    /// If no logger factory is provided, a null logger factory is used.
    /// </summary>
    /// <param name="loggerFactory">
    /// Optional logger factory for creating component loggers. If null, no logging will be performed.
    /// </param>
    /// <returns>A new JyroBuilder instance ready for configuration.</returns>
    public static JyroBuilder Create(ILoggerFactory? loggerFactory = null) =>
        new JyroBuilder(loggerFactory ?? NullLoggerFactory.Instance);

    /// <summary>
    /// Configures the Jyro script source code to be compiled and executed.
    /// </summary>
    /// <param name="source">The Jyro script source code.</param>
    /// <returns>This builder instance for method chaining.</returns>
    /// <exception cref="ArgumentNullException">Thrown when source is null.</exception>
    public JyroBuilder WithScript(string source)
    {
        _scriptSource = source ?? throw new ArgumentNullException(nameof(source));
        return this;
    }

    /// <summary>
    /// Configures the initial data context that the script will operate on.
    /// This data serves as the root object and can be modified during script execution.
    /// </summary>
    /// <param name="data">The initial data context for script execution.</param>
    /// <returns>This builder instance for method chaining.</returns>
    /// <exception cref="ArgumentNullException">Thrown when data is null.</exception>
    public JyroBuilder WithData(JyroValue data)
    {
        _data = data ?? throw new ArgumentNullException(nameof(data));
        return this;
    }

    /// <summary>
    /// Configures the execution options including resource limits and runtime settings.
    /// </summary>
    /// <param name="options">The execution options to use.</param>
    /// <returns>This builder instance for method chaining.</returns>
    /// <exception cref="ArgumentNullException">Thrown when options is null.</exception>
    public JyroBuilder WithOptions(JyroExecutionOptions options)
    {
        _options = options ?? throw new ArgumentNullException(nameof(options));
        return this;
    }

    /// <summary>
    /// Configures a script resolver for dynamic script loading during execution.
    /// This enables modular script architectures and runtime script inclusion.
    /// </summary>
    /// <param name="resolver">The script resolver to use for dynamic script loading.</param>
    /// <returns>This builder instance for method chaining.</returns>
    /// <exception cref="ArgumentNullException">Thrown when resolver is null.</exception>
    public JyroBuilder WithResolver(JyroScriptResolver resolver)
    {
        _resolver = resolver ?? throw new ArgumentNullException(nameof(resolver));
        return this;
    }

    /// <summary>
    /// Configures the script environment with a comprehensive standard library of commonly used functions.
    /// This includes string manipulation, array operations, mathematical functions, utilities, and date/time operations.
    /// </summary>
    /// <returns>This builder instance for method chaining.</returns>
    /// <remarks>
    /// The standard library includes the following function categories:
    /// <list type="bullet">
    /// <item><description>String functions: upper, lower, trim, replace, contains, startsWith, endsWith, split, join</description></item>
    /// <item><description>Array functions: length, append, appendWithFields, removeLast, mergeArrays, sort, sortByField, reverse, insert, removeAt, clear</description></item>
    /// <item><description>Math functions: min, max, sum, abs, round</description></item>
    /// <item><description>Utility functions: equal, notEqual, typeOf, exists, isNull</description></item>
    /// <item><description>Date functions: now, today, parseDate, formatDate, dateAdd, dateDiff, datePart</description></item>
    /// </list>
    /// </remarks>
    public JyroBuilder WithStandardLibrary()
    {
        var standardFunctions = new IJyroFunction[]
        {
            // String manipulation functions
            new ContainsFunction(),
            new EndsWithFunction(),
            new JoinFunction(),
            new LowerFunction(),
            new ReplaceFunction(),
            new SplitFunction(),
            new StartsWithFunction(),
            new TrimFunction(),
            new UpperFunction(),

            // Array manipulation functions
            new AppendFunction(),
            new ClearFunction(),
            new IndexOfFunction(),
            new InsertFunction(),
            new LengthFunction(),
            new MergeArraysFunction(),
            new RemoveAtFunction(),
            new RemoveLastFunction(),
            new ReverseFunction(),
            new SortByFieldFunction(),
            new SortFunction(),

            // Mathematical functions
            new AbsFunction(),
            new MaxFunction(),
            new MinFunction(),
            new RoundFunction(),
            new SumFunction(),

            // Utility functions
            new CallScriptFunction(),
            new EqualFunction(),
            new ExistsFunction(),
            new IsNullFunction(),
            new NewGuidFunction(),
            new Base64EncodeFunction(),
            new NotEqualFunction(),
            new TypeOfFunction(),

            // Date and time functions
            new DateAddFunction(),
            new DateDiffFunction(),
            new DatePartFunction(),
            new FormatDateFunction(),
            new NowFunction(),
            new ParseDateFunction(),
            new TodayFunction()
        };

        foreach (var function in standardFunctions)
        {
            _hostFunctions.Add(function);
        }

        return this;
    }

    /// <summary>
    /// Adds a custom host function to the script execution environment.
    /// Host functions extend the script's capabilities by providing access to host application functionality.
    /// </summary>
    /// <param name="function">The host function to add to the execution environment.</param>
    /// <returns>This builder instance for method chaining.</returns>
    /// <exception cref="ArgumentNullException">Thrown when function is null.</exception>
    public JyroBuilder WithFunction(IJyroFunction function)
    {
        ArgumentNullException.ThrowIfNull(function);
        _hostFunctions.Add(function);
        return this;
    }

    /// <summary>
    /// Enables REST API functionality in Jyro scripts, providing the InvokeRestMethod() function.
    /// This is an opt-in feature for security reasons, as it allows scripts to make outbound HTTP requests.
    /// </summary>
    /// <param name="options">
    /// Optional security configuration for REST API calls. If not provided, default safe settings are used.
    /// Configure URL allow/deny lists, size limits, timeouts, and allowed HTTP methods.
    /// </param>
    /// <returns>This builder instance for method chaining.</returns>
    /// <remarks>
    /// <para>
    /// The InvokeRestMethod() function is inspired by PowerShell's Invoke-RestMethod cmdlet and provides
    /// the following capabilities:
    /// </para>
    /// <list type="bullet">
    /// <item><description>Execute HTTP requests (GET, POST, PUT, PATCH, DELETE, etc.)</description></item>
    /// <item><description>Send custom headers and request bodies</description></item>
    /// <item><description>Automatic JSON serialization/deserialization</description></item>
    /// <item><description>Response status codes, headers, and content</description></item>
    /// </list>
    /// <para>
    /// Security controls include:
    /// </para>
    /// <list type="bullet">
    /// <item><description>URL allowlist/denylist using regex patterns</description></item>
    /// <item><description>Maximum request/response size limits</description></item>
    /// <item><description>Concurrent request limits</description></item>
    /// <item><description>Request timeout controls</description></item>
    /// <item><description>HTTP method restrictions</description></item>
    /// </list>
    /// <para>
    /// Example usage in Jyro:
    /// <code>
    /// var result = InvokeRestMethod("https://api.example.com/data", "GET");
    /// if (result.isSuccessStatusCode) {
    ///     Data = result.content;
    /// }
    ///
    /// var postResult = InvokeRestMethod(
    ///     "https://api.example.com/items",
    ///     "POST",
    ///     { "Content-Type": "application/json", "Authorization": "Bearer token123" },
    ///     { "name": "New Item", "value": 42 }
    /// );
    /// </code>
    /// </para>
    /// </remarks>
    /// <example>
    /// Enable REST API with default settings:
    /// <code>
    /// var result = JyroBuilder.Create()
    ///     .WithScript("var response = InvokeRestMethod('https://api.example.com/data', 'GET');")
    ///     .WithData(new JyroObject())
    ///     .WithRestApi()
    ///     .Run();
    /// </code>
    ///
    /// Enable REST API with custom security settings:
    /// <code>
    /// var restOptions = new RestApiOptions
    /// {
    ///     AllowedUrlPatterns = new List&lt;Regex&gt;
    ///     {
    ///         new Regex(@"^https://api\.example\.com/", RegexOptions.IgnoreCase)
    ///     },
    ///     MaxRequestBodySize = 512_000, // 500KB
    ///     MaxResponseSize = 5_242_880,  // 5MB
    ///     RequestTimeout = TimeSpan.FromSeconds(15)
    /// };
    ///
    /// var result = JyroBuilder.Create()
    ///     .WithScript(script)
    ///     .WithData(data)
    ///     .WithRestApi(restOptions)
    ///     .Run();
    /// </code>
    ///
    /// Enable for local development only:
    /// <code>
    /// var result = JyroBuilder.Create()
    ///     .WithScript(script)
    ///     .WithData(data)
    ///     .WithRestApi(RestApiOptions.CreateForLocalDevelopment())
    ///     .Run();
    /// </code>
    /// </example>
    public JyroBuilder WithRestApi(RestApiOptions? options = null)
    {
        options ??= RestApiOptions.CreateDefault();
        _hostFunctions.Add(new InvokeRestMethodFunction(options));
        return this;
    }

    /// <summary>
    /// Executes the configured script through the complete compilation and execution pipeline.
    /// This method performs parsing, validation, linking, and execution in sequence using ANTLR.
    /// </summary>
    /// <param name="cancellationToken">
    /// Optional cancellation token for cooperative cancellation of long-running operations.
    /// </param>
    /// <returns>
    /// A <see cref="JyroExecutionResult"/> containing the final data state, diagnostic messages,
    /// and execution metadata from the complete pipeline execution.
    /// </returns>
    /// <exception cref="InvalidOperationException">
    /// Thrown when either script source or data has not been configured before calling Run.
    /// </exception>
    public JyroExecutionResult Run(CancellationToken cancellationToken = default)
    {
        if (_scriptSource is null)
        {
            throw new InvalidOperationException("Script source must be configured before execution.");
        }
        if (_data is null)
        {
            throw new InvalidOperationException("Data context must be configured before execution.");
        }

        // Create ANTLR-based pipeline components
        var validator = new Validator(_loggerFactory.CreateLogger<Validator>(), new[] { "Data" });
        var linker = new Linker(_loggerFactory.CreateLogger<Linker>());
        var interpreter = new Interpreter();
        var jyroPipeline = new Jyro(validator, linker, interpreter);

        // Use the convenience Run method that does all stages
        return jyroPipeline.Run(_scriptSource, _data, _options, _hostFunctions, _resolver, cancellationToken);
    }

    /// <summary>
    /// Parses the configured script without executing it, making the parse tree available for analysis.
    /// </summary>
    public JyroBuilder Parse()
    {
        if (_scriptSource is null)
        {
            throw new InvalidOperationException("Script source must be configured before parsing.");
        }

        var validator = new Validator(_loggerFactory.CreateLogger<Validator>(), new[] { "Data" });
        var linker = new Linker(_loggerFactory.CreateLogger<Linker>());
        var interpreter = new Interpreter();
        var jyroPipeline = new Jyro(validator, linker, interpreter);

        _lastParsingResult = jyroPipeline.Parse(_scriptSource);
        if (!_lastParsingResult.IsSuccessful)
        {
            throw new InvalidOperationException($"Parsing failed: {string.Join(", ", _lastParsingResult.Messages)}");
        }

        return this;
    }
}
