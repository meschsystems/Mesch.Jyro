# FromJson

Parses a JSON string into a Jyro value.

## Signature

```
FromJson(string json)
```

## Parameters

- **json** (string): The JSON string to parse.

## Returns

- **any**: The parsed value (object, array, string, number, boolean, or null).
- **null**: Returned if parsing fails.

## Description

Parses the JSON string and recursively converts to Jyro types. JSON objects become Jyro objects, arrays become Jyro arrays, etc.

Returns `null` on parse failure rather than throwing an error.

## Examples

```jyro
var obj = FromJson("{\"name\": \"Alice\", \"age\": 30}")
# obj.name = "Alice"
# obj.age = 30

var arr = FromJson("[1, 2, 3]")
# arr = [1, 2, 3]

var invalid = FromJson("not json")
# invalid = null
```
