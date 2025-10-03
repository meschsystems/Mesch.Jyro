using Xunit;
using Xunit.Abstractions;

namespace Mesch.Jyro.Tests;

public class CompoundConditionBugTest
{
    private readonly ITestOutputHelper _output;

    public CompoundConditionBugTest(ITestOutputHelper output)
    {
        _output = output;
    }

    [Fact]
    public void AndCondition_BothTrue_ShouldExecute()
    {
        var script = @"
            var a = true
            var b = true
            var result = false

            if a == true and b == true then
                result = true
            end

            Data.result = result
        ";

        var result = TestHelpers.ExecuteSuccessfully(script, null, _output);
        var data = (JyroObject)result.Data;
        var resultValue = (JyroBoolean)data.GetProperty("result");

        Assert.True(resultValue.Value);
    }

    [Fact]
    public void AndCondition_FirstTrueSecondFalse_ShouldNotExecute()
    {
        var script = @"
            var a = true
            var b = false
            var result = false

            if a == true and b == true then
                result = true
            end

            Data.result = result
        ";

        var result = TestHelpers.ExecuteSuccessfully(script, null, _output);
        var data = (JyroObject)result.Data;
        var resultValue = (JyroBoolean)data.GetProperty("result");

        Assert.False(resultValue.Value);
    }

    [Fact]
    public void AndCondition_ComparisonAndComparison_ShouldWork()
    {
        var script = @"
            var active = true
            var price = 5
            var matched = false

            if active == true and price > 10 then
                matched = true
            end

            Data.matched = matched
        ";

        var result = TestHelpers.ExecuteSuccessfully(script, null, _output);
        var data = (JyroObject)result.Data;
        var matched = (JyroBoolean)data.GetProperty("matched");

        _output.WriteLine($"active=true, price=5, price>10=false, matched={matched.Value}");
        Assert.False(matched.Value); // Should NOT match because price <= 10
    }

    [Fact]
    public void AndCondition_WithPropertyAccess_ShouldWork()
    {
        var script = @"
            Data.result = false

            if Data.item.active == true and Data.item.price > 10 then
                Data.result = true
            end
        ";

        var testData = new JyroObject();
        var item = new JyroObject();
        item.SetProperty("active", JyroBoolean.FromBoolean(true));
        item.SetProperty("price", new JyroNumber(5));
        testData.SetProperty("item", item);

        var result = TestHelpers.ExecuteSuccessfully(script, testData, _output);
        var data = (JyroObject)result.Data;
        var matched = (JyroBoolean)data.GetProperty("result");

        _output.WriteLine($"item.active=true, item.price=5, matched={matched.Value}");
        Assert.False(matched.Value); // Should NOT match because price <= 10
    }
}
