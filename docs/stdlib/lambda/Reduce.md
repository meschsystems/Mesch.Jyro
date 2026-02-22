# Reduce

Accumulates an array into a single value using a lambda function.

## Signature

```
Reduce(array arr, lambda fn, any initial)
```

## Parameters

- **arr** (array): The array to reduce.
- **fn** (lambda): A lambda `(accumulator, item) => newAccumulator` applied to each element.
- **initial** (any): The initial value of the accumulator.

## Returns

- **any**: The final accumulated value.

## Description

Iterates through the array, calling the lambda with the current accumulator and each element. The lambda's return value becomes the new accumulator. After all elements are processed, the final accumulator value is returned.

## Examples

```jyro
var numbers = [1, 2, 3, 4, 5]
var total = Reduce(numbers, (acc, x) => acc + x, 0)
# total = 15

# Build a string
var words = ["hello", "world"]
var sentence = Reduce(words, (acc, w) => acc + " " + w, "")
# sentence = " hello world"

# Count items matching a condition
var items = [1, 2, 3, 4, 5, 6]
var evenCount = Reduce(items, (acc, x) => if x % 2 == 0 then acc + 1 else acc, 0)
# evenCount = 3
```
