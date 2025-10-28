using Xunit.Abstractions;

namespace Mesch.Jyro.Tests;

public class StandardLibraryTests
{
    private readonly ITestOutputHelper _output;

    public StandardLibraryTests(ITestOutputHelper output)
    {
        _output = output;
    }

    #region String Functions

    [Fact]
    public void Upper_ConvertsToUpperCase()
    {
        var script = "Data.result = Upper(\"hello\")";
        var result = TestHelpers.ExecuteSuccessfully(script, output: _output);

        var data = (JyroObject)result.Data;
        Assert.Equal("HELLO", ((JyroString)data.GetProperty("result")).Value);
    }

    [Fact]
    public void Lower_ConvertsToLowerCase()
    {
        var script = "Data.result = Lower(\"WORLD\")";
        var result = TestHelpers.ExecuteSuccessfully(script, output: _output);

        var data = (JyroObject)result.Data;
        Assert.Equal("world", ((JyroString)data.GetProperty("result")).Value);
    }

    [Fact]
    public void Trim_RemovesWhitespace()
    {
        var script = "Data.result = Trim(\"  hello  \")";
        var result = TestHelpers.ExecuteSuccessfully(script, output: _output);

        var data = (JyroObject)result.Data;
        Assert.Equal("hello", ((JyroString)data.GetProperty("result")).Value);
    }

    [Fact]
    public void Replace_ReplacesSubstring()
    {
        var script = "Data.result = Replace(\"hello world\", \"world\", \"Jyro\")";
        var result = TestHelpers.ExecuteSuccessfully(script, output: _output);

        var data = (JyroObject)result.Data;
        Assert.Equal("hello Jyro", ((JyroString)data.GetProperty("result")).Value);
    }

    [Fact]
    public void Contains_FindsSubstring()
    {
        var script = "Data.result = Contains(\"hello world\", \"world\")";
        var result = TestHelpers.ExecuteSuccessfully(script, output: _output);

        var data = (JyroObject)result.Data;
        Assert.True(((JyroBoolean)data.GetProperty("result")).Value);
    }

    [Fact]
    public void StartsWith_ChecksPrefix()
    {
        var script = "Data.result = StartsWith(\"hello world\", \"hello\")";
        var result = TestHelpers.ExecuteSuccessfully(script, output: _output);

        var data = (JyroObject)result.Data;
        Assert.True(((JyroBoolean)data.GetProperty("result")).Value);
    }

    [Fact]
    public void EndsWith_ChecksSuffix()
    {
        var script = "Data.result = EndsWith(\"hello world\", \"world\")";
        var result = TestHelpers.ExecuteSuccessfully(script, output: _output);

        var data = (JyroObject)result.Data;
        Assert.True(((JyroBoolean)data.GetProperty("result")).Value);
    }

    [Fact]
    public void Split_SplitsString()
    {
        var script = "Data.result = Split(\"a,b,c\", \",\")";
        var result = TestHelpers.ExecuteSuccessfully(script, output: _output);

        var data = (JyroObject)result.Data;
        var arr = (JyroArray)data.GetProperty("result");
        Assert.Equal(3, arr.Length);
        Assert.Equal("a", ((JyroString)arr[0]).Value);
        Assert.Equal("b", ((JyroString)arr[1]).Value);
        Assert.Equal("c", ((JyroString)arr[2]).Value);
    }

    [Fact]
    public void Join_JoinsArray()
    {
        var script = @"
            var arr = [""a"", ""b"", ""c""]
            Data.result = Join(arr, ""-"")
        ";
        var result = TestHelpers.ExecuteSuccessfully(script, output: _output);

        var data = (JyroObject)result.Data;
        Assert.Equal("a-b-c", ((JyroString)data.GetProperty("result")).Value);
    }

    #endregion

    #region Array Functions

