namespace Mesch.Jyro.Tests;

public class SchemaValidationTests
{
    private readonly ITestOutputHelper _output;

    public SchemaValidationTests(ITestOutputHelper output)
    {
        _output = output;
    }

    #region ValidateSchema - Basic Type Validation

    [Fact]
    public void ValidateSchema_StringType_ValidString_ReturnsTrue()
    {
        var script = @"
            var schema = { ""type"": ""string"" }
            Data.result = ValidateSchema(""hello"", schema)
        ";
        var result = TestHelpers.ExecuteSuccessfully(script, output: _output);

        var data = (JyroObject)result.Data;
        Assert.True(((JyroBoolean)data.GetProperty("result")).Value);
    }

    [Fact]
    public void ValidateSchema_StringType_InvalidType_ReturnsFalse()
    {
        var script = @"
            var schema = { ""type"": ""string"" }
            Data.result = ValidateSchema(123, schema)
        ";
        var result = TestHelpers.ExecuteSuccessfully(script, output: _output);

        var data = (JyroObject)result.Data;
        Assert.False(((JyroBoolean)data.GetProperty("result")).Value);
    }

    [Fact]
    public void ValidateSchema_NumberType_ValidNumber_ReturnsTrue()
    {
        var script = @"
            var schema = { ""type"": ""number"" }
            Data.result = ValidateSchema(42, schema)
        ";
        var result = TestHelpers.ExecuteSuccessfully(script, output: _output);

        var data = (JyroObject)result.Data;
        Assert.True(((JyroBoolean)data.GetProperty("result")).Value);
    }

    [Fact]
    public void ValidateSchema_NumberType_InvalidType_ReturnsFalse()
    {
        var script = @"
            var schema = { ""type"": ""number"" }
            Data.result = ValidateSchema(""not a number"", schema)
        ";
        var result = TestHelpers.ExecuteSuccessfully(script, output: _output);

        var data = (JyroObject)result.Data;
        Assert.False(((JyroBoolean)data.GetProperty("result")).Value);
    }

    [Fact]
    public void ValidateSchema_BooleanType_ValidBoolean_ReturnsTrue()
    {
        var script = @"
            var schema = { ""type"": ""boolean"" }
            Data.result = ValidateSchema(true, schema)
        ";
        var result = TestHelpers.ExecuteSuccessfully(script, output: _output);

        var data = (JyroObject)result.Data;
        Assert.True(((JyroBoolean)data.GetProperty("result")).Value);
    }

    [Fact]
    public void ValidateSchema_NullType_ValidNull_ReturnsTrue()
    {
        var script = @"
            var schema = { ""type"": ""null"" }
            Data.result = ValidateSchema(null, schema)
        ";
        var result = TestHelpers.ExecuteSuccessfully(script, output: _output);

        var data = (JyroObject)result.Data;
        Assert.True(((JyroBoolean)data.GetProperty("result")).Value);
    }

    #endregion

    #region ValidateSchema - Object Validation

    [Fact]
    public void ValidateSchema_ObjectType_ValidObject_ReturnsTrue()
    {
        var script = @"
            var schema = { ""type"": ""object"" }
            var obj = { ""name"": ""Alice"" }
            Data.result = ValidateSchema(obj, schema)
        ";
        var result = TestHelpers.ExecuteSuccessfully(script, output: _output);

        var data = (JyroObject)result.Data;
        Assert.True(((JyroBoolean)data.GetProperty("result")).Value);
    }

    [Fact]
    public void ValidateSchema_ObjectWithProperties_ValidData_ReturnsTrue()
    {
        var script = @"
            var schema = {
                ""type"": ""object"",
                ""properties"": {
                    ""name"": { ""type"": ""string"" },
                    ""age"": { ""type"": ""number"" }
                }
            }
            var obj = { ""name"": ""Alice"", ""age"": 30 }
            Data.result = ValidateSchema(obj, schema)
        ";
        var result = TestHelpers.ExecuteSuccessfully(script, output: _output);

        var data = (JyroObject)result.Data;
        Assert.True(((JyroBoolean)data.GetProperty("result")).Value);
    }

