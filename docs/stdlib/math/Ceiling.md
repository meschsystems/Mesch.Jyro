# Ceiling

Rounds a number up to the nearest integer toward positive infinity.

## Signature

```
Ceiling(number value)
```

## Parameters

- **value** (number): The number to ceil.

## Returns

- **number**: The smallest integer greater than or equal to the input.

## Description

Computes the ceiling using `Math.Ceiling`. Always rounds toward positive infinity. For positive numbers with a fractional part this rounds up. For negative numbers this rounds toward zero.

## Examples

```jyro
var a = Ceiling(3.2)
# a = 4

var b = Ceiling(3.0)
# b = 3

var c = Ceiling(-2.7)
# c = -2

var d = Ceiling(0.1)
# d = 1
```
