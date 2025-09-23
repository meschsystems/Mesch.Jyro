namespace Mesch.Jyro.CodeAnalysis;

/// <summary>
/// Contains quantitative metrics about code structure and complexity
/// calculated during static analysis.
/// </summary>
public sealed class CodeMetrics
{
    /// <summary>
    /// Initializes metrics with the specified values.
    /// </summary>
    public CodeMetrics(
        int totalStatements,
        int totalExpressions,
        int variableDeclarations,
        int assignmentStatements,
        int controlFlowStatements,
        int functionCalls,
        int maxNestingDepth,
        int averageNestingDepth,
        int cyclomaticComplexity,
        int cognitiveComplexity,
        int totalBranches,
        int uniqueVariableNames,
        int longestStatementChain)
    {
        TotalStatements = totalStatements;
        TotalExpressions = totalExpressions;
        VariableDeclarations = variableDeclarations;
        AssignmentStatements = assignmentStatements;
        ControlFlowStatements = controlFlowStatements;
        FunctionCalls = functionCalls;
        MaxNestingDepth = maxNestingDepth;
        AverageNestingDepth = averageNestingDepth;
        CyclomaticComplexity = cyclomaticComplexity;
        CognitiveComplexity = cognitiveComplexity;
        TotalBranches = totalBranches;
        UniqueVariableNames = uniqueVariableNames;
        LongestStatementChain = longestStatementChain;
    }

    /// <summary>
    /// Gets the total number of statements in the analyzed code.
    /// </summary>
    public int TotalStatements { get; }

    /// <summary>
    /// Gets the total number of expressions in the analyzed code.
    /// </summary>
    public int TotalExpressions { get; }

    /// <summary>
    /// Gets the number of variable declarations.
    /// </summary>
    public int VariableDeclarations { get; }

    /// <summary>
    /// Gets the number of assignment statements.
    /// </summary>
    public int AssignmentStatements { get; }

    /// <summary>
    /// Gets the number of control flow statements (if, while, switch, etc.).
    /// </summary>
    public int ControlFlowStatements { get; }

    /// <summary>
    /// Gets the number of function call expressions.
    /// </summary>
    public int FunctionCalls { get; }

    /// <summary>
    /// Gets the maximum nesting depth found in the code.
    /// </summary>
    public int MaxNestingDepth { get; }

    /// <summary>
    /// Gets the average nesting depth across all code blocks.
    /// </summary>
    public int AverageNestingDepth { get; }

    /// <summary>
    /// Gets the cyclomatic complexity of the code.
    /// </summary>
    public int CyclomaticComplexity { get; }

    /// <summary>
    /// Gets the cognitive complexity of the code (weighted by nesting).
    /// </summary>
    public int CognitiveComplexity { get; }

    /// <summary>
    /// Gets the total number of decision branches in the code.
    /// </summary>
    public int TotalBranches { get; }

    /// <summary>
    /// Gets the ratio of code to comments (placeholder for when comments are added).
    /// </summary>
    public double CodeToCommentRatio { get; }

    /// <summary>
    /// Gets the number of unique variable names used.
    /// </summary>
    public int UniqueVariableNames { get; }

    /// <summary>
    /// Gets the length of the longest sequential statement chain.
    /// </summary>
    public int LongestStatementChain { get; }

    /// <summary>
    /// Categorizes the overall complexity based on thresholds.
    /// </summary>
    public ComplexityLevel GetComplexityLevel(ComplexityThresholds thresholds)
    {
        if (CyclomaticComplexity <= thresholds.LowComplexityThreshold)
        {
            return ComplexityLevel.Low;
        }

        if (CyclomaticComplexity <= thresholds.ModerateComplexityThreshold)
        {
            return ComplexityLevel.Moderate;
        }

        if (CyclomaticComplexity <= thresholds.HighComplexityThreshold)
        {
            return ComplexityLevel.High;
        }

        return ComplexityLevel.VeryHigh;
    }
}