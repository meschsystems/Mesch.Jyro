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
            var arr = []
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

    [Fact]
    public void Take_ReturnsFirstNElements()
    {
        var script = @"
            var arr = [1, 2, 3, 4, 5]
            Data.result = Take(arr, 3)
        ";
        var result = TestHelpers.ExecuteSuccessfully(script, output: _output);

        var data = (JyroObject)result.Data;
        var resultArray = (JyroArray)data.GetProperty("result");
        Assert.Equal(3, resultArray.Length);
        Assert.Equal(1.0, ((JyroNumber)resultArray[0]).Value);
        Assert.Equal(2.0, ((JyroNumber)resultArray[1]).Value);
        Assert.Equal(3.0, ((JyroNumber)resultArray[2]).Value);
    }

    [Fact]
    public void Take_DoesNotModifyOriginalArray()
    {
        var script = @"
            var arr = [1, 2, 3, 4, 5]
            var taken = Take(arr, 2)
            Data.originalLength = Length(arr)
            Data.takenLength = Length(taken)
        ";
        var result = TestHelpers.ExecuteSuccessfully(script, output: _output);

        var data = (JyroObject)result.Data;
        Assert.Equal(5.0, ((JyroNumber)data.GetProperty("originalLength")).Value);
        Assert.Equal(2.0, ((JyroNumber)data.GetProperty("takenLength")).Value);
    }

    [Fact]
    public void Take_CountGreaterThanLength_ReturnsAllElements()
    {
        var script = @"
            var arr = [1, 2, 3]
            Data.result = Take(arr, 10)
        ";
        var result = TestHelpers.ExecuteSuccessfully(script, output: _output);

        var data = (JyroObject)result.Data;
        var resultArray = (JyroArray)data.GetProperty("result");
        Assert.Equal(3, resultArray.Length);
        Assert.Equal(1.0, ((JyroNumber)resultArray[0]).Value);
        Assert.Equal(2.0, ((JyroNumber)resultArray[1]).Value);
        Assert.Equal(3.0, ((JyroNumber)resultArray[2]).Value);
    }

    [Fact]
    public void Take_ZeroCount_ReturnsEmptyArray()
    {
        var script = @"
            var arr = [1, 2, 3]
            Data.result = Take(arr, 0)
        ";
        var result = TestHelpers.ExecuteSuccessfully(script, output: _output);

        var data = (JyroObject)result.Data;
        var resultArray = (JyroArray)data.GetProperty("result");
        Assert.Equal(0, resultArray.Length);
    }

    [Fact]
    public void Take_NegativeCount_ReturnsEmptyArray()
    {
        var script = @"
            var arr = [1, 2, 3]
            Data.result = Take(arr, -1)
        ";
        var result = TestHelpers.ExecuteSuccessfully(script, output: _output);

        var data = (JyroObject)result.Data;
        var resultArray = (JyroArray)data.GetProperty("result");
        Assert.Equal(0, resultArray.Length);
    }

    [Fact]
    public void Take_EmptyArray_ReturnsEmptyArray()
    {
        var script = @"
            var arr = []
            Data.result = Take(arr, 5)
        ";
        var result = TestHelpers.ExecuteSuccessfully(script, output: _output);

        var data = (JyroObject)result.Data;
        var resultArray = (JyroArray)data.GetProperty("result");
        Assert.Equal(0, resultArray.Length);
    }

    [Fact]
    public void Take_WorksWithObjects()
    {
        var script = @"
            var people = [
                { ""name"": ""Alice"", ""age"": 30 },
                { ""name"": ""Bob"", ""age"": 25 },
                { ""name"": ""Charlie"", ""age"": 35 }
            ]
            Data.result = Take(people, 2)
        ";
        var result = TestHelpers.ExecuteSuccessfully(script, output: _output);

        var data = (JyroObject)result.Data;
        var resultArray = (JyroArray)data.GetProperty("result");
        Assert.Equal(2, resultArray.Length);

        var firstPerson = (JyroObject)resultArray[0];
        Assert.Equal("Alice", ((JyroString)firstPerson.GetProperty("name")).Value);

        var secondPerson = (JyroObject)resultArray[1];
        Assert.Equal("Bob", ((JyroString)secondPerson.GetProperty("name")).Value);
    }

    [Fact]
    public void Take_TakeOne_ReturnsArrayNotSingleElement()
    {
        var script = @"
            var arr = [42, 43, 44]
            Data.result = Take(arr, 1)
        ";
        var result = TestHelpers.ExecuteSuccessfully(script, output: _output);

        var data = (JyroObject)result.Data;
        var resultArray = (JyroArray)data.GetProperty("result");
        Assert.Equal(1, resultArray.Length);
        Assert.Equal(42.0, ((JyroNumber)resultArray[0]).Value);
    }

    [Fact]
    public void GroupBy_GroupsObjectsByField()
    {
        var script = @"
            var orders = [
                {""id"": 1, ""status"": ""pending"", ""amount"": 100},
                {""id"": 2, ""status"": ""completed"", ""amount"": 200},
                {""id"": 3, ""status"": ""pending"", ""amount"": 150},
                {""id"": 4, ""status"": ""completed"", ""amount"": 50}
            ]
            Data.result = GroupBy(orders, ""status"")
        ";
        var result = TestHelpers.ExecuteSuccessfully(script, output: _output);

        var data = (JyroObject)result.Data;
        var grouped = (JyroObject)data.GetProperty("result");

        var pendingGroup = (JyroArray)grouped.GetProperty("pending");
        var completedGroup = (JyroArray)grouped.GetProperty("completed");

        Assert.Equal(2, pendingGroup.Length);
        Assert.Equal(2, completedGroup.Length);

        // Verify first pending item
        var firstPending = (JyroObject)pendingGroup[0];
        Assert.Equal(1.0, ((JyroNumber)firstPending.GetProperty("id")).Value);

        // Verify first completed item
        var firstCompleted = (JyroObject)completedGroup[0];
        Assert.Equal(2.0, ((JyroNumber)firstCompleted.GetProperty("id")).Value);
    }

    [Fact]
    public void GroupBy_GroupsNullValuesUnderNullKey()
    {
        var script = @"
            var items = [
                {""id"": 1, ""category"": ""A""},
                {""id"": 2, ""category"": null},
                {""id"": 3},
                {""id"": 4, ""category"": ""A""}
            ]
            Data.result = GroupBy(items, ""category"")
        ";
        var result = TestHelpers.ExecuteSuccessfully(script, output: _output);

        var data = (JyroObject)result.Data;
        var grouped = (JyroObject)data.GetProperty("result");

        var aGroup = (JyroArray)grouped.GetProperty("A");
        var nullGroup = (JyroArray)grouped.GetProperty("null");

        Assert.Equal(2, aGroup.Length);
        Assert.Equal(2, nullGroup.Length);

        // Verify null group contains items with id 2 and 3
        var firstNull = (JyroObject)nullGroup[0];
        var secondNull = (JyroObject)nullGroup[1];
        Assert.Equal(2.0, ((JyroNumber)firstNull.GetProperty("id")).Value);
        Assert.Equal(3.0, ((JyroNumber)secondNull.GetProperty("id")).Value);
    }

    [Fact]
    public void GroupBy_SkipsNonObjectItems()
    {
        var script = @"
            var items = [
                {""id"": 1, ""type"": ""A""},
                ""not an object"",
                42,
                {""id"": 2, ""type"": ""A""},
                null
            ]
            Data.result = GroupBy(items, ""type"")
        ";
        var result = TestHelpers.ExecuteSuccessfully(script, output: _output);

        var data = (JyroObject)result.Data;
        var grouped = (JyroObject)data.GetProperty("result");

        var aGroup = (JyroArray)grouped.GetProperty("A");
        Assert.Equal(2, aGroup.Length);
    }

    [Fact]
    public void GroupBy_SupportsNestedFieldPaths()
    {
        var script = @"
            var people = [
                {""name"": ""Alice"", ""address"": {""city"": ""NYC""}},
                {""name"": ""Bob"", ""address"": {""city"": ""LA""}},
                {""name"": ""Charlie"", ""address"": {""city"": ""NYC""}}
            ]
            Data.result = GroupBy(people, ""address.city"")
        ";
        var result = TestHelpers.ExecuteSuccessfully(script, output: _output);

        var data = (JyroObject)result.Data;
        var grouped = (JyroObject)data.GetProperty("result");

        var nycGroup = (JyroArray)grouped.GetProperty("NYC");
        var laGroup = (JyroArray)grouped.GetProperty("LA");

        Assert.Equal(2, nycGroup.Length);
        Assert.Equal(1, laGroup.Length);

        var firstNyc = (JyroObject)nycGroup[0];
        Assert.Equal("Alice", ((JyroString)firstNyc.GetProperty("name")).Value);
    }

    [Fact]
    public void GroupBy_DoesNotModifyOriginalArray()
    {
        var script = @"
            var arr = [
                {""id"": 1, ""type"": ""A""},
                {""id"": 2, ""type"": ""B""}
            ]
            var grouped = GroupBy(arr, ""type"")
            Data.originalLength = Length(arr)
        ";
        var result = TestHelpers.ExecuteSuccessfully(script, output: _output);

        var data = (JyroObject)result.Data;
        Assert.Equal(2.0, ((JyroNumber)data.GetProperty("originalLength")).Value);
    }

    [Fact]
    public void GroupBy_EmptyArray_ReturnsEmptyObject()
    {
        var script = @"
            var arr = []
            Data.result = GroupBy(arr, ""type"")
        ";
        var result = TestHelpers.ExecuteSuccessfully(script, output: _output);

        var data = (JyroObject)result.Data;
        var grouped = (JyroObject)data.GetProperty("result");
        Assert.Equal(0, grouped.Count);
    }

    [Fact]
    public void GroupBy_GroupsByNumericField()
    {
        var script = @"
            var items = [
                {""name"": ""A"", ""priority"": 1},
                {""name"": ""B"", ""priority"": 2},
                {""name"": ""C"", ""priority"": 1}
            ]
            Data.result = GroupBy(items, ""priority"")
        ";
        var result = TestHelpers.ExecuteSuccessfully(script, output: _output);

        var data = (JyroObject)result.Data;
        var grouped = (JyroObject)data.GetProperty("result");

        // Numbers are converted to string keys
        var priority1 = (JyroArray)grouped.GetProperty("1");
        var priority2 = (JyroArray)grouped.GetProperty("2");

        Assert.Equal(2, priority1.Length);
        Assert.Equal(1, priority2.Length);
    }

    [Fact]
    public void GroupBy_GroupsByBooleanField()
    {
        var script = @"
            var tasks = [
                {""name"": ""Task 1"", ""done"": true},
                {""name"": ""Task 2"", ""done"": false},
                {""name"": ""Task 3"", ""done"": true}
            ]
            Data.result = GroupBy(tasks, ""done"")
        ";
        var result = TestHelpers.ExecuteSuccessfully(script, output: _output);

        var data = (JyroObject)result.Data;
        var grouped = (JyroObject)data.GetProperty("result");

        // Booleans are converted to string keys "True" and "False" (C# casing)
        var doneGroup = (JyroArray)grouped.GetProperty("True");
        var notDoneGroup = (JyroArray)grouped.GetProperty("False");

        Assert.Equal(2, doneGroup.Length);
        Assert.Equal(1, notDoneGroup.Length);
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
    public void Exists_ReturnsFalseForNonexistentProperty()
    {
        var script = @"
            var obj = {""name"": ""Alice""}
            Data.result = Exists(obj, ""age"")
        ";
        var result = TestHelpers.ExecuteSuccessfully(script, output: _output);

        var data = (JyroObject)result.Data;
        Assert.False(((JyroBoolean)data.GetProperty("result")).Value);
    }

    [Fact]
    public void Exists_ReturnsFalseForNullProperty()
    {
        var script = @"
            var obj = {""name"": null}
            Data.result = Exists(obj, ""name"")
        ";
        var result = TestHelpers.ExecuteSuccessfully(script, output: _output);

        var data = (JyroObject)result.Data;
        Assert.False(((JyroBoolean)data.GetProperty("result")).Value);
    }

    [Fact]
    public void Exists_ReturnsFalseWhenFirstArgNotObject()
    {
        var script = @"
            Data.result = Exists(""not an object"", ""property"")
        ";
        var result = TestHelpers.ExecuteSuccessfully(script, output: _output);

        var data = (JyroObject)result.Data;
        Assert.False(((JyroBoolean)data.GetProperty("result")).Value);
    }

    [Fact]
    public void Exists_WorksWithNestedProperties()
    {
        var script = @"
            var obj = {
                ""person"": {
                    ""name"": ""Bob"",
                    ""age"": 30
                }
            }
            Data.hasPerson = Exists(obj, ""person"")
            Data.hasName = Exists(obj.person, ""name"")
            Data.hasAddress = Exists(obj.person, ""address"")
        ";
        var result = TestHelpers.ExecuteSuccessfully(script, output: _output);

        var data = (JyroObject)result.Data;
        Assert.True(((JyroBoolean)data.GetProperty("hasPerson")).Value);
        Assert.True(((JyroBoolean)data.GetProperty("hasName")).Value);
        Assert.False(((JyroBoolean)data.GetProperty("hasAddress")).Value);
    }

    [Fact]
    public void Exists_WorksInConditionals()
    {
        var script = @"
            var obj = {""id"": 123}
            if Exists(obj, ""id"") then
                Data.result = ""found""
            else
                Data.result = ""not found""
            end
        ";
        var result = TestHelpers.ExecuteSuccessfully(script, output: _output);

        var data = (JyroObject)result.Data;
        Assert.Equal("found", ((JyroString)data.GetProperty("result")).Value);
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
    public void Keys_ReturnsPropertyNames()
    {
        var script = @"
            var obj = {""name"": ""Alice"", ""age"": 30, ""active"": true}
            Data.result = Keys(obj)
        ";
        var result = TestHelpers.ExecuteSuccessfully(script, output: _output);

        var data = (JyroObject)result.Data;
        var keys = (JyroArray)data.GetProperty("result");

        Assert.Equal(3, keys.Length);

        // Convert to list for easier assertion (order may vary)
        var keyList = new List<string>();
        for (int i = 0; i < keys.Length; i++)
        {
            keyList.Add(((JyroString)keys[i]).Value);
        }

        Assert.Contains("name", keyList);
        Assert.Contains("age", keyList);
        Assert.Contains("active", keyList);
    }

    [Fact]
    public void Keys_EmptyObject_ReturnsEmptyArray()
    {
        var script = @"
            var obj = {}
            Data.result = Keys(obj)
        ";
        var result = TestHelpers.ExecuteSuccessfully(script, output: _output);

        var data = (JyroObject)result.Data;
        var keys = (JyroArray)data.GetProperty("result");

        Assert.Equal(0, keys.Length);
    }

    [Fact]
    public void Keys_UsedWithGroupBy()
    {
        var script = @"
            var orders = [
                {""id"": 1, ""status"": ""pending""},
                {""id"": 2, ""status"": ""completed""},
                {""id"": 3, ""status"": ""pending""}
            ]
            var grouped = GroupBy(orders, ""status"")
            Data.result = Keys(grouped)
        ";
        var result = TestHelpers.ExecuteSuccessfully(script, output: _output);

        var data = (JyroObject)result.Data;
        var keys = (JyroArray)data.GetProperty("result");

        Assert.Equal(2, keys.Length);

        var keyList = new List<string>();
        for (int i = 0; i < keys.Length; i++)
        {
            keyList.Add(((JyroString)keys[i]).Value);
        }

        Assert.Contains("pending", keyList);
        Assert.Contains("completed", keyList);
    }

    [Fact]
    public void Keys_IterateOverObject()
    {
        var script = @"
            var obj = {""a"": 1, ""b"": 2, ""c"": 3}
            var sum = 0
            foreach key in Keys(obj) do
                sum = sum + obj[key]
            end
            Data.result = sum
        ";
        var result = TestHelpers.ExecuteSuccessfully(script, output: _output);

        var data = (JyroObject)result.Data;
        Assert.Equal(6.0, ((JyroNumber)data.GetProperty("result")).Value);
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

    #region Random Functions

    [Fact]
    public void RandomInt_SingleArgument_ReturnsValueInRange()
    {
        var script = @"
            Data.result = RandomInt(10)
        ";
        var result = TestHelpers.ExecuteSuccessfully(script, output: _output);

        var data = (JyroObject)result.Data;
        var value = ((JyroNumber)data.GetProperty("result")).Value;
        Assert.True(value >= 0 && value < 10, $"Expected value in [0, 10), got {value}");
        Assert.True(value == Math.Floor(value), "Expected integer value");
    }

    [Fact]
    public void RandomInt_TwoArguments_ReturnsValueInRange()
    {
        var script = @"
            Data.result = RandomInt(5, 15)
        ";
        var result = TestHelpers.ExecuteSuccessfully(script, output: _output);

        var data = (JyroObject)result.Data;
        var value = ((JyroNumber)data.GetProperty("result")).Value;
        Assert.True(value >= 5 && value < 15, $"Expected value in [5, 15), got {value}");
        Assert.True(value == Math.Floor(value), "Expected integer value");
    }

    [Fact]
    public void RandomInt_MultipleCallsProduceDifferentValues()
    {
        var script = @"
            Data.val1 = RandomInt(1000)
            Data.val2 = RandomInt(1000)
            Data.val3 = RandomInt(1000)
        ";
        var result = TestHelpers.ExecuteSuccessfully(script, output: _output);

        var data = (JyroObject)result.Data;
        var val1 = ((JyroNumber)data.GetProperty("val1")).Value;
        var val2 = ((JyroNumber)data.GetProperty("val2")).Value;
        var val3 = ((JyroNumber)data.GetProperty("val3")).Value;

        // Very unlikely all three are the same with a large range
        Assert.False(val1 == val2 && val2 == val3, "Expected at least some variation in random values");
    }

    [Fact]
    public void RandomString_DefaultCharset_ReturnsAlphanumericString()
    {
        var script = @"
            Data.result = RandomString(16)
        ";
        var result = TestHelpers.ExecuteSuccessfully(script, output: _output);

        var data = (JyroObject)result.Data;
        var str = ((JyroString)data.GetProperty("result")).Value;

        Assert.Equal(16, str.Length);
        Assert.All(str, c => Assert.True(
            (c >= 'a' && c <= 'z') || (c >= 'A' && c <= 'Z') || (c >= '0' && c <= '9'),
            $"Expected alphanumeric character, got '{c}'"));
    }

    [Fact]
    public void RandomString_CustomCharset_UsesSpecifiedCharacters()
    {
        var script = @"
            Data.result = RandomString(20, ""ABC123"")
        ";
        var result = TestHelpers.ExecuteSuccessfully(script, output: _output);

        var data = (JyroObject)result.Data;
        var str = ((JyroString)data.GetProperty("result")).Value;

        Assert.Equal(20, str.Length);
        Assert.All(str, c => Assert.Contains(c, "ABC123"));
    }

    [Fact]
    public void RandomString_NumericCharset_GeneratesPin()
    {
        var script = @"
            Data.result = RandomString(4, ""0123456789"")
        ";
        var result = TestHelpers.ExecuteSuccessfully(script, output: _output);

        var data = (JyroObject)result.Data;
        var str = ((JyroString)data.GetProperty("result")).Value;

        Assert.Equal(4, str.Length);
        Assert.All(str, c => Assert.True(c >= '0' && c <= '9', $"Expected digit, got '{c}'"));
    }

    [Fact]
    public void RandomString_MultipleCallsProduceDifferentStrings()
    {
        var script = @"
            Data.str1 = RandomString(32)
            Data.str2 = RandomString(32)
            Data.str3 = RandomString(32)
        ";
        var result = TestHelpers.ExecuteSuccessfully(script, output: _output);

        var data = (JyroObject)result.Data;
        var str1 = ((JyroString)data.GetProperty("str1")).Value;
        var str2 = ((JyroString)data.GetProperty("str2")).Value;
        var str3 = ((JyroString)data.GetProperty("str3")).Value;

        // Extremely unlikely all three 32-char random strings are identical
        Assert.False(str1 == str2 && str2 == str3, "Expected different random strings");
    }

    [Fact]
    public void RandomString_ZeroLength_ReturnsEmptyString()
    {
        var script = @"
            Data.result = RandomString(0)
        ";
        var result = TestHelpers.ExecuteSuccessfully(script, output: _output);

        var data = (JyroObject)result.Data;
        var str = ((JyroString)data.GetProperty("result")).Value;
        Assert.Equal(string.Empty, str);
    }

    [Fact]
    public void RandomChoice_SelectsElementFromArray()
    {
        var script = @"
            var arr = [10, 20, 30, 40, 50]
            Data.result = RandomChoice(arr)
        ";
        var result = TestHelpers.ExecuteSuccessfully(script, output: _output);

        var data = (JyroObject)result.Data;
        var value = ((JyroNumber)data.GetProperty("result")).Value;

        // Check that the value is one of the array elements
        Assert.Contains(value, new[] { 10.0, 20.0, 30.0, 40.0, 50.0 });
    }

    [Fact]
    public void RandomChoice_WorksWithStrings()
    {
        var script = @"
            var colors = [""red"", ""green"", ""blue""]
            Data.result = RandomChoice(colors)
        ";
        var result = TestHelpers.ExecuteSuccessfully(script, output: _output);

        var data = (JyroObject)result.Data;
        var color = ((JyroString)data.GetProperty("result")).Value;

        Assert.Contains(color, new[] { "red", "green", "blue" });
    }

    [Fact]
    public void RandomChoice_WorksWithObjects()
    {
        var script = @"
            var people = [
                { ""name"": ""Alice"" },
                { ""name"": ""Bob"" },
                { ""name"": ""Charlie"" }
            ]
            var chosen = RandomChoice(people)
            Data.result = chosen.name
        ";
        var result = TestHelpers.ExecuteSuccessfully(script, output: _output);

        var data = (JyroObject)result.Data;
        var name = ((JyroString)data.GetProperty("result")).Value;

        Assert.Contains(name, new[] { "Alice", "Bob", "Charlie" });
    }

    [Fact]
    public void RandomChoice_SingleElement_ReturnsThatElement()
    {
        var script = @"
            var arr = [42]
            Data.result = RandomChoice(arr)
        ";
        var result = TestHelpers.ExecuteSuccessfully(script, output: _output);

        var data = (JyroObject)result.Data;
        var value = ((JyroNumber)data.GetProperty("result")).Value;

        Assert.Equal(42.0, value);
    }

    [Fact]
    public void RandomChoice_MultipleCalls_ProduceVariation()
    {
        var script = @"
            var arr = [1, 2, 3, 4, 5, 6, 7, 8, 9, 10]
            Data.val1 = RandomChoice(arr)
            Data.val2 = RandomChoice(arr)
            Data.val3 = RandomChoice(arr)
            Data.val4 = RandomChoice(arr)
            Data.val5 = RandomChoice(arr)
        ";
        var result = TestHelpers.ExecuteSuccessfully(script, output: _output);

        var data = (JyroObject)result.Data;
        var values = new[]
        {
            ((JyroNumber)data.GetProperty("val1")).Value,
            ((JyroNumber)data.GetProperty("val2")).Value,
            ((JyroNumber)data.GetProperty("val3")).Value,
            ((JyroNumber)data.GetProperty("val4")).Value,
            ((JyroNumber)data.GetProperty("val5")).Value
        };

        // With 5 random selections from 10 elements, extremely unlikely all are the same
        var distinctCount = values.Distinct().Count();
        Assert.True(distinctCount > 1, "Expected some variation in random selections");
    }

    #endregion
}
