namespace Mesch.Jyro;

/// <summary>
/// Represents the comprehensive result of the parsing stage in the Jyro compilation pipeline.
/// This result encapsulates the abstract syntax tree, diagnostic messages, success status,
/// and processing metadata for use by subsequent compilation stages.
/// </summary>
/// <remarks>
/// The parsing result serves as the primary output of the syntax analysis phase and provides
/// all necessary information for semantic analysis, validation, and eventual execution.
/// It follows the established pattern of compilation stage results, maintaining consistency
/// with lexing, linking, and execution phases.
/// 
/// <para>
/// The success status is determined by the absence of error-level diagnostic messages,
/// enabling callers to quickly assess whether parsing completed successfully without
/// examining the full diagnostic collection. Even failed parsing attempts may produce
/// partial AST structures that can be useful for development tools and error recovery.
/// </para>
/// 
/// <para>
/// The statement collection contains the top-level constructs parsed from the input,
/// forming the root nodes of the abstract syntax tree that represents the program structure.
/// </para>
/// </remarks>
public sealed class JyroParsingResult
{
    /// <summary>
    /// Initializes a new instance of the <see cref="JyroParsingResult"/> class with the specified
    /// parsing outcome, diagnostic messages, and processing metadata.
    /// </summary>
    /// <param name="isSuccessful">
    /// Indicates whether the parsing process completed without any error-level diagnostic messages.
    /// This flag enables quick success/failure determination for pipeline decision making.
    /// </param>
    /// <param name="statements">
    /// The sequence of top-level statements parsed from the input token stream.
    /// These form the root nodes of the abstract syntax tree representing the program structure.
    /// </param>
    /// <param name="messages">
    /// The diagnostic messages generated during the parsing process. This includes
    /// errors, warnings, and informational messages with precise location information.
    /// </param>
    /// <param name="metadata">
    /// Metadata containing statistical and performance information about the parsing process,
    /// including timing data, complexity metrics, and structural analysis for debugging.
    /// </param>
    /// <exception cref="ArgumentNullException">Thrown when metadata is null.</exception>
    public JyroParsingResult(
        bool isSuccessful,
        IReadOnlyList<IJyroStatement> statements,
        IReadOnlyList<IMessage> messages,
        ParsingMetadata metadata)
    {
        IsSuccessful = isSuccessful;
        Statements = statements ?? Array.Empty<IJyroStatement>();
        Messages = messages ?? Array.Empty<IMessage>();
        Metadata = metadata ?? throw new ArgumentNullException(nameof(metadata));
    }

    /// <summary>
    /// Gets a value indicating whether parsing completed successfully without any error-level diagnostics.
    /// When true, the abstract syntax tree is ready for semantic analysis. When false, error-level
    /// diagnostics should be examined before proceeding to subsequent compilation stages.
    /// </summary>
    public bool IsSuccessful { get; }

    /// <summary>
    /// Gets the sequence of top-level statements parsed from the input token stream.
    /// These statements form the root nodes of the abstract syntax tree and represent
    /// the primary structural elements of the parsed program. Even partial parsing
    /// results may contain some valid statements for analysis purposes.
    /// </summary>
    public IReadOnlyList<IJyroStatement> Statements { get; }

    /// <summary>
    /// Gets the diagnostic messages produced during the parsing process.
    /// This collection includes all errors, warnings, and informational messages
    /// generated while analyzing the token stream, each with precise location information
    /// for accurate error reporting and debugging.
    /// </summary>
    public IReadOnlyList<IMessage> Messages { get; }

    /// <summary>
    /// Gets the number of error-level diagnostic messages in the result.
    /// This count provides a quick way to assess the severity of parsing issues
    /// without examining each diagnostic message individually.
    /// </summary>
    public int ErrorCount => Messages.Count(message => message.Severity == MessageSeverity.Error);

    /// <summary>
    /// Gets metadata containing statistical and performance information about the parsing process.
    /// This includes timing data, complexity metrics, and structural analysis useful for
    /// performance monitoring, debugging, and development tool integration.
    /// </summary>
    public ParsingMetadata Metadata { get; }
}