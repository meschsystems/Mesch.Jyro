# IndexOf

Finds the position of a value within a string or array.

## Signature

```
IndexOf(any source, any search)
```

## Parameters

- **source** (any): A string or array to search within.
- **search** (any): The value to find.

## Returns

- **number**: The zero-based index of the first occurrence, or `-1` if not found.

## Description

Polymorphic function supporting strings and arrays.

- **String mode**: Finds the first occurrence of a substring using ordinal comparison. Case-sensitive.
- **Array mode**: Finds the first element matching `search` using deep equality.
- Throws a runtime error if `source` is not a string or array.

## Examples

```jyro
# String search
var a = IndexOf("Hello World", "World")
# a = 6

var b = IndexOf("Hello", "xyz")
# b = -1

# Array search (deep equality)
var c = IndexOf([10, 20, 30], 20)
# c = 1

var d = IndexOf(["a", "b", "c"], "b")
# d = 1

# Object deep equality
var users = [{ "id": 1 }, { "id": 2 }]
var idx = IndexOf(users, { "id": 2 })
# idx = 1
```
