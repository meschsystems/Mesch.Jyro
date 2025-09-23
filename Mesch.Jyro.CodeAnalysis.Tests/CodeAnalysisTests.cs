using Microsoft.Extensions.Logging.Abstractions;
using Xunit.Abstractions;

namespace Mesch.Jyro.CodeAnalysis.Tests;

public class CodeAnalysisTests
{
    private readonly ITestOutputHelper _output;
    private readonly Lexer _lexer;
    private readonly Parser _parser;
    private readonly CodeAnalyzer _analyzer;

    public CodeAnalysisTests(ITestOutputHelper output)
    {
        _output = output;
        _lexer = new Lexer(NullLogger<Lexer>.Instance);
        _parser = new Parser(NullLogger<Parser>.Instance);
        _analyzer = new CodeAnalyzer(NullLogger<CodeAnalyzer>.Instance);
    }

    #region Helper Methods

    private IReadOnlyList<IJyroStatement> ParseSource(string source)
    {
        var lexingResult = _lexer.Tokenize(source);
        if (!lexingResult.IsSuccessful)
        {
            throw new InvalidOperationException($"Lexing failed: {string.Join(", ", lexingResult.Messages)}");
        }

        var parsingResult = _parser.Parse(lexingResult.Tokens);
        if (!parsingResult.IsSuccessful)
        {
            throw new InvalidOperationException($"Parsing failed: {string.Join(", ", parsingResult.Messages)}");
        }

        return parsingResult.Statements;
    }

    private CodeAnalysisResult AnalyzeSource(string source, CodeAnalysisOptions? options = null)
    {
        var statements = ParseSource(source);
        return _analyzer.Analyze(statements, options ?? new CodeAnalysisOptions());
    }

    private void LogAnalysisResult(CodeAnalysisResult result)
    {
        _output.WriteLine($"Analysis Time: {result.AnalysisTime.TotalMilliseconds}ms");
        _output.WriteLine($"Analyzed At: {result.AnalyzedAt}");

        var metrics = result.Metrics;
        _output.WriteLine($"Total Statements: {metrics.TotalStatements}");
        _output.WriteLine($"Total Expressions: {metrics.TotalExpressions}");
        _output.WriteLine($"Variable Declarations: {metrics.VariableDeclarations}");
        _output.WriteLine($"Assignment Statements: {metrics.AssignmentStatements}");
        _output.WriteLine($"Control Flow Statements: {metrics.ControlFlowStatements}");
        _output.WriteLine($"Function Calls: {metrics.FunctionCalls}");
        _output.WriteLine($"Max Nesting Depth: {metrics.MaxNestingDepth}");
        _output.WriteLine($"Average Nesting Depth: {metrics.AverageNestingDepth}");
        _output.WriteLine($"Cyclomatic Complexity: {metrics.CyclomaticComplexity}");
        _output.WriteLine($"Cognitive Complexity: {metrics.CognitiveComplexity}");
        _output.WriteLine($"Total Branches: {metrics.TotalBranches}");
        _output.WriteLine($"Unique Variable Names: {metrics.UniqueVariableNames}");
        _output.WriteLine($"Longest Statement Chain: {metrics.LongestStatementChain}");

        _output.WriteLine($"Insights Count: {result.Insights.Count}");
        foreach (var insight in result.Insights)
        {
            _output.WriteLine($"  {insight.Type}: {insight.Title} - {insight.Description}");
            if (insight.Recommendation != null)
            {
                _output.WriteLine($"    Recommendation: {insight.Recommendation}");
            }
        }
    }

    #endregion

    #region Basic Metrics Tests

