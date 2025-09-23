namespace Mesch.Jyro;

/// <summary>
/// Represents an index access expression in the abstract syntax tree.
/// Index access expressions provide element access for collections using bracket notation,
/// such as array[0], string[5], or object["key"]. The index can be any expression that
/// evaluates to an appropriate key or numeric index.
/// </summary>
/// <remarks>
/// Index access expressions support different access patterns based on the target type:
/// <list type="bullet">
/// <item><description>Arrays: Numeric indices to access elements by position</description></item>
/// <item><description>Strings: Numeric indices to access individual characters</description></item>
/// <item><description>Objects: String keys to access property values by name</description></item>
/// </list>
/// The target expression is evaluated first to determine the collection, then the index
/// expression is evaluated to determine the specific element to access.
/// </remarks>
public sealed class IndexAccessExpression : IExpression
{
    /// <summary>
    /// Initializes a new instance of the <see cref="IndexAccessExpression"/> class with the specified
    /// target expression, index expression, and source location information.
    /// </summary>
    /// <param name="target">
    /// The expression that evaluates to the collection being indexed.
    /// This can be any expression that produces an indexable value such as an array, string, or object.
    /// </param>
    /// <param name="index">
    /// The expression that evaluates to the index or key used to access the collection element.
    /// For arrays and strings, this should evaluate to a numeric value.
    /// For objects, this should evaluate to a string key.
    /// </param>
    /// <param name="lineNumber">The line number in the source code where this index access appears.</param>
    /// <param name="columnPosition">The column position in the source code where this index access begins.</param>
    /// <exception cref="ArgumentNullException">Thrown when target or index parameter is null.</exception>
    public IndexAccessExpression(IExpression target, IExpression index, int lineNumber, int columnPosition)
    {
        Target = target ?? throw new ArgumentNullException(nameof(target));
        Index = index ?? throw new ArgumentNullException(nameof(index));
        LineNumber = lineNumber;
        ColumnPosition = columnPosition;
    }

    /// <summary>
    /// Gets the target expression that evaluates to the collection being indexed.
    /// This expression is evaluated first to determine the collection that will be accessed.
    /// </summary>
    public IExpression Target { get; }

    /// <summary>
    /// Gets the index expression that evaluates to the key or index for element access.
    /// This expression is evaluated second to determine which element of the collection to retrieve.
    /// </summary>
    public IExpression Index { get; }

    /// <summary>
    /// Gets the line number in the source code where this index access expression appears.
    /// </summary>
    public int LineNumber { get; }

    /// <summary>
    /// Gets the column position in the source code where this index access expression begins.
    /// </summary>
    public int ColumnPosition { get; }

    /// <summary>
    /// Accepts a visitor for traversal using the Visitor pattern.
    /// This method enables various operations to be performed on the AST node
    /// without modifying the node structure itself.
    /// </summary>
    /// <param name="visitor">The visitor instance that will process this index access expression.</param>
    public void Accept(IVisitor visitor) => visitor.VisitIndexAccessExpression(this);

    /// <summary>
    /// Accepts a visitor for traversal using the Visitor pattern with a return value.
    /// This method enables various operations to be performed on the AST node
    /// that produce a result of the specified type.
    /// </summary>
    /// <typeparam name="T">The type of result produced by the visitor.</typeparam>
    /// <param name="visitor">The visitor instance that will process this index access expression.</param>
    /// <returns>The result produced by the visitor's processing of this index access expression.</returns>
    public T Accept<T>(IVisitor<T> visitor) => visitor.VisitIndexAccessExpression(this);
}