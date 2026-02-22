# Contains

Tests whether a string contains a substring or an array contains an element.

## Signature

```
Contains(any source, any search)
```

## Parameters

- **source** (any): A string or array to search within.
- **search** (any): The value to search for.

## Returns

- **boolean**: `true` if the value is found, `false` otherwise.

## Description

Polymorphic function supporting strings and arrays.

- **String mode**: Returns `true` if `source` contains `search` as a substring. Case-sensitive.
- **Array mode**: Returns `true` if any element in the array equals `search` using deep equality.
- Returns `false` if either argument is `null`.
- Throws a runtime error if `source` is not a string or array.

## Examples

```jyro
# String contains
var a = Contains("Hello, World", "World")
# a = true

var b = Contains("Hello", "hello")
# b = false (case-sensitive)

# Array contains
var c = Contains([1, 2, 3], 2)
# c = true

var d = Contains(["apple", "banana"], "cherry")
# d = false

# Deep equality for objects
var users = [{ "id": 1 }, { "id": 2 }]
var found = Contains(users, { "id": 2 })
# found = true

# Null returns false
var e = Contains(null, "test")
# e = false
```
