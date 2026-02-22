# Any

Tests whether any element in an array satisfies a lambda predicate.

## Signature

```
Any(array arr, lambda fn)
```

## Parameters

- **arr** (array): The array to test.
- **fn** (lambda): A lambda `(item) => boolean` applied to each element.

## Returns

- **boolean**: `true` if the lambda returns truthy for at least one element. Returns `false` for an empty array.

## Description

Iterates the array and tests each element with the lambda. Short-circuits and returns `true` at the first element that passes the test.

For field-based testing on object arrays, see `AnyByField`.

## Examples

```jyro
var numbers = [1, 3, 5, 6]
var hasEven = Any(numbers, (x) => x % 2 == 0)
# hasEven = true

var odds = [1, 3, 5]
var hasEvenOdds = Any(odds, (x) => x % 2 == 0)
# hasEvenOdds = false

var users = [
    { "name": "Alice", "age": 30 },
    { "name": "Bob", "age": 17 }
]
var hasMinor = Any(users, (u) => u.age < 18)
# hasMinor = true
```
