# Abs

Returns the absolute value of a number.

## Signature

```
Abs(number value)
```

## Parameters

- **value** (number): The number to take the absolute value of.

## Returns

- **number**: The non-negative absolute value of the input.

## Description

Computes the absolute value using `Math.Abs`. Negative values are returned as their positive equivalent. Positive values and zero are returned unchanged.

## Examples

```jyro
var result = Abs(-42)
# result = 42

var zero = Abs(0)
# zero = 0

var positive = Abs(3.14)
# positive = 3.14

var negativeDecimal = Abs(-0.5)
# negativeDecimal = 0.5
```
