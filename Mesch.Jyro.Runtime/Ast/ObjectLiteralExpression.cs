namespace Mesch.Jyro;

/// <summary>
/// Represents an object literal expression in the abstract syntax tree.
/// Object literals define objects using brace notation with key-value pairs,
/// such as { name: "John", age: 30 } or { "key": value, "other": expression }.
/// Properties can have string literal keys or computed keys using bracket notation.
/// </summary>
/// <remarks>
/// Object literal expressions support different property definition styles:
/// <list type="bullet">
/// <item><description>Simple properties: { name: "value", count: 42 }</description></item>
/// <item><description>String literal keys: { "complex key": value }</description></item>
/// <item><description>Computed keys: { [expression]: value }</description></item>
/// <item><description>Mixed property types within the same object</description></item>
/// </list>
/// During evaluation, each property's key and value expressions are evaluated
/// to create a new JyroObject containing the resulting key-value pairs.
/// </remarks>
public sealed class ObjectLiteralExpression : IExpression
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ObjectLiteralExpression"/> class with the specified
    /// property definitions and source location information.
    /// </summary>
    /// <param name="properties">
    /// The collection of property definitions that comprise this object literal.
    /// Each property defines a key-value pair that will be included in the resulting object.
    /// </param>
    /// <param name="lineNumber">The line number in the source code where this object literal appears.</param>
    /// <param name="columnPosition">The column position in the source code where this object literal begins.</param>
    /// <exception cref="ArgumentNullException">Thrown when properties collection is null.</exception>
    public ObjectLiteralExpression(IReadOnlyList<ObjectProperty> properties, int lineNumber, int columnPosition)
    {
        Properties = properties ?? throw new ArgumentNullException(nameof(properties));
        LineNumber = lineNumber;
        ColumnPosition = columnPosition;
    }

    /// <summary>
    /// Gets the collection of property definitions that comprise this object literal.
    /// Each property specifies a key-value pair that will be evaluated and included
    /// in the resulting object. The collection may be empty for empty object literals ({}).
    /// </summary>
    public IReadOnlyList<ObjectProperty> Properties { get; }

    /// <summary>
    /// Gets the line number in the source code where this object literal expression appears.
    /// </summary>
    public int LineNumber { get; }

    /// <summary>
    /// Gets the column position in the source code where this object literal expression begins.
    /// </summary>
    public int ColumnPosition { get; }

    /// <summary>
    /// Accepts a visitor for traversal using the Visitor pattern.
    /// This method enables various operations to be performed on the AST node
    /// without modifying the node structure itself.
    /// </summary>
    /// <param name="visitor">The visitor instance that will process this object literal expression.</param>
    public void Accept(IVisitor visitor) => visitor.VisitObjectLiteralExpression(this);

    /// <summary>
    /// Accepts a visitor for traversal using the Visitor pattern with a return value.
    /// This method enables various operations to be performed on the AST node
    /// that produce a result of the specified type.
    /// </summary>
    /// <typeparam name="T">The type of result produced by the visitor.</typeparam>
    /// <param name="visitor">The visitor instance that will process this object literal expression.</param>
    /// <returns>The result produced by the visitor's processing of this object literal expression.</returns>
    public T Accept<T>(IVisitor<T> visitor) => visitor.VisitObjectLiteralExpression(this);
}