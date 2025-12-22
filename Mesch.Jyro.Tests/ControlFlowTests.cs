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

    // ===== Return Statement Tests =====

    [Fact]
    public void ReturnStatement_WithNoMessage_HaltsExecution()
    {
        var script = @"
            Data.before = 1
            return
            Data.after = 2
        ";
        var result = TestHelpers.ExecuteSuccessfully(script, output: _output);

        var data = (JyroObject)result.Data;
        Assert.Equal(1.0, ((JyroNumber)data.GetProperty("before")).Value);
        Assert.True(data.GetProperty("after") is JyroNull);
    }

    [Fact]
    public void ReturnStatement_WithMessage_HaltsExecutionAndAddsInfoMessage()
    {
        var script = @"
            Data.status = ""started""
            return ""All done!""
            Data.status = ""ended""
        ";
        var result = TestHelpers.ExecuteSuccessfully(script, output: _output);

        var data = (JyroObject)result.Data;
        Assert.Equal("started", ((JyroString)data.GetProperty("status")).Value);

        // Check for ScriptReturn message
        var returnMessage = result.Messages.FirstOrDefault(m => m.Code == MessageCode.ScriptReturn);
        Assert.NotNull(returnMessage);
        Assert.Equal(MessageSeverity.Info, returnMessage.Severity);
        Assert.Contains("All done!", returnMessage.Arguments);
    }

    [Fact]
    public void ReturnStatement_WithExpression_EvaluatesExpression()
    {
        var script = @"
            var name = ""World""
            return ""Hello, "" + name + ""!""
        ";
        var result = TestHelpers.ExecuteSuccessfully(script, output: _output);

        var returnMessage = result.Messages.FirstOrDefault(m => m.Code == MessageCode.ScriptReturn);
        Assert.NotNull(returnMessage);
        Assert.Contains("Hello, World!", returnMessage.Arguments);
    }

    [Fact]
    public void ReturnStatement_InsideLoop_ExitsImmediately()
    {
        var script = @"
            var items = [1, 2, 3, 4, 5]
            var sum = 0
            foreach item in items do
                if item == 3 then
                    return ""Stopped at 3""
                end
                sum = sum + item
            end
            Data.sum = sum
        ";
        var result = TestHelpers.ExecuteSuccessfully(script, output: _output);

        // sum should not be set since we returned early
        var data = (JyroObject)result.Data;
        Assert.True(data.GetProperty("sum") is JyroNull);

        var returnMessage = result.Messages.FirstOrDefault(m => m.Code == MessageCode.ScriptReturn);
        Assert.NotNull(returnMessage);
    }

    // ===== Fail Statement Tests =====

    [Fact]
    public void FailStatement_WithNoMessage_HaltsExecutionAndFails()
    {
        var script = @"
            Data.before = 1
            fail
            Data.after = 2
        ";
        var result = TestHelpers.Execute(script, output: _output);

        Assert.False(result.IsSuccessful);
        var data = (JyroObject)result.Data;
        Assert.Equal(1.0, ((JyroNumber)data.GetProperty("before")).Value);
        Assert.True(data.GetProperty("after") is JyroNull);

        // Check for ScriptFailure message with default text
        var failMessage = result.Messages.FirstOrDefault(m => m.Code == MessageCode.ScriptFailure);
        Assert.NotNull(failMessage);
        Assert.Equal(MessageSeverity.Error, failMessage.Severity);
    }

    [Fact]
    public void FailStatement_WithMessage_HaltsExecutionAndAddsErrorMessage()
    {
        var script = @"
            Data.status = ""validating""
            fail ""Validation failed: invalid input""
            Data.status = ""completed""
        ";
        var result = TestHelpers.Execute(script, output: _output);

        Assert.False(result.IsSuccessful);
        var data = (JyroObject)result.Data;
        Assert.Equal("validating", ((JyroString)data.GetProperty("status")).Value);

        // Check for ScriptFailure message
        var failMessage = result.Messages.FirstOrDefault(m => m.Code == MessageCode.ScriptFailure);
        Assert.NotNull(failMessage);
        Assert.Equal(MessageSeverity.Error, failMessage.Severity);
        Assert.Contains("Validation failed: invalid input", failMessage.Arguments);
    }

    [Fact]
    public void FailStatement_WithExpression_EvaluatesExpression()
    {
        var script = @"
            var field = ""email""
            fail ""Field '"" + field + ""' is required""
        ";
        var result = TestHelpers.Execute(script, output: _output);

        Assert.False(result.IsSuccessful);
        var failMessage = result.Messages.FirstOrDefault(m => m.Code == MessageCode.ScriptFailure);
        Assert.NotNull(failMessage);
        Assert.Contains("Field 'email' is required", failMessage.Arguments);
    }

    [Fact]
    public void FailStatement_InsideConditional_OnlyFailsWhenConditionMet()
    {
        var script = @"
            var age = 15
            if age < 18 then
                fail ""Must be 18 or older""
            end
            Data.approved = true
        ";
        var result = TestHelpers.Execute(script, output: _output);

        Assert.False(result.IsSuccessful);
        var data = (JyroObject)result.Data;
        Assert.True(data.GetProperty("approved") is JyroNull);
    }

    [Fact]
    public void FailStatement_InsideLoop_ExitsImmediately()
    {
        var script = @"
            var items = [1, 2, 3, 4, 5]
            Data.sum = 0
            foreach item in items do
                if item == 3 then
                    fail ""Found forbidden value 3""
                end
                Data.sum = Data.sum + item
            end
        ";
        var result = TestHelpers.Execute(script, output: _output);

        Assert.False(result.IsSuccessful);
        // sum should be 1+2=3 (processed before hitting 3)
        var data = (JyroObject)result.Data;
        Assert.Equal(3.0, ((JyroNumber)data.GetProperty("sum")).Value);
    }

    // ===== Mixed Return/Fail Scenarios =====

    [Fact]
    public void ReturnAndFail_ConditionalPath_Success()
    {
        var script = @"
            var valid = true
            if valid then
                return ""Validation passed""
            else
                fail ""Validation failed""
            end
        ";
        var result = TestHelpers.Execute(script, output: _output);

        Assert.True(result.IsSuccessful);
        Assert.Contains(result.Messages, m => m.Code == MessageCode.ScriptReturn);
    }

    [Fact]
    public void ReturnAndFail_ConditionalPath_Failure()
    {
        var script = @"
            var valid = false
            if valid then
                return ""Validation passed""
            else
                fail ""Validation failed""
            end
        ";
        var result = TestHelpers.Execute(script, output: _output);

        Assert.False(result.IsSuccessful);
        Assert.Contains(result.Messages, m => m.Code == MessageCode.ScriptFailure);
    }
}
