# Log

Returns the logarithm of a number, optionally with a specified base.

## Signature

```
Log(number value, number base?)
```

## Parameters

- **value** (number): The number to compute the logarithm of.
- **base** (number, optional): The logarithm base. Defaults to the natural logarithm (base *e*).

## Returns

- **number**: The logarithm of the input value. Returns `NaN` for negative inputs. Returns `-Infinity` for zero.

## Description

Computes the logarithm using `Math.Log`. When called with one argument, returns the natural logarithm (ln). When called with two arguments, returns the logarithm in the specified base using `Math.Log(value, base)`.

## Examples

```jyro
# Natural logarithm
var ln = Log(1)
# ln = 0

var lnE = Log(2.718281828)
# lnE = ~1.0

# Base-10 logarithm
var log10 = Log(100, 10)
# log10 = 2

# Base-2 logarithm
var log2 = Log(8, 2)
# log2 = 3

# Negative input
var invalid = Log(-1)
# invalid = NaN
```
