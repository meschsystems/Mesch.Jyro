namespace Mesch.Jyro;

/// <summary>
/// Represents the comprehensive result of the lexical analysis stage, including the token sequence,
/// diagnostic messages, success status, and processing metadata. This result encapsulates all
/// information produced during tokenization for use by subsequent compilation stages.
/// </summary>
/// <remarks>
/// The lexing result serves as the primary output of the lexical analysis phase and provides
/// all necessary information for the parser to proceed with syntax analysis. It includes both
/// the successful tokenization output and any error or warning conditions encountered during
/// the process, enabling robust error handling and detailed diagnostic reporting.
/// 
/// <para>
/// The success status is determined by the absence of error-level diagnostic messages,
/// allowing callers to quickly determine whether tokenization completed successfully
/// or requires attention before proceeding to the parsing stage.
/// </para>
/// </remarks>
public sealed class JyroLexingResult
{
    /// <summary>
    /// Initializes a new instance of the <see cref="JyroLexingResult"/> class with the specified
    /// tokenization outcome, diagnostic messages, and processing metadata.
    /// </summary>
    /// <param name="isSuccessful">
    /// Indicates whether the lexical analysis completed without any error-level diagnostic messages.
    /// This flag enables quick success/failure determination without examining the diagnostic collection.
    /// </param>
    /// <param name="tokens">
    /// The sequence of tokens produced during lexical analysis. This collection contains all
    /// meaningful tokens extracted from the source code in the order they appeared.
    /// </param>
    /// <param name="messages">
    /// The diagnostic messages generated during the tokenization process. This includes
    /// errors, warnings, and informational messages with precise location information.
    /// </param>
    /// <param name="metadata">
    /// Metadata containing statistical and performance information about the lexing process,
    /// including timing data and token counts for analysis and debugging purposes.
    /// </param>
    /// <exception cref="ArgumentNullException">Thrown when metadata is null.</exception>
    public JyroLexingResult(
        bool isSuccessful,
        IReadOnlyList<JyroToken> tokens,
        IReadOnlyList<IMessage> messages,
        LexingMetadata metadata)
    {
        IsSuccessful = isSuccessful;
        Tokens = tokens ?? Array.Empty<JyroToken>();
        Messages = messages ?? Array.Empty<IMessage>();
        Metadata = metadata ?? throw new ArgumentNullException(nameof(metadata));
    }

    /// <summary>
    /// Gets a value indicating whether lexical analysis completed successfully without any error-level diagnostics.
    /// When true, the token sequence is ready for parsing. When false, error-level diagnostics
    /// should be examined before proceeding to subsequent compilation stages.
    /// </summary>
    public bool IsSuccessful { get; }

    /// <summary>
    /// Gets the sequence of tokens produced during lexical analysis.
    /// This collection contains all meaningful tokens extracted from the source code,
    /// including an end-of-file token to mark the completion of the input stream.
    /// The tokens are ordered according to their appearance in the source code.
    /// </summary>
    public IReadOnlyList<JyroToken> Tokens { get; }

    /// <summary>
    /// Gets the diagnostic messages produced during the lexical analysis process.
    /// This collection includes all errors, warnings, and informational messages
    /// generated while tokenizing the source code, each with precise location information
    /// for accurate error reporting and debugging.
    /// </summary>
    public IReadOnlyList<IMessage> Messages { get; }

    /// <summary>
    /// Gets the number of error-level diagnostic messages in the result.
    /// This count provides a quick way to determine the severity of issues
    /// encountered during tokenization without examining each diagnostic individually.
    /// </summary>
    public int ErrorCount => Messages.Count(message => message.Severity == MessageSeverity.Error);

    /// <summary>
    /// Gets metadata containing statistical and performance information about the lexical analysis process.
    /// This includes timing data, token counts, and other metrics useful for performance analysis,
    /// debugging, and development tool integration.
    /// </summary>
    public LexingMetadata Metadata { get; }
}