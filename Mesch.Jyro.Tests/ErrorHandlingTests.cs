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

    #region Validator Branch Validation Tests

    [Fact]
    public void Validator_CatchesUndeclaredVariableInElseIfBranch()
    {
        var script = @"
            if true then
                var x = 1
            elseif true then
                y = undeclaredVar
            end
        ";

        var result = TestHelpers.Execute(script, output: _output);

        Assert.False(result.IsSuccessful);
        Assert.Contains(result.Messages, m =>
            m.Severity == MessageSeverity.Error &&
            m.Stage == ProcessingStage.Validation);
    }

    [Fact]
    public void Validator_CatchesUndeclaredVariableInElseBranch()
    {
        var script = @"
            if false then
                var x = 1
            else
                y = undeclaredVar
            end
        ";

        var result = TestHelpers.Execute(script, output: _output);

        Assert.False(result.IsSuccessful);
        Assert.Contains(result.Messages, m =>
            m.Severity == MessageSeverity.Error &&
            m.Stage == ProcessingStage.Validation);
    }

    [Fact]
    public void Validator_CatchesUndeclaredVariableInSwitchCase()
    {
        var script = @"
            switch 1 do
                case 1 then
                    y = undeclaredVar
            end
        ";

        var result = TestHelpers.Execute(script, output: _output);

        Assert.False(result.IsSuccessful);
        Assert.Contains(result.Messages, m =>
            m.Severity == MessageSeverity.Error &&
            m.Stage == ProcessingStage.Validation);
    }

    [Fact]
    public void Validator_CatchesUndeclaredVariableInSwitchDefault()
    {
        var script = @"
            switch 1 do
                case 2 then
                    var x = 1
                default then
                    y = undeclaredVar
            end
        ";

        var result = TestHelpers.Execute(script, output: _output);

        Assert.False(result.IsSuccessful);
        Assert.Contains(result.Messages, m =>
            m.Severity == MessageSeverity.Error &&
            m.Stage == ProcessingStage.Validation);
    }

    [Fact]
    public void Validator_ValidatesAllIfElseIfElseBranches()
    {
        var script = @"
            if true then
                var a = 1
                Data.x = a
            elseif true then
                var b = 2
                Data.y = b
            else
                var c = 3
                Data.z = c
            end
        ";

        var result = TestHelpers.ExecuteSuccessfully(script, output: _output);
        Assert.True(result.IsSuccessful);
    }

    [Fact]
    public void Validator_ValidatesAllSwitchCasesAndDefault()
    {
        var script = @"
            switch 2 do
                case 1 then
                    var a = 1
                    Data.x = a
                case 2 then
                    var b = 2
                    Data.y = b
                default then
                    var c = 3
                    Data.z = c
            end
        ";

        var result = TestHelpers.ExecuteSuccessfully(script, output: _output);
        Assert.True(result.IsSuccessful);
    }

    [Fact]
    public void Validator_VariablesInOneBranchNotVisibleInOther()
    {
        var script = @"
            if true then
                var x = 1
            else
                Data.result = x
            end
        ";

        var result = TestHelpers.Execute(script, output: _output);

        Assert.False(result.IsSuccessful);
        Assert.Contains(result.Messages, m =>
            m.Severity == MessageSeverity.Error &&
            m.Stage == ProcessingStage.Validation);
    }

    #endregion

    #region Syntax Error Tests

    [Fact]
    public void SyntaxError_MissingCommaInArray_ReturnsSyntaxError()
    {
        var script = "var arr = [1 2 3]";

        var result = TestHelpers.Execute(script, output: _output);

        Assert.False(result.IsSuccessful);
        Assert.NotEmpty(result.Messages);
        Assert.Contains(result.Messages, m => m.Severity == MessageSeverity.Error);
        Assert.Contains(result.Messages, m => m.Stage == ProcessingStage.Parsing);
    }

    [Fact]
    public void SyntaxError_MissingCommaInObjectArray_ReturnsSyntaxError()
    {
        var script = @"
            var orders = [
                { ""id"": 1, ""status"": ""pending"" }
                { ""id"": 2, ""status"": ""completed"" }
            ]
        ";

        var result = TestHelpers.Execute(script, output: _output);

        Assert.False(result.IsSuccessful);
        Assert.NotEmpty(result.Messages);
        Assert.Contains(result.Messages, m => m.Severity == MessageSeverity.Error);
        Assert.Contains(result.Messages, m => m.Stage == ProcessingStage.Parsing);
    }

    [Fact]
    public void SyntaxError_MissingColonInObject_ReturnsSyntaxError()
    {
        var script = @"var obj = { ""name"" ""Alice"" }";

        var result = TestHelpers.Execute(script, output: _output);

        Assert.False(result.IsSuccessful);
        Assert.NotEmpty(result.Messages);
        Assert.Contains(result.Messages, m => m.Severity == MessageSeverity.Error);
        Assert.Contains(result.Messages, m => m.Stage == ProcessingStage.Parsing);
    }

    [Fact]
    public void SyntaxError_UnclosedBracket_ReturnsSyntaxError()
    {
        var script = "var arr = [1, 2, 3";

        var result = TestHelpers.Execute(script, output: _output);

        Assert.False(result.IsSuccessful);
        Assert.NotEmpty(result.Messages);
        Assert.Contains(result.Messages, m => m.Severity == MessageSeverity.Error);
        Assert.Contains(result.Messages, m => m.Stage == ProcessingStage.Parsing);
    }

    [Fact]
    public void SyntaxError_UnclosedBrace_ReturnsSyntaxError()
    {
        var script = @"var obj = { ""name"": ""Alice""";

        var result = TestHelpers.Execute(script, output: _output);

        Assert.False(result.IsSuccessful);
        Assert.NotEmpty(result.Messages);
        Assert.Contains(result.Messages, m => m.Severity == MessageSeverity.Error);
        Assert.Contains(result.Messages, m => m.Stage == ProcessingStage.Parsing);
    }

    [Fact]
    public void SyntaxError_ValidSyntax_ReturnsSuccess()
    {
        var script = "var arr = [1, 2, 3]";

        var result = TestHelpers.ExecuteSuccessfully(script, output: _output);

        Assert.True(result.IsSuccessful);
    }

    [Fact]
    public void SyntaxError_ValidObjectLiteral_ReturnsSuccess()
    {
        var script = @"
            var orders = [
                { ""id"": 1, ""status"": ""pending"" },
                { ""id"": 2, ""status"": ""completed"" }
            ]
            Data.count = Length(orders)
        ";

        var result = TestHelpers.ExecuteSuccessfully(script, output: _output);

        Assert.True(result.IsSuccessful);
        var data = (JyroObject)result.Data;
        Assert.Equal(2.0, ((JyroNumber)data.GetProperty("count")).Value);
    }

    [Fact]
    public void SyntaxError_ErrorMessageIncludesLineNumber()
    {
        var script = @"
            var x = 1
            var arr = [1 2 3]
        ";

        var result = TestHelpers.Execute(script, output: _output);

        Assert.False(result.IsSuccessful);
        var syntaxError = result.Messages.First(m => m.Stage == ProcessingStage.Parsing);
        Assert.True(syntaxError.LineNumber > 0, "Line number should be reported");
    }

    #endregion

    #region Argument Count Validation Tests

    [Fact]
    public void TooFewArguments_FailsLinking()
    {
        // Length() requires 1 argument
        var script = "Data.result = Length()";

        var result = TestHelpers.Execute(script, output: _output);

        Assert.False(result.IsSuccessful);
        Assert.Contains(result.Messages, m =>
            m.Code == MessageCode.TooFewArguments &&
            m.Stage == ProcessingStage.Linking);
    }

    [Fact]
    public void TooManyArguments_FailsLinking()
    {
        // Length() accepts 1 argument max
        var script = "Data.result = Length([1,2,3], \"extra\", 999)";

        var result = TestHelpers.Execute(script, output: _output);

        Assert.False(result.IsSuccessful);
        Assert.Contains(result.Messages, m =>
            m.Code == MessageCode.TooManyArguments &&
            m.Stage == ProcessingStage.Linking);
    }

    [Fact]
    public void OptionalArguments_AllowsVariableCount()
    {
        // Round() has an optional second parameter (digits)
        var script = @"
            Data.r1 = Round(3.14159)
            Data.r2 = Round(3.14159, 2)
        ";

        var result = TestHelpers.ExecuteSuccessfully(script, output: _output);
        Assert.True(result.IsSuccessful);
        var data = (JyroObject)result.Data;
        Assert.Equal(3.0, ((JyroNumber)data.GetProperty("r1")).Value);
        Assert.Equal(3.14, ((JyroNumber)data.GetProperty("r2")).Value);
    }

    [Fact]
    public void CorrectArgumentCount_Succeeds()
    {
        // Length() with exactly 1 argument
        var script = "Data.result = Length([1, 2, 3])";

        var result = TestHelpers.ExecuteSuccessfully(script, output: _output);
        Assert.True(result.IsSuccessful);
        var data = (JyroObject)result.Data;
        Assert.Equal(3.0, ((JyroNumber)data.GetProperty("result")).Value);
    }

    #endregion
}
