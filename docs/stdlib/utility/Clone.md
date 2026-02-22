# Clone

Creates a deep copy of a value.

## Signature

```
Clone(any value)
```

## Parameters

- **value** (any): The value to clone.

## Returns

- **any**: A deep copy of the value. Modifications to the clone do not affect the original.

## Description

Recursively copies all nested structures (arrays and objects). Primitive types (number, string, boolean, null) are copied by value. Useful when you need to modify a copy without affecting the original data.

## Examples

```jyro
var original = { "name": "Alice", "tags": ["admin", "user"] }
var copy = Clone(original)

copy.name = "Bob"
Append(copy.tags, "editor")

# original.name = "Alice" (unchanged)
# original.tags = ["admin", "user"] (unchanged)
```
