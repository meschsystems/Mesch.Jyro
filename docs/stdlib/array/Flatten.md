# Flatten

Recursively flattens nested arrays into a single-level array.

## Signature

```
Flatten(array arr)
```

## Parameters

- **arr** (array): The array to flatten.

## Returns

- **array**: A new single-level array with all nested elements.

## Description

Recursively traverses nested arrays and collects all non-array elements into a flat result. Flattens to any depth. Returns a new array; the original is not modified.

## Examples

```jyro
var a = Flatten([[1, 2], [3, 4]])
# a = [1, 2, 3, 4]

var b = Flatten([1, [2, [3, [4]]]])
# b = [1, 2, 3, 4]

var c = Flatten([1, 2, 3])
# c = [1, 2, 3] (already flat)
```