    [Fact]
    public void ValidateSchema_ObjectWithProperties_InvalidPropertyType_ReturnsFalse()
    {
        var script = @"
            var schema = {
                ""type"": ""object"",
                ""properties"": {
                    ""age"": { ""type"": ""number"" }
                }
            }
            var obj = { ""age"": ""thirty"" }
            Data.result = ValidateSchema(obj, schema)
        ";
        var result = TestHelpers.ExecuteSuccessfully(script, output: _output);

        var data = (JyroObject)result.Data;
        Assert.False(((JyroBoolean)data.GetProperty("result")).Value);
    }

    [Fact]
    public void ValidateSchema_RequiredProperties_AllPresent_ReturnsTrue()
    {
        var script = @"
            var schema = {
                ""type"": ""object"",
                ""required"": [""name"", ""email""]
            }
            var obj = { ""name"": ""Alice"", ""email"": ""alice@example.com"" }
            Data.result = ValidateSchema(obj, schema)
        ";
        var result = TestHelpers.ExecuteSuccessfully(script, output: _output);

        var data = (JyroObject)result.Data;
        Assert.True(((JyroBoolean)data.GetProperty("result")).Value);
    }

    [Fact]
    public void ValidateSchema_RequiredProperties_MissingRequired_ReturnsFalse()
    {
        var script = @"
            var schema = {
                ""type"": ""object"",
                ""required"": [""name"", ""email""]
            }
            var obj = { ""name"": ""Alice"" }
            Data.result = ValidateSchema(obj, schema)
        ";
        var result = TestHelpers.ExecuteSuccessfully(script, output: _output);

        var data = (JyroObject)result.Data;
        Assert.False(((JyroBoolean)data.GetProperty("result")).Value);
    }

    #endregion

    #region ValidateSchema - Array Validation

    [Fact]
    public void ValidateSchema_ArrayType_ValidArray_ReturnsTrue()
    {
        var script = @"
            var schema = { ""type"": ""array"" }
            var arr = [1, 2, 3]
            Data.result = ValidateSchema(arr, schema)
        ";
        var result = TestHelpers.ExecuteSuccessfully(script, output: _output);

        var data = (JyroObject)result.Data;
        Assert.True(((JyroBoolean)data.GetProperty("result")).Value);
    }

    [Fact]
    public void ValidateSchema_ArrayWithItemsSchema_ValidItems_ReturnsTrue()
    {
        var script = @"
            var schema = {
                ""type"": ""array"",
                ""items"": { ""type"": ""number"" }
            }
            var arr = [1, 2, 3, 4, 5]
            Data.result = ValidateSchema(arr, schema)
        ";
        var result = TestHelpers.ExecuteSuccessfully(script, output: _output);

        var data = (JyroObject)result.Data;
        Assert.True(((JyroBoolean)data.GetProperty("result")).Value);
    }

    [Fact]
    public void ValidateSchema_ArrayWithItemsSchema_InvalidItem_ReturnsFalse()
    {
        var script = @"
            var schema = {
                ""type"": ""array"",
                ""items"": { ""type"": ""number"" }
            }
            var arr = [1, ""two"", 3]
            Data.result = ValidateSchema(arr, schema)
        ";
        var result = TestHelpers.ExecuteSuccessfully(script, output: _output);

        var data = (JyroObject)result.Data;
        Assert.False(((JyroBoolean)data.GetProperty("result")).Value);
    }

    #endregion

    #region ValidateSchema - Numeric Constraints

    [Fact]
    public void ValidateSchema_Minimum_ValueAboveMinimum_ReturnsTrue()
    {
        var script = @"
            var schema = { ""type"": ""number"", ""minimum"": 0 }
            Data.result = ValidateSchema(50, schema)
        ";
        var result = TestHelpers.ExecuteSuccessfully(script, output: _output);

        var data = (JyroObject)result.Data;
        Assert.True(((JyroBoolean)data.GetProperty("result")).Value);
    }

    [Fact]
    public void ValidateSchema_Minimum_ValueBelowMinimum_ReturnsFalse()
    {
        var script = @"
            var schema = { ""type"": ""number"", ""minimum"": 0 }
            Data.result = ValidateSchema(-5, schema)
        ";
        var result = TestHelpers.ExecuteSuccessfully(script, output: _output);

        var data = (JyroObject)result.Data;
        Assert.False(((JyroBoolean)data.GetProperty("result")).Value);
    }

