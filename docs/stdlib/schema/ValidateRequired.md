# ValidateRequired

Checks whether an object contains all required fields with non-null values.

## Signature

```
ValidateRequired(object obj, array fields)
```

## Parameters

- **obj** (object): The object to validate.
- **fields** (array): An array of field name strings that must be present and non-null.

## Returns

- **object**: A validation result with properties:
  - **valid** (boolean): `true` if all required fields are present and non-null.
  - **errors** (array): An array of error message strings describing which fields failed.

## Description

Iterates the required field names and checks each against the object. A field fails validation if it is missing from the object or if its value is null. Non-string elements in the `fields` array are skipped.

## Examples

```jyro
var user = { "name": "Alice", "email": "alice@example.com" }

var result = ValidateRequired(user, ["name", "email", "age"])
# result.valid = false
# result.errors = ["Missing required field: age"]

var ok = ValidateRequired(user, ["name", "email"])
# ok.valid = true
# ok.errors = []

# Null values fail
var partial = { "name": "Alice", "age": null }
var check = ValidateRequired(partial, ["name", "age"])
# check.valid = false
# check.errors = ["Field is null: age"]
```
