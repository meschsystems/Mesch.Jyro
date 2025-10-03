using Xunit.Abstractions;

namespace Mesch.Jyro.Tests;

public class ErrorHandlingTests
{
    private readonly ITestOutputHelper _output;

    public ErrorHandlingTests(ITestOutputHelper output)
    {
        _output = output;
    }

    [Fact]
    public void DivisionByZero_ThrowsRuntimeException()
    {
        var script = "Data.result = 10 / 0";

        var result = TestHelpers.Execute(script, output: _output);

        Assert.False(result.IsSuccessful);
        Assert.Contains(result.Messages, m => m.Severity == MessageSeverity.Error);
    }

    [Fact]
    public void UndefinedVariable_FailsValidation()
    {
        var script = "Data.result = undefinedVariable";

        var result = TestHelpers.Execute(script, output: _output);

        Assert.False(result.IsSuccessful);
        Assert.Contains(result.Messages, m => m.Severity == MessageSeverity.Error);
    }

    [Fact]
    public void UndefinedFunction_FailsLinking()
    {
        var script = "Data.result = UndefinedFunction()";

        var result = TestHelpers.Execute(script, output: _output);

        Assert.False(result.IsSuccessful);
        Assert.Contains(result.Messages, m => m.Severity == MessageSeverity.Error);
    }

    [Fact]
    public void InvalidArrayIndex_ThrowsRuntimeException()
    {
        var script = @"
            var arr = [1, 2, 3]
            Data.result = arr[10]
        ";

        var result = TestHelpers.Execute(script, output: _output);

        Assert.False(result.IsSuccessful);
        Assert.Contains(result.Messages, m => m.Severity == MessageSeverity.Error);
    }

    [Fact]
    public void NegativeArrayIndex_ThrowsRuntimeException()
    {
        var script = @"
            var arr = [1, 2, 3]
            Data.result = arr[-1]
        ";

        var result = TestHelpers.Execute(script, output: _output);

        Assert.False(result.IsSuccessful);
        Assert.Contains(result.Messages, m => m.Severity == MessageSeverity.Error);
    }

    [Fact]
    public void AccessPropertyOnNull_ThrowsRuntimeException()
    {
        var script = @"
            var obj = null
            Data.result = obj.property
        ";

        var result = TestHelpers.Execute(script, output: _output);

        Assert.False(result.IsSuccessful);
        Assert.Contains(result.Messages, m => m.Severity == MessageSeverity.Error);
    }

    [Fact]
    public void TypeMismatch_InComparison()
    {
        // Comparing incompatible types should work but return false
        var script = @"Data.result = ""hello"" == 123";

        var result = TestHelpers.ExecuteSuccessfully(script, output: _output);
        var data = (JyroObject)result.Data;
        Assert.False(((JyroBoolean)data.GetProperty("result")).Value);
    }

    [Fact]
    public void BreakOutsideLoop_FailsValidation()
    {
        var script = @"
            Data.x = 1
            break
        ";

        var result = TestHelpers.Execute(script, output: _output);

        Assert.False(result.IsSuccessful);
        Assert.Contains(result.Messages, m => m.Severity == MessageSeverity.Error);
    }

    [Fact]
    public void ContinueOutsideLoop_FailsValidation()
    {
        var script = @"
            Data.x = 1
            continue
        ";

        var result = TestHelpers.Execute(script, output: _output);

        Assert.False(result.IsSuccessful);
        Assert.Contains(result.Messages, m => m.Severity == MessageSeverity.Error);
    }

    [Fact]
    public void EmptyScript_ExecutesSuccessfully()
    {
        var script = "";

        var result = TestHelpers.ExecuteSuccessfully(script, output: _output);
        Assert.True(result.IsSuccessful);
    }

    [Fact]
    public void CommentsOnly_ExecutesSuccessfully()
    {
        var script = @"
            # This is a comment
            # Another comment
        ";

        var result = TestHelpers.ExecuteSuccessfully(script, output: _output);
        Assert.True(result.IsSuccessful);
    }

