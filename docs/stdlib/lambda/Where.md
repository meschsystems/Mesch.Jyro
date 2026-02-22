# Where

Filters an array using a lambda predicate.

## Signature

```
Where(array arr, lambda fn)
```

## Parameters

- **arr** (array): The array to filter.
- **fn** (lambda): A lambda `(item) => boolean` that returns truthy to include the element.

## Returns

- **array**: A new array containing only elements for which the lambda returned truthy.

## Description

Applies the lambda to each element and includes it in the result if the lambda returns a truthy value. The original array is not modified.

For field-based filtering on object arrays, see `WhereByField`.

## Examples

```jyro
var numbers = [1, 2, 3, 4, 5, 6]
var evens = Where(numbers, (x) => x % 2 == 0)
# evens = [2, 4, 6]

var users = [
    { "name": "Alice", "active": true },
    { "name": "Bob", "active": false },
    { "name": "Carol", "active": true }
]
var active = Where(users, (u) => u.active)
# active = [Alice, Carol]
```
