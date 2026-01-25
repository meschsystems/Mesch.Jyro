using System.Text.Json;
using Json.Schema;

namespace Mesch.Jyro;

/// <summary>
/// Validates a Jyro value against a JSON Schema definition.
/// Returns true if the data conforms to the schema, false otherwise.
/// </summary>
public sealed class ValidateSchemaFunction : JyroFunctionBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ValidateSchemaFunction"/> class
    /// with a signature that accepts any value and a schema object, returning a boolean.
    /// </summary>
    public ValidateSchemaFunction()
        : base(new JyroFunctionSignature(
            "ValidateSchema",
            [
                new Parameter("data", ParameterType.Any),
                new Parameter("schema", ParameterType.Object)
            ],
            ParameterType.Boolean))
    {
    }

    /// <summary>
    /// Executes the JSON Schema validation operation.
    /// </summary>
    /// <param name="arguments">
    /// The function arguments where:
    /// - arguments[0]: The data to validate (any JyroValue type)
    /// - arguments[1]: The JSON Schema to validate against (JyroObject)
    /// </param>
    /// <param name="executionContext">The execution context.</param>
    /// <returns>
    /// A <see cref="JyroBoolean"/> indicating whether the data conforms to the schema.
    /// Returns true if valid, false if validation fails.
    /// </returns>
    public override JyroValue Execute(IReadOnlyList<JyroValue> arguments, JyroExecutionContext executionContext)
    {
        var data = arguments[0];
        var schemaObj = GetObjectArgument(arguments, 1);

        // Convert schema to JsonSchema.Net schema
        var schemaJson = schemaObj.ToJson();
        var schema = JsonSchema.FromText(schemaJson);

        // Convert data to JsonDocument for validation
        var dataJson = data.ToJson();
        using var document = JsonDocument.Parse(dataJson);

        // Evaluate and return result
        var result = schema.Evaluate(document.RootElement);
        return JyroBoolean.FromBoolean(result.IsValid);
    }
}
