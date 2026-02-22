# Append

Returns a new array with an element added to the end.

## Signature

```
Append(array arr, any value)
```

## Parameters

- **arr** (array): The source array.
- **value** (any): The value to add.

## Returns

- **array**: A new array with all elements from `arr` followed by `value`.

## Description

Returns a new array. The original array is not modified.

## Examples

```jyro
var items = [1, 2, 3]
var result = Append(items, 4)
# result = [1, 2, 3, 4]
# items = [1, 2, 3] (unchanged)

# Chaining
var extended = Append(Append(items, 4), 5)
# extended = [1, 2, 3, 4, 5]
```
