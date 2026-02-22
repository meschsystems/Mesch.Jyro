# All

Tests whether all elements in an array satisfy a lambda predicate.

## Signature

```
All(array arr, lambda fn)
```

## Parameters

- **arr** (array): The array to test.
- **fn** (lambda): A lambda `(item) => boolean` applied to each element.

## Returns

- **boolean**: `true` if the lambda returns truthy for every element. Returns `true` for an empty array.

## Description

Iterates the array and tests each element with the lambda. Short-circuits and returns `false` at the first element that fails the test.

For field-based testing on object arrays, see `AllByField`.

## Examples

```jyro
var numbers = [2, 4, 6, 8]
var allEven = All(numbers, (x) => x % 2 == 0)
# allEven = true

var mixed = [2, 3, 4]
var allEvenMixed = All(mixed, (x) => x % 2 == 0)
# allEvenMixed = false

var users = [
    { "name": "Alice", "active": true },
    { "name": "Bob", "active": true }
]
var allActive = All(users, (u) => u.active)
# allActive = true
```
