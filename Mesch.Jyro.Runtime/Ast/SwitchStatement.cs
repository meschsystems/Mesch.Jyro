namespace Mesch.Jyro;

/// <summary>
/// Represents a switch statement in the abstract syntax tree with case branches and an optional default clause.
/// Switch statements provide multi-way branching by comparing a switch expression against multiple case values
/// and executing the corresponding statement block when a match is found.
/// </summary>
/// <remarks>
/// Switch statements evaluate the switch expression once and then compare it against each case clause
/// in order. When a matching case is found, its statement block is executed. If no cases match and
/// a default clause is present, the default statements are executed. Switch statements support
/// fall-through behavior where execution continues to subsequent cases unless a break statement is encountered.
/// 
/// <para>
/// The comparison between the switch expression and case expressions uses equality semantics,
/// similar to the == operator in expressions.
/// </para>
/// </remarks>
public sealed class SwitchStatement : IJyroStatement
{
    /// <summary>
    /// Initializes a new instance of the <see cref="SwitchStatement"/> class with the specified
    /// switch expression, case clauses, default branch, and source location information.
    /// </summary>
    /// <param name="expression">
    /// The expression that will be evaluated and compared against each case clause.
    /// This expression is evaluated once at the beginning of the switch statement.
    /// </param>
    /// <param name="cases">
    /// The collection of case clauses that define the conditional branches.
    /// Each case contains a match expression and a statement body to execute when matched.
    /// </param>
    /// <param name="defaultBranch">
    /// The collection of statements to execute when no case clauses match the switch expression.
    /// This collection may be empty if no default clause is provided.
    /// </param>
    /// <param name="lineNumber">The line number in the source code where this switch statement appears.</param>
    /// <param name="columnPosition">The column position in the source code where this switch statement begins.</param>
    /// <exception cref="ArgumentNullException">Thrown when expression, cases, or defaultBranch is null.</exception>
    public SwitchStatement(
        IExpression expression,
        IReadOnlyList<CaseClause> cases,
        IReadOnlyList<IJyroStatement> defaultBranch,
        int lineNumber,
        int columnPosition)
    {
        Expression = expression ?? throw new ArgumentNullException(nameof(expression));
        Cases = cases ?? throw new ArgumentNullException(nameof(cases));
        DefaultBranch = defaultBranch ?? throw new ArgumentNullException(nameof(defaultBranch));
        LineNumber = lineNumber;
        ColumnPosition = columnPosition;
    }

    /// <summary>
    /// Gets the switch expression that will be evaluated and compared against case clauses.
    /// This expression is evaluated once at the beginning of switch statement execution
    /// and its result is used for all case comparisons.
    /// </summary>
    public IExpression Expression { get; }

    /// <summary>
    /// Gets the collection of case clauses that define the conditional branches of the switch statement.
    /// Each case clause contains a match expression and a statement body that will be executed
    /// when the case matches the switch expression.
    /// </summary>
    public IReadOnlyList<CaseClause> Cases { get; }

    /// <summary>
    /// Gets the collection of statements that comprise the default branch of the switch statement.
    /// These statements are executed when no case clauses match the switch expression.
    /// This collection may be empty if no default clause was provided in the source code.
    /// </summary>
    public IReadOnlyList<IJyroStatement> DefaultBranch { get; }

    /// <summary>
    /// Gets the line number in the source code where this switch statement appears.
    /// </summary>
    public int LineNumber { get; }

    /// <summary>
    /// Gets the column position in the source code where this switch statement begins.
    /// </summary>
    public int ColumnPosition { get; }

    /// <summary>
    /// Accepts a visitor for traversal using the Visitor pattern.
    /// This method enables various operations to be performed on the AST node
    /// without modifying the node structure itself.
    /// </summary>
    /// <param name="visitor">The visitor instance that will process this switch statement.</param>
    public void Accept(IVisitor visitor) => visitor.VisitSwitchStatement(this);

    /// <summary>
    /// Accepts a visitor for traversal using the Visitor pattern with a return value.
    /// This method enables various operations to be performed on the AST node
    /// that produce a result of the specified type.
    /// </summary>
    /// <typeparam name="T">The type of result produced by the visitor.</typeparam>
    /// <param name="visitor">The visitor instance that will process this switch statement.</param>
    /// <returns>The result produced by the visitor's processing of this switch statement.</returns>
    public T Accept<T>(IVisitor<T> visitor) => visitor.VisitSwitchStatement(this);
}