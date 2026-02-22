# Clamp

Constrains a number to a specified range.

## Signature

```
Clamp(number value, number min, number max)
```

## Parameters

- **value** (number): The number to constrain.
- **min** (number): The lower bound of the range.
- **max** (number): The upper bound of the range.

## Returns

- **number**: The clamped value. Returns `min` if `value < min`, `max` if `value > max`, otherwise `value` unchanged.

## Description

Uses `Math.Clamp` to constrain the value within the inclusive range `[min, max]`. If `min > max`, the behaviour follows .NET's `Math.Clamp` which returns `min`.

## Examples

```jyro
var a = Clamp(150, 0, 100)
# a = 100 (above max)

var b = Clamp(-50, 0, 100)
# b = 0 (below min)

var c = Clamp(50, 0, 100)
# c = 50 (within range)

var d = Clamp(0, 0, 100)
# d = 0 (at lower bound)

# Normalize a percentage
var pct = Clamp(Data.score, 0, 100)
```
