# Length

Returns the length or count of a value.

## Signature

```
Length(any value)
```

## Parameters

- **value** (any): A string, array, object, or other value.

## Returns

- **number**: The length or count of the input.

## Description

Polymorphic function that returns:

- **string**: Number of characters.
- **array**: Number of elements.
- **object**: Number of properties.
- **null**: `0`.
- **other**: `1`.

## Examples

```jyro
var a = Length("hello")
# a = 5

var b = Length([1, 2, 3])
# b = 3

var c = Length({ "name": "Alice", "age": 30 })
# c = 2

var d = Length(null)
# d = 0
```
