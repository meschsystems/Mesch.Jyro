namespace Mesch.Jyro;

/// <summary>
/// Defines the contract for lexical analyzers that tokenize Jyro source code.
/// Lexers are responsible for the first phase of compilation, converting raw source text
/// into a structured sequence of tokens that can be processed by the parser.
/// </summary>
/// <remarks>
/// The lexer interface abstracts the tokenization process, allowing different implementations
/// to provide varying approaches to lexical analysis while maintaining a consistent contract.
/// This enables flexibility in lexer implementation strategies, performance optimizations,
/// and potential future enhancements such as incremental parsing or error recovery.
/// 
/// <para>
/// Implementations should handle all aspects of lexical analysis including keyword recognition,
/// operator tokenization, literal parsing, identifier extraction, comment handling, and
/// comprehensive error reporting with precise location information.
/// </para>
/// </remarks>
public interface ILexer
{
    /// <summary>
    /// Tokenizes the provided source text and returns a comprehensive result containing
    /// the token sequence, diagnostic messages, and processing metadata.
    /// </summary>
    /// <param name="source">
    /// The Jyro source code to tokenize. This should be the complete text of a script
    /// or code fragment that needs to be converted into tokens for parsing.
    /// </param>
    /// <returns>
    /// A <see cref="JyroLexingResult"/> containing the complete tokenization outcome,
    /// including the sequence of tokens produced, any diagnostic messages generated
    /// during the process, success status, and timing/statistical metadata.
    /// </returns>
    /// <remarks>
    /// The tokenization process should be comprehensive and robust, handling all valid
    /// Jyro language constructs while providing detailed error information for invalid
    /// or malformed input. The result should include sufficient information for the
    /// parser to proceed with syntax analysis or for error reporting to provide
    /// meaningful feedback to developers.
    /// </remarks>
    JyroLexingResult Tokenize(string source);
}