# ToJson

Converts a Jyro value to a JSON string.

## Signature

```
ToJson(any value)
```

## Parameters

- **value** (any): The value to serialise.

## Returns

- **string**: The JSON string representation.

## Description

Serialises the value to JSON. Objects and arrays are serialised recursively. Primitives are converted to their JSON equivalents.

## Examples

```jyro
var json = ToJson({ "name": "Alice", "age": 30 })
# json = "{\"name\":\"Alice\",\"age\":30}"

var arrJson = ToJson([1, 2, 3])
# arrJson = "[1,2,3]"

var num = ToJson(42)
# num = "42"
```