    [Fact]
    public void ValidateSchema_Maximum_ValueBelowMaximum_ReturnsTrue()
    {
        var script = @"
            var schema = { ""type"": ""number"", ""maximum"": 100 }
            Data.result = ValidateSchema(50, schema)
        ";
        var result = TestHelpers.ExecuteSuccessfully(script, output: _output);

        var data = (JyroObject)result.Data;
        Assert.True(((JyroBoolean)data.GetProperty("result")).Value);
    }

    [Fact]
    public void ValidateSchema_Maximum_ValueAboveMaximum_ReturnsFalse()
    {
        var script = @"
            var schema = { ""type"": ""number"", ""maximum"": 100 }
            Data.result = ValidateSchema(150, schema)
        ";
        var result = TestHelpers.ExecuteSuccessfully(script, output: _output);

        var data = (JyroObject)result.Data;
        Assert.False(((JyroBoolean)data.GetProperty("result")).Value);
    }

    [Fact]
    public void ValidateSchema_MinimumAndMaximum_ValueInRange_ReturnsTrue()
    {
        var script = @"
            var schema = { ""type"": ""number"", ""minimum"": 0, ""maximum"": 100 }
            Data.result = ValidateSchema(50, schema)
        ";
        var result = TestHelpers.ExecuteSuccessfully(script, output: _output);

        var data = (JyroObject)result.Data;
        Assert.True(((JyroBoolean)data.GetProperty("result")).Value);
    }

    #endregion

    #region ValidateSchema - Enum Validation

    [Fact]
    public void ValidateSchema_Enum_ValidValue_ReturnsTrue()
    {
        var script = @"
            var schema = {
                ""type"": ""string"",
                ""enum"": [""draft"", ""published"", ""archived""]
            }
            Data.result = ValidateSchema(""published"", schema)
        ";
        var result = TestHelpers.ExecuteSuccessfully(script, output: _output);

        var data = (JyroObject)result.Data;
        Assert.True(((JyroBoolean)data.GetProperty("result")).Value);
    }

    [Fact]
    public void ValidateSchema_Enum_InvalidValue_ReturnsFalse()
    {
        var script = @"
            var schema = {
                ""type"": ""string"",
                ""enum"": [""draft"", ""published"", ""archived""]
            }
            Data.result = ValidateSchema(""deleted"", schema)
        ";
        var result = TestHelpers.ExecuteSuccessfully(script, output: _output);

        var data = (JyroObject)result.Data;
        Assert.False(((JyroBoolean)data.GetProperty("result")).Value);
    }

    #endregion

    #region ValidateSchema - Nested Objects

    [Fact]
    public void ValidateSchema_NestedObject_ValidData_ReturnsTrue()
    {
        var script = @"
            var schema = {
                ""type"": ""object"",
                ""properties"": {
                    ""user"": {
                        ""type"": ""object"",
                        ""properties"": {
                            ""name"": { ""type"": ""string"" },
                            ""email"": { ""type"": ""string"" }
                        },
                        ""required"": [""email""]
                    }
                }
            }
            var obj = {
                ""user"": {
                    ""name"": ""Alice"",
                    ""email"": ""alice@example.com""
                }
            }
            Data.result = ValidateSchema(obj, schema)
        ";
        var result = TestHelpers.ExecuteSuccessfully(script, output: _output);

        var data = (JyroObject)result.Data;
        Assert.True(((JyroBoolean)data.GetProperty("result")).Value);
    }

    [Fact]
    public void ValidateSchema_NestedObject_MissingNestedRequired_ReturnsFalse()
    {
        var script = @"
            var schema = {
                ""type"": ""object"",
                ""properties"": {
                    ""user"": {
                        ""type"": ""object"",
                        ""required"": [""email""]
                    }
                }
            }
            var obj = {
                ""user"": {
                    ""name"": ""Alice""
                }
            }
            Data.result = ValidateSchema(obj, schema)
        ";
        var result = TestHelpers.ExecuteSuccessfully(script, output: _output);

        var data = (JyroObject)result.Data;
        Assert.False(((JyroBoolean)data.GetProperty("result")).Value);
    }

    #endregion

    #region GetSchemaErrors - Basic Tests

