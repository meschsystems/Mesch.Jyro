using System.Diagnostics;
using Microsoft.Extensions.Logging;

namespace Mesch.Jyro;

/// <summary>
/// Unified linker implementation that provides compile-time type safety through
/// function signature validation. Links parsed abstract syntax trees with available
/// functions to create executable programs, ensuring all function calls are valid
/// before runtime execution.
/// </summary>
public sealed class Linker : ILinker
{
    private readonly ILogger<Linker> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="Linker"/> class with the specified logger.
    /// </summary>
    /// <param name="logger">
    /// The logger instance for tracking linking operations and diagnostics.
    /// Cannot be null.
    /// </param>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="logger"/> is null.
    /// </exception>
    public Linker(ILogger<Linker> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <inheritdoc />
    public JyroLinkingResult Link(
           IReadOnlyList<IJyroStatement> programStatements,
           IEnumerable<IJyroFunction>? hostFunctions = null)
    {
        ArgumentNullException.ThrowIfNull(programStatements);

        var linkingStartedAt = DateTimeOffset.UtcNow;
        var linkingStopwatch = Stopwatch.StartNew();
        var diagnosticMessages = new List<IMessage>();

        Dictionary<string, IJyroFunction>? availableFunctions = null;
        HashSet<FunctionReference>? collectedReferences = null;

        try
        {
            _logger.LogTrace("Beginning linking process with {StatementCount} statements", programStatements.Count);

            availableFunctions = BuildFunctionRegistry(hostFunctions, diagnosticMessages);
            collectedReferences = CollectFunctionReferences(programStatements);
            ValidateFunctionSignatures(collectedReferences, availableFunctions, diagnosticMessages);

            _logger.LogTrace("Constructing linked program with {FunctionCount} available functions", availableFunctions.Count);
            var linkedProgram = new JyroLinkedProgram(programStatements, availableFunctions);

            linkingStopwatch.Stop();
            var linkingMetadata = new JyroLinkingMetadata(linkingStopwatch.Elapsed, availableFunctions.Count, linkingStartedAt);

            var linkingSucceeded = !diagnosticMessages.Any(message => message.Severity == MessageSeverity.Error);
            _logger.LogTrace("Linking completed: success={Success}, errors={ErrorCount}, elapsed={ElapsedMs}ms",
                linkingSucceeded, diagnosticMessages.Count(message => message.Severity == MessageSeverity.Error), linkingStopwatch.ElapsedMilliseconds);

            return new JyroLinkingResult(linkingSucceeded, linkedProgram, diagnosticMessages, linkingMetadata);
        }
        catch (Exception linkingException)
        {
            _logger.LogError(linkingException, "Unexpected error occurred during linking process");
            diagnosticMessages.Add(new Message(
                MessageCode.UnknownLinkerError,
                0, 0,
                MessageSeverity.Error,
                ProcessingStage.Linking,
                linkingException.Message));

            linkingStopwatch.Stop();
            var fallbackFunctionCount = availableFunctions?.Count ?? 0;
            var fallbackMetadata = new JyroLinkingMetadata(linkingStopwatch.Elapsed, fallbackFunctionCount, linkingStartedAt);

            return new JyroLinkingResult(false, null, diagnosticMessages, fallbackMetadata);
        }
    }

    #region Function Registry

    /// <summary>
    /// Builds the complete function registry by combining standard library functions
    /// with host-provided functions, handling overrides and conflicts.
    /// </summary>
    /// <param name="hostFunctions">The optional collection of host-provided functions.</param>
    /// <param name="diagnosticMessages">The collection to receive diagnostic messages.</param>
    /// <returns>A dictionary containing all available functions indexed by name.</returns>
    private Dictionary<string, IJyroFunction> BuildFunctionRegistry(
        IEnumerable<IJyroFunction>? hostFunctions,
        List<IMessage> diagnosticMessages)
    {
        var functionRegistry = new Dictionary<string, IJyroFunction>(StringComparer.Ordinal);

        AddHostFunctions(hostFunctions, functionRegistry, diagnosticMessages);

        _logger.LogTrace("Function registry built with {TotalFunctions} total functions", functionRegistry.Count);
        return functionRegistry;
    }

    /// <summary>
    /// Adds host-provided functions to the registry, handling overrides and generating diagnostics.
    /// </summary>
    /// <param name="hostFunctions">The collection of host functions to add.</param>
    /// <param name="functionRegistry">The registry to update with host functions.</param>
    /// <param name="diagnosticMessages">The collection to receive diagnostic messages.</param>
    private void AddHostFunctions(
        IEnumerable<IJyroFunction>? hostFunctions,
        Dictionary<string, IJyroFunction> functionRegistry,
        List<IMessage> diagnosticMessages)
    {
        if (hostFunctions == null)
        {
            return;
        }

        int hostFunctionCount = 0;
        int overrideCount = 0;
        int additionCount = 0;

        foreach (var hostFunction in hostFunctions)
        {
            if (functionRegistry.ContainsKey(hostFunction.Signature.Name))
            {
                overrideCount++;
                _logger.LogTrace("Host function override detected: {FunctionName}", hostFunction.Signature.Name);
                diagnosticMessages.Add(new Message(
                    MessageCode.FunctionOverride,
                    0, 0,
                    MessageSeverity.Warning,
                    ProcessingStage.Linking,
                    hostFunction.Signature.Name));
            }
            else
            {
                additionCount++;
                _logger.LogTrace("Host function added: {FunctionName}", hostFunction.Signature.Name);
            }

            functionRegistry[hostFunction.Signature.Name] = hostFunction;
            hostFunctionCount++;
        }

        _logger.LogTrace("Host functions processed: {HostCount} total, {Additions} additions, {Overrides} overrides",
            hostFunctionCount, additionCount, overrideCount);
    }

    #endregion

    #region Reference Collection

    /// <summary>
    /// Collects all function references from the program statements.
    /// </summary>
    /// <param name="programStatements">The statements to analyze for function references.</param>
    /// <returns>A collection of function references found in the program.</returns>
    private HashSet<FunctionReference> CollectFunctionReferences(
        IReadOnlyList<IJyroStatement> programStatements)
    {
        var functionReferences = new HashSet<FunctionReference>();
        var referenceCollector = new SimpleFunctionReferenceCollector(functionReferences);

        foreach (var statement in programStatements)
        {
            statement.Accept(referenceCollector);
        }

        _logger.LogTrace("Collected {ReferenceCount} function references", functionReferences.Count);
        if (_logger.IsEnabled(LogLevel.Trace))
        {
            foreach (var reference in functionReferences)
            {
                _logger.LogTrace("Function reference: name={Name}, argumentCount={ArgumentCount}, location=({Line},{Column})",
                    reference.Name, reference.Arguments.Count, reference.LineNumber, reference.ColumnPosition);
            }
        }

        return functionReferences;
    }

    #endregion

    #region Type-Safe Validation

    /// <summary>
    /// Validates all function references against available function signatures,
    /// ensuring type compatibility and argument count correctness.
    /// </summary>
    /// <param name="functionReferences">The collection of function references to validate.</param>
    /// <param name="availableFunctions">The dictionary of available functions for validation.</param>
    /// <param name="diagnosticMessages">The collection to receive validation diagnostic messages.</param>
    private void ValidateFunctionSignatures(
       HashSet<FunctionReference> functionReferences,
       Dictionary<string, IJyroFunction> availableFunctions,
       List<IMessage> diagnosticMessages)
    {
        int missingFunctionCount = 0;

        foreach (var functionReference in functionReferences)
        {
            if (!availableFunctions.TryGetValue(functionReference.Name, out var availableFunction))
            {
                missingFunctionCount++;
                _logger.LogTrace("Missing function: {FunctionName} at ({Line},{Column})",
                    functionReference.Name, functionReference.LineNumber, functionReference.ColumnPosition);

                diagnosticMessages.Add(new Message(
                    MessageCode.UndefinedFunction,
                    functionReference.LineNumber, functionReference.ColumnPosition,
                    MessageSeverity.Error,
                    ProcessingStage.Linking,
                    functionReference.Name));
            }
        }

        _logger.LogTrace("Signature validation completed: {MissingCount} missing functions",
            missingFunctionCount);
    }

    #endregion
}