    [Fact]
    public void Length_ReturnsArrayLength()
    {
        var script = @"
            var arr = [1, 2, 3, 4, 5]
            Data.result = Length(arr)
        ";
        var result = TestHelpers.ExecuteSuccessfully(script, output: _output);

        var data = (JyroObject)result.Data;
        Assert.Equal(5.0, ((JyroNumber)data.GetProperty("result")).Value);
    }

    [Fact]
    public void Append_AddsElement()
    {
        var script = @"
            var arr = [1, 2, 3]
            Append(arr, 4)
            Data.result = Length(arr)
        ";
        var result = TestHelpers.ExecuteSuccessfully(script, output: _output);

        var data = (JyroObject)result.Data;
        Assert.Equal(4.0, ((JyroNumber)data.GetProperty("result")).Value);
    }

    [Fact]
    public void RemoveLast_RemovesLastElement()
    {
        var script = @"
            var arr = [1, 2, 3]
            RemoveLast(arr)
            Data.result = Length(arr)
        ";
        var result = TestHelpers.ExecuteSuccessfully(script, output: _output);

        var data = (JyroObject)result.Data;
        Assert.Equal(2.0, ((JyroNumber)data.GetProperty("result")).Value);
    }

    [Fact]
    public void RemoveAt_RemovesElementAtIndex()
    {
        var script = @"
            var arr = [10, 20, 30]
            RemoveAt(arr, 1)
            Data.result = arr
        ";
        var result = TestHelpers.ExecuteSuccessfully(script, output: _output);

        var data = (JyroObject)result.Data;
        var arr = (JyroArray)data.GetProperty("result");
        Assert.Equal(2, arr.Length);
        Assert.Equal(10.0, ((JyroNumber)arr[0]).Value);
        Assert.Equal(30.0, ((JyroNumber)arr[1]).Value);
    }

    [Fact]
    public void Insert_InsertsElementAtIndex()
    {
        var script = @"
            var arr = [1, 3]
            Insert(arr, 1, 2)
            Data.result = arr
        ";
        var result = TestHelpers.ExecuteSuccessfully(script, output: _output);

        var data = (JyroObject)result.Data;
        var arr = (JyroArray)data.GetProperty("result");
        Assert.Equal(3, arr.Length);
        Assert.Equal(1.0, ((JyroNumber)arr[0]).Value);
        Assert.Equal(2.0, ((JyroNumber)arr[1]).Value);
        Assert.Equal(3.0, ((JyroNumber)arr[2]).Value);
    }

    [Fact]
    public void Clear_EmptiesArray()
    {
        var script = @"
            var arr = [1, 2, 3]
            Clear(arr)
            Data.result = Length(arr)
        ";
        var result = TestHelpers.ExecuteSuccessfully(script, output: _output);

        var data = (JyroObject)result.Data;
        Assert.Equal(0.0, ((JyroNumber)data.GetProperty("result")).Value);
    }

    [Fact]
    public void Reverse_ReversesArray()
    {
        var script = @"
            var arr = [1, 2, 3]
            Data.result = Reverse(arr)
        ";
        var result = TestHelpers.ExecuteSuccessfully(script, output: _output);

        var data = (JyroObject)result.Data;
        var arr = (JyroArray)data.GetProperty("result");
        Assert.Equal(3.0, ((JyroNumber)arr[0]).Value);
        Assert.Equal(2.0, ((JyroNumber)arr[1]).Value);
        Assert.Equal(1.0, ((JyroNumber)arr[2]).Value);
    }

    [Fact]
    public void Sort_SortsNumbersAscending()
    {
        // Sort only accepts one parameter (array) and always sorts ascending
        var script = @"
            var arr = [3, 1, 2]
            Data.result = Sort(arr)
        ";
        var result = TestHelpers.ExecuteSuccessfully(script, output: _output);

        var data = (JyroObject)result.Data;
        var arr = (JyroArray)data.GetProperty("result");
        Assert.Equal(1.0, ((JyroNumber)arr[0]).Value);
        Assert.Equal(2.0, ((JyroNumber)arr[1]).Value);
        Assert.Equal(3.0, ((JyroNumber)arr[2]).Value);
    }

