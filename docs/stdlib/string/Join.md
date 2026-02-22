# Join

Joins array elements into a single string with a separator.

## Signature

```
Join(array values, string separator)
```

## Parameters

- **values** (array): The array of values to join.
- **separator** (string): The string to place between elements.

## Returns

- **string**: A single string with all elements joined by the separator.

## Description

Iterates through the array and converts each element to a string before joining. Element conversion rules:

- **string**: Used as-is.
- **null**: Converted to the literal string `"null"`.
- **other types**: Converted via `.ToString()`.

## Examples

```jyro
var result = Join(["a", "b", "c"], ",")
# result = "a,b,c"

var spaced = Join(["Hello", "World"], " ")
# spaced = "Hello World"

# Mixed types
var mixed = Join([1, "two", true, null], " | ")
# mixed = "1 | two | True | null"

# Empty separator
var concat = Join(["a", "b", "c"], "")
# concat = "abc"
```
