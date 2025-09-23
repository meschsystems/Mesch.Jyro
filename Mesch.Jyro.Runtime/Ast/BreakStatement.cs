namespace Mesch.Jyro;

/// <summary>
/// Represents a break statement in the abstract syntax tree.
/// Break statements are used to immediately exit from loop constructs such as while loops
/// and foreach loops, transferring control to the statement immediately following the loop.
/// </summary>
/// <remarks>
/// Break statements can only appear within loop contexts. Using a break statement outside
/// of a loop will result in a compilation error during the validation phase. When executed,
/// the break statement causes the innermost enclosing loop to terminate immediately.
/// </remarks>
public sealed class BreakStatement : IJyroStatement
{
    /// <summary>
    /// Initializes a new instance of the <see cref="BreakStatement"/> class with the specified
    /// source location information.
    /// </summary>
    /// <param name="lineNumber">The line number in the source code where this break statement appears.</param>
    /// <param name="columnPosition">The column position in the source code where this break statement begins.</param>
    public BreakStatement(int lineNumber, int columnPosition)
    {
        LineNumber = lineNumber;
        ColumnPosition = columnPosition;
    }

    /// <summary>
    /// Gets the line number in the source code where this break statement appears.
    /// </summary>
    public int LineNumber { get; }

    /// <summary>
    /// Gets the column position in the source code where this break statement begins.
    /// </summary>
    public int ColumnPosition { get; }

    /// <summary>
    /// Accepts a visitor for traversal using the Visitor pattern.
    /// This method enables various operations to be performed on the AST node
    /// without modifying the node structure itself.
    /// </summary>
    /// <param name="visitor">The visitor instance that will process this break statement.</param>
    public void Accept(IVisitor visitor) => visitor.VisitBreakStatement(this);

    /// <summary>
    /// Accepts a visitor for traversal using the Visitor pattern with a return value.
    /// This method enables various operations to be performed on the AST node
    /// that produce a result of the specified type.
    /// </summary>
    /// <typeparam name="T">The type of result produced by the visitor.</typeparam>
    /// <param name="visitor">The visitor instance that will process this break statement.</param>
    /// <returns>The result produced by the visitor's processing of this break statement.</returns>
    public T Accept<T>(IVisitor<T> visitor) => visitor.VisitBreakStatement(this);
}