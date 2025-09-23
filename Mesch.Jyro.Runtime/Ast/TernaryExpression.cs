namespace Mesch.Jyro;

/// <summary>
/// Represents a ternary conditional expression that evaluates a condition and returns one of two expressions
/// based on the truthiness of the condition. Follows the syntax: condition ? trueExpression : falseExpression
/// </summary>
/// <remarks>
/// The ternary operator provides a concise way to perform conditional evaluation within expressions.
/// The condition is evaluated first, and if it evaluates to a truthy value, the true expression is returned;
/// otherwise, the false expression is returned. Only one of the two result expressions is evaluated,
/// providing short-circuit evaluation behavior.
/// 
/// Ternary expressions are right-associative, meaning nested ternary operators are grouped from right to left.
/// For example: a ? b : c ? d : e is parsed as a ? b : (c ? d : e)
/// 
/// The ternary operator has lower precedence than logical OR but higher precedence than assignment operations.
/// </remarks>
public sealed class TernaryExpression : IExpression
{
    /// <summary>
    /// Initializes a new instance of the TernaryExpression class with the specified condition and result expressions.
    /// </summary>
    /// <param name="condition">The expression to evaluate for truthiness.</param>
    /// <param name="trueExpression">The expression to return if the condition is truthy.</param>
    /// <param name="falseExpression">The expression to return if the condition is falsy.</param>
    /// <param name="lineNumber">The line number where this expression appears in the source code.</param>
    /// <param name="columnPosition">The column position where this expression appears in the source code.</param>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="condition"/>, <paramref name="trueExpression"/>, or <paramref name="falseExpression"/> is null.
    /// </exception>
    public TernaryExpression(
        IExpression condition,
        IExpression trueExpression,
        IExpression falseExpression,
        int lineNumber,
        int columnPosition)
    {
        Condition = condition ?? throw new ArgumentNullException(nameof(condition));
        TrueExpression = trueExpression ?? throw new ArgumentNullException(nameof(trueExpression));
        FalseExpression = falseExpression ?? throw new ArgumentNullException(nameof(falseExpression));
        LineNumber = lineNumber;
        ColumnPosition = columnPosition;
    }

    /// <summary>
    /// Gets the condition expression that determines which result expression to evaluate.
    /// </summary>
    /// <value>
    /// The expression that is evaluated for truthiness to determine the result of the ternary operation.
    /// </value>
    public IExpression Condition { get; }

    /// <summary>
    /// Gets the expression that is returned when the condition evaluates to a truthy value.
    /// </summary>
    /// <value>
    /// The expression to evaluate and return if the condition is truthy.
    /// </value>
    public IExpression TrueExpression { get; }

    /// <summary>
    /// Gets the expression that is returned when the condition evaluates to a falsy value.
    /// </summary>
    /// <value>
    /// The expression to evaluate and return if the condition is falsy.
    /// </value>
    public IExpression FalseExpression { get; }

    /// <summary>
    /// Gets the line number where this ternary expression appears in the source code.
    /// </summary>
    /// <value>
    /// The one-based line number of the expression's location in the source text.
    /// </value>
    public int LineNumber { get; }

    /// <summary>
    /// Gets the column position where this ternary expression appears in the source code.
    /// </summary>
    /// <value>
    /// The one-based column position of the expression's location in the source text.
    /// </value>
    public int ColumnPosition { get; }

    /// <summary>
    /// Accepts a visitor for processing this ternary expression using the visitor pattern.
    /// </summary>
    /// <param name="visitor">The visitor that will process this expression.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="visitor"/> is null.</exception>
    public void Accept(IVisitor visitor) => visitor.VisitTernaryExpression(this);

    /// <summary>
    /// Accepts a generic visitor for processing this ternary expression and returns a result.
    /// </summary>
    /// <typeparam name="T">The type of result returned by the visitor.</typeparam>
    /// <param name="visitor">The generic visitor that will process this expression.</param>
    /// <returns>The result of the visitor's processing of this ternary expression.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="visitor"/> is null.</exception>
    public T Accept<T>(IVisitor<T> visitor) => visitor.VisitTernaryExpression(this);
}