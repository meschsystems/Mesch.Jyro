namespace Mesch.Jyro;

/// <summary>
/// Generates a new globally unique identifier (GUID) and returns it as a string.
/// This function creates a new random GUID each time it is called, providing
/// a unique identifier suitable for tracking, correlation, or identification purposes
/// in Jyro scripts.
/// </summary>
/// <remarks>
/// The generated GUID follows the standard format: xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx
/// where each x is a hexadecimal digit. The GUID is generated using the system's
/// cryptographically secure random number generator, ensuring uniqueness across
/// different machines and time periods.
///
/// Common use cases include:
/// <list type="bullet">
/// <item><description>Creating unique identifiers for records or transactions</description></item>
/// <item><description>Generating correlation IDs for tracking operations</description></item>
/// <item><description>Creating temporary unique keys in data processing</description></item>
/// <item><description>Providing unique session or request identifiers</description></item>
/// </list>
/// </remarks>
public sealed class NewGuidFunction : JyroFunctionBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="NewGuidFunction"/> class
    /// with a signature that takes no parameters and returns a string.
    /// </summary>
    public NewGuidFunction() : base(new JyroFunctionSignature(
    "NewGuid",
    Array.Empty<Parameter>(),
    ParameterType.String))
    {
    }

    /// <summary>
    /// Executes the GUID generation operation, creating a new unique identifier.
    /// </summary>
    /// <param name="arguments">
    /// The function arguments (none required for this function).
    /// </param>
    /// <param name="executionContext">
    /// The execution context.
    /// </param>
    /// <returns>
    /// A <see cref="JyroString"/> containing a newly generated GUID in standard
    /// format (xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx).
    /// </returns>
    /// <remarks>
    /// This function generates a Version 4 (random) GUID using .NET's <see cref="Guid.NewGuid"/>
    /// method, which provides cryptographically strong random values. Each call to this
    /// function will produce a different GUID, making it suitable for creating unique
    /// identifiers in scripts that process multiple records or operations.
    ///
    /// The returned string is always in lowercase format for consistency.
    /// </remarks>
    public override JyroValue Execute(IReadOnlyList<JyroValue> arguments, JyroExecutionContext executionContext)
    {
        var newGuid = Guid.NewGuid();
        var guidString = newGuid.ToString("D").ToLowerInvariant();
        return new JyroString(guidString);
    }
}