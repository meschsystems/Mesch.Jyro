using Antlr4.Runtime;

namespace Mesch.Jyro;

/// <summary>
/// Custom ANTLR error listener that captures syntax errors as Jyro diagnostic messages.
/// </summary>
public sealed class SyntaxErrorListener : BaseErrorListener
{
    private readonly List<IMessage> _errors = new();

    /// <summary>
    /// Gets the collection of syntax errors captured during parsing.
    /// </summary>
    public IReadOnlyList<IMessage> Errors => _errors;

    /// <summary>
    /// Gets whether any syntax errors were captured.
    /// </summary>
    public bool HasErrors => _errors.Count > 0;

    /// <summary>
    /// Called when ANTLR encounters a syntax error during parsing.
    /// </summary>
    public override void SyntaxError(
        TextWriter output,
        IRecognizer recognizer,
        IToken offendingSymbol,
        int line,
        int charPositionInLine,
        string msg,
        RecognitionException e)
    {
        // Determine appropriate error code based on ANTLR error message
        var code = msg.Contains("missing")
            ? MessageCode.MissingToken
            : MessageCode.UnexpectedToken;

        _errors.Add(new Message(
            code,
            line,
            charPositionInLine + 1,  // Convert to 1-based column position
            MessageSeverity.Error,
            ProcessingStage.Parsing,
            offendingSymbol?.Text ?? "unknown"));
    }
}
