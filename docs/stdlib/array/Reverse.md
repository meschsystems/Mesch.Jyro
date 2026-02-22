# Reverse

Returns a new array with elements in reverse order.

## Signature

```
Reverse(array arr)
```

## Parameters

- **arr** (array): The array to reverse.

## Returns

- **array**: A new array with elements in reverse order.

## Description

Returns a new array. The original array is not modified.

## Examples

```jyro
var result = Reverse([1, 2, 3])
# result = [3, 2, 1]

var original = [10, 20, 30]
var reversed = Reverse(original)
# original = [10, 20, 30] (unchanged)
# reversed = [30, 20, 10]
```
