namespace Mesch.Jyro;

/// <summary>
/// Represents a unary operation expression in the abstract syntax tree.
/// Unary expressions apply a single operator to one operand, such as arithmetic negation (-x),
/// logical negation (not y), or other prefix operators. The operator is applied to the result
/// of evaluating the operand expression.
/// </summary>
/// <remarks>
/// Unary expressions support various operator types:
/// <list type="bullet">
/// <item><description>Arithmetic negation: -expression (negates numeric values)</description></item>
/// <item><description>Logical negation: not expression (inverts boolean truthiness)</description></item>
/// <item><description>Alternative logical negation: !expression (equivalent to 'not')</description></item>
/// </list>
/// The operand expression is evaluated first, then the unary operator is applied to the result
/// according to the operator's semantics and the operand's type. Type-specific behavior is
/// defined by the JyroValue type system.
/// </remarks>
public sealed class UnaryExpression : IExpression
{
    /// <summary>
    /// Initializes a new instance of the <see cref="UnaryExpression"/> class with the specified
    /// operator, operand expression, and source location information.
    /// </summary>
    /// <param name="operator">
    /// The unary operator to apply to the operand. This determines the specific operation
    /// that will be performed, such as arithmetic negation or logical negation.
    /// </param>
    /// <param name="operand">
    /// The expression that serves as the operand for the unary operation.
    /// This expression is evaluated first, and then the operator is applied to its result.
    /// </param>
    /// <param name="lineNumber">The line number in the source code where this unary expression appears.</param>
    /// <param name="columnPosition">The column position in the source code where this unary expression begins.</param>
    /// <exception cref="ArgumentNullException">Thrown when the operand parameter is null.</exception>
    public UnaryExpression(
        JyroTokenType @operator,
        IExpression operand,
        int lineNumber,
        int columnPosition)
    {
        Operator = @operator;
        Operand = operand ?? throw new ArgumentNullException(nameof(operand));
        LineNumber = lineNumber;
        ColumnPosition = columnPosition;
    }

    /// <summary>
    /// Gets the unary operator that will be applied to the operand.
    /// This operator determines the specific operation to perform, such as arithmetic
    /// negation (Minus) or logical negation (Not).
    /// </summary>
    public JyroTokenType Operator { get; }

    /// <summary>
    /// Gets the expression that serves as the operand for this unary operation.
    /// This expression is evaluated first to produce the value that the unary operator
    /// will be applied to.
    /// </summary>
    public IExpression Operand { get; }

    /// <summary>
    /// Gets the line number in the source code where this unary expression appears.
    /// </summary>
    public int LineNumber { get; }

    /// <summary>
    /// Gets the column position in the source code where this unary expression begins.
    /// </summary>
    public int ColumnPosition { get; }

    /// <summary>
    /// Accepts a visitor for traversal using the Visitor pattern.
    /// This method enables various operations to be performed on the AST node
    /// without modifying the node structure itself.
    /// </summary>
    /// <param name="visitor">The visitor instance that will process this unary expression.</param>
    public void Accept(IVisitor visitor) => visitor.VisitUnaryExpression(this);

    /// <summary>
    /// Accepts a visitor for traversal using the Visitor pattern with a return value.
    /// This method enables various operations to be performed on the AST node
    /// that produce a result of the specified type.
    /// </summary>
    /// <typeparam name="T">The type of result produced by the visitor.</typeparam>
    /// <param name="visitor">The visitor instance that will process this unary expression.</param>
    /// <returns>The result produced by the visitor's processing of this unary expression.</returns>
    public T Accept<T>(IVisitor<T> visitor) => visitor.VisitUnaryExpression(this);
}