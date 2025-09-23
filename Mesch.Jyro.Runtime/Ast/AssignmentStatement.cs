namespace Mesch.Jyro;

/// <summary>
/// Represents an assignment statement in the abstract syntax tree.
/// Assignment statements assign values to variables, object properties, or array elements
/// using the assignment operator (=). The target can be a simple identifier, property access,
/// or index access expression.
/// </summary>
/// <remarks>
/// Assignment statements support various target types:
/// <list type="bullet">
/// <item><description>Variable assignment: x = 5</description></item>
/// <item><description>Property assignment: obj.property = "value"</description></item>
/// <item><description>Index assignment: array[0] = 42</description></item>
/// <item><description>Nested access: obj.array[1].property = true</description></item>
/// </list>
/// The value expression is evaluated first, then assigned to the resolved target location.
/// </remarks>
public sealed class AssignmentStatement : IJyroStatement
{
    /// <summary>
    /// Initializes a new instance of the <see cref="AssignmentStatement"/> class with the specified
    /// target expression, value expression, and source location information.
    /// </summary>
    /// <param name="target">
    /// The left-hand side expression that specifies where to assign the value.
    /// This can be a variable identifier, property access, or index access expression.
    /// </param>
    /// <param name="value">
    /// The right-hand side expression that provides the value to be assigned.
    /// This expression is evaluated before the assignment takes place.
    /// </param>
    /// <param name="lineNumber">The line number in the source code where this assignment statement appears.</param>
    /// <param name="columnPosition">The column position in the source code where this assignment statement begins.</param>
    /// <exception cref="ArgumentNullException">Thrown when target or value expression is null.</exception>
    public AssignmentStatement(IExpression target, IExpression value, int lineNumber, int columnPosition)
    {
        Target = target ?? throw new ArgumentNullException(nameof(target));
        Value = value ?? throw new ArgumentNullException(nameof(value));
        LineNumber = lineNumber;
        ColumnPosition = columnPosition;
    }

    /// <summary>
    /// Gets the target expression that specifies the assignment destination.
    /// This represents the left-hand side of the assignment and can be a variable,
    /// property access, or index access that will receive the assigned value.
    /// </summary>
    public IExpression Target { get; }

    /// <summary>
    /// Gets the value expression that provides the data to be assigned.
    /// This represents the right-hand side of the assignment and is evaluated
    /// to produce the value that will be stored in the target location.
    /// </summary>
    public IExpression Value { get; }

    /// <summary>
    /// Gets the line number in the source code where this assignment statement appears.
    /// </summary>
    public int LineNumber { get; }

    /// <summary>
    /// Gets the column position in the source code where this assignment statement begins.
    /// </summary>
    public int ColumnPosition { get; }

    /// <summary>
    /// Accepts a visitor for traversal using the Visitor pattern.
    /// This method enables various operations to be performed on the AST node
    /// without modifying the node structure itself.
    /// </summary>
    /// <param name="visitor">The visitor instance that will process this assignment statement.</param>
    public void Accept(IVisitor visitor) => visitor.VisitAssignmentStatement(this);

    /// <summary>
    /// Accepts a visitor for traversal using the Visitor pattern with a return value.
    /// This method enables various operations to be performed on the AST node
    /// that produce a result of the specified type.
    /// </summary>
    /// <typeparam name="T">The type of result produced by the visitor.</typeparam>
    /// <param name="visitor">The visitor instance that will process this assignment statement.</param>
    /// <returns>The result produced by the visitor's processing of this assignment statement.</returns>
    public T Accept<T>(IVisitor<T> visitor) => visitor.VisitAssignmentStatement(this);
}