    [Fact]
    public void SortByField_SortsObjectsByField()
    {
        // SortByField returns a new sorted array, doesn't sort in-place
        var script = @"
            var arr = [
                {""name"": ""Charlie"", ""age"": 30},
                {""name"": ""Alice"", ""age"": 25},
                {""name"": ""Bob"", ""age"": 35}
            ]
            arr = SortByField(arr, ""age"", ""asc"")
            Data.result = arr[0].name
        ";
        var result = TestHelpers.ExecuteSuccessfully(script, output: _output);

        var data = (JyroObject)result.Data;
        Assert.Equal("Alice", ((JyroString)data.GetProperty("result")).Value);
    }

    [Fact]
    public void MergeArrays_CombinesArrays()
    {
        var script = @"
            var arr1 = [1, 2]
            var arr2 = [3, 4]
            Data.result = MergeArrays(arr1, arr2)
        ";
        var result = TestHelpers.ExecuteSuccessfully(script, output: _output);

        var data = (JyroObject)result.Data;
        var arr = (JyroArray)data.GetProperty("result");
        Assert.Equal(4, arr.Length);
        Assert.Equal(1.0, ((JyroNumber)arr[0]).Value);
        Assert.Equal(4.0, ((JyroNumber)arr[3]).Value);
    }

    [Fact]
    public void IndexOf_FindsElementInArray()
    {
        var script = @"
            var arr = [1, 2, 3, 4, 5]
            Data.result = IndexOf(arr, 3)
        ";
        var result = TestHelpers.ExecuteSuccessfully(script, output: _output);

        var data = (JyroObject)result.Data;
        Assert.Equal(2.0, ((JyroNumber)data.GetProperty("result")).Value);
    }

    [Fact]
    public void IndexOf_ReturnsNegativeOneWhenNotFound()
    {
        var script = @"
            var arr = [1, 2, 3]
            Data.result = IndexOf(arr, 10)
        ";
        var result = TestHelpers.ExecuteSuccessfully(script, output: _output);

        var data = (JyroObject)result.Data;
        Assert.Equal(-1.0, ((JyroNumber)data.GetProperty("result")).Value);
    }

    [Fact]
    public void IndexOf_FindsFirstElement()
    {
        var script = @"
            var arr = [""apple"", ""banana"", ""orange""]
            Data.result = IndexOf(arr, ""apple"")
        ";
        var result = TestHelpers.ExecuteSuccessfully(script, output: _output);

        var data = (JyroObject)result.Data;
        Assert.Equal(0.0, ((JyroNumber)data.GetProperty("result")).Value);
    }

    [Fact]
    public void IndexOf_FindsLastElement()
    {
        var script = @"
            var arr = [""red"", ""green"", ""blue""]
            Data.result = IndexOf(arr, ""blue"")
        ";
        var result = TestHelpers.ExecuteSuccessfully(script, output: _output);

        var data = (JyroObject)result.Data;
        Assert.Equal(2.0, ((JyroNumber)data.GetProperty("result")).Value);
    }

    [Fact]
    public void IndexOf_FindsObjectByDeepEquality()
    {
        var script = @"
            var orders = [
                { ""id"": ""ORD-1001"", ""total"": 100 },
                { ""id"": ""ORD-1002"", ""total"": 200 },
                { ""id"": ""ORD-1003"", ""total"": 150 }
            ]
            Data.result = IndexOf(orders, { ""id"": ""ORD-1002"", ""total"": 200 })
        ";
        var result = TestHelpers.ExecuteSuccessfully(script, output: _output);

        var data = (JyroObject)result.Data;
        Assert.Equal(1.0, ((JyroNumber)data.GetProperty("result")).Value);
    }

