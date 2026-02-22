# Median

Returns the median (middle value) of all numeric values in an array.

## Signature

```
Median(array values)
```

## Parameters

- **values** (array): An array of values. Non-numeric elements are ignored.

## Returns

- **number**: The median of the numeric elements.
- **null**: Returned if the array contains no numeric elements.

## Description

Extracts all numeric values from the array, sorts them, and returns the middle value. For arrays with an even count of numeric elements, returns the average of the two middle values. Non-numeric elements are silently skipped.

## Examples

```jyro
# Odd count - returns middle value
var a = Median([1, 3, 5])
# a = 3

# Even count - returns average of two middle values
var b = Median([1, 3, 5, 7])
# b = 4

# Unsorted input is sorted automatically
var c = Median([5, 1, 3])
# c = 3

# Non-numbers are skipped
var d = Median([10, "text", 30, null, 20])
# d = 20

var empty = Median([])
# empty = null
```
