namespace Mesch.Jyro;

/// <summary>
/// Represents a binary operation expression in the abstract syntax tree.
/// Binary expressions consist of two operands and an operator, such as arithmetic operations
/// (a + b, x * y), comparison operations (x == y, a &lt; b), or logical operations (p and q, x or y).
/// </summary>
/// <remarks>
/// Binary expressions follow operator precedence rules during evaluation and support
/// type-specific operations based on the operand types. The operator determines both
/// the semantic meaning and the evaluation behavior of the expression.
/// </remarks>
public sealed class BinaryExpression : IExpression
{
    /// <summary>
    /// Initializes a new instance of the <see cref="BinaryExpression"/> class with the specified operands,
    /// operator, and source location information.
    /// </summary>
    /// <param name="left">The left operand of the binary expression.</param>
    /// <param name="operator">The binary operator that defines the operation to perform.</param>
    /// <param name="right">The right operand of the binary expression.</param>
    /// <param name="lineNumber">The line number in the source code where this expression appears.</param>
    /// <param name="columnPosition">The column position in the source code where this expression begins.</param>
    /// <exception cref="ArgumentNullException">Thrown when left or right operand is null.</exception>
    public BinaryExpression(
        IExpression left,
        JyroTokenType @operator,
        IExpression right,
        int lineNumber,
        int columnPosition)
    {
        Left = left ?? throw new ArgumentNullException(nameof(left));
        Operator = @operator;
        Right = right ?? throw new ArgumentNullException(nameof(right));
        LineNumber = lineNumber;
        ColumnPosition = columnPosition;
    }

    /// <summary>
    /// Gets the left operand of the binary expression.
    /// This operand is evaluated first and serves as the left-hand side of the operation.
    /// </summary>
    public IExpression Left { get; }

    /// <summary>
    /// Gets the binary operator that defines the operation to perform between the operands.
    /// The operator determines the semantic meaning and evaluation behavior of the expression.
    /// </summary>
    public JyroTokenType Operator { get; }

    /// <summary>
    /// Gets the right operand of the binary expression.
    /// This operand is evaluated second and serves as the right-hand side of the operation.
    /// </summary>
    public IExpression Right { get; }

    /// <summary>
    /// Gets the line number in the source code where this binary expression appears.
    /// </summary>
    public int LineNumber { get; }

    /// <summary>
    /// Gets the column position in the source code where this binary expression begins.
    /// </summary>
    public int ColumnPosition { get; }

    /// <summary>
    /// Accepts a visitor for traversal using the Visitor pattern.
    /// This method enables various operations to be performed on the AST node
    /// without modifying the node structure itself.
    /// </summary>
    /// <param name="visitor">The visitor instance that will process this binary expression.</param>
    public void Accept(IVisitor visitor) => visitor.VisitBinaryExpression(this);

    /// <summary>
    /// Accepts a visitor for traversal using the Visitor pattern with a return value.
    /// This method enables various operations to be performed on the AST node
    /// that produce a result of the specified type.
    /// </summary>
    /// <typeparam name="T">The type of result produced by the visitor.</typeparam>
    /// <param name="visitor">The visitor instance that will process this binary expression.</param>
    /// <returns>The result produced by the visitor's processing of this binary expression.</returns>
    public T Accept<T>(IVisitor<T> visitor) => visitor.VisitBinaryExpression(this);
}