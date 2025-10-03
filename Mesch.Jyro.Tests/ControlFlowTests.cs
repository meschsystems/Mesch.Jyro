using Xunit.Abstractions;

namespace Mesch.Jyro.Tests;

public class ControlFlowTests
{
    private readonly ITestOutputHelper _output;

    public ControlFlowTests(ITestOutputHelper output)
    {
        _output = output;
    }

    [Fact]
    public void IfStatement_TrueBranch()
    {
        var script = @"
            if 5 > 3 then
                Data.result = ""yes""
            end
        ";
        var result = TestHelpers.ExecuteSuccessfully(script, output: _output);

        var data = (JyroObject)result.Data;
        Assert.Equal("yes", ((JyroString)data.GetProperty("result")).Value);
    }

    [Fact]
    public void IfStatement_FalseBranch()
    {
        var script = @"
            Data.result = ""default""
            if 3 > 5 then
                Data.result = ""yes""
            end
        ";
        var result = TestHelpers.ExecuteSuccessfully(script, output: _output);

        var data = (JyroObject)result.Data;
        Assert.Equal("default", ((JyroString)data.GetProperty("result")).Value);
    }

    [Fact]
    public void IfElseStatement()
    {
        var script = @"
            if 3 > 5 then
                Data.result = ""yes""
            else
                Data.result = ""no""
            end
        ";
        var result = TestHelpers.ExecuteSuccessfully(script, output: _output);

        var data = (JyroObject)result.Data;
        Assert.Equal("no", ((JyroString)data.GetProperty("result")).Value);
    }

    [Fact]
    public void IfElseIfStatement()
    {
        var script = @"
            var x = 50
            if x >= 100 then
                Data.result = ""high""
            else if x >= 50 then
                Data.result = ""medium""
            else
                Data.result = ""low""
            end
        ";
        var result = TestHelpers.ExecuteSuccessfully(script, output: _output);

        var data = (JyroObject)result.Data;
        Assert.Equal("medium", ((JyroString)data.GetProperty("result")).Value);
    }

    [Fact]
    public void SwitchStatement_MatchCase()
    {
        var script = @"
            var x = 2
            switch x do
                case 1 then
                    Data.result = ""one""
                case 2 then
                    Data.result = ""two""
                case 3 then
                    Data.result = ""three""
            end
        ";
        var result = TestHelpers.ExecuteSuccessfully(script, output: _output);

        var data = (JyroObject)result.Data;
        Assert.Equal("two", ((JyroString)data.GetProperty("result")).Value);
    }

    [Fact]
    public void SwitchStatement_DefaultCase()
    {
        var script = @"
            var x = 99
            switch x do
                case 1 then
                    Data.result = ""one""
                case 2 then
                    Data.result = ""two""
                default then
                    Data.result = ""other""
            end
        ";
        var result = TestHelpers.ExecuteSuccessfully(script, output: _output);

        var data = (JyroObject)result.Data;
        Assert.Equal("other", ((JyroString)data.GetProperty("result")).Value);
    }

    [Fact]
    public void SwitchStatement_BooleanConditions()
    {
        var script = @"
            var points = 75
            switch true do
                case points >= 100 then
                    Data.tier = ""Gold""
                case points >= 50 then
                    Data.tier = ""Silver""
                default then
                    Data.tier = ""Bronze""
            end
        ";
        var result = TestHelpers.ExecuteSuccessfully(script, output: _output);

        var data = (JyroObject)result.Data;
        Assert.Equal("Silver", ((JyroString)data.GetProperty("tier")).Value);
    }

    [Fact]
    public void WhileLoop_BasicIteration()
    {
        var script = @"
            var i = 0
            var sum = 0
            while i < 5 do
                sum = sum + i
                i = i + 1
            end
            Data.result = sum
        ";
        var result = TestHelpers.ExecuteSuccessfully(script, output: _output);

        var data = (JyroObject)result.Data;
        Assert.Equal(10.0, ((JyroNumber)data.GetProperty("result")).Value); // 0+1+2+3+4
    }

