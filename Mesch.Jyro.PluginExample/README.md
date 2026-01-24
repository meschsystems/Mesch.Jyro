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

    public override JyroValue Execute(IReadOnlyList<JyroValue> arguments, JyroExecutionContext executionContext)
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
var result = JyroBuilder.Create(loggerFactory)
    .WithScript("Data.greeting = Greet(\"World\")")
    .WithData(new JyroObject())
    .WithFunctionsFromAssemblyPath("path/to/MyFunctions.dll")
    .Run();
```

### Option 2: Load from Assembly Object

```csharp
var assembly = Assembly.LoadFrom("path/to/MyFunctions.dll");
var result = JyroBuilder.Create(loggerFactory)
    .WithScript("Data.greeting = Greet(\"World\")")
    .WithData(new JyroObject())
    .WithFunctionsFromAssembly(assembly)
    .Run();
```

## Combining with Standard Library

Plugin functions can be combined with the Jyro standard library:

```csharp
var result = JyroBuilder.Create(loggerFactory)
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

## Per-Execution State

If your function needs to maintain state across multiple calls within a single script execution (e.g., sequence generation, caching), use `executionContext.FunctionState`:

```csharp
public class NextIdFunction : JyroFunctionBase
{
    private const string CounterKey = "NextId.Counter";

    public NextIdFunction() : base(new JyroFunctionSignature(
        "NextId", [], ParameterType.Number))
    {
    }

    public override JyroValue Execute(IReadOnlyList<JyroValue> arguments, JyroExecutionContext executionContext)
    {
        // Get current counter value
        var counter = 0;
        if (executionContext.FunctionState.TryGetValue(CounterKey, out var stored))
        {
            counter = (int)stored;
        }

        counter++;

        // Store updated counter
        executionContext.FunctionState[CounterKey] = counter;

        return new JyroNumber(counter);
    }
}
```

```jyro
# Each call returns the next ID: 1, 2, 3...
var id1 = NextId()  # 1
var id2 = NextId()  # 2
var id3 = NextId()  # 3
```

This state is isolated per script execution — the counter resets for each new execution, ensuring tenant isolation and preventing cross-execution data leakage.

## Requirements

- All function classes must have a public parameterless constructor
- Functions must implement the `IJyroFunction` interface (or inherit from `JyroFunctionBase`)
- The DLL must be accessible at runtime
