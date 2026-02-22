# Average

Returns the arithmetic mean of all numeric values in an array. Alias for `Avg`.

## Signature

```
Average(array values)
```

## Parameters

- **values** (array): An array of values to average. Non-numeric elements are ignored.

## Returns

- **number**: The arithmetic mean of all numeric elements.
- **null**: Returned if the array is empty or contains no numeric elements.

## Description

Identical to `Avg`. Computes the average by summing all numeric values and dividing by the count of numeric values. Non-numeric elements are silently skipped and excluded from both the sum and the count. Returns `null` if no numeric elements are found.

## Examples

```jyro
var result = Average([10, 20, 30])
# result = 20

var withMixed = Average([10, "text", 30])
# withMixed = 20 (only 10 and 30 counted)

var empty = Average([])
# empty = null
```
