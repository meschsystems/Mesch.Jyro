# Distinct

Removes duplicate elements from an array.

## Signature

```
Distinct(array arr)
```

## Parameters

- **arr** (array): The array to deduplicate.

## Returns

- **array**: A new array with duplicates removed. First occurrence order is preserved.

## Description

Compares elements using deep equality (`JyroValue.Equals`). Returns a new array containing only the first occurrence of each unique value. The original array is not modified.

Works with all value types including objects and nested arrays.

## Examples

```jyro
var a = Distinct([1, 2, 2, 3, 1])
# a = [1, 2, 3]

var b = Distinct(["a", "b", "a", "c"])
# b = ["a", "b", "c"]

# Deep equality for objects
var c = Distinct([{ "id": 1 }, { "id": 2 }, { "id": 1 }])
# c = [{ "id": 1 }, { "id": 2 }]
```
