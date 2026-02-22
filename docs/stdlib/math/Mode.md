# Mode

Returns the most frequently occurring numeric value in an array.

## Signature

```
Mode(array values)
```

## Parameters

- **values** (array): An array of values. Non-numeric elements are ignored.

## Returns

- **number**: The most frequent numeric value.
- **null**: Returned if the array contains no numeric elements.

## Description

Counts the frequency of each numeric value in the array and returns the value with the highest count. If multiple values share the highest frequency, returns the first one encountered during iteration. Non-numeric elements are silently skipped.

## Examples

```jyro
var a = Mode([1, 2, 2, 3, 3, 3])
# a = 3 (appears 3 times)

var b = Mode([5, 5, 10, 10, 10])
# b = 10

# Tie - returns first encountered mode
var c = Mode([1, 1, 2, 2])
# c = 1

# Non-numbers skipped
var d = Mode([1, "a", 1, "b", 2])
# d = 1

var empty = Mode([])
# empty = null
```
