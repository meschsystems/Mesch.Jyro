# RemoveAt

Returns a new array with the element at a specified index removed.

## Signature

```
RemoveAt(array arr, number index)
```

## Parameters

- **arr** (array): The source array.
- **index** (number): The zero-based index of the element to remove. Must be an integer.

## Returns

- **array**: A new array without the element at `index`.

## Description

Returns a new array. The original array is not modified. If the index is out of bounds, the returned array is a copy of the original (no element removed, no error thrown).

Throws a runtime error if `index` is not an integer.

## Examples

```jyro
var items = [10, 20, 30, 40]
var result = RemoveAt(items, 1)
# result = [10, 30, 40]
# items = [10, 20, 30, 40] (unchanged)

# Out of bounds - returns copy
var same = RemoveAt(items, 99)
# same = [10, 20, 30, 40]
```
