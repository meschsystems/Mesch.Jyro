# RemoveLast

Returns a new array without the last element.

## Signature

```
RemoveLast(array arr)
```

## Parameters

- **arr** (array): The source array.

## Returns

- **array**: A new array containing all elements except the last. Returns an empty array if the source is empty.

## Description

Returns a new array containing all elements except the last. The original array is not modified. Use `Last` to retrieve the last element without removal.

## Examples

```jyro
var items = [1, 2, 3]
var result = RemoveLast(items)
# result = [1, 2]
# items = [1, 2, 3] (unchanged)

var empty = RemoveLast([])
# empty = []
```
