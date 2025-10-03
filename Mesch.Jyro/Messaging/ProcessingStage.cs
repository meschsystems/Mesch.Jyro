namespace Mesch.Jyro;

/// <summary>
/// Defines the compilation pipeline stages that can produce diagnostic messages during Jyro processing.
/// Each stage represents a distinct phase in the compilation process, from source code tokenization
/// through final execution. This categorization enables precise identification of where issues occur
/// and supports targeted debugging and error resolution workflows.
/// </summary>
/// <remarks>
/// The processing stages follow the standard compilation pipeline architecture:
/// <list type="bullet">
/// <item><description>Lexing: Converts source text into tokens</description></item>
/// <item><description>Parsing: Converts tokens into abstract syntax trees</description></item>
/// <item><description>Validation: Performs semantic analysis and type checking</description></item>
/// <item><description>Linking: Resolves references and prepares executable representations</description></item>
/// <item><description>Execution: Runs the compiled program and evaluates expressions</description></item>
/// </list>
/// Understanding which stage produced a diagnostic message helps developers and tools
/// provide appropriate guidance and resolution strategies for different types of issues.
/// </remarks>
public enum ProcessingStage
{
    /// <summary>
    /// Indicates that the diagnostic message was produced during lexical analysis (tokenization).
    /// Lexing stage messages typically relate to character-level issues such as invalid characters,
    /// unterminated strings, or malformed tokens in the source code.
    /// </summary>
    Lexing,

    /// <summary>
    /// Indicates that the diagnostic message was produced during parsing (syntax analysis).
    /// Parsing stage messages typically relate to grammar violations, missing tokens,
    /// unexpected token sequences, or structural syntax errors in the source code.
    /// </summary>
    Parsing,

    /// <summary>
    /// Indicates that the diagnostic message was produced during semantic validation.
    /// Validation stage messages typically relate to type checking, scope analysis,
    /// variable declarations, function signatures, and other semantic correctness issues.
    /// </summary>
    Validation,

    /// <summary>
    /// Indicates that the diagnostic message was produced during linking and reference resolution.
    /// Linking stage messages typically relate to function resolution, module dependencies,
    /// symbol binding, and preparation of executable representations from validated code.
    /// </summary>
    Linking,

    /// <summary>
    /// Indicates that the diagnostic message was produced during runtime execution.
    /// Execution stage messages typically relate to runtime errors, type violations,
    /// resource limits, function call failures, and other dynamic execution issues.
    /// </summary>
    Execution
}