    [Fact]
    public void GetSchemaErrors_ValidData_ReturnsEmptyArray()
    {
        var script = @"
            var schema = { ""type"": ""string"" }
            Data.errors = GetSchemaErrors(""hello"", schema)
            Data.count = Length(Data.errors)
        ";
        var result = TestHelpers.ExecuteSuccessfully(script, output: _output);

        var data = (JyroObject)result.Data;
        Assert.Equal(0.0, ((JyroNumber)data.GetProperty("count")).Value);
    }

    [Fact]
    public void GetSchemaErrors_InvalidType_ReturnsErrorArray()
    {
        var script = @"
            var schema = { ""type"": ""number"" }
            Data.errors = GetSchemaErrors(""not a number"", schema)
            Data.count = Length(Data.errors)
        ";
        var result = TestHelpers.ExecuteSuccessfully(script, output: _output);

        var data = (JyroObject)result.Data;
        var count = ((JyroNumber)data.GetProperty("count")).Value;
        Assert.True(count > 0, "Expected at least one error");
    }

    [Fact]
    public void GetSchemaErrors_InvalidType_ErrorHasPathKeywordMessage()
    {
        var script = @"
            var schema = { ""type"": ""number"" }
            Data.errors = GetSchemaErrors(""not a number"", schema)
            var firstError = First(Data.errors)
            Data.hasPath = Exists(firstError.path)
            Data.hasKeyword = Exists(firstError.keyword)
            Data.hasMessage = Exists(firstError.message)
        ";
        var result = TestHelpers.ExecuteSuccessfully(script, output: _output);

        var data = (JyroObject)result.Data;
        Assert.True(((JyroBoolean)data.GetProperty("hasPath")).Value);
        Assert.True(((JyroBoolean)data.GetProperty("hasKeyword")).Value);
        Assert.True(((JyroBoolean)data.GetProperty("hasMessage")).Value);
    }

    [Fact]
    public void GetSchemaErrors_MissingRequired_ReturnsError()
    {
        var script = @"
            var schema = {
                ""type"": ""object"",
                ""required"": [""name""]
            }
            var obj = { ""age"": 30 }
            Data.errors = GetSchemaErrors(obj, schema)
            Data.count = Length(Data.errors)
        ";
        var result = TestHelpers.ExecuteSuccessfully(script, output: _output);

        var data = (JyroObject)result.Data;
        var count = ((JyroNumber)data.GetProperty("count")).Value;
        Assert.True(count > 0, "Expected error for missing required property");
    }

    #endregion

    #region GetSchemaErrors - Multiple Errors

    [Fact]
    public void GetSchemaErrors_MultipleViolations_ReturnsMultipleErrors()
    {
        var script = @"
            var schema = {
                ""type"": ""object"",
                ""properties"": {
                    ""age"": { ""type"": ""number"" },
                    ""name"": { ""type"": ""string"" }
                }
            }
            var obj = { ""age"": ""thirty"", ""name"": 123 }
            Data.errors = GetSchemaErrors(obj, schema)
            Data.count = Length(Data.errors)
        ";
        var result = TestHelpers.ExecuteSuccessfully(script, output: _output);

        var data = (JyroObject)result.Data;
        var count = ((JyroNumber)data.GetProperty("count")).Value;
        Assert.True(count >= 2, "Expected at least two errors for two type violations");
    }

    #endregion

    #region GetSchemaErrors - Nested Errors

    [Fact]
    public void GetSchemaErrors_NestedObjectError_ReturnsErrorWithPath()
    {
        var script = @"
            var schema = {
                ""type"": ""object"",
                ""properties"": {
                    ""user"": {
                        ""type"": ""object"",
                        ""properties"": {
                            ""age"": { ""type"": ""number"" }
                        }
                    }
                }
            }
            var obj = {
                ""user"": {
                    ""age"": ""not a number""
                }
            }
            Data.errors = GetSchemaErrors(obj, schema)
            Data.count = Length(Data.errors)
        ";
        var result = TestHelpers.ExecuteSuccessfully(script, output: _output);

        var data = (JyroObject)result.Data;
        var count = ((JyroNumber)data.GetProperty("count")).Value;
        Assert.True(count > 0, "Expected error for nested property type violation");
    }

    #endregion

    #region GetSchemaErrors - Array Item Errors

