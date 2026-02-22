# Map

Transforms each element of an array using a lambda function.

## Signature

```
Map(array arr, lambda fn)
```

## Parameters

- **arr** (array): The array to transform.
- **fn** (lambda): A lambda `(item) => transformedValue` applied to each element.

## Returns

- **array**: A new array containing the transformed elements.

## Description

Applies the lambda to each element of the array and collects the results into a new array. The original array is not modified. The lambda receives one argument (the current element) and should return the transformed value.

## Examples

```jyro
var numbers = [1, 2, 3, 4]
var doubled = Map(numbers, (x) => x * 2)
# doubled = [2, 4, 6, 8]

var users = [
    { "name": "Alice", "age": 30 },
    { "name": "Bob", "age": 25 }
]
var names = Map(users, (u) => u.name)
# names = ["Alice", "Bob"]

var lengths = Map(["hello", "world"], (s) => Length(s))
# lengths = [5, 5]
```
