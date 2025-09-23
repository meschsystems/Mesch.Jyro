namespace Mesch.Jyro;

/// <summary>
/// Defines the contract for parsers that transform token sequences into abstract syntax trees.
/// Parsers are responsible for the second phase of compilation, converting the structured
/// token output from lexical analysis into a hierarchical representation suitable for
/// semantic analysis and execution.
/// </summary>
/// <remarks>
/// The parser interface abstracts the syntax analysis process, enabling different parsing
/// strategies and implementations while maintaining a consistent contract with the compilation
/// pipeline. This flexibility supports various parsing approaches such as recursive descent,
/// operator precedence parsing, or table-driven parsing algorithms.
/// 
/// <para>
/// Parser implementations should handle all aspects of syntax analysis including:
/// <list type="bullet">
/// <item><description>Grammar rule enforcement according to the Jyro language specification</description></item>
/// <item><description>Precedence and associativity handling for expressions</description></item>
/// <item><description>Error detection and recovery with meaningful diagnostic messages</description></item>
/// <item><description>AST construction with proper source location tracking</description></item>
/// <item><description>Robustness against malformed or incomplete input</description></item>
/// </list>
/// </para>
/// 
/// <para>
/// The parsing process should produce a well-formed abstract syntax tree that accurately
/// represents the structure and semantics of the input program while preserving all
/// necessary information for subsequent compilation stages.
/// </para>
/// </remarks>
public interface IParser
{
    /// <summary>
    /// Parses the provided token sequence and returns a comprehensive result containing
    /// the abstract syntax tree, diagnostic messages, and processing metadata.
    /// </summary>
    /// <param name="tokens">
    /// The sequence of tokens produced by lexical analysis. This should be a complete
    /// and valid token stream including proper termination markers for successful parsing.
    /// The token sequence must include an end-of-file token to properly terminate parsing.
    /// </param>
    /// <returns>
    /// A <see cref="JyroParsingResult"/> containing the complete parsing outcome,
    /// including the abstract syntax tree statements, any diagnostic messages generated
    /// during the process, success status, and timing/structural metadata for analysis.
    /// </returns>
    /// <remarks>
    /// The parsing process should be robust and comprehensive, handling all valid Jyro
    /// language constructs while providing detailed error information for invalid syntax.
    /// The result should include sufficient information for semantic analysis to proceed
    /// or for error reporting to provide meaningful feedback to developers.
    /// 
    /// <para>
    /// Even when parsing fails due to syntax errors, the result should contain as much
    /// of the successfully parsed AST as possible to enable partial analysis and better
    /// error recovery in integrated development environments.
    /// </para>
    /// </remarks>
    JyroParsingResult Parse(IReadOnlyList<JyroToken> tokens);
}