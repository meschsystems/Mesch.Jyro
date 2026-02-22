# SquareRoot

Returns the square root of a number.

## Signature

```
SquareRoot(number value)
```

## Parameters

- **value** (number): The number to compute the square root of.

## Returns

- **number**: The square root of the input. Returns `NaN` for negative inputs.

## Description

Computes the square root using `Math.Sqrt`. Equivalent to `Power(value, 0.5)`. Returns `NaN` for negative inputs rather than throwing an error.

## Examples

```jyro
var a = SquareRoot(9)
# a = 3

var b = SquareRoot(2)
# b = 1.4142135623730951

var c = SquareRoot(0)
# c = 0

var d = SquareRoot(-1)
# d = NaN
```