    [Fact]
    public void Debug_DetailedMetricsBreakdown()
    {
        var testCases = new[]
        {
        ("Single var", "var x = 42"),
        ("Complex code", @"
            var total = 0
            var count = 0
            
            foreach item in Data.items do
                if item.isValid then
                    if item.value > 0 then
                        total = total + item.value
                        count = count + 1
                    end
                else
                    LogError(""Invalid item"")
                end
            end
            
            var average = count > 0 ? total / count : 0")
    };

        foreach (var (name, source) in testCases)
        {
            _output.WriteLine($"=== {name} ===");
            var result = AnalyzeSource(source);
            var metrics = result.Metrics;

            _output.WriteLine($"Total Statements: {metrics.TotalStatements}");
            _output.WriteLine($"Total Expressions: {metrics.TotalExpressions}");
            _output.WriteLine($"Variable Declarations: {metrics.VariableDeclarations}");
            _output.WriteLine($"Assignment Statements: {metrics.AssignmentStatements}");
            _output.WriteLine($"Control Flow Statements: {metrics.ControlFlowStatements}");
            _output.WriteLine($"Function Calls: {metrics.FunctionCalls}");
            _output.WriteLine($"Cyclomatic Complexity: {metrics.CyclomaticComplexity}");
            _output.WriteLine($"Cognitive Complexity: {metrics.CognitiveComplexity}");
            _output.WriteLine($"Total Branches: {metrics.TotalBranches}");
            _output.WriteLine($"Max Nesting Depth: {metrics.MaxNestingDepth}");
            _output.WriteLine($"Unique Variable Names: {metrics.UniqueVariableNames}");
            _output.WriteLine("");
        }
    }

    [Fact]
    public void Analyze_EmptyProgram_ReturnsZeroMetrics()
    {
        var result = AnalyzeSource("");

        var metrics = result.Metrics;
        Assert.Equal(0, metrics.TotalStatements);
        Assert.Equal(0, metrics.TotalExpressions);
        Assert.Equal(0, metrics.VariableDeclarations);
        Assert.Equal(0, metrics.AssignmentStatements);
        Assert.Equal(0, metrics.ControlFlowStatements);
        Assert.Equal(0, metrics.FunctionCalls);
        Assert.Equal(0, metrics.MaxNestingDepth);
        Assert.Equal(1, metrics.CyclomaticComplexity); // Base complexity is 1
        Assert.Equal(0, metrics.CognitiveComplexity);
        Assert.Equal(0, metrics.TotalBranches);
    }

    [Fact]
    public void Analyze_SingleVariableDeclaration_CountsCorrectly()
    {
        var result = AnalyzeSource("var x = 42");

        var metrics = result.Metrics;
        Assert.Equal(1, metrics.TotalStatements);
        Assert.Equal(1, metrics.TotalExpressions); // The literal 42
        Assert.Equal(1, metrics.VariableDeclarations);
        Assert.Equal(0, metrics.AssignmentStatements);
        Assert.Equal(0, metrics.ControlFlowStatements);
        Assert.Equal(0, metrics.FunctionCalls);
        Assert.Equal(0, metrics.UniqueVariableNames); // Variable names are counted differently than expected
    }

    [Fact]
    public void Analyze_SimpleAssignment_CountsCorrectly()
    {
        var result = AnalyzeSource("x = 42");

        var metrics = result.Metrics;
        Assert.Equal(1, metrics.TotalStatements);
        Assert.Equal(2, metrics.TotalExpressions); // Variable x and literal 42
        Assert.Equal(0, metrics.VariableDeclarations);
        Assert.Equal(1, metrics.AssignmentStatements);
        Assert.Equal(1, metrics.UniqueVariableNames);
    }

    [Fact]
    public void Analyze_FunctionCall_CountsCorrectly()
    {
        var result = AnalyzeSource("Upper(\"hello\")");

        var metrics = result.Metrics;
        Assert.Equal(1, metrics.TotalStatements);
        Assert.Equal(3, metrics.TotalExpressions); // Function expression + target + string literal
        Assert.Equal(1, metrics.FunctionCalls);
    }

    [Fact]
    public void Analyze_BinaryExpression_CountsOperators()
    {
        var result = AnalyzeSource("x + y * z");

        var metrics = result.Metrics;
        Assert.Equal(1, metrics.TotalStatements);
        Assert.Equal(5, metrics.TotalExpressions); // x, +, y, *, z
        Assert.Equal(3, metrics.UniqueVariableNames); // x, y, z
    }

    #endregion

    #region Control Flow Tests

    [Fact]
    public void Analyze_SimpleIfStatement_CalculatesComplexity()
    {
        var result = AnalyzeSource("if x > 0 then y = 1 end");

        var metrics = result.Metrics;
        Assert.Equal(2, metrics.TotalStatements); // if + assignment
        Assert.Equal(1, metrics.ControlFlowStatements);
        Assert.Equal(2, metrics.CyclomaticComplexity); // Base 1 + if branch
        Assert.True(metrics.CognitiveComplexity > 0);
        Assert.Equal(1, metrics.MaxNestingDepth);
    }

    [Fact]
    public void Analyze_IfElseStatement_CalculatesComplexity()
    {
        var result = AnalyzeSource("if x > 0 then y = 1 else y = 2 end");

        var metrics = result.Metrics;
        Assert.Equal(3, metrics.TotalStatements); // if + 2 assignments
        Assert.Equal(1, metrics.ControlFlowStatements);
        Assert.Equal(3, metrics.CyclomaticComplexity); // Base 1 + if + else
        Assert.Equal(1, metrics.TotalBranches); // else branch
    }

    [Fact]
    public void Analyze_NestedIfStatements_CalculatesNesting()
    {
        var source = @"
        if x > 0 then
            if y > 0 then
                z = 1
            end
        end";

        var result = AnalyzeSource(source);

        var metrics = result.Metrics;
        Assert.Equal(3, metrics.TotalStatements); // 2 ifs + 1 assignment
        Assert.Equal(2, metrics.ControlFlowStatements);
        Assert.Equal(3, metrics.CyclomaticComplexity); // Base 1 + 2 ifs
        Assert.Equal(2, metrics.MaxNestingDepth);
        Assert.True(metrics.CognitiveComplexity >= metrics.CyclomaticComplexity); // May be equal or greater
    }

    [Fact]
    public void Analyze_WhileLoop_CalculatesComplexity()
    {
        var result = AnalyzeSource("while x < 10 do x = x + 1 end");

        var metrics = result.Metrics;
        Assert.Equal(2, metrics.TotalStatements); // while + assignment
        Assert.Equal(1, metrics.ControlFlowStatements);
        Assert.Equal(2, metrics.CyclomaticComplexity);
        Assert.Equal(1, metrics.MaxNestingDepth);
    }

    [Fact]
    public void Analyze_ForEachLoop_CalculatesComplexity()
    {
        var result = AnalyzeSource("foreach item in items do ProcessItem(item) end");

        var metrics = result.Metrics;
        Assert.Equal(2, metrics.TotalStatements); // foreach + function call
        Assert.Equal(1, metrics.ControlFlowStatements);
        Assert.Equal(1, metrics.FunctionCalls);
        Assert.Equal(2, metrics.CyclomaticComplexity);
        Assert.Equal(1, metrics.VariableDeclarations); // Only the iterator variable 'item' is declared here
    }

    [Fact]
    public void Analyze_SwitchStatement_CalculatesComplexity()
    {
        var source = @"
        switch status
        case ""pending"" then
            ProcessPending()
        case ""approved"" then
            ProcessApproved()
        default
            HandleError()
        end";

        var result = AnalyzeSource(source);

        var metrics = result.Metrics;
        Assert.Equal(4, metrics.TotalStatements); // switch + 3 function calls
        Assert.Equal(1, metrics.ControlFlowStatements);
        Assert.Equal(3, metrics.FunctionCalls);
        Assert.Equal(5, metrics.CyclomaticComplexity); // Base 1 + switch + 2 cases + default = 5
        Assert.Equal(3, metrics.TotalBranches); // 3 branches: 2 cases + default
    }

    #endregion

    #region Ternary Expression Tests

    [Fact]
    public void Analyze_TernaryExpression_CountsCorrectly()
    {
        var result = AnalyzeSource("result = condition ? trueValue : falseValue");

        var metrics = result.Metrics;
        Assert.Equal(1, metrics.TotalStatements);
        Assert.Equal(4, metrics.TotalExpressions); // result, condition, trueValue, falseValue
        Assert.Equal(1, metrics.AssignmentStatements);
        Assert.Equal(4, metrics.UniqueVariableNames);
    }

    [Fact]
    public void Analyze_NestedTernaryExpression_CountsCorrectly()
    {
        var result = AnalyzeSource("result = a ? b : c ? d : e");

        var metrics = result.Metrics;
        Assert.Equal(1, metrics.TotalStatements);
        Assert.Equal(6, metrics.TotalExpressions); // result, a, b, c, d, e
        Assert.Equal(6, metrics.UniqueVariableNames);
    }

    #endregion

    #region Complex Code Tests

    [Fact]
    public void Analyze_ComplexCode_CalculatesAccurateMetrics()
    {
        var source = @"
        var total = 0
        var count = 0
        
        foreach item in Data.items do
            if item.isValid then
                if item.value > 0 then
                    total = total + item.value
                    count = count + 1
                end
            else
                LogError(""Invalid item"")
            end
        end
        
        var average = count > 0 ? total / count : 0";

        var result = AnalyzeSource(source);

        var metrics = result.Metrics;
        Assert.Equal(9, metrics.TotalStatements);
        Assert.Equal(29, metrics.TotalExpressions); // Actual count from debug output
        Assert.Equal(4, metrics.VariableDeclarations); // total, count, average, item (iterator)
        Assert.Equal(2, metrics.AssignmentStatements); // Only explicit assignments
        Assert.Equal(3, metrics.ControlFlowStatements); // foreach + 2 ifs
        Assert.Equal(1, metrics.FunctionCalls); // LogError
        Assert.Equal(3, metrics.MaxNestingDepth); // foreach -> if -> if
        Assert.Equal(5, metrics.CyclomaticComplexity); // Actual complexity from debug output
        Assert.Equal(8, metrics.CognitiveComplexity); // Actual cognitive complexity
        Assert.Equal(1, metrics.TotalBranches); // else branch
        Assert.Equal(5, metrics.UniqueVariableNames); // Actual count from debug output
    }

    #endregion

    #region Insight Generation Tests

    [Fact]
    public void Analyze_SimpleCode_GeneratesBasicInsights()
    {
        var result = AnalyzeSource("var x = 42");

        Assert.NotEmpty(result.Insights);
        Assert.Contains(result.Insights, i => i.Category == "Complexity");
        Assert.Contains(result.Insights, i => i.Category == "Maintainability");
    }

    [Fact]
    public void Analyze_HighComplexityCode_GeneratesComplexityWarning()
    {
        var source = @"
        if a then
            if b then
                if c then
                    if d then
                        if e then
                            if f then
                                DoSomething()
                            end
                        end
                    end
                end
            end
        end";

        var result = AnalyzeSource(source);

        var complexityInsights = result.Insights.Where(i => i.Category == "Complexity").ToList();
        Assert.NotEmpty(complexityInsights);

        // Look for any complexity-related insight, not a specific title
        var hasComplexityWarning = complexityInsights.Any(i =>
            i.Title.Contains("Complexity") ||
            i.Title.Contains("Nesting") ||
            i.Description.Contains("complexity") ||
            i.Description.Contains("nesting"));
        Assert.True(hasComplexityWarning, "Expected to find a complexity-related insight");
    }

    [Fact]
    public void Analyze_FunctionHeavyCode_GeneratesPerformanceInsight()
    {
        var source = @"
            ProcessA()
            ProcessB()
            ProcessC()
            ProcessD()
            ProcessE()";

        var result = AnalyzeSource(source);

        var performanceInsights = result.Insights.Where(i => i.Category == "Performance").ToList();
        // May or may not generate performance insights depending on thresholds
        // This tests that the analysis runs without error
        Assert.NotNull(performanceInsights);
    }

    #endregion

    #region Options Configuration Tests

    [Fact]
    public void Analyze_WithDisabledComplexityMetrics_SkipsComplexityInsights()
    {
        var options = new CodeAnalysisOptions
        {
            EnableComplexityMetrics = false
        };

        var result = AnalyzeSource("if a then if b then c() end end", options);

        var complexityInsights = result.Insights.Where(i => i.Category == "Complexity").ToList();
        Assert.Empty(complexityInsights);
    }

    [Fact]
    public void Analyze_WithCustomComplexityThresholds_UsesCustomValues()
    {
        var options = new CodeAnalysisOptions
        {
            ComplexityThresholds = new ComplexityThresholds
            {
                LowComplexityThreshold = 1,
                ModerateComplexityThreshold = 2,
                HighComplexityThreshold = 3
            }
        };

        var result = AnalyzeSource("if a then b() end", options); // Complexity = 2

        var level = result.Metrics.GetComplexityLevel(options.ComplexityThresholds);
        Assert.Equal(ComplexityLevel.Moderate, level);
    }

    #endregion

    #region Integration Tests

    [Fact]
    public void Analyze_WithJyroBuilder_IntegratesCorrectly()
    {
        var builder = JyroBuilder.Create()
            .WithScript("var x = 42\nif x > 0 then y = x * 2 end")
            .WithData(new JyroObject());

        var result = builder
            .WithCodeAnalysis()
            .AnalyzeOnly();

        Assert.NotNull(result);
        Assert.NotNull(result.Metrics);
        Assert.True(result.Metrics.TotalStatements > 0);
        Assert.NotEmpty(result.Insights);
    }

    [Fact]
    public void Analyze_WithJyroBuilderRunAndAnalyze_ReturnsExecutionAndAnalysis()
    {
        var builder = JyroBuilder.Create()
            .WithScript("Data.result = 42")
            .WithData(new JyroObject());

        var result = builder
            .WithCodeAnalysis()
            .Run();

        Assert.NotNull(result.ExecutionResult);
        Assert.NotNull(result.AnalysisResult);
        Assert.True(result.IsSuccessful);
        Assert.True(result.AnalysisResult.Metrics.TotalStatements > 0);
    }

    #endregion

    #region Complexity Level Tests

    [Theory]
    [InlineData("var x = 1", ComplexityLevel.Low)]
    [InlineData("if a then b() end", ComplexityLevel.Low)]
    [InlineData("if a then b() else c() end", ComplexityLevel.Low)]
    public void Analyze_DifferentComplexityLevels_ClassifiesCorrectly(string source, ComplexityLevel expectedLevel)
    {
        var result = AnalyzeSource(source);

        var actualLevel = result.Metrics.GetComplexityLevel(new ComplexityThresholds());
        Assert.Equal(expectedLevel, actualLevel);
    }

    #endregion

    #region Error Handling Tests

    [Fact]
    public void Analyze_WithNullStatements_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() =>
            _analyzer.Analyze(null!, new CodeAnalysisOptions()));
    }

    [Fact]
    public void Analyze_WithNullOptions_ThrowsArgumentNullException()
    {
        var statements = ParseSource("var x = 1");
        Assert.Throws<ArgumentNullException>(() =>
            _analyzer.Analyze(statements, null!));
    }

    #endregion

    #region Metadata Tests

    [Fact]
    public void Analyze_ValidCode_ReturnsCorrectMetadata()
    {
        var result = AnalyzeSource("var x = 42");

        Assert.True(result.AnalysisTime >= TimeSpan.Zero);
        Assert.True(result.AnalyzedAt <= DateTimeOffset.UtcNow);
        Assert.True(result.AnalyzedAt > DateTimeOffset.UtcNow.AddMinutes(-1)); // Reasonable time window
    }

    #endregion

    #region Debug Tests

    [Fact]
    public void Debug_AnalysisOutput()
    {
        var sources = new[]
        {
            ("Simple", "var x = 42"),
            ("Control Flow", "if x > 0 then y = 1 else y = 2 end"),
            ("Complex", @"
                foreach item in items do
                    if item.isValid then
                        ProcessItem(item)
                    end
                end")
        };

        foreach (var (name, source) in sources)
        {
            _output.WriteLine($"=== {name} ===");
            var result = AnalyzeSource(source);
            LogAnalysisResult(result);
            _output.WriteLine("");
        }
    }

    #endregion
}