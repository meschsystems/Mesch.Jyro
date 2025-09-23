namespace Mesch.Jyro.CodeAnalysis;

/// <summary>
/// Execution result that includes optional code analysis results.
/// </summary>
public sealed class JyroExecutionResultWithAnalysis
{
    public JyroExecutionResultWithAnalysis(JyroExecutionResult executionResult, CodeAnalysisResult? analysisResult)
    {
        ExecutionResult = executionResult ?? throw new ArgumentNullException(nameof(executionResult));
        AnalysisResult = analysisResult;
    }

    /// <summary>
    /// Gets the execution result.
    /// </summary>
    public JyroExecutionResult ExecutionResult { get; }

    /// <summary>
    /// Gets the code analysis result, if analysis was performed.
    /// </summary>
    public CodeAnalysisResult? AnalysisResult { get; }

    /// <summary>
    /// Gets whether the execution was successful.
    /// </summary>
    public bool IsSuccessful => ExecutionResult.IsSuccessful;

    /// <summary>
    /// Gets the resulting data from execution.
    /// </summary>
    public JyroValue Data => ExecutionResult.Data;

    /// <summary>
    /// Gets the diagnostic messages from execution.
    /// </summary>
    public IReadOnlyList<IMessage> Messages => ExecutionResult.Messages;
}