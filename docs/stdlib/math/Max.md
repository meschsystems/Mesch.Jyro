# Max

Returns the largest numeric value in an array.

## Signature

```
Max(array values)
```

## Parameters

- **values** (array): An array of values to search. Non-numeric elements are ignored.

## Returns

- **number**: The largest numeric value found in the array.
- **null**: Returned if the array contains no numeric elements.

## Description

Iterates through the array and tracks the maximum numeric value. Non-numeric elements (strings, booleans, nulls, objects, arrays) are silently skipped. Returns `null` if no numeric elements are found.

## Examples

```jyro
var result = Max([5, 2, 8, 1, 9])
# result = 9

var withMixed = Max([10, "text", 3, null, 7])
# withMixed = 10 (non-numbers skipped)

var negative = Max([-5, -2, -8])
# negative = -2

var empty = Max([])
# empty = null
```