    [Fact]
    public void IndexOf_ReturnsNegativeOneForNonMatchingObject()
    {
        var script = @"
            var orders = [
                { ""id"": ""ORD-1001"", ""total"": 100 },
                { ""id"": ""ORD-1002"", ""total"": 200 }
            ]
            Data.result = IndexOf(orders, { ""id"": ""ORD-9999"", ""total"": 999 })
        ";
        var result = TestHelpers.ExecuteSuccessfully(script, output: _output);

        var data = (JyroObject)result.Data;
        Assert.Equal(-1.0, ((JyroNumber)data.GetProperty("result")).Value);
    }

    [Fact]
    public void IndexOf_ReturnsFirstOccurrenceOfDuplicate()
    {
        var script = @"
            var arr = [1, 2, 3, 2, 4]
            Data.result = IndexOf(arr, 2)
        ";
        var result = TestHelpers.ExecuteSuccessfully(script, output: _output);

        var data = (JyroObject)result.Data;
        Assert.Equal(1.0, ((JyroNumber)data.GetProperty("result")).Value);
    }

    [Fact]
    public void IndexOf_WorksWithEmptyArray()
    {
        var script = @"
            var arr = array []
            Data.result = IndexOf(arr, 1)
        ";
        var result = TestHelpers.ExecuteSuccessfully(script, output: _output);

        var data = (JyroObject)result.Data;
        Assert.Equal(-1.0, ((JyroNumber)data.GetProperty("result")).Value);
    }

    [Fact]
    public void IndexOf_FindsNestedArray()
    {
        var script = @"
            var matrix = [[1, 2], [3, 4], [5, 6]]
            Data.result = IndexOf(matrix, [3, 4])
        ";
        var result = TestHelpers.ExecuteSuccessfully(script, output: _output);

        var data = (JyroObject)result.Data;
        Assert.Equal(1.0, ((JyroNumber)data.GetProperty("result")).Value);
    }

    [Fact]
    public void IndexOf_UsedWithRemoveAt()
    {
        var script = @"
            var items = [
                { ""id"": 1, ""name"": ""Item A"" },
                { ""id"": 2, ""name"": ""Item B"" },
                { ""id"": 3, ""name"": ""Item C"" }
            ]
            var idx = IndexOf(items, { ""id"": 2, ""name"": ""Item B"" })
            if idx >= 0 then
                RemoveAt(items, idx)
            end
            Data.result = Length(items)
        ";
        var result = TestHelpers.ExecuteSuccessfully(script, output: _output);

        var data = (JyroObject)result.Data;
        Assert.Equal(2.0, ((JyroNumber)data.GetProperty("result")).Value);
    }

    #endregion

    #region Math Functions

    [Fact]
    public void Abs_ReturnsAbsoluteValue()
    {
        var script = "Data.result = Abs(-42)";
        var result = TestHelpers.ExecuteSuccessfully(script, output: _output);

        var data = (JyroObject)result.Data;
        Assert.Equal(42.0, ((JyroNumber)data.GetProperty("result")).Value);
    }

    [Fact]
    public void Min_ReturnsMinimum()
    {
        var script = "Data.result = Min(5, 3)";
        var result = TestHelpers.ExecuteSuccessfully(script, output: _output);

        var data = (JyroObject)result.Data;
        Assert.Equal(3.0, ((JyroNumber)data.GetProperty("result")).Value);
    }

    [Fact]
    public void Max_ReturnsMaximum()
    {
        var script = "Data.result = Max(5, 3)";
        var result = TestHelpers.ExecuteSuccessfully(script, output: _output);

        var data = (JyroObject)result.Data;
        Assert.Equal(5.0, ((JyroNumber)data.GetProperty("result")).Value);
    }

    [Fact]
    public void Round_RoundsNumber()
    {
        var script = "Data.result = Round(3.7, 0)";
        var result = TestHelpers.ExecuteSuccessfully(script, output: _output);

        var data = (JyroObject)result.Data;
        Assert.Equal(4.0, ((JyroNumber)data.GetProperty("result")).Value);
    }

