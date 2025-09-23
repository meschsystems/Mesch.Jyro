namespace Mesch.Jyro;

/// <summary>
/// Represents an expression statement in the abstract syntax tree.
/// Expression statements evaluate an expression for its side effects and discard the result.
/// This allows expressions such as function calls to be used as standalone statements
/// even when their return values are not needed.
/// </summary>
/// <remarks>
/// Expression statements are commonly used for:
/// <list type="bullet">
/// <item><description>Function calls that perform actions: print("Hello")</description></item>
/// <item><description>Method calls with side effects: array.clear()</description></item>
/// <item><description>Operations that modify state: counter + 1 (when the result is not assigned)</description></item>
/// </list>
/// While the expression result is discarded at the statement level, any side effects
/// from evaluating the expression (such as function calls or property modifications) are preserved.
/// </remarks>
public sealed class ExpressionStatement : IJyroStatement
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ExpressionStatement"/> class with the specified
    /// expression and source location information.
    /// </summary>
    /// <param name="expression">
    /// The expression to evaluate. The result of this expression will be discarded,
    /// but any side effects from its evaluation will be preserved.
    /// </param>
    /// <param name="lineNumber">The line number in the source code where this expression statement appears.</param>
    /// <param name="columnPosition">The column position in the source code where this expression statement begins.</param>
    /// <exception cref="ArgumentNullException">Thrown when the expression parameter is null.</exception>
    public ExpressionStatement(IExpression expression, int lineNumber, int columnPosition)
    {
        Expression = expression ?? throw new ArgumentNullException(nameof(expression));
        LineNumber = lineNumber;
        ColumnPosition = columnPosition;
    }

    /// <summary>
    /// Gets the expression that will be evaluated when this statement is executed.
    /// The result of evaluating this expression is discarded, but any side effects are preserved.
    /// </summary>
    public IExpression Expression { get; }

    /// <summary>
    /// Gets the line number in the source code where this expression statement appears.
    /// </summary>
    public int LineNumber { get; }

    /// <summary>
    /// Gets the column position in the source code where this expression statement begins.
    /// </summary>
    public int ColumnPosition { get; }

    /// <summary>
    /// Accepts a visitor for traversal using the Visitor pattern.
    /// This method enables various operations to be performed on the AST node
    /// without modifying the node structure itself.
    /// </summary>
    /// <param name="visitor">The visitor instance that will process this expression statement.</param>
    public void Accept(IVisitor visitor) => visitor.VisitExpressionStatement(this);

    /// <summary>
    /// Accepts a visitor for traversal using the Visitor pattern with a return value.
    /// This method enables various operations to be performed on the AST node
    /// that produce a result of the specified type.
    /// </summary>
    /// <typeparam name="T">The type of result produced by the visitor.</typeparam>
    /// <param name="visitor">The visitor instance that will process this expression statement.</param>
    /// <returns>The result produced by the visitor's processing of this expression statement.</returns>
    public T Accept<T>(IVisitor<T> visitor) => visitor.VisitExpressionStatement(this);
}