namespace Mesch.Jyro.CodeAnalysis;

/// <summary>
/// Represents characteristics of data flow within analyzed code, including variable usage patterns
/// and mutation behaviors that affect code maintainability and performance.
/// </summary>
public sealed class DataFlowCharacteristics
{
    /// <summary>
    /// Initializes a new instance of the <see cref="DataFlowCharacteristics"/> class.
    /// </summary>
    /// <param name="readOnlyVariables">The number of variables that are only read from, never assigned.</param>
    /// <param name="writeOnlyVariables">The number of variables that are only assigned to, never read.</param>
    /// <param name="readWriteVariables">The number of variables that are both read from and assigned to.</param>
    /// <param name="variableReuseRatio">The ratio of variables used multiple times (0.0 to 1.0).</param>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when variable counts are negative or ratio is outside valid range.</exception>
    public DataFlowCharacteristics(
        int readOnlyVariables,
        int writeOnlyVariables,
        int readWriteVariables,
        double variableReuseRatio)
    {
        if (readOnlyVariables < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(readOnlyVariables), "Read-only variable count cannot be negative.");
        }

        if (writeOnlyVariables < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(writeOnlyVariables), "Write-only variable count cannot be negative.");
        }

        if (readWriteVariables < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(readWriteVariables), "Read-write variable count cannot be negative.");
        }

        if (variableReuseRatio < 0.0 || variableReuseRatio > 1.0)
        {
            throw new ArgumentOutOfRangeException(nameof(variableReuseRatio), "Variable reuse ratio must be between 0.0 and 1.0.");
        }

        ReadOnlyVariables = readOnlyVariables;
        WriteOnlyVariables = writeOnlyVariables;
        ReadWriteVariables = readWriteVariables;
        VariableReuseRatio = variableReuseRatio;
    }

    /// <summary>
    /// Gets the number of variables that are only read from but never assigned to after their initial declaration.
    /// High numbers may indicate good immutability practices or potential for optimization.
    /// </summary>
    public int ReadOnlyVariables { get; }

    /// <summary>
    /// Gets the number of variables that are assigned to but never read from.
    /// These typically represent dead code or unused computations that should be removed.
    /// </summary>
    public int WriteOnlyVariables { get; }

    /// <summary>
    /// Gets the number of variables that are both read from and assigned to during their lifecycle.
    /// These represent mutable state that may impact code complexity and debugging.
    /// </summary>
    public int ReadWriteVariables { get; }

    /// <summary>
    /// Gets the ratio of variables that are referenced multiple times versus single-use variables.
    /// Values closer to 1.0 indicate higher variable reuse, which may suggest good abstraction
    /// or potentially over-complicated logic depending on context.
    /// </summary>
    public double VariableReuseRatio { get; }

    /// <summary>
    /// Gets the total number of variables tracked in this analysis.
    /// </summary>
    public int TotalVariables => ReadOnlyVariables + WriteOnlyVariables + ReadWriteVariables;

    /// <summary>
    /// Gets the percentage of variables that are effectively immutable (read-only).
    /// </summary>
    public double ImmutabilityPercentage => TotalVariables > 0
        ? (double)ReadOnlyVariables / TotalVariables
        : 0.0;

    /// <summary>
    /// Gets the percentage of variables that represent potentially dead code (write-only).
    /// </summary>
    public double DeadCodePercentage => TotalVariables > 0
        ? (double)WriteOnlyVariables / TotalVariables
        : 0.0;
}