# Prepend

Creates a new array with a value added to the beginning.

## Signature

```
Prepend(array arr, any value)
```

## Parameters

- **arr** (array): The source array.
- **value** (any): The value to prepend.

## Returns

- **array**: A new array with `value` at position 0 followed by all elements from `arr`.

## Description

Returns a new array. The original array is not modified.

## Examples

```jyro
var items = [2, 3, 4]
var result = Prepend(items, 1)
# result = [1, 2, 3, 4]
# items = [2, 3, 4] (unchanged)
```
