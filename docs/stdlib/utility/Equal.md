# Equal

Tests whether two values are deeply equal.

## Signature

```
Equal(any left, any right)
```

## Parameters

- **left** (any): The first value.
- **right** (any): The second value.

## Returns

- **boolean**: `true` if the values are deeply equal.

## Description

Uses `JyroValue.Equals` for deep structural comparison. Unlike the `==` operator (which uses reference equality for objects and arrays), `Equal` compares values recursively.

## Examples

```jyro
var a = Equal(1, 1)
# a = true

var b = Equal("hello", "hello")
# b = true

# Deep equality for objects
var obj1 = { "name": "Alice", "age": 30 }
var obj2 = { "name": "Alice", "age": 30 }
var c = Equal(obj1, obj2)
# c = true

# == uses reference equality for objects
var d = obj1 == obj2
# d = false
```
