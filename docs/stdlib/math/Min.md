# Min

Returns the smallest numeric value in an array.

## Signature

```
Min(array values)
```

## Parameters

- **values** (array): An array of values to search. Non-numeric elements are ignored.

## Returns

- **number**: The smallest numeric value found in the array.
- **null**: Returned if the array contains no numeric elements.

## Description

Iterates through the array and tracks the minimum numeric value. Non-numeric elements (strings, booleans, nulls, objects, arrays) are silently skipped. Returns `null` if no numeric elements are found.

## Examples

```jyro
var result = Min([5, 2, 8, 1, 9])
# result = 1

var withMixed = Min([10, "text", 3, null, 7])
# withMixed = 3 (non-numbers skipped)

var negative = Min([-5, -2, -8])
# negative = -8

var empty = Min([])
# empty = null

var noNumbers = Min(["a", "b", "c"])
# noNumbers = null
```
