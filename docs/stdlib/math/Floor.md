# Floor

Rounds a number down to the nearest integer toward negative infinity.

## Signature

```
Floor(number value)
```

## Parameters

- **value** (number): The number to floor.

## Returns

- **number**: The largest integer less than or equal to the input.

## Description

Computes the floor using `Math.Floor`. Always rounds toward negative infinity. For positive numbers with a fractional part this rounds down. For negative numbers this rounds away from zero.

## Examples

```jyro
var a = Floor(3.7)
# a = 3

var b = Floor(3.0)
# b = 3

var c = Floor(-2.3)
# c = -3

var d = Floor(0.9)
# d = 0
```
