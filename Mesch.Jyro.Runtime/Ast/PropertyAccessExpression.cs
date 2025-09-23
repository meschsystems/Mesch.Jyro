namespace Mesch.Jyro;

/// <summary>
/// Represents a property access expression in the abstract syntax tree.
/// Property access expressions use dot notation to access properties of objects,
/// such as 'person.name' or 'data.items'. The property name is known at compile time
/// and specified as a string literal identifier.
/// </summary>
/// <remarks>
/// Property access expressions provide convenient syntax for accessing object properties
/// using familiar dot notation. This is equivalent to index access with a string key
/// (object["property"]) but with more concise syntax for known property names.
/// 
/// <para>
/// The target expression is evaluated first to obtain the object, then the property
/// is accessed using the specified property name. If the target is not an object or
/// the property does not exist, the result is typically a null value.
/// </para>
/// </remarks>
public sealed class PropertyAccessExpression : IExpression
{
    /// <summary>
    /// Initializes a new instance of the <see cref="PropertyAccessExpression"/> class with the specified
    /// target expression, property name, and source location information.
    /// </summary>
    /// <param name="target">
    /// The expression that evaluates to the object whose property will be accessed.
    /// This expression should produce an object value, though the runtime will handle
    /// non-object values gracefully by returning null.
    /// </param>
    /// <param name="propertyName">
    /// The name of the property to access on the target object.
    /// This is a compile-time constant string that identifies the property.
    /// </param>
    /// <param name="lineNumber">The line number in the source code where this property access appears.</param>
    /// <param name="columnPosition">The column position in the source code where this property access begins.</param>
    /// <exception cref="ArgumentNullException">Thrown when target or propertyName parameter is null.</exception>
    public PropertyAccessExpression(IExpression target, string propertyName, int lineNumber, int columnPosition)
    {
        Target = target ?? throw new ArgumentNullException(nameof(target));
        PropertyName = propertyName ?? throw new ArgumentNullException(nameof(propertyName));
        LineNumber = lineNumber;
        ColumnPosition = columnPosition;
    }

    /// <summary>
    /// Gets the target expression that evaluates to the object being accessed.
    /// This expression is evaluated first to determine the object from which
    /// the property will be retrieved.
    /// </summary>
    public IExpression Target { get; }

    /// <summary>
    /// Gets the name of the property being accessed on the target object.
    /// This is a compile-time constant that specifies which property to retrieve
    /// from the object produced by evaluating the target expression.
    /// </summary>
    public string PropertyName { get; }

    /// <summary>
    /// Gets the line number in the source code where this property access expression appears.
    /// </summary>
    public int LineNumber { get; }

    /// <summary>
    /// Gets the column position in the source code where this property access expression begins.
    /// </summary>
    public int ColumnPosition { get; }

    /// <summary>
    /// Accepts a visitor for traversal using the Visitor pattern.
    /// This method enables various operations to be performed on the AST node
    /// without modifying the node structure itself.
    /// </summary>
    /// <param name="visitor">The visitor instance that will process this property access expression.</param>
    public void Accept(IVisitor visitor) => visitor.VisitPropertyAccessExpression(this);

    /// <summary>
    /// Accepts a visitor for traversal using the Visitor pattern with a return value.
    /// This method enables various operations to be performed on the AST node
    /// that produce a result of the specified type.
    /// </summary>
    /// <typeparam name="T">The type of result produced by the visitor.</typeparam>
    /// <param name="visitor">The visitor instance that will process this property access expression.</param>
    /// <returns>The result produced by the visitor's processing of this property access expression.</returns>
    public T Accept<T>(IVisitor<T> visitor) => visitor.VisitPropertyAccessExpression(this);
}