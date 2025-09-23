namespace Mesch.Jyro;

/// <summary>
/// Represents a while loop statement in the abstract syntax tree.
/// While loops provide conditional iteration by repeatedly executing a statement block
/// as long as a specified condition evaluates to true using truthiness semantics.
/// </summary>
/// <remarks>
/// While statements follow standard loop semantics where the condition is evaluated
/// before each iteration. If the condition is initially false, the loop body is never
/// executed. The loop continues until the condition becomes false or until a break
/// statement is encountered within the loop body.
/// 
/// <para>
/// The loop body maintains its own scope for variable declarations, and the condition
/// is re-evaluated before each iteration using Jyro's truthiness rules where different
/// value types have specific true/false semantics.
/// </para>
/// </remarks>
public sealed class WhileStatement : IJyroStatement
{
    /// <summary>
    /// Initializes a new instance of the <see cref="WhileStatement"/> class with the specified
    /// condition expression, loop body, and source location information.
    /// </summary>
    /// <param name="condition">
    /// The condition expression that determines whether the loop should continue iterating.
    /// This expression is evaluated before each iteration using truthiness semantics.
    /// </param>
    /// <param name="body">
    /// The collection of statements that comprise the body of the while loop.
    /// These statements are executed repeatedly as long as the condition remains true.
    /// </param>
    /// <param name="lineNumber">The line number in the source code where this while statement appears.</param>
    /// <param name="columnPosition">The column position in the source code where this while statement begins.</param>
    /// <exception cref="ArgumentNullException">Thrown when condition or body parameter is null.</exception>
    public WhileStatement(
        IExpression condition,
        IReadOnlyList<IJyroStatement> body,
        int lineNumber,
        int columnPosition)
    {
        Condition = condition ?? throw new ArgumentNullException(nameof(condition));
        Body = body ?? throw new ArgumentNullException(nameof(body));
        LineNumber = lineNumber;
        ColumnPosition = columnPosition;
    }

    /// <summary>
    /// Gets the condition expression that controls loop iteration.
    /// This expression is evaluated before each iteration to determine whether
    /// the loop should continue executing. The evaluation uses truthiness semantics.
    /// </summary>
    public IExpression Condition { get; }

    /// <summary>
    /// Gets the collection of statements that comprise the body of the while loop.
    /// These statements are executed in order during each iteration of the loop,
    /// and the loop body maintains its own scope for variable declarations.
    /// </summary>
    public IReadOnlyList<IJyroStatement> Body { get; }

    /// <summary>
    /// Gets the line number in the source code where this while statement appears.
    /// </summary>
    public int LineNumber { get; }

    /// <summary>
    /// Gets the column position in the source code where this while statement begins.
    /// </summary>
    public int ColumnPosition { get; }

    /// <summary>
    /// Accepts a visitor for traversal using the Visitor pattern.
    /// This method enables various operations to be performed on the AST node
    /// without modifying the node structure itself.
    /// </summary>
    /// <param name="visitor">The visitor instance that will process this while statement.</param>
    public void Accept(IVisitor visitor) => visitor.VisitWhileStatement(this);

    /// <summary>
    /// Accepts a visitor for traversal using the Visitor pattern with a return value.
    /// This method enables various operations to be performed on the AST node
    /// that produce a result of the specified type.
    /// </summary>
    /// <typeparam name="T">The type of result produced by the visitor.</typeparam>
    /// <param name="visitor">The visitor instance that will process this while statement.</param>
    /// <returns>The result produced by the visitor's processing of this while statement.</returns>
    public T Accept<T>(IVisitor<T> visitor) => visitor.VisitWhileStatement(this);
}