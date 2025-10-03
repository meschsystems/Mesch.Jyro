using System.Diagnostics;
using Antlr4.Build.Tasks;
using Antlr4.CodeGenerator;
using Microsoft.Extensions.Logging;

namespace Mesch.Jyro;

public sealed class Linker
{
    private readonly ILogger<Linker> _logger;

    public Linker(ILogger<Linker> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public JyroLinkingResult Link(
        JyroParser.ProgramContext programContext,
        IEnumerable<IJyroFunction>? hostFunctions = null)
    {
        ArgumentNullException.ThrowIfNull(programContext);

        var linkingStartedAt = DateTimeOffset.UtcNow;
        var linkingStopwatch = Stopwatch.StartNew();
        var diagnosticMessages = new List<IMessage>();

        try
        {
            _logger.LogTrace("Beginning linking process");

            var availableFunctions = BuildFunctionRegistry(hostFunctions, diagnosticMessages);
            var collectedReferences = CollectFunctionReferences(programContext);
            ValidateFunctionSignatures(collectedReferences, availableFunctions, diagnosticMessages);

            _logger.LogTrace("Constructing linked program with {FunctionCount} available functions", availableFunctions.Count);
            var linkedProgram = new LinkedProgram(programContext, availableFunctions);

            linkingStopwatch.Stop();
            var linkingMetadata = new LinkingMetadata(linkingStopwatch.Elapsed, availableFunctions.Count, linkingStartedAt);

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
            var fallbackMetadata = new LinkingMetadata(linkingStopwatch.Elapsed, 0, linkingStartedAt);

            return new JyroLinkingResult(false, null, diagnosticMessages, fallbackMetadata);
        }
    }

    private Dictionary<string, IJyroFunction> BuildFunctionRegistry(
        IEnumerable<IJyroFunction>? hostFunctions,
        List<IMessage> diagnosticMessages)
    {
        var functionRegistry = new Dictionary<string, IJyroFunction>(StringComparer.Ordinal);

        if (hostFunctions != null)
        {
            foreach (var hostFunction in hostFunctions)
            {
                if (functionRegistry.ContainsKey(hostFunction.Signature.Name))
                {
                    _logger.LogTrace("Host function override detected: {FunctionName}", hostFunction.Signature.Name);
                    diagnosticMessages.Add(new Message(
                        MessageCode.FunctionOverride,
                        0, 0,
                        MessageSeverity.Warning,
                        ProcessingStage.Linking,
                        hostFunction.Signature.Name));
                }

                functionRegistry[hostFunction.Signature.Name] = hostFunction;
            }
        }

        _logger.LogTrace("Function registry built with {TotalFunctions} total functions", functionRegistry.Count);
        return functionRegistry;
    }

    private HashSet<FunctionReference> CollectFunctionReferences(JyroParser.ProgramContext programContext)
    {
        var collector = new FunctionReferenceCollector();
        var references = collector.CollectReferences(programContext);

        _logger.LogTrace("Collected {ReferenceCount} function references", references.Count);
        return references;
    }

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

        _logger.LogTrace("Signature validation completed: {MissingCount} missing functions", missingFunctionCount);
    }
}