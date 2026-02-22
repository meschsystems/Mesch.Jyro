# Skip

Returns an array with the first N elements removed.

## Signature

```
Skip(array arr, number count)
```

## Parameters

- **arr** (array): The source array.
- **count** (number): The number of elements to skip.

## Returns

- **array**: A new array with the first `count` elements removed.

## Description

Returns a new array. The original is not modified. If `count` exceeds the array length, returns an empty array.

## Examples

```jyro
var a = Skip([10, 20, 30, 40, 50], 2)
# a = [30, 40, 50]

var b = Skip([1, 2, 3], 10)
# b = []

var c = Skip([1, 2, 3], 0)
# c = [1, 2, 3]
```
