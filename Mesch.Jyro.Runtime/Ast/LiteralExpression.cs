namespace Mesch.Jyro;

/// <summary>
/// Represents a literal constant expression in the abstract syntax tree.
/// Literal expressions contain compile-time constant values such as numbers, strings,
/// booleans, or null values that are directly embedded in the source code.
/// </summary>
/// <remarks>
/// Literal expressions represent the following constant value types:
/// <list type="bullet">
/// <item><description>Numeric literals: 42, 3.14, -7</description></item>
/// <item><description>String literals: "hello", "world"</description></item>
/// <item><description>Boolean literals: true, false</description></item>
/// <item><description>Null literal: null</description></item>
/// </list>
/// These values are known at compile time and do not require evaluation during execution,
/// making them efficient building blocks for more complex expressions.
/// </remarks>
public sealed class LiteralExpression : IExpression
{
    /// <summary>
    /// Initializes a new instance of the <see cref="LiteralExpression"/> class with the specified
    /// constant value and source location information.
    /// </summary>
    /// <param name="value">
    /// The constant value represented by this literal expression.
    /// This can be a number (double), string, boolean, or null value.
    /// </param>
    /// <param name="lineNumber">The line number in the source code where this literal appears.</param>
    /// <param name="columnPosition">The column position in the source code where this literal begins.</param>
    public LiteralExpression(object? value, int lineNumber, int columnPosition)
    {
        Value = value;
        LineNumber = lineNumber;
        ColumnPosition = columnPosition;
    }

    /// <summary>
    /// Gets the constant value represented by this literal expression.
    /// This value is determined at parse time and remains constant throughout
    /// the lifetime of the expression. The value can be a number, string, boolean, or null.
    /// </summary>
    public object? Value { get; }

    /// <summary>
    /// Gets the line number in the source code where this literal expression appears.
    /// </summary>
    public int LineNumber { get; }

    /// <summary>
    /// Gets the column position in the source code where this literal expression begins.
    /// </summary>
    public int ColumnPosition { get; }

    /// <summary>
    /// Accepts a visitor for traversal using the Visitor pattern.
    /// This method enables various operations to be performed on the AST node
    /// without modifying the node structure itself.
    /// </summary>
    /// <param name="visitor">The visitor instance that will process this literal expression.</param>
    public void Accept(IVisitor visitor) => visitor.VisitLiteralExpression(this);

    /// <summary>
    /// Accepts a visitor for traversal using the Visitor pattern with a return value.
    /// This method enables various operations to be performed on the AST node
    /// that produce a result of the specified type.
    /// </summary>
    /// <typeparam name="T">The type of result produced by the visitor.</typeparam>
    /// <param name="visitor">The visitor instance that will process this literal expression.</param>
    /// <returns>The result produced by the visitor's processing of this literal expression.</returns>
    public T Accept<T>(IVisitor<T> visitor) => visitor.VisitLiteralExpression(this);
}