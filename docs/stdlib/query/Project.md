# Project

Extracts specified fields from each object in an array into new objects.

## Signature

```
Project(array arr, array fields)
```

## Parameters

- **arr** (array): An array of objects.
- **fields** (array): An array of field name strings to include. Supports dot notation.

## Returns

- **array**: A new array of objects containing only the specified fields.

## Description

Creates a new object for each source object containing only the requested fields. Non-object elements are skipped entirely (not included in the result). For dot-notation fields, only the last segment is used as the property name in the output.

## Examples

```jyro
var users = [
    { "name": "Alice", "age": 30, "email": "alice@example.com" },
    { "name": "Bob", "age": 25, "email": "bob@example.com" }
]

var projected = Project(users, ["name", "age"])
# projected = [
#   { "name": "Alice", "age": 30 },
#   { "name": "Bob", "age": 25 }
# ]

# Dot notation - last segment becomes key
var items = [
    { "meta": { "id": 100 }, "name": "Widget" }
]
var result = Project(items, ["name", "meta.id"])
# result = [{ "name": "Widget", "id": 100 }]
```
