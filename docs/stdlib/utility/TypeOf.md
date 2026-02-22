# TypeOf

Returns the type name of a value as a string.

## Signature

```
TypeOf(any value)
```

## Parameters

- **value** (any): The value to inspect.

## Returns

- **string**: One of `"null"`, `"boolean"`, `"number"`, `"string"`, `"array"`, or `"object"`.

## Description

Returns a lowercase string identifying the runtime type of the value. Useful for dynamic type checking before performing type-specific operations.

## Examples

```jyro
var a = TypeOf(42)
# a = "number"

var b = TypeOf("hello")
# b = "string"

var c = TypeOf(true)
# c = "boolean"

var d = TypeOf(null)
# d = "null"

var e = TypeOf([1, 2])
# e = "array"

var f = TypeOf({ "name": "Alice" })
# f = "object"
```
