# Sort

Sorts an array of primitive values in ascending order.

## Signature

```
Sort(array arr)
```

## Parameters

- **arr** (array): The array to sort.

## Returns

- **array**: A new sorted array.

## Description

Returns a new array. The original is not modified. Elements are sorted by type group, then by value within each group:

1. `null` values (first)
2. Numbers (ascending)
3. Strings (ordinal/lexicographic)
4. Booleans (`false` before `true`)

Mixed-type elements within the same group maintain their relative order. For sorting arrays of objects by a field, use `SortByField`.

## Examples

```jyro
var a = Sort([5, 2, 8, 1, 9])
# a = [1, 2, 5, 8, 9]

var b = Sort(["cherry", "apple", "banana"])
# b = ["apple", "banana", "cherry"]

# Mixed types
var c = Sort([null, "zebra", 5, "apple", 1, true, false])
# c = [null, 1, 5, "apple", "zebra", false, true]

# Original unchanged
var original = [3, 1, 2]
var sorted = Sort(original)
# original = [3, 1, 2]
```
