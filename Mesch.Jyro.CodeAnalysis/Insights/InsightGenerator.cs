namespace Mesch.Jyro.CodeAnalysis;

/// <summary>
/// Generates actionable insights based on collected code metrics and structural patterns.
/// This generator examines analysis data to provide recommendations for code improvement,
/// performance optimization, and maintainability enhancement.
/// </summary>
internal sealed class InsightGenerator
{
    private readonly AnalysisContext _analysisContext;
    private readonly CodeAnalysisOptions _analysisOptions;

    private const double HighComplexityImpactThreshold = 0.7;
    private const double VeryHighComplexityImpactThreshold = 0.9;
    private const double DeepNestingImpactThreshold = 0.6;
    private const double WriteOnlyVariableImpactThreshold = 0.5;
    private const double LowVariableReuseThreshold = 0.3;
    private const double LongStatementChainImpactThreshold = 0.4;
    private const double HighCallDensityThreshold = 0.5;
    private const double BranchHeavyCodeThreshold = 0.3;
    private const double LowMaintainabilityThreshold = 6.0;
    private const double HighComplexityRatioThreshold = 0.2;
    private const double HighComplexityRatioImpactThreshold = 0.6;
    private const double LowMaintainabilityImpactThreshold = 0.8;
    private const int MaxVariablesToDisplay = 3;
    private const int MaxFunctionsToDisplay = 3;
    private const int LongStatementChainThreshold = 10;
    private const int ShortVariableNameLength = 2;

    /// <summary>
    /// Initializes a new instance of the <see cref="InsightGenerator"/> class.
    /// </summary>
    /// <param name="analysisContext">The context containing collected analysis data.</param>
    /// <param name="analysisOptions">The options controlling insight generation behavior.</param>
    /// <exception cref="ArgumentNullException">Thrown when analysisContext or analysisOptions are null.</exception>
    public InsightGenerator(AnalysisContext analysisContext, CodeAnalysisOptions analysisOptions)
    {
        _analysisContext = analysisContext ?? throw new ArgumentNullException(nameof(analysisContext));
        _analysisOptions = analysisOptions ?? throw new ArgumentNullException(nameof(analysisOptions));
    }

    /// <summary>
    /// Generates actionable insights based on the collected metrics and patterns.
    /// </summary>
    /// <returns>A read-only collection of code insights with recommendations.</returns>
    public IReadOnlyList<CodeInsight> GenerateInsights()
    {
        var generatedInsights = new List<CodeInsight>();
        var codeMetrics = _analysisContext.GetMetrics();

        if (_analysisOptions.EnableComplexityMetrics)
        {
            generatedInsights.AddRange(GenerateComplexityInsights(codeMetrics));
        }

        if (_analysisOptions.EnablePerformanceInsights)
        {
            generatedInsights.AddRange(GeneratePerformanceInsights(codeMetrics));
        }

        if (_analysisOptions.EnableMaintainabilityMetrics)
        {
            generatedInsights.AddRange(GenerateMaintainabilityInsights(codeMetrics));
        }

        return generatedInsights;
    }

    /// <summary>
    /// Generates insights related to code complexity metrics.
    /// </summary>
    /// <param name="codeMetrics">The collected code metrics.</param>
    /// <returns>A collection of complexity-related insights.</returns>
    private IEnumerable<CodeInsight> GenerateComplexityInsights(CodeMetrics codeMetrics)
    {
        var complexityLevel = codeMetrics.GetComplexityLevel(_analysisOptions.ComplexityThresholds);

        yield return new CodeInsight(
            "Complexity",
            InsightType.Information,
            $"Code Complexity: {complexityLevel}",
            $"Cyclomatic complexity is {codeMetrics.CyclomaticComplexity}, cognitive complexity is {codeMetrics.CognitiveComplexity}",
            recommendation: complexityLevel switch
            {
                ComplexityLevel.High => "Consider breaking down complex logic into smaller, more focused functions",
                ComplexityLevel.VeryHigh => "High complexity detected - refactoring is strongly recommended to improve maintainability",
                _ => null
            },
            impact: complexityLevel switch
            {
                ComplexityLevel.High => HighComplexityImpactThreshold,
                ComplexityLevel.VeryHigh => VeryHighComplexityImpactThreshold,
                _ => null
            });

        if (codeMetrics.MaxNestingDepth > _analysisOptions.ComplexityThresholds.ModerateNestingThreshold)
        {
            yield return new CodeInsight(
                "Complexity",
                InsightType.Maintainability,
                "Deep Nesting Detected",
                $"Maximum nesting depth of {codeMetrics.MaxNestingDepth} may impact code readability and maintainability",
                recommendation: "Consider extracting nested logic into separate functions or using early returns to reduce nesting",
                impact: DeepNestingImpactThreshold);
        }
    }