    [Fact]
    public void NullHandling_AllowsNullAssignment()
    {
        var script = "Data.result = null";

        var result = TestHelpers.ExecuteSuccessfully(script, output: _output);
        var data = (JyroObject)result.Data;
        Assert.True(data.GetProperty("result").IsNull);
    }

    [Fact]
    public void NullHandling_NullCoalescing()
    {
        var script = @"
            var x = null
            Data.result = x == null ? ""was null"" : ""not null""
        ";

        var result = TestHelpers.ExecuteSuccessfully(script, output: _output);
        var data = (JyroObject)result.Data;
        Assert.Equal("was null", ((JyroString)data.GetProperty("result")).Value);
    }

    [Fact]
    public void EmptyArray_HandlesGracefully()
    {
        var script = @"
            var arr = []
            Data.length = Length(arr)
            var sum = 0
            foreach item in arr do
                sum = sum + 1
            end
            Data.iterations = sum
        ";

        var result = TestHelpers.ExecuteSuccessfully(script, output: _output);
        var data = (JyroObject)result.Data;
        Assert.Equal(0.0, ((JyroNumber)data.GetProperty("length")).Value);
        Assert.Equal(0.0, ((JyroNumber)data.GetProperty("iterations")).Value);
    }

    [Fact]
    public void EmptyObject_HandlesGracefully()
    {
        var script = @"
            var obj = {}
            var count = 0
            foreach key in obj do
                count = count + 1
            end
            Data.count = count
        ";

        var result = TestHelpers.ExecuteSuccessfully(script, output: _output);
        var data = (JyroObject)result.Data;
        Assert.Equal(0.0, ((JyroNumber)data.GetProperty("count")).Value);
    }

    [Fact]
    public void DeepNesting_HandlesCorrectly()
    {
        var script = @"
            Data.result = {
                ""level1"": {
                    ""level2"": {
                        ""level3"": {
                            ""value"": 42
                        }
                    }
                }
            }
        ";

        var result = TestHelpers.ExecuteSuccessfully(script, output: _output);
        var data = (JyroObject)result.Data;
        var level1 = (JyroObject)data.GetProperty("result");
        var level2 = (JyroObject)level1.GetProperty("level1");
        var level3 = (JyroObject)level2.GetProperty("level2");
        var level4 = (JyroObject)level3.GetProperty("level3");
        Assert.Equal(42.0, ((JyroNumber)level4.GetProperty("value")).Value);
    }

    [Fact]
    public void StringEscaping_HandlesSpecialCharacters()
    {
        var script = @"Data.result = ""Line1\nLine2""";

        var result = TestHelpers.ExecuteSuccessfully(script, output: _output);
        var data = (JyroObject)result.Data;
        var str = ((JyroString)data.GetProperty("result")).Value;
        Assert.Contains("\n", str);
    }

    [Fact]
    public void BooleanTruthiness_ZeroIsFalsy()
    {
        var script = @"
            var x = 0
            if x then
                Data.result = ""truthy""
            else
                Data.result = ""falsy""
            end
        ";

        var result = TestHelpers.ExecuteSuccessfully(script, output: _output);
        var data = (JyroObject)result.Data;
        Assert.Equal("falsy", ((JyroString)data.GetProperty("result")).Value);
    }

    [Fact]
    public void BooleanTruthiness_EmptyStringIsFalsy()
    {
        var script = @"
            var x = """"
            if x then
                Data.result = ""truthy""
            else
                Data.result = ""falsy""
            end
        ";

        var result = TestHelpers.ExecuteSuccessfully(script, output: _output);
        var data = (JyroObject)result.Data;
        Assert.Equal("falsy", ((JyroString)data.GetProperty("result")).Value);
    }

    [Fact]
    public void BooleanTruthiness_NonZeroIsTruthy()
    {
        var script = @"
            var x = 5
            if x then
                Data.result = ""truthy""
            else
                Data.result = ""falsy""
            end
        ";

        var result = TestHelpers.ExecuteSuccessfully(script, output: _output);
        var data = (JyroObject)result.Data;
        Assert.Equal("truthy", ((JyroString)data.GetProperty("result")).Value);
    }
}
