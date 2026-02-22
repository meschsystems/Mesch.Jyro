# RandomChoice

Selects a random element from an array.

## Signature

```
RandomChoice(array arr)
```

## Parameters

- **arr** (array): The array to choose from.

## Returns

- **any**: A randomly selected element.
- **null**: Returned if the array is empty.

## Description

Uses `RandomNumberGenerator.GetInt32` for cryptographically secure random selection. Each element has equal probability of being chosen. The array is not modified.

## Examples

```jyro
var colors = ["red", "green", "blue"]
var picked = RandomChoice(colors)
# picked = "red", "green", or "blue" (random)

var dice = [1, 2, 3, 4, 5, 6]
var roll = RandomChoice(dice)
# roll = 1..6 (random)

var empty = RandomChoice([])
# empty = null
```
