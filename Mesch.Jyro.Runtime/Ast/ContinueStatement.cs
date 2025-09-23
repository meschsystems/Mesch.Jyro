namespace Mesch.Jyro;

/// <summary>
/// Represents a continue statement in the abstract syntax tree.
/// Continue statements are used to skip the remaining statements in the current iteration
/// of a loop and immediately proceed to the next iteration. This applies to both while loops
/// and foreach loops.
/// </summary>
/// <remarks>
/// Continue statements can only appear within loop contexts. Using a continue statement outside
/// of a loop will result in a compilation error during the validation phase. When executed,
/// the continue statement causes the current loop iteration to terminate immediately and
/// control to transfer to the loop's condition check or next iteration.
/// </remarks>
public sealed class ContinueStatement : IJyroStatement
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ContinueStatement"/> class with the specified
    /// source location information.
    /// </summary>
    /// <param name="lineNumber">The line number in the source code where this continue statement appears.</param>
    /// <param name="columnPosition">The column position in the source code where this continue statement begins.</param>
    public ContinueStatement(int lineNumber, int columnPosition)
    {
        LineNumber = lineNumber;
        ColumnPosition = columnPosition;
    }

    /// <summary>
    /// Gets the line number in the source code where this continue statement appears.
    /// </summary>
    public int LineNumber { get; }

    /// <summary>
    /// Gets the column position in the source code where this continue statement begins.
    /// </summary>
    public int ColumnPosition { get; }

    /// <summary>
    /// Accepts a visitor for traversal using the Visitor pattern.
    /// This method enables various operations to be performed on the AST node
    /// without modifying the node structure itself.
    /// </summary>
    /// <param name="visitor">The visitor instance that will process this continue statement.</param>
    public void Accept(IVisitor visitor) => visitor.VisitContinueStatement(this);

    /// <summary>
    /// Accepts a visitor for traversal using the Visitor pattern with a return value.
    /// This method enables various operations to be performed on the AST node
    /// that produce a result of the specified type.
    /// </summary>
    /// <typeparam name="T">The type of result produced by the visitor.</typeparam>
    /// <param name="visitor">The visitor instance that will process this continue statement.</param>
    /// <returns>The result produced by the visitor's processing of this continue statement.</returns>
    public T Accept<T>(IVisitor<T> visitor) => visitor.VisitContinueStatement(this);
}