# Last

Returns the last element of an array.

## Signature

```
Last(array arr)
```

## Parameters

- **arr** (array): The array to read from.

## Returns

- **any**: The last element.
- **null**: Returned if the array is empty.

## Description

Returns the element at the highest index. Does not modify the array.

## Examples

```jyro
var a = Last([10, 20, 30])
# a = 30

var b = Last([])
# b = null
```
