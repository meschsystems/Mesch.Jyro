namespace Mesch.Jyro;

/// <summary>
/// Represents an expression node in the Jyro abstract syntax tree.
/// Expressions are evaluable language constructs that produce values when executed,
/// forming the computational core of the Jyro language system.
/// </summary>
/// <remarks>
/// Expressions encompass all value-producing constructs in the Jyro language, including:
/// <list type="bullet">
/// <item><description>Literal values: numbers, strings, booleans, arrays, objects</description></item>
/// <item><description>Variable references: identifiers that resolve to stored values</description></item>
/// <item><description>Binary operations: arithmetic, comparison, and logical operations</description></item>
/// <item><description>Unary operations: negation and logical not operations</description></item>
/// <item><description>Property access: object member access using dot notation</description></item>
/// <item><description>Index access: array and object element access using bracket notation</description></item>
/// <item><description>Function calls: invocation of built-in or host-provided functions</description></item>
/// <item><description>Type checking: runtime type verification using the 'is' operator</description></item>
/// </list>
/// 
/// All expressions implement the visitor pattern to enable various operations such as
/// interpretation, code generation, optimization, and static analysis without modifying
/// the expression classes themselves.
/// </remarks>
public interface IExpression
{
    /// <summary>
    /// Gets the one-based line number in the source code where this expression begins.
    /// This location information is essential for providing accurate error messages
    /// and debugging information during compilation and execution.
    /// </summary>
    int LineNumber { get; }

    /// <summary>
    /// Gets the one-based column position in the source code where this expression begins.
    /// Combined with the line number, this provides precise source location information
    /// for development tools and error reporting systems.
    /// </summary>
    int ColumnPosition { get; }

    /// <summary>
    /// Accepts a visitor for abstract syntax tree traversal without return values.
    /// This method enables various operations to be performed on expressions such as
    /// validation, transformation, or side-effect operations like code generation.
    /// </summary>
    /// <param name="visitor">
    /// The visitor instance that will process this expression node.
    /// The visitor's implementation determines the specific operation performed.
    /// </param>
    void Accept(IVisitor visitor);

    /// <summary>
    /// Accepts a visitor for abstract syntax tree traversal with return values.
    /// This method enables operations that compute and return results based on the
    /// expression, such as evaluation, compilation, or analysis that produces data.
    /// </summary>
    /// <typeparam name="T">
    /// The type of value returned by the visitor operation.
    /// This can be any type appropriate for the visitor's purpose, such as JyroValue
    /// for interpretation or string for code generation.
    /// </typeparam>
    /// <param name="visitor">
    /// The visitor instance that will process this expression node and return a result.
    /// </param>
    /// <returns>
    /// The result produced by the visitor's processing of this expression.
    /// The specific type and meaning depend on the visitor implementation.
    /// </returns>
    T Accept<T>(IVisitor<T> visitor);
}