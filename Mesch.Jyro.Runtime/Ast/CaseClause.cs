namespace Mesch.Jyro;

/// <summary>
/// Represents a single case clause within a switch statement in the abstract syntax tree.
/// Each case clause contains a match expression that is compared against the switch expression
/// and a body of statements to execute when the match succeeds.
/// </summary>
/// <remarks>
/// Case clauses are evaluated in the order they appear in the switch statement.
/// When the switch expression equals the case's match expression, the case body is executed
/// and control continues to subsequent cases unless a break statement is encountered.
/// This enables fall-through behavior similar to other programming languages.
/// </remarks>
public sealed class CaseClause
{
    /// <summary>
    /// Initializes a new instance of the <see cref="CaseClause"/> class with the specified
    /// match expression, statement body, and source location information.
    /// </summary>
    /// <param name="matchExpression">
    /// The expression that will be compared against the switch expression to determine
    /// if this case should be executed.
    /// </param>
    /// <param name="body">
    /// The collection of statements to execute when this case matches the switch expression.
    /// </param>
    /// <param name="lineNumber">The line number in the source code where this case clause appears.</param>
    /// <param name="columnPosition">The column position in the source code where this case clause begins.</param>
    /// <exception cref="ArgumentNullException">Thrown when matchExpression or body is null.</exception>
    public CaseClause(IExpression matchExpression, IReadOnlyList<IJyroStatement> body, int lineNumber, int columnPosition)
    {
        MatchExpression = matchExpression ?? throw new ArgumentNullException(nameof(matchExpression));
        Body = body ?? throw new ArgumentNullException(nameof(body));
        LineNumber = lineNumber;
        ColumnPosition = columnPosition;
    }

    /// <summary>
    /// Gets the expression that will be compared against the switch expression.
    /// When this expression evaluates to a value equal to the switch expression,
    /// the case body will be executed.
    /// </summary>
    public IExpression MatchExpression { get; }

    /// <summary>
    /// Gets the collection of statements that comprise the body of this case clause.
    /// These statements are executed when the match expression equals the switch expression.
    /// </summary>
    public IReadOnlyList<IJyroStatement> Body { get; }

    /// <summary>
    /// Gets the line number in the source code where this case clause is defined.
    /// This information is used for error reporting and debugging purposes.
    /// </summary>
    public int LineNumber { get; }

    /// <summary>
    /// Gets the column position in the source code where this case clause is defined.
    /// This information is used for error reporting and debugging purposes.
    /// </summary>
    public int ColumnPosition { get; }
}