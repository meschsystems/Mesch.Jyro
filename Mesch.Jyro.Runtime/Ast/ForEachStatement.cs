namespace Mesch.Jyro;

/// <summary>
/// Represents a foreach iteration statement in the abstract syntax tree.
/// Foreach statements iterate over collections (arrays, strings, or objects) and execute
/// a block of statements for each element, with the iterator variable bound to the current element.
/// </summary>
/// <remarks>
/// Foreach statements support iteration over different collection types:
/// <list type="bullet">
/// <item><description>Arrays: Iterates over each element in order</description></item>
/// <item><description>Strings: Iterates over each character as a single-character string</description></item>
/// <item><description>Objects: Iterates over property values (not keys)</description></item>
/// </list>
/// The iterator variable is created in a new scope for the duration of the loop and
/// shadows any existing variable with the same name in outer scopes.
/// </remarks>
public sealed class ForEachStatement : IJyroStatement
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ForEachStatement"/> class with the specified
    /// iterator variable name, source expression, loop body, and source location information.
    /// </summary>
    /// <param name="iteratorName">
    /// The name of the variable that will be bound to each element during iteration.
    /// This variable is created in a new scope and is only accessible within the loop body.
    /// </param>
    /// <param name="source">
    /// The expression that evaluates to the collection to iterate over.
    /// This can be any expression that produces an iterable value (array, string, or object).
    /// </param>
    /// <param name="body">
    /// The collection of statements that comprise the loop body.
    /// These statements are executed once for each element in the source collection.
    /// </param>
    /// <param name="lineNumber">The line number in the source code where this foreach statement appears.</param>
    /// <param name="columnPosition">The column position in the source code where this foreach statement begins.</param>
    /// <exception cref="ArgumentNullException">Thrown when iteratorName, source, or body is null.</exception>
    public ForEachStatement(
        string iteratorName,
        IExpression source,
        IReadOnlyList<IJyroStatement> body,
        int lineNumber,
        int columnPosition)
    {
        IteratorName = iteratorName ?? throw new ArgumentNullException(nameof(iteratorName));
        Source = source ?? throw new ArgumentNullException(nameof(source));
        Body = body ?? throw new ArgumentNullException(nameof(body));
        LineNumber = lineNumber;
        ColumnPosition = columnPosition;
    }

    /// <summary>
    /// Gets the name of the iterator variable that will be bound to each element during iteration.
    /// This variable is created in a new scope for the duration of the loop and contains
    /// the current element value for each iteration.
    /// </summary>
    public string IteratorName { get; }

    /// <summary>
    /// Gets the expression that provides the collection to iterate over.
    /// This expression is evaluated once at the beginning of the loop to determine
    /// the collection elements that will be iterated over.
    /// </summary>
    public IExpression Source { get; }

    /// <summary>
    /// Gets the collection of statements that comprise the body of the foreach loop.
    /// These statements are executed once for each element in the source collection,
    /// with the iterator variable bound to the current element.
    /// </summary>
    public IReadOnlyList<IJyroStatement> Body { get; }

    /// <summary>
    /// Gets the line number in the source code where this foreach statement appears.
    /// </summary>
    public int LineNumber { get; }

    /// <summary>
    /// Gets the column position in the source code where this foreach statement begins.
    /// </summary>
    public int ColumnPosition { get; }

    /// <summary>
    /// Accepts a visitor for traversal using the Visitor pattern.
    /// This method enables various operations to be performed on the AST node
    /// without modifying the node structure itself.
    /// </summary>
    /// <param name="visitor">The visitor instance that will process this foreach statement.</param>
    public void Accept(IVisitor visitor) => visitor.VisitForEachStatement(this);

    /// <summary>
    /// Accepts a visitor for traversal using the Visitor pattern with a return value.
    /// This method enables various operations to be performed on the AST node
    /// that produce a result of the specified type.
    /// </summary>
    /// <typeparam name="T">The type of result produced by the visitor.</typeparam>
    /// <param name="visitor">The visitor instance that will process this foreach statement.</param>
    /// <returns>The result produced by the visitor's processing of this foreach statement.</returns>
    public T Accept<T>(IVisitor<T> visitor) => visitor.VisitForEachStatement(this);
}