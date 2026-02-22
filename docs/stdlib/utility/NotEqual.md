# NotEqual

Tests whether two values are not deeply equal.

## Signature

```
NotEqual(any left, any right)
```

## Parameters

- **left** (any): The first value.
- **right** (any): The second value.

## Returns

- **boolean**: `true` if the values differ.

## Description

Inverse of `Equal`. Uses `JyroValue.Equals` for deep structural comparison, then negates the result.

## Examples

```jyro
var a = NotEqual(1, 2)
# a = true

var b = NotEqual("hello", "hello")
# b = false

var obj1 = { "id": 1 }
var obj2 = { "id": 2 }
var c = NotEqual(obj1, obj2)
# c = true
```
