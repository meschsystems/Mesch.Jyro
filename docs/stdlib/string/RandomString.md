# RandomString

Generates a cryptographically secure random string of a specified length.

## Signature

```
RandomString(number length, string characterSet?)
```

## Parameters

- **length** (number): The number of characters to generate. Must be a non-negative integer.
- **characterSet** (string, optional): The characters to choose from. Defaults to `A-Za-z0-9` (62 alphanumeric characters).

## Returns

- **string**: A random string of the specified length.

## Description

Generates each character independently using `RandomNumberGenerator.GetInt32`, producing cryptographically secure random selections from the character set.

Throws a runtime error if:
- `length` is not a non-negative integer.
- `characterSet` is provided but empty.

## Examples

```jyro
# Default alphanumeric
var token = RandomString(16)
# token = "aB3kF9mNpQ2wX7yZ" (random)

# Numeric PIN
var pin = RandomString(6, "0123456789")
# pin = "847293" (random)

# Hex string
var hex = RandomString(8, "0123456789abcdef")
# hex = "3f7a1c9e" (random)

# Custom charset
var code = RandomString(4, "ABCDEFGHJKLMNPQRSTUVWXYZ23456789")
# code = "K7N3" (random, no ambiguous chars)

# Zero length
var empty = RandomString(0)
# empty = ""
```
