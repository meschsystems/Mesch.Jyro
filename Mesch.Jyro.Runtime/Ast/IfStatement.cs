namespace Mesch.Jyro;

/// <summary>
/// Represents a conditional if statement in the abstract syntax tree with support for else-if chains.
/// If statements evaluate conditions and execute different statement blocks based on the results,
/// providing multi-way conditional branching with proper block scoping semantics for all branches.
/// </summary>
/// <remarks>
/// If statements support the following conditional structures:
/// <list type="bullet">
/// <item><description>Simple if: if condition then statements end</description></item>
/// <item><description>If-else: if condition then statements else statements end</description></item>
/// <item><description>If-else-if chains: if condition then statements else if condition then statements else statements end</description></item>
/// </list>
/// Each branch maintains its own scope for variable declarations, and conditions are evaluated
/// using truthiness rules where different value types have specific true/false semantics.
/// </remarks>
public sealed class IfStatement : IJyroStatement
{
    /// <summary>
    /// Initializes a new instance of the <see cref="IfStatement"/> class with the specified
    /// condition, branches, and source location information.
    /// </summary>
    /// <param name="condition">
    /// The primary condition expression that determines which branch to execute.
    /// This expression is evaluated using truthiness rules.
    /// </param>
    /// <param name="thenBranch">
    /// The collection of statements to execute when the primary condition evaluates to true.
    /// </param>
    /// <param name="elseIfClauses">
    /// The collection of else-if clauses that provide additional conditional branches.
    /// These are evaluated in order if the primary condition is false.
    /// </param>
    /// <param name="elseBranch">
    /// The collection of statements to execute when all conditions evaluate to false.
    /// This collection may be empty if no else clause is provided.
    /// </param>
    /// <param name="lineNumber">The line number in the source code where this if statement appears.</param>
    /// <param name="columnPosition">The column position in the source code where this if statement begins.</param>
    /// <exception cref="ArgumentNullException">Thrown when any parameter is null.</exception>
    public IfStatement(
        IExpression condition,
        IReadOnlyList<IJyroStatement> thenBranch,
        IReadOnlyList<ElseIfClause> elseIfClauses,
        IReadOnlyList<IJyroStatement> elseBranch,
        int lineNumber,
        int columnPosition)
    {
        Condition = condition ?? throw new ArgumentNullException(nameof(condition));
        ThenBranch = thenBranch ?? throw new ArgumentNullException(nameof(thenBranch));
        ElseIfClauses = elseIfClauses ?? throw new ArgumentNullException(nameof(elseIfClauses));
        ElseBranch = elseBranch ?? throw new ArgumentNullException(nameof(elseBranch));
        LineNumber = lineNumber;
        ColumnPosition = columnPosition;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="IfStatement"/> class for simple if-else statements
    /// without else-if clauses. This constructor provides backward compatibility and convenience
    /// for the common case of binary conditional branching.
    /// </summary>
    /// <param name="condition">
    /// The condition expression that determines which branch to execute.
    /// </param>
    /// <param name="thenBranch">
    /// The collection of statements to execute when the condition evaluates to true.
    /// </param>
    /// <param name="elseBranch">
    /// The collection of statements to execute when the condition evaluates to false.
    /// This collection may be empty if no else clause is provided.
    /// </param>
    /// <param name="lineNumber">The line number in the source code where this if statement appears.</param>
    /// <param name="columnPosition">The column position in the source code where this if statement begins.</param>
    public IfStatement(
        IExpression condition,
        IReadOnlyList<IJyroStatement> thenBranch,
        IReadOnlyList<IJyroStatement> elseBranch,
        int lineNumber,
        int columnPosition)
        : this(condition, thenBranch, new List<ElseIfClause>(), elseBranch, lineNumber, columnPosition)
    {
    }

    /// <summary>
    /// Gets the primary condition expression that determines the initial branching decision.
    /// This condition is evaluated first, and if true, the then branch is executed.
    /// </summary>
    public IExpression Condition { get; }

    /// <summary>
    /// Gets the collection of statements that comprise the then branch.
    /// These statements are executed when the primary condition evaluates to true.
    /// </summary>
    public IReadOnlyList<IJyroStatement> ThenBranch { get; }

    /// <summary>
    /// Gets the collection of else-if clauses that provide additional conditional branches.
    /// These clauses are evaluated in order if the primary condition is false,
    /// enabling multi-way conditional branching.
    /// </summary>
    public IReadOnlyList<ElseIfClause> ElseIfClauses { get; }

    /// <summary>
    /// Gets the collection of statements that comprise the else branch.
    /// These statements are executed when all conditions (primary and else-if) evaluate to false.
    /// This collection may be empty if no else clause was provided in the source code.
    /// </summary>
    public IReadOnlyList<IJyroStatement> ElseBranch { get; }

    /// <summary>
    /// Gets the line number in the source code where this if statement appears.
    /// </summary>
    public int LineNumber { get; }

    /// <summary>
    /// Gets the column position in the source code where this if statement begins.
    /// </summary>
    public int ColumnPosition { get; }

    /// <summary>
    /// Accepts a visitor for traversal using the Visitor pattern.
    /// This method enables various operations to be performed on the AST node
    /// without modifying the node structure itself.
    /// </summary>
    /// <param name="visitor">The visitor instance that will process this if statement.</param>
    public void Accept(IVisitor visitor) => visitor.VisitIfStatement(this);

    /// <summary>
    /// Accepts a visitor for traversal using the Visitor pattern with a return value.
    /// This method enables various operations to be performed on the AST node
    /// that produce a result of the specified type.
    /// </summary>
    /// <typeparam name="T">The type of result produced by the visitor.</typeparam>
    /// <param name="visitor">The visitor instance that will process this if statement.</param>
    /// <returns>The result produced by the visitor's processing of this if statement.</returns>
    public T Accept<T>(IVisitor<T> visitor) => visitor.VisitIfStatement(this);
}