# First

Returns the first element of an array.

## Signature

```
First(array arr)
```

## Parameters

- **arr** (array): The array to read from.

## Returns

- **any**: The first element.
- **null**: Returned if the array is empty.

## Description

Returns the element at index 0. Does not modify the array.

## Examples

```jyro
var a = First([10, 20, 30])
# a = 10

var b = First([])
# b = null
```
