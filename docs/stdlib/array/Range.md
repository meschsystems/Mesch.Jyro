# Range

Generates an array of numbers within a specified range.

## Signature

```
Range(number start, number end, number step?)
```

## Parameters

- **start** (number): The starting value (inclusive).
- **end** (number): The ending value (inclusive).
- **step** (number, optional): The increment between values. Defaults to `1`.

## Returns

- **array**: An array of numbers from `start` to `end`.

## Description

Generates a sequence of numbers. If `step` is positive, counts up while `<= end`. If `step` is negative, counts down while `>= end`. If `step` is `0`, returns an empty array (no infinite loop).

## Examples

```jyro
var a = Range(1, 5)
# a = [1, 2, 3, 4, 5]

var b = Range(0, 10, 2)
# b = [0, 2, 4, 6, 8, 10]

var c = Range(5, 1, -1)
# c = [5, 4, 3, 2, 1]

var d = Range(0, 1, 0.25)
# d = [0, 0.25, 0.5, 0.75, 1]
```
