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
            else if quantity >= 5 then
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
            else if quantity >= 5 then
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
            else if quantity >= 5 then
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
}
