# Sum

Returns the sum of all numeric values in an array.

## Signature

```
Sum(array values)
```

## Parameters

- **values** (array): An array of values to sum. Non-numeric elements are ignored.

## Returns

- **number**: The sum of all numeric elements. Returns `0` if the array is empty or contains no numeric elements.

## Description

Iterates through the array and accumulates the total of all numeric values. Non-numeric elements (strings, booleans, nulls, objects, arrays) are silently skipped. An empty array or an array with no numeric elements returns `0`.

## Examples

```jyro
var total = Sum([1, 2, 3, 4, 5])
# total = 15

var withMixed = Sum([10, "text", 3, null, 7])
# withMixed = 20 (non-numbers skipped)

var empty = Sum([])
# empty = 0

var noNumbers = Sum(["a", "b"])
# noNumbers = 0

# Sum prices from objects
var prices = [10.5, 20.0, 5.75]
var total = Sum(prices)
# total = 36.25
```
