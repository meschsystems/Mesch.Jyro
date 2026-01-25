using System.Text.Json;
using Json.Schema;

namespace Mesch.Jyro;

/// <summary>
/// Validates a Jyro value against a JSON Schema definition and returns detailed error information.
/// Returns an array of error objects if validation fails, or an empty array if valid.
/// </summary>
public sealed class GetSchemaErrorsFunction : JyroFunctionBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="GetSchemaErrorsFunction"/> class
    /// with a signature that accepts any value and a schema object, returning an array of errors.
    /// </summary>
    public GetSchemaErrorsFunction()
        : base(new JyroFunctionSignature(
            "GetSchemaErrors",
            [
                new Parameter("data", ParameterType.Any),
                new Parameter("schema", ParameterType.Object)
            ],
            ParameterType.Array))
    {
    }

    /// <summary>
    /// Executes the JSON Schema validation and returns detailed error information.
    /// </summary>
    /// <param name="arguments">
    /// The function arguments where:
    /// - arguments[0]: The data to validate (any JyroValue type)
    /// - arguments[1]: The JSON Schema to validate against (JyroObject)
    /// </param>
    /// <param name="executionContext">The execution context.</param>
    /// <returns>
    /// A <see cref="JyroArray"/> containing error objects. Each error object has:
    /// - path: The JSON path to the failing value
    /// - keyword: The schema keyword that failed validation
    /// - message: A human-readable error message
    /// Returns an empty array if the data is valid.
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

        // Configure evaluation to collect all errors with detailed output
        var options = new EvaluationOptions
        {
            OutputFormat = OutputFormat.List
        };

        // Evaluate and collect errors
        var result = schema.Evaluate(document.RootElement, options);
        var errors = new JyroArray();

        if (!result.IsValid)
        {
            CollectErrors(result, errors);
        }

        return errors;
    }

    private static void CollectErrors(EvaluationResults result, JyroArray errors)
    {
        // Check if this result has errors
        if (result.Errors != null && result.Errors.Count > 0)
        {
            foreach (var error in result.Errors)
            {
                var errorObj = new JyroObject
                {
                    ["path"] = new JyroString(result.InstanceLocation.ToString()),
                    ["keyword"] = new JyroString(error.Key),
                    ["message"] = new JyroString(error.Value)
                };
                errors.Add(errorObj);
            }
        }

        // Recursively collect errors from nested results
        if (result.Details != null)
        {
            foreach (var detail in result.Details)
            {
                if (!detail.IsValid)
                {
                    CollectErrors(detail, errors);
                }
            }
        }
    }
}
