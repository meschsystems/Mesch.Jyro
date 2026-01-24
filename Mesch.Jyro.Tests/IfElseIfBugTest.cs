using Xunit.Abstractions;

namespace Mesch.Jyro.Tests;

/// <summary>
/// Isolated test to investigate the if-else-if bug
/// </summary>
public class IfElseIfBugTest
{
    private readonly ITestOutputHelper _output;

    public IfElseIfBugTest(ITestOutputHelper output)
    {
        _output = output;
    }

    [Fact]
    public void IfElseIf_FirstConditionTrue_ShouldExecuteFirstBranch()
    {
        var script = @"
            var quantity = 12
            var discount = 0

            if quantity >= 10 then
                discount = 0.15
            elseif quantity >= 5 then
                discount = 0.10
            end

            Data.discount = discount
        ";

        var result = TestHelpers.ExecuteSuccessfully(script, output: _output);

        var data = (JyroObject)result.Data;
        var discount = ((JyroNumber)data.GetProperty("discount")).Value;

        _output.WriteLine($"Quantity: 12, Discount: {discount}");
        _output.WriteLine($"Expected: 0.15 (first branch should execute when quantity >= 10)");

        Assert.Equal(0.15, discount);
    }

    [Fact]
    public void IfElseIf_SecondConditionTrue_ShouldExecuteSecondBranch()
    {
        var script = @"
            var quantity = 7
            var discount = 0

            if quantity >= 10 then
                discount = 0.15
            elseif quantity >= 5 then
                discount = 0.10
            end

            Data.discount = discount
        ";

        var result = TestHelpers.ExecuteSuccessfully(script, output: _output);

        var data = (JyroObject)result.Data;
        var discount = ((JyroNumber)data.GetProperty("discount")).Value;

        _output.WriteLine($"Quantity: 7, Discount: {discount}");
        _output.WriteLine($"Expected: 0.10 (second branch should execute when 5 <= quantity < 10)");

        Assert.Equal(0.10, discount);
    }

    [Fact]
    public void IfElseIf_NoConditionTrue_ShouldNotModify()
    {
        var script = @"
            var quantity = 3
            var discount = 0

            if quantity >= 10 then
                discount = 0.15
            elseif quantity >= 5 then
                discount = 0.10
            end

            Data.discount = discount
        ";

        var result = TestHelpers.ExecuteSuccessfully(script, output: _output);

        var data = (JyroObject)result.Data;
        var discount = ((JyroNumber)data.GetProperty("discount")).Value;

        _output.WriteLine($"Quantity: 3, Discount: {discount}");
        _output.WriteLine($"Expected: 0 (no branch should execute when quantity < 5)");

        Assert.Equal(0.0, discount);
    }

    [Fact]
    public void IfElseIf_MultipleElseIfBranches_MatchesCorrectBranch()
    {
        var script = @"
            var score = 75
            var grade = ""F""

            if score >= 90 then
                grade = ""A""
            elseif score >= 80 then
                grade = ""B""
            elseif score >= 70 then
                grade = ""C""
            elseif score >= 60 then
                grade = ""D""
            else
                grade = ""F""
            end

            Data.grade = grade
        ";

        var result = TestHelpers.ExecuteSuccessfully(script, output: _output);

        var data = (JyroObject)result.Data;
        var grade = ((JyroString)data.GetProperty("grade")).Value;

        _output.WriteLine($"Score: 75, Grade: {grade}");
        _output.WriteLine($"Expected: C (third branch should match when 70 <= score < 80)");

        Assert.Equal("C", grade);
    }

    [Fact]
    public void IfElseIf_FirstTrueWithMultipleBranches_SkipsAllElseIf()
    {
        var script = @"
            var score = 95
            var grade = ""F""
            var evaluationCount = 0

            if score >= 90 then
                grade = ""A""
                evaluationCount = 1
            elseif score >= 80 then
                grade = ""B""
                evaluationCount = 2
            elseif score >= 70 then
                grade = ""C""
                evaluationCount = 3
            elseif score >= 60 then
                grade = ""D""
                evaluationCount = 4
            else
                grade = ""F""
                evaluationCount = 5
            end

            Data.grade = grade
            Data.evaluationCount = evaluationCount
        ";

        var result = TestHelpers.ExecuteSuccessfully(script, output: _output);

        var data = (JyroObject)result.Data;
        var grade = ((JyroString)data.GetProperty("grade")).Value;
        var evalCount = ((JyroNumber)data.GetProperty("evaluationCount")).Value;

        _output.WriteLine($"Score: 95, Grade: {grade}, EvaluationCount: {evalCount}");
        _output.WriteLine($"Expected: A with evaluationCount=1 (first branch only)");

        Assert.Equal("A", grade);
        Assert.Equal(1.0, evalCount);
    }

    [Fact]
    public void IfElseIf_LastElseIfMatches_ExecutesCorrectBranch()
    {
        var script = @"
            var score = 62
            var grade = ""F""

            if score >= 90 then
                grade = ""A""
            elseif score >= 80 then
                grade = ""B""
            elseif score >= 70 then
                grade = ""C""
            elseif score >= 60 then
                grade = ""D""
            else
                grade = ""F""
            end

            Data.grade = grade
        ";

        var result = TestHelpers.ExecuteSuccessfully(script, output: _output);

        var data = (JyroObject)result.Data;
        var grade = ((JyroString)data.GetProperty("grade")).Value;

        _output.WriteLine($"Score: 62, Grade: {grade}");
        _output.WriteLine($"Expected: D (last elseif branch should match when 60 <= score < 70)");

        Assert.Equal("D", grade);
    }

    [Fact]
    public void IfElseIf_NoneMatch_ExecutesElseBranch()
    {
        var script = @"
            var score = 45
            var grade = ""X""

            if score >= 90 then
                grade = ""A""
            elseif score >= 80 then
                grade = ""B""
            elseif score >= 70 then
                grade = ""C""
            elseif score >= 60 then
                grade = ""D""
            else
                grade = ""F""
            end

            Data.grade = grade
        ";

        var result = TestHelpers.ExecuteSuccessfully(script, output: _output);

        var data = (JyroObject)result.Data;
        var grade = ((JyroString)data.GetProperty("grade")).Value;

        _output.WriteLine($"Score: 45, Grade: {grade}");
        _output.WriteLine($"Expected: F (else branch should execute when score < 60)");

        Assert.Equal("F", grade);
    }
}