    [Fact]
    public void GetSchemaErrors_ArrayItemViolation_ReturnsError()
    {
        var script = @"
            var schema = {
                ""type"": ""array"",
                ""items"": { ""type"": ""number"" }
            }
            var arr = [1, 2, ""three"", 4]
            Data.errors = GetSchemaErrors(arr, schema)
            Data.count = Length(Data.errors)
        ";
        var result = TestHelpers.ExecuteSuccessfully(script, output: _output);

        var data = (JyroObject)result.Data;
        var count = ((JyroNumber)data.GetProperty("count")).Value;
        Assert.True(count > 0, "Expected error for invalid array item type");
    }

    #endregion

    #region GetSchemaErrors - Numeric Constraint Errors

    [Fact]
    public void GetSchemaErrors_MinimumViolation_ReturnsError()
    {
        var script = @"
            var schema = { ""type"": ""number"", ""minimum"": 0 }
            Data.errors = GetSchemaErrors(-5, schema)
            Data.count = Length(Data.errors)
        ";
        var result = TestHelpers.ExecuteSuccessfully(script, output: _output);

        var data = (JyroObject)result.Data;
        var count = ((JyroNumber)data.GetProperty("count")).Value;
        Assert.True(count > 0, "Expected error for minimum constraint violation");
    }

    [Fact]
    public void GetSchemaErrors_MaximumViolation_ReturnsError()
    {
        var script = @"
            var schema = { ""type"": ""number"", ""maximum"": 100 }
            Data.errors = GetSchemaErrors(150, schema)
            Data.count = Length(Data.errors)
        ";
        var result = TestHelpers.ExecuteSuccessfully(script, output: _output);

        var data = (JyroObject)result.Data;
        var count = ((JyroNumber)data.GetProperty("count")).Value;
        Assert.True(count > 0, "Expected error for maximum constraint violation");
    }

    #endregion

    #region GetSchemaErrors - Enum Errors

    [Fact]
    public void GetSchemaErrors_EnumViolation_ReturnsError()
    {
        var script = @"
            var schema = {
                ""type"": ""string"",
                ""enum"": [""active"", ""inactive""]
            }
            Data.errors = GetSchemaErrors(""unknown"", schema)
            Data.count = Length(Data.errors)
        ";
        var result = TestHelpers.ExecuteSuccessfully(script, output: _output);

        var data = (JyroObject)result.Data;
        var count = ((JyroNumber)data.GetProperty("count")).Value;
        Assert.True(count > 0, "Expected error for enum constraint violation");
    }

    #endregion

    #region Edge Cases

    [Fact]
    public void ValidateSchema_EmptyObject_ValidatesAsObject()
    {
        var script = @"
            var schema = { ""type"": ""object"" }
            var obj = {}
            Data.result = ValidateSchema(obj, schema)
        ";
        var result = TestHelpers.ExecuteSuccessfully(script, output: _output);

        var data = (JyroObject)result.Data;
        Assert.True(((JyroBoolean)data.GetProperty("result")).Value);
    }

    [Fact]
    public void ValidateSchema_EmptyArray_ValidatesAsArray()
    {
        var script = @"
            var schema = { ""type"": ""array"" }
            var arr = []
            Data.result = ValidateSchema(arr, schema)
        ";
        var result = TestHelpers.ExecuteSuccessfully(script, output: _output);

        var data = (JyroObject)result.Data;
        Assert.True(((JyroBoolean)data.GetProperty("result")).Value);
    }

    [Fact]
    public void ValidateSchema_EmptySchema_AcceptsAnyValue()
    {
        var script = @"
            var schema = {}
            Data.result1 = ValidateSchema(""string"", schema)
            Data.result2 = ValidateSchema(123, schema)
            Data.result3 = ValidateSchema(true, schema)
            Data.result4 = ValidateSchema(null, schema)
        ";
        var result = TestHelpers.ExecuteSuccessfully(script, output: _output);

        var data = (JyroObject)result.Data;
        Assert.True(((JyroBoolean)data.GetProperty("result1")).Value);
        Assert.True(((JyroBoolean)data.GetProperty("result2")).Value);
        Assert.True(((JyroBoolean)data.GetProperty("result3")).Value);
        Assert.True(((JyroBoolean)data.GetProperty("result4")).Value);
    }

    #endregion
}
