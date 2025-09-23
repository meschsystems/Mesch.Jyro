namespace Mesch.Jyro.CodeAnalysis;

/// <summary>
/// Contains information about structural patterns and characteristics identified during code analysis.
/// This class aggregates various pattern-based insights including variable usage, function calls,
/// operator frequency, control flow patterns, and data flow characteristics.
/// </summary>
public sealed class CodePatterns
{
    /// <summary>
    /// Initializes a new instance of the <see cref="CodePatterns"/> class with the specified pattern data.
    /// </summary>
    /// <param name="mostUsedVariableNames">The most frequently used variable names in order of usage frequency.</param>
    /// <param name="mostCalledFunctions">The most frequently called functions in order of call frequency.</param>
    /// <param name="operatorFrequency">A dictionary mapping operator types to their usage frequency.</param>
    /// <param name="controlFlowPatterns">The control flow patterns identified in the code.</param>
    /// <param name="dataFlowCharacteristics">Characteristics describing data flow and variable mutation patterns.</param>
    /// <exception cref="ArgumentNullException">Thrown when dataFlowCharacteristics is null.</exception>
    public CodePatterns(
        IReadOnlyList<string> mostUsedVariableNames,
        IReadOnlyList<string> mostCalledFunctions,
        IReadOnlyDictionary<string, int> operatorFrequency,
        IReadOnlyList<ControlFlowPattern> controlFlowPatterns,
        DataFlowCharacteristics dataFlowCharacteristics)
    {
        MostUsedVariableNames = mostUsedVariableNames ?? Array.Empty<string>();
        MostCalledFunctions = mostCalledFunctions ?? Array.Empty<string>();
        OperatorFrequency = operatorFrequency ?? new Dictionary<string, int>();
        ControlFlowPatterns = controlFlowPatterns ?? Array.Empty<ControlFlowPattern>();
        DataFlowCharacteristics = dataFlowCharacteristics ?? throw new ArgumentNullException(nameof(dataFlowCharacteristics));
    }

    /// <summary>
    /// Gets the most frequently used variable names in the analyzed code, ordered by usage frequency.
    /// This can help identify variables that might benefit from better naming or refactoring opportunities.
    /// </summary>
    public IReadOnlyList<string> MostUsedVariableNames { get; }

    /// <summary>
    /// Gets the most frequently called functions in the analyzed code, ordered by call frequency.
    /// Functions with high call frequency may be candidates for performance optimization or caching.
    /// </summary>
    public IReadOnlyList<string> MostCalledFunctions { get; }

    /// <summary>
    /// Gets the frequency of different operators used in the code, mapped by operator type.
    /// This provides insights into the computational patterns and complexity of operations.
    /// </summary>
    public IReadOnlyDictionary<string, int> OperatorFrequency { get; }

    /// <summary>
    /// Gets the control flow patterns identified in the code, including their frequency and descriptions.
    /// These patterns help understand the structural complexity and branching behavior of the code.
    /// </summary>
    public IReadOnlyList<ControlFlowPattern> ControlFlowPatterns { get; }

    /// <summary>
    /// Gets characteristics about data flow in the code, including variable access patterns and mutation behaviors.
    /// This information helps identify potential issues like unused variables or excessive mutation.
    /// </summary>
    public DataFlowCharacteristics DataFlowCharacteristics { get; }
}