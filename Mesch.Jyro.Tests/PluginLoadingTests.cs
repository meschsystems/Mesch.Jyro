using System.Reflection;

namespace Mesch.Jyro.Tests;

/// <summary>
/// Tests for dynamically loading JyroFunctions from external assemblies and DLL files.
/// </summary>
public class PluginLoadingTests
{
    [Fact]
    public void WithFunctionsFromAssembly_LoadsAndExecutesPluginFunctions()
    {
        // Arrange
        var pluginAssembly = Assembly.LoadFrom(
            Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Mesch.Jyro.PluginExample.dll"));

        var script = @"
            Data.greeting = Greet(""World"")
            Data.reversed = ReverseString(""Hello"")
            Data.product = Multiply(6, 7)
        ";

        var data = new JyroObject();

        // Act
        var result = JyroBuilder.Create()
            .WithScript(script)
            .WithData(data)
            .WithFunctionsFromAssembly(pluginAssembly)
            .Run();

        // Assert
        Assert.True(result.IsSuccessful);
        Assert.Equal("Hello, World! Welcome to Jyro plugins!", ((JyroString)data["greeting"]).Value);
        Assert.Equal("olleH", ((JyroString)data["reversed"]).Value);
        Assert.Equal(42, ((JyroNumber)data["product"]).Value);
    }

    [Fact]
    public void WithFunctionsFromAssemblyPath_LoadsAndExecutesPluginFunctions()
    {
        // Arrange
        var pluginPath = Path.Combine(
            AppDomain.CurrentDomain.BaseDirectory,
            "Mesch.Jyro.PluginExample.dll");

        var script = "Data.result = Multiply(10, 5)";

        var data = new JyroObject();

        // Act
        var result = JyroBuilder.Create()
            .WithScript(script)
            .WithData(data)
            .WithFunctionsFromAssemblyPath(pluginPath)
            .Run();

        // Assert
        Assert.True(result.IsSuccessful);
        Assert.Equal(50, ((JyroNumber)data["result"]).Value);
    }

    [Fact]
    public void WithFunctionsFromAssemblyPath_ThrowsFileNotFoundException_WhenPathDoesNotExist()
    {
        // Arrange
        var nonExistentPath = "C:\\NonExistent\\Path\\Plugin.dll";

        // Act & Assert
        Assert.Throws<FileNotFoundException>(() =>
            JyroBuilder.Create()
                .WithScript("Data.x = 1")
                .WithData(new JyroObject())
                .WithFunctionsFromAssemblyPath(nonExistentPath));
    }

    [Fact]
    public void WithFunctionsFromAssemblyPath_ThrowsArgumentNullException_WhenPathIsNull()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() =>
            JyroBuilder.Create()
                .WithScript("Data.x = 1")
                .WithData(new JyroObject())
                .WithFunctionsFromAssemblyPath(null!));
    }

    [Fact]
    public void WithFunctionsFromAssembly_ThrowsArgumentNullException_WhenAssemblyIsNull()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() =>
            JyroBuilder.Create()
                .WithScript("Data.x = 1")
                .WithData(new JyroObject())
                .WithFunctionsFromAssembly(null!));
    }

    [Fact]
    public void WithFunctionsFromAssembly_CanCombineWithStandardLibrary()
    {
        // Arrange
        var pluginAssembly = Assembly.LoadFrom(
            Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Mesch.Jyro.PluginExample.dll"));

        var script = @"
            var upperName = Upper(""jyro"")
            Data.greeting = Greet(upperName)
        ";

        var data = new JyroObject();

        // Act
        var result = JyroBuilder.Create()
            .WithScript(script)
            .WithData(data)
            .WithStandardLibrary()
            .WithFunctionsFromAssembly(pluginAssembly)
            .Run();

        // Assert
        if (!result.IsSuccessful)
        {
            var errors = string.Join(", ", result.Messages.Select(m => $"[{m.Severity}] Code: {m.Code} at Line {m.LineNumber}:{m.ColumnPosition}"));
            throw new Exception($"Script execution failed: {errors}");
        }
        Assert.True(result.IsSuccessful);
        Assert.Equal("Hello, JYRO! Welcome to Jyro plugins!", ((JyroString)data["greeting"]).Value);
    }

    [Fact]
    public void WithFunctionsFromAssembly_CanBeCompiledOnceAndExecutedMultipleTimes()
    {
        // Arrange
        var pluginAssembly = Assembly.LoadFrom(
            Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Mesch.Jyro.PluginExample.dll"));

        var script = "Data.result = Multiply(Data.a, Data.b)";

        // Compile once
        var linkResult = JyroBuilder.Create()
            .WithScript(script)
            .WithFunctionsFromAssembly(pluginAssembly)
            .Compile();

        Assert.True(linkResult.IsSuccessful);

        // Execute multiple times with different data
        var data1 = new JyroObject { ["a"] = new JyroNumber(2), ["b"] = new JyroNumber(3) };
        var result1 = JyroBuilder.Create()
            .WithCompiledProgram(linkResult.Program!)
            .WithData(data1)
            .Execute();

        var data2 = new JyroObject { ["a"] = new JyroNumber(5), ["b"] = new JyroNumber(7) };
        var result2 = JyroBuilder.Create()
            .WithCompiledProgram(linkResult.Program!)
            .WithData(data2)
            .Execute();

        // Assert
        Assert.True(result1.IsSuccessful);
        Assert.Equal(6, ((JyroNumber)data1["result"]).Value);

        Assert.True(result2.IsSuccessful);
        Assert.Equal(35, ((JyroNumber)data2["result"]).Value);
    }

    [Fact]
    public void WithFunctionsFromDirectory_LoadsAllPluginsFromDirectory()
    {
        // Arrange
        var pluginDirectory = AppDomain.CurrentDomain.BaseDirectory;

        var script = @"
            Data.greeting = Greet(""World"")
            Data.reversed = ReverseString(""Hello"")
            Data.product = Multiply(3, 4)
        ";

        var data = new JyroObject();

        // Act
        var result = JyroBuilder.Create()
            .WithScript(script)
            .WithData(data)
            .WithFunctionsFromDirectory(pluginDirectory)
            .Run();

        // Assert
        Assert.True(result.IsSuccessful);
        Assert.Equal("Hello, World! Welcome to Jyro plugins!", ((JyroString)data["greeting"]).Value);
        Assert.Equal("olleH", ((JyroString)data["reversed"]).Value);
        Assert.Equal(12, ((JyroNumber)data["product"]).Value);
    }

    [Fact]
    public void WithFunctionsFromDirectory_WithSearchPattern_LoadsOnlyMatchingFiles()
    {
        // Arrange
        var pluginDirectory = AppDomain.CurrentDomain.BaseDirectory;

        var script = "Data.result = Multiply(5, 6)";
        var data = new JyroObject();

        // Act - Using a pattern that matches the plugin DLL
        var result = JyroBuilder.Create()
            .WithScript(script)
            .WithData(data)
            .WithFunctionsFromDirectory(pluginDirectory, "*.PluginExample.dll")
            .Run();

        // Assert
        Assert.True(result.IsSuccessful);
        Assert.Equal(30, ((JyroNumber)data["result"]).Value);
    }

    [Fact]
    public void WithFunctionsFromDirectory_ThrowsDirectoryNotFoundException_WhenDirectoryDoesNotExist()
    {
        // Arrange
        var nonExistentPath = "C:\\NonExistent\\Plugins";

        // Act & Assert
        Assert.Throws<DirectoryNotFoundException>(() =>
            JyroBuilder.Create()
                .WithScript("Data.x = 1")
                .WithData(new JyroObject())
                .WithFunctionsFromDirectory(nonExistentPath));
    }

    [Fact]
    public void WithFunctionsFromDirectory_ThrowsArgumentNullException_WhenDirectoryPathIsNull()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() =>
            JyroBuilder.Create()
                .WithScript("Data.x = 1")
                .WithData(new JyroObject())
                .WithFunctionsFromDirectory(null!));
    }

    [Fact]
    public void WithFunctionsFromDirectory_CanCombineWithStandardLibrary()
    {
        // Arrange
        var pluginDirectory = AppDomain.CurrentDomain.BaseDirectory;

        var script = @"
            var upperName = Upper(""jyro"")
            Data.greeting = Greet(upperName)
        ";

        var data = new JyroObject();

        // Act
        var result = JyroBuilder.Create()
            .WithScript(script)
            .WithData(data)
            .WithStandardLibrary()
            .WithFunctionsFromDirectory(pluginDirectory)
            .Run();

        // Assert
        Assert.True(result.IsSuccessful);
        Assert.Equal("Hello, JYRO! Welcome to Jyro plugins!", ((JyroString)data["greeting"]).Value);
    }

    [Fact]
    public void WithFunctionsFromDirectory_SkipsNonDotNetAssemblies()
    {
        // Arrange
        var pluginDirectory = AppDomain.CurrentDomain.BaseDirectory;

        var script = "Data.result = Multiply(2, 3)";
        var data = new JyroObject();

        // Act - Should not throw even if directory contains native DLLs
        var result = JyroBuilder.Create()
            .WithScript(script)
            .WithData(data)
            .WithFunctionsFromDirectory(pluginDirectory)
            .Run();

        // Assert - Should still work with valid plugin DLLs
        Assert.True(result.IsSuccessful);
        Assert.Equal(6, ((JyroNumber)data["result"]).Value);
    }
}
