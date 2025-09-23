namespace Mesch.Jyro;

/// <summary>
/// Represents the comprehensive result of the linking stage, containing the linked program,
/// diagnostic messages, and performance metadata. Provides complete information about
/// the linking operation's outcome and any issues encountered.
/// </summary>
public sealed class JyroLinkingResult
{
    /// <summary>
    /// Initializes a new instance of the <see cref="JyroLinkingResult"/> class
    /// with the specified linking outcome and associated information.
    /// </summary>
    /// <param name="linkingSucceeded">
    /// A value indicating whether the linking operation completed successfully
    /// without any errors.
    /// </param>
    /// <param name="linkedProgram">
    /// The successfully linked program ready for execution, or null if linking failed.
    /// </param>
    /// <param name="diagnosticMessages">
    /// The collection of diagnostic messages generated during the linking process.
    /// </param>
    /// <param name="linkingMetadata">
    /// Metadata containing linking statistics and timing information. Cannot be null.
    /// </param>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="linkingMetadata"/> is null.
    /// </exception>
    public JyroLinkingResult(
        bool linkingSucceeded,
        JyroLinkedProgram? linkedProgram,
        IReadOnlyList<IMessage> diagnosticMessages,
        JyroLinkingMetadata linkingMetadata)
    {
        IsSuccessful = linkingSucceeded;
        Program = linkedProgram;
        Messages = diagnosticMessages ?? Array.Empty<IMessage>();
        Metadata = linkingMetadata ?? throw new ArgumentNullException(nameof(linkingMetadata));
    }

    /// <summary>
    /// Gets a value indicating whether the linking operation completed successfully
    /// without any errors that would prevent program execution.
    /// </summary>
    /// <value>
    /// <c>true</c> if linking succeeded and the program is ready for execution;
    /// otherwise, <c>false</c>.
    /// </value>
    public bool IsSuccessful { get; }

    /// <summary>
    /// Gets the successfully linked program that is ready for execution by the runtime,
    /// or null if linking failed due to errors.
    /// </summary>
    /// <value>
    /// A <see cref="JyroLinkedProgram"/> instance containing the executable program
    /// and resolved functions, or null if linking was unsuccessful.
    /// </value>
    public JyroLinkedProgram? Program { get; }

    /// <summary>
    /// Gets the collection of diagnostic messages produced during the linking process,
    /// including errors, warnings, and informational messages.
    /// </summary>
    /// <value>
    /// A collection of <see cref="IMessage"/> instances representing all diagnostics
    /// generated during linking. Never null, but may be empty.
    /// </value>
    public IReadOnlyList<IMessage> Messages { get; }

    /// <summary>
    /// Gets the number of error-level diagnostic messages in the linking result.
    /// </summary>
    /// <value>
    /// The count of messages with <see cref="MessageSeverity.Error"/> severity.
    /// </value>
    public int ErrorCount => Messages.Count(message => message.Severity == MessageSeverity.Error);

    /// <summary>
    /// Gets metadata containing detailed statistics and timing information
    /// about the linking operation.
    /// </summary>
    /// <value>
    /// A <see cref="JyroLinkingMetadata"/> instance providing performance
    /// and statistical data about the linking process.
    /// </value>
    public JyroLinkingMetadata Metadata { get; }
}