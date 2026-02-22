# RandomInt

Generates a cryptographically secure random integer within a specified range.

## Signature

```
RandomInt(number min, number max)
```

## Parameters

- **min** (number): The inclusive lower bound.
- **max** (number): The inclusive upper bound.

## Returns

- **number**: A random integer between `min` and `max` (both inclusive).

## Description

Generates a random integer using `System.Security.Cryptography.RandomNumberGenerator.GetInt32`. This produces cryptographically secure random values suitable for security-sensitive use cases such as generating PINs or tokens. Both bounds are inclusive.

The values are truncated to integers before use. The internal call uses `GetInt32(min, max + 1)` to make the upper bound inclusive.

## Examples

```jyro
# Roll a six-sided die
var roll = RandomInt(1, 6)
# roll = 1..6

# Generate a random ID in a range
var id = RandomInt(1000, 9999)
# id = 1000..9999

# Coin flip
var coin = RandomInt(0, 1)
# coin = 0 or 1

# Select a random index from an array
var items = ["red", "green", "blue"]
var idx = RandomInt(0, Length(items) - 1)
var picked = items[idx]
```
