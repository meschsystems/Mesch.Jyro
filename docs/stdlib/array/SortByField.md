# SortByField

Sorts an array of objects by a specified field.

## Signature

```
SortByField(array arr, string fieldName, string direction)
```

## Parameters

- **arr** (array): An array of objects to sort.
- **fieldName** (string): The property name to sort by. Supports dot notation for nested properties.
- **direction** (string): `"asc"` for ascending or `"desc"` for descending. Case-insensitive.

## Returns

- **array**: A new sorted array.

## Description

Returns a new array. The original is not modified. Compares field values as numbers (numeric comparison) or strings (ordinal comparison). Non-object elements or objects missing the field maintain their relative position.

## Examples

```jyro
var users = [
    { "name": "Carol", "age": 35 },
    { "name": "Alice", "age": 30 },
    { "name": "Bob", "age": 25 }
]

var byAge = SortByField(users, "age", "asc")
# byAge = [Bob(25), Alice(30), Carol(35)]

var byName = SortByField(users, "name", "desc")
# byName = [Carol, Bob, Alice]

# Nested field
var items = [
    { "meta": { "priority": 3 } },
    { "meta": { "priority": 1 } }
]
var sorted = SortByField(items, "meta.priority", "asc")
```
