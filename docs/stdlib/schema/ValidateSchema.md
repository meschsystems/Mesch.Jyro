# ValidateSchema

Validates data against a JSON Schema and returns any errors.

## Signature

```
ValidateSchema(any data, object schema)
```

## Parameters

- **data** (any): The value to validate.
- **schema** (object): A JSON Schema definition as a Jyro object.

## Returns

- **array**: An array of error objects. Empty array if the data is valid.

Each error object has:

- **path** (string): The JSON Pointer path to the failing element.
- **keyword** (string): The schema keyword that failed (e.g., `"type"`, `"required"`).
- **message** (string): A description of the failure.

## Description

Serialises both the data and schema to JSON, then evaluates using the JsonSchema.Net library with list output format to collect all errors. Supports JSON Schema draft 2020-12 features including type constraints, required properties, patterns, min/max values, and nested schemas. Returns an empty array when the data is valid.

## Examples

```jyro
var schema = {
    "type": "object",
    "properties": {
        "name": { "type": "string" },
        "age": { "type": "number", "minimum": 0 }
    },
    "required": ["name", "age"]
}

var errors = ValidateSchema({ "age": -5 }, schema)
# errors = [
#   { "path": "", "keyword": "required", "message": "..." },
#   { "path": "/age", "keyword": "minimum", "message": "..." }
# ]

var noErrors = ValidateSchema({ "name": "Alice", "age": 30 }, schema)
# noErrors = []

# Check validity
if Length(errors) == 0 then
    Data.valid = true
end
```
