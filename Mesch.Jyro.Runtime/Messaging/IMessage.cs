namespace Mesch.Jyro;

/// <summary>
/// Represents a diagnostic message produced during any stage of Jyro compilation or execution.
/// Diagnostic messages provide detailed information about issues, warnings, and informational
/// events that occur throughout the processing pipeline, enabling comprehensive error reporting
/// and debugging capabilities.
/// </summary>
/// <remarks>
/// Diagnostic messages serve multiple purposes in the Jyro system:
/// <list type="bullet">
/// <item><description>Error reporting: Communicating compilation and runtime failures to users</description></item>
/// <item><description>Development support: Providing detailed information for debugging and troubleshooting</description></item>
/// <item><description>Tool integration: Enabling IDEs and development tools to provide rich diagnostic experiences</description></item>
/// <item><description>Quality assurance: Highlighting potential issues and best practice violations</description></item>
/// </list>
/// 
/// Each message contains comprehensive context including location information, categorization,
/// and extensible argument support for localization and customization.
/// </remarks>
public interface IMessage
{
    /// <summary>
    /// Gets the unique diagnostic code that categorizes this message type.
    /// Message codes enable systematic handling, filtering, and localization of diagnostic information.
    /// The code determines the specific issue type and provides a stable identifier across different
    /// message provider implementations and localization scenarios.
    /// </summary>
    MessageCode Code { get; }

    /// <summary>
    /// Gets the severity level that indicates the impact and urgency of this message.
    /// Severity levels enable appropriate handling and presentation of diagnostic information,
    /// allowing tools and users to prioritize critical errors over warnings and informational messages.
    /// </summary>
    MessageSeverity Severity { get; }

    /// <summary>
    /// Gets the compilation pipeline stage that generated this diagnostic message.
    /// Stage information helps identify where in the processing pipeline the issue occurred,
    /// enabling targeted debugging and appropriate resolution strategies.
    /// </summary>
    ProcessingStage Stage { get; }

    /// <summary>
    /// Gets the one-based line number in the source code where this diagnostic message applies.
    /// Line numbers provide precise location information for error reporting and enable
    /// development tools to highlight specific source code locations.
    /// </summary>
    int LineNumber { get; }

    /// <summary>
    /// Gets the one-based column position in the source code where this diagnostic message applies.
    /// Column positions work with line numbers to provide exact source location information
    /// for precise error highlighting and navigation in development environments.
    /// </summary>
    int ColumnPosition { get; }

    /// <summary>
    /// Gets the collection of arguments that provide context-specific information for this message.
    /// Arguments enable parameterized diagnostic messages that can include relevant details
    /// such as variable names, expected values, or other contextual information.
    /// These arguments are typically used with message templates for localization and formatting.
    /// </summary>
    IReadOnlyList<string> Arguments { get; }
}