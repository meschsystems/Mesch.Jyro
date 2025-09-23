namespace Mesch.Jyro;

/// <summary>
/// Provides a high-level facade for the Jyro compilation and execution pipeline.
/// This class orchestrates the complete process from source code to execution results,
/// offering simple entry points for host applications to run Jyro scripts.
/// </summary>
/// <remarks>
/// The Jyro facade implements the Pipeline pattern, coordinating between the lexer,
/// parser, validator, linker, and executor components. Each stage can be executed
/// independently for advanced scenarios, or the entire pipeline can be run as a unit.
/// </remarks>
public sealed class Jyro
{
    private readonly ILexer _lexer;
    private readonly IParser _parser;
    private readonly IValidator _validator;
    private readonly ILinker _linker;
    private readonly IExecutor _executor;

    /// <summary>
    /// Initializes a new instance of the <see cref="Jyro"/> class with the specified pipeline components.
    /// </summary>
    /// <param name="lexer">The lexer component for tokenizing source code.</param>
    /// <param name="parser">The parser component for building abstract syntax trees.</param>
    /// <param name="validator">The validator component for semantic analysis.</param>
    /// <param name="linker">The linker component for resolving references and preparing execution.</param>
    /// <param name="executor">The executor component for running linked programs.</param>
    /// <exception cref="ArgumentNullException">Thrown when any component parameter is null.</exception>
    public Jyro(
        ILexer lexer,
        IParser parser,
        IValidator validator,
        ILinker linker,
        IExecutor executor)
    {
        _lexer = lexer ?? throw new ArgumentNullException(nameof(lexer));
        _parser = parser ?? throw new ArgumentNullException(nameof(parser));
        _validator = validator ?? throw new ArgumentNullException(nameof(validator));
        _linker = linker ?? throw new ArgumentNullException(nameof(linker));
        _executor = executor ?? throw new ArgumentNullException(nameof(executor));
    }

    /// <summary>
    /// Tokenizes the specified source code into a sequence of lexical tokens.
    /// This is the first stage of the compilation pipeline.
    /// </summary>
    /// <param name="source">The Jyro source code to tokenize.</param>
    /// <returns>
    /// A <see cref="JyroLexingResult"/> containing the tokens, any diagnostic messages,
    /// and processing metadata from the lexical analysis phase.
    /// </returns>
    public JyroLexingResult Lex(string source) => _lexer.Tokenize(source);

    /// <summary>
    /// Parses a sequence of tokens into an abstract syntax tree (AST).
    /// This is the second stage of the compilation pipeline.
    /// </summary>
    /// <param name="tokens">The sequence of tokens to parse.</param>
    /// <returns>
    /// A <see cref="JyroParsingResult"/> containing the AST statements, any diagnostic messages,
    /// and processing metadata from the parsing phase.
    /// </returns>
    public JyroParsingResult Parse(IReadOnlyList<JyroToken> tokens) => _parser.Parse(tokens);

    /// <summary>
    /// Validates an abstract syntax tree for semantic correctness and type safety.
    /// This is the third stage of the compilation pipeline.
    /// </summary>
    /// <param name="abstractSyntaxTree">The AST statements to validate.</param>
    /// <returns>
    /// A <see cref="JyroValidationResult"/> containing validation status, any diagnostic messages,
    /// and processing metadata from the semantic analysis phase.
    /// </returns>
    public JyroValidationResult Validate(IReadOnlyList<IJyroStatement> abstractSyntaxTree) =>
        _validator.Validate(abstractSyntaxTree);

    /// <summary>
    /// Links an abstract syntax tree into an executable Jyro program by resolving
    /// function references and preparing runtime structures.
    /// This is the fourth stage of the compilation pipeline.
    /// </summary>
    /// <param name="abstractSyntaxTree">The validated AST statements to link.</param>
    /// <param name="hostFunctions">
    /// Optional collection of host functions to register and make available during execution.
    /// These functions extend the script's capabilities beyond the standard library.
    /// </param>
    /// <returns>
    /// A <see cref="JyroLinkingResult"/> containing the linked program, any diagnostic messages,
    /// and processing metadata from the linking phase.
    /// </returns>
    public JyroLinkingResult Link(
        IReadOnlyList<IJyroStatement> abstractSyntaxTree,
        IEnumerable<IJyroFunction>? hostFunctions = null)
        => _linker.Link(abstractSyntaxTree, hostFunctions);

    /// <summary>
    /// Executes a linked Jyro program against the specified data context.
    /// This is the final stage of the pipeline where the script logic is applied.
    /// </summary>
    /// <param name="program">The linked Jyro program to execute.</param>
    /// <param name="data">
    /// The root data object that the script will operate on. This serves as the initial
    /// context and can be modified during script execution.
    /// </param>
    /// <param name="options">
    /// Execution configuration options including resource limits, diagnostics settings,
    /// and additional host functions.
    /// </param>
    /// <param name="resolver">
    /// Optional script resolver for dynamically loading additional scripts during execution.
    /// This enables modular script architectures and runtime script inclusion.
    /// </param>
    /// <param name="cancellationToken">
    /// Optional cancellation token for cooperative cancellation of long-running scripts.
    /// </param>
    /// <returns>
    /// A <see cref="JyroExecutionResult"/> containing the final data state, any diagnostic messages,
    /// execution statistics, and processing metadata from the execution phase.
    /// </returns>
    public JyroExecutionResult ExecuteLinkedProgram(
        JyroLinkedProgram program,
        JyroValue data,
        JyroExecutionOptions options,
        JyroScriptResolver? resolver = null,
        CancellationToken cancellationToken = default)
    {
        var context = new JyroExecutionContext(
            data,
            program,
            options,
            new JyroResourceLimiter(options),
            resolver,
            cancellationToken);

        return _executor.Execute(program, context);
    }
}