namespace Mesch.Jyro;

/// <summary>
/// Represents a statement node in the Jyro abstract syntax tree.
/// Statements are executable language constructs that perform actions and control program flow
/// but do not produce values that can be used in expressions.
/// </summary>
/// <remarks>
/// Statements form the structural backbone of Jyro programs and encompass all imperative
/// constructs that modify program state or control execution flow:
/// <list type="bullet">
/// <item><description>Variable declarations: introducing new variables into scope with optional initialization</description></item>
/// <item><description>Assignment statements: modifying the values of existing variables or object properties</description></item>
/// <item><description>Expression statements: evaluating expressions for their side effects</description></item>
/// <item><description>Control flow statements: if-then-else constructs for conditional execution</description></item>
/// <item><description>Loop statements: while and foreach constructs for iterative execution</description></item>
/// <item><description>Switch statements: multi-way branching based on expression values</description></item>
/// <item><description>Jump statements: return, break, and continue for altering execution flow</description></item>
/// </list>
/// 
/// Statements implement the visitor pattern to enable various operations such as execution,
/// code generation, static analysis, and transformation without modifying the statement
/// classes themselves. This design provides flexibility for different execution strategies
/// and tooling requirements.
/// </remarks>
public interface IJyroStatement
{
    /// <summary>
    /// Gets the one-based line number in the source code where this statement begins.
    /// This location information is crucial for providing accurate error messages,
    /// debugging information, and development tool integration.
    /// </summary>
    int LineNumber { get; }

    /// <summary>
    /// Gets the one-based column position in the source code where this statement begins.
    /// Combined with the line number, this provides precise source location information
    /// for error reporting and debugging tools.
    /// </summary>
    int ColumnPosition { get; }

    /// <summary>
    /// Accepts a visitor for abstract syntax tree traversal without return values.
    /// This method enables various operations to be performed on statements such as
    /// execution, validation, or transformation operations that work through side effects.
    /// </summary>
    /// <param name="visitor">
    /// The visitor instance that will process this statement node.
    /// The visitor's implementation determines the specific operation performed.
    /// </param>
    void Accept(IVisitor visitor);

    /// <summary>
    /// Accepts a visitor for abstract syntax tree traversal with return values.
    /// This method enables operations that compute and return results based on the
    /// statement, such as code generation, analysis that produces data, or execution
    /// strategies that need to return control flow information.
    /// </summary>
    /// <typeparam name="T">
    /// The type of value returned by the visitor operation.
    /// This can be any type appropriate for the visitor's purpose, such as execution
    /// results, generated code strings, or analysis metrics.
    /// </typeparam>
    /// <param name="visitor">
    /// The visitor instance that will process this statement node and return a result.
    /// </param>
    /// <returns>
    /// The result produced by the visitor's processing of this statement.
    /// The specific type and meaning depend on the visitor implementation and its purpose.
    /// </returns>
    T Accept<T>(IVisitor<T> visitor);
}