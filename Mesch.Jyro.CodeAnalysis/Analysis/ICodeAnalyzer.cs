namespace Mesch.Jyro.CodeAnalysis;

/// <summary>
/// Defines the contract for code analyzers that perform static analysis on validated Jyro abstract syntax trees.
/// Code analyzers examine AST structures to gather metrics, identify patterns, and provide insights
/// without validating correctness (which is handled by the validator).
/// </summary>
public interface ICodeAnalyzer
{
    /// <summary>
    /// Analyzes the provided validated abstract syntax tree and returns comprehensive analysis results.
    /// </summary>
    /// <param name="statements">The collection of validated top-level statements to analyze.</param>
    /// <param name="options">Configuration options that control the analysis behavior.</param>
    /// <returns>
    /// A <see cref="CodeAnalysisResult"/> containing all metrics, patterns, and insights
    /// discovered during the static analysis process.
    /// </returns>
    CodeAnalysisResult Analyze(IReadOnlyList<IJyroStatement> statements, CodeAnalysisOptions options);
}