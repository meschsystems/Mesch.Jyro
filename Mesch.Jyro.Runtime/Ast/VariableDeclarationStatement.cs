namespace Mesch.Jyro;

/// <summary>
/// Represents a variable declaration statement in the abstract syntax tree.
/// Variable declarations introduce new variables into the current scope with optional type hints
/// and optional initial values. This statement supports both explicit typing and type inference
/// based on the initializer expression.
/// </summary>
/// <remarks>
/// Variable declaration statements support several declaration patterns:
/// <list type="bullet">
/// <item><description>Simple declaration: var name</description></item>
/// <item><description>Declaration with type hint: var name: type</description></item>
/// <item><description>Declaration with initializer: var name = expression</description></item>
/// <item><description>Full declaration: var name: type = expression</description></item>
/// </list>
/// When a type hint is provided, the runtime may perform type checking to ensure the initial
/// value and subsequent assignments are compatible. When only an initializer is provided,
/// the variable's type is inferred from the initial value.
/// </remarks>
public sealed class VariableDeclarationStatement : IJyroStatement
{
    /// <summary>
    /// Initializes a new instance of the <see cref="VariableDeclarationStatement"/> class with the specified
    /// variable name, optional type hint, optional initializer, and source location information.
    /// </summary>
    /// <param name="name">
    /// The name of the variable being declared. This name will be used to identify
    /// the variable in the current scope and must be a valid identifier.
    /// </param>
    /// <param name="typeHint">
    /// The optional type hint expression that specifies the expected type of the variable.
    /// When provided, this may be used for type checking during validation and execution.
    /// </param>
    /// <param name="initializer">
    /// The optional initializer expression that provides the initial value for the variable.
    /// When provided, this expression is evaluated and its result is assigned to the variable.
    /// </param>
    /// <param name="lineNumber">The line number in the source code where this variable declaration appears.</param>
    /// <param name="columnPosition">The column position in the source code where this variable declaration begins.</param>
    /// <exception cref="ArgumentNullException">Thrown when the name parameter is null.</exception>
    public VariableDeclarationStatement(
        string name,
        IExpression? typeHint,
        IExpression? initializer,
        int lineNumber,
        int columnPosition)
    {
        Name = name ?? throw new ArgumentNullException(nameof(name));
        TypeHint = typeHint;
        Initializer = initializer;
        LineNumber = lineNumber;
        ColumnPosition = columnPosition;
    }

    /// <summary>
    /// Gets the name of the variable being declared.
    /// This name serves as the identifier for the variable within its scope
    /// and is used for subsequent variable references and assignments.
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// Gets the optional type hint expression that specifies the expected type of the variable.
    /// When present, this expression typically evaluates to a type token that can be used
    /// for type checking and validation. When null, no explicit type constraint is applied.
    /// </summary>
    public IExpression? TypeHint { get; }

    /// <summary>
    /// Gets the optional initializer expression that provides the initial value for the variable.
    /// When present, this expression is evaluated during variable declaration and its result
    /// becomes the initial value of the variable. When null, the variable is initialized with a null value.
    /// </summary>
    public IExpression? Initializer { get; }

    /// <summary>
    /// Gets the line number in the source code where this variable declaration statement appears.
    /// </summary>
    public int LineNumber { get; }

    /// <summary>
    /// Gets the column position in the source code where this variable declaration statement begins.
    /// </summary>
    public int ColumnPosition { get; }

    /// <summary>
    /// Accepts a visitor for traversal using the Visitor pattern.
    /// This method enables various operations to be performed on the AST node
    /// without modifying the node structure itself.
    /// </summary>
    /// <param name="visitor">The visitor instance that will process this variable declaration statement.</param>
    public void Accept(IVisitor visitor) => visitor.VisitVariableDeclarationStatement(this);

    /// <summary>
    /// Accepts a visitor for traversal using the Visitor pattern with a return value.
    /// This method enables various operations to be performed on the AST node
    /// that produce a result of the specified type.
    /// </summary>
    /// <typeparam name="T">The type of result produced by the visitor.</typeparam>
    /// <param name="visitor">The visitor instance that will process this variable declaration statement.</param>
    /// <returns>The result produced by the visitor's processing of this variable declaration statement.</returns>
    public T Accept<T>(IVisitor<T> visitor) => visitor.VisitVariableDeclarationStatement(this);
}