    /// <summary>
    /// Generates insights related to performance characteristics.
    /// </summary>
    /// <param name="codeMetrics">The collected code metrics.</param>
    /// <returns>A collection of performance-related insights.</returns>
    private IEnumerable<CodeInsight> GeneratePerformanceInsights(CodeMetrics codeMetrics)
    {
        if (codeMetrics.TotalStatements > 0)
        {
            var functionCallDensity = (double)codeMetrics.FunctionCalls / codeMetrics.TotalStatements;
            if (functionCallDensity > HighCallDensityThreshold)
            {
                yield return new CodeInsight(
                    "Performance",
                    InsightType.Performance,
                    "High Function Call Density",
                    $"Function calls comprise {functionCallDensity:P0} of all statements",
                    recommendation: "High call density may impact performance - consider inlining simple operations or reducing abstraction layers");
            }
        }

        var branchToStatementRatio = codeMetrics.TotalStatements > 0
            ? (double)codeMetrics.TotalBranches / codeMetrics.TotalStatements
            : 0;

        if (branchToStatementRatio > BranchHeavyCodeThreshold)
        {
            yield return new CodeInsight(
                "Performance",
                InsightType.Performance,
                "Branch-Heavy Code Pattern",
                $"{codeMetrics.TotalBranches} branches in {codeMetrics.TotalStatements} statements ({branchToStatementRatio:P0})",
                recommendation: "Consider refactoring complex branching logic to improve predictability and performance");
        }
    }

    /// <summary>
    /// Generates insights related to code maintainability.
    /// </summary>
    /// <param name="codeMetrics">The collected code metrics.</param>
    /// <returns>A collection of maintainability-related insights.</returns>
    private IEnumerable<CodeInsight> GenerateMaintainabilityInsights(CodeMetrics codeMetrics)
    {
        var maintainabilityScore = CalculateMaintainabilityScore(codeMetrics);

        yield return new CodeInsight(
            "Maintainability",
            InsightType.Information,
            "Overall Maintainability Score",
            $"Maintainability score: {maintainabilityScore:F1}/10.0",
            recommendation: maintainabilityScore < LowMaintainabilityThreshold
                ? "Consider refactoring to improve overall code maintainability"
                : null,
            impact: maintainabilityScore < LowMaintainabilityThreshold
                ? LowMaintainabilityImpactThreshold
                : null);

        if (codeMetrics.TotalStatements > 0)
        {
            var complexityToStatementRatio = (double)codeMetrics.CyclomaticComplexity / codeMetrics.TotalStatements;
            if (complexityToStatementRatio > HighComplexityRatioThreshold)
            {
                yield return new CodeInsight(
                    "Maintainability",
                    InsightType.Maintainability,
                    "High Complexity-to-Statement Ratio",
                    $"Complexity to statement ratio: {complexityToStatementRatio:F2}",
                    recommendation: "High complexity ratio suggests opportunities for logic simplification and decomposition",
                    impact: HighComplexityRatioImpactThreshold);
            }
        }
    }

    /// <summary>
    /// Calculates an overall maintainability score based on various code metrics.
    /// </summary>
    /// <param name="codeMetrics">The code metrics to evaluate.</param>
    /// <returns>A maintainability score from 0 to 10, where 10 represents the most maintainable code.</returns>
    private static double CalculateMaintainabilityScore(CodeMetrics codeMetrics)
    {
        const double baseScore = 10.0;
        const double maxComplexityPenalty = 3.0;
        const double maxNestingPenalty = 2.0;
        const double maxSizePenalty = 1.0;
        const double complexityPenaltyDivisor = 20.0;
        const double nestingPenaltyDivisor = 10.0;
        const double sizePenaltyDivisor = 100.0;

        var complexityPenalty = Math.Min(codeMetrics.CyclomaticComplexity / complexityPenaltyDivisor, 1.0) * maxComplexityPenalty;
        var nestingPenalty = Math.Min(codeMetrics.MaxNestingDepth / nestingPenaltyDivisor, 1.0) * maxNestingPenalty;
        var sizePenalty = Math.Min(codeMetrics.TotalStatements / sizePenaltyDivisor, 1.0) * maxSizePenalty;

        var calculatedScore = baseScore - complexityPenalty - nestingPenalty - sizePenalty;
        return Math.Max(0, calculatedScore);
    }
}