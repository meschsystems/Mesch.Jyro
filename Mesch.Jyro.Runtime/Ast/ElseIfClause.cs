namespace Mesch.Jyro;

/// <summary>
/// Represents an else-if clause within a conditional if statement in the abstract syntax tree.
/// Else-if clauses provide additional conditional branches that are evaluated when the main
/// if condition and any previous else-if conditions evaluate to false.
/// </summary>
/// <remarks>
/// Else-if clauses are evaluated in the order they appear in the if statement.
/// When an else-if condition evaluates to true, its statement body is executed and
/// no subsequent else-if clauses or else clause are evaluated. This provides efficient
/// multi-way conditional branching similar to switch statements but with arbitrary expressions.
/// </remarks>
public sealed class ElseIfClause
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ElseIfClause"/> class with the specified
    /// condition expression, statement body, and source location information.
    /// </summary>
    /// <param name="condition">
    /// The boolean expression that determines whether this else-if branch should be executed.
    /// This condition is evaluated only if all previous conditions in the if statement were false.
    /// </param>
    /// <param name="statements">
    /// The collection of statements to execute when this else-if condition evaluates to true.
    /// These statements are executed in order and form the body of this conditional branch.
    /// </param>
    /// <param name="lineNumber">The line number in the source code where this else-if clause appears.</param>
    /// <param name="columnPosition">The column position in the source code where this else-if clause begins.</param>
    /// <exception cref="ArgumentNullException">Thrown when condition or statements parameter is null.</exception>
    public ElseIfClause(
        IExpression condition,
        IReadOnlyList<IJyroStatement> statements,
        int lineNumber,
        int columnPosition)
    {
        Condition = condition ?? throw new ArgumentNullException(nameof(condition));
        Statements = statements ?? throw new ArgumentNullException(nameof(statements));
        LineNumber = lineNumber;
        ColumnPosition = columnPosition;
    }

    /// <summary>
    /// Gets the condition expression that determines whether this else-if branch should be executed.
    /// This expression is evaluated using truthiness rules, where values are considered true
    /// or false based on their type-specific truthiness semantics.
    /// </summary>
    public IExpression Condition { get; }

    /// <summary>
    /// Gets the collection of statements that comprise the body of this else-if clause.
    /// These statements are executed in order when the condition evaluates to true,
    /// and no subsequent else-if or else clauses are evaluated.
    /// </summary>
    public IReadOnlyList<IJyroStatement> Statements { get; }

    /// <summary>
    /// Gets the line number in the source code where this else-if clause is defined.
    /// This information is used for error reporting and debugging purposes.
    /// </summary>
    public int LineNumber { get; }

    /// <summary>
    /// Gets the column position in the source code where this else-if clause is defined.
    /// This information is used for error reporting and debugging purposes.
    /// </summary>
    public int ColumnPosition { get; }
}