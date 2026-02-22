# Insert

Returns a new array with an element inserted at a specified index.

## Signature

```
Insert(array arr, number index, any value)
```

## Parameters

- **arr** (array): The source array.
- **index** (number): The zero-based position to insert at. Must be an integer.
- **value** (any): The value to insert.

## Returns

- **array**: A new array with `value` inserted at `index`.

## Description

Returns a new array. The original array is not modified. Existing elements at and after `index` are shifted right. Valid indices are `0` through `Length(arr)` (insert at end).

Throws a runtime error if `index` is not an integer or is out of bounds.

## Examples

```jyro
var items = [1, 2, 3]
var result = Insert(items, 1, "x")
# result = [1, "x", 2, 3]
# items = [1, 2, 3] (unchanged)

# Insert at start
var first = Insert(items, 0, "first")
# first = ["first", 1, 2, 3]
```
