# ToString

Converts any value to its string representation.

## Signature

```
ToString(any value)
```

## Parameters

- **value** (any): The value to convert.

## Returns

- **string**: The string representation of the value.

## Description

Converts the value using `JyroValue.ToStringValue()`. Numbers, booleans, and null are converted to their standard string forms.

## Examples

```jyro
var a = ToString(42)
# a = "42"

var b = ToString(true)
# b = "true"

var c = ToString(null)
# c = "null"
```
