# Slice

Extracts a portion of an array by index range.

## Signature

```
Slice(array arr, number start, number end?)
```

## Parameters

- **arr** (array): The source array.
- **start** (number): The zero-based start index (inclusive).
- **end** (number, optional): The end index (exclusive). Defaults to the array length.

## Returns

- **array**: A new array containing the elements from `start` up to (but not including) `end`.

## Description

Returns a new array. The original is not modified. Out-of-bounds indices are clamped to the valid range.

## Examples

```jyro
var a = Slice([10, 20, 30, 40, 50], 1, 4)
# a = [20, 30, 40]

var b = Slice([10, 20, 30, 40, 50], 2)
# b = [30, 40, 50] (to end)

# Clamped bounds
var c = Slice([1, 2, 3], 0, 100)
# c = [1, 2, 3]
```