    [Fact]
    public void Sum_SumsNumbers()
    {
        // Sum is a variadic function that takes individual numbers, not an array
        var script = "Data.result = Sum(1, 2, 3, 4)";
        var result = TestHelpers.ExecuteSuccessfully(script, output: _output);

        var data = (JyroObject)result.Data;
        Assert.Equal(10.0, ((JyroNumber)data.GetProperty("result")).Value);
    }

    #endregion

    #region Utility Functions

    [Fact]
    public void Equal_ComparesValues()
    {
        var script = "Data.result = Equal(5, 5)";
        var result = TestHelpers.ExecuteSuccessfully(script, output: _output);

        var data = (JyroObject)result.Data;
        Assert.True(((JyroBoolean)data.GetProperty("result")).Value);
    }

    [Fact]
    public void NotEqual_ComparesValues()
    {
        var script = "Data.result = NotEqual(5, 3)";
        var result = TestHelpers.ExecuteSuccessfully(script, output: _output);

        var data = (JyroObject)result.Data;
        Assert.True(((JyroBoolean)data.GetProperty("result")).Value);
    }

    [Fact]
    public void TypeOf_ReturnsType()
    {
        var script = "Data.result = TypeOf(42)";
        var result = TestHelpers.ExecuteSuccessfully(script, output: _output);

        var data = (JyroObject)result.Data;
        Assert.Equal("number", ((JyroString)data.GetProperty("result")).Value);
    }

    [Fact]
    public void Exists_ChecksForProperty()
    {
        var script = @"
            var obj = {""name"": ""Alice""}
            Data.result = Exists(obj, ""name"")
        ";
        var result = TestHelpers.ExecuteSuccessfully(script, output: _output);

        var data = (JyroObject)result.Data;
        Assert.True(((JyroBoolean)data.GetProperty("result")).Value);
    }

    [Fact]
    public void IsNull_ChecksNull()
    {
        var script = "Data.result = IsNull(null)";
        var result = TestHelpers.ExecuteSuccessfully(script, output: _output);

        var data = (JyroObject)result.Data;
        Assert.True(((JyroBoolean)data.GetProperty("result")).Value);
    }

    [Fact]
    public void NewGuid_GeneratesGuid()
    {
        var script = "Data.result = NewGuid()";
        var result = TestHelpers.ExecuteSuccessfully(script, output: _output);

        var data = (JyroObject)result.Data;
        var guid = ((JyroString)data.GetProperty("result")).Value;
        Assert.NotEmpty(guid);
        Assert.Equal(36, guid.Length); // Standard GUID format with hyphens
    }

    #endregion

    #region Date/Time Functions

    [Fact]
    public void Now_ReturnsCurrentDateTime()
    {
        var script = "Data.result = Now()";
        var result = TestHelpers.ExecuteSuccessfully(script, output: _output);

        var data = (JyroObject)result.Data;
        var now = ((JyroString)data.GetProperty("result")).Value;
        Assert.NotEmpty(now);
    }

    [Fact]
    public void Today_ReturnsCurrentDate()
    {
        var script = "Data.result = Today()";
        var result = TestHelpers.ExecuteSuccessfully(script, output: _output);

        var data = (JyroObject)result.Data;
        var today = ((JyroString)data.GetProperty("result")).Value;
        Assert.NotEmpty(today);
    }

    [Fact]
    public void FormatDate_FormatsDate()
    {
        var script = @"Data.result = FormatDate(""2024-01-15"", ""yyyy-MM-dd"")";
        var result = TestHelpers.ExecuteSuccessfully(script, output: _output);

        var data = (JyroObject)result.Data;
        Assert.Equal("2024-01-15", ((JyroString)data.GetProperty("result")).Value);
    }

    #endregion
}
