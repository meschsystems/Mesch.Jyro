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
    #region Lexical Analysis Errors (1000-1999)

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

    #region Parsing Errors (2000-2999)

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

    #region Validation Errors (3000-3999)

    /// <summary>
    /// An unexpected error occurred during semantic validation that doesn't fit other categories.
    /// This typically indicates an internal validator error or complex semantic issue.
    /// </summary>
    UnknownValidatorError = 3000,

    /// <summary>
    /// A variable was referenced that has not been declared in any accessible scope.
    /// This error enforces proper variable declaration before use.
    /// </summary>
    UndeclaredVariable = 3001,

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

    /// <summary>
    /// A variable name conflicts with a reserved identifier (built-in function or keyword).
    /// </summary>
    ReservedIdentifier = 3007,

    /// <summary>
    /// A variable was declared that already exists in the current scope.
    /// </summary>
    VariableAlreadyDeclared = 3008,

    #endregion

    #region Linking Errors (4000-4999)

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

    /// <summary>
    /// A function was called with fewer arguments than the minimum required.
    /// </summary>
    TooFewArguments = 4005,

    /// <summary>
    /// A function was called with more arguments than the maximum allowed.
    /// </summary>
    TooManyArguments = 4006,

    #endregion

    #region Execution Errors (5000-5999)

    #region General Execution Errors (5000-5099)

    /// <summary>
    /// The `return` keyword was used, indicating normal/successful script return
    /// </summary>
    ScriptReturn = 5000,

    /// <summary>
    /// An unexpected error occurred during execution that doesn't fit other categories.
    /// This typically indicates an internal executor error or system-level issue.
    /// </summary>
    UnknownExecutorError = 5001,

    /// <summary>
    /// A general runtime error occurred during script execution.
    /// This is a catch-all category for execution-time issues that don't fit specific categories.
    /// </summary>
    RuntimeError = 5002,

    /// <summary>
    /// Script execution was cancelled by the host application or execution environment.
    /// This occurs when external systems terminate execution before natural completion.
    /// </summary>
    CancelledByHost = 5003,

    /// <summary>
    /// An operation was attempted on a value of an incompatible type.
    /// This error occurs at runtime when type checking reveals incompatible operations.
    /// </summary>
    InvalidType = 5004,

    /// <summary>
    /// A function was called with an argument of an incorrect type for the expected parameter.
    /// This error enforces type safety at function call boundaries during execution.
    /// </summary>
    InvalidArgumentType = 5005,

    #endregion

    #region Arithmetic Errors (5100-5199)

    /// <summary>
    /// A division or modulo operation attempted to divide by zero.
    /// </summary>
    DivisionByZero = 5100,

    /// <summary>
    /// A unary negation operator was applied to a non-numeric value.
    /// </summary>
    NegateNonNumber = 5101,

    /// <summary>
    /// An increment or decrement operator was applied to a non-numeric value.
    /// </summary>
    IncrementDecrementNonNumber = 5102,

    /// <summary>
    /// An arithmetic operation was attempted on incompatible operand types.
    /// This includes add, subtract, multiply, divide, and modulo on mismatched types.
    /// </summary>
    IncompatibleOperandTypes = 5103,

    /// <summary>
    /// A comparison operation was attempted on incompatible types.
    /// </summary>
    IncompatibleComparison = 5104,

    #endregion

    #region Collection Access Errors (5200-5299)

    /// <summary>
    /// An array index was out of the valid bounds of the array.
    /// </summary>
    IndexOutOfRange = 5200,

    /// <summary>
    /// An array index was negative, which is not allowed.
    /// </summary>
    NegativeIndex = 5201,

    /// <summary>
    /// An index access operation was attempted on a null value.
    /// </summary>
    IndexAccessOnNull = 5202,

    /// <summary>
    /// An index access operation was attempted on a value that is neither an array nor an object.
    /// </summary>
    InvalidIndexTarget = 5203,

    /// <summary>
    /// A property access operation was attempted on a null value.
    /// </summary>
    PropertyAccessOnNull = 5204,

    /// <summary>
    /// A property access operation was attempted on a value that does not support properties.
    /// </summary>
    PropertyAccessInvalidType = 5205,

    #endregion

    #region Type Errors (5300-5399)

    /// <summary>
    /// A type check operation was provided with an invalid type specifier.
    /// Type checks require a string type name or a type keyword.
    /// </summary>
    InvalidTypeCheck = 5300,

    /// <summary>
    /// A type check operation referenced an unknown type name.
    /// Valid type names are: number, string, boolean, object, array, null.
    /// </summary>
    UnknownTypeName = 5301,

    /// <summary>
    /// A foreach loop attempted to iterate over a non-iterable value.
    /// Only arrays and objects can be iterated.
    /// </summary>
    NotIterable = 5302,

    #endregion

    #region Function Errors (5400-5499)

    /// <summary>
    /// A function was called that was not found in the execution environment at runtime.
    /// </summary>
    UndefinedFunctionRuntime = 5400,

    /// <summary>
    /// A function call was attempted on a value that is not a valid function target.
    /// Only named functions can be called.
    /// </summary>
    InvalidFunctionTarget = 5401,

    /// <summary>
    /// CallScriptByName was called but no script resolver was configured on the execution context.
    /// </summary>
    ScriptResolverNotConfigured = 5402,

    /// <summary>
    /// CallScriptByName was called with a script name that the resolver could not find.
    /// </summary>
    ScriptNotFound = 5403,

    #endregion

    #region Internal/Syntax Errors (5500-5599)

    /// <summary>
    /// An internal error occurred due to invalid expression syntax.
    /// This typically indicates an internal interpreter error.
    /// </summary>
    InvalidExpressionSyntax = 5500,

    /// <summary>
    /// An unknown operator was encountered during expression evaluation.
    /// </summary>
    UnknownOperator = 5501,

    #endregion

    #region Resource Limit Errors (5600-5699)

    /// <summary>
    /// Script execution exceeded the maximum allowed statement count.
    /// </summary>
    StatementLimitExceeded = 5600,

    /// <summary>
    /// Script execution exceeded the maximum allowed loop iteration count.
    /// </summary>
    LoopIterationLimitExceeded = 5601,

    /// <summary>
    /// Script execution exceeded the maximum allowed call stack depth.
    /// </summary>
    CallDepthLimitExceeded = 5602,

    /// <summary>
    /// Script execution exceeded the maximum allowed script call chain depth.
    /// </summary>
    ScriptCallDepthLimitExceeded = 5603,

    /// <summary>
    /// Script execution exceeded the maximum allowed execution time.
    /// </summary>
    ExecutionTimeLimitExceeded = 5604,

    #endregion

    #region Parse Errors (5700-5799)

    /// <summary>
    /// A numeric literal could not be parsed into a valid number at runtime.
    /// </summary>
    InvalidNumberParse = 5700,

    #endregion

    #region Fail keyword

    /// <summary>
    /// The `fail` keyword was used, indicating a business-logic failure and early (unsuccessful) return
    /// </summary>
    ScriptFailure = 5999

    #endregion

    #endregion // Execution Errors
}