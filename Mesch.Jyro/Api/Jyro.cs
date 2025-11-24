using Antlr4.CodeGenerator;
using Antlr4.Runtime;

namespace Mesch.Jyro;

/// <summary>
/// Provides a high-level facade for the Jyro compilation and execution pipeline.
/// This class orchestrates the complete process from source code to execution results,
/// offering simple entry points for host applications to run Jyro scripts.
/// </summary>
/// <remarks>
/// The Jyro facade implements the Pipeline pattern, using ANTLR for lexing and parsing,
/// then linking and executing the parse tree. Each stage can be executed independently
/// for advanced scenarios, or the entire pipeline can be run as a unit.
/// </remarks>
public sealed class Jyro
{
    private readonly Validator _validator;
    private readonly Linker _linker;
    private readonly Interpreter _interpreter;

    /// <summary>
    /// Initializes a new instance of the <see cref="Jyro"/> class with the specified pipeline components.
    /// </summary>
    /// <param name="validator">The validator component for semantic analysis.</param>
    /// <param name="linker">The linker component for resolving references and preparing execution.</param>
    /// <param name="interpreter">The interpreter component for executing linked programs.</param>
    /// <exception cref="ArgumentNullException">Thrown when any component parameter is null.</exception>
    public Jyro(Validator validator, Linker linker, Interpreter interpreter)
    {
        _validator = validator ?? throw new ArgumentNullException(nameof(validator));
        _linker = linker ?? throw new ArgumentNullException(nameof(linker));
        _interpreter = interpreter ?? throw new ArgumentNullException(nameof(interpreter));
    }

    /// <summary>
    /// Parses the specified source code into an ANTLR parse tree.
    /// This is the first stage of the ANTLR-based compilation pipeline.
    /// </summary>
    /// <param name="source">The Jyro source code to parse.</param>
    /// <returns>
    /// A <see cref="JyroParsingResult"/> containing the parse tree context, any diagnostic messages,
    /// and processing metadata from the parsing phase.
    /// </returns>
    public JyroParsingResult Parse(string source)
    {
        ArgumentNullException.ThrowIfNull(source);

        var inputStream = new AntlrInputStream(source);
        var lexer = new JyroLexer(inputStream);
        var tokenStream = new CommonTokenStream(lexer);
        var parser = new JyroParser(tokenStream);

        // TODO: Add error listener to collect syntax errors
        var programContext = parser.program();

        // TODO: Return proper result with diagnostics
        return new JyroParsingResult(
            true,
            programContext,
            Array.Empty<IMessage>(),
            new ParsingMetadata(TimeSpan.Zero, 0, DateTimeOffset.UtcNow));
    }

    /// <summary>
    /// Validates a parse tree for semantic correctness.
    /// This is the second stage of the compilation pipeline.
    /// </summary>
    /// <param name="programContext">The parse tree program context to validate.</param>
    /// <returns>
    /// A <see cref="JyroValidationResult"/> containing validation diagnostics and metadata.
    /// </returns>
    public JyroValidationResult Validate(JyroParser.ProgramContext programContext)
        => _validator.Validate(programContext);

    /// <summary>
    /// Links a parse tree into an executable Jyro program by resolving
    /// function references and preparing runtime structures.
    /// This is the third stage of the compilation pipeline.
    /// </summary>
    /// <param name="programContext">The parse tree program context to link.</param>
    /// <param name="hostFunctions">
    /// Optional collection of host functions to register and make available during execution.
    /// These functions extend the script's capabilities beyond the standard library.
    /// </param>
    /// <returns>
    /// A <see cref="JyroLinkingResult"/> containing the linked program, any diagnostic messages,
    /// and processing metadata from the linking phase.
    /// </returns>
    public JyroLinkingResult Link(
        JyroParser.ProgramContext programContext,
        IEnumerable<IJyroFunction>? hostFunctions = null)
        => _linker.Link(programContext, hostFunctions);

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
        LinkedProgram program,
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

        return _interpreter.Execute(program, context);
    }

    /// <summary>
    /// Runs the complete Jyro pipeline from source code to execution result.
    /// This is a convenience method that combines parsing, validation, linking, and execution.
    /// </summary>
    /// <param name="source">The Jyro source code to execute.</param>
    /// <param name="data">The root data object for script execution.</param>
    /// <param name="options">Execution configuration options.</param>
    /// <param name="hostFunctions">Optional host functions to make available.</param>
    /// <param name="resolver">Optional script resolver for dynamic loading.</param>
    /// <param name="cancellationToken">Optional cancellation token.</param>
    /// <returns>The execution result.</returns>
    public JyroExecutionResult Run(
        string source,
        JyroValue data,
        JyroExecutionOptions options,
        IEnumerable<IJyroFunction>? hostFunctions = null,
        JyroScriptResolver? resolver = null,
        CancellationToken cancellationToken = default)
    {
        // Stage 1: Parse
        var parseResult = Parse(source);
        if (!parseResult.IsSuccessful || parseResult.ProgramContext == null)
        {
            return new JyroExecutionResult(
                false,
                data,
                parseResult.Messages,
                new ExecutionMetadata(TimeSpan.Zero, 0, 0, 0, 0, DateTimeOffset.UtcNow));
        }

        // Stage 2: Validate
        var validationResult = Validate(parseResult.ProgramContext);
        if (!validationResult.IsSuccessful)
        {
            return new JyroExecutionResult(
                false,
                data,
                validationResult.Messages,
                new ExecutionMetadata(TimeSpan.Zero, 0, 0, 0, 0, DateTimeOffset.UtcNow));
        }

        // Stage 3: Link
        var linkResult = Link(parseResult.ProgramContext, hostFunctions);
        if (!linkResult.IsSuccessful || linkResult.Program == null)
        {
            return new JyroExecutionResult(
                false,
                data,
                linkResult.Messages,
                new ExecutionMetadata(TimeSpan.Zero, 0, 0, 0, 0, DateTimeOffset.UtcNow));
        }

        // Stage 4: Execute
        return ExecuteLinkedProgram(linkResult.Program, data, options, resolver, cancellationToken);
    }
}
