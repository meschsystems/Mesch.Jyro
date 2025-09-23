namespace Mesch.Jyro;

/// <summary>
/// Represents a function call expression in the abstract syntax tree.
/// Function calls invoke functions or methods with a specified set of arguments,
/// such as print("Hello") or calculate(x, y, z). The target can be a simple identifier
/// or a more complex expression that resolves to a callable function.
/// </summary>
/// <remarks>
/// Function call expressions support various calling patterns:
/// <list type="bullet">
/// <item><description>Built-in functions: upper("text"), length(array)</description></item>
/// <item><description>Host functions: custom functions provided by the host application</description></item>
/// <item><description>Method-style calls: object.method(args) (resolved as property access + call)</description></item>
/// </list>
/// Arguments are evaluated in order before the function is invoked, and the function
/// receives the evaluated values as its parameters.
/// </remarks>
public sealed class FunctionCallExpression : IExpression
{
    /// <summary>
    /// Initializes a new instance of the <see cref="FunctionCallExpression"/> class with the specified
    /// target expression, argument expressions, and source location information.
    /// </summary>
    /// <param name="target">
    /// The expression that identifies the function to be called. This is typically an identifier
    /// but can be any expression that resolves to a callable function.
    /// </param>
    /// <param name="arguments">
    /// The collection of expressions that represent the arguments to pass to the function.
    /// These expressions are evaluated in order before the function is invoked.
    /// </param>
    /// <param name="lineNumber">The line number in the source code where this function call appears.</param>
    /// <param name="columnPosition">The column position in the source code where this function call begins.</param>
    /// <exception cref="ArgumentNullException">Thrown when target or arguments parameter is null.</exception>
    public FunctionCallExpression(
        IExpression target,
        IReadOnlyList<IExpression> arguments,
        int lineNumber,
        int columnPosition)
    {
        Target = target ?? throw new ArgumentNullException(nameof(target));
        Arguments = arguments ?? throw new ArgumentNullException(nameof(arguments));
        LineNumber = lineNumber;
        ColumnPosition = columnPosition;
    }

    /// <summary>
    /// Gets the target expression that identifies the function to be called.
    /// This expression is evaluated to resolve the function that will be invoked
    /// with the provided arguments.
    /// </summary>
    public IExpression Target { get; }

    /// <summary>
    /// Gets the collection of argument expressions that will be passed to the function.
    /// These expressions are evaluated in order during the function call to produce
    /// the parameter values for the function invocation.
    /// </summary>
    public IReadOnlyList<IExpression> Arguments { get; }

    /// <summary>
    /// Gets the line number in the source code where this function call expression appears.
    /// </summary>
    public int LineNumber { get; }

    /// <summary>
    /// Gets the column position in the source code where this function call expression begins.
    /// </summary>
    public int ColumnPosition { get; }

    /// <summary>
    /// Accepts a visitor for traversal using the Visitor pattern.
    /// This method enables various operations to be performed on the AST node
    /// without modifying the node structure itself.
    /// </summary>
    /// <param name="visitor">The visitor instance that will process this function call expression.</param>
    public void Accept(IVisitor visitor) => visitor.VisitFunctionCallExpression(this);

    /// <summary>
    /// Accepts a visitor for traversal using the Visitor pattern with a return value.
    /// This method enables various operations to be performed on the AST node
    /// that produce a result of the specified type.
    /// </summary>
    /// <typeparam name="T">The type of result produced by the visitor.</typeparam>
    /// <param name="visitor">The visitor instance that will process this function call expression.</param>
    /// <returns>The result produced by the visitor's processing of this function call expression.</returns>
    public T Accept<T>(IVisitor<T> visitor) => visitor.VisitFunctionCallExpression(this);
}