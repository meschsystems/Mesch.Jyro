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

    // Add these properties to expose intermediate compilation results
    private IReadOnlyList<IJyroStatement>? _lastParsedStatements;
    private JyroValidationResult? _lastValidationResult;
    private JyroLinkingResult? _lastLinkingResult;

    /// <summary>
    /// Gets the AST statements from the last parsing operation, if available.
    /// This is populated after the first call to Run() or when explicitly parsed.
    /// </summary>
    public IReadOnlyList<IJyroStatement>? ParsedStatements => _lastParsedStatements;

    /// <summary>
    /// Gets the validation result from the last validation operation, if available.
    /// </summary>
    public JyroValidationResult? LastValidationResult => _lastValidationResult;

    /// <summary>
    /// Gets the linking result from the last linking operation, if available.
    /// </summary>
    public JyroLinkingResult? LastLinkingResult => _lastLinkingResult;

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
            new UpperFunction(),
            new LowerFunction(),
            new TrimFunction(),
            new ReplaceFunction(),
            new ContainsFunction(),
            new StartsWithFunction(),
            new EndsWithFunction(),
            new SplitFunction(),
            new JoinFunction(),
            
            // Array manipulation functions
            new LengthFunction(),
            new AppendFunction(),
            new RemoveLastFunction(),
            new MergeArraysFunction(),
            new SortFunction(),
            new SortByFieldFunction(),
            new ReverseFunction(),
            new InsertFunction(),
            new RemoveAtFunction(),
            new ClearFunction(),
            
            // Mathematical functions
            new MinFunction(),
            new MaxFunction(),
            new SumFunction(),
            new AbsFunction(),
            new RoundFunction(),
            
            // Utility functions
            new EqualFunction(),
            new NotEqualFunction(),
            new TypeOfFunction(),
            new ExistsFunction(),
            new IsNullFunction(),
            
            // Date and time functions
            new NowFunction(),
            new TodayFunction(),
            new ParseDateFunction(),
            new FormatDateFunction(),
            new DateAddFunction(),
            new DateDiffFunction(),
            new DatePartFunction()
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
    /// Executes the configured script through the complete compilation and execution pipeline.
    /// This method performs lexical analysis, parsing, validation, linking, and execution in sequence.
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

        var lexer = new Lexer(_loggerFactory.CreateLogger<Lexer>());
        var parser = new Parser(_loggerFactory.CreateLogger<Parser>());
        var linker = new Linker(_loggerFactory.CreateLogger<Linker>());
        var executor = new Executor(_loggerFactory.CreateLogger<Executor>());
        var builderLogger = _loggerFactory.CreateLogger<JyroBuilder>();

        // Build available function names for validation
        var availableFunctionNames = new HashSet<string>(StringComparer.Ordinal) { "Data" };
        foreach (var function in _hostFunctions)
        {
            availableFunctionNames.Add(function.Signature.Name);
        }

        var validator = new Validator(_loggerFactory.CreateLogger<Validator>(), availableFunctionNames);
        var jyroPipeline = new Jyro(lexer, parser, validator, linker, executor);

        // Execute lexical analysis
        var lexingResult = jyroPipeline.Lex(_scriptSource);
        if (!lexingResult.IsSuccessful)
        {
            return new JyroExecutionResult(false, _data, lexingResult.Messages,
                new ExecutionMetadata(lexingResult.Metadata.ProcessingTime, 0, 0, 0, 0, DateTimeOffset.UtcNow));
        }

        // Execute parsing
        var parsingResult = jyroPipeline.Parse(lexingResult.Tokens);
        _lastParsedStatements = parsingResult.Statements; // Store for potential analysis
        if (!parsingResult.IsSuccessful)
        {
            return new JyroExecutionResult(false, _data, parsingResult.Messages,
                new ExecutionMetadata(parsingResult.Metadata.ProcessingTime, 0, 0, 0, 0, DateTimeOffset.UtcNow));
        }

        // Execute validation
        _lastValidationResult = validator.Validate(parsingResult.Statements);
        if (!_lastValidationResult.IsSuccessful)
        {
            return new JyroExecutionResult(false, _data, _lastValidationResult.Messages,
                new ExecutionMetadata(_lastValidationResult.Metadata.ProcessingTime, 0, 0, 0, 0, DateTimeOffset.UtcNow));
        }

        // Execute linking
        _lastLinkingResult = linker.Link(parsingResult.Statements, _hostFunctions);
        if (!_lastLinkingResult.IsSuccessful || _lastLinkingResult.Program is null)
        {
            return new JyroExecutionResult(false, _data, _lastLinkingResult.Messages,
                new ExecutionMetadata(_lastLinkingResult.Metadata.ProcessingTime, 0, 0, 0, 0, DateTimeOffset.UtcNow));
        }

        // Execute the linked program
        return jyroPipeline.ExecuteLinkedProgram(_lastLinkingResult.Program, _data, _options, _resolver, cancellationToken);
    }

    /// <summary>
    /// Parses the configured script without executing it, making the AST available for analysis.
    /// </summary>
    public JyroBuilder Parse()
    {
        if (_scriptSource is null)
        {
            throw new InvalidOperationException("Script source must be configured before parsing.");
        }

        var lexer = new Lexer(_loggerFactory.CreateLogger<Lexer>());
        var parser = new Parser(_loggerFactory.CreateLogger<Parser>());

        var lexingResult = lexer.Tokenize(_scriptSource);
        if (!lexingResult.IsSuccessful)
        {
            throw new InvalidOperationException($"Lexing failed: {string.Join(", ", lexingResult.Messages)}");
        }

        var parsingResult = parser.Parse(lexingResult.Tokens);
        if (!parsingResult.IsSuccessful)
        {
            throw new InvalidOperationException($"Parsing failed: {string.Join(", ", parsingResult.Messages)}");
        }

        _lastParsedStatements = parsingResult.Statements;
        return this;
    }
}
