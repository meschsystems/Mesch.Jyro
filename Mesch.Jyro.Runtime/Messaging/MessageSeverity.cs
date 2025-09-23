namespace Mesch.Jyro;

/// <summary>
/// Defines the severity levels of diagnostic messages produced during Jyro compilation and execution.
/// These severity levels enable proper categorization and filtering of diagnostic information,
/// allowing tools and users to focus on the most critical issues while maintaining visibility
/// into warnings and informational messages.
/// </summary>
/// <remarks>
/// The severity levels follow standard diagnostic practices found in compilers and development tools:
/// <list type="bullet">
/// <item><description>Info: Provides useful information without indicating problems</description></item>
/// <item><description>Warning: Indicates potential issues that don't prevent processing</description></item>
/// <item><description>Error: Indicates serious problems that prevent successful completion</description></item>
/// </list>
/// Processing typically continues after Info and Warning messages but stops after Error messages
/// to prevent cascading failures and invalid execution states.
/// </remarks>
public enum MessageSeverity
{
    /// <summary>
    /// Indicates an informational message that provides useful context or status information.
    /// Informational messages do not indicate problems and do not prevent processing from continuing.
    /// These messages are typically used for debugging, profiling, or providing insight into
    /// the compilation or execution process.
    /// </summary>
    Info,

    /// <summary>
    /// Indicates a warning message about a condition that may be problematic but does not prevent processing.
    /// Warning messages highlight potential issues, questionable practices, or conditions that might
    /// lead to unexpected behavior, but the compilation or execution can continue successfully.
    /// Examples include unused variables, unreachable code, or deprecated feature usage.
    /// </summary>
    Warning,

    /// <summary>
    /// Indicates an error message about a condition that prevents successful processing.
    /// Error messages represent serious problems such as syntax errors, type mismatches, or
    /// runtime failures that make it impossible to continue compilation or execution.
    /// Processing typically stops when error messages are encountered to prevent invalid states.
    /// </summary>
    Error
}