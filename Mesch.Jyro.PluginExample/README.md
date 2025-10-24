# Jyro Plugin Example

This project demonstrates how to create custom Jyro functions in an external DLL that can be loaded at runtime.

## Creating Custom Functions

To create custom Jyro functions:

1. Create a new .NET class library project
2. Add a reference to the `Mesch.Jyro` NuGet package
3. Create classes that inherit from `JyroFunctionBase`
4. Implement the required constructor and `Execute` method

### Example Function

```csharp
using Mesch.Jyro;

public sealed class GreetFunction : JyroFunctionBase
{
    public GreetFunction() : base(new JyroFunctionSignature(
        "Greet",
        new[] { new Parameter("name", ParameterType.String) },
        ParameterType.String))
    {
    }

    public override JyroValue Execute(IReadOnlyList<JyroValue> arguments, ExecutionContext executionContext)
    {
        var name = GetStringArgument(arguments, 0);
        var greeting = $"Hello, {name}! Welcome to Jyro plugins!";
        return new JyroString(greeting);
    }
}
```

## Loading Functions at Runtime

### Option 1: Load from Assembly Path

```csharp
var result = JyroBuilder.Create()
    .WithScript("Data.greeting = Greet(\"World\")")
    .WithData(new JyroObject())
    .WithFunctionsFromAssemblyPath("path/to/MyFunctions.dll")
    .Run();
```

### Option 2: Load from Assembly Object

```csharp
var assembly = Assembly.LoadFrom("path/to/MyFunctions.dll");
var result = JyroBuilder.Create()
    .WithScript("Data.greeting = Greet(\"World\")")
    .WithData(new JyroObject())
    .WithFunctionsFromAssembly(assembly)
    .Run();
```

## Combining with Standard Library

Plugin functions can be combined with the Jyro standard library:

```csharp
var result = JyroBuilder.Create()
    .WithScript(@"
        var upperName = Upper(""jyro"")
        Data.greeting = Greet(upperName)
    ")
    .WithData(new JyroObject())
    .WithStandardLibrary()
    .WithFunctionsFromAssemblyPath("path/to/MyFunctions.dll")
    .Run();
```

## Example Functions in This Project

This example project includes three sample functions:

- **Greet(name)** - Creates a greeting message
- **ReverseString(text)** - Reverses a string
- **Multiply(a, b)** - Multiplies two numbers

## Requirements

- All function classes must have a public parameterless constructor
- Functions must implement the `IJyroFunction` interface (or inherit from `JyroFunctionBase`)
- The DLL must be accessible at runtime
