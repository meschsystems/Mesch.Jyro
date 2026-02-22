# Power

Raises a number to the specified power.

## Signature

```
Power(number base, number exponent)
```

## Parameters

- **base** (number): The base value.
- **exponent** (number): The power to raise the base to.

## Returns

- **number**: The result of `base` raised to `exponent`.

## Description

Computes exponentiation using `Math.Pow`. Supports fractional exponents (e.g., `0.5` for square root). Returns `Infinity` or `-Infinity` for overflow. Returns `NaN` for undefined operations such as raising a negative number to a fractional power.

## Examples

```jyro
var squared = Power(2, 3)
# squared = 8

var sqrtOf9 = Power(9, 0.5)
# sqrtOf9 = 3

var inverse = Power(2, -1)
# inverse = 0.5

var one = Power(5, 0)
# one = 1
```
