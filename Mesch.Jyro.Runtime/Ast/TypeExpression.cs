namespace Mesch.Jyro;

/// <summary>
/// Represents a type expression in the abstract syntax tree.
/// Type expressions represent references to type names used in variable declarations,
/// type checking operations, and type annotations. These expressions evaluate to
/// type tokens that identify specific types within the Jyro type system.
/// </summary>
/// <remarks>
/// Type expressions provide compile-time type information that can be used for:
/// <list type="bullet">
/// <item><description>Variable type hints: var name: number</description></item>
/// <item><description>Type checking expressions: value is string</description></item>
/// <item><description>Function parameter typing (in future extensions)</description></item>
/// </list>
/// The type tokens correspond to the fundamental types in the Jyro type system:
/// number, string, boolean, array, object, and null.
/// </remarks>
public sealed class TypeExpression : IExpression
{
    /// <summary>
    /// Initializes a new instance of the <see cref="TypeExpression"/> class with the specified
    /// type token and source location information.
    /// </summary>
    /// <param name="type">
    /// The type token that identifies which type this expression represents.
    /// This should be one of the type-related tokens such as NumberType, StringType,
    /// BooleanType, ArrayType, or ObjectType.
    /// </param>
    /// <param name="lineNumber">The line number in the source code where this type expression appears.</param>
    /// <param name="columnPosition">The column position in the source code where this type expression begins.</param>
    public TypeExpression(JyroTokenType type, int lineNumber, int columnPosition)
    {
        Type = type;
        LineNumber = lineNumber;
        ColumnPosition = columnPosition;
    }

    /// <summary>
    /// Gets the type token that represents the specific type referenced by this expression.
    /// This token identifies which type from the Jyro type system this expression refers to,
    /// such as NumberType for numeric types or StringType for string types.
    /// </summary>
    public JyroTokenType Type { get; }

    /// <summary>
    /// Gets the line number in the source code where this type expression appears.
    /// </summary>
    public int LineNumber { get; }

    /// <summary>
    /// Gets the column position in the source code where this type expression begins.
    /// </summary>
    public int ColumnPosition { get; }

    /// <summary>
    /// Accepts a visitor for traversal using the Visitor pattern.
    /// This method enables various operations to be performed on the AST node
    /// without modifying the node structure itself.
    /// </summary>
    /// <param name="visitor">The visitor instance that will process this type expression.</param>
    public void Accept(IVisitor visitor) => visitor.VisitTypeExpression(this);

    /// <summary>
    /// Accepts a visitor for traversal using the Visitor pattern with a return value.
    /// This method enables various operations to be performed on the AST node
    /// that produce a result of the specified type.
    /// </summary>
    /// <typeparam name="T">The type of result produced by the visitor.</typeparam>
    /// <param name="visitor">The visitor instance that will process this type expression.</param>
    /// <returns>The result produced by the visitor's processing of this type expression.</returns>
    public T Accept<T>(IVisitor<T> visitor) => visitor.VisitTypeExpression(this);
}