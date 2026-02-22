# SelectMany

Flattens a nested array field from each object into a single array.

## Signature

```
SelectMany(array arr, string fieldName)
```

## Parameters

- **arr** (array): An array of objects.
- **fieldName** (string): The property containing arrays to flatten. Supports dot notation.

## Returns

- **array**: A new flat array containing all elements from the specified field across all objects.

## Description

Iterates the array, reads the specified field from each object, and collects all elements from array-valued fields into a single result. Non-object elements are skipped. Objects where the field is not an array are skipped.

## Examples

```jyro
var orders = [
    { "id": 1, "items": ["apple", "banana"] },
    { "id": 2, "items": ["cherry"] },
    { "id": 3, "items": ["date", "elderberry"] }
]

var allItems = SelectMany(orders, "items")
# allItems = ["apple", "banana", "cherry", "date", "elderberry"]

# Skips non-array fields
var mixed = [
    { "tags": ["a", "b"] },
    { "tags": "not-an-array" },
    { "tags": ["c"] }
]
var tags = SelectMany(mixed, "tags")
# tags = ["a", "b", "c"]
```
