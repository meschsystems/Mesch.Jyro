namespace Mesch.Jyro;

/// <summary>
/// Represents a lexical token in a Jyro script, containing the token type, source text,
/// and location information. Tokens are the fundamental units produced by the lexer
/// and consumed by the parser during the compilation process.
/// </summary>
/// <remarks>
/// Tokens serve as the bridge between the lexical analysis phase and the parsing phase
/// of compilation. Each token encapsulates a meaningful unit of source code along with
/// its classification and precise location information for error reporting and debugging.
/// The lexeme property contains the exact text from the source code, preserving the
/// original representation for accurate diagnostics and potential source reconstruction.
/// </remarks>
public sealed class JyroToken
{
    /// <summary>
    /// Initializes a new instance of the <see cref="JyroToken"/> class with the specified
    /// token type, source text, and location information.
    /// </summary>
    /// <param name="type">
    /// The classification of this token, indicating its grammatical role in the language.
    /// This determines how the parser will interpret and process the token.
    /// </param>
    /// <param name="lexeme">
    /// The exact text from the source code that this token represents.
    /// This preserves the original spelling and formatting for diagnostics and debugging.
    /// </param>
    /// <param name="lineNumber">
    /// The one-based line number in the source code where this token begins.
    /// Used for error reporting and debugging information.
    /// </param>
    /// <param name="columnPosition">
    /// The one-based column position in the source code where this token begins.
    /// Used for precise error location reporting and debugging information.
    /// </param>
    /// <exception cref="ArgumentNullException">Thrown when lexeme is null.</exception>
    public JyroToken(
        JyroTokenType type,
        string lexeme,
        int lineNumber,
        int columnPosition)
    {
        Type = type;
        Lexeme = lexeme ?? throw new ArgumentNullException(nameof(lexeme));
        LineNumber = lineNumber;
        ColumnPosition = columnPosition;
    }

    /// <summary>
    /// Gets the type of this token, which determines its grammatical role in the language.
    /// This classification is used by the parser to understand how to process the token
    /// within the context of the language grammar.
    /// </summary>
    public JyroTokenType Type { get; }

    /// <summary>
    /// Gets the exact text from the source code that this token represents.
    /// This lexeme preserves the original spelling and formatting, enabling accurate
    /// error reporting and potential source code reconstruction.
    /// </summary>
    public string Lexeme { get; }

    /// <summary>
    /// Gets the one-based line number in the source code where this token begins.
    /// This location information is essential for providing meaningful error messages
    /// and debugging information to developers.
    /// </summary>
    public int LineNumber { get; }

    /// <summary>
    /// Gets the one-based column position in the source code where this token begins.
    /// Combined with the line number, this provides precise location information
    /// for error reporting and development tool integration.
    /// </summary>
    public int ColumnPosition { get; }

    /// <summary>
    /// Returns a string representation of this token for debugging and diagnostic purposes.
    /// The format includes the token type, lexeme, and source location information.
    /// </summary>
    /// <returns>
    /// A formatted string containing the token type, lexeme text, and source location
    /// in the format "Type 'Lexeme' at Line:Column".
    /// </returns>
    public override string ToString() =>
        $"{Type} '{Lexeme}' at {LineNumber}:{ColumnPosition}";
}