    [Fact]
    public void WhileLoop_Break()
    {
        var script = @"
            var i = 0
            var sum = 0
            while i < 10 do
                if i == 5 then
                    break
                end
                sum = sum + i
                i = i + 1
            end
            Data.result = sum
        ";
        var result = TestHelpers.ExecuteSuccessfully(script, output: _output);

        var data = (JyroObject)result.Data;
        Assert.Equal(10.0, ((JyroNumber)data.GetProperty("result")).Value); // 0+1+2+3+4
    }

    [Fact]
    public void WhileLoop_Continue()
    {
        var script = @"
            var i = 0
            var sum = 0
            while i < 5 do
                i = i + 1
                if i == 3 then
                    continue
                end
                sum = sum + i
            end
            Data.result = sum
        ";
        var result = TestHelpers.ExecuteSuccessfully(script, output: _output);

        var data = (JyroObject)result.Data;
        Assert.Equal(12.0, ((JyroNumber)data.GetProperty("result")).Value); // 1+2+4+5
    }

    [Fact]
    public void ForEachLoop_Array()
    {
        var script = @"
            var arr = [10, 20, 30]
            var sum = 0
            foreach item in arr do
                sum = sum + item
            end
            Data.result = sum
        ";
        var result = TestHelpers.ExecuteSuccessfully(script, output: _output);

        var data = (JyroObject)result.Data;
        Assert.Equal(60.0, ((JyroNumber)data.GetProperty("result")).Value);
    }

    [Fact]
    public void ForEachLoop_Object()
    {
        var script = @"
            var obj = {""a"": 1, ""b"": 2, ""c"": 3}
            var keys = []
            foreach key in obj do
                Append(keys, key)
            end
            Data.count = Length(keys)
        ";
        var result = TestHelpers.ExecuteSuccessfully(script, output: _output);

        var data = (JyroObject)result.Data;
        Assert.Equal(3.0, ((JyroNumber)data.GetProperty("count")).Value);
    }

    [Fact]
    public void ForEachLoop_WithBreak()
    {
        var script = @"
            var arr = [1, 2, 3, 4, 5]
            var sum = 0
            foreach item in arr do
                if item > 3 then
                    break
                end
                sum = sum + item
            end
            Data.result = sum
        ";
        var result = TestHelpers.ExecuteSuccessfully(script, output: _output);

        var data = (JyroObject)result.Data;
        Assert.Equal(6.0, ((JyroNumber)data.GetProperty("result")).Value); // 1+2+3
    }

    [Fact]
    public void NestedLoops()
    {
        var script = @"
            var result = []
            var i = 1
            while i <= 3 do
                var j = 1
                while j <= 2 do
                    Append(result, i * 10 + j)
                    j = j + 1
                end
                i = i + 1
            end
            Data.result = result
        ";
        var result = TestHelpers.ExecuteSuccessfully(script, output: _output);

        var data = (JyroObject)result.Data;
        var arr = (JyroArray)data.GetProperty("result");
        Assert.Equal(6, arr.Length);
        Assert.Equal(11.0, ((JyroNumber)arr[0]).Value);
        Assert.Equal(12.0, ((JyroNumber)arr[1]).Value);
        Assert.Equal(21.0, ((JyroNumber)arr[2]).Value);
    }

    [Fact]
    public void VariableScoping_LocalScope()
    {
        var script = @"
            var x = 10
            if true then
                var x = 20
                Data.inner = x
            end
            Data.outer = x
        ";
        var result = TestHelpers.ExecuteSuccessfully(script, output: _output);

        var data = (JyroObject)result.Data;
        Assert.Equal(20.0, ((JyroNumber)data.GetProperty("inner")).Value);
        Assert.Equal(10.0, ((JyroNumber)data.GetProperty("outer")).Value);
    }
}
