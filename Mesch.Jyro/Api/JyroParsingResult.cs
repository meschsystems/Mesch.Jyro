using Antlr4.CodeGenerator;

namespace Mesch.Jyro;

/// <summary>
/// Represents the comprehensive result of the parsing stage, containing the parse tree,
/// diagnostic messages, and performance metadata. Provides complete information about
/// the parsing operation's outcome and any syntax errors encountered.
/// </summary>
public sealed class JyroParsingResult
{
    /// <summary>
    /// Initializes a new instance of the <see cref="JyroParsingResult"/> class
    /// with the specified parsing outcome and associated information.
    /// </summary>
    /// <param name="parsingSucceeded">
    /// A value indicating whether the parsing operation completed successfully
    /// without any syntax errors.
    /// </param>
    /// <param name="programContext">
    /// The successfully parsed program context, or null if parsing failed.
    /// </param>
    /// <param name="diagnosticMessages">
    /// The collection of diagnostic messages generated during the parsing process.
    /// </param>
    /// <param name="parsingMetadata">
    /// Metadata containing parsing statistics and timing information. Cannot be null.
    /// </param>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="parsingMetadata"/> is null.
    /// </exception>
    public JyroParsingResult(
        bool parsingSucceeded,
        JyroParser.ProgramContext? programContext,
        IReadOnlyList<IMessage> diagnosticMessages,
        ParsingMetadata parsingMetadata)
    {
        IsSuccessful = parsingSucceeded;
        ProgramContext = programContext;
        Messages = diagnosticMessages ?? Array.Empty<IMessage>();
        Metadata = parsingMetadata ?? throw new ArgumentNullException(nameof(parsingMetadata));
    }

    /// <summary>
    /// Gets a value indicating whether the parsing operation completed successfully
    /// without any syntax errors that would prevent further processing.
    /// </summary>
    /// <value>
    /// <c>true</c> if parsing succeeded and the parse tree is ready for linking;
    /// otherwise, <c>false</c>.
    /// </value>
    public bool IsSuccessful { get; }

    /// <summary>
    /// Gets the successfully parsed program context that is ready for linking,
    /// or null if parsing failed due to syntax errors.
    /// </summary>
    /// <value>
    /// A <see cref="JyroParser.ProgramContext"/> instance containing the parse tree,
    /// or null if parsing was unsuccessful.
    /// </value>
    public JyroParser.ProgramContext? ProgramContext { get; }

    /// <summary>
    /// Gets the collection of diagnostic messages produced during the parsing process,
    /// including syntax errors, warnings, and informational messages.
    /// </summary>
    /// <value>
    /// A collection of <see cref="IMessage"/> instances representing all diagnostics
    /// generated during parsing. Never null, but may be empty.
    /// </value>
    public IReadOnlyList<IMessage> Messages { get; }

    /// <summary>
    /// Gets the number of error-level diagnostic messages in the parsing result.
    /// </summary>
    /// <value>
    /// The count of messages with <see cref="MessageSeverity.Error"/> severity.
    /// </value>
    public int ErrorCount => Messages.Count(message => message.Severity == MessageSeverity.Error);

    /// <summary>
    /// Gets metadata containing detailed statistics and timing information
    /// about the parsing operation.
    /// </summary>
    /// <value>
    /// A <see cref="ParsingMetadata"/> instance providing performance
    /// and statistical data about the parsing process.
    /// </value>
    public ParsingMetadata Metadata { get; }

    /// <summary>
    /// Gets a convenience property that returns the linked program if available.
    /// This is an alias to maintain API compatibility.
    /// </summary>
    public LinkedProgram? LinkedProgram => null;
}
