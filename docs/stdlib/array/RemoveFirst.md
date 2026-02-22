# RemoveFirst

Returns a new array without the first element.

## Signature

```
RemoveFirst(array arr)
```

## Parameters

- **arr** (array): The source array.

## Returns

- **array**: A new array containing all elements except the first. Returns an empty array if the source is empty.

## Description

Returns a new array containing all elements except the first. The original array is not modified. Use `First` to retrieve the first element without removal.

## Examples

```jyro
var items = [1, 2, 3]
var result = RemoveFirst(items)
# result = [2, 3]
# items = [1, 2, 3] (unchanged)

var empty = RemoveFirst([])
# empty = []
```
