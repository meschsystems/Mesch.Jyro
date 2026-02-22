# Keys

Returns all property names of an object as an array.

## Signature

```
Keys(object obj)
```

## Parameters

- **obj** (object): The object to inspect.

## Returns

- **array**: An array of strings representing the property names.

## Description

Returns the keys in their insertion order. Returns an empty array for objects with no properties.

## Examples

```jyro
var user = { "name": "Alice", "age": 30, "email": "alice@example.com" }
var keys = Keys(user)
# keys = ["name", "age", "email"]

var empty = Keys({})
# empty = []

# Iterate over dynamic properties
foreach key in Keys(Data.config) do
    var value = Data.config[key]
end
```
