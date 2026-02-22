# Concatenate

Concatenates two arrays into a new array.

## Signature

```
Concatenate(array first, array second)
```

## Parameters

- **first** (array): The first array.
- **second** (array): The second array.

## Returns

- **array**: A new array containing all elements from `first` followed by all elements from `second`.

## Description

Returns a new array. Neither input array is modified. For concatenating more than two arrays, see `FlattenOnce`.

## Examples

```jyro
var result = Concatenate([1, 2], [3, 4])
# result = [1, 2, 3, 4]

var mixed = Concatenate(["a", "b"], [1, 2])
# mixed = ["a", "b", 1, 2]
```
