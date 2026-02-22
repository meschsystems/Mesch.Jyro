# Coalesce

Returns the first non-null value from an array.

## Signature

```
Coalesce(array values)
```

## Parameters

- **values** (array): An array of values to evaluate. Evaluated left to right.

## Returns

- **any**: The first non-null value. Returns `null` if all values are null or the array is empty.

## Description

Iterates through the provided array left to right and returns the first value that is not null. If all values are null or the array is empty, returns null.

## Examples

```jyro
var a = Coalesce([null, null, "fallback"])
# a = "fallback"

var b = Coalesce([Data.primary, Data.secondary, "default"])
# Returns first non-null value

var c = Coalesce([null])
# c = null
```
