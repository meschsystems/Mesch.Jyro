namespace Mesch.Jyro;

/// <summary>
/// Defines the standardized set of diagnostic message codes used throughout the Jyro compilation pipeline.
/// Each code represents a specific type of issue that can occur during lexical analysis, parsing,
/// validation, linking, or execution phases. The numeric values are organized by processing stage
/// to enable systematic categorization and filtering of diagnostic messages.
/// </summary>
/// <remarks>
/// The message code system follows a structured numbering scheme:
/// <list type="bullet">
/// <item><description>1000-1999: Lexical analysis (tokenization) errors</description></item>
/// <item><description>2000-2999: Parsing (syntax analysis) errors</description></item>
/// <item><description>3000-3999: Validation (semantic analysis) errors</description></item>
/// <item><description>4000-4999: Linking (reference resolution) errors</description></item>
/// <item><description>5000-5999: Execution (runtime) errors</description></item>
/// </list>
/// This organization enables tools and diagnostic systems to categorize and filter messages
/// by compilation stage, providing better user experience and debugging capabilities.
/// </remarks>
public enum MessageCode
{
    #region Lexical Analysis Errors (1000–1999)

    /// <summary>
    /// An unexpected error occurred during lexical analysis that doesn't fit other categories.
    /// This typically indicates an internal lexer error or system-level issue.
    /// </summary>
    UnknownLexerError = 1000,

    /// <summary>
    /// An unexpected character was encountered in the source code that cannot be tokenized.
    /// This occurs when the lexer encounters characters not valid in the Jyro language.
    /// </summary>
    UnexpectedCharacter = 1001,

    /// <summary>
    /// A string literal was started but never properly terminated with a closing quote.
    /// This error is reported when the lexer reaches end-of-file or a newline within a string.
    /// </summary>
    UnterminatedString = 1002,

    #endregion

    #region Parsing Errors (2000–2999)

    /// <summary>
    /// An unexpected error occurred during parsing that doesn't fit other categories.
    /// This typically indicates an internal parser error or malformed AST construction.
    /// </summary>
    UnknownParserError = 2000,

    /// <summary>
    /// An unexpected token was encountered during parsing that doesn't match the expected grammar.
    /// This is the most common parsing error, indicating syntax violations in the source code.
    /// </summary>
    UnexpectedToken = 2001,

    /// <summary>
    /// A required token was missing from the expected position in the grammar.
    /// This occurs when mandatory syntax elements are omitted from the source code.
    /// </summary>
    MissingToken = 2002,

    /// <summary>
    /// A numeric literal could not be parsed into a valid number representation.
    /// This error occurs when number tokens contain invalid formatting or exceed numeric limits.
    /// </summary>
    InvalidNumberFormat = 2003,

    #endregion

    #region Validation Errors (3000–3999)

    /// <summary>
    /// An unexpected error occurred during semantic validation that doesn't fit other categories.
    /// This typically indicates an internal validator error or complex semantic issue.
    /// </summary>
    UnknownValidatorError = 3000,

    /// <summary>
    /// A variable was referenced that has not been declared in any accessible scope.
    /// This error enforces proper variable declaration before use.
    /// </summary>
    InvalidVariableReference = 3001,

    /// <summary>
    /// An assignment was attempted to an expression that cannot be assigned to.
    /// Valid assignment targets are variables, properties, and indexed elements.
    /// </summary>
    InvalidAssignmentTarget = 3002,

    /// <summary>
    /// A type mismatch was detected between expected and actual types in an operation.
    /// This error occurs when operations are applied to incompatible value types.
    /// </summary>
    TypeMismatch = 3003,

    /// <summary>
    /// A loop control statement (break or continue) was used outside of a loop context.
    /// These statements are only valid within while, foreach, or other loop constructs.
    /// </summary>
    LoopStatementOutsideOfLoop = 3004,

    /// <summary>
    /// Loop nesting exceeded the maximum allowed depth for performance and safety reasons.
    /// This prevents deeply nested loops that could impact execution performance.
    /// </summary>
    ExcessiveLoopNesting = 3005,

    /// <summary>
    /// Code was detected that can never be reached during normal execution flow.
    /// This typically occurs after return statements or unconditional branches.
    /// </summary>
    UnreachableCode = 3006,

    #endregion

    #region Linking Errors (4000–4999)

    /// <summary>
    /// An unexpected error occurred during linking that doesn't fit other categories.
    /// This typically indicates an internal linker error or reference resolution issue.
    /// </summary>
    UnknownLinkerError = 4000,

    /// <summary>
    /// A function was called that has not been defined or registered in the execution environment.
    /// This error occurs when scripts reference functions that are not available.
    /// </summary>
    UndefinedFunction = 4001,

    /// <summary>
    /// A function was defined multiple times with the same signature.
    /// This error prevents ambiguous function resolution during execution.
    /// </summary>
    DuplicateFunction = 4002,

    /// <summary>
    /// A function definition overrides an existing function with the same name.
    /// This may be a warning or error depending on the execution environment policy.
    /// </summary>
    FunctionOverride = 4003,

    /// <summary>
    /// A function was called with an incorrect number of arguments for its signature.
    /// This error enforces proper function call syntax and parameter matching.
    /// </summary>
    InvalidNumberArguments = 4004,

    #endregion

    #region Execution Errors (5000–5999)

    /// <summary>
    /// An unexpected error occurred during execution that doesn't fit other categories.
    /// This typically indicates an internal executor error or system-level issue.
    /// </summary>
    UnknownExecutorError = 5000,

    /// <summary>
    /// A general runtime error occurred during script execution.
    /// This is a catch-all category for execution-time issues that don't fit specific categories.
    /// </summary>
    RuntimeError = 5001,

    /// <summary>
    /// Script execution was cancelled by the host application or execution environment.
    /// This occurs when external systems terminate execution before natural completion.
    /// </summary>
    CancelledByHost = 5002,

    /// <summary>
    /// An operation was attempted on a value of an incompatible type.
    /// This error occurs at runtime when type checking reveals incompatible operations.
    /// </summary>
    InvalidType = 5003,

    /// <summary>
    /// A function was called with an argument of an incorrect type for the expected parameter.
    /// This error enforces type safety at function call boundaries during execution.
    /// </summary>
    InvalidArgumentType = 5004

    #endregion
}