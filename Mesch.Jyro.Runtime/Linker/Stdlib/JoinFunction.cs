namespace Mesch.Jyro;

/// <summary>
/// Joins elements of an array into a single string using a specified delimiter.
/// Converts all array elements to their string representation and concatenates
/// them with the delimiter between each element. Handles mixed data types by
/// converting non-string values to appropriate string representations.
/// </summary>
public sealed class JoinFunction : JyroFunctionBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="JoinFunction"/> class
    /// with a signature that accepts an array and delimiter string, returning a joined string.
    /// </summary>
    public JoinFunction() : base(FunctionSignatures.Binary("Join", ParameterType.Array, ParameterType.String, ParameterType.String))
    {
    }

    /// <summary>
    /// Executes the string joining operation by concatenating array elements with the delimiter.
    /// </summary>
    /// <param name="arguments">
    /// The function arguments where:
    /// - arguments[0]: The array of elements to join (JyroArray)
    /// - arguments[1]: The delimiter string to place between elements (JyroString)
    /// </param>
    /// <param name="executionContext">The execution context (not used by this function).</param>
    /// <returns>
    /// A <see cref="JyroString"/> containing all array elements joined with the delimiter.
    /// String elements are used directly, null values become "null", and other types
    /// are converted using their ToString() representation.
    /// </returns>
    public override JyroValue Execute(IReadOnlyList<JyroValue> arguments, JyroExecutionContext executionContext)
    {
        var sourceArray = GetArrayArgument(arguments, 0);
        var delimiterString = GetArgument<JyroString>(arguments, 1);

        var stringRepresentations = new List<string>();
        foreach (var arrayElement in sourceArray)
        {
            if (arrayElement is JyroString stringElement)
            {
                stringRepresentations.Add(stringElement.Value);
            }
            else if (arrayElement.IsNull)
            {
                stringRepresentations.Add("null");
            }
            else
            {
                stringRepresentations.Add(arrayElement.ToString() ?? "");
            }
        }

        var joinedResult = string.Join(delimiterString.Value, stringRepresentations);
        return new JyroString(joinedResult);
    }
}