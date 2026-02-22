# Select

Extracts a single field from each object in an array.

## Signature

```
Select(array arr, string fieldName)
```

## Parameters

- **arr** (array): An array of objects.
- **fieldName** (string): The property to extract. Supports dot notation.

## Returns

- **array**: A new array of the extracted values. Non-object elements produce `null` in the result.

## Description

Maps each object in the array to the value of the specified field. Non-object elements are mapped to `null` (preserving array length). If the field does not exist on an object, `null` is added.

## Examples

```jyro
var users = [
    { "name": "Alice", "age": 30 },
    { "name": "Bob", "age": 25 },
    { "name": "Carol", "age": 35 }
]

var names = Select(users, "name")
# names = ["Alice", "Bob", "Carol"]

var ages = Select(users, "age")
# ages = [30, 25, 35]

# Nested field
var items = [
    { "meta": { "id": 100 } },
    { "meta": { "id": 200 } }
]
var ids = Select(items, "meta.id")
# ids = [100, 200]
```
