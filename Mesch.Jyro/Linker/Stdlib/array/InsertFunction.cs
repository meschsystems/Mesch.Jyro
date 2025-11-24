namespace Mesch.Jyro;

/// <summary>
/// Inserts a value at a specific index position within an array, shifting existing
/// elements to accommodate the new value. The insertion operation modifies the
/// original array in-place and supports insertion at any valid position including
/// the beginning, middle, or end of the array.
/// </summary>
public sealed class InsertFunction : JyroFunctionBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="InsertFunction"/> class
    /// with a signature that accepts an array, index position, and value to insert.
    /// </summary>
    public InsertFunction() : base(new JyroFunctionSignature(
        "Insert",
        [
            new Parameter("array", ParameterType.Array),
            new Parameter("index", ParameterType.Number),
            new Parameter("value", ParameterType.Any)
        ],
        ParameterType.Array))
    {
    }

    /// <summary>
    /// Executes the insertion operation by placing the specified value at the given index position.
    /// </summary>
    /// <param name="arguments">
    /// The function arguments where:
    /// - arguments[0]: The target array to modify (JyroArray)
    /// - arguments[1]: The zero-based index position for insertion (JyroNumber, must be integer)
    /// - arguments[2]: The value to insert (any JyroValue type)
    /// </param>
    /// <param name="executionContext">The execution context.</param>
    /// <returns>
    /// The modified array with the value inserted at the specified position.
    /// Elements at and after the insertion point are shifted to higher indices.
    /// </returns>
    /// <exception cref="JyroRuntimeException">
    /// Thrown when the index is not an integer or when the index is outside the valid
    /// range (0 to array.Length inclusive). The upper bound allows insertion at the end.
    /// </exception>
    public override JyroValue Execute(IReadOnlyList<JyroValue> arguments, JyroExecutionContext executionContext)
    {
        var targetArray = GetArrayArgument(arguments, 0);
        var indexArgument = GetArgument<JyroNumber>(arguments, 1);
        var valueToInsert = arguments[2];

        if (!indexArgument.IsInteger)
        {
            throw new JyroRuntimeException("Insert() function requires an integer index");
        }

        var insertionIndex = indexArgument.ToInteger();
        if (insertionIndex < 0 || insertionIndex > targetArray.Length)
        {
            throw new JyroRuntimeException($"Insert() index {insertionIndex} is out of bounds for array of length {targetArray.Length}");
        }

        targetArray.Insert(insertionIndex, valueToInsert);
        return targetArray;
    }
}