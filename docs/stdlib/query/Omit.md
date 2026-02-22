# Omit

Removes specified fields from each object in an array, returning new objects.

## Signature

```
Omit(array arr, array fields)
```

## Parameters

- **arr** (array): An array of objects.
- **fields** (array): An array of field name strings to exclude.

## Returns

- **array**: A new array of objects containing all fields except those specified.

## Description

Creates a new object for each source object containing all fields except those listed in `fields`. Non-object elements are skipped entirely (not included in the result). This is the inverse of `Project`, which keeps only specified fields.

## Examples

```jyro
var users = [
    { "name": "Alice", "age": 30, "ssn": "12345" },
    { "name": "Bob", "age": 25, "ssn": "67890" }
]

var clean = Omit(users, ["ssn"])
# clean = [
#   { "name": "Alice", "age": 30 },
#   { "name": "Bob", "age": 25 }
# ]

# Remove multiple fields
var minimal = Omit(users, ["age", "ssn"])
# minimal = [
#   { "name": "Alice" },
#   { "name": "Bob" }
# ]
```
