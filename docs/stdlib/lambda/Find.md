# Find

Returns the first element in an array that matches a lambda predicate.

## Signature

```
Find(array arr, lambda fn)
```

## Parameters

- **arr** (array): The array to search.
- **fn** (lambda): A lambda `(item) => boolean` that returns truthy for the desired element.

## Returns

- **any**: The first element for which the lambda returned truthy.
- **null**: Returned if no element matches.

## Description

Iterates the array and returns the first element where the lambda returns a truthy value. Short-circuits on the first match. The original array is not modified.

For field-based searching on object arrays, see `FindByField`.

## Examples

```jyro
var numbers = [1, 2, 3, 4, 5]
var firstEven = Find(numbers, (x) => x % 2 == 0)
# firstEven = 2

var users = [
    { "name": "Alice", "age": 30 },
    { "name": "Bob", "age": 25 },
    { "name": "Carol", "age": 35 }
]
var senior = Find(users, (u) => u.age > 30)
# senior = { "name": "Carol", "age": 35 }

var missing = Find(numbers, (x) => x > 100)
# missing = null
```
