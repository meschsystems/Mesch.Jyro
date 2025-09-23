namespace Mesch.Jyro;

/// <summary>
/// Represents an array literal expression in the abstract syntax tree.
/// Array literals define arrays using bracket notation with comma-separated elements,
/// such as [1, 2, 3] or ["hello", "world"]. Elements can be any valid expression.
/// </summary>
/// <remarks>
/// Array literals are evaluated by first evaluating each element expression in order,
/// then creating a new JyroArray containing the resulting values. The elements can be
/// of mixed types, as Jyro arrays are heterogeneous collections that can contain any
/// combination of values including numbers, strings, objects, and nested arrays.
/// </remarks>
public sealed class ArrayLiteralExpression : IExpression
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ArrayLiteralExpression"/> class with the specified
    /// element expressions and source location information.
    /// </summary>
    /// <param name="elements">
    /// The collection of expressions that represent the elements of the array literal.
    /// Each expression will be evaluated to produce the corresponding array element.
    /// </param>
    /// <param name="lineNumber">The line number in the source code where this array literal appears.</param>
    /// <param name="columnPosition">The column position in the source code where this array literal begins.</param>
    /// <exception cref="ArgumentNullException">Thrown when elements collection is null.</exception>
    public ArrayLiteralExpression(IReadOnlyList<IExpression> elements, int lineNumber, int columnPosition)
    {
        Elements = elements ?? throw new ArgumentNullException(nameof(elements));
        LineNumber = lineNumber;
        ColumnPosition = columnPosition;
    }

    /// <summary>
    /// Gets the collection of expressions that represent the elements of the array literal.
    /// These expressions are evaluated in order during execution to produce the array contents.
    /// The collection may be empty for empty array literals ([]).
    /// </summary>
    public IReadOnlyList<IExpression> Elements { get; }

    /// <summary>
    /// Gets the line number in the source code where this array literal expression appears.
    /// </summary>
    public int LineNumber { get; }

    /// <summary>
    /// Gets the column position in the source code where this array literal expression begins.
    /// </summary>
    public int ColumnPosition { get; }

    /// <summary>
    /// Accepts a visitor for traversal using the Visitor pattern.
    /// This method enables various operations to be performed on the AST node
    /// without modifying the node structure itself.
    /// </summary>
    /// <param name="visitor">The visitor instance that will process this array literal expression.</param>
    public void Accept(IVisitor visitor) => visitor.VisitArrayLiteralExpression(this);

    /// <summary>
    /// Accepts a visitor for traversal using the Visitor pattern with a return value.
    /// This method enables various operations to be performed on the AST node
    /// that produce a result of the specified type.
    /// </summary>
    /// <typeparam name="T">The type of result produced by the visitor.</typeparam>
    /// <param name="visitor">The visitor instance that will process this array literal expression.</param>
    /// <returns>The result produced by the visitor's processing of this array literal expression.</returns>
    public T Accept<T>(IVisitor<T> visitor) => visitor.VisitArrayLiteralExpression